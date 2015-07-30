using CsFormAnalyzer.Core;
using CsFormAnalyzer.Data;
using CsFormAnalyzer.Foundation;
using CsFormAnalyzer.Mvvm;
using CsFormAnalyzer.Types;
using CsFormAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;

namespace CsFormAnalyzer.ViewModels
{
    partial class CodeConverterVM : Mvvm.ViewModelBase
    {   
        #region Fields
        #endregion

        #region Properties
      
        public override void InitProperties()
        {
            base.InitProperties();

            this.Title = "Code Converter";

            InitConvert();
            InitRegexTool();
            InitIndexMatch();
            InitSplitter();
            InitUserWork();
        }

        #endregion

        #region Commands
        
        public ICommand RunCommand { get; private set; }
        public ICommand AddToListCommand { get; private set; }
        public ICommand RemoveRegexItemCommand { get; private set; }
        public ICommand UpdateRegexListCommand { get; private set; }
        public ICommand UpdateDbConvertDictionaryCommand { get; private set; }        

        public override void InitCommands()
        {
            RunCommand = base.CreateCommand(OnExecuteRunCommand);
            AddToListCommand = base.CreateCommand(OnExecuteAddToListCommandd);
            RemoveRegexItemCommand = base.CreateCommand(OnExecuteRemoveRegexItemCommand);
            UpdateRegexListCommand = base.CreateCommand(OnExecuteUpdateRegexListCommand);
            UpdateDbConvertDictionaryCommand = base.CreateCommand(OnExecuteUpdateDbConvertDictionaryCommand);
        }

