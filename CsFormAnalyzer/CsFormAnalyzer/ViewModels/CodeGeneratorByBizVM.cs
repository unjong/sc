using CsFormAnalyzer.Core;
using CsFormAnalyzer.Mvvm;
using CsFormAnalyzer.Utils;
using CsFormAnalyzer.Views;
using SC.WPF.Tools.CodeHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CsFormAnalyzer.ViewModels
{
    public partial class CodeGeneratorByBizVM : ViewModelBase
    {
        #region Fields

        private int maxStep = 1;
        private DataTable _ViewBiz;
        private DataTable _TblSpModel;

        #endregion

        #region Properties

        public int Step
        {
            get { return GetPropertyValue(); }
            set
            {
                SetPropertyValue(value);
                RunStep(value);
            }
        }

        public IEnumerable<FacadeItem> FacadeItems { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public FacadeItem SelectedFacadeItem
        {
            get { return GetPropertyValue(); }
            set
            {
                SetPropertyValue(value);
                FacadeMethodItems = FacadeMethodItem.GetMethodItems(_ViewBiz, value);

                ClearResult();                
                if (value == null) SelectedFacadeMethodItem = null;
            }
        }

        public IEnumerable<FacadeMethodItem> FacadeMethodItems { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public FacadeMethodItem SelectedFacadeMethodItem 
        { 
            get { return GetPropertyValue(); } 
            set 
            { 
                SetPropertyValue(value); 

                ClearResult();

                if (this.IsLoaded && value != null)
                {
                    base.InvokeAsyncAction(delegate
                    {
                        var firstRow = _ViewBiz.Select(string.Format(@"LAYER='Facade' AND NAMESPACE='{0}' AND CLASSNM='{1}' AND METHODNM='{2}'"
                            , value.Namespace, value.ClassName, value.MethodName)).First();
                        if (firstRow == null) return;

                        SelectedBizItem = BizItems.Where(p => p.Name == firstRow.ToStr("CALLOBJNM")).FirstOrDefault();
                        if (SelectedBizItem == null) return;

                        SelectedMethodItem = MethodItems.Where(p => p.MethodName == firstRow.ToStr("CALLFUNCNM")).FirstOrDefault();
                    });
                }
            } 
        }
        
        public IEnumerable<BizItem> BizItems { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public BizItem SelectedBizItem 
        { 
            get { return GetPropertyValue(); } 
            set 
            { 
                SetPropertyValue(value); 
                MethodItems = MethodItem.GetMethodItems(_ViewBiz, value);

                ClearResult();
                if (value == null) SelectedMethodItem = null;
            } 
        }

        public IEnumerable<MethodItem> MethodItems { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public MethodItem SelectedMethodItem 
        { 
            get { return GetPropertyValue(); } 
            set 
            { 
                SetPropertyValue(value);
                DAItems = DAItem.GetDAItems(_ViewBiz, value);
                
                ClearResult();

                if (value == null) return;
                TobeNamespace = SelectedBizItem.GetTobeNamespace(false);
                TobeBaseName = value.MethodName + SelectedBizItem.Postfix;
            } 
        }

        public IEnumerable<DAItem> DAItems { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public string TobeNamespace { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string TobeBaseName { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        [InstanceNew]
        public ObservableCollection<UspItem> UspItems { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }


        public string ModelCode { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string RequestCode { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string ServiceCode { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string CallRequestCode { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        
        private void ClearResult()
        {
            ModelCode = string.Empty;
            RequestCode = string.Empty;
            ServiceCode = string.Empty;
            CallRequestCode = string.Empty;
        }

        public override void InitProperties()
        {
            base.InitProperties();
                        
            this._ViewBiz = WorkService.Current.GetViewBizInfo();
            this._TblSpModel = WorkService.Current.GetTblSpModel();

            FacadeItems = FacadeItem.GetItemFromBizTable(_ViewBiz);
            BizItems = BizItem.GetItemFromBizTable(_ViewBiz);

            UspItems.CollectionChanged += delegate
            {
                ClearResult();
            };
        }

        #endregion

        #region Commands

        public ICommand PreviousCommand { get; private set; }
        public ICommand NextCommand { get; private set; }

        public ICommand AddNewUspItemCommand { get; private set; }
        public ICommand ShowMethodCommand { get; private set; }
        public ICommand OpenFileCommand { get; private set; }

        public ICommand GenCommand { get; private set; }
        public ICommand ShowCodeCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
                
        public override void InitCommands()
        {
            PreviousCommand = base.CreateCommand(delegate { Step -= 1; }, delegate { return Step > 0; });
            NextCommand = base.CreateCommand(delegate { if (CanNext(Step)) Step += 1; }, delegate { return Step < maxStep; });

            AddNewUspItemCommand = base.CreateCommand(OnExecuteAddNewUspItemCommand);
            ShowMethodCommand = base.CreateCommand(OnExecuteShowMethodCommand);
            OpenFileCommand = base.CreateCommand(OnExecuteOpenFileCommand);

            GenCommand = base.CreateCommand(OnExecuteGenCommand);
            ShowCodeCommand = base.CreateCommand(OnExecuteShowCodeCommand, OnCanExecuteShowCodeCommand);
            SaveCommand = base.CreateCommand(OnExecuteSaveCommand, OnCanExecuteSaveCommand);
        }

        private void OnExecuteAddNewUspItemCommand(object obj)
        {
            var vm = ViewModelLocator.Current.GetInstance<ModelGeneratorByUspVM>(true);
            vm.BaseName = TobeBaseName;

            if (obj is DAItem)
            {
                var daItem = obj as DAItem;                
                vm.UspName = daItem.UspName;                
            }
            
            if (AppManager.Current.ShowDialogView(typeof(ModelGeneratorByUspView), vm))
            {
                var uspItem = new UspItem()
                {
                    Name = vm.UspName,
                    SpResults = vm.ResultSpItems,
                };

                UspItems.Add(uspItem);
            }
        }

        private void OnExecuteShowMethodCommand(object obj)
        {
            if (obj is MethodItem)
            {
                var item = obj as MethodItem;
                var path = Core.WorkService.Current.GetFullFilePath(item.Namespace, item.ClassName);
                if (string.IsNullOrEmpty(path)) return;

                var fullCode = IOHelper.ReadFileToString(path);
                var pattern = @"(public|private) [\w]+ " + item.MethodName + @"\(";
                var search = StringExtensions.RegexReturn(fullCode, pattern);

                var vm = ViewModelLocator.Current.GetInstance<CodeViewerVM>(true);
                vm.Code = fullCode;
                vm.SearchText = search;

                AppManager.Current.ShowView(typeof(CodeViewerView), vm);
            }
            else if (obj is DAItem)
            {
                var item = obj as DAItem;
                var path = Core.WorkService.Current.GetFullFilePath(item.Namespace, item.ClassName);
                if (string.IsNullOrEmpty(path)) return;

                var fullCode = IOHelper.ReadFileToString(path);
                var pattern = @"(public|private) [\w]+ " + item.MethodName + @"\(";
                var search = StringExtensions.RegexReturn(fullCode, pattern);

                var vm = ViewModelLocator.Current.GetInstance<CodeViewerVM>(true);
                vm.Code = fullCode;
                vm.SearchText = search;

                AppManager.Current.ShowView(typeof(CodeViewerView), vm);
            }
        }

        private void OnExecuteOpenFileCommand(object obj)
        {
            if (obj is MethodItem)
            {
                var item = obj as MethodItem;
                var path = Core.WorkService.Current.GetFullFilePath(item.Namespace, item.ClassName);
                Process.Start(path);
            }
            else if (obj is DAItem)
            {
                var item = obj as DAItem;
                var path = Core.WorkService.Current.GetFullFilePath(item.Namespace, item.ClassName);
                Process.Start(path);
            }
        }

        private void OnExecuteGenCommand(object obj)
        {
            ActGenerate();
        }

        private void OnExecuteShowCodeCommand(object obj)
        {
            if (this.SelectedFacadeItem == null || this.SelectedFacadeMethodItem == null) return;

            var facadeNamespace = this.SelectedFacadeItem.Namespace;
            var facadeClassNm = this.SelectedFacadeItem.Name;
            var facadeMethodNm = this.SelectedFacadeMethodItem.MethodName;

            var rows = _TblSpModel.Select(string.Format(@"FacadeNameSpace='{0}' AND FacadeClassNM='{1}' AND FacadeMethodNM='{2}'"
                , facadeNamespace, facadeClassNm, facadeMethodNm));

            var requestName = rows.First().ToStr("Location").Replace(".cs", "");
            var code = string.Format(@"
            var request = new {0}()
            {{{1}
            }};
            var response = Context.Proxy.Post<{2}>(request);
", requestName, null, requestName.Replace("Request", "Response"));

            var txtEditor = new ICSharpCode.AvalonEdit.TextEditor();
            txtEditor.FontFamily = new FontFamily("Consolas");
            txtEditor.FontSize = (double)new FontSizeConverter().ConvertFrom("10pt");
            txtEditor.ShowLineNumbers = true;
            txtEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("C#");
            txtEditor.Text = code;

            this.ShowPopup(txtEditor);
        }

        private bool OnCanExecuteShowCodeCommand()
        {
            if (this.SelectedFacadeItem == null || this.SelectedFacadeMethodItem == null) return false;

            var facadeNamespace = this.SelectedFacadeItem.Namespace;
            var facadeClassNm = this.SelectedFacadeItem.Name;
            var facadeMethodNm = this.SelectedFacadeMethodItem.MethodName;

            var rows = _TblSpModel.Select(string.Format(@"FacadeNameSpace='{0}' AND FacadeClassNM='{1}' AND FacadeMethodNM='{2}'"
                , facadeNamespace, facadeClassNm, facadeMethodNm));

            return rows.Count() > 0;
        }

        private void OnExecuteSaveCommand(object obj)
        {
            foreach(var spResult in this.GetSpResults())
            {
                var spName = spResult.SpInfo.SPName;
                var modelName = spResult.ModelName;
                var baseName = TobeBaseName;
                var requestName = baseName + "Request";
                var responseName = baseName + "Response";
                var location = TobeNamespace + "." + requestName;
                var userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                var facadeNamespace = this.SelectedFacadeItem.Namespace;
                var facadeClassNm = this.SelectedFacadeItem.Name;
                var facadeMethodNm = this.SelectedFacadeMethodItem.MethodName;

                WorkService.Current.SaveSpModel(spName, modelName, requestName, responseName, location, facadeNamespace, facadeClassNm, facadeMethodNm);
            }

            _TblSpModel = WorkService.Current.GetTblSpModel();
            MessageBox.Show("저장되었습니다.");
        }

        private bool OnCanExecuteSaveCommand()
        {
            return string.IsNullOrEmpty(ServiceCode) != true;
        }

        #endregion

        #region Methods

        private void RunStep(int step)
        {
        }

        private bool CanNext(int step)
        {
            try
            {
                if (step == 0)
                {
                    ActGenerate();
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private void ActGenerate()
        {
            if (TobeNamespace == null) throw new ArgumentNullException("TobeNamespace");
            if (TobeBaseName == null) throw new ArgumentNullException("TobeBaseName");
            //if (UspItems.Count < 1) throw new ArgumentNullException("UspItems");

            ModelCode = GetModelCode();
            //RequestCode = GetRequestCode();
            RequestCode = string.Format(@"{0}
using SAF.Model;
using System.Collections.Generic;

namespace {1}
{{{2}{3}
}}", GetClassComment(false), TobeNamespace, GetRequestCode(), ModelCode.Length < 1 ? string.Empty : string.Format(@"

    #region Model
{0}
    #endregion", ModelCode));
            ServiceCode = string.Format(@"{0}
using SAF.Server;
using SAF.Server.Data;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using {1};

namespace {2}
{{{3}
}}
", GetClassComment(true), TobeNamespace, TobeNamespace.Replace("Model", "Server"), GetServiceCode());

            CallRequestCode = GetCallRequestCode();
        }

        #region Code Generate...

        private string GetCallRequestCode()
        {
            var sbPropertyAssign = new StringBuilder();

            var parameters = GetRequestParameters(this.UspItems);
            parameters.Distinct().ToList().ForEach(p =>
            {
                sbPropertyAssign.Append(string.Format(@"
                {0} = {1},", p, p.StartsWith("i") ? "p" + p.Substring(1) : "p" + p));
            });

            var sbPropertyDefine = new StringBuilder();
            parameters.Distinct().ToList().ForEach(p =>
            {
                sbPropertyDefine.Append(string.Format(@"string {0}, ", p.StartsWith("i") ? "p" + p.Substring(1) : "p" + p));
            });

            var request = string.Format(@"            var request = new {0}()
            {{{1}
            }};
            var response = Context.Proxy.Post<{2}>(request);"
                , TobeNamespace + "." + TobeBaseName + "Request"
                , sbPropertyAssign.ToString()
                , TobeNamespace + "." + TobeBaseName + "Response");

            var sb = new StringBuilder();
            sb.AppendLine(request);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(sbPropertyDefine.ToString());

            return sb.ToString();
        }

        private string GetModelCode()
        {
            var sb = new StringBuilder();
            var modelList = GetModelList();

            foreach(var model in modelList)
            {
                var sbProperty = new StringBuilder();
                foreach(var name in model.ModelPropertyList)
                {
                    var smallName = "_" + name.Left(1).ToLower() + name.Substring(1);
                    sbProperty.AppendLine(string.Format(@"
        #region {0}
        private string {1};
        public string {0}
        {{
            get {{ return {1}; }}
            set {{ RaiseAndSetIfChanged(ref {1}, value); }}
        }}
        #endregion", name, smallName));
                }

                sb.AppendLine(string.Format(@"
    ///<summary>
    ///{0}에서 생성
    ///</summary>
    public class {1} : SAFModel
    {{{2}
    }}", model.SPName, model.ModelName, sbProperty.ToString()));
            }

            return sb.ToString();
        }

        private string GetRequestCode()
        {
            var sb = new StringBuilder();
            var sbProperty = new StringBuilder();
            var sbPropertyInit = new StringBuilder();
            var sbResponseProperty = new StringBuilder();
            var name = TobeBaseName;

            var parameters = GetRequestParameters(this.UspItems);
            parameters.Distinct().ToList().ForEach(p =>
                {
                    sbProperty.Append(string.Format(@"
        public string {0} {{ get; set; }}", p));
                    sbPropertyInit.Append(string.Format(@"
            {0} = string.Empty;", p));
                });

            var cnt = 0;
            foreach (var uspItem in this.UspItems)
            {                
                foreach (var spResult in uspItem.SpResults)
                {
                    if (spResult.SpInfo.IsSaveSP)
                    {
                        sbResponseProperty.Append(string.Format(@"
        public int ReturnValue{0} {{ get; set; }}", cnt < 1 ? "" : cnt.ToString()));
                    }
                    else
                    {
                        sbResponseProperty.Append(string.Format(@"
        public IList<{0}> {0}List {{ get; set; }}", spResult.ModelName));
                    }

                    cnt++;
                }
            }

            sb.Append(string.Format(@"
    public class {0}Request : SAF.Model.ISAFReturn<{0}Response>
    {{{1}

        public {0}Request()
        {{{2}
        }}
    }}

    public class {0}Response
    {{{3}
    }}", name, sbProperty.ToString(), sbPropertyInit.ToString(), sbResponseProperty.ToString()));

            return sb.ToString();
        }

        private string GetServiceCode()
        {
            var sb = new StringBuilder();
            var sbAny = new StringBuilder();
            var sbPropertyInit = new StringBuilder();
            var name = TobeBaseName;
            var ns = TobeNamespace.Replace("Model", "Server");
            var dbName = ns.SplitAt(".", 1).ToUpper();

            bool isTx = SelectedBizItem.IsTx;

            int cnt = 0;
            foreach (var uspItem in this.UspItems)
            {                
                if (uspItem.SpResults.Count() < 1) continue;
                sbPropertyInit.Clear();

                var spResult = uspItem.SpResults.First();
                var spDbName = WorkService.Current.GetHis3DbName(spResult.SpInfo.DBName);

                var parameters = GetRequestParameters(spResult.SpInfo);
                parameters.Distinct().ToList().ForEach(p =>
                {
                    sbPropertyInit.Append(string.Format(@"
                    {0} = request.{0},", p));
                });
                var propertyInit = string.Empty;
                if (sbPropertyInit.Length > 0)
                {
                    propertyInit = string.Format(@"new 
                {{{0}
                }}", sbPropertyInit.ToString());
                }

                var callMethodName = spResult.SpInfo.IsSaveSP
                    ? "ExecuteAsync"
                    : uspItem.SpResults.Count() > 1
                    ? "QueryMultipleAsync"
                    : string.Format("QueryAsync<{0}>", spResult.ModelName);

                sbAny.AppendLine(string.Format(@"

            var data{0} = await DataProvider.{1}(""{2}"",
                {3}, {4}, System.Data.CommandType.{5}{6});"
                    , cnt < 1 ? string.Empty : cnt.ToString(),
                    callMethodName,
                    spResult.SpInfo.SPName,
                    propertyInit,
                    spDbName == dbName ? "null" : string.Format(@"""{0}""", spDbName),
                    spResult.SpInfo.SPName.IndexOf("USP") < 0 && spResult.SpInfo.SPName.IndexOf(" ") > 0 ? "Text" : "StoredProcedure",
                    SelectedBizItem.IsTx ? ", null, uw" : ""
                    ));
                
                foreach (var result in uspItem.SpResults)
                {
                    if (result.SpInfo.IsSaveSP)
                    {
                        sbAny.Append(string.Format(@"
            ret.ReturnValue{0} = data;", cnt < 1 ? string.Empty : cnt.ToString()));
                    }
                    else
                    {
                        if (uspItem.SpResults.Count() > 1)
                        {
                            sbAny.Append(string.Format(@"
            if (data{0}.HasMoreResults) ret.{1}List = data{0}.NextResult<{1}>();"
                                , cnt < 1 ? string.Empty : cnt.ToString(),
                                result.ModelName));
                        }
                        else
                        {
                            sbAny.Append(string.Format(@"
            ret.{1}List = data{0};"
                                , cnt < 1 ? string.Empty : cnt.ToString(),
                                result.ModelName));
                        }
                    }
                }

                cnt++;
            }

            if (SelectedBizItem.IsTx)
            {
                sb.Append(string.Format(@"
    class {0}Service : SAFService
    {{
        public override string DefaultConnectionName
        {{
            get
            {{
                return ""{1}"";
            }}
        }}
        
        public async Task<{0}Response> Any({0}Request request, IUnitOfWork uw)
        {{
            bool requireCommit = His3.Hp.Server.Common.HpServerUtil.IsNewTran(ref uw);

            var ret = new {0}Response();{2}

            if(requireCommit)
            {{
                uw.Commit();
                uw.Dispose();
            }}

            return ret;
        }}
    }}
", name, dbName, sbAny.ToString()));
            }
            else
            {
                sb.Append(string.Format(@"
    class {0}Service : SAFService
    {{
        public override string DefaultConnectionName
        {{
            get
            {{
                return ""{1}"";
            }}
        }}
        
        public async Task<{0}Response> Any({0}Request request)
        {{
            var ret = new {0}Response();{2}

            return ret;
        }}
    }}
", name, dbName, sbAny.ToString()));
            }            


            return sb.ToString();
        }

        private IEnumerable<ModelGeneratorByUspVM.ResultSpItem> GetSpResults()
        {            
            foreach (var uspItem in this.UspItems)
            {
                foreach (var spResult in uspItem.SpResults)
                {
                    yield return spResult;
                }
            }
        }

        private IEnumerable<string> GetRequestParameters(ObservableCollection<UspItem> observableCollection)
        {
            foreach (var uspItem in this.UspItems)
            {
                foreach (var spResult in uspItem.SpResults)
                {
                    foreach (var p in GetRequestParameters(spResult.SpInfo))
                    {
                        yield return p;
                    }
                }
            }
        }

        private IEnumerable<string> GetRequestParameters(SPInfo spInfo)
        {
            if (spInfo.IsQueryText)
            {
                foreach (var p in spInfo.RequestPropertyList)
                {
                    yield return p;
                }
            }
            else
            {
                var dsParam = DBObject.GetSPParameters(spInfo.DbConnectionString, spInfo.SPName);
                foreach (DataRow row in dsParam.Tables[0].Rows)
                {
                    yield return row[0].ToString().Substring(1);
                }
            }
        }

        private IEnumerable<Model> GetModelList()
        {
            foreach (var uspItem in this.UspItems)
            {
                foreach (var spResult in uspItem.SpResults)
                {
                    if (spResult.SpInfo.IsSaveSP) continue;

                    Model model = new Model();
                    model.ModelName = spResult.ModelName;
                    model.SPName = spResult.SpInfo.SPName;
                    List<string> modelPropertyList = new List<string>();
                    foreach (DataColumn col in spResult.Table.Columns)
                    {
                        modelPropertyList.Add(col.ColumnName);
                    }
                    model.ModelPropertyList = modelPropertyList;
                    yield return model;
                }
            }
        }

        private string GetClassComment(bool isService)
        {
            var _Namespace = isService ? TobeNamespace.Replace("Model", "Server") : TobeNamespace;
            var _ClassName = string.Format("{0}{1}", TobeBaseName, isService ? "Service" : "Request");

            var sb = new StringBuilder();
            sb.Append(string.Format(@"// ================================================
// {0}
// [AS-IS] {1}.{2}()
// -----------------------------------------------
// {3} 작성자({4})
// ================================================", _ClassName,
 _ClassName,
 TobeBaseName,
 DateTime.Today.Date.ToShortDateString(),
 Environment.UserName
 ));
            return sb.ToString();
        }

        #endregion

        #endregion
    }

    public partial class CodeGeneratorByBizVM : ViewModelBase
    {
        public class FacadeItem
        {
            public string Namespace { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return string.Format("{0}.{1}", Namespace, Name);
            }

            internal static IEnumerable<FacadeItem> GetItemFromBizTable(DataTable dt)
            {
                var dTable = dt.Select("LAYER='FACADE'").Distinct("NAMESPACE", "CLASSNM");
                foreach (DataRow dr in dTable.Rows)
                {
                    var item = new FacadeItem()
                    {
                        Namespace = dr.ToStr("NAMESPACE"),
                        Name = dr.ToStr("CLASSNM"),
                    };

                    yield return item;
                }
            }
        }

        public class FacadeMethodItem
        {
            public string Namespace { get; set; }
            public string ClassName { get; set; }
            public string MethodName { get; set; }

            public override string ToString()
            {
                return string.Format("{0}", MethodName);
            }

            internal static IEnumerable<FacadeMethodItem> GetMethodItems(DataTable dt, FacadeItem facadeItem)
            {
                if (facadeItem == null) yield break;

                var dTable = dt.Select(string.Format("LAYER='FACADE' and NAMESPACE='{0}' and CLASSNM='{1}'", facadeItem.Namespace, facadeItem.Name))
                    .Distinct("NAMESPACE", "CLASSNM", "METHODNM");
                foreach (DataRow dr in dTable.Rows)
                {
                    var item = new FacadeMethodItem()
                    {
                        Namespace = dr.ToStr("NAMESPACE"),
                        ClassName = dr.ToStr("CLASSNM"),
                        MethodName = dr.ToStr("METHODNM"),
                    };

                    yield return item;
                }
            }

            public string FullPath 
            {
                get
                {
                    return string.Format("{0}.{1}.{2}()", Namespace, ClassName, MethodName);
                }
            }
        }

        public class BizItem
        {
            public string Namespace { get; set; }
            public string Name { get; set; }

            public string Postfix
            {
                get
                {
                    var postfix = this.Name.EndsWith("nTx") ? "Tx"
                        : this.Name.EndsWith("NTx") ? "NTx"
                        : this.Name.EndsWith("Tx") ? "Tx"
                        : this.Name.EndsWith("nSu") ? "Su"
                        : this.Name.EndsWith("NSu") ? "NSu"
                        : this.Name.EndsWith("Su") ? "Su"
                        : this.Name.EndsWith("ntx", StringComparison.CurrentCultureIgnoreCase) ? "NTx"
                        : this.Name.EndsWith("tx", StringComparison.CurrentCultureIgnoreCase) ? "Tx"
                        : string.Empty;

                    if (IsTx && postfix == "NTx")
                        postfix = "Tx";

                    return postfix;
                }
            }

            internal static IEnumerable<BizItem> GetItemFromBizTable(DataTable dt)
            {                
                var dTable = dt.Select("LAYER='BIZ'").Distinct("NAMESPACE", "CLASSNM");
                foreach (DataRow dr in dTable.Rows)
                {
                    var item = new BizItem()
                    {
                        Namespace = dr.ToStr("NAMESPACE"),
                        Name = dr.ToStr("CLASSNM"),
                    };

                    yield return item;
                }
            }

            public override string ToString()
            {
                return string.Format("{0}.{1}", Namespace, Name);
            }

            public string GetTobeNamespace(bool isServer)
            {   
                var nm = Postfix.Equals("NTx") ? this.Name.Substring(0, this.Name.Length - 3)
                    : Postfix.Equals("Tx") ? this.Name.Substring(0, this.Name.Length - 2)
                    : Postfix.Equals("NSu") ? this.Name.Substring(0, this.Name.Length - 3)
                    : Postfix.Equals("Su") ? this.Name.Substring(0, this.Name.Length - 2)
                    : null;

                if (nm == null) Debugger.Break();
                
                var nsArray = this.Namespace.Split('.');
                var ns = string.Format("His3.{0}.{1}.{2}.{3}.{4}",
                    nsArray[2].ToCamel(),
                    isServer ? "Server" : "Model",
                    nsArray[3].ToCamel().Replace("Zzz", "Etc"),
                    nsArray[4].ToCamel(),
                    nm);

                return ns;
            }


            private bool? _IsTx = null;
            public bool IsTx 
            {
                get
                {
                    if (_IsTx == null)
                    {
                        var path = Core.WorkService.Current.GetFullFilePath(this.Namespace, this.Name);
                        var fullCode = IOHelper.ReadFileToString(path);
                        //var pattern = @"\[SAFAop\(""(\w+)""";
                        
                        //string nm = "NTx";
                        //var match = Regex.Match(fullCode, pattern);
                        //if (match.Success)
                        //{
                        //    nm = match.Groups[1].Value;
                        //}
                        //else
                        //{
                        //    MessageBox.Show("AOP 어트리뷰트를 찾지 못했습니다. 담당자에게 알려주세요");
                        //}

                        //_IsTx = nm.Equals("tx", StringComparison.CurrentCultureIgnoreCase);

                        if (fullCode.IndexOf("BizTxBase") > 0)
                            _IsTx = true;
                        else //if (fullCode.IndexOf("BizNTxBase");
                            _IsTx = false;
                    }

                    return (bool)_IsTx;
                }
            }
        }

        public class MethodItem
        {
            public string Namespace { get; set; }
            public string ClassName { get; set; }
            public string MethodName { get; set; }            

            internal static IEnumerable<MethodItem> GetMethodItems(DataTable dt, BizItem bizItem)
            {
                if (bizItem == null) yield break;

                var dTable = dt.Select(string.Format("LAYER='BIZ' and NAMESPACE='{0}' and CLASSNM='{1}'", bizItem.Namespace, bizItem.Name))
                    .Distinct("NAMESPACE", "CLASSNM", "METHODNM");
                foreach (DataRow dr in dTable.Rows)
                {
                    var item = new MethodItem()
                    {
                        Namespace = dr.ToStr("NAMESPACE"),
                        ClassName = dr.ToStr("CLASSNM"),
                        MethodName = dr.ToStr("METHODNM"),
                    };

                    yield return item;
                }
            }

            public override string ToString()
            {
                return string.Format("{0}", MethodName);
            }

            public string FullPath
            {
                get
                {
                    return string.Format("{0}.{1}.{2}()", Namespace, ClassName, MethodName);
                }
            }
        }


        public class DAItem
        {
            public string Namespace { get; set; }
            public string ClassName { get; set; }
            public string MethodName { get; set; }
            public string UspName { get; set; }
            public string FullName
            {
                get
                {
                    return string.Format("{0}.{1}.{2}()", Namespace, ClassName, MethodName);
                }
            }

            internal static IEnumerable<DAItem> GetDAItems(DataTable dt, MethodItem methodItem)
            {
                if (methodItem == null) yield break;

                var uspNames = new List<string>();

                var bizRows = dt.Select(string.Format("NAMESPACE='{0}' and CLASSNM='{1}' and METHODNM='{2}'"
                    , methodItem.Namespace, methodItem.ClassName, methodItem.MethodName));

                var daRows = bizRows.Where(p => p.ToStr("CALLOBJNS").StartsWith("HIS.DA"));
                foreach (DataRow daRow in daRows)
                {
                    var rows = dt.Select(string.Format("NAMESPACE='{0}' and CLASSNM='{1}' and METHODNM='{2}'"
                        , daRow.ToStr("CALLOBJNS"), daRow.ToStr("CALLOBJNM"), daRow.ToStr("CALLFUNCNM")));
                    
                    foreach (DataRow row in rows)
                    {
                        if (uspNames.Contains(row.ToStr("CALLOBJNM"))) continue;
                        uspNames.Add(row.ToStr("CALLOBJNM"));

                        var item = new DAItem()
                        {
                            Namespace = row.ToStr("NAMESPACE"),
                            ClassName = row.ToStr("CLASSNM"),
                            MethodName = row.ToStr("METHODNM"),
                            UspName = row.ToStr("CALLOBJNM")
                        };

                        yield return item;
                    }
                }
            }

            public override string ToString()
            {
                return string.Format("{0}", MethodName);
            }
        }

        public class UspItem : PropertyNotifier, IConditionalTemplateItem
        {
            #region Interfaces...

            public string MatchingKey { get; set; }

            #endregion

            #region Properteis

            public string Name { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public IEnumerable<ModelGeneratorByUspVM.ResultSpItem> SpResults { get; set; }

            #endregion                        
        }
    }
}
