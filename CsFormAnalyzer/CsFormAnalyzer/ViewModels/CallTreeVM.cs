using CsFormAnalyzer.Core;
using CsFormAnalyzer.Utils;
using CsFormAnalyzer.Mvvm;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Data;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using SC.WPF.Tools.CodeHelper;
using System.Data.SqlClient;
using CsFormAnalyzer.Views;
using System.Diagnostics;
using CsFormAnalyzer.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace CsFormAnalyzer.ViewModels
{
    public partial class CallTreeVM : ViewModelBase
    {
        #region Fields

        private DataTable _CallTreeDTOriginal;

        #endregion

        #region FacadeGenVM

        private FacadeGenVM _FacadeGenVM;
        ///
        public FacadeGenVM FacadeGenViewModel
        {
            get
            {
                if (_FacadeGenVM == null)
                    _FacadeGenVM = new FacadeGenVM();
                return _FacadeGenVM;
            }
            set
            {
                OnPropertyChanged("FacadeGenViewModel");
            }
        }

        #endregion

        #region TargetFile
        private string _TargetFile;
        public string TargetFile
        {
            get { return _TargetFile; }
            set
            {
                _TargetFile = value;
                OnPropertyChanged();

                var fileName = System.IO.Path.GetFileName(value);
                if (string.IsNullOrEmpty(fileName) != true)
                    fileName = fileName.LeftBySearch(".");

                ViewModelLocator.Current.CodeGenViewModelVM.ViewModelInfo.Name = fileName;
            }
        }

        #endregion

        #region Namespace
        public string Namespace { get { return _Namespace; } set { _Namespace = value; OnPropertyChanged(); } }
        private string _Namespace;
        #endregion

        #region ClassName
        public string ClassName { get { return _ClassName; } set { _ClassName = value; OnPropertyChanged(); } }
        private string _ClassName;
        #endregion

        #region MethodName
        public string MethodName { get { return _MethodName; } set { _MethodName = value; OnPropertyChanged(); } }
        private string _MethodName;
        #endregion

        #region RunCommand
        public ICommand RunCommand { get; private set; }
        #endregion

        #region SearchCommand
        ICommand _searchCommand;
        public ICommand SearchCommand
        {
            get
            {
                if (_searchCommand == null)
                    _searchCommand = CreateCommand(ExecSearchCommand);
                return _searchCommand;
            }
        }

        public void ExecSearchCommand(object param)
        {
            if (string.IsNullOrEmpty(ClassName))
            {
                MessageBox.Show("클래스 이름은 필수 입니다.");
                return;
            }

            SqlParameter[] salParams = new SqlParameter[3];
            salParams[0] = new SqlParameter("@ns", Namespace + "");
            salParams[1] = new SqlParameter("@classNM", ClassName);
            salParams[2] = new SqlParameter("@methodNM", MethodName + "");
            DataSet ds = DataAccess.GetDataSet2(AppManager.DataConnectionString, "dbo.[USP_SelectClassInfo]", salParams, CommandType.StoredProcedure);
            if (ds.Tables.Count > 0)
                this.CallTreeDT = ds.Tables[0];
            else
                this.CallTreeDT = null;
        }

        #endregion

        #region OpenModelGeneratorCommand
        ICommand _OpenModelGeneratorCommand;
        public ICommand OpenModelGeneratorCommand
        {
            get
            {
                if (_OpenModelGeneratorCommand == null)
                    _OpenModelGeneratorCommand = CreateCommand(ExecOpenModelGeneratorCommand);
                return _OpenModelGeneratorCommand;
            }
        }

        public void ExecOpenModelGeneratorCommand(object param)
        {
            if (param.Equals("Design"))
            {
                var vm = ViewModelLocator.Current.GetInstance<ModelGeneratorVM>(true);
                AppManager.Current.ShowView(typeof(ModelGeneratorView), vm);
            }
            else if (param.Equals("Code"))
            {
                var filePath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "CreatedModel1.exe");
                if (System.IO.File.Exists(filePath))
                {
                    var p = new Process();
                    p.StartInfo = new ProcessStartInfo(filePath);
                    p.Start();
                }
            }
            else if (param.Equals("MultiUsp"))
            {
                var vm = ViewModelLocator.Current.GetInstance<CodeGeneratorByMultiUspVM>(true);
                AppManager.Current.ShowView(typeof(CodeGeneratorByMultiUspView), vm);                
            }
            else if (param.Equals("Biz"))
            {
                var vm = ViewModelLocator.Current.GetInstance<CodeGeneratorByBizVM>(true);
                AppManager.Current.ShowView(typeof(CodeGeneratorByBizView), vm);

                vm.InitComplated += delegate
                {
                    if (this.SelectedCallTreeItem != null
                        && this.SelectedCallTreeItem.BizNames != null)
                    {
                        lock (new object())
                        {
                            var facade = this.SelectedCallTreeItem.Facade;
                            var facadeName = facade.LastLeftBySearch(".");
                            var facadeMethodName = facade.RightBySearch(".");

                            vm.SelectedFacadeItem = vm.FacadeItems.Where(p => p.ToString().Equals(facadeName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                            vm.SelectedFacadeMethodItem = vm.FacadeMethodItems.Where(p => p.MethodName.Equals(facadeMethodName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

                            var firstBiz = this.SelectedCallTreeItem.BizNames.FirstOrDefault();
                            var bizName = firstBiz.LastLeftBySearch(".");
                            var methodName = firstBiz.RightBySearch(".");

                            vm.SelectedBizItem = vm.BizItems.Where(p => p.ToString().Equals(bizName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                            vm.SelectedMethodItem = vm.MethodItems.Where(p => p.MethodName.Equals(methodName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                        }
                    }
                };
            }
        }
        #endregion

        #region OpenCodeConverterCommand
        ICommand _OpenCodeConverterCommand;
        public ICommand OpenCodeConverterCommand
        {
            get
            {
                if (_OpenCodeConverterCommand == null)
                    _OpenCodeConverterCommand = CreateCommand(ExecOpenCodeConverterCommand);
                return _OpenCodeConverterCommand;
            }
        }

        public void ExecOpenCodeConverterCommand(object param)
        {
            var vm = ViewModelLocator.Current.GetInstance<CodeConverterVM>(true);
            AppManager.Current.ShowView(typeof(CodeConverterView), vm);
        }
        #endregion

        #region OpenModelGeneratorByUspCommand
        ICommand _OpenModelGeneratorByUspCommand;
        public ICommand OpenModelGeneratorByUspCommand
        {
            get
            {
                if (_OpenModelGeneratorByUspCommand == null)
                    _OpenModelGeneratorByUspCommand = CreateCommand(ExecOpenModelGeneratorByUspCommand);
                return _OpenModelGeneratorByUspCommand;
            }
        }

        public void ExecOpenModelGeneratorByUspCommand(object param)
        {
            var vm = ViewModelLocator.Current.GetInstance<ModelGeneratorByUspVM>(true);
            AppManager.Current.ShowView(typeof(ModelGeneratorByUspView), vm);
        }
        #endregion
        
        #region ScmdOpenView
        ICommand _ScmdOpenview;
        public ICommand ScmdOpenView
        {
            get
            {
                if (_ScmdOpenview == null)
                    _ScmdOpenview = CreateCommand(ExecScmdOpenView);
                return _ScmdOpenview;
            }
        }

        internal void ExecScmdOpenView(object param)
        {
            //var owner = param as FrameworkElement;
            //if(SelectedCallTreeItem.Row.ItemArray[5] == null)
            //{
            //    MessageBox.Show("SP가 없음");
            //    return;
            //}
            //var vm = new CodeGen_Model_ServiceVM();
            //int spNameRowIndex = CallTreeDT.Columns.Count -2;
            //foreach(DataRow dr in CallTreeDT.Rows)
            //{ 
            //    string spname = dr[spNameRowIndex] +"";
            //    if (spname.Length > 0)
            //    {
            //        vm.SPNameList.Add(spname);
            //        vm.ModelNameList.Add(spname, dr[spNameRowIndex + 1] + "");
            //    }
            //}
            //vm.SPName = SelectedCallTreeItem.Row.ItemArray[5] + ""; 
            //vm.ModelName = vm.ModelNameList[vm.SPName];
            //OpenDialog(typeof(CodeGen_Model_ServiceView), vm, owner);

            //int spNameRowIndex = CallTreeDT.Columns.Count - 2;
            //foreach (DataRow dr in CallTreeDT.Rows)
            //{
            //    string spname = dr[spNameRowIndex] + "";
            //    if (spname.Length > 0)
            //    {
            //        CodeGenModelVM.SPNameList.Add(spname);
            //        CodeGenModelVM.ModelNameList.Add(spname, dr[spNameRowIndex + 1] + "");
            //    }
            //}
            if (SelectedCallTreeItem == null) return;

            if (SelectedCallTreeItem.Row["SP"] == null)
            {
                MessageBox.Show("SP가 없음");
                return;
            }

            if (string.IsNullOrEmpty(SelectedCallTreeItem.Facade))
                return;

            CodeGenModelVM.SelectedCallTreeItem = SelectedCallTreeItem;
            CodeGenModelVM.SelectedFacadeFunc = SelectedCallTreeItem.Facade;
            //CodeGenModelVM.SPName = SelectedCallTreeItem.Row.ToStr("SP");
            //if (string.IsNullOrEmpty(CodeGenModelVM.SPName) != true 
            //    && CodeGenModelVM.ModelNameList.ContainsKey(CodeGenModelVM.SPName))
            //    CodeGenModelVM.ModelName = CodeGenModelVM.ModelNameList[CodeGenModelVM.SPName];


        }
        #endregion

        #region SelectedCallTreeItem

        CallTreeItem _selectedCallTreeItem;
        public CallTreeItem SelectedCallTreeItem
        {
            get { return _selectedCallTreeItem; }
            set
            {
                _selectedCallTreeItem = value;
            }
        }

        #endregion

        #region CallTreeList
        public IList<CallTreeVM.CallTreeItem> CallTreeList { get { return _CallTreeList; } set { _CallTreeList = value; OnPropertyChanged(); } }
        private IList<CallTreeVM.CallTreeItem> _CallTreeList;
        #endregion

        #region CallTreeDT
        private DataTable _callTreeDT;
        public DataTable CallTreeDT
        {
            get
            {
                return _callTreeDT;
            }
            set
            {
                _callTreeDT = value;
                OnPropertyChanged("CallTreeDT");
            }
        }
        #endregion

        #region CodeGenModelVM
        CodeGen_Model_ServiceVM mCodeGenModelVM;
        public CodeGen_Model_ServiceVM CodeGenModelVM
        {
            get
            {
                if (mCodeGenModelVM == null)
                {
                    mCodeGenModelVM = new CodeGen_Model_ServiceVM();
                }
                return mCodeGenModelVM;
            }
            set
            {
                mCodeGenModelVM = value;
                OnPropertyChanged("CodeGenModelVM");
            }
        }
        #endregion

        #region ScmdSourceUrl
        ICommand _scmdSourceUrl;
        public ICommand ScmdSourceUrl
        {
            get
            {
                if (_scmdSourceUrl == null)
                    _scmdSourceUrl = CreateCommand(ExecScmdSourceUrl);
                return _scmdSourceUrl;
            }
        }

        internal void ExecScmdSourceUrl(object param)
        {

            MessageBox.Show("");
        }
        #endregion

        #region ClientProject
        string mClientProject;
        public string ClientProject
        {
            get
            {
                return mClientProject;
            }
            set
            {
                mClientProject = value;
                OnPropertyChanged("ClientProject");
            }
        }
        #endregion

        #region ModelProject
        string mModelProject;
        public string ModelProject
        {
            get
            {
                return mModelProject;
            }
            set
            {
                mModelProject = value;
                OnPropertyChanged("ModelProject");
            }
        }
        #endregion

        #region ServerProject
        string mServerProject;
        public string ServerProject
        {
            get
            {
                return mServerProject;
            }
            set
            {                
                mServerProject = value;
                OnPropertyChanged("ServerProject");
            }
        }
        #endregion

        #region constructor

        public CallTreeVM()
        {
            this.RunCommand = base.CreateCommand(delegate
            {
                base.InvokeAsyncAction(Run);
            });
        }

        #endregion
        
        #region ScmdViewCode
        ICommand _ScmdViewCode;        
        public ICommand ScmdViewCode
        {
            get
            {
                if (_ScmdViewCode == null)
                    _ScmdViewCode = CreateCommand(ExecScmdViewCode);
                return _ScmdViewCode;
            }
        }

        internal void ExecScmdViewCode(object param)
        {
            DataRow dr = param as DataRow;

            if (SelectedCallTreeItem == null) return;

            if (SelectedCallTreeItem.Row["SP"] == null)
            {
                MessageBox.Show("SP가 없음");
                return;
            }

            if (string.IsNullOrEmpty(SelectedCallTreeItem.Facade))
                return;

            CodeGenModelVM.SelectedCallTreeItem = SelectedCallTreeItem;
            CodeGenModelVM.SelectedFacadeFunc = SelectedCallTreeItem.Facade;
            SPInfo spinfo = CodeGenModelVM.FacadeSPList.FirstOrDefault(x => x.SPName == dr["SP"] + "");
            //CodeGenModelVM.ScmdRunSPCode.Execute(null);
            
            //CodeGenModelVM.SetSPInfo(spinfo);
            
            CodeGenModelVM.SelectedSPInfo = spinfo;
            if (!CodeGenModelVM.CheckHasRequestcode())
            {
                CodeGenModelVM.RunSP(string.Empty);
                CodeGenModelVM.SPExcuteVisible = Visibility.Visible;
            }

            //var vm = new HasRequestInfoVM() { SpInfomation = SelectedSPInfo, RequestInfoDataSet = ds2 };
            //if (!OpenDialog(typeof(HasRequstView), vm, 500, 500, "Reqeust정보보기", null, true))
            //{
            //    GenCode gencode = new GenCode();
            //    gencode.ModelCode = vm.RequestCode;
            //    gencode.RequestName = SelectedSPInfo.FacadePath;
            //    this.GenCodeList.Add(gencode);
            //    PathVisibility = Visibility.Collapsed;
            //    return;
            //}
        } 
        #endregion


        public string FindCallTree { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        [InstanceNew]
        public CallObjectListVM CallObjectListVM { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public List<CallTreeObjectItem> CallTreeListForTreeView { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public void Run()
        {
            var path = this.TargetFile;
            if (File.Exists(path) != true) return;
            CodeGenModelVM = new CodeGen_Model_ServiceVM();
            
            //C:\00._Sevrance\His2\HIS2.0\Source\WinUI\SP\BLO\HIS.WinUI.SP.BLO.CM.Com\BLOBcdMF.cs

            //string ns = path.Remove(path.LastIndexOf(@"\")).RightBySearch(@"\");            
            string ns = WorkService.Current.GetNamespaceByFilePath(path);
            string classNM = Path.GetFileNameWithoutExtension(path);
            string layer;
            if (classNM.IndexOf("Controller") > 0)
                layer = "Controller";
            else
                layer = "WinUI";

            SearchData(ns, classNM, "", layer);

            int spNameRowIndex = CallTreeDT.Columns.Count - 2;
            var spNameList = new Dictionary<string, string>();
            var modelNameList = new Dictionary<string, string>();
            var facadeList = new Dictionary<string, string>();
            foreach (DataRow dr in CallTreeDT.Rows)
            {
               
                string facade = dr["Facade"] +"";
                if(!string.IsNullOrEmpty(facade) && !facadeList.ContainsKey(facade) )
                {
                    facadeList.Add(facade, dr["FacadeMethod"] + "");
                }

                string spname = dr["SP"] + "";
                string bizName = dr["Biz"] +"";
                if (spname.Length > 0)
                {
                    if (!spNameList.ContainsKey(facade + "^" + spname + "^" + bizName))
                        spNameList.Add(facade + "^" + spname+"^"+ bizName, facade);
                }
            }
            CodeGenModelVM.CallTreeDT = CallTreeDT;
            CodeGenModelVM.SPNameList = spNameList;
            CodeGenModelVM.FacadeFuncList = facadeList;
            CodeGenModelVM.ModelNameList = modelNameList;
            CodeGenModelVM.UINameSpace = ns + "." + classNM;

            CallObjectListVM.Source = GetCallTreeObjectTable(ns, classNM);
            this.CallTreeList = this.CallTreeDT.AsEnumerable()
                .Select(row => new CallTreeVM.CallTreeItem(row)).ToList<CallTreeVM.CallTreeItem>()
                .OrderBy(p => p.WinUI).ToList();

            //this.CallTreeListForTreeView = GetCallTreeListForTreeView(ns, classNM);
            this.CallTreeListForTreeView = GetCallTreeListForTreeView(this._CallTreeDTOriginal);

            string[] tmp = ns.Split('.');
            string postName = string.Empty;
            for (int i = 3; i < tmp.Length; i++)
            {
                postName += "." + tmp[i];
            }

            this.ClientProject = "His3." + tmp[2] + ".Client" + postName;
            this.ModelProject = "His3." + tmp[2] + ".Model" + postName;
            this.ServerProject = "His3." + tmp[2] + ".Server" + postName;

            // 이미 생성된 아이들 체크
            CodeGenModelVM.GenCodeList.CollectionChanged += (s, e) =>
                {
                    // 전체 원복
                    CallTreeListForTreeView.ForEach(x => x.RowColor = Colors.Black);
                    // 다음 있는 것만 체크
                    foreach(var gencode in CodeGenModelVM.GenCodeList)
                    {
                        foreach(var node in CallTreeListForTreeView )
                        {
                            if (node.HasFacadeNameNode(gencode.FacadeNM))
                                node.RowColor = Colors.Red;
                        }
                    }
                };
        }

        private DataTable GetCallTreeObjectTable(string ns, string classNM)
        {
            SqlParameter[] salParams = new SqlParameter[2];
            salParams[0] = new SqlParameter("@ns", ns);
            salParams[1] = new SqlParameter("@classNM", classNM);
            DataSet ds = DataAccess.GetDataSet2(AppManager.DataConnectionString, "USP_WINUI_CALLTREE", salParams, CommandType.StoredProcedure);
            if (ds.Tables.Count > 0)
                return ds.Tables[0];
            else
                return null;
        }
        
        void SearchData(string ns, string classNM, string methodNM, string layer)
        {
            //string path = AppDomain.CurrentDomain.BaseDirectory + @"Data\HISBiz.mdf";
            //path = @"C:\00._Sevrance\00.Socen\CsFormAnalyzer\CsFormAnalyzer\Data\HISBiz.mdf";
            ////var path = @"\\10.28.16.54\Share\WorkSource\CSFormAnalyzer\HISBiz.mdf";
            //string conStr =  @"Data Source=(LocalDB)\v11.0;" +  @"AttachDbFilename= '" + path + "';" + @"Integrated Security=True;";

            SqlParameter[] salParams = new SqlParameter[4];

            salParams[0] = new SqlParameter("@ns", ns);
            salParams[1] = new SqlParameter("@classNM", classNM);
            salParams[2] = new SqlParameter("@methodNM", methodNM);
            salParams[3] = new SqlParameter("@layer", layer);
            DataSet ds = DataAccess.GetDataSet2(AppManager.DataConnectionString, "USP_SEARCH_CALLTREE_NEW", salParams, CommandType.StoredProcedure);
            //salParams[0] = new SqlParameter("@classNM", classNM);
            //DataSet ds = DataAccess.GetDataSet2(AppManager.DataConnectionString, "USP_WINUI_CALLTREE", salParams, CommandType.StoredProcedure);
            
            if (ds.Tables.Count > 0)
            {
                
                ds.Tables[0].Columns.Add("Facade");
                ds.Tables[0].Columns.Add("FacadeMethod");
                ds.Tables[0].Columns.Add("DA");
                ds.Tables[0].Columns.Add("DAMethod");
                ds.Tables[0].Columns.Add("Biz");
                DataTable callTreeDT = ds.Tables[0].Clone();
                int columnIdx = ds.Tables[0].Columns.Count - 2;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string spName = dr[columnIdx] + "";
                    string modelName = string.Empty;
                    //USP_HP_ZZZ_SOC_SocWrkDA_SelectSupFundDtl

                    for (int i = 0; i < dr.ItemArray.Count(); i++)
                    {
                        if (dr[i] + "" == "Facade")
                        {
                            dr["Facade"] = dr[i+1]+"."+ dr[i + 2];
                            dr["FacadeMethod"] = (dr[i + 2] + "").Split('.')[1].Replace("NTx", "").Replace("Tx", "");
                        }
                        else if (dr[i] + "" == "DA")
                        {
                            dr["DA"] = dr[i + 2];
                            dr["DAMethod"] = (dr[i + 2] + "");
                            break;
                        }
                        else if (dr[i] + "" == "BIZ")
                        {
                            if ((dr["Biz"] + "").Length > 0)
                                continue;
                            dr["Biz"] = dr[i + 1] + "\\" + (dr[i + 2] + "").Split('.')[0] + ".cs";
                        }
                       
                    }

                    //// MARK : USP 에서 제거 하였음
                    //// facade 이름이 같고, sp 이름이 같으면 제거 하자.
                    //if (callTreeDT.Select("DA = '" + dr["DA"] + "' and DAMethod ='" + dr["DAMethod"] + "' and SP = '" + dr.ToStr("SP").Escape() + "'").Length > 0)
                    //    continue;
                    //else
                    //    callTreeDT.Rows.Add(dr.ItemArray);
                    callTreeDT.Rows.Add(dr.ItemArray);

                    //if (spName.Trim().Length > 0)
                    //{
                    //    string[] spNameSplited = spName.Split('_');
                    //    modelName = spNameSplited[spNameSplited.Length - 1].Replace("Select", "")
                    //                       .Replace("Update", "").Replace("Insert", "")
                    //                       .Replace("Get", "").Replace("Del", "").Replace("Save", "").Replace("Check", "").Replace("Cancel", "");

                    //    modelName += "Model";
                    //}
                    //dr["ModelName"] = modelName;
                }

                this._CallTreeDTOriginal = ds.Tables[0];
                this.CallTreeDT = callTreeDT;
            }
            else
            {
                this.CallTreeDT = null;
            }
        }

        private List<CallTreeObjectItem> GetCallTreeListForTreeView(DataTable dataTable)
        {
            var list = new List<CallTreeObjectItem>();
            var rows = dataTable.Select("", "Depth0,Depth1,Depth2,Depth3,Depth4,Depth5,Depth6,Depth7,Depth8,Depth9");

            var dic = new Dictionary<string, CallTreeObjectItem>();
            foreach (var row in rows)
            {
                CallTreeObjectItem item = null;
                string dicKey = string.Empty;
                for (int depth = 0; depth < 10; depth++)
                {
                    var layerField = string.Format("Layer{0}", depth);
                    var nsField = string.Format("Namespace{0}", depth);
                    var keyField = string.Format("Depth{0}", depth);
                    var idField = string.Format("ID{0}", depth);
                    
                    var layer = row.ToStr(layerField);
                    var ns = row.ToStr(nsField);
                    var key = row.ToStr(keyField);
                    var id = row.ToStr(idField);
                    dicKey = string.Format("{0}@{1}", dicKey, key);

                    if (string.IsNullOrEmpty(key))
                    {
                        var parent = item;
                        layer = "SP";
                        key = row.ToStr("SP");
                        if (string.IsNullOrEmpty(key)) break;
                        item = new CallTreeObjectItem(layer, ns, key);
                        item.Parent = parent;
                        parent.Children.Add(item);
                        break;
                    }

                    if (item == null)
                    {                        
                        if (dic.ContainsKey(dicKey))
                        {
                            item = dic[dicKey];
                            var parent = item.Parent;
                            if (parent != null)
                            {
                                item = new CallTreeObjectItem(layer, ns, key);
                                item.ID = id;
                                parent.Children.Add(item);                            
                            }
                        }
                        else
                        {
                            item = new CallTreeObjectItem(layer, ns, key);
                            item.ID = id;
                            dic.Add(dicKey, item);
                            list.Add(item);
                        }
                        continue;
                    }

                    if (dic.ContainsKey(dicKey))
                    {
                        item = dic[dicKey];
                    }
                    else
                    {
                        var parent = item;
                        item = new CallTreeObjectItem(layer, ns, key);
                        item.ID = id;
                        item.Parent = parent;
                        dic.Add(dicKey, item);
                        parent.Children.Add(item);
                    }

                    Console.Write(1);
                }
            }

            return list;
        }
    }

    public partial class CallTreeVM : ViewModelBase
    {
        public class CallTreeObjectItem : ViewModelBase
        {
            public List<CallTreeObjectItem> Children { get; set; }
            public CallTreeObjectItem Parent { get; set; }

            public string Layer { get; set; }
            public string Ns { get; set; }            
            public string Key { get; set; }
            public string ID { get; set; }
            public string ClassName
            {
                get
                {
                    if (string.IsNullOrEmpty(Key) || Key.IndexOf('.') < 0) return null;
                    return Key.Split('.').ElementAt(0);
                }
            }

            public string MethodName
            {
                get
                {
                    if (string.IsNullOrEmpty(Key) || Key.IndexOf('.') < 0) return null;
                    return Key.Split('.').ElementAt(1);
                }
            }

            public CallTreeObjectItem(string layer, string ns, string key)
            {
                this.Layer = layer;
                this.Ns = ns;
                this.Key = key;

                this.Children = new List<CallTreeObjectItem>();
            }

            public override string ToString()
            {
                return string.Format("[{0}] {1}", this.Layer, this.Key);
            }

            #region SelectCallTreeCommand

            private ICommand _SelectCallTreeCommand;
            public ICommand SelectCallTreeCommand
            {
                get
                {
                    if (_SelectCallTreeCommand == null)
                    {
                        _SelectCallTreeCommand = new Mvvm.SimpleCommand(OnExecuteSelectCallTreeCommand);
                    }
                    return _SelectCallTreeCommand;
                }
            }

            private void OnExecuteSelectCallTreeCommand(object obj)
            {
                SqlParameter[] salParams = new SqlParameter[4];

                salParams[0] = new SqlParameter("@ns", "");
                salParams[1] = new SqlParameter("@classNM", ClassName);
                salParams[2] = new SqlParameter("@methodNM", MethodName);
                salParams[3] = new SqlParameter("@layer", Layer);
                DataSet ds = DataAccess.GetDataSet2(AppManager.DataConnectionString, "USP_SEARCH_CALLTREE_NEW", salParams, CommandType.StoredProcedure);
                if (ds.Tables.Count < 1) return;

                var dt = ds.Tables[0];

                var element = new Border();
                element.Width = 800;
                element.Height = 400;
                element.Padding = new Thickness(4);
                element.Background = new SolidColorBrush(Colors.White);
                element.BorderBrush = new SolidColorBrush(Colors.Blue);
                element.BorderThickness = new Thickness(1);

                var dataGrid = new DataGrid();
                dataGrid.ItemsSource = dt.AsDataView();
                element.Child = dataGrid;

                var popup = new Popup();
                popup.Child = element;
                popup.PlacementTarget = obj as UIElement;
                //popup.Placement = PlacementMode.MousePoint;
                popup.StaysOpen = false;
                popup.IsOpen = true;
            }

            #endregion

            #region ShowFilePathCommand

            private ICommand _ShowFilePathCommand;
            public ICommand ShowFilePathCommand
            {
                get
                {
                    if (_ShowFilePathCommand == null)
                    {
                        _ShowFilePathCommand = new Mvvm.SimpleCommand(OnExecuteShowFilePathCommand);
                    }
                    return _ShowFilePathCommand;
                }
            }

            private void OnExecuteShowFilePathCommand(object obj)
            {
                var path = WorkService.Current.GetFullFilePath(Ns, ClassName);

                var txtBox = new TextBox();
                txtBox.Text = path;

                AppManager.Current.ShowPopup(txtBox);
            }

            #endregion

            #region ShowMethodCommand

            private ICommand _ShowMethodCommand;
            public ICommand ShowMethodCommand
            {
                get
                {
                    if (_ShowMethodCommand == null)
                    {
                        _ShowMethodCommand = new Mvvm.SimpleCommand(OnExecuteShowMethodCommand);
                    }
                    return _ShowMethodCommand;
                }
            }

            private void OnExecuteShowMethodCommand(object obj)
            {
                var path = WorkService.Current.GetFullFilePath(Ns, ClassName);
                if (string.IsNullOrEmpty(path)) return;

                var fullCode = IOHelper.ReadFileToString(path);                

                var vm = ViewModelLocator.Current.GetInstance<CallTreeCodeVM>(true);
                var item = new CallTreeCodeVM.ItemContext() { Item = this };
                vm.ItemsContext.Add(item);
                vm.SelectedItem = item;
                AppManager.Current.ShowView(typeof(CallTreeCodeView), vm);
            }

            #endregion

            #region ShowMethodAllCommand

            private ICommand _ShowMethodAllCommand;
            public ICommand ShowMethodAllCommand
            {
                get
                {
                    if (_ShowMethodAllCommand == null)
                    {
                        _ShowMethodAllCommand = new Mvvm.SimpleCommand(OnExecuteShowMethodAllCommand);
                    }
                    return _ShowMethodAllCommand;
                }
            }

            private void OnExecuteShowMethodAllCommand(object obj)
            {
                var vm = ViewModelLocator.Current.GetInstance<CallTreeCodeVM>(true);

                var rootItem = this.Parent ?? this;
                while (rootItem.Parent != null)
                {
                    rootItem = rootItem.Parent;
                }

                var list = new List<CallTreeCodeVM.ItemContext>();
                AppendtemsContext(list, rootItem);

                vm.ItemsContext = list;
                vm.SelectedItem = list.Where(p => p.Item == this).FirstOrDefault();
                AppManager.Current.ShowView(typeof(CallTreeCodeView), vm);
            }
            
            private void AppendtemsContext(List<CallTreeCodeVM.ItemContext> list, CallTreeObjectItem item)
            {   
                list.Add(new CallTreeCodeVM.ItemContext() { Item = item});
                foreach (var child in item.Children)
                {
                    if (string.IsNullOrEmpty(child.ClassName)) continue;
                    AppendtemsContext(list, child);
                }
            }
            
            #endregion

            #region OpenFileCommand

            private ICommand _OpenFileCommand;
            public ICommand OpenFileCommand
            {
                get
                {
                    if (_OpenFileCommand == null)
                    {
                        _OpenFileCommand = new Mvvm.SimpleCommand(OnExecuteOpenFileCommand);
                    }
                    return _OpenFileCommand;
                }
            }

            private void OnExecuteOpenFileCommand(object obj)
            {
                var path = WorkService.Current.GetFullFilePath(Ns, ClassName);
                Process.Start(path);
            }

            #endregion

            #region ScmdAddCallTreeInfo - Call Tree 정보를 추가 합니다.
            ICommand _ScmdAddCallTreeInfo;
            public ICommand ScmdAddCallTreeInfo
            {
                get
                {
                    if (_ScmdAddCallTreeInfo == null)
                        _ScmdAddCallTreeInfo = CreateCommand(ExecScmdAddCallTreeInfo);
                    return _ScmdAddCallTreeInfo;
                }
            }

            internal void ExecScmdAddCallTreeInfo(object param)
            {
                var vm = new AddCallTreeVM();
                vm.CallTreeItem = this;
                this.OpenDialog(typeof(AddCallTreeView), vm, 400, 330, "콜트리 입력");
            }
            #endregion


            Color _rowColor = Colors.Black;
            /// <summary>
            /// 로우컬러를 조정
            /// </summary>
            public Color RowColor
            {
                get
                {
                    return _rowColor;
                }
                set
                {
                    _rowColor = value;
                    Children.ForEach(x => x.RowColor = _rowColor);
                    OnPropertyChanged("RowColor");
                }
            }


            public bool HasFacadeNameNode(string name)
            {
                bool has = false;
                if (Ns+"."+ClassName+"."+MethodName == name)
                    return true;
                foreach(var n in Children)
                {
                    has = (n.HasFacadeNameNode(name));
                    if (has)
                        break;
                }
                return has;
            }
            
            public string GetFullCode()
            {
                var path = WorkService.Current.GetFullFilePath(Ns, ClassName);
                if (string.IsNullOrEmpty(path)) return string.Empty;

                var fullCode = IOHelper.ReadFileToString(path);
                return fullCode;
            }
        }

        public class CallTreeItem
        {
            public CallTreeItem(DataRow row)
            {
                this.Row = row;

                var list = new List<CallTreeItemData>();

                foreach (DataColumn dataColumn in row.Table.Columns)
                {
                    if (dataColumn.ColumnName.Equals("SP"))
                    {
                        this.SP = Convert.ToString(row[dataColumn]);
                        continue;
                    }
                    else if (dataColumn.ColumnName.Equals("ModelName"))
                    {
                        this.ModelName = Convert.ToString(row[dataColumn]);
                        continue;
                    }
                    else if(dataColumn.ColumnName.Equals("Facade"))
                    {
                        this.Facade = row[dataColumn]+"";
                        continue;
                    }
                    else if(dataColumn.ColumnName.Equals("HasCode"))
                    {
                        this.HasCode = row[dataColumn] + "";
                       continue;
                    }

                    var level = dataColumn.ColumnName.GetNumeric();
                    var item = list.Where(p => p.Level.Equals(level)).FirstOrDefault();
                    if (item == null)
                    {
                        item = new CallTreeItemData();
                        list.Add(item);
                    }
                    item.Level = level;

                    if (dataColumn.ColumnName.StartsWith("Layer"))
                        item.Layer = Convert.ToString(row[dataColumn]);

                    else if (dataColumn.ColumnName.StartsWith("Namespace"))
                        item.Namespace = Convert.ToString(row[dataColumn]);

                    else if (dataColumn.ColumnName.StartsWith("Depth"))
                        item.Depth = Convert.ToString(row[dataColumn]);

                    //else
                    //    Debugger.Break();
                }

                foreach (var item in list.ToArray())
                {
                    if (string.IsNullOrEmpty(item.Depth))
                        list.Remove(item);
                }

                this.Items = list;

                var items = row.ItemArray.ToList();
                int ndx = 0;
                foreach (var item in items.Where(p => "BIZ".Equals(p)))
                {
                    ndx = items.IndexOf(item, ndx + 1);
                    if (this.BizNames == null) this.BizNames = new List<string>();

                    this.BizNames.Add(string.Format("{0}.{1}", items[ndx + 1], items[ndx + 2]));
                }
            }
            
            public IList<CallTreeItemData> Items { get; set; }

            public string WinUI
            {
                get
                {
                    var item = Items.Where(p => p.Layer.Equals("WinUI")).FirstOrDefault();
                    if (item == null) return null;

                    return item.Depth;
                }
            }

            public string Facade { get; set; }
            public IList<string> BizNames { get; set; }
            public string SP { get; set; }
            public string ModelName { get; set; }
            public DataRow Row { get; set; }

            public string HasCode { get; set; }
        }
        
        public class CallTreeItemData
        {
            public int Row { get; set; }
            public int Level { get; set; }
            public string Layer { get; set; }
            public string Namespace { get; set; }
            public string Depth { get; set; }

            public override string ToString()
            {
                return string.Format("[{0}]{1}", Layer, this.Depth);
            }
        }
    }
}