        private void OnExecuteRunCommand(object obj)
        {
            this.InvokeAsyncAction(delegate
            {
                var code = OriginalCode;
                if (string.IsNullOrEmpty(code)) return;
#if !DEBUG
            try
            {
#endif
                if (ConvertType == "ViewModel")
                    code = ConvertForViewModel(code);
                else //if (ConvertType == "Server")
                    code = ConvertForServer(code);
#if !DEBUG
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
#endif
                ResultCode = code;
            });
        }

        private void OnExecuteAddToListCommandd(object obj)
        {
            var query = string.Format(@"
INSERT TBL_RegexTool
       (Pattern, Replace, Text, Description)
VALUES (N'{0}', N'{1}', N'{2}', N'{3}')", this.Expression, this.ReplaceRegex, this.Text, this.RegexDescription);
            
            var conn = SqlHelper.GetConnection(AppManager.DataConnectionString);
            var result = SqlHelper.ExecuteNonQuery(conn, query);
            System.Windows.MessageBox.Show("저장되었습니다.");

            LoadRegexList();
            
            //RegexList.Add(this.Expression.ConcatDiv("#$%", this.ReplaceRegex));
            //var regexList = string.Join("@#$", RegexList.ToArray());
            //AppManager.Current.Settings.Set("RegexList", regexList);
        }

        private void OnExecuteRemoveRegexItemCommand(object obj)
        {
            //var selector = obj as System.Windows.Controls.Primitives.Selector;
            //selector.RemoveSelectedItem();

            //var regexList = string.Join("@#$", RegexList.ToArray());
            //AppManager.Current.Settings.Set("RegexList", regexList);

            if (this.SelectedRegexToolItem == null) return;
            
            var query = string.Format(@"
DELETE FROM TBL_RegexTool
 WHERE seqID={0}", this.SelectedRegexToolItem.seqID);

            var conn = SqlHelper.GetConnection(AppManager.DataConnectionString);
            var result = SqlHelper.ExecuteNonQuery(conn, query);

            LoadRegexList();
        }

        private void OnExecuteUpdateRegexListCommand(object obj)
        {
            AppManager.Current.Settings.Remove("RegexList");
            LoadRegexList();
        }

        private void OnExecuteUpdateDbConvertDictionaryCommand(object obj)
        {
            DataTable dt;
            if (ConvertType == "ViewModel")
                dt = ConvertDictionaryByDb;
            else //if (ConvertType == "Server")
                dt = ConvertDictionaryByDbForServer;

            var updateQuery = string.Format(@"
UPDATE TBL_CodeConvertDictionary
   SET Target=@Target, Pattern=@Pattern, Replacement=@Replacement, Type='{0}'
 WHERE seqID=@seqID", this.ConvertType);

            var insertQuery = string.Format(@"
INSERT TBL_CodeConvertDictionary
       (Target, Pattern, Replacement, Type)
VALUES (@Target, @Pattern, @Replacement, '{0}')", this.ConvertType);

            var deleteQuery = @"
DELETE TBL_CodeConvertDictionary
 WHERE seqID=@seqID";

            var conn = SqlHelper.GetConnection(AppManager.DataConnectionString);
            var result = dt.Update(conn, updateQuery, insertQuery, deleteQuery);
            System.Windows.MessageBox.Show(string.Format("{0}개행 저장되었습니다.", result));

            //InitConvertDictionaryByDb();
        }

        #endregion

        #region CodeConverter...

        public string OriginalCode { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string ResultCode { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        [RestoreValue("CodeConvertVM.ConvertType", "ViewModel")]
        public string ConvertType { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public ObservableCollection<MatchReplaceDelegator> ConvertDictionaryByBinding { get; set; }
        public DataTable ConvertDictionaryByDb { get; set; }
        public DataTable ConvertDictionaryByDbForServer { get; set; }
        public ObservableCollection<MatchReplaceDelegator> ConvertDictionaryByPre { get; set; }
        public ObservableCollection<MatchReplaceDelegator> ConvertDictionaryByPost { get; set; }
        public ObservableCollection<MatchReplaceDelegator> ConvertDictionaryByServer { get; set; }
                
        public string Namespace { get; set; }

        private void InitConvert()
        {
            ConvertDictionaryByBinding = new ObservableCollection<MatchReplaceDelegator>();
            ConvertDictionaryByPre = new ObservableCollection<MatchReplaceDelegator>();
            ConvertDictionaryByPost = new ObservableCollection<MatchReplaceDelegator>();
            ConvertDictionaryByServer = new ObservableCollection<MatchReplaceDelegator>();

            InitConvertDictionaryByDb("ViewModel");
            InitConvertDictionaryByDb("Server");
            InitConvertDictionaryByPre();
            InitConvertDictionaryByPost();
            InitConvertDictionaryByServer();
        }

        private void InitConvertDictionaryByDb(string convertType)
        {
            var conn = SqlHelper.GetConnection(AppManager.DataConnectionString);
            var query = "SELECT * FROM TBL_CodeConvertDictionary WHERE Type='" + convertType + "'";

            var dt = new DataTable();
            dt.Fill(conn, query);

            if (convertType == "ViewModel")
                ConvertDictionaryByDb = dt;
            else //if (convertType == "Server")
                ConvertDictionaryByDbForServer = dt;
        }

        private void InitConvertDictionaryByPre()
        {
            #region replacement

            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"HIS\.Legacy\.Win\.LApplicationContext\.Current\.GlobalState\.Contains", "Context.GlobalInfo.GlobalState.ContainsKey"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"HIS\.Legacy\.Win\.LApplicationContext\.Current\.GlobalState", "Context.GlobalInfo.GlobalState"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"((this\.|)Cursor = Cursors\.WaitCursor;|FormUtil\.CursorWait\(this, true\);)", "IsViewBusy = true;"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"((this\.|)Cursor = Cursors\.Default;|FormUtil\.CursorWait\(this, false\);)", "IsViewBusy = false;"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"MessageBoxViewer\.ShowAlert\(", "Context.ViewManager.ShowMessagePopup("));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"(this\.)*_controller\.ShowAlert\(", "Context.ViewManager.ShowMessagePopup("));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"LApplicationContext.Current.UserInfo", "Context.GlobalInfo"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"LApplicationContext.Current.GlobalState", "Context.GlobalInfo.GlobalState"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"MessageBoxViewer.ShowConfirm", "Context.ViewManager.ShowConfirmPopup"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(
                @"MessageBoxViewer.ShowError\([\s]*ex.Message,[\s]*ex[\s]*\);", 
                "throw new SafClientException(this.GetType().Name, ex.Message, ex);"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"(System\.)*(Windows\.)*Forms\.DialogResult", "MessageBoxResult"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"HIS.Legacy.Win.Context", "Context"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"this.Text", "Context.ViewManager.GetWindow(this).Title"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"private void [\w]+_Load\(object sender, (System\.)*EventArgs e\)", "public void InitLoad()"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"private void [\w]+_Closed\(object sender, System.EventArgs e\)", "public override void OnClosed()"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"System.Drawing.Color", "Color"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"InitializeComponent\(\);", "//? InitializeComponent();"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"InitializeSpread", "InitializeGridView"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"GlobalState.Add", "AddGlobalState<T>"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"DialogResult\.", "MessageBoxResult."));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"Color\.FromArgb", "Color.FromRgb"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"TODO : YUMC", "YUMC"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"TODO : YDMC", "YDMC"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"TODO : InitializeComponent", "InitializeComponent"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"new Exception\(""", @"new SafClientException("""));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"DialogResult = DialogResult\.Yes;", @"this.CurrentWindow.DialogResult = true;"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"this.((Top|Left))[\s]+=", @"this.CurrentWindow.$1 ="));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"DialogResult = MessageBoxResult\.OK;", @"CurrentWindow.DialogResult = true;"));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"#region 생성자 및 소멸자", @"#region Initialize..."));
            ConvertDictionaryByPre.Add(new MatchReplaceDelegator(@"#region 속성", @"#region Properties"));
            
            #endregion

            MatchReplaceDelegator matchReplaceDelegator;

            #region @"public [\w]+\(.*\)"

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"public [\w]+\(.*\)",
                RegexOptions = RegexOptions.IgnoreCase,
                MatchLogic = (m, v) =>
                {
                    var methodName = m.Value.Between(" ", "(");
                    if (m.Value.Between("(", ")", false, true).Length < 1)
                    {
                        // 생성자
                        return m.Value.Replace(methodName, "override void Init ? base.Init();");
                    }
                    else
                    {
                        // 생성자오버로딩
                        return m.Value.Replace(methodName, "void InvokeInitAfter");
                    }                    
                },
            };
            ConvertDictionaryByPre.Add(matchReplaceDelegator);

            #endregion

            #region @"[^\n]*(DialogResult[\s]+[\w]+[\s]+=[\s]+MessageBox\.Show\(.*?\);)",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"[^\n]*(DialogResult[\s]+[\w]+[\s]+=[\s]+MessageBox\.Show\(.*?\);)",
                RegexOptions = RegexOptions.Singleline | RegexOptions.IgnoreCase,
                MatchLogic = (m, v) =>
                {
                    var ret = m.Value;
                    if (ret.Trim().StartsWith("//")) return ret;

                    var innerBlock = ret.Between("(", ")", false, true);
                    var parameters = StringHelper.GetParams(innerBlock);

                    var msg = parameters.ElementAt(0).Trim();
                    ret = string.Format("var dialogResult = Context.ViewManager.ShowConfirmPopup({0});", msg);
                    
                    return ret;
                },
            };
            ConvertDictionaryByPre.Add(matchReplaceDelegator);

            #endregion

            #region @"[^\n]*((this\.)*btn([\w]+)_Click\(null,\s*null\);)",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"[^\n]*((this\.)*btn([\w]+)_Click\(null,\s*null\);)",
                RegexOptions = RegexOptions.Singleline | RegexOptions.IgnoreCase,
                MatchLogic = (m, v) =>
                {
                    var ret = m.Value;
                    if (ret.Trim().StartsWith("//")) return ret;

                    var target = m.Groups[1].Value;
                    var replacement = string.Format("Exec{0}Command(null);", m.Groups[3].Value);
                    ret = m.Value.Replace(target, replacement);
                    return ret;
                },
            };
            ConvertDictionaryByPre.Add(matchReplaceDelegator);

            #endregion

            #region @"(HIS\.Legacy\.Message\.)*MessageManager\.GetMessage\([^\)]+\)",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"((HIS\.)*Legacy\.Message\.)*MessageManager\.GetMessage\([^\)]+\)",
                MatchLogic = (m, v) =>
                {
                    // HIS.Legacy.Message.MessageManager.GetMessage("HIS.SP", "NOHAVE_RCPT_DATA")
                    // GetLocalizedText("SpResourceLocalize", "NOHAVE_RCPT_DATA")
                    var block = m.Value.Between("(", ")", true, true);
                    var p = block.RegexReturn(@"HIS\.[\w]+");
                    block = block.Replace(p, p.RightBySearch(".").ToCamel() + "ResourceLocalize");
                    var ret = "GetLocalizedText" + block;
                    return ret;
                },
            };
            ConvertDictionaryByPre.Add(matchReplaceDelegator);

            #endregion

            #region @"[^\s]+(ShowInstanceMessage|ShowMessagePopup)\([^;]+",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"[^\s]+(ShowInstanceMessage|ShowMessagePopup)\([^;]+",
                MatchLogic = (m, v) =>
                {
                    //MessageBoxViewer.ShowError(this, "HIS.SP", ex.Message, ex);
                    //this.ShowInstanceMessage("HIS.SP", "SC_INPUT_PTHONO_DATA", 3);
                    if (m.Value.TrimStart().StartsWith("//")) return m.Value;

                    var parameters = StringHelper.GetParams(m.Value.Between("(", ")", false, true));

                    if (parameters.Length == 1)
                    {
                        return string.Format(@"// TODO : [개발예정] 리소스 적용 필요
this.ShowMessagePopup(GetLocalizedText(""{0}MsgResourceLocalize"", {1}))",
                            "?", parameters.ElementAt(0));
                    }

                    var ns = parameters.ElementAt(0).Between("HIS.", @"""").ToCamel();
                    var msg = parameters.ElementAt(1);
                    if (parameters.Length == 2)
                    {
                        return string.Format(@"// TODO : [개발예정] 리소스 적용 필요
this.ShowMessagePopup(GetLocalizedText(""{0}MsgResourceLocalize"", {1}))",
                            ns, msg);
                    }
                    else if (parameters.Length == 3)
                    {
                        if (parameters.ElementAt(2).IsNumeric())
                        {
                            var sec = Convert.ToInt32(parameters.ElementAt(2));
                            return string.Format(@"// TODO : [개발예정] 리소스 적용 필요
this.ShowMessagePopup(GetLocalizedText(""{0}MsgResourceLocalize"", {1}), TimeSpan.FromSeconds({2}), null)",
                                ns, msg, sec);
                        }
                        else
                        {
                            ns = parameters.ElementAt(1).Between("HIS.", @"""").ToCamel();
                            msg = parameters.ElementAt(2);
                            return string.Format(@"// TODO : [개발예정] 리소스 적용 필요
this.ShowMessagePopup(GetLocalizedText(""{0}MsgResourceLocalize"", {1}))",
                                ns, msg);
                        }
                    }
                    else
                    {
                        return m.Value;
                    }                        
                },
            };
            ConvertDictionaryByPre.Add(matchReplaceDelegator);

            #endregion

            #region @"(?:MessageBox\.Show\()(.*?)(?:\))",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"(?:MessageBox\.Show\()(.*?)(?:\))",
                MatchLogic = (m, v) =>
                {
                    // if(MessageBox.Show("시행취소 하시겠습니까?","확인",MessageBoxButtons.YesNo).Equals(System.Windows.Forms.DialogResult.Yes))
                    // Context.ViewManager.ShowConfirmPopup("시행취소 하시겠습니까?")
                    var block = m.Value.Between("(", ")", false, true);
                    var msg = StringHelper.GetParams(block).ElementAt(0);
                    if (m.Value.IndexOf("MessageBoxButtons") > 0)
                    {
                        var ret = string.Format(@"Context.ViewManager.ShowConfirmPopup({0})", msg);
                        return ret;
                    }
                    else
                    {
                        var ret = string.Format(@"Context.ViewManager.ShowMessagePopup({0})", msg);
                        return ret;
                    }
                },
            };
            ConvertDictionaryByPre.Add(matchReplaceDelegator);

            #endregion

            #region @"this\.[\w]+\.Focus\(\);",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"this\.[\w]+\.Focus\(\);",
                MatchLogic = (m, v) =>
                {
                    var name = m.Value.Between("this.", ".Focus", false, true);
                    var ret = string.Format(@"ControlSetters[""{0}""].IsFocus = true;", name);
                    
                    return ret;
                },
            };
            ConvertDictionaryByPre.Add(matchReplaceDelegator);

            #endregion

            //#region @"[\w]*[hH][tT][\w]*\.Add\(".*\);",

            //matchReplaceDelegator = new MatchReplaceDelegator()
            //{
            //    Pattern = @"[\w]*[hH][tT][\w]*\.Add\("".*\);",
            //    MatchLogic = (m, v) =>
            //    {
            //        // 			htAddParam.Add("OrdUserPos","Z2");
            //        // request.OrdUserPos = "Z2";
            //        var block = m.Value.Between("(", ")", false, true);
            //        var parameters = StringHelper.GetParams(block);
            //        var property = parameters.ElementAt(0).Replace(@"""", "");
            //        var value = parameters.ElementAt(1);

            //        var ret = string.Format(@"parameter.{0} = {1};", property, value);
            //        return ret;
            //    },
            //};
            //ConvertDictionaryByPre.Add(matchReplaceDelegator);

            //#endregion

            //#region @"[\w]*[hH][tT][\w]*\[\""[\w]+""\]",

            //matchReplaceDelegator = new MatchReplaceDelegator()
            //{
            //    Pattern = @"[\w]*[hH][tT][\w]*\[\""[\w]+""\]",
            //    MatchLogic = (m, v) =>
            //    {
            //        var property = m.Value.Between(@"""", @"""", false, true);
            //        var ret = string.Format(@"parameter.{0}", property, property);
            //        return ret;
            //    },
            //};
            //ConvertDictionaryByPre.Add(matchReplaceDelegator);

            //#endregion

            #region @"Color\.[\w]+;",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"Color\.[\w]+;",
                MatchLogic = (m, v) =>
                {
                    var colorKey = m.Value.Between(".", ";");
                    var ret = string.Format(@"new SolidColorBrush(Colors.{0});", colorKey);
                    return ret;
                },
            };
            ConvertDictionaryByPre.Add(matchReplaceDelegator);

            #endregion

            #region @"\[[^\]]+]\["[^\]]+"]",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"\[[^\]]+]\[""[^\]]+""]",
                MatchLogic = (m, v) =>
                {
                    //selectList[i]["Qty"]
                    var target = m.Value.RegexBetween(@"[""", @"""]", true).FirstOrDefault();
                    var name = m.Value.RegexBetween(@"[""", @"""]", false).FirstOrDefault();
                    if (target.IndexOf(" ") > 0) return m.Value;

                    var ret = m.Value.Replace(target, "." + name);
                    return ret;
                },
            };
            ConvertDictionaryByPre.Add(matchReplaceDelegator);

            #endregion            

            #region @"(Context\.GlobalInfo\.)*GlobalState\["(.*)"\]

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"(Context\.GlobalInfo\.)*GlobalState\[""(.*)""\]",
                MatchLogic = (m, v) =>
                {
                    var key = m.Groups.Count < 2 
                        ? m.Groups[1].Value
                        : m.Groups[2].Value;
                    //var right = m.Value.RightBySearch(key + "]");
                    if (key.StartsWith("SP"))
                        return string.Format("Context.GetSpGlobalStates().{0}", key);
                    else if (key.StartsWith("HP"))
                        return string.Format("Context.GetHpGlobalStates().{0}", key);

                    return m.Value;
                },
            };
            ConvertDictionaryByPre.Add(matchReplaceDelegator);

            #endregion            

            #region @"MessageBoxViewer.ShowError\(.*?\);",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"MessageBoxViewer.ShowError\(.*?\);",
                MatchLogic = (m, v) =>
                {
                    var parammeters = StringHelper.GetParams(m.Value.Between("(", ")", false, true));
                    string ret = m.Value;
                    if (parammeters.Length == 4)
                        ret = string.Format("throw new SafClientException(this.GetType().Name, {0}, ex);", parammeters.ElementAt(2));
                    else if (parammeters.Length == 2)
                        ret = string.Format("throw new SafClientException(this.GetType().Name, {0}, ex);", parammeters.ElementAt(0));
                    
                    return ret;
                },
            };
            ConvertDictionaryByPre.Add(matchReplaceDelegator);

            #endregion

            #region @"txt[\w]+\.Clear\(\);",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"txt[\w]+\.Clear\(\);",
                MatchLogic = (m, v) =>
                {
                    var name = m.Value.Between("txt", ".");
                    var ret = string.Format("{0}TextProperty = string.Empty;", name);
                    return ret;
                },
            };
            ConvertDictionaryByPre.Add(matchReplaceDelegator);

            #endregion

            #region @"this.Location = new Point\(([^\))]+\);",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"this.Location = new Point\(([^\)]+)\);",
                MatchLogic = (m, v) =>
                {
                    var values = m.Groups[1].Value.RegexMatches(@"[\d]+");
                    var ret = string.Format(@"this.CurrentWindow.Left = {0};
this.CurrentWindow.Top = {1};", values.ElementAt(0), values.ElementAt(1));
                    return ret;
                },
            };
            ConvertDictionaryByPre.Add(matchReplaceDelegator);

            #endregion

            #region //

            //#region @"HIS\.WinUI\.[\w.]+",
            //matchReplaceDelegator = new MatchReplaceDelegator()
            //{
            //    Pattern = @"HIS\.WinUI\.[\w.]+",
            //    MatchLogic = (m, v) =>
            //    {
            //        string ret = m.Value;
            //        var ns = m.Value.Split('.');
            //        // HIS.WinUI.SP.CEL.CM.Common > His3.Sp.Client.Cel.Cm.Common
            //        var kind = ns.ElementAt(2).ToCamel();
            //        if (kind.Equals("Common"))
            //        {
            //            ret = string.Format(@"His3.{0}.Client.{1}.{2}.{3}"
            //                , "Cm"
            //                , ns.ElementAt(2).ToCamel()
            //                , ns.ElementAt(3).ToCamel()
            //                , ns.LastOrDefault());
            //        }
            //        else
            //        {
            //            ret = string.Format(@"His3.{0}.Client.{1}.{2}.{3}.{4}"
            //                , ns.ElementAt(2).ToCamel()
            //                , ns.ElementAt(3).ToCamel()
            //                , ns.ElementAt(4).ToCamel()
            //                , ns.ElementAt(5).ToCamel()
            //                , ns.LastOrDefault());
            //        }

            //        return ret;
            //    },
            //};
            //ConvertDictionaryByPre.Add(matchReplaceDelegator);

            //#endregion

            //#region @"[^\n]+(DataSet|DataTable) [^\r\n]+", // 코드 주석처리

            //matchReplaceDelegator = new MatchReplaceDelegator()
            //{
            //    Pattern = @"[^\n]+(DataSet|DataTable) [^\r\n]+",
            //    MatchLogic = (m, v) =>
            //    {
            //        return string.Format("//remove? {0}", m.Value.Trim());
            //    },
            //};
            //ConvertDictionaryByPre.Add(matchReplaceDelegator);

            //#endregion

            //#region @"[^\n]+new (DataSet|DataTable|[\w]+Controller)\(", // 코드 주석처리

            //matchReplaceDelegator = new MatchReplaceDelegator()
            //{
            //    Pattern = @"[^\n]+new (DataSet|DataTable|[\w]+Controller)\(",
            //    MatchLogic = (m, v) =>
            //    {
            //        return string.Format("//remove? {0}", m.Value.Trim());
            //    },
            //};
            //ConvertDictionaryByPre.Add(matchReplaceDelegator);

            //#endregion

            //#region @"[^\n]+\.Focus\(\);", // 코드 주석처리
            //matchReplaceDelegator = new MatchReplaceDelegator()
            //{
            //    Pattern = @"[^\n]+\.Focus\(\);",
            //    MatchLogic = (m, v) =>
            //    {
            //        return string.Format("//remove? {0}", m.Value);
            //    },
            //};
            //ConvertDictionaryByPre.Add(matchReplaceDelegator);
            //#endregion     

            //#region @"[^\t]+Controller\.[^\r\n]+", // Request 작성

            //matchReplaceDelegator = new MatchReplaceDelegator()
            //{
            //    Pattern = @"[^\t]+Controller\.[^\r\n]+",
            //    RegexOptions = RegexOptions.IgnoreCase,
            //    MatchLogic = (m, v) =>
            //    {

            //        if (string.IsNullOrEmpty(this.Namespace))
            //            return m.Value;

            //        //"var request = new His3.Sp.Model.Cel.Cb.PthoRcpt.SelectAsisRsltByUnitNoRequest();"                        
            //        //HIS.WinUI.SP.CEL.CB.PthoRcpt
            //        var sb = new StringBuilder();
            //        sb.AppendLine(string.Format("//{0}", m.Value));
            //        var methodName = m.Value.Between("Controller.", "(");
            //        if (string.IsNullOrEmpty(methodName))
            //        {
            //            var match = Regex.Match(m.Value, @"Controller.[\w]+", RegexOptions.IgnoreCase);
            //            if (match.Success)
            //            {
            //                methodName = match.Value.RightBySearch(".");
            //            }
            //        }

            //        var nsArry = this.Namespace.Split('.');
            //        string line;
            //        if (nsArry.ElementAt(3).Equals("ZZZ") != true)
            //        {
            //            line = string.Format("var request = new His3.{0}.Model.{1}.{2}.{3}.{4}Request();",
            //                nsArry.ElementAt(2).ToCamel(),
            //                nsArry.ElementAt(3).ToCamel(),
            //                nsArry.ElementAt(4).ToCamel(),
            //                nsArry.ElementAt(5),
            //                methodName,
            //                string.Format("//{0}", m.Value));
            //        }
            //        else
            //        {
            //            line = string.Format("var request = new His3.{0}.Model.{1}.{2}.{3}Request();",
            //                nsArry.ElementAt(2).ToCamel(),
            //                "Com",
            //                nsArry.ElementAt(4).ToCamel(),
            //                methodName,
            //                string.Format("//{0}", m.Value));
            //        }

            //        sb.AppendLine(line);
            //        sb.AppendLine("var response = Context.Proxy.Post(request);");
            //        return sb.ToString();
            //    },
            //};
            //ConvertDictionaryByPre.Add(matchReplaceDelegator);

            //#endregion
            #endregion
        }

        private void InitConvertDictionaryByPost()
        {            
            MatchReplaceDelegator matchReplaceDelegator;

            //ConvertDictionaryByPost.Add(new MatchReplaceDelegator(@"lblDrgst\.Tag", "drgstTag"));

            #region @"[\w]+\.(ActiveSheet|Sheets\[0\])(\.Rows.Count|\.RowCount) = 0;",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"[\w]+\.(ActiveSheet|[\w]+_Sheet1|Sheets\[0\])(\.Rows.Count|\.RowCount) = 0;",
                MatchLogic = (m, v) =>
                {
                    var name = m.Value.LeftBySearch(".");
                    if (name.Equals("this")) name = m.Value.Between(".", ".");
                    if (name.IndexOf("_Sheet1") >= 0) name = name.LeftBySearch("_");

                    var key = name + @"\.DataSource";
                    var item = ConvertDictionaryByBinding.Where(p => p.Pattern.Equals(key, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (item == null) return m.Value;

                    return item.Replacement + ".Clear();";
                },
            };
            ConvertDictionaryByPost.Add(matchReplaceDelegator);

            #endregion

            #region @"[\w]+\.(ActiveSheet|Sheets\[0\])(\.Rows.Count|\.RowCount)",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"[\w]+\.(ActiveSheet|Sheets\[0\])(\.Rows.Count|\.RowCount)",
                MatchLogic = (m, v) =>
                {
                    var name = m.Value.LeftBySearch(".");
                    var key = name + @"\.DataSource";
                    var item = ConvertDictionaryByBinding.Where(p => p.Pattern.Equals(key, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (item == null) return m.Value;

                    return item.Replacement + ".Count";
                },
            };
            ConvertDictionaryByPost.Add(matchReplaceDelegator);

            #endregion

            #region @"[\w]+\.(?:(ActiveSheet|Sheets\[0\])\.Cells)[^\n]+(?:(ActiveSheet|Sheets\[0\])\.ActiveRowIndex)[^\n]+(?:\.Text)[^\n]+",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"[\w]+\.(?:(ActiveSheet|Sheets\[0\])\.Cells)[^\n]+(?:(ActiveSheet|Sheets\[0\])\.ActiveRowIndex)[^\n]+(?:\.Text)[^\n]+",
                MatchLogic = (m, v) =>
                {
                    //spdRcptList.ActiveSheet.Cells[spdRcptList.ActiveSheet.ActiveRowIndex, 15].Text.Trim();   //반환여부
                    //SelectedPthoRcptStusByPthoNoModelProperty.15.Trim();                    
                    var name = m.Value.LeftBySearch(".");
                    var key = name + @"\.SelectedValue";
                    var item = ConvertDictionaryByBinding.Where(p => p.Pattern.Equals(key, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (item == null) return m.Value;

                    var block = m.Value.GetBlocks(false, "[", "]").FirstOrDefault();
                    string column = string.Empty;
                    if (string.IsNullOrEmpty(block) != true)
                        column = block.SplitAt(",", 1).Trim();

                    var right = m.Value.RightBySearch(".Text");
                    var ret = string.Format("{0}.{1}{2}", item.Replacement, column, right);

                    return ret;
                },
            };
            ConvertDictionaryByPost.Add(matchReplaceDelegator);

            #endregion            

            #region @"[\w]+\.(ActiveSheet|Sheets\[0\])\.Cells",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"[\w]+\.(ActiveSheet|Sheets\[0\])\.Cells",
                MatchLogic = (m, v) =>
                {
                    var name = m.Value.LeftBySearch(".");
                    var key = name + @"\.DataSource";
                    var item = ConvertDictionaryByBinding.Where(p => p.Pattern.Equals(key, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (item == null) return m.Value;

                    return item.Replacement;
                },
            };
            ConvertDictionaryByPost.Add(matchReplaceDelegator);

            #endregion            

            #region @"[\w]+\.(ActiveSheet|Sheets\[0\])\.Columns",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"[\w]+\.(ActiveSheet|Sheets\[0\])\.Columns\[[^\]]+\]\.[^\s]+",
                MatchLogic = (m, v) =>
                {
                    var name = m.Value.LeftBySearch(".");
                    var key = name + @"\.DataSource";
                    var item = ConvertDictionaryByBinding.Where(p => p.Pattern.Equals(key, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (item == null) return m.Value;
                    
                    var listName = item.Replacement.LeftBySearch("ListProperty");
                    var columnName = m.Value.LastBetween("[", "]");
                    var pName = m.Value.RightBySearch(".");
                    if ("Label".Equals(pName)) pName = "Title";

                    var property = string.Format("{0}{1}{2}Property", listName, columnName, pName);
                    return property;
                },
            };
            ConvertDictionaryByPost.Add(matchReplaceDelegator);

            #endregion            

            #region @"[\w]+\.(ActiveSheet|Sheets\[0\])",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"[\w]+\.(ActiveSheet|Sheets\[0\])",
                MatchLogic = (m, v) =>
                {
                    var name = m.Value.LeftBySearch(".");
                    var key = name + @"\.DataSource";
                    var item = ConvertDictionaryByBinding.Where(p => p.Pattern.Equals(key, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (item == null) return m.Value;
                                        
                    return item.Replacement;
                },
            };
            ConvertDictionaryByPost.Add(matchReplaceDelegator);

            #endregion 

            #region @"[\w]+\.SelectedIndex (==|>|<|>=|<=|<>) [\w]+",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"[\w]+\.SelectedIndex\s+(==|>|<|>=|<=|<>)\s+[\w]+",
                MatchLogic = (m, v) =>
                {
                    var name = m.Value.LeftBySearch(".");                    
                    var key = name + @"\.DataSource";
                    var item = ConvertDictionaryByBinding.Where(p => p.Pattern.Equals(key, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (item == null) return m.Value;

                    var listName = item.Replacement;
                    var entity = item.Replacement.LeftBySearch("ListProperty");
                    var right = m.Value.Substring(m.Value.IndexOf(" ")).Trim();
                    var ret = string.Format("{0}.IndexOf(Selected{1}Property) {2}", listName, entity, right);
                    return ret;
                },
            };
            ConvertDictionaryByPost.Add(matchReplaceDelegator);

            #endregion

            #region @"[\w]+\.SelectedIndex\s+=\s+0;",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"[\w]+\.SelectedIndex\s+=\s+0;",
                MatchLogic = (m, v) =>
                {
                    var name = m.Value.LeftBySearch(".");
                    var key = name + @"\.DataSource";
                    var item = ConvertDictionaryByBinding.Where(p => p.Pattern.Equals(key, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (item == null) return m.Value;

                    var listName = item.Replacement;
                    var entity = item.Replacement.LeftBySearch("ListProperty");
                    var right = m.Value.Substring(m.Value.IndexOf(" ")).Trim();
                    var ret = string.Format("Selected{0}Property = {1}.FirstOrDefault();", entity, listName);
                    return ret;
                },
            };
            ConvertDictionaryByPre.Add(matchReplaceDelegator);

            #endregion
            
            #region @"[\w]+\.SelectedIndex[\s]+=.*Count - 1;",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"[\w]+\.SelectedIndex[\s]+=.*Count - 1;",
                MatchLogic = (m, v) =>
                {
                    var name = m.Value.LeftBySearch(".");
                    var key = name + @"\.DataSource";
                    var item = ConvertDictionaryByBinding.Where(p => p.Pattern.Equals(key, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (item == null) return m.Value;

                    var listName = item.Replacement;
                    var entity = item.Replacement.LeftBySearch("ListProperty");
                    var right = m.Value.Substring(m.Value.IndexOf(" ")).Trim();
                    var ret = string.Format("Selected{0}Property = {1}.LastOrDefault();", entity, listName);
                    return ret;
                },
            };
            ConvertDictionaryByPre.Add(matchReplaceDelegator);

            #endregion                        

            #region @"[\w]+(\.Sheets\[0\]|\.ActiveSheet)\.(Rows\.Count|RowCount) (==|>|<|<=|>=|<>)",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"[\w]+(\.Sheets\[0\]|\.ActiveSheet)\.(Rows\.Count|RowCount) (==|>|<|<=|>=|<>)",
                MatchLogic = (m, v) =>
                {
                    var name = m.Value.LeftBySearch(".");
                    var key = name + @"\.DataSource";
                    var item = ConvertDictionaryByBinding.Where(p => p.Pattern.Equals(key, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (item == null) return m.Value;

                    var listName = item.Replacement;
                    var right = m.Value.Substring(m.Value.IndexOf(" ")).Trim();
                    var ret = string.Format(@"{0}.Count {1}", listName, right);
                    return ret;
                },
            };
            ConvertDictionaryByPost.Add(matchReplaceDelegator);

            #endregion            

            #region @"([\w]+)_Sheet1\.Cells\[[\w]+_Sheet1\.ActiveRowIndex\, ",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"([\w]+)_Sheet1\.Cells\[[\w]+_Sheet1\.ActiveRowIndex\, ",
                MatchLogic = (m, v) =>
                {
                    var name = m.Groups[1].Value;
                    var key = name + @"\.SelectedValue";
                    var item = ConvertDictionaryByBinding.Where(p => p.Pattern.Equals(key, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (item == null) return m.Value;

                    var ret = item.Replacement + "[";
                    return ret;
                },
            };
            ConvertDictionaryByPost.Add(matchReplaceDelegator);

            #endregion            
            
            #region @"[\w]+ListProperty\[[\w]\]\[""[^\.]+",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"[\w]+ListProperty\[[\w]\]\[""[^\.]+",
                MatchLogic = (m, v) =>
                {
                    var vmProperty = m.Value.RegexReturn(@"[\w]+ListProperty");
                    var blocks = m.Value.Substring(m.Value.IndexOf(vmProperty)).GetBlocks(true, "[", "]");
                    if (blocks.Length < 2) return m.Value;
                    var index = blocks.ElementAt(0);
                    var property = blocks.ElementAt(1).Substring(2, blocks.ElementAt(1).Length - 4);
                    var ret = m.Value.Replace(m.Value.RightBySearch("[", false, true), "." + property);
                    return ret;
                },
            };
            ConvertDictionaryByPost.Add(matchReplaceDelegator);

            #endregion

            #region @"[^\n]+ListProperty\.Rows\[\w+\]\[""\w+""\][^\n]+",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"[^\n]+ListProperty\.Rows\[\w+\]\[""\w+""\][^\n]+",
                MatchLogic = (m, v) =>
                {
                    var vmProperty = m.Value.RegexReturn(@"[\w]+ListProperty");
                    var pName = m.Value.Substring(m.Value.IndexOf(".Rows")).Between(@"[""", @"""]");
                    var ret = m.Value;
                    if (string.IsNullOrEmpty(pName) != true)
                    {
                        ret = m.Value.RegexReplace(@"\.Rows\[", @"[");
                        ret = ret.Replace(string.Format(@"[""{0}""]", pName), "." + pName);

                        var text = ret.RegexReturn(@"\[[\w, ]+\]\.Text");
                        if (string.IsNullOrEmpty(text) != true)
                        {
                            if (text.IndexOf(",") > 0)
                                ret = ret.Replace(text.Between(",", "]", true), "]");

                            ret = ret.Replace(".Text", "." + pName);
                        }
                    }

                    return ret;
                },
            };
            ConvertDictionaryByPost.Add(matchReplaceDelegator);

            #endregion

            #region @"ListProperty\.Rows\.Count",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"ListProperty\.Rows\.Count",
                Replacement = @"ListProperty.Count",
            };
            ConvertDictionaryByPost.Add(matchReplaceDelegator);

            #endregion

            #region @"(Selected([\w]+)Property) = ("".*?"");",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"(Selected([\w]+)Property) = ("".*?"");",
                RegexOptions = RegexOptions.Multiline,
                MatchLogic = (m, v) =>
                {
                    var variable = m.Groups[1].Value;
                    var name = m.Groups[2].Value;
                    var expression = m.Groups[3].Value;

                    var ret = string.Format(@"{0} = {1}ListProperty.Where(p => p.ComCd.Equals({2})).FirstOrDefault();"
                        , variable, name, expression);
                    return ret;
                },
            };
            ConvertDictionaryByPost.Add(matchReplaceDelegator);

            #endregion            

            #region @"VisibleProperty[\s]+=+[^;]+",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"VisibleProperty[\s]+=+[^;]+",
                MatchLogic = (m, v) =>
                {
                    string ret;
                    ret = Regex.Replace(m.Value, "true", "System.Windows.Visibility.Visible", RegexOptions.IgnoreCase);
                    ret = Regex.Replace(ret, "false", "System.Windows.Visibility.Hidden", RegexOptions.IgnoreCase);
                    return ret;
                },
            };
            ConvertDictionaryByPost.Add(matchReplaceDelegator);

            #endregion

            #region @"(?<=DateProperty\.)(.*)",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"(?<=DateProperty\.)(?!ToShortDateString)(.*)",
                MatchLogic = (m, v) =>
                {
                    if (m.Value.StartsWith(@"ToString("""))
                        return m.Value;
                    else
                    {
                        var ret = "ToShortDateString()." + m.Value;
                        return ret;
                    }
                },
            };
            ConvertDictionaryByPost.Add(matchReplaceDelegator);

            #endregion

            #region @"[^ ]+ValidateComboBox\([^\);]+\)",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"[^ ]+ValidateComboBox\([^\);]+\)",
                MatchLogic = (m, v) =>
                {
                    var name = m.Value.Between("(", ")").Trim();
                    if (name.IndexOf(".") > 0) name = name.RightBySearch(".").Trim();
                    var key = name + @"\.SelectedValue";
                    var item = ConvertDictionaryByBinding.Where(p => p.Pattern.Equals(key, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (item == null) return m.Value;

                    return string.Format("{0}.#Value#", item.Replacement);
                },
            };
            ConvertDictionaryByPost.Add(matchReplaceDelegator);

            #endregion

            #region @"lbl[\w]+\.Tag",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"lbl[\w]+\.Tag",
                MatchLogic = (m, v) =>
                {
                    var name = m.Value.Between("lbl", ".");
                    var ret = name.ToCamel();

                    return string.Format("_{0}Tag", ret);
                },
            };
            ConvertDictionaryByPost.Add(matchReplaceDelegator);

            #endregion

