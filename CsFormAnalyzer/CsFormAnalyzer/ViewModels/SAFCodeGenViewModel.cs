using CsFormAnalyzer.Core;
using CsFormAnalyzer.Mvvm;
using SC.WPF.Tools.CodeHelper;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CsFormAnalyzer.Utils;
using System.Text.RegularExpressions;
using System.Diagnostics;
namespace CsFormAnalyzer.ViewModels
{
	public partial class SAFCodeGenViewModel : ViewModelBase
    {

        #region DBConnectionList
        Dictionary<string, string> dbConnectionList;
        public Dictionary<string,string> DBConnectionList
        {
            get
            {
                if (dbConnectionList == null)
                {
                    dbConnectionList = new Dictionary<string, string>();
                    dbConnectionList.Add("HP", "Data Source=10.28.16.21;Initial Catalog=HISH;User ID=dev_user;Password=password1!;");
                    dbConnectionList.Add("SP", "Data Source=10.28.16.21;Initial Catalog=HISS;User ID=dev_user;Password=password1!;");
                    dbConnectionList.Add("MR", "Data Source=10.28.16.21;Initial Catalog=HISE;User ID=dev_user;Password=password1!;");
                    dbConnectionList.Add("CM", "Data Source=10.28.16.21;Initial Catalog=HISZ;User ID=dev_user;Password=password1!;");
                    dbConnectionList.Add("MD", "Data Source=10.28.16.21;Initial Catalog=HISM;User ID=dev_user;Password=password1!;");
                }
                return dbConnectionList;
            }
        } 
        #endregion

        #region SelectedDBConnection
        string _selectedDbCon;
        public string SelectedDBConnection
        {
            get
            {
                return _selectedDbCon;
            }
            set
            {
                _selectedDbCon = value;
            }
        } 
        #endregion

        public string DBConnectionString
        {
            get;
            set;
        }

        

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
      
        #region ModelName
        public string SPName
        {
            get;
            set;
        }
        public string ModelName
        {
            get;
            set;
        } 
        #endregion

        #region SPNameList
        List<string> spNameList;
        public List<string> SPNameList
        {
            get
            {
                return spNameList;
            }
            set
            {
                spNameList = value;
                OnPropertyChanged("SPNameList");
            }
        } 
        #endregion

        #region SPParamList
        DataTable spParamsDataTable;
        public DataTable SPParamsDataTable
        {
            get { return spParamsDataTable; }
            set { spParamsDataTable = value; OnPropertyChanged("SPParamsDataTable"); }
        } 
        #endregion

        Visibility spExcuteVisible = Visibility.Collapsed;
        public Visibility SPExcuteVisible
        {
            get { return spExcuteVisible; }
            set { spExcuteVisible = value; OnPropertyChanged("SPExcuteVisible"); }
        }

        #region StartGentCommand
        public ICommand StartGentCommand { get; private set; }
        private void OnStartGentCommand()
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
            if(SPParamsDataTable == null)
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

                SPParamsDataTable = dt;
                SPExcuteVisible = Visibility.Visible;
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

                DataSet dsParam = DBObject.GetSPParameters(dbcon, SPName);
                StringBuilder sbCode = new StringBuilder();
                // Model Request 
                sbCode.AppendLine(" public class " + ModelName + "Request : SAF.Model.ISAFReturn<" + ModelName + "Response>");
                sbCode.AppendLine(" {");

                foreach (DataRow r in dsParam.Tables[0].Rows)
                {
                    sbCode.AppendLine("\tpublic string " + r[0].ToString().Replace("@", "") + " { get; set; }");
                }
                sbCode.AppendLine(" }");
                sbCode.AppendLine();


                // Model Response

                sbCode.AppendLine(" public class " + ModelName + "Response");
                sbCode.AppendLine(" {");
                sbCode.AppendLine("\tpublic IList<" + ModelName + "> " + ModelName + " { get; set; }");
                sbCode.AppendLine(" }");
                sbCode.AppendLine();

