using CsFormAnalyzer.Core;
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
using System.Text;
using System.Data.SqlClient;
using SC.WPF.Tools.CodeHelper;

namespace CsFormAnalyzer.ViewModels
{
	class BizCallTreeAnalysisVM : ViewModelBase
	{

        string sourceDirectroy;
        #region CallTreeAnalysisDirectory
        public string CallTreeAnalysisDirectory { get; set; } 
        #endregion

        #region CallTreeDataList
        ObservableCollection<CallTreeData> _CallTreeDataListT = new ObservableCollection<CallTreeData>();
        public ObservableCollection<CallTreeData> CallTreeDataList
        {
            get
            {
                return _CallTreeDataListT;
            }
            set
            {
                _CallTreeDataListT = value;
                OnPropertyChanged("CallTreeDataList");
            }
        }
        #endregion

        #region CallTreeItemList
        private ObservableCollection<CallTreeAnalysis.CallTreeItem> _CallTreeList = new ObservableCollection<CallTreeAnalysis.CallTreeItem>();
        public ObservableCollection<CallTreeAnalysis.CallTreeItem> CallTreeItemList { get { return _CallTreeList; } set { _CallTreeList = value; OnPropertyChanged("CallTreeItemList"); } } 
         
        #endregion

        #region ParsingProcessingInfo
        public string ParsingProcessingInfo
        {
            get { return _Parsinginfo; }
            set
            {
                _Parsinginfo = value;
                //Application.Current.Dispatcher.Invoke(() => { OnPropertyChanged(); });
                OnPropertyChanged("ParsingProcessingInfo");
            }
        }
        private string _Parsinginfo; 
        #endregion

        #region ScmdStartParasing

        ICommand _scmdStartParsing;
        public ICommand ScmdStartParasing { 
            get
            {
                if (_scmdStartParsing == null)
                {
                    _scmdStartParsing = base.CreateCommand(delegate
                    {
                        base.InvokeAsyncAction(ExecParase);
                    });
                   // _scmdStartParsing = CreateCommand(ExecParase);
                }
                return _scmdStartParsing;
            } 
        }
        internal void ExecParase()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CallTreeDataList.Clear();
            });

            if (string.IsNullOrWhiteSpace(CallTreeAnalysisDirectory))
            {
                MessageBox.Show("Directory를 지정하세요");
                return;
            }

            sourceDirectroy = CallTreeAnalysisDirectory.LeftBySearch("Source") + "Source\\";
            Application.Current.Dispatcher.Invoke(() =>
            {
                CallTreeItemList = new ObservableCollection<Core.CallTreeAnalysis.CallTreeItem>();
            });


            if (Directory.Exists(CallTreeAnalysisDirectory))
            {
                DirTotal = Directory.GetDirectories(CallTreeAnalysisDirectory, "*", SearchOption.AllDirectories).Count();
                
                var dir = new DirectoryInfo(CallTreeAnalysisDirectory);
                ParsingFilesInDirectory(dir);
                RecursiveDirectoryParsing(dir);
            }
            else if (File.Exists(CallTreeAnalysisDirectory))
            {
                List<CallTreeAnalysis.CallTreeItem> list = null;
                if (CallTreeAnalysisDirectory.Contains(".WinUI"))
                {
                    list = Core.CallTreeAnalysis.GetCallTree(CallTreeAnalysisDirectory, Statics);
                }
                else
                {
                    list = AnalysisBizLayer(new FileInfo(CallTreeAnalysisDirectory));
                }
              
                foreach (var item in list)
                {
                    CallTreeItemList.Add(item);
                    /* View 용 */
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        string minfos = (item.Namespace + "." + item.ClassName + "." + item.MethodName) + "("+ item.MethodParams+")";
                        string callinfos = item.CallObjectNamespace + "." + item.CallObjectName + "." + item.CallFunctionName + "(" + item.CallFunctionParams + ")";
                        CallTreeDataList.Add(new CallTreeData() { CallFrom = minfos, CallTo = callinfos });
                    });
                }
            }
            else
            {
                MessageBox.Show("Directory 를 탐색 할 수 없습니다.");
                return;
            }
            AppManager.Current.Settings.Set("IgnoreObjNames", this.IgnoreObjNames);
            AppManager.Current.Settings.Set("CallTreeAnalysisDirectory", this.CallTreeAnalysisDirectory);

        }
        #endregion

        #region ScmdInsertDatabase
        ICommand mScmdInsertDatabase;
        /// <summary>
        /// 커멘드에 대한 설명
        /// </summary>
        public ICommand ScmdInsertDatabase
        {
            get
            {
                if (mScmdInsertDatabase == null)
                    mScmdInsertDatabase = base.CreateCommand(delegate
                    {
                        base.InvokeAsyncAction(onExcuteScmdInsertDatabase);
                    });
                return mScmdInsertDatabase;
            }
        }
        public void onExcuteScmdInsertDatabase()
        {
            DirTotal = CallTreeItemList.Count();
            /* insert Data */
            foreach (var c in CallTreeItemList)
            {
                DirCurrent++;
                //string minfos = (c.Namespace + "." + c.ClassName + "." + c.MethodName) + "(" + c.MethodParams + ")";
                //if (minfos.Length <= 300)
                //    minfos.PadRight(500 - minfos.Length);
                //string callinfos = " =>>>>[call to] =" + c.CallObjectName + "." + c.CallFunctionName + "()";
                //var arr = new string[] { minfos, callinfos };
                //this.CallTreeDT.Rows.Add(arr);

                //ParsingProcessingInfo += "\r\n " + minfos + " \r\n\t\t =>>>>[call to] =" + c.CallObjectName + "." + c.CallFunctionName + "()";
                //InsertData(c.ClassName, c.MethodName, c.Namespace, c.Layer, c.ReturnValue, c.MethodParams, c.CallObjectName, c.CallFunctionName);
                InsertData(c);
            }

            ExecScmdUpdateParameter();
            ExecScmdUpdateDBName();
        }

        #endregion

        #region ScmdUpdateParameter

        private ICommand _ScmdUpdateParameter;

        public ICommand ScmdUpdateParameter
        {
            get

            {
                if (_ScmdUpdateParameter == null)
                {
                    _ScmdUpdateParameter = base.CreateAsyncCommand(ExecScmdUpdateParameter);
                }
                return _ScmdUpdateParameter;
            }
        }

        private Task ExecScmdUpdateParameter()
        {   
            var task = Task.Run(delegate
            {
                var ds = DataAccess.GetDataSet2(AppManager.DataConnectionString, "SELECT * FROM TBL_BIZ_INFO WHERE ISNULL(PARAMSCNT, -1) < 0", CommandType.Text);
                DirTotal = ds.Tables[0].Rows.Count;
                var sqlConn = new SqlConnection(AppManager.DataConnectionString);
                sqlConn.Open();
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    DirCurrent++;
                    var callerParams = row.ToStr("PARAMS");
                    var calleeParams = row.ToStr("CALLFUNCPARAMS");

                    var sql = string.Format("UPDATE TBL_BIZ_INFO SET PARAMSCNT={0}, CALLPARAMSCNT={1} WHERE ID={2}"
                        , GetParmasCount(callerParams), GetParmasCount(calleeParams), row.ToInt("ID"));
                    var sqlCmd = new SqlCommand(sql, sqlConn);                    
                    sqlCmd.ExecuteNonQuery();
                }
                sqlConn.Close();
            });

            return task;
        }

        public static int GetParmasCount(string parameter)
        {
            if (string.IsNullOrEmpty(parameter)) return 0;
            var value = parameter.Trim();
            var parameters = StringHelper.GetParams(value);
            return parameters.Count();
        }

        #endregion

        #region SCmdUpdateDBName

        private ICommand _SCmdUpdateDBName;

        public ICommand SCmdUpdateDBName
        {
            get
            {
                if (_SCmdUpdateDBName == null)
                {
                    _SCmdUpdateDBName = base.CreateAsyncCommand(ExecScmdUpdateDBName);
                }
                return _SCmdUpdateDBName;
            }
        }

        private Task ExecScmdUpdateDBName()
        {
            var task = Task.Run(delegate
            {
                var ds = DataAccess.GetDataSet2(AppManager.DataConnectionString, "SELECT * FROM TBL_BIZ_INFO WHERE LAYER='DA' AND ISNULL(DBNAME, '') = ''", CommandType.Text);
                DirTotal = ds.Tables[0].Rows.Count;
                var sqlConn = new SqlConnection(AppManager.DataConnectionString);
                sqlConn.Open();

                var connStr = "Data Source=10.10.11.20;Initial Catalog=HISS;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;";
                var conn = new SqlConnection(connStr);
                conn.Open();

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    DirCurrent++;

                    var spName = row.ToStr("CALLOBJNM").Trim();

                    var query = string.Format(@"
select * from HISE.INFORMATION_SCHEMA.ROUTINES where SPECIFIC_NAME=@UspName
union select * from HISH.INFORMATION_SCHEMA.ROUTINES where SPECIFIC_NAME=@UspName
union select * from HISI.INFORMATION_SCHEMA.ROUTINES where SPECIFIC_NAME=@UspName
union select * from HISM.INFORMATION_SCHEMA.ROUTINES where SPECIFIC_NAME=@UspName
union select * from HISO.INFORMATION_SCHEMA.ROUTINES where SPECIFIC_NAME=@UspName
union select * from HISP.INFORMATION_SCHEMA.ROUTINES where SPECIFIC_NAME=@UspName
union select * from HISR.INFORMATION_SCHEMA.ROUTINES where SPECIFIC_NAME=@UspName
union select * from HISRS.INFORMATION_SCHEMA.ROUTINES where SPECIFIC_NAME=@UspName
union select * from HISS.INFORMATION_SCHEMA.ROUTINES where SPECIFIC_NAME=@UspName
union select * from HISU.INFORMATION_SCHEMA.ROUTINES where SPECIFIC_NAME=@UspName
union select * from HISZ.INFORMATION_SCHEMA.ROUTINES where SPECIFIC_NAME=@UspName
", spName);
                    var sqlCommand = new SqlCommand(query, conn);
                    sqlCommand.Parameters.Add(new SqlParameter("@UspName", spName));
                    var sqlAdapter = new SqlDataAdapter(sqlCommand);
                    var dt = new DataTable();
                    sqlAdapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        var dbName = dt.Rows[0].ToStr("SPECIFIC_CATALOG");
                        query = string.Format("UPDATE TBL_BIZ_INFO SET DBName='{0}' WHERE ID={1}"
                            , dbName, row.ToInt("ID"));
                        var sqlCmd = new SqlCommand(query, sqlConn);
                        sqlCmd.ExecuteNonQuery();
                    }
                }
                sqlConn.Close();
                conn.Close();
            });

            return task;
        }

        #endregion

        #region DirTotal
        public double DirTotal { get { return _DirTotal; } set { _DirTotal = value; DirCurrent = 0; OnPropertyChanged(); } }
        private double _DirTotal; 
        #endregion

        #region DirCurrent

        public double DirCurrent { get { return _DirCurrent; } set { _DirCurrent = value; OnPropertyChanged(); } }
        private double _DirCurrent; 
        #endregion

        #region FileTotal
        public double FileTotal { get { return _FileTotal; } set { _FileTotal = value; FileCurrent = 0; OnPropertyChanged(); } }
        private double _FileTotal; 
        #endregion

        #region FileCurrent
        public double FileCurrent { get { return _FileCurrent; } set { _FileCurrent = value; OnPropertyChanged(); } }
        private double _FileCurrent; 
        #endregion

        #region IgnoreObjNames

        private string _IgnoreObjNames;
        ///
        public string IgnoreObjNames
        {
            get
            {
                if (string.IsNullOrEmpty(_IgnoreObjNames))
                {
                    _IgnoreObjNames = AppManager.Current.Settings.Get("IgnoreObjNames");
                    if(_IgnoreObjNames==null)
                        _IgnoreObjNames = "DataTable,DataSet,\",[,throw,Hashtable,FileStream,StreamWriter,ArrayList,DataColumn,StringBuilder"; 
                    ignoreList = IgnoreObjNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
                return _IgnoreObjNames;
            }
            set
            {
                _IgnoreObjNames = value;
                ignoreList = IgnoreObjNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                this.OnPropertyChanged("");
            }
        }
        string[] ignoreList;
        private bool ContainIgnoreObjectCase(string line)
        {
            bool rvalue = false;
            foreach(string ig in ignoreList)
            {
                if (line.Contains(ig))
                {
                    rvalue = true;
                    break;
                }
            }
            return rvalue;
        }
        #endregion

        #region Statics

        private string _Statics;
        ///
        public string Statics
        {
            get
            {
                if (string.IsNullOrEmpty(_Statics))
                {
                    _Statics = AppManager.Current.Settings.Get("Statics");
                    if(_Statics==null)
                        _Statics = "HIS.WinUI.MD.COM.CA.Pat.RetrUnitNoController, HIS.WinUI.SP.LAB.CM.Library.LabCommLib"; 
                }
                return _Statics;
            }
            set
            {
                _Statics = value;
                this.OnPropertyChanged("");
            }
        }

        #endregion
        
        public BizCallTreeAnalysisVM()
        {
            CallTreeAnalysisDirectory =  AppManager.Current.Settings.Get("CallTreeAnalysisDirectory", this.CallTreeAnalysisDirectory);
        }
       
        void RecursiveDirectoryParsing(DirectoryInfo dirInfo )
        {   
            foreach(var dir in dirInfo.GetDirectories())
            {
                DirCurrent++;

                ParsingFilesInDirectory(dir);
                RecursiveDirectoryParsing(dir);
            }
        }
       
        void ParsingFilesInDirectory(DirectoryInfo dir)
        {
            if (dir.Name.Contains("HIS.BIZ.") || dir.Name.Contains("HIS.DA.") || dir.Name.Contains(".Facade.") || dir.Name.Contains(".WinUI"))
            {
                var files = dir.GetFiles();
                FileTotal = files.Count();
                foreach (var fi in files)
                {
                    FileCurrent++;
                    if (fi.Extension != ".cs") continue;
                    List<CallTreeAnalysis.CallTreeItem> list = null;
                    if(dir.Name.Contains(".WinUI"))
                    {
                        list = Core.CallTreeAnalysis.GetCallTree(fi.FullName, Statics);
                    }
                    else
                    {
                        list = AnalysisBizLayer(fi);
                    }

                    if (list == null) continue;
                    foreach (var item in list)
                    {
                       
                        /* View 용 */
                        Application.Current.Dispatcher.Invoke(() => {

                            CallTreeItemList.Add(item);
                            

                            string minfos = string.Format("{0}.{1}.{2}({3})", item.Namespace, item.ClassName, item.MethodName, item.MethodParams);
                            string callinfos = string.Format("{0}.{1}.{2}({3})", item.CallObjectNamespace, item.CallObjectName, item.CallFunctionName, item.CallFunctionParams);

                            CallTreeDataList.Add(new CallTreeData() { CallFrom = minfos, CallTo = callinfos });
                        });
                    }
                    //OnPropertyChanged("CallTreeItemList");
                    //CallTreeItemList = list;
                }
            }
            
        }

        List<CallTreeAnalysis.CallTreeItem> AnalysisBizLayer(FileInfo fi)
        {
            List<CallTreeAnalysis.CallTreeItem> callTreeItemList = new List<CallTreeAnalysis.CallTreeItem>();
            using (System.IO.StreamReader file = new System.IO.StreamReader(fi.FullName, Encoding.Default, true))
            {
                // using 블럭을 만들어 둠
                #region 로직
                List<string> nameSpaceList = new List<string>();
                string line = string.Empty;
                string nmSpace = string.Empty;
                string classNM = string.Empty;
                while ((line = file.ReadLine()) != null)
                {
                    line = line.Replace("\t", " ").Trim();
                    line = line.RegexReplace(@"\s+", " ");
                    if (line.StartsWith("//")) continue;
                    // namespace 명
                    if (line.StartsWith("namespace"))
                    {
                        nmSpace = line.Split(' ')[1];
                        continue;
                    }

                    if (line.StartsWith("using"))
                        nameSpaceList.Add(line.Replace("using", "").Replace(";","").Trim());

                    // class 명
                    if (line.StartsWith("public class"))
                    {
                        classNM = line.Split(' ')[2];
                        if (classNM.LastIndexOf(":") > 0)
                            classNM = classNM.Remove(classNM.LastIndexOf(":"));
                        continue;
                    }

                    int blockCnt = 0;
                    // method 명
                    if (line.StartsWith("public") && line.Contains("(") && !line.Replace(" ","").Contains("public"+ classNM))
                    {
                        string methodLine = line;
                        if (!line.Contains(")"))
                        {
                            while ((line = file.ReadLine()) != null)
                            {
                                methodLine += line;
                                if (line.Contains(")"))
                                    break;
                            }
                        }
                        if (line.Contains("{")) blockCnt++;
                        string[] methodSplit = methodLine.Split('(');
                        string[] methodInfo = methodSplit[0].Split(' ');
                        string methodNM = methodInfo[2];
                        string rtnValue = methodInfo[1];
                        string paramInfo = methodSplit[1].Replace(")", "").Replace(" ","").Replace("\t","");
                        string layer = string.Empty;
                        // call hierarchy info
                        List<CallTreeAnalysis.CallTreeItem> callTreeItemList2 = null;
                        if (nmSpace.Contains(".BIZ."))
                        {
                            callTreeItemList2  = GetFacadeClassInfo(file, blockCnt, nameSpaceList);
                            callTreeItemList.ForEach(x => x.Layer = "BIZ");
                            //return callTreeItemList2;
                        }
                        else if (nmSpace.Contains(".DA."))
                        {
                            callTreeItemList2 = GetDAClassInfo(file, blockCnt, nameSpaceList, methodNM);
                            callTreeItemList.ForEach(x => x.Layer = "DA");
                        }
                        else if (nmSpace.Contains(".Facade."))
                        {
                            callTreeItemList2 = GetFacadeClassInfo(file, blockCnt, nameSpaceList);
                            callTreeItemList.ForEach(x => x.Layer = "Facade");
                        }
                        if (callTreeItemList2 != null && callTreeItemList2.Count>0)
                        {
                            foreach (var item in callTreeItemList2)
                            {
                                item.ClassName = classNM;
                                item.MethodName = methodNM;
                                item.Namespace = nmSpace;
                                item.Layer = layer;
                                item.ReturnValue = rtnValue;
                                item.MethodParams = paramInfo;
                                callTreeItemList.Add(item);
                            }
                        }
                        else
                        {
                            var item = new CallTreeAnalysis.CallTreeItem()
                            {
                                ClassName = classNM,
                                MethodName = methodNM,
                                Namespace = nmSpace,
                                Layer = layer,
                                ReturnValue = rtnValue,
                                MethodParams = paramInfo,
                                CallObjectName = ""
                            };
                            callTreeItemList.Add(item);
                        }
                    } 
                } 
                #endregion
            }
            return callTreeItemList;
        }



        List<CallTreeAnalysis.CallTreeItem> GetDAClassInfo(System.IO.StreamReader file, int blockCnt, List<string> nmSpaceList, string methodNM)
        {
            string regEx = string.Empty;
            string code = GetMethodCodeBlock(file, blockCnt);
            string strline =  code.LastBetween("SqlHelper", ");");
            List<CallTreeAnalysis.CallTreeItem> rtnCalltreeList = new List<Core.CallTreeAnalysis.CallTreeItem>();

            List<string> spnamelist = new List<string>();
            // sp 찾기
            using(StringReader sr = new StringReader(code))
            {
                string line = string.Empty;
                bool beginBlock = false;
                #region sqlhelper 를 찾아 spnamelist에 추가
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("/*"))
                        beginBlock = true;

                    if (line.EndsWith("*/"))
                        beginBlock = false;

                    if (line.StartsWith("//"))
                        continue;

                    if (beginBlock) continue;
                    if (line.Contains("//"))
                        line = line.Remove(line.LastIndexOf("//"));
                    if (line.Contains("SqlHelper"))
                    {
                        string spLine = line.RightBySearch("SqlHelper");
                        int functionBlockCont = 0;
                        functionBlockCont = line.Count(x=>x =='(');  // SqlHelper.FillDataset(this.ConnectionString, CommandType.StoredProcedure,strSPNm, ds, tableArr, sqlParams);
                        functionBlockCont -= line.Count(x=>x==')');
                        if( functionBlockCont !=0 || !line.Contains("("))
                        {
                            while ((line = sr.ReadLine()) != null)
                            {
                                spLine += line;
                                functionBlockCont += line.Count(x => x == '(');
                                functionBlockCont -= line.Count(x => x == ')');
                                if (functionBlockCont == 0)
                                {
                                    break;
                                }
                            }
                        }
                        

                        string[] splines = spLine.Split(',');
                        string spName = string.Empty;
                        if (splines.Count() < 3) continue;
                        if (splines[1].Contains("CommandType"))
                            spName = splines[2].Trim();
                        else
                            spName = splines[1].Trim();
                        spnamelist.Add(spName);
                    } 
                #endregion
                }
            }
            
            //SqlHelper.[\w \t=" + "\"" + @"\(,@\)\.]*
            // 다른 메소드를 콜하는 경우 메소드 이름을 넘겨주자.
            if (spnamelist.Count == 0)
            {
                if(code.Contains(methodNM))
                {
                    CallTreeAnalysis.CallTreeItem item = new Core.CallTreeAnalysis.CallTreeItem();
                    item.CallFunctionName = methodNM;
                    item.CallFunctionParams = code.Between(methodNM, ";", false, true).Trim('(').Trim(')');
                    rtnCalltreeList.Add(item);
                    return rtnCalltreeList;
                }
                else
                {
                   return GetCallTreeItem(nmSpaceList, code);
                }
                  
            }

            foreach(string spv in spnamelist)
            {
                string spName = spv;
                if (!spName.Contains("\"")) //spName 이 "US" 와 같이 문자열이 아닌경우 string builder 로 append 하는 경우
                {

                    if (spName.Contains(".ToString")) // stringbuilder 인경우
                    {
                        spName = spName.Remove(spName.LastIndexOf("."));
                        regEx = spName + @".Append.*?;";
                        spName = string.Empty;
                        foreach (var m in Regex.Matches(code, regEx, RegexOptions.Multiline))
                        {
                            string qdata = m.ToString();

                            spName += qdata.Between("(", ";").Replace("\"", "").Replace("\t", " ").Replace(")", "");
                        }
                        rtnCalltreeList.Add(new CallTreeAnalysis.CallTreeItem()
                        {
                            CallObjectName = spName
                        });
                    }
                    else
                    {
                        //regEx = spName.Replace(");","") + @"[\w \t=" + "\"" + @"\(,@\)\.]*;";
                        string q = spName.Replace(");","");
                        regEx = q.Replace(");", "") + ".*?;";
                        foreach (var m in Regex.Matches(code, regEx, RegexOptions.Singleline))
                        {
                            string qdata = m.ToString();
                            if(qdata.Contains("SqlHelper") || qdata.Contains("ds") || qdata.Contains(q+",") || qdata.Contains(qdata+" ,")) continue;

                            spName = qdata.Between("\"", ";").Replace("\"", "").Replace("\t", " ").Trim();
                            if (!string.IsNullOrEmpty(spName) && rtnCalltreeList.Count(x => x.CallObjectName == spName) == 0 )
                            {
                                rtnCalltreeList.Add(new CallTreeAnalysis.CallTreeItem()
                                {
                                    CallObjectName = spName.Replace("\"","")
                                });
                            }
                        }
                    }
                }
                else
                {
                    rtnCalltreeList.Add(new CallTreeAnalysis.CallTreeItem()
                    {
                        CallObjectName = spName.Replace("\"","")
                    });
                }

            }
            
            
           return rtnCalltreeList;
        }

        List<CallTreeAnalysis.CallTreeItem> GetFacadeClassInfo(System.IO.StreamReader file, int blockCnt, List<string> nmSpaceList)
        {
            
           
            string code = GetMethodCodeBlock(file, blockCnt);
            
            List<CallTreeAnalysis.CallTreeItem> callTreeItemList = GetCallTreeItem(nmSpaceList, code);
            return callTreeItemList;

            //while ((line = file.ReadLine()) != null)
            //{
            //    line = line.Trim();
            //    if (line.StartsWith("/*"))
            //        beginBlock = true;

            //    if (line.EndsWith("*/"))
            //        beginBlock = false;

            //    if (line.StartsWith("//"))
            //        continue;

            //    if (beginBlock) continue;

            //    if (line.Contains("//"))
            //        line = line.Remove(line.LastIndexOf("//"));

            //    if (line.Contains("{"))
            //    {
            //        blockCnt++;
            //    }
            //    if (line.Contains("}"))
            //    {
            //        blockCnt--;
            //    }

            //    // biz call 부분 // using 으로 가자
            //    if (line.Contains("using")) //line.Contains("HIS.BIZ.") ||
            //    {
            //        if (!line.Contains(")"))
            //        {
            //            while ((line += file.ReadLine()) != null)
            //            {
            //                if (line.Contains(")")) break;
            //            }
            //        }

            //        callObjName = line.LastBetween("new", "(").Trim();
            //        if (callObjName.Contains("."))
            //            callObjName = callObjName.RightBySearch(".");
            //        bizInstanceName = line.BetweenWithEmpty(callObjName, "=").Trim();
            //        //UserNTx tc = new UserNTx()
            //        //using (HIS.BIZ.CM.COM.UM.ServerWorkNTx tc = new ServerWorkNTx())
            //        //callObjName = line.Split('=')[0].Trim();
            //        //if (callObjName.Contains("."))
            //        //    callObjName = callObjName.LastBetween(".", " ");
            //        //else
            //        //    callObjName = callObjName.Between("(", " ");
                    
            //       // bizInstanceName = line.Between(callObjName,"=").Trim();
            //    }
            //    else if(!string.IsNullOrEmpty(bizInstanceName) && line.Contains(bizInstanceName+"."))
            //    {
            //        callMethodNM = line.Between(".", "(");
            //    }

            //    if (blockCnt == 0)
            //        break;
            //}
        }

        private List<Core.CallTreeAnalysis.CallTreeItem> GetCallTreeItem(List<string> nmSpaceList, string code)
        {
            string bizInstanceName = string.Empty;
            string line = string.Empty;
            string regEx = @"[^\s][^\n]* new [^\n]*";
            List<CallTreeAnalysis.CallTreeItem> callTreeItemList = new List<CallTreeAnalysis.CallTreeItem>();
            string callObjName = string.Empty;
            string callNMSpace = string.Empty;

            foreach (var m in Regex.Matches(code, regEx))
            {
                line = m.ToString();
                if (ContainIgnoreObjectCase(line))
                {
                    continue;
                }
                callObjName = line.LastBetween("new", "(").Trim();
                if (callObjName.Contains("."))
                {
                    callObjName = callObjName.RightBySearch(".");
                }

                string tmns = line.Left(line.IndexOf(callObjName));
                if (!tmns.Contains("."))
                {
                    callNMSpace = FindNameSpace(nmSpaceList, callObjName);
                }
                else
                    callNMSpace = tmns.Replace("=", "").Replace("new", "").Trim('.').Trim().Trim('{').Trim().TrimStart('(');

                bizInstanceName = line.BetweenWithEmpty(callObjName, "=").Trim();
                #region 메소드 추출 = 여러번 콜 할 수 있다.
                using (StringReader sr = new StringReader(code))
                {
                    string mline = string.Empty;
                    string cllfunction = string.Empty;
                    bool beginBlock = false;
                    while ((mline = sr.ReadLine()) != null)
                    {
                        if (mline.StartsWith("/*"))
                            beginBlock = true;

                        if (mline.EndsWith("*/"))
                            beginBlock = false;

                        if (mline.StartsWith("//") || beginBlock)
                            continue;

                        if (mline.Contains(bizInstanceName + ".") && !mline.Contains("new"))
                        {
                            cllfunction = mline.RightBySearch(bizInstanceName + ".", false, true);
                            int functionBlockCont = 0;
                            functionBlockCont = mline.Count(x => x == '(');
                            functionBlockCont -= mline.Count(x => x == ')');
                            if (functionBlockCont != 0)
                            {
                                while ((mline = sr.ReadLine()) != null)
                                {
                                    if (mline.StartsWith("/*"))
                                        beginBlock = true;

                                    if (mline.EndsWith("*/"))
                                        beginBlock = false;

                                    if (mline.StartsWith("//") || beginBlock)
                                        continue;
                                    if (mline.Contains("//"))
                                        mline = mline.Remove(mline.LastIndexOf("//"));
                                    cllfunction += mline;
                                    functionBlockCont += mline.Count(x => x == '(');
                                    functionBlockCont -= mline.Count(x => x == ')');
                                    if (functionBlockCont == 0)
                                    {
                                        break;
                                    }
                                }
                            }



                            string functionNM = cllfunction.Between(".", "(");
                            string functionParam = cllfunction.Substring(cllfunction.IndexOf("(") + 1).Replace(");", "");
                            // 이미 들어가 있는 call object 면 제거
                            if (callTreeItemList.Count(x => x.CallFunctionName == functionNM &&
                                                        x.CallObjectName == callObjName &&
                                                        x.CallObjectNamespace == callNMSpace &&
                                                        x.CallFunctionParams.Split(',').Count() == functionParam.Split(',').Count()) == 0)
                            {

                                var callTreeItem = new CallTreeAnalysis.CallTreeItem();
                                callTreeItem.CallObjectNamespace = callNMSpace.Replace("using", "").Trim().TrimStart('(').Trim();
                                callTreeItem.CallObjectName = callObjName;
                                callTreeItem.CallFunctionName = functionNM;
                                callTreeItem.CallFunctionParams = functionParam;
                                callTreeItemList.Add(callTreeItem);
                            }
                        }
                    }
                }
                #endregion
            }
            return callTreeItemList;
        }


        string GetMethodCodeBlock(System.IO.StreamReader file, int blockCnt )
        {
            string line = string.Empty;
            StringBuilder sbMethodCode = new StringBuilder();
                       
            bool beginBlock = false;
            while ((line = file.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0) continue;

                if (line.StartsWith("/*"))
                    beginBlock = true;

                if (line.EndsWith("*/"))
                    beginBlock = false;

                if (line.StartsWith("//"))
                    continue;

                if (beginBlock) continue;

                if (line.Contains("//"))
                    line = line.Remove(line.LastIndexOf("//"));

                if(line.Contains("using") && !line.Contains("new"))
                    sbMethodCode.Append(line);
                else if(!line.Contains(".commandTimeOut"))
                    sbMethodCode.AppendLine(line);
                

                //if (line.Contains("using") && line.Contains("\r\n"))
                //    line.Replace("\r\n", "");

                if (line.Contains("{"))
                {
                    blockCnt++;
                }
                if (line.Contains("}"))
                {
                    blockCnt--;
                }

                if (blockCnt == 0)
                    break;
            }
            return sbMethodCode.ToString();
        }
        
        string FindNameSpace(List<string> nsList, string clsNM )
        {
            string nmspace = string.Empty;
            string folderPath = string.Empty;
            foreach(var ns in nsList)
            {
                string[] splitns = ns.Split('.');
                if(ns.Contains("BIZ") || ns.Contains("DA"))
                {
                    if(splitns.Length>=4)
                        folderPath = sourceDirectroy + @"COM\" + splitns[1] + "\\" + splitns[2] + "\\" + splitns[3] + "\\" + ns;
                }
                else if(ns.Contains("Facade"))
                {
                    if (splitns.Length >= 3)
                    folderPath = sourceDirectroy + @"Facade\" + splitns[2] + "\\" + ns;
                }
                else if(ns.Contains("WinUI"))
                {
                    if (splitns.Length >=4)
                        folderPath = sourceDirectroy + @"WinUI\" + splitns[2] + "\\" +splitns[3] + "\\" + ns;
                }

                if(File.Exists(folderPath + "\\" + clsNM+".cs"))
                {
                    nmspace = ns;
                    break;
                }
            }
            return nmspace;
        }



        string GetUsingCodeBlock(string code, string csStart)
        {
            StringBuilder sbMethodCode = new StringBuilder();
            // usinig 블럭을 찾기 위한 로직
            using (StringReader sr = new StringReader(code))
            {
                csStart = csStart.Replace("\r", "");
                string line = string.Empty;
                bool beginBlock = false;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    
                    if (line.StartsWith("/*"))
                        beginBlock = true;

                    if (line.EndsWith("*/"))
                        beginBlock = false;

                    if (line.StartsWith("//"))
                        continue;

                    if (beginBlock) continue;

                    if (line.Contains("//"))
                        line = line.Remove(line.LastIndexOf("//"));

                    if (line == csStart)
                    {
                        int blockCnt = 0;
                        while ((line = sr.ReadLine()) != null)
                        {
                            sbMethodCode.AppendLine(line);
                            if (line.Contains("{"))
                            {
                                blockCnt++;
                            }
                            if (line.Contains("}"))
                            {
                                blockCnt--;
                            }

                            if (blockCnt < 0)
                                break;
                        }
                    }

                }
            }
            return sbMethodCode.ToString();
        }
        //void InsertData(string classNM, string methodNM, string namespaceName, string layer, string rtnValue, string paramInfos,
        //    string callObject, string callfuncNM)
        //{
        //    SqlConnection conn = new SqlConnection(AppManager.DataConnectionString);

        //    SqlCommand sqlCmd = new SqlCommand("USP_SAVE_BIZINFO", conn);
        //    sqlCmd.CommandType = CommandType.StoredProcedure;
        //    sqlCmd.Parameters.AddWithValue("@className", classNM);
        //    sqlCmd.Parameters.AddWithValue("@method", methodNM);
        //    sqlCmd.Parameters.AddWithValue("@layer", layer);
        //    sqlCmd.Parameters.AddWithValue("@namespace", namespaceName);
        //    sqlCmd.Parameters.AddWithValue("@rtnvalue", rtnValue);
        //    sqlCmd.Parameters.AddWithValue("@params", paramInfos);
        //    sqlCmd.Parameters.AddWithValue("@callobjNM", callObject);
        //    sqlCmd.Parameters.AddWithValue("@callFuncNM", callfuncNM+"");
        //    conn.Open();
        //    sqlCmd.ExecuteNonQuery();
        //    conn.Close();
        //}

        private void InsertData(CallTreeAnalysis.CallTreeItem c)
        {
            SqlConnection conn = new SqlConnection(AppManager.DataConnectionString);

            SqlCommand sqlCmd = new SqlCommand("USP_SAVE_BIZINFO", conn);
            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("@layer", c.Layer);
            sqlCmd.Parameters.AddWithValue("@namespace", c.Namespace);
            sqlCmd.Parameters.AddWithValue("@className", c.ClassName);
            sqlCmd.Parameters.AddWithValue("@method", c.MethodName);
            sqlCmd.Parameters.AddWithValue("@params", c.MethodParams);                        
            sqlCmd.Parameters.AddWithValue("@rtnvalue", c.ReturnValue);
            sqlCmd.Parameters.AddWithValue("@callobjNS", "" + c.CallObjectNamespace);
            sqlCmd.Parameters.AddWithValue("@callobjNM", c.CallObjectName+"");
            sqlCmd.Parameters.AddWithValue("@callFuncNM", "" + c.CallFunctionName);
            sqlCmd.Parameters.AddWithValue("@callFuncParams", "" + c.CallFunctionParams);
            conn.Open();
            var result = sqlCmd.ExecuteNonQuery();
            conn.Close();
        }

        //List<CallTreeAnalysis.CallTreeItem> GetBizClassInfo(System.IO.StreamReader file, int blockCnt, List<string> nmSpaceList)
        //{
        //    string code = GetMethodCodeBlock(file, blockCnt);

        //    bool beginBlock = false;
        //    StringBuilder sbMethodCode = new StringBuilder();
        //    // usinig 블럭을 찾기 위한 로직
        //    List<CallTreeAnalysis.CallTreeItem> callTreeItemList = new List<CallTreeAnalysis.CallTreeItem>();



        //    using (StringReader sr = new StringReader(code))
        //    {
        //        string line = string.Empty;
        //        while ((line = sr.ReadLine()) != null)
        //        {
        //            if (line.StartsWith("/*"))
        //                beginBlock = true;

        //            if (line.EndsWith("*/"))
        //                beginBlock = false;

        //            if (line.StartsWith("//") || beginBlock)
        //                continue;

        //            if (line.Contains("new ") &&
        //                !line.Contains("DataTable") && !line.Contains("DataSet") && !line.Contains("\"") && !line.Contains("[") && !line.Contains("throw")
        //                && !line.Contains("Hashtable("))
        //            {
        //                string methodLine = line;
        //                if (!line.Contains(")"))
        //                {
        //                    while ((line = file.ReadLine()) != null)
        //                    {
        //                        methodLine += line;
        //                        if (line.Contains(")"))
        //                            break;
        //                    }
        //                }

        //                string callObjName = string.Empty;
        //                string callNamespace = string.Empty;
        //                if (line.Contains("{"))
        //                {
        //                    blockCnt++;
        //                }

        //                callObjName = line.LastBetween("new", "(").Trim();

        //                if (callObjName.Contains("."))
        //                {
        //                    callObjName = callObjName.RightBySearch(".");
        //                    callNamespace = callObjName.Left(callObjName.LastIndexOf("."));
        //                }
        //                else
        //                {
        //                    string tmns = line.Left(line.LastIndexOf(callObjName));
        //                    if (!tmns.Contains("."))
        //                    {
        //                        callNamespace = FindNameSpace(nmSpaceList, callObjName);
        //                    }
        //                    else
        //                        callNamespace = tmns.RightBySearch("(");

        //                }

        //                string bizInstanceName = line.BetweenWithEmpty(callObjName, "=").Trim();

        //                while ((line = sr.ReadLine()) != null)
        //                {
        //                    if (line.Contains("{"))
        //                    {
        //                        blockCnt++;
        //                    }
        //                    if (line.Contains("}"))
        //                    {
        //                        blockCnt--;
        //                    }

        //                    if (blockCnt < 0)
        //                        break;

        //                    if (line.Contains(bizInstanceName + "."))
        //                    {
        //                        var callTreeItem = new CallTreeAnalysis.CallTreeItem();
        //                        callTreeItem.CallObjectNamespace = callNamespace;
        //                        callTreeItem.CallObjectName = callObjName;
        //                        callTreeItem.CallFunctionName = line.Between(".", "(");
        //                        callTreeItem.CallFunctionParams = line.Between("(", ")");
        //                        callTreeItemList.Add(callTreeItem);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return callTreeItemList;
        //}
    }

    public class CallTreeData
    {
        public string CallFrom { get; set; }
        public string CallTo { get; set; }
    }
}