//            #region Request...

//            var conn = SqlHelper.GetConnection(AppManager.DataConnectionString);

//            var dtSpModel = new DataTable();
//            dtSpModel.Fill(conn, "SELECT * FROM TBL_SP_MODEL");

//            var dtViewBizInfo = new DataTable();
//            dtViewBizInfo.Fill(conn, "SELECT * FROM VIEW_BIZ_INFO WHERE LAYER='Controller'");

//            matchReplaceDelegator = new MatchReplaceDelegator()
//            {
//                Pattern = @"[^\n]+([Cc]ontroller|[Ff]acade)\.[\w]+\([^;]+;",
//                MatchLogic = (m, v) =>
//                {
//                    var value = m.Value.Trim();
//                    if (value.StartsWith("//")) return m.Value;

//                    var methodName = value.RegexBetween(@"([Cc]ontroller|[Ff]acade)\.", @"\(", false, false).FirstOrDefault();
//                    var parameterLine = value.Between("(", ")", false, true);

//                    var facadeFunctionName = methodName;
//                    if (m.Value.IndexOf("Controller", StringComparison.CurrentCultureIgnoreCase) > 0)
//                    {
//                        var parameterCount = string.IsNullOrEmpty(parameterLine)
//                            ? 0 
//                            : StringHelper.GetParams(parameterLine).Count();
//                        var controllerRow = dtViewBizInfo.Select(string.Format(@"METHODNM='{0}' AND ParamsCount={1}", methodName, parameterCount)).FirstOrDefault();
//                        if (controllerRow == null) return m.Value;

