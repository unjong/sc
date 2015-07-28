using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CsFormAnalyzer.Mvvm;
using CsFormAnalyzer.Utils;
using SC.WPF.Tools.CodeHelper;
using System.Security.Principal;
using CsFormAnalyzer.Views;
using System.Text.RegularExpressions;
using System.IO;
using System.Configuration;
namespace CsFormAnalyzer.ViewModels
{
	public class CodeGen_Model_ServiceVM : ViewModelBase
    {
        string _location;
        public string Location
        {
            get { return _location; }
            set
            {
                _location = value;
                OnPropertyChanged("Location");
            }
        }


        #region DBConnectionList
        Dictionary<string, string> dbConnectionList;
        public Dictionary<string,string> DBConnectionList
        {
            get
            {
                if (dbConnectionList == null)
                {
                    dbConnectionList = new Dictionary<string, string>();
                    foreach(ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
                    {
                        dbConnectionList.Add(item.Name, item.ConnectionString);
                    }
                    

                    //dbConnectionList.Add("HP", GetDBConnectionString("HP"));
                    //dbConnectionList.Add("SP", GetDBConnectionString("SP"));
                    //dbConnectionList.Add("MR", GetDBConnectionString("MR"));
                    //dbConnectionList.Add("CM", GetDBConnectionString("CM"));
                    //dbConnectionList.Add("MD", GetDBConnectionString("MD"));
                    //dbConnectionList.Add("HD", GetDBConnectionString("HD"));
                    //dbConnectionList.Add("HISU", GetDBConnectionString("HISU"));
                    //dbConnectionList.Add("HISR", GetDBConnectionString("HISR"));
                    //dbConnectionList.Add("HISO", GetDBConnectionString("HISO"));
                    //dbConnectionList.Add("강남HP", GetDBConnectionString("강남HP"));
                    //dbConnectionList.Add("강남SP", GetDBConnectionString("강남SP"));
                }
                return dbConnectionList;
            }
        } 
        #endregion

        #region DBConnectionString

        private string _DBConnectionString;
        public string DBConnectionString
        {
            get
            {
                return _DBConnectionString;
            }
            set
            {
                _DBConnectionString = value;
                OnPropertyChanged("DBConnectionString");
            }
        } 
        #endregion

        #region SPCodeText
        string _SPCodeText;
        public string SPCodeText
        {
            get { return _SPCodeText; }
            set
            {
                _SPCodeText = value;
                OnPropertyChanged("SPCodeText");
            }
        } 
        #endregion

        #region Code_Req_Resp_Model
        string code_Req_Resp_Model;
        public string Code_Req_Resp_Model
        {
            get
            {
                return code_Req_Resp_Model;
            }
            set
            {
                code_Req_Resp_Model = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Code_Service
        string mCode_Service;
        public string Code_Service
        {
            get
            {
                return mCode_Service;
            }
            set
            {
                mCode_Service = value;
                OnPropertyChanged("Code_Service");
            }
        } 
        #endregion

        #region SPName
        string _spName;
        public string SPName
        {
            get { return _spName; }
            set { _spName = value; 
                if(!String.IsNullOrEmpty(_spName))
                {
                    SPParamsDataTable = null;
                    SPCodeText =  Code_Req_Resp_Model = Code_Service = string.Empty;
                    SPExcuteVisible = Visibility.Collapsed;

                    IsCUD = false;
                    PathVisibility = Visibility.Collapsed;

                    string[] spNameSplit = SPName.Split('_');
                    if(spNameSplit.Length>2)
                    {
                        DBConnectionString = DBConnectionList[spNameSplit[1]];
                        if (ModelNameList.ContainsKey(SPName))
                            ModelName = ModelNameList[SPName];

                        if (!string.IsNullOrEmpty(ModelName))
                        {
                            RequestName = ModelName + "Request";
                            ResponseName = ModelName + "Response";
                        }
                    }
                }
                OnPropertyChanged("SPName");
            }
        }
        #endregion

        #region ModelName
        string mModelName;
        public string ModelName
        {
            get
            {
                return mModelName;
            }
            set
            {
                RequestName = string.Empty;
                ResponseName = string.Empty;
                mModelName = value;
                OnPropertyChanged("ModelName");

                if (!string.IsNullOrEmpty(ModelName))
                {
                    RequestName = ModelName + "Request";
                    ResponseName = ModelName + "Response";
                    ServiceName = ModelName + "Service";
                }
            }
        } 
        #endregion

        #region ModelNameList
        Dictionary<string, string> mModelNameList;
        public Dictionary<string, string> ModelNameList
        {
            get
            {
                if (mModelNameList == null)
                    mModelNameList = new Dictionary<string, string>();
                return mModelNameList;
            }
            set
            {
                mModelNameList = value;
                OnPropertyChanged("ModelNameList");
            }
        } 
        #endregion


        public DataTable CallTreeDT
        {
            get;
            set;
        }


        #region SPNameList
        Dictionary<string,string> spNameList;
        public Dictionary<string, string> SPNameList
        {
            get
            {
                if (spNameList == null)
                    spNameList = new Dictionary<string, string>();
                return spNameList;
            }
            set
            {
                spNameList = value;
                OnPropertyChanged("SPNameList");
            }
        } 
        #endregion

        #region FacadeSPList
        ObservableCollection<SPInfo> _facadeSPList;
        public ObservableCollection<SPInfo> FacadeSPList
        {
            get
            {

                return _facadeSPList;
            }
            set
            {
                _facadeSPList = value;
                OnPropertyChanged("FacadeSPList");
            }
        }  
        #endregion

        #region SelectedSPInfo
        SPInfo _selectedSPInfo;
        public SPInfo SelectedSPInfo
        {
            get
            {
                return _selectedSPInfo;
            }
            set
            {
                _selectedSPInfo = value;
                OnPropertyChanged("SelectedSPInfo");
            }
        } 
        #endregion



        #region FacadeFuncList

        
        Dictionary<string, string> mFacadeFuncList;
        /// <summary>
        /// funnamespace, class + method
        /// </summary>
        public Dictionary<string, string> FacadeFuncList
        {
            get
            {
                if (mFacadeFuncList == null)
                    mFacadeFuncList = new Dictionary<string, string>();
                return mFacadeFuncList;
            }
            set
            {
                mFacadeFuncList = value;
                OnPropertyChanged("FacadeFuncList");
            }
        }
        #endregion

        #region SelectedFacadeFunc

        private string _SelectedFacadeFunc;
        ///
        public string SelectedFacadeFunc
        {
            get
            {
                return _SelectedFacadeFunc;
            }
            set
            {
                _SelectedFacadeFunc = value;
                OnPropertyChanged("SelectedFacadeFunc");
                if (SPNameList.Count == 0) return;
                RequestName = FacadeFuncList[_SelectedFacadeFunc] +"Request";
                ResponseName = FacadeFuncList[_SelectedFacadeFunc] + "Response";
                ServiceName = FacadeFuncList[_SelectedFacadeFunc] + "Service";
                // facade method 에서 호출하는 sp 를 찾음
                var spNameList = SPNameList.Where(x => x.Value == _SelectedFacadeFunc).Select(x => x.Key.Split('^')[1]);

                ObservableCollection<SPInfo> spInfos = new ObservableCollection<SPInfo>();
                foreach(var spName in spNameList)
                {
                    string[] spNameSplit = spName.Split('_');
                    string dbName = SelectedCallTreeItem.Row.ToStr("DBName");

                    if (string.IsNullOrEmpty(dbName))
                    {
                        // db name 갖어오기 
                        string query = "select DBNM from [dbo.TBL_HIS2DB_USP] where SPNM = '" + spName + "' order by DBNM desc";
                        DataSet ds = DataAccess.GetDataSet2(AppManager.DataConnectionString, query, CommandType.Text);
                        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            string dbnm = ds.Tables[0].Rows[0][0] + "";
                            switch (dbnm)
                            {
                                case "HISR": dbName = "HISR"; break;
                                case "HISZ": dbName = "CM"; break;
                                case "HISM": dbName = "MD"; break;
                                case "HISP": dbName = "HISP"; break;
                                case "HISH": dbName = "HP"; break;
                                case "HISU": dbName = "HISU"; break;
                                case "HISS": dbName = "SP"; break;
                                case "HISRS": dbName = "HISRS"; break;
                                case "MRCIMG": break;
                                case "HISE": dbName = "MR"; break;
                                case "HISO": dbName = "HISO"; break;
                            }

                            if (!string.IsNullOrEmpty(dbName) && DBConnectionList.ContainsKey(dbName))
                                dbName = DBConnectionList[dbName];
                        }
                    }

                    if(string.IsNullOrEmpty(dbName) && spNameSplit.Length>2)
                    {
                        if (DBConnectionList.ContainsKey(spNameSplit[1]))
                            dbName = DBConnectionList[spNameSplit[1]];
                        else
                            dbName = DBConnectionList["SP"];
                    }

                    string spuperName = spName.ToUpper();
                    bool isCUD = (spuperName.Contains("INSERT") || spuperName.Contains("UPDATE") ||
                    spuperName.Contains("DELETE") || spuperName.Contains("SAVE"));

                    if (string.IsNullOrEmpty(dbName))
                    {
                        var dbKey = this.SelectedFacadeFunc.Between("Facade.", ".");
                        if (DBConnectionList.ContainsKey(dbKey))
                            dbName = DBConnectionList[dbKey];
                    }

                    spInfos.Add(new SPInfo()
                    {
                        DBName = dbName,
                        SPName = spName,
                        IsSaveSP = isCUD,
                        FacadeMethodName = FacadeFuncList[_SelectedFacadeFunc],
                        FacadePath = _SelectedFacadeFunc
                    });
                }
                FacadeSPList = spInfos;
            }
        }

        #endregion

        #region RequestName
        string mRequestName;
        public string RequestName
        {
            get
            {
                return mRequestName;
            }
            set
            {
                mRequestName = value;
                OnPropertyChanged("RequestName");
            }
        }
        #endregion

        #region ServiceName
        string mServiceName;
        public string ServiceName
        {
            get
            {
                return mServiceName;
            }
            set
            {
                mServiceName = value;
                OnPropertyChanged("ServiceName");
            }
        }
        #endregion

        #region ResponseName
        string mResponseName;
        public string ResponseName
        {
            get
            {
                return mResponseName;
            }
            set
            {
                mResponseName = value;
                OnPropertyChanged("ResponseName");
            }
        }
        #endregion

        #region GenCodeList
        ObservableCollection<GenCode> _genCodeList;
        public ObservableCollection<GenCode> GenCodeList
        {
            get
            {
                if (_genCodeList == null)
                    _genCodeList = new ObservableCollection<GenCode>();
                return _genCodeList;
            }
            set
            {
                _genCodeList = value;
                OnPropertyChanged("GenCodeList");
            }
        }
        #endregion

        #region IsCUD
        bool _isCUD = false;
        public bool IsCUD
        {
            get
            {
                return _isCUD;
            }
            set
            {
                _isCUD = value;
                OnPropertyChanged("IsCUD");
            }
        } 
        #endregion

        #region IsSimple (Brace Style)
        bool _isSimple = Convert.ToBoolean(string.IsNullOrEmpty(AppManager.Current.Settings.Get("IsSimple")) ? "False" : AppManager.Current.Settings.Get("IsSimple"));
        public bool IsSimple
        {
            get { return _isSimple; }
            set
            {
                _isSimple = value;
                AppManager.Current.Settings.Set("IsSimple", value.ToString());

                OnPropertyChanged("IsSimple");
            }
        }
        #endregion

        #region PathVisibility
        Visibility _PathVisibility = Visibility.Collapsed;
        public Visibility PathVisibility
        {
            get { return _PathVisibility; }
            set 
            { 
                _PathVisibility = value;
                OnPropertyChanged("PathVisibility");
            }
        }
        #endregion

        #region ShowPath
        ICommand _ShowPath;
        public ICommand ShowPath
        {
            get
            {
                if (_ShowPath == null)
                    _ShowPath = CreateCommand(OnShowPathCommand);
                return _ShowPath;
            }
        }
        string _pathMsg = string.Empty;
        private void OnShowPathCommand(object param)
        {
            OpenMessage(_pathMsg);
        }
       
        #endregion

        #region ShowPath
        ICommand _ShowModelPath;
        public ICommand ShowModelPath
        {
            get
            {
                if (_ShowModelPath == null)
                    _ShowModelPath = CreateCommand(OnShowModelPathCommand);
                return _ShowModelPath;
            }
        }
        private void OnShowModelPathCommand(object param)
        {
            if (param != null)
                OpenMessage(param as string);
        }

        #endregion

        #region SPParamsDataTable
        DataTable spParamsDataTable;
        public DataTable SPParamsDataTable
        {
            get { return spParamsDataTable; }
            set { spParamsDataTable = value; OnPropertyChanged("SPParamsDataTable"); }
        } 
        #endregion

        #region SPExcuteVisible
        Visibility spExcuteVisible = Visibility.Collapsed;
        public Visibility SPExcuteVisible 
        {
            get { return spExcuteVisible; }
            set { spExcuteVisible = value; OnPropertyChanged("SPExcuteVisible"); }
        } 
        #endregion

        #region ScmdStartGeneration
        ICommand _scmdStartGeneration;
        public ICommand ScmdStartGeneration {
            get
            {
                if (_scmdStartGeneration == null)
                    _scmdStartGeneration = CreateCommand(OnStartGentCommand);
                return _scmdStartGeneration;
            }
        }
        private void OnStartGentCommand(object param)
        {
            string dbcon = DBConnectionString;


            if (string.IsNullOrEmpty(dbcon))
            {
                MessageBox.Show("DBConnection 입력하세요");
                return;
            }
            if (string.IsNullOrEmpty(SPName))
            {
                MessageBox.Show("SPName 입력하세요");
                return;
            }

            if(this.GenCodeList.Count(x => x.UspName == SPName) >0)
            {
                MessageBox.Show("이미 생성된 Model이 Tab 에  있습니다.");
                return;
            }

            


            #region 먼저 생성된 SP Model 이 있는지 확인
            if (SPParamsDataTable == null)
            {
                SqlParameter[] sqlparams = new SqlParameter[1];
                sqlparams[0] = new SqlParameter("@spName", SPName.Trim());

                DataSet ds2 = DataAccess.GetDataSet2(AppManager.DataConnectionString, "[USP_SELECT_SP_MODEL]", sqlparams, CommandType.StoredProcedure);
                if (ds2.Tables.Count > 0 && ds2.Tables[0].Rows.Count > 0)
                {
                    StringBuilder sbmessage = new StringBuilder();
                    sbmessage.AppendLine(SPName + " 에 대한 Model이 이미 생성되었습니다.");
                    //sbmessage.AppendLine(ds2.Tables[0].Rows[0]["SPName"] + " 이 이미 " + ds2.Tables[0].Rows[0]["ModelName"] + " 으로 만들어졌습니다");
                    foreach (DataRow row in ds2.Tables[0].Rows)
                    {
                        sbmessage.AppendLine();
                        sbmessage.AppendLine("ID\t\t: " + row["id"]);
                        //sbmessage.AppendLine("SP\t\t: " + row["SPName"]);
                        sbmessage.AppendLine("Model\t\t: " + row["ModelName"]);
                        sbmessage.AppendLine("Request\t\t: " + row["RequestName"]);
                        sbmessage.AppendLine("Response\t: " + row["ResponseName"]);
                        sbmessage.AppendLine("Location\t\t: " + row["Location"]);
                        sbmessage.AppendLine();
                    }

                    _pathMsg = sbmessage.ToString();


                    PathVisibility = Visibility.Visible;

                    MessageBoxResult msgResult = MessageBox.Show("이미 생성된 SP입니다." + Environment.NewLine +
                        "다시 생성하시겠습니까?", "Model 위치 확인", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (msgResult != MessageBoxResult.Yes)
                    {
                        OpenMessage(_pathMsg);
                        return;
                    }
                }
                else
                {
                    PathVisibility = Visibility.Collapsed;
                    _pathMsg = string.Empty;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_pathMsg))
                {
                    if (MessageBox.Show("정말 다시 생성하시겠습니까?", "Model 생성 확인", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
            }
            #endregion

            if (SPParamsDataTable == null)
            {
                DataTable dt = DBObject.GetSPParames(DBConnectionString, SPName);
                dt.Columns.Add("Value");
                foreach(DataRow dr in dt.Rows)
                {
                    int len = int.Parse(dr["length"].ToString());
                    if(len == 8 && dr["parameter"].ToString().Contains("Ymd"))
                    {
                        dr["value"] = "20080909";
                    }
                    else
                    {
                        dr["value"] = "1".PadRight(len - 1,'0');
                    }
                }


                string query = @"SELECT definition FROM sys.sql_modules WHERE object_id = (OBJECT_ID(N'[" + SPName + "]'));";
                DataSet ds = SC.WPF.Tools.CodeHelper.DataAccess.GetDataSet2(DBConnectionString, query, CommandType.Text);
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    this.SPCodeText = ds.Tables[0].Rows[0][0] + "";

                SPParamsDataTable = dt;
                SPExcuteVisible = Visibility.Visible;


                #region CUD 체크
                string tempSPName = SPName.ToUpper();
                string tempSPCodeText = SPCodeText.ToUpper();

                string prefix = string.Empty;

                IsCUD = (SPName.ToUpper().Contains("INSERT") || SPName.ToUpper().Contains("UPDATE") ||
                         SPName.ToUpper().Contains("DELETE") || SPName.ToUpper().Contains("SAVE"))
                        &&
                        (SPCodeText.ToUpper().Contains("INSERT") || SPCodeText.ToUpper().Contains("UPDATE") ||
                         SPCodeText.ToUpper().Contains("DELETE"));

                if (tempSPName.Contains("SAVE")) { prefix = "Save"; }
                else if (tempSPName.Contains("INSERT")) { prefix = "Ins"; }
                else if (tempSPName.Contains("UPDATE")) { prefix = "Upt"; }
                else if (tempSPName.Contains("DELETE")) { prefix = "Del"; }

                if (!string.IsNullOrEmpty(prefix))
                {
                    if (!tempSPCodeText.Contains("INSERT") && !tempSPCodeText.Contains("UPDATE") && !tempSPCodeText.Contains("DELETE"))
                    {
                        prefix = string.Empty;
                    }
                }

                if (IsCUD) ModelName = ModelName.Replace("Model", string.Empty);

                ModelName = prefix + ModelName;
                //RequestName = prefix + RequestName;
                //ResponseName = prefix + ResponseName;

                #endregion

                return;
            }
            //var dbOBJECT = UCDBObjectVM.DBObjectModelList.FirstOrDefault(x => x.IsChecked);
            //if (dbOBJECT == null || string.IsNullOrEmpty(dbcon))
            //{
            //    MessageBox.Show("DBConnection 정보와 SP를 선택해주세요");
            //    return;
            //}

            //string spName = dbOBJECT.ObjectName;
            this.IsShowProgressRing = true;
            try
            {

                if (string.IsNullOrEmpty(ModelName))
                {
                    MessageBox.Show("ModelName을 입력하세요.");
                    return;
                }

                if (string.IsNullOrEmpty(RequestName))
                {
                    MessageBox.Show("RequestName을 입력하세요.");
                    return;
                }

                if (string.IsNullOrEmpty(ResponseName))
                {
                    MessageBox.Show("ResponseName을 입력하세요.");
                    return;
                }

                DataSet dsParam = DBObject.GetSPParameters(dbcon, SPName);
                StringBuilder sbModelMainCode = new StringBuilder();
                StringBuilder sbRequestCode = new StringBuilder();
                StringBuilder sbResponseCode = new StringBuilder();
                StringBuilder sbModelCode = new StringBuilder();

                StringBuilder sbServerCode = new StringBuilder();
                IList<string> tempModelNames = new List<string>();


                #region ModelCode Generate - Original

                #region Model
                if (!IsCUD)
                {
                    DataSet dsColumn = GetSPExcuteDataSet(param as string);
                    if (dsColumn == null || dsColumn.Tables.Count == 0) return;

                    #region Comment
                    //for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                    //{
                    //    // uc.AddGridRow(ds.Tables[0].Columns[i].ColumnName,"");
                    //}



                    //System.Data.SqlClient.SqlParameter[] sqlParams = new System.Data.SqlClient.SqlParameter[dsParam.Tables[0].Rows.Count];

                    //for (int i = 0; i < dsParam.Tables[0].Rows.Count; i++)
                    //{
                    //    DataRow dr = dsParam.Tables[0].Rows[i];
                    //    string paramValue = "20051022".Left(int.Parse(dr[2].ToString()));
                    //    sqlParams[i] = new System.Data.SqlClient.SqlParameter(dsParam.Tables[0].Rows[i][0] + "", paramValue);
                    //}
                    ////            +		[0]	{paramName}	object {System.Data.DataColumn}
                    ////+		[1]	{DataType}	object {System.Data.DataColumn}
                    ////+		[2]	{length}	object {System.Data.DataColumn}
                    ////+		[3]	{status}	object {System.Data.DataColumn}
                    ////+		[4]	{isnullable}	object {System.Data.DataColumn}

                    //DataSet dsColumn = DataAccess.GetDataSet2(dbcon, SPName, sqlParams, CommandType.StoredProcedure);
                    #endregion

                    string tempModelName = ModelName;

                    for (int i = 0; i < dsColumn.Tables.Count; i++)
                    {
                        // XxxModel_1
                        if (i > 0) tempModelName = ModelName + "_" + i.ToString();

                        tempModelNames.Add(tempModelName);

                        sbModelCode.AppendLine(" public class " + tempModelName + " : SAFModel");
                        sbModelCode.AppendLine(" {");

                        foreach (DataColumn col in dsColumn.Tables[i].Columns)
                        {
                            string pName = col.ColumnName;
                            string pLocalName = "_" + pName.Left(1).ToLower() + pName.Substring(1);
                            sbModelCode.AppendLine("\t#region " + pName);
                            sbModelCode.AppendLine("\tstring " + pLocalName + ";");
                            sbModelCode.AppendLine("\tpublic string " + pName);

                            if (IsSimple)
                            {
                                sbModelCode.AppendLine("\t{");
                                sbModelCode.AppendLine("\t\tget { return " + pLocalName + "; }");
                                sbModelCode.AppendLine("\t\tset { RaiseAndSetIfChanged(ref " + pLocalName + ", value); }");
                            }
                            else 
                            { 
                                sbModelCode.AppendLine("\t{");
                                sbModelCode.AppendLine("\t\tget{");
                                sbModelCode.AppendLine("\t\t\treturn " + pLocalName + ";");
                                sbModelCode.AppendLine("\t\t}");
                                sbModelCode.AppendLine("\t\tset{");
                                sbModelCode.AppendLine("\t\t\tRaiseAndSetIfChanged(ref " + pLocalName + ", value);");
                                sbModelCode.AppendLine("\t\t}");
                            }
                            sbModelCode.AppendLine("\t}");
                            sbModelCode.AppendLine("\t#endregion");
                            sbModelCode.AppendLine();
                        }

                        sbModelCode.AppendLine();
                        sbModelCode.AppendLine(" }");

                    }

                    sbModelCode.AppendLine();
                }

                #endregion


                #region Request
                // Model Request 
                sbRequestCode.AppendLine(" public class " + RequestName + " : SAF.Model.ISAFReturn<" + ResponseName + ">");
                sbRequestCode.AppendLine(" {");

                foreach (DataRow r in dsParam.Tables[0].Rows)
                {
                    //get { return _IndicatorGb; } set { RaiseAndSetIfChanged(ref _IndicatorGb, value); }
                    string pName = r[0].ToString().Replace("@", "");
                    sbRequestCode.AppendLine("\tpublic string " + pName + " { get; set; }");

                }
                sbRequestCode.AppendLine(" }");
                sbRequestCode.AppendLine();
                #endregion

                #region Response
                // Model Response
                sbResponseCode.AppendLine(" public class " + ResponseName);
                sbResponseCode.AppendLine(" {");

                if (IsCUD)
                {
                    sbResponseCode.AppendLine("\tpublic int ReturnValue { get; set; }");
                }
                else
                {
                    foreach (string modelName in tempModelNames)
                    {
                        sbResponseCode.AppendLine("\tpublic IList<" + modelName + "> " + modelName + "List { get; set; }");
                    }
                }
                sbResponseCode.AppendLine(" }");
                sbResponseCode.AppendLine();
                #endregion

                if (IsSimple) sbRequestCode.Replace("SAF.Model.", string.Empty);
                sbModelMainCode.Append(sbRequestCode);
                sbModelMainCode.Append(sbResponseCode);
                sbModelMainCode.Append(sbModelCode);

                #endregion

                SPExcuteVisible = Visibility.Collapsed;

                GenCode genCode = new GenCode();
                genCode.FacadeNM = SelectedSPInfo.FacadePath;
                this.GenCodeList.Add(genCode);
                genCode.ModelCode = sbModelMainCode.ToString();
                genCode.ServerCode = GetServerCode(dsParam, tempModelNames);
                genCode.UspName = SPName;
                genCode.ModelName = ModelName;
                genCode.RequestName = RequestName;
                genCode.ResponseName = ResponseName;
                genCode.PathMessage = _pathMsg;
                genCode.IsExist = !string.IsNullOrEmpty(_pathMsg);
                genCode.IsNotSaved = true;

                string location = string.Empty;
                string[] tmp = UINameSpace.Split('.');

                //HIS.WinUI.SP.BLO.CM.Com
                //location = "His3." + tmp[2] + ".Model";
                //for (int i = 3; i < tmp.Length; i++)
                //    location += "." + tmp[i];

                location = "His3." + tmp[2].Substring(0, 1) + tmp[2].Substring(1, tmp[2].Length - 1).ToLower() + ".Model";
                for (int i = 3; i < tmp.Length; i++)
                {
                    tmp[i] = tmp[i].Replace("ZZZ", "Etc");

                    if (i == tmp.Length - 1) location += "." + tmp[i];
                    else location += "." + tmp[i].Substring(0, 1) + tmp[i].Substring(1, tmp[i].Length - 1).ToLower();
                }
                genCode.Location = location + "." + RequestName + ".cs";
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.SxGetErrorMessage());
            }
            finally
            {
                this.IsShowProgressRing = false;
            }
        }

        private string GetServerCode(DataSet dsParam, IList<string> models)
        {
            StringBuilder sbServerCode = new StringBuilder();
            if (IsSimple) { sbServerCode.AppendLine(" public class " + ModelName + "Service : SAFService"); }
            else { sbServerCode.AppendLine(" public class " + ModelName + "Service : SAF.Server.SAFService"); }
            sbServerCode.AppendLine(" {");
            sbServerCode.AppendLine("\tpublic override string DefaultConnectionName");
            sbServerCode.AppendLine("\t{");
            sbServerCode.AppendLine("\t\tget");
            sbServerCode.AppendLine("\t\t{");
            sbServerCode.AppendLine("\t\t\treturn \"" + (DBConnectionList.FirstOrDefault(x=>x.Value == DBConnectionString).Key+"").Replace("강남","") +"\";");
            sbServerCode.AppendLine("\t\t}");
            sbServerCode.AppendLine("\t}");
            sbServerCode.AppendLine();
            sbServerCode.AppendLine("\tpublic async Task<" + ModelName + "Response> Any(" + ModelName + "Request request)");
            sbServerCode.AppendLine("\t{");
            sbServerCode.AppendLine("\t\tvar ret = new " + ModelName + "Response();");


            if (!IsCUD)
            {
                if (models.Count > 1)
                {
                    sbServerCode.AppendLine("\t\tvar data = await DataProvider.QueryMultipleAsync( \"" + SPName + "\",");
                }
                else
                {
                    sbServerCode.AppendLine("\t\tvar data = await DataProvider.QueryAsync<" + ModelName + ">( \"" + SPName + "\",");
                }
            }
            else
            {
                sbServerCode.AppendLine("\t\tret.ReturnValue = await DataProvider.ExecuteAsync( \"" + SPName + "\",");
            }

            sbServerCode.AppendLine("\t\tnew");
            sbServerCode.AppendLine("\t\t\t {");
            foreach (DataRow r in dsParam.Tables[0].Rows)
            {
                string paramName = r[0].ToString().Replace("@", "");
                sbServerCode.AppendLine("\t\t\t" + paramName + "= request." + paramName + ",");
            }
            sbServerCode.Remove(sbServerCode.Length - 3, 3);
            sbServerCode.AppendLine();
            sbServerCode.AppendLine("\t\t\t }");
            sbServerCode.AppendLine("\t\t,null, System.Data.CommandType.StoredProcedure);");
            sbServerCode.AppendLine();

            if (!IsCUD)
            {
                if (models.Count == 1)
                {
                    sbServerCode.AppendLine("\t\tret." + ModelName + "List = data;");
                }
                else
                {
                    foreach (string modelName in models)
                    {
                        sbServerCode.AppendLine("\t\tret." + modelName + "List = data.NextResult<" + modelName + ">();");
                    }
                }
            }

            sbServerCode.AppendLine("\t\treturn ret;");
            sbServerCode.AppendLine("\t}");
            sbServerCode.AppendLine(" }");
            return sbServerCode.ToString();
        }
        private DataSet GetSPExcuteDataSet(string excuteSPQuery)
        {
            if (!String.IsNullOrEmpty(excuteSPQuery))
            {
                return DataAccess.GetDataSet2(SelectedSPInfo.DBName, excuteSPQuery.Trim(), CommandType.Text);
            }
            else
            {
                SqlParameter[] sqlparams = new SqlParameter[SelectedSPInfo.SPParamsDT.Rows.Count];
                for (int i = 0; i < SelectedSPInfo.SPParamsDT.Rows.Count; i++)
                {
                    if (SelectedSPInfo.SPParamsDT.Rows[i]["Value"] == null || SelectedSPInfo.SPParamsDT.Rows[i]["Value"].ToString() == "")
                    {
                        MessageBox.Show(SelectedSPInfo.SPParamsDT.Rows[i]["parameter"] + "  parameter 의 Value 값을 지정 하셔야 합니다!");
                        return null;
                    }
                    sqlparams[i] = new SqlParameter(SelectedSPInfo.SPParamsDT.Rows[i]["parameter"] + "", SelectedSPInfo.SPParamsDT.Rows[i]["Value"] + "");
                }

                DataSet ds = null;
                if(SelectedSPInfo.IsQueryText)
                {
                   ds=  DataAccess.GetDataSet2(SelectedSPInfo.DBName, SelectedSPInfo.SPCodeText, sqlparams, CommandType.Text);
                   SelectedSPInfo.SPName = SelectedSPInfo.SPCodeText;
                }
                else
                    ds = DataAccess.GetDataSet2(SelectedSPInfo.DBName, SelectedSPInfo.SPName, sqlparams, CommandType.StoredProcedure);

                return ds;
            }
        }
        #endregion

        public string GetRequestCode()
        {
            StringBuilder sbRequestCode = new StringBuilder();
            // 주석 부분
            sbRequestCode.AppendLine(GetClassComment(false));
            sbRequestCode.AppendLine("using System.Collections.Generic;");
            sbRequestCode.AppendLine("using SAF.Model;");
            sbRequestCode.AppendLine();
            sbRequestCode.AppendLine("namespace " + GetModelLocation());
            sbRequestCode.AppendLine("{");
            sbRequestCode.AppendLine("\tpublic class " + RequestName + " : SAF.Model.ISAFReturn<" + ResponseName + ">");
            sbRequestCode.AppendLine("\t{");

            List<string> propList = new List<string>();
            List<string> reqPropList = new List<string>();
            foreach(var sp in FacadeSPList)
            {
                if (sp.RequestPropertyList == null) continue;
                foreach(string p in sp.RequestPropertyList)
                {
                    int cnt = propList.Count(x => x == p);
                    propList.Add(p);
                    if(IsSeperate)
                    {
                        string pname = p;
                        if(cnt>0)
                            pname = pname + cnt;
                        sbRequestCode.AppendLine(("\t\tpublic string " + pname + " { get; set; }"));
                        reqPropList.Add(pname);
                    }
                    else
                    {
                        if(cnt==0)
                            sbRequestCode.AppendLine(("\t\tpublic string " + p + " { get; set; }"));
                        reqPropList.Add(p);
                    }
                }
            }

            // 생성자 List
            sbRequestCode.AppendLine("\t\tpublic " + RequestName + "()");
            sbRequestCode.AppendLine("\t\t{");
            foreach (var p in reqPropList)
                sbRequestCode.AppendLine("\t\t\t"+p + "=\"\";");
            sbRequestCode.AppendLine("\t\t}");

            sbRequestCode.AppendLine("\t}");

            return sbRequestCode.ToString();
       } 

        public string GetResponseCode()
        {
            StringBuilder sbResponseCode = new StringBuilder();
            sbResponseCode.AppendLine("\tpublic class " + ResponseName);
            sbResponseCode.AppendLine("\t{");

            if (IsCUD)
            {
                sbResponseCode.AppendLine("\t\tpublic int ReturnValue { get; set; }");
            }
            else
            {
                foreach (var sp in FacadeSPList)
                {
                    if (sp.ModelList == null) continue;
                    foreach (var model in sp.ModelList)
                    {
                        if (sp.IsSaveSP)
                        {
                            sbResponseCode.AppendLine("\t\tpublic int " + sp.SPName + "ReturnValue { get; set; }");
                        }
                        else
                        {
                            sbResponseCode.AppendLine("\t\tpublic IList<" + model.ModelName + "> " + model.ModelName + "List { get; set; }");
                        }
                    }

                    if(sp.ModelList.Count == 0 && sp.IsSaveSP)
                        sbResponseCode.AppendLine("\t\tpublic int ReturnValue { get; set; }");
                }
            }
            sbResponseCode.AppendLine("\t}");
            sbResponseCode.AppendLine();
           
            return sbResponseCode.ToString();
        }

        public string GetServerMethodCode()
        {
            StringBuilder sbServerCode = new StringBuilder();
            sbServerCode.AppendLine( GetClassComment(true) );
            sbServerCode.AppendLine("using System.Threading.Tasks;");
            sbServerCode.AppendLine("using SAF.Server;");
            sbServerCode.AppendLine("using " + GetModelLocation() +";");
            sbServerCode.AppendLine();
            sbServerCode.AppendLine("namespace "+ GetModelLocation().Replace("Model","Server"));
            sbServerCode.AppendLine("{");
            sbServerCode.AppendLine("\tclass " + RequestName.Replace("Request", "Service : SAFService"));
            sbServerCode.AppendLine("\t{");
            sbServerCode.AppendLine("\t\tpublic override string DefaultConnectionName");
            sbServerCode.AppendLine("\t\t{");
            sbServerCode.AppendLine("\t\t\tget");
            sbServerCode.AppendLine("\t\t\t{");
            sbServerCode.AppendLine("\t\t\t\treturn \"" + DBConnectionList.FirstOrDefault(x => x.Value == SelectedSPInfo.DBName).Key + "\";");
            sbServerCode.AppendLine("\t\t\t}");
            sbServerCode.AppendLine("\t\t}");
            sbServerCode.AppendLine();

            sbServerCode.AppendLine("\t\tpublic async Task<" + ResponseName+"> Any(" + RequestName + " request)");
            sbServerCode.AppendLine("\t\t{");
            sbServerCode.AppendLine("\t\t\tvar ret = new " + ResponseName+"();");

            bool useTran = false;
            int openTran = 0;
            foreach (var sp in FacadeSPList)
            {
                if (sp.ModelList == null) continue;

                // Transaction 처리가 필요한지 여부
                DataRow[] drs = this.CallTreeDT.Select("FacadeMethod='"+ sp.FacadeMethodName+"' and SP='"+ sp.SPName+"'");
                if(drs.Length>0)
                {
                    var basePath = ViewModelLocator.Current.CallTreeVM.TargetFile.LeftBySearch("WinUI");
                    string[] bizs = (drs[0]["Biz"] + "").Split("\\");
                    string[] ns = bizs[0].Split('.');
                    string path = string.Format(@"COM\{0}\{1}\{2}\{3}\{4}",
                        ns[1],
                        ns[2],
                        ns[3],
                        bizs[0],
                        bizs[1]);
                    string filePath  = Path.Combine(basePath, path);
                    if (File.Exists( filePath ) )
                    {
                        using(StreamReader r = File.OpenText(filePath))
                        {
                            string line = string.Empty;
                            while( (line = r.ReadLine() ) != null)
                            {
                                if(line.Contains("class") && line.Contains(bizs[1].Replace(".cs","")) )
                                {
                                    string[] baseTypes = line.Split(':');
                                    if (baseTypes[1].Contains("BizTxBase"))
                                    {
                                        useTran = true;
                                        openTran++;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                if (useTran && openTran==1)
                {
                    sbServerCode.AppendLine("\t\t\tusing (var tran = this.CreateUnitOfWork())");
                    sbServerCode.AppendLine("\t\t\t{");
                }



                if (!sp.IsSaveSP)
                {
                    if (sp.ModelList.Count > 1)
                    {
                        sbServerCode.AppendLine("\t\t\t\tvar data = await DataProvider.QueryMultipleAsync( \"" + sp.SPName + "\",");
                    }
                    else
                    {
                        sbServerCode.AppendLine("\t\t\t\tvar data = await DataProvider.QueryAsync<" + sp.ModelList[0].ModelName + ">( \"" + sp.SPName + "\",");
                    }
                }
                else
                {
                    sbServerCode.AppendLine("\t\t\t\tret.ReturnValue = await DataProvider.ExecuteAsync( \"" + sp.SPName + "\",");
                }

                if (sp.SPParamsDT.Rows.Count > 0)
                {
                    sbServerCode.AppendLine("\t\t\t\t new");
                    sbServerCode.AppendLine("\t\t\t\t {");
                    foreach (DataRow r in sp.SPParamsDT.Rows)
                    {
                        string paramName = r[0].ToString().Replace("@", "");
                        sbServerCode.AppendLine("\t\t\t\t\t " + paramName + "= request." + paramName + ",");
                    }
                    sbServerCode.Remove(sbServerCode.Length - 3, 3);
                    sbServerCode.AppendLine("\t\t\t\t }");
                }
                else
                {
                    sbServerCode.AppendLine("\t\t\t\t null");
                }


                if(useTran)
                    sbServerCode.Append("\t\t\t\t,null, System.Data.CommandType.StoredProcedure,null,tran);");
                else
                    sbServerCode.Append("\t\t\t\t,null, System.Data.CommandType.StoredProcedure);");
                sbServerCode.AppendLine();

                if (!IsCUD)
                {
                    if (sp.ModelList.Count == 1)
                    {
                        sbServerCode.AppendLine("\t\t\t\tret." + sp.ModelList[0].ModelName + "List = data;");
                    }
                    else
                    {
                        foreach (var model in sp.ModelList)
                        {
                            sbServerCode.AppendLine("\t\t\t\tif(data.HasMoreResults)");
                            sbServerCode.AppendLine("\t\t\t\t\tret." + model.ModelName + "List = data.NextResult<" + model.ModelName + ">();");
                        }
                    }
                }
            }

            if (useTran)
            {
                sbServerCode.AppendLine("\t\t\t\ttran.Commit();");
                sbServerCode.AppendLine("\t\t\t}");
            }

            sbServerCode.AppendLine("\t\t\treturn ret;");
            sbServerCode.AppendLine("\t\t}");
            sbServerCode.AppendLine("\t}");
            sbServerCode.AppendLine("}");
            return sbServerCode.ToString();
        }

        private string GetClassComment(bool isService)
        {
            StringBuilder sbServerCode = new StringBuilder();
            sbServerCode.AppendLine("// ================================================");
            if(isService)
                sbServerCode.AppendLine("// " + RequestName.Replace("Request", "Service"));
            else
                sbServerCode.AppendLine("// " + RequestName);
            sbServerCode.AppendLine("//");
            sbServerCode.AppendLine("// [AS-IS] " + UINameSpace);
            sbServerCode.AppendLine("//         " + SelectedSPInfo.FacadePath);
            foreach (SPInfo sp in FacadeSPList)
                sbServerCode.AppendLine("//         " + sp.SPName);
            sbServerCode.AppendLine("// -----------------------------------------------");
            sbServerCode.AppendLine("// " + DateTime.Today.Date.ToShortDateString() + " 작성자(" + Environment.UserName + ")");
            sbServerCode.AppendLine("// ===============================================");
            return sbServerCode.ToString();
        }


        #region ExcuteSPCommand
        ICommand excuteSPCommand;
        public ICommand ExcuteSPCommand
        {
            get
            {
                if (excuteSPCommand == null)
                    excuteSPCommand = CreateCommand(OnExcuteSPCommand);
                return excuteSPCommand;
            }
        }

        private void OnExcuteSPCommand(object param)
        {
          
        }
        #endregion   

        #region ScmdParamClose
        ICommand _ScmdParamClose;
        public ICommand ScmdParamClose
        {
            get
            {
                if (_ScmdParamClose == null)
                    _ScmdParamClose = CreateCommand(ExecScmdParamClose);
                return _ScmdParamClose;
            }
        }

        internal void ExecScmdParamClose(object param)
        {
            this.SPExcuteVisible = Visibility.Collapsed;
        } 
        #endregion


        /// <summary>
        ///  Reqeust 파라메터를 Merge 할지 Serperate 할지 결정 한다.
        /// </summary>
        public bool IsSeperate
        { get; set; }

        #region ScmdRunSPCode
        ICommand _ScmdRunSPCode;
        /// <summary>
        ///  sp 를 통해 model, request 등을 generate 할 준비를 한다.
        /// </summary>
        public ICommand ScmdRunSPCode
        {
            get
            {
                if (_ScmdRunSPCode == null)
                    _ScmdRunSPCode = CreateCommand(ExecScmdRunSPCode);
                return _ScmdRunSPCode;
            }
        }

        internal void ExecScmdRunSPCode(object param)
        {
            if (SelectedSPInfo == null) return;
            string excuteSPCode = param as string;

            // sp가 아닌 경우에는 
            // query string 인 경우
            if (SelectedSPInfo.RequestPropertyList == null)
            {
                ObservableCollection<string> paramList = new ObservableCollection<string>();
                // 1.request 정보
                DataSet dsParam = DBObject.GetSPParameters(SelectedSPInfo.DbConnectionString, SelectedSPInfo.SPName);
                foreach (DataRow r in dsParam.Tables[0].Rows)
                {
                    string pName = r[0].ToString().Replace("@", "");
                    if (paramList.Count(x => x == pName) == 0)
                        paramList.Add(pName);
                    //sbRequestCode.AppendLine("\tpublic string " + pName + " { get; set; }");
                }
                SelectedSPInfo.RequestPropertyList = paramList;
            }




            if (string.IsNullOrEmpty(excuteSPCode) && SelectedSPInfo.SPParamsDT == null)
            {
                    return;
            }
            // cm 에 있는 놈이면. autogen거를 뒤져서 가져온다.
            if (SelectedSPInfo.DBName.Contains("Catalog=HISZ") && File.Exists("AutoGeneratedService.cs"))
            {
                using (StreamReader s = File.OpenText("AutoGeneratedService.cs"))
                {
                    string line = string.Empty;
                    while ((line = s.ReadLine()) != null)
                    {
                        if (line.Contains(SelectedSPInfo.SPName))
                        {
                            s.ReadLine(); s.ReadLine(); s.ReadLine(); s.ReadLine();
                            line = s.ReadLine();
                            if (line.Contains("Any"))
                            {
                                string requestName = line.Split('(')[1].Split(' ')[0];

                                StringBuilder sbmessage = new StringBuilder();
                                sbmessage.AppendLine("....................................................................");
                                sbmessage.AppendLine(SelectedSPInfo.SPName + " 은 공통에서 만든 SP임..................");
                                //sbmessage.AppendLine(ds2.Tables[0].Rows[0]["SPName"] + " 이 이미 " + ds2.Tables[0].Rows[0]["ModelName"] + " 으로 만들어졌습니다");
                                sbmessage.AppendLine(requestName);
                                sbmessage.AppendLine("================================================================================");
                                sbmessage.AppendLine("\t"+requestName + " 이름에 오른쪽 마우스 클릭 => Resolve => using 문을 추가 하세요");
                                sbmessage.AppendLine("------------------------------------------------------------------------------------------");
                                sbmessage.AppendLine("사용예)복사 -붙여넣기 한 다음 필요한 부분 수정");
                                sbmessage.AppendLine("------------------------------------------------------------------------------------------");
                                sbmessage.AppendLine("var req = new " + requestName + "();");
                                sbmessage.AppendLine("req.프로퍼티 = \"넘길 값\";");
                                sbmessage.AppendLine("var data = Context.Proxy.Post<" + requestName.Replace("Request", "Response") + ">(req);");
                                sbmessage.AppendLine("내가만든프로퍼티 =  new List<" + requestName.Replace("Request", "Model") + ">(data.ResultData);");
                                sbmessage.AppendLine();
                                sbmessage.AppendLine("================================================================================");

                                sbmessage.AppendLine("================================================================================");
                                sbmessage.AppendLine("새로 만드실려면 OK 버튼을 클릭 하세요");


                                _pathMsg = sbmessage.ToString();


                                PathVisibility = Visibility.Visible;

                                //MessageBoxResult msgResult = MessageBox.Show(SelectedSPInfo.SPName + " 은 공통에서 만든 SP 입니다.\r\n다시 생성 하지 마시고 가능한 모델 위치 확인을 통해 사용하세요." +
                                //    Environment.NewLine +  " == 다시 생성하시겠습니까? == ", "Requset 위치 확인", MessageBoxButton.YesNo, MessageBoxImage.Information);

                                //OpenMessage(_pathMsg, 700, 450);
                                if (!OpenConfirm(_pathMsg, 700,450))
                                {
                                    GenCode gencode = new GenCode();
                                    gencode.ModelCode = _pathMsg;
                                    gencode.RequestName = SelectedSPInfo.FacadePath;
                                    this.GenCodeList.Add(gencode);
                                    PathVisibility = Visibility.Collapsed;
                                    return;
                                }
                            }
                            break;
                        }
                    }

                }
            }
            else
            {
                if (CheckHasRequestcode()) return;
            }
            RunSP(excuteSPCode);
        }

        public void RunSP(string excuteSPCode)
        {
            List<Model> modelList = new List<Model>();
            // Model 정보
            if (!SelectedSPInfo.IsSaveSP)
            {
                DataSet dsColumn = GetSPExcuteDataSet(excuteSPCode);
                if (dsColumn == null || dsColumn.Tables.Count == 0)
                {
                    MessageBox.Show("결과값을 조회 할 수 없습니다. 파라메터 정보를 다시 지정해보세요.");
                    return;
                }

                string[] spNameSplit = SelectedSPInfo.SPName.Split('_');
                string modelName = string.Empty;
                modelName = spNameSplit[spNameSplit.Length - 1].RegexReplace("(Select|Save|Insert|Update)", "");
                if (modelName.StartsWith("By"))
                {
                    modelName = spNameSplit[spNameSplit.Length - 2].RegexReplace("(Select|Save|Insert|Update)", "") + modelName;
                }

                for (int i = 0; i < dsColumn.Tables.Count; i++)
                {
                    Model model = new Model();

                    if (i > 0)
                        model.ModelName = modelName + "_" + i + "Model";
                    else
                        model.ModelName = modelName + "Model";
                    List<string> modelPropertyList = new List<string>();
                    foreach (DataColumn col in dsColumn.Tables[i].Columns)
                    {
                        modelPropertyList.Add(col.ColumnName);
                    }
                    model.ModelPropertyList = modelPropertyList;
                    modelList.Add(model);
                }

            }
            SelectedSPInfo.ModelList = modelList;

            var saveVm = new SaveTobeInfoVM();
            saveVm.RequestName = RequestName;
            saveVm.ResponseName = ResponseName;
            saveVm.ModelNameList = modelList;
            saveVm.ServiceName = ServiceName;
            saveVm.Location = GetModelLocation() + "." + RequestName + ".cs";
            saveVm.SPName = SelectedSPInfo.SPName;
            OpenDialog(typeof(SaveTobeInfoView), saveVm, 500, 500, true);
            RequestName = saveVm.RequestName;
            ResponseName = saveVm.ResponseName;
            ServiceName = saveVm.ServiceName;
            Location = saveVm.Location;

            SelectedSPInfo.ModelList = modelList;
            if (FacadeSPList.Count == 1)
            {
                ExecScmdGenerateCode(null);
            }
            SPExcuteVisible = Visibility.Collapsed;
        }

        public bool CheckHasRequestcode()
        {
            var methodnm = SelectedSPInfo.FacadePath.RightBySearch(".");
            var clsnm = SelectedSPInfo.FacadePath.LeftBySearch("." + methodnm).RightBySearch(".");
            var nsname = SelectedSPInfo.FacadePath.LeftBySearch("." + clsnm);
            if (SelectedSPInfo.SPName.Contains("USP_CM_COM_CD_ComCDDA_SelectComCD"))
            {



                if (!OpenConfirm(_pathMsg, 700, 450))
                {
                    GenCode gencode = new GenCode();
                    gencode.ModelCode = _pathMsg;
                    gencode.RequestName = SelectedSPInfo.FacadePath;
                    this.GenCodeList.Add(gencode);
                    PathVisibility = Visibility.Collapsed;
                    return true;
                }
            }
            else
            {

                SqlParameter[] sqlparams = new SqlParameter[4];
                sqlparams[0] = new SqlParameter("@facadeNMSpace", nsname);
                sqlparams[1] = new SqlParameter("@facadeCLSNM", clsnm);
                sqlparams[2] = new SqlParameter("@facadeMethodNM", methodnm);
                sqlparams[3] = new SqlParameter("@spname", SelectedSPInfo.SPName);

                DataSet ds2 = DataAccess.GetDataSet2(AppManager.DataConnectionString, "[USP_SELECT_SP_MODEL]", sqlparams, CommandType.StoredProcedure);
                if ((ds2.Tables.Count > 0 && ds2.Tables[0].Rows.Count > 0)
                    || (ds2.Tables.Count > 1 && ds2.Tables[1].Rows.Count > 0))
                {
                    var vm = new HasRequestInfoVM() { SpInfomation = SelectedSPInfo, RequestInfoDataSet = ds2 };
                    if (!OpenDialog(typeof(HasRequstView), vm, 850, 550, "Reqeust정보보기", null, true))
                    {
                        GenCode gencode = new GenCode();
                        gencode.ModelCode = vm.RequestCode;
                        gencode.FacadeMethod = SelectedSPInfo.FacadeMethodName;
                        gencode.RequestName = SelectedSPInfo.FacadePath;
                        this.GenCodeList.Add(gencode);
                        PathVisibility = Visibility.Collapsed;
                        return true;
                    }
                }
                else
                {
                    PathVisibility = Visibility.Collapsed;
                    _pathMsg = string.Empty;
                }
            }
            return false;
        }


        #endregion

        #region ScmdSPGen
        ICommand _ScmdSPGen;
        /// <summary>
        ///  sp 를 실행하기 위한 준비 및 실행 창을 open
        /// </summary>
        public ICommand ScmdSPGen
        {
            get
            {
                if (_ScmdSPGen == null)
                    _ScmdSPGen = CreateCommand(ExecScmdSPGen);
                return _ScmdSPGen;
            }
        }

        internal void ExecScmdSPGen(object param)
        {
            try
            {
                var spinfo = param as SPInfo;
                if (spinfo == null) return;
                if (string.IsNullOrEmpty(spinfo.DBName))
                {
                    MessageBox.Show("DBName을 선택하세요");
                    return;
                }
                // spcode 를 가져오지 않은 경우
                //if (string.IsNullOrEmpty(spinfo.SPCodeText))
                //{
                //    string query = @"SELECT definition FROM sys.sql_modules WHERE object_id = (OBJECT_ID(N'[" + spinfo.SPName + "]'));";
                //    DataSet ds = SC.WPF.Tools.CodeHelper.DataAccess.GetDataSet2(spInfo.DbConnectionString, query, CommandType.Text);
                //    if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                //        spinfo.SPCodeText = (ds.Tables[0].Rows[0][0] + "").Trim();
                //}

                if (!SetSPInfo(spinfo)) return;
                
                this.SelectedSPInfo = spinfo;
                
            }
            catch(Exception ex) {
                MessageBox.Show(ex.Message + "\r\n");
            }
            finally
            {
                SPExcuteVisible = Visibility.Visible;
            }
            
        } 

        public bool SetSPInfo(SPInfo spinfo)
        {
            if (spinfo.IsQueryText)
            {
                spinfo.SPCodeText = spinfo.SPName;


                DataTable dt = new DataTable();
                dt.Columns.Add("parameter");
                dt.Columns.Add("Value");

                ObservableCollection<string> paramList = new ObservableCollection<string>();
                foreach (var m in Regex.Matches(spinfo.SPCodeText, @"@[\w]+"))
                {
                    DataRow dr = dt.NewRow();
                    dr[0] = m;
                    dr[1] = "0";
                    dt.Rows.Add(dr);

                    string pName = m.ToString().Replace("@", "");
                    if (paramList.Count(x => x == pName) == 0)
                        paramList.Add(pName);
                }

                bool isCUD = (spinfo.SPCodeText.Contains("INSERT") || spinfo.SPCodeText.Contains("UPDATE") ||
                        spinfo.SPCodeText.Contains("DELETE") || spinfo.SPCodeText.Contains("SAVE"));

                spinfo.IsSaveSP = isCUD;
                spinfo.RequestPropertyList = paramList;
                spinfo.SPParamsDT = dt;
                return true;
            }

            //if (string.IsNullOrEmpty(spinfo.SPCodeText))
            //{
                DataTable dt2 = DBObject.GetSPParames(spinfo.DbConnectionString, spinfo.SPName);
                dt2.Columns.Add("Value");
                foreach (DataRow dr in dt2.Rows)
                {
                    int len = int.Parse(dr["length"].ToString());
                    if (len == 8 && dr["parameter"].ToString().Contains("Ymd"))
                    {
                        dr["value"] = "20080909";
                    }
                    else
                    {
                        dr["value"] = "1".PadRight(len - 1, '0');
                    }
                }


                string query = @"SELECT definition FROM sys.sql_modules WHERE object_id = (OBJECT_ID(N'[" + spinfo.SPName + "]'));";
                DataSet ds = SC.WPF.Tools.CodeHelper.DataAccess.GetDataSet2(spinfo.DbConnectionString, query, CommandType.Text);
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    spinfo.SPCodeText = (ds.Tables[0].Rows[0][0] + "").Trim();
                }
                else
                {
                    this.SelectedSPInfo = spinfo;
                    MessageBox.Show("선택된 DB에서 SP를 찾을 수 없습니다. 다른 DB를 선택한후 가져오기를 해보세요.");
                    return false;
                }

                spinfo.SPParamsDT = dt2;

                #region CUD 체크
                string spUperName = spinfo.SPName.ToUpper();
                string tempSPCodeText = (spinfo.SPCodeText + "").ToUpper();

                string prefix = string.Empty;

                bool isCUD2 = (spUperName.Contains("INSERT") || spUperName.Contains("UPDATE") ||
                         spUperName.Contains("DELETE") || spUperName.Contains("SAVE"))
                        &&
                        (tempSPCodeText.Contains("INSERT") || tempSPCodeText.Contains("UPDATE") ||
                         tempSPCodeText.Contains("DELETE"));

                spinfo.IsSaveSP = isCUD2;

                #endregion
            //}
            return true;
        }
        #endregion

        #region ScmdGenerateCode
        ICommand _ScmdGenerateCode;
        public ICommand ScmdGenerateCode
        {
            get
            {
                if (_ScmdGenerateCode == null)
                    _ScmdGenerateCode = CreateCommand(ExecScmdGenerateCode);
                return _ScmdGenerateCode;
            }
        }

        internal void ExecScmdGenerateCode(object param)
        {
            if (FacadeSPList == null || FacadeSPList.Count() == 0 || SelectedSPInfo == null) return;

            GenCode gencode = new GenCode();
            gencode.FacadeNM = SelectedSPInfo.FacadePath;
            this.GenCodeList.Add(gencode);

            string modelCode = GetRequestCode();
            modelCode += GetResponseCode();
            foreach(var sp in FacadeSPList)
            {
                modelCode += sp.GetModelCode();
            }
            modelCode += "}";

            gencode.ModelCode = modelCode;
            gencode.ServerCode = GetServerMethodCode();
            gencode.RequestName = RequestName;
            gencode.ResponseName = ResponseName;
            gencode.SPInfoList = FacadeSPList;
            gencode.Location = Location; 
        }

        private string GetModelLocation()
        {
            string location = string.Empty;
            string[] tmp = UINameSpace.Split('.');

            //HIS.WinUI.SP.BLO.CM.Com
            //location = "His3." + tmp[2] + ".Model";
            //for (int i = 3; i < tmp.Length; i++)
            //    location += "." + tmp[i];

            location = "His3." + tmp[2].Substring(0, 1) + tmp[2].Substring(1, tmp[2].Length - 1).ToLower() + ".Model";
            for (int i = 3; i < tmp.Length-1; i++)
            {
                tmp[i] = tmp[i].Replace("ZZZ", "Etc");

                location += i == tmp.Length - 2
                    ? "." + tmp[i]
                    : "." + tmp[i].ToCamel();
            }
            return location;
        } 
        #endregion

        #region ScmdRemoveSP
        ICommand _ScmdRemoveSP;
        public ICommand ScmdRemoveSP
        {
            get
            {
                if (_ScmdRemoveSP == null)
                    _ScmdRemoveSP = CreateCommand(ExecScmdRemoveSP);
                return _ScmdRemoveSP;
            }
        }

        internal void ExecScmdRemoveSP(object param)
        {
            SPInfo spinfo = param as SPInfo;
            if (spinfo == null)
                return;
            this.FacadeSPList.Remove(spinfo);
        } 
        #endregion

        #region UINameSpace
        string _uiNameSpace;
        public string UINameSpace
        {
            get
            {
                return _uiNameSpace;
            }
            set
            {
                _uiNameSpace = value;
                OnPropertyChanged("UINameSpace");
            }
        } 
        #endregion


        #region Model 위치를 확인 하고 저장 합니다.
        ICommand _scmdSaveModel;
        public ICommand ScmdSaveModel
        {
            get
            {
                if (_scmdSaveModel == null)
                    _scmdSaveModel = CreateCommand(ExecscmdSaveModel);
                return _scmdSaveModel;
            }
        }

        internal void ExecscmdSaveModel(object param)
        {
             var genCode = param as GenCode;
            if(genCode== null)return;
            //OpenDialog(typeof(SaveTobeInfoView), new SaveTobeInfoVM() { GenCodeProperty = genCode  }, 600, 450, "SaveCode");




            foreach (var sp in genCode.SPInfoList)
            {
                SqlParameter[] sqlparams = new SqlParameter[9];
                sqlparams[0] = new SqlParameter("@SPName", sp.SPName);
                string modelName = string.Empty;
                if (sp.ModelList == null) continue;

                foreach (var m in sp.ModelList)
                    modelName += m.ModelName + (", ");
                sqlparams[1] = new SqlParameter("@ModelName", modelName.Trim(new char[] { ',', ' ' }));
                sqlparams[2] = new SqlParameter("@RequestName", genCode.RequestName);
                sqlparams[3] = new SqlParameter("@ResponseName", genCode.ResponseName);
                sqlparams[4] = new SqlParameter("@Location", genCode.Location);
                var methodnm = sp.FacadePath.RightBySearch(".");
                var clsnm = sp.FacadePath.LeftBySearch("." + methodnm).RightBySearch(".");
                var nsname = sp.FacadePath.LeftBySearch("." + clsnm);

                sqlparams[5] = new SqlParameter("@facadeNMSPace", nsname);
                sqlparams[6] = new SqlParameter("@facadeCLSNM", clsnm);
                sqlparams[7] = new SqlParameter("@facadeMethodNM", methodnm);
                sqlparams[8] = new SqlParameter("@UserName", WindowsIdentity.GetCurrent().Name);

                DataAccess.GetDataSet2(AppManager.DataConnectionString, @"[USP_SAVE_SP_MODEL]", sqlparams, CommandType.StoredProcedure);
            }
            MessageBox.Show("저장완료");

            genCode.IsNotSaved = false;
        } 
        #endregion

        public string GetDBConnectionString(string dbName)
        {
            string dbCon = string.Empty;

    //<add name="HOOH" connectionString="Data Source=10.10.11.20;Initial Catalog=HOOHTESTDB;User Id=dev_user;Password=password1!;MultipleActiveResultSets=True;" providerName="System.Data.SqlClient" />
    //<add name="CDR" connectionString="Data Source=10.10.11.20;Initial Catalog=HISZ;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;" providerName="System.Data.SqlClient" />
    //<add name="CDR30" connectionString="Data Source=10.10.11.20;Initial Catalog=HIS_TEST;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;" providerName="System.Data.SqlClient" />
    //<add name="CDRS" connectionString="Data Source=10.10.11.20;Initial Catalog=CDR;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;" providerName="System.Data.SqlClient" />
    //<add name="CM" connectionString="Data Source=10.10.11.20;Initial Catalog=HISZ;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;" providerName="System.Data.SqlClient" />
    //<add name="MR" connectionString="Data Source=10.10.11.20;Initial Catalog=HISE;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;" providerName="System.Data.SqlClient" />
    //<add name="HP" connectionString="Data Source=10.10.11.20;Initial Catalog=HISH;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;" providerName="System.Data.SqlClient" />
    //<add name="MD" connectionString="Data Source=10.10.11.20;Initial Catalog=HISM;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;" providerName="System.Data.SqlClient" />
    //<add name="SP" connectionString="Data Source=10.10.11.20;Initial Catalog=HISS;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;" providerName="System.Data.SqlClient" />
    //<add name="TEST" connectionString="Data Source=10.10.11.20;Initial Catalog=HIS_TEST;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;" providerName="System.Data.SqlClient" />
    //<add name="BizTalk" connectionString="Data Source=10.10.11.20;Initial Catalog=Yumc_EMR;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;" providerName="System.Data.SqlClient" />
    //<add name="Cache" connectionString="his@10.26.10.151:6379;" />
    //<add name="HISRS" connectionString="Data Source=10.10.11.20;Initial Catalog=HISRS;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;" providerName="System.Data.SqlClient" />
    //<add name="HISR" connectionString="Data Source=10.10.11.20;Initial Catalog=HISR;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;" providerName="System.Data.SqlClient" />
    //<!-- 적정성 평가 관련 2014.07.01 추가-->
    //<add name="CDWE" connectionString="Data Source=10.10.11.20;Initial Catalog=HISE;User Id=dev_user;Password=password1!;MultipleActiveResultSets=True;Connection Timeout=120" providerName="System.Data.SQLClient" />


            if (dbName == "HP")
            {
                dbCon = "Data Source=10.10.11.20;Initial Catalog=HISH;User ID=dev_user;Password=password1!;";
            }
            else if (dbName == "SP")
            {
                dbCon = "Data Source=10.10.11.20;Initial Catalog=HISS;User ID=dev_user;Password=password1!;";
            }
            else if (dbName == "CDWE")
            {
                dbCon = "Data Source=10.10.11.20;Initial Catalog=HISE;User ID=dev_user;Password=password1!;";
            }
            else if (dbName == "CM")
            {
                dbCon = "Data Source=10.10.11.20;Initial Catalog=HISZ;User ID=dev_user;Password=password1!;";
            }
            else if (dbName == "MD")
            {
                dbCon = "Data Source=10.10.11.20;Initial Catalog=HISM;User ID=dev_user;Password=password1!;";
            }
            else if (dbName == "HISR")
            {
                dbCon = "Data Source=10.10.11.20;Initial Catalog=HISR;User ID=dev_user;Password=password1!;";
            }
            else if (dbName == "HISU")
            {
                dbCon = "Data Source=10.10.11.20;Initial Catalog=HISU;User ID=dev_user;Password=password1!;";
            }
            else if (dbName == "MR")
            {
                dbCon = "Data Source=10.10.11.20;Initial Catalog=HISE;User ID=dev_user;Password=password1!;";
            }
            else if (dbName == "강남HP")
            {
                dbCon = @"Data Source=172.31.7.49\DEV08;Initial Catalog=HISH;User ID=his_user;Password=cns0909;";
            }
            else if (dbName == "강남SP")
            {
                dbCon = @"Data Source=172.31.7.49\DEV08;Initial Catalog=HISS;User ID=his_user;Password=cns0909;";
            }
            else //if (dbName.Equals("HISO"))
            {
                dbCon = string.Format("Data Source=10.10.11.20;Initial Catalog={0};User ID=dev_user;Password=password1!;", dbName);
            }
            return dbCon;
        }

        public CallTreeVM.CallTreeItem SelectedCallTreeItem { get; set; }
    }

    /// <summary>
    /// sp Excute 시 필요한 parameter 정보
    /// </summary>
    public class SPParam
    {
        public string ParamName
        { get; set; }

        public string ParamValue
        {
            get;
            set;
        }
    }
    public class SPInfo : PropertyNotifier
    {

        public bool IsQueryText { 
            get
            {
              if (!string.IsNullOrEmpty(SPName) && (!SPName.StartsWith("USP") && !SPName.StartsWith("UPS") && !string.IsNullOrEmpty(SPCodeText)) )
                        return true;
              
                return false;
            }
        }
        public string FacadePath { get; set; }
        public string FacadeMethodName { get; set; }

        string _dbname;
        public string DBName
        {
            get {
                if(string.IsNullOrEmpty(_dbname))
                {
                    string[] spNameSplit = SPName.Split('_');
                    if (spNameSplit.Length > 2)
                    {
                        _dbname = spNameSplit[1];
                    }
                }
                _dbname = _dbname.Replace("강남", "");
                return _dbname; 
            }
            set { _dbname = value; }
        }

        public string DbConnectionString { get; set; }

        public string SPName { get; set; }
        public bool IsSaveSP { get; set; }
        public string SPCodeText { get; set; }

        public DataTable SPParamsDT { get; set; }

        public string BizClassPath { get; set; }

        #region RequestPropertyList
        private ObservableCollection<string> _RequestPropertyLis;
        public ObservableCollection<string> RequestPropertyList
        {
            get { return _RequestPropertyLis; }
            set
            {
                _RequestPropertyLis = value;
                OnPropertyChanged("RequestPropertyList");
            }
        } 
        #endregion
        public List<Model> ModelList { get; set; }
        public List<string> ServiceCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetModelCode()
        {
            if (ModelList == null) return null;
            StringBuilder sbModelCode = new StringBuilder();
            foreach (var m in ModelList)
            {
                sbModelCode.AppendLine("///<summary>");
                sbModelCode.AppendLine("///" + SPName +" 에서 생성");
                sbModelCode.AppendLine("///</summary>");
                sbModelCode.AppendLine(" public class " + m.ModelName + " : SAFModel");
                sbModelCode.AppendLine(" {");

                foreach (string pName in m.ModelPropertyList)
                {
                    string pLocalName = "_" + pName.Left(1).ToLower() + pName.Substring(1);
                    sbModelCode.AppendLine("\t#region " + pName);
                    sbModelCode.AppendLine("\tstring " + pLocalName + ";");
                    sbModelCode.AppendLine("\tpublic string " + pName);

                    //if (IsSimple)
                    //{
                        sbModelCode.AppendLine("\t{");
                        sbModelCode.AppendLine("\t\tget { return " + pLocalName + "; }");
                        sbModelCode.AppendLine("\t\tset { RaiseAndSetIfChanged(ref " + pLocalName + ", value); }");
                    //}
                    //else
                    //{
                    //    sbModelCode.AppendLine("\t{");
                    //    sbModelCode.AppendLine("\t\tget{");
                    //    sbModelCode.AppendLine("\t\t\treturn " + pLocalName + ";");
                    //    sbModelCode.AppendLine("\t\t}");
                    //    sbModelCode.AppendLine("\t\tset{");
                    //    sbModelCode.AppendLine("\t\t\tRaiseAndSetIfChanged(ref " + pLocalName + ", value);");
                    //    sbModelCode.AppendLine("\t\t}");
                    //}
                    sbModelCode.AppendLine("\t}");
                    sbModelCode.AppendLine("\t#endregion");
                    sbModelCode.AppendLine();
                }
                sbModelCode.AppendLine(" }");
            }
            return sbModelCode.ToString();
        }        
    }

    public class Model
    {
        public string ModelName { get; set; }
        public List<string> ModelPropertyList{get;set;}

        public string SPName { get; set; }
    }

    public class GenCode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<SPInfo> SPInfoList
        {
            get;
            set;
        }

        public string FacadeNM { get; set; }


        #region FacadeMethod
        string _facademethod = string.Empty;
        public string FacadeMethod
        {
            get
            {
                if (string.IsNullOrEmpty(_facademethod))
                    return SPInfoList[0].FacadeMethodName;
                else
                    return _facademethod;
            }
            set
            {
                _facademethod = value;
            }
        } 
        #endregion
        public string ModelName { get; set; }
        public string UspName { get; set; }

        #region ModelCode
        private string _ModelCode;
        public string ModelCode
        {
            get { return _ModelCode; }
            set
            {
                _ModelCode = value;
                OnPropertyChanged("ModelCode");
            }
        } 
        #endregion
        //public string Location { get; set; }
        #region Location
        string _Location;
        public string Location
        {
            get { return _Location; }
            set
            {
                _Location = value;
                OnPropertyChanged("Location");
            }
        } 
        #endregion

        #region ServerCode

        private string _ServerCode;
        ///
        public string ServerCode
        {
            get
            {
                return _ServerCode;
            }
            set
            {
                _ServerCode = value;
                OnPropertyChanged("ServerCode");
            }
        }

        #endregion
        public string RequestName { get; set; }
        public string ResponseName { get; set; }

        public string PathMessage { get; set; }
        public bool IsExist { get; set; }
        private bool _IsNotSaved = true;
        public bool IsNotSaved
        {
            get { return _IsNotSaved; }
            set
            {
                _IsNotSaved = value;
                OnPropertyChanged("IsNotSaved");
            }
        }
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #region ReqeuestServicecode

        private string _ReqeuestServicecode;
        ///
        public string ReqeuestServicecode
        {
            get
            {
                return _ReqeuestServicecode;
            }
            set
            {
                this._ReqeuestServicecode = value;
                OnPropertyChanged("ReqeuestServicecode");
            }
        }

        #endregion
    }
}