                // Model 
                sbCode.AppendLine(" public class " + ModelName);
                sbCode.AppendLine(" {");


                DataSet dsColumn = null;
                SqlParameter[] sqlparams = new SqlParameter[SPParamsDataTable.Rows.Count];
                for (int i = 0; i < SPParamsDataTable.Rows.Count; i++)
                {
                    if (SPParamsDataTable.Rows[i]["Value"] == null || SPParamsDataTable.Rows[i]["Value"].ToString() == "")
                    {
                        MessageBox.Show(SPParamsDataTable.Rows[i]["parameter"] + "  parameter 의 Value 값을 지정 하셔야 합니다!");
                        return;
                    }
                    sqlparams[i] = new SqlParameter(SPParamsDataTable.Rows[i]["parameter"] + "", SPParamsDataTable.Rows[i]["Value"] + "");
                }
                dsColumn = DataAccess.GetDataSet2(DBConnectionString, this.SPName, sqlparams, CommandType.StoredProcedure);

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

                foreach (DataColumn col in dsColumn.Tables[0].Columns)
                {
                    sbCode.AppendLine("\tpublic string " + col.ColumnName + " { get; set; }");
                    sbCode.AppendLine();
                }

                sbCode.AppendLine();
                sbCode.AppendLine(" }");
                sbCode.AppendLine();

                Code_Req_Resp_Model = sbCode.ToString();
                sbCode.Clear();
                // Service
                sbCode.AppendLine(" public class " + ModelName + "Service : SAF.Server.SAFService");
                sbCode.AppendLine(" {");
                sbCode.AppendLine("\tpublic override string DefaultConnectionName");
                sbCode.AppendLine("\t{");
                sbCode.AppendLine("\t\tget");
                sbCode.AppendLine("\t\t{");
                sbCode.AppendLine("\t\t\treturn \"CDR\";");
                sbCode.AppendLine("\t\t}");
                sbCode.AppendLine("\t}");
                sbCode.AppendLine();
                sbCode.AppendLine("\tpublic async Task<" + ModelName + "Response> Any(" + ModelName + "Request request)");
                sbCode.AppendLine("\t{");
                sbCode.AppendLine("\t\tvar ret = new " + ModelName + "Response();");
                sbCode.AppendLine("\t\tvar data = await DataProvider.QueryAsync<" + ModelName + ">( " + SPName + ",");

                sbCode.AppendLine("\t\tnew { ");
                foreach (DataRow r in dsParam.Tables[0].Rows)
                {
                    string paramName = r[0].ToString().Replace("@", "");
                    sbCode.AppendLine("\t\t\t" + paramName + "= request." + paramName + ",");
                }
                sbCode.Remove(sbCode.Length - 3, 3);
                sbCode.AppendLine();
                sbCode.AppendLine("\t\t\t }");
                sbCode.AppendLine("\t\t,null, System.Data.CommandType.StoredProcedure);");
                sbCode.AppendLine();
                sbCode.AppendLine("\t\tret." + ModelName + " = data");

                sbCode.AppendLine("\t\treturn ret;");
                sbCode.AppendLine("\t}");
                sbCode.AppendLine(" }");

                Code_Service = sbCode.ToString();

                SPExcuteVisible = Visibility.Collapsed;
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
        #endregion

        #region ConnectCommand
        public ICommand ConnectCommand { get; private set; }

        private void OnConnectCommand()
        {
            DataSet ds = DBObject.GetTables(DBConnectionString);
            ds = DBObject.GetSPs(DBConnectionString);
            var info = new List<string>();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                info.Add(row[0] + "." + row[1] + "");
            }
            SPNameList = info;
        }

        #endregion

        #region EditSPParamCommand
        public ICommand GetSPParamCommand { get; private set; }

        private void OnEditSPParamCommand()
        {
          
        }
        #endregion 

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

        internal void Run()
        {
        }
    }
}