//                        facadeFunctionName = controllerRow.ToStr("CALLFUNCNM");
//                    }

//                    var row = dtSpModel.Select(string.Format("FacadeMethodNM='{0}'", facadeFunctionName)).FirstOrDefault();
//                    if (row == null) return m.Value;                    
//                    string todoLine;
//                    if (string.IsNullOrEmpty(parameterLine))
//                    {
//                        todoLine = string.Format(@"
//                var request = new {0}();
//                var response = Context.Proxy.Post(request);", row.ToStr("RequestName"));
//                    }
//                    else
//                    {
//                        todoLine = string.Format(@"
//                var request = new {0}()
//                {{
//                    {1}
//                }};
//                var response = Context.Proxy.Post(request);", row.ToStr("RequestName"), GetSplitParameter(parameterLine));
//                    }

//                    var ret = string.Format(@"//? {0}{1}", m.Value, todoLine);
//                    return string.Format("{0}", ret);
//                },
//            };
//            ConvertDictionaryByPost.Add(matchReplaceDelegator);

//            #endregion

            #region replacement

            ConvertDictionaryByPost.Add(new MatchReplaceDelegator(@"(Selected[\w]+Property\.)ToString\(\)", @"$1#Value#.ToString()"));
            ConvertDictionaryByPost.Add(new MatchReplaceDelegator(@"([\w]+ListProperty\[[\w]+),[\s]*([\w]+)\]", @"$1].$2"));
            ConvertDictionaryByPost.Add(new MatchReplaceDelegator(@"([\w]+ListProperty\[[\w]+\]\.[\w]+\.)Text\.", @"$1"));

            #endregion            
        }

        private void InitConvertDictionaryByServer()
        {
            MatchReplaceDelegator matchReplaceDelegator;

            var parametersAtColumn = new FullyDictionary<string, string>();
            var parametersAtIndex = new FullyDictionary<string, string>();

            #region @"(\w+)\[\s*(\d+)\s*\]\.Value\s*=\s*(.*?);",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {                
                Pattern = @"(\w+)\[\s*(\d+)\s*\]\.Value\s*=\s*(.*?);",
                MatchLogic = (m, v) =>
                {
                    var returnValue = m.Value;

                    var varName = m.Groups[1].Value;
                    var index = m.Groups[2].Value;
                    var setValue = m.Groups[3].Value;

                    var pattern = string.Format(@"{0}\[\s*{1}\s*\]\s*=\s*new SqlParameter\(""@(\w+)"",\s*(?:System\.)*(?:Data\.)*SqlDbType\.(\w+)\s*(?:,\s*(\d+)|)\)\s*;", varName, index);
                    var matches = Regex.Matches(v, pattern);

                    var m1 = matches.Cast<Match>().LastOrDefault();
                    if (m1 != null && m1.Success)
                    {
                        var parameterName = m1.Groups[1].Value;
                        var dbType = m1.Groups[2].Value;
                        var length = m1.Groups[3].Value;

                        if ((dbType == "VarChar" || dbType == "Char" || dbType == "Text" || dbType == "Decimal" || dbType == "Int" || dbType == "SmallInt") != true) Debugger.Break();

                        var setDbType = dbType == "Decimal"
                            ? ", DbType.Double"
                            : dbType == "Int" || dbType == "SmallInt"
                            ? ", DbType.Int32"
                            : ", DbType.String";
                        var parameterDirection = ", ParameterDirection.Input";
                        var setLength = length.Length < 1 ? "" : string.Format(", {0}", length);
                        if (setValue.Length < 1) setValue = "string.Empty";

                        parametersAtColumn[parameterName] = setValue;
                        parametersAtIndex[index] = setValue;

                        //var content = string.Format(@"dynamicParameters.Add(""{0}"", {1}{2}{3}{4});", parameterName, setValue, setDbType, parameterDirection, setLength);
                        var content = string.Format(@"dynamicParameters.Add(""{0}"", {1});", parameterName, setValue);
                        returnValue = content;
                    }
                    else
                    {
                        Debugger.Break();
                    }

                    return returnValue;
                },
            };
            ConvertDictionaryByServer.Add(matchReplaceDelegator);

            #endregion

            #region @"(\w+)\[\s*(\d+)\s*\]\.Direction\s*=\s*(ParameterDirection\.\w+);",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"(\w+)\[\s*(\d+)\s*\]\.Direction\s*=\s*(ParameterDirection\.\w+);",
                MatchLogic = (m, v) =>
                {
                    var returnValue = m.Value;

                    var varName = m.Groups[1].Value;
                    var index = m.Groups[2].Value;
                    var direction = m.Groups[3].Value;

                    var pattern = string.Format(@"{0}\[\s*{1}\s*\]\s*=\s*new SqlParameter\(""@(\w+)"",\s*(?:System\.)*(?:Data\.)*SqlDbType\.(\w+)\s*(?:,\s*(\d+)|)\)\s*;", varName, index);
                    var matches = Regex.Matches(v, pattern);

                    var m1 = matches.Cast<Match>().LastOrDefault();
                    if (m1 != null && m1.Success)
                    {
                        var parameterName = m1.Groups[1].Value;
                        var dbType = m1.Groups[2].Value;
                        var length = m1.Groups[3].Value;

                        if ((dbType == "VarChar" || dbType == "Char" || dbType == "Text" || dbType == "Decimal" || dbType == "Int" || dbType == "SmallInt") != true) Debugger.Break();
                        
                        var setDbType = dbType == "Decimal"
                            ? ", DbType.Double"
                            : dbType == "Int" || dbType == "SmallInt"
                            ? ", DbType.Int32"
                            : ", DbType.String";
                        var setValue = "string.Empty";
                        var parameterDirection = string.Format(", {0}", direction);
                        var setLength = length.Length < 1 ? "" : string.Format(", {0}", length);
                        
                        var content = string.Format(@"dynamicParameters.Add(""{0}"", {1}{2}{3}{4});", parameterName, setValue, setDbType, parameterDirection, setLength);
                        returnValue = content;
                    }
                    else
                    {
                        Debugger.Break();
                    }

                    return returnValue;
                },
            };
            ConvertDictionaryByServer.Add(matchReplaceDelegator);

            #endregion

            #region @"\s*=\s*(\w+\[(\d+)\]\.Value(?:\.ToString\(\))*);",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"\s*=\s*(\w+\[(\d+)\]\.Value(?:\.ToString\(\))*);",
                MatchLogic = (m, v) =>
                {
                    var returnValue = m.Value;

                    var setValue = m.Groups[1].Value;
                    var index = m.Groups[2].Value;

                    var pValue = parametersAtIndex[index];
                    if (string.IsNullOrEmpty(pValue) != true)
                    {
                        returnValue = returnValue.Replace(m.Groups[1].Value, pValue);
                    }

                    return returnValue;
                },
            };
            ConvertDictionaryByServer.Add(matchReplaceDelegator);

            #endregion
            
            #region @"[^\n]+new SqlParameter\(""@(\w+)""[^;]+",

            matchReplaceDelegator = new MatchReplaceDelegator()
            {
                Pattern = @"[^\n]+new SqlParameter\(""@(\w+)""[^;]+",
                MatchLogic = (m, v) =>
                {
                    var returnValue = m.Value;
                    var columnName = m.Groups[1].Value;

                    if (v.IndexOf(string.Format("dynamicParameters.Add(\"{0}\"", columnName)) < 0)
                    {
                        Debugger.Break();
                    }
                    else
                    {
                        returnValue = returnValue.RegexReplace(@"(\s+)(.*)", "$1//! $2");
                    }

                    return returnValue;
                },
            };
            ConvertDictionaryByServer.Add(matchReplaceDelegator);

            #endregion
        }

        private string ConvertForViewModel(string code)
        {
            // Step 1
            foreach (var item in ConvertDictionaryByPre) code = item.Replace(code);

            // Step 2
            foreach (var item in ConvertDictionaryByBinding) code = item.Replace(code);

            // Step 3
            foreach (var item in ConvertDictionaryByPost) code = item.Replace(code);

            // Step 4
            foreach (var row in ConvertDictionaryByDb.AsEnumerable())
            {
                var target = row.ToStr("Target");
                var pattern = row.ToStr("Pattern");
                var replacement = row.ToStr("Replacement");
                if (string.IsNullOrEmpty(target) && string.IsNullOrEmpty(pattern)) continue;
                if (string.IsNullOrEmpty(replacement)) continue;

                if (string.IsNullOrEmpty(target))
                {
                    if (Regex.IsMatch(code, pattern))
                        code = Regex.Replace(code, pattern, replacement);
                }
                else if (code.IndexOf(target) > 0)
                    code = code.Replace(target, replacement);
            }

            return code;
        }

        private string ConvertForServer(string code)
        {
            // Step 1
            foreach(Match m in Regex.Matches(code, @"new SqlParameter\[\d+\];(.*?)", RegexOptions.RightToLeft | RegexOptions.Singleline))
            {
                var matchCode = m.Value;
                foreach (var item in ConvertDictionaryByServer) matchCode = item.Replace(matchCode);

                code = code.Replace(m.Value, matchCode);
            }

            // Step 2
            foreach (var row in ConvertDictionaryByDbForServer.AsEnumerable())
            {
                var target = row.ToStr("Target");
                var pattern = row.ToStr("Pattern");
                var replacement = row.ToStr("Replacement");
                if (string.IsNullOrEmpty(target) && string.IsNullOrEmpty(pattern)) continue;
                if (string.IsNullOrEmpty(replacement)) continue;

                if (string.IsNullOrEmpty(target))
                {
                    if (Regex.IsMatch(code, pattern))
                        code = Regex.Replace(code, pattern, replacement);
                }
                else if (code.IndexOf(target) > 0)
                    code = code.Replace(target, replacement);
            }

            return code;
        }
        
        #endregion

        #region RegexTool...

        [InstanceNew]
        public IList<RegexToolItem> RegexList { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public RegexToolItem SelectedRegexToolItem
        {
            get { return GetPropertyValue(); }
            set
            {
                SetPropertyValue(value);
                if (value == null) return;

                SetPropertyValue(value.Pattern, "Expression");
                SetPropertyValue(value.Replace, "ReplaceRegex");
                if (string.IsNullOrEmpty(value.Text) != true) SetPropertyValue(value.Text, "Text");
                SetPropertyValue(value.Description, "RegexDescription");

                RunRegex();
            }
        }

        public string Expression
        {
            get { return GetPropertyValue(); }
            set
            {
                SetPropertyValue(value);
                RunRegex();
            }
        }
        public string ReplaceRegex
        {
            get { return GetPropertyValue(); }
            set
            {
                SetPropertyValue(value);
                RunRegex();
            }
        }
        public string RegexDescription { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public string Text
        {
            get { return GetPropertyValue(); }
            set
            {
                SetPropertyValue(value);
                RunRegex();
            }
        }

        public string MatchResult { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string CaptureOnly { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string MatchGroups { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string CaptureGroups { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public RegexOptions RegexOptions
        {
            get { return GetPropertyValue("RegexOptions"); }
            set
            {
                var currentValue = GetPropertyValue("RegexOptions");
                if (currentValue.HasFlag(value))
                    currentValue &= ~value;
                else
                    currentValue |= value;

                SetPropertyValue(currentValue, "RegexOptions");

                RunRegex();
            }
        }

        private void InitRegexTool()
        {
            LoadRegexList();
        }

        private void LoadRegexList()
        {
            var conn = SqlHelper.GetConnection(AppManager.DataConnectionString);
            var query = "SELECT * FROM TBL_RegexTool";

            var dt = new DataTable();
            dt.Fill(conn, query);

            var list = dt.AsEnumerable().Select(p => 
                new RegexToolItem()
                {
                    Pattern = p.Field<string>("Pattern"),
                    Replace = p.Field<string>("Replace"),
                    seqID = p.Field<int>("seqID"),
                    Text = p.Field<string>("Text"),
                    Description = p.Field<string>("Description"),
                });

            RegexList = list.ToList();

//            RegexList.Clear();
//            var regexList = AppManager.Current.Settings.Get("RegexList");
//            if (string.IsNullOrEmpty(regexList))
//            {
//                regexList = string.Join("@#$",
//                    new string[] { @"(?<=#region )[\w]+"
//                        , 
//@"[^\n]+(FrozenColumnCount|OperationMode|IMEMode|CharacterCasing|EditMode|AllowAutoSort|[\w]+CellType|DecimalPlaces|FixedPoint|UserDefinedFormat|\.Focused =)[^\n]+"
//                        , @"(?:{)(.*?)(?:})"
//                        , @".*("".*?"").*("".*?"").*#$%ComCodeModelList.Add(new ComCodeModel() { ComCdNm = $1, ComCd = $2 });"
//                    });
//            }

//            foreach (var item in regexList.Split("@#$"))
//            {
//                RegexList.Add(item);
//            }
        }

        private void RunRegex()
        {
            if (string.IsNullOrEmpty(this.Text)) return;
            
            try
            {                
                if (string.IsNullOrEmpty(ReplaceRegex))
                {
                    var matches = Regex.Matches(this.Text, this.Expression, this.RegexOptions).Cast<Match>();
                    var sbResult = new StringBuilder();                    
                    matches.ToList().ForEach(p => sbResult.AppendLine(p.Value));
                    this.MatchResult = sbResult.ToString();
                    this.CaptureOnly = sbResult.ToString();
                    this.MatchGroups = GetMatchesGroup(matches);
                    this.CaptureGroups = GetCaptureGroups(matches);
                }
                else
                {
                    this.MatchResult = this.Text.RegexReplace(this.Expression, ReplaceRegex);                                        
                    var regex = new Regex(this.Expression, RegexOptions.None);
                    var matches = Regex.Matches(this.Text, this.Expression).Cast<Match>();
                    var sbResult = new StringBuilder();
                    matches.ToList().ForEach(p => sbResult.AppendLine(regex.Replace(p.Value, ReplaceRegex)));
                    this.CaptureOnly = sbResult.ToString();
                    var m = Regex.Matches(this.Text, this.Expression).Cast<Match>();
                    this.MatchGroups = GetMatchesGroup(m);
                    this.CaptureGroups = GetCaptureGroups(m);
                }
            }
            catch(Exception e)
            {
                this.MatchResult = e.Message;
            }
        }

        private string GetMatchesGroup(IEnumerable<Match> matches)
        {
            var sbGroup = new StringBuilder();

            bool first = false;
            foreach (var match in matches)
            {
                first = true;
                string line = string.Empty;
                foreach (Group group in match.Groups)
                {
                    if (first) { first = false; continue; }
                    line = line.ConcatDiv(", ", group.Value);
                }
                sbGroup.AppendLine(line);
            }

            return sbGroup.ToString();
        }

        private string GetCaptureGroups(IEnumerable<Match> matches)
        {
            var dic = new FullyDictionary<int, string>();

            bool first = false;
            foreach (var match in matches)
            {
                first = true;
                string line = string.Empty;
                foreach (Group group in match.Groups)
                {
                    if (first) { first = false; continue; }
                    line = line.ConcatDiv(", ", group.Value);

                    dic[group.Index] += line.ConcatDiv(", ", group.Value);
                }                
            }

            var sbGroup = new StringBuilder();

            foreach (var d in dic)
            {
                sbGroup.AppendLine(string.Format(@"{0} Groups", d.Key));
                sbGroup.AppendLine(d.Value);
                sbGroup.AppendLine();
            }

            return sbGroup.ToString();
        }

        public class RegexToolItem
        {
            public string Pattern { get; set; }
            public string Replace { get; set; }
            public string Text { get; set; }
            public string Description { get; set; }
            public int seqID { get; set; }

            public override string ToString()
            {
                return string.Format(@"{0}
    {1}", Pattern, Replace);
            }
        }

        #endregion

        #region IndexMatch...

        public string IndexMatchAsis { get { return GetPropertyValue(); } set { SetPropertyValue(value); RunIndexMatch(); } }
        public string IndexMatchNames { get { return GetPropertyValue(); } set { SetPropertyValue(value); RunIndexMatch(); } }
        public string IndexMatchTobe { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        private void InitIndexMatch()
        {
            this.IndexMatchAsis = @"spdRsRcptSF2.ActiveSheet.Columns[10].CellType = txt;
spdRsRcptSF2.ActiveSheet.Columns[2].CellType = txt;
spdRsRcptSF2.ActiveSheet.Columns[3].CellType = txt;
spdRsRcptSF2.ActiveSheet.Columns[8].CellType = txt;
spdRsRcptSF2.ActiveSheet.Columns[9].CellType = txt;";

            this.IndexMatchNames = @"ImageNm
DrgNm
IngrdNm
IdFactor
TQty
Qty
Frq
EfcyDesc
OCSDrgNm
Rem
OrdSeq
PharmCd
ImagePath
DrgCd
DrgCd";
        }

        public void RunIndexMatch()
        {
            if (string.IsNullOrEmpty(IndexMatchNames)) return;

            var names = IndexMatchNames.Split('\n');
            var code = IndexMatchAsis;

            //var result = Regex.Replace(IndexMatchAsis, @"\[[\d]+\]", m =>
            //    {
            //        var index = Convert.ToInt32(m.Value);
            //        return names.Length > index ? names.ElementAt(index).Trim() : m.Value;
            //    });

            #region @"\[[^\]]+]\[[^\]]+]",

            var mrd = new MatchReplaceDelegator()
            {
                Pattern = @"\[[^\]]+]\[[^\]]+]",
                MatchLogic = (m, v) =>
                {
                    //selectList[i]["Qty"]
                    var target = m.Value.LastBetween("[", "]", true);
                    var name = m.Value.LastBetween("[", "]", false);
                    if (name.StartsWith(@"""") && name.EndsWith(@""""))
                    {
                        var ret = m.Value.Replace(target, "." + name.Between(@"""", @""""));
                        return ret;
                    }
                    else
                    {
                        var index = Convert.ToInt32(name);
                        name = names.Length > index ? names.ElementAt(index).Trim() : m.Value;
                        var ret = m.Value.Replace(target, "." + name);
                        return ret;
                    }
                },
            };
            code = mrd.Replace(code);

            #endregion

            #region @"Columns\[[\d]+\]",

            mrd = new MatchReplaceDelegator()
            {
                Pattern = @"Columns\[[\d]+\]",
                MatchLogic = (m, v) =>
                {
                    //Columns.[4]
                    var value = m.Value.RegexReturn(@"[\d]+");
                    var index = Convert.ToInt32(value);
                    var name = names.Length > index ? names.ElementAt(index).Trim() : m.Value;
                    return name;
                },
            };
            code = mrd.Replace(code);

            #endregion

            #region @"\[[ \w]+,[ \w]+\]",

            mrd = new MatchReplaceDelegator()
            {
                Pattern = @"\[[ \w]+,[ \w]+\]",
                MatchLogic = (m, v) =>
                {
                    var value = m.Value.Between("[", "]", false, true);
                    var valueArray = value.Split(",");
                    var row = valueArray.ElementAt(0).Trim();
                    var column = valueArray.ElementAt(1).RegexReturn(@"[\d]+");
                    if (string.IsNullOrEmpty(column))
                        return m.Value;
                    var index = Convert.ToInt32(column);
                    var name = names.Length > index ? names.ElementAt(index).Trim() : m.Value;
                    return string.Format(@"[{0}].{1}", row, name);
                },
            };
            code = mrd.Replace(code);

            #endregion

            #region @"\[[\d]+\](\.Text)*",

            mrd = new MatchReplaceDelegator()
            {
                Pattern = @"\[[\d]+\](\.Text)*",
                MatchLogic = (m, v) =>
                {
                    var value = m.Value.Between("[", "]", false, true);
                    var column = value.RegexReturn(@"[\d]+");
                    if (string.IsNullOrEmpty(column))
                        return m.Value;
                    var index = Convert.ToInt32(column);
                    var name = names.Length > index ? names.ElementAt(index).Trim() : m.Value;
                    return string.Format(@".{0}", name);
                },
            };
            code = mrd.Replace(code);

            #endregion

            #region @"\.[\d]+((\.)*)",

            mrd = new MatchReplaceDelegator()
            {
                Pattern = @"\.[\d]+((\.)*)",
                MatchLogic = (m, v) =>
                {
                    var value = m.Value.Between(".", ".", false, true);
                    var column = value.RegexReturn(@"[\d]+");
                    if (string.IsNullOrEmpty(column))
                        return m.Value;
                    var index = Convert.ToInt32(column);
                    var name = names.Length > index ? names.ElementAt(index).Trim() : m.Value;
                    return string.Format(@".{0}{1}", name, m.Groups[1].Value);
                },
            };
            code = mrd.Replace(code);

            #endregion

            #region @"Property\.(\d+)",

            mrd = new MatchReplaceDelegator()
            {
                Pattern = @"Property\.(\d+)",
                MatchLogic = (m, v) =>
                {
                    var index = Convert.ToInt32(m.Groups[1].Value);
                    var name = names.Length > index ? names.ElementAt(index).Trim() : m.Value;
                    return string.Format(@"Property.{0}", name);
                },
            };
            code = mrd.Replace(code);

            #endregion

            #region @"\[.*?\]",

            mrd = new MatchReplaceDelegator()
            {
                Pattern = @"\[(.*?)\]",
                MatchLogic = (m, v) =>
                {
                    var value = m.Groups[1].Value;
                    if (value.IndexOf(",") > 0)
                    {
                        var matches = Regex.Matches(value, @"\d+");
                        
                        string indexStr = string.Empty;
                        if (matches.Count < 0 || matches.Count > 2)
                            return m.Value;
                        if (matches.Count == 1)
                            indexStr = matches.Cast<Match>().First().Value;
                        else if (matches.Count == 2)
                            indexStr = matches.Cast<Match>().First().Value;

                        if (string.IsNullOrEmpty(indexStr)) return m.Value;

                        var index = Convert.ToInt32(indexStr);
                        var name = names.Length > index ? names.ElementAt(index).Trim() : indexStr;
                        return m.Value.Replace(indexStr, name);
                    }
                    else
                    {
                        return m.Value;
                    }

                },
            };
            code = mrd.Replace(code);

            #endregion

            IndexMatchTobe = code;
        }

        #endregion

        #region Splitter...

        public string SplitterAsis { get { return GetPropertyValue(); } set { SetPropertyValue(value); RunSplitter(); } }
        public string SplitterTobe { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string Prefix { get { return GetPropertyValue(); } set { SetPropertyValue(value); RunSplitter(); } }

        private void InitSplitter()
        {
            this.SplitterAsis = @"string pGubun, string pDrgstGb, string pFrYmd, string pToYmd, string pFrPrtNo, string pToPrtNo, 
			string pWard, string pUnitNo, string pDrgCd, string pOrdGb, string pPrescGb, string pFinishDgr, string pPrtGb, string pRosset";
        }

        private void RunSplitter()
        {
            var ret = GetSplitParameter(SplitterAsis);
            this.SplitterTobe = ret;
        }

        private string GetSplitParameter(string parameterLine)
        {
            var sb = new StringBuilder();
            if (parameterLine.Contains(".") && parameterLine.Contains(" = ") && parameterLine.Contains(";"))
            {
                if (parameterLine.Contains("new SqlParameter(") && parameterLine.Contains("@"))
                {
                    // @파라미터 로 구분
                    foreach (var item in parameterLine.RegexMatches(@"@[\w]+"))
                    {
                        var property = item.Substring(1);
                        var parameter = item.Substring(1);
                        if (parameter.StartsWith("i")) parameter = parameter.Substring(1);
                        sb.AppendLine(string.Format("{0} = {1},", property, Prefix + parameter));
                    }
                }
                else
                {
                    // ;세미콜론 으로 구분
                    foreach (var item in parameterLine.Split(";"))
                    {
                        var property = item.Between(".", "=").Trim();
                        var parameter = item.RightBySearch("=").Trim();
                        if (string.IsNullOrEmpty(property) || string.IsNullOrEmpty(parameter)) continue;

                        property = property.StartsWith("p") || property.StartsWith("s") ? property.Substring(1) : property;
                        if (parameter.StartsWith("i")) parameter = parameter.Substring(1);

                        sb.AppendLine(string.Format("{0} = {1},", property, Prefix + parameter));
                    }
                }
            }
            else if (parameterLine.Contains("Add(") && parameterLine.Contains(";"))
            {
                foreach (var item in parameterLine.RegexBetween("Add(", ");"))
                {
                    if (item.IndexOf(",") < 1)
                    {
                        var value = item;
                        var property = value.StartsWith("p") || value.StartsWith("s") ? value.Substring(1) : value;
                        var parameter = value;
                        if (parameter.StartsWith("i")) parameter = parameter.Substring(1);

                        sb.AppendLine(string.Format("{0} = {1},", property, Prefix + parameter));
                    }
                    else
                    {
                        var value = item;
                        var proeprty = StringExtensions.SplitAt(value, ",", 0).Replace(@"""", "");
                        var pValue = StringExtensions.SplitAt(value, ",", 1);

                        sb.AppendLine(string.Format("{0} = {1},", proeprty, pValue));
                    }
                }
            }
            else if (parameterLine.Contains("public string") && parameterLine.Contains("{ get;") && parameterLine.Contains("}"))
            {
                foreach (var item in parameterLine.RegexBetween("public string ", " {"))
                {
                    var value = item;
                    var property = value;
                    var parameter = value;
                    if (parameter.StartsWith("i")) parameter = parameter.Substring(1);

                    sb.AppendLine(string.Format("{0} = {1},", property, Prefix + parameter));
                }
            }
            else if (parameterLine.Contains(@" = """";"))
            {
                foreach (var item in parameterLine.Split(";"))
                {
                    var value = item.Trim().RegexReturn(@"[\w]+");
                    if (string.IsNullOrEmpty(value)) continue;

                    var property = value;
                    var parameter = value;
                    if (parameter.StartsWith("i")) parameter = parameter.Substring(1);

                    sb.AppendLine(string.Format("{0} = {1},", property, Prefix + parameter));
                }
            }
            else
            {
                // , 콤마로 구분
                foreach (var item in parameterLine.Split(","))
                {
                    //var value = item.Trim();
                    var value = item.Trim();
                    if (value.IndexOf(" ") > 0)
                        value = value.Substring(value.IndexOf(" ") + 1);
                    if (value.IndexOf(".") > 0)
                        value = value.Substring(value.IndexOf(".") + 1);

                    value = value.RegexReturn(@"[\w]+");
                    if (string.IsNullOrEmpty(value)) continue;

                    var property = value.StartsWith("p") || value.StartsWith("s") ? value.Substring(1) : value;
                    var parameter = value;
                    if (parameter.StartsWith("i")) parameter = parameter.Substring(1);

                    sb.AppendLine(string.Format("{0} = {1},", property, Prefix + parameter));
                }
            }

            return sb.ToString();
        }

        #endregion

        #region UserWork...

        public IEnumerable<IUserWorkItem> UserWorks { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public IUserWorkItem SelectedUserWork 
        { 
            get { return GetPropertyValue(); } 
            set 
            { 
                SetPropertyValue(value);
                UserWorkInput = value.SampleInput;
                //RunUserWork(); 
            } 
        }

        public string UserWorkInput { get { return GetPropertyValue(); } set { SetPropertyValue(value); RunUserWork(); } }
        public string UserWorkOutput { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        private void InitUserWork()
        {
            this.UserWorks = LoadUserWorks();
        }

        private void RunUserWork()
        {
            if (SelectedUserWork == null) return;
            UserWorkOutput = SelectedUserWork.Run(UserWorkInput);
        }

        private IEnumerable<IUserWorkItem> LoadUserWorks()
        {
            yield return new GenDataColumn();
        }

        public interface IUserWorkItem
        {
            string Run(string UserWorkInput);
            string SampleInput { get; }
        }

        public class GenDataColumn : IUserWorkItem
        {
            public override string ToString()
            {
                return "DataColumn Generate!";
            }

            public string Run(string input)
            {
                var sbResult = new StringBuilder();

                var rows = input.Split("\r\n");
                foreach (var row in rows)
                {   
                    var values = row.Split("\t");

                    var dic = new Dictionary<string, string>();
                    dic.Add("Binding", values.Length > 0 ? values[0] : null);
                    dic.Add("Title", values.Length > 1 ? values[1] : null);
                    dic.Add("DefaultValue", values.Length > 2 ? values[2] : null);
                    dic.Add("CellType", values.Length > 3 ? values[3] : null);
                    dic.Add("DataType", values.Length > 4 ? values[4] : null);
                    dic.Add("Hide", values.Length > 5 ? values[5] : null);
                    dic.Add("NotNull", values.Length > 6 ? values[6] : null);
                    dic.Add("ReadOnly", values.Length > 7 ? values[7] : null);
                    dic.Add("Length", values.Length > 8 ? values[8] : null);
                    dic.Add("Width", values.Length > 9 ? values[9] : null);
                    dic.Add("HorizontalAlignment", values.Length > 10 ? values[10] : null);
                    dic.Add("BackColor", values.Length > 11 ? values[11] : null);
                    dic.Add("Format", values.Length > 12 ? values[12] : null);

                    var sb = new StringBuilder();

                    if (string.IsNullOrEmpty(dic["Title"]) != true)
                        sb.Append(string.Format(@" Header=""{0}""", dic["Title"]));

                    if (string.IsNullOrEmpty(dic["Binding"]) != true)
                        sb.Append(string.Format(@" DataMemberBinding=""{{Binding {0}}}""", dic["Binding"]));

                    if (string.IsNullOrEmpty(dic["Hide"]) != true && Convert.ToBoolean(dic["Hide"]))
                        sb.Append(string.Format(@" IsVisible=""True"""));

                    if (string.IsNullOrEmpty(dic["Width"]) != true)
                        sb.Append(string.Format(@" Width=""{0}""", dic["Width"]));

                    if (sb.Length < 1) continue;
                    sbResult.AppendLine(string.Format(@"<saf:SafTelerikGridViewDataColumn{0} />", sb.ToString()));
                }

                return sbResult.ToString();
            }


            public string SampleInput
            {
                get 
                {
                    return @"OrdYmd	처방일자		TextCell		FALSE		TRUE		67	Left		
StusNm	상태		TextCell		FALSE		TRUE		33	Left		
PrtNo	출력번호		TextCell		FALSE		TRUE		75	Left		

		<< sample : 화면분 분석서의 DataColumn 붙여넣기 >>";
                }
            }
        }

        #endregion
    }
}