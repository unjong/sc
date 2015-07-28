using CsFormAnalyzer.Mvvm;
using CsFormAnalyzer.Utils;
using SC.WPF.Tools.CodeHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CsFormAnalyzer.ViewModels
{
    public partial class ModelGeneratorByUspVM : ViewModelBase
    {
        #region Fields

        private int maxStep = 3;

        #endregion

        #region Properties

        public int Step
        {
            get { return GetPropertyValue(); }
            set
            {
                var preStep = GetPropertyValue("Step");
                SetPropertyValue(value);
                RunStep(value, preStep);
            }
        }

        public string BaseName { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public IEnumerable<string> DbNames 
        { 
            get 
            {
                var value = GetPropertyValue();
                if (value == null)
                {
                    value = new string[] { "HISE", "HISH", "HISI", "HISM", "HISO", "HISP", "HISR", "HISRS", "HISS", "HISU", "HISZ", "강남HP", "강남SP" };
                    SetPropertyValue(value, "DbNames", false);
                }

                return value;
            } 
            set { SetPropertyValue(value); } 
        }

        public string DbName { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string UspName 
        { 
            get { return GetPropertyValue(); } 
            set 
            { 
                SetPropertyValue(value);

                //string query = "select DBNM from [dbo.TBL_HIS2DB_USP] where SPNM = '" + value + "' order by DBNM desc";
                //DataSet ds = DataAccess.GetDataSet2(AppManager.DataConnectionString, query, CommandType.Text);
                //if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                //{
                //    this.DbName = ds.Tables[0].Rows[0][0] + "";
                //}

                string query = @"
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
";
                var sqlParameters = new SqlParameter[] 
                {
                    new SqlParameter() { ParameterName = "UspName", Value = value },
                };
                var ds = DataAccess.GetDataSet2(AppManager.ServerConnectionString, query,
                    sqlParameters, CommandType.Text);
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    this.DbName = ds.Tables[0].Rows[0][0] + "";
                }
            } 
        }
        public bool IsSaveUsp { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public SPInfo CurrentSpInfo { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public string ExecuteSpCode { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        [InstanceNew]
        public ObservableCollection<ResultSpItem> ResultSpItems { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public string ModelCode { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string RequestCode { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string ServiceCode { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        #endregion

        #region Commands

        public ICommand PreviousCommand { get; private set; }
        public ICommand NextCommand { get; private set; }

        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public override void InitCommands()
        {
            PreviousCommand = base.CreateCommand(delegate { Step -= 1; }, delegate { return Step > 0; });
            NextCommand = base.CreateCommand(delegate { if (CanNext(Step)) Step += 1; }, delegate { return Step < maxStep; });

            OkCommand = base.CreateCommand(delegate { DialogResult = true; }, delegate { return Step == maxStep; });
            CancelCommand = base.CreateCommand(delegate { base.DialogResult = false; });
        }

        #endregion

        #region Methods

        private void RunStep(int step, int preStep)
        {
            if (step == 2)
            {
                if (IsSaveUsp)
                {
                    if (preStep > step)
                        Step--;
                    else
                        Step++; 
                    return;
                }
            }
            else if (step == 3)
            {
                //ModelCode = GetModelCode();
                //RequestCode = GetRequestCode();
                RequestCode = string.Format(@"{0}

{1}", GetRequestCode(), GetModelCode());
                ServiceCode = GetServiceCode();

                var win = GetWindow();
                if (win != null && win.IsModal())
                {
                    DialogResult = true;
                }
            }
        }

        private bool CanNext(int step)
        {
            if (step == 0)
            {
                if (string.IsNullOrEmpty(this.BaseName)) throw new ArgumentNullException("RequestName");

                CurrentSpInfo = GetSpInfo();
                return CurrentSpInfo != null;
            }
            else if (step == 1)
            {
                bool success = ExecuteSp();
                return success;
            }
            else if (step == 2)
            {
                foreach (var item in ResultSpItems)
                {
                    if (string.IsNullOrEmpty(item.ModelName))
                    {
                        MessageBox.Show("모델명을 지정하세요");
                        return false;
                    }
                }
            }

            return true;
        }

        private SPInfo GetSpInfo()
        {
            try
            {
                var spInfo = new SPInfo()
                {
                    DBName = this.DbName,
                    DbConnectionString = GetConnectionString(this.DbName),
                    SPName = this.UspName,
                    SPCodeText = this.UspName,
                    IsSaveSP = this.IsSaveUsp,
                    FacadeMethodName = "",
                    FacadePath = "",
                };
                
                if (spInfo.IsQueryText)
                {
                    spInfo.SPCodeText = spInfo.SPName;

                    DataTable dt = new DataTable();
                    dt.Columns.Add("parameter");
                    dt.Columns.Add("Value");

                    ObservableCollection<string> paramList = new ObservableCollection<string>();
                    foreach (var m in Regex.Matches(spInfo.SPCodeText, @"@[\w]+"))
                    {
                        DataRow dr = dt.NewRow();
                        dr[0] = m;
                        dr[1] = "0";
                        dt.Rows.Add(dr);

                        string pName = m.ToString().Replace("@", "");
                        if (paramList.Count(x => x == pName) == 0)
                            paramList.Add(pName);
                    }

                    bool isCUD = (spInfo.SPCodeText.Contains("INSERT") || spInfo.SPCodeText.Contains("UPDATE") ||
                            spInfo.SPCodeText.Contains("DELETE") || spInfo.SPCodeText.Contains("SAVE"));

                    spInfo.IsSaveSP = isCUD;
                    spInfo.RequestPropertyList = paramList;
                    spInfo.SPParamsDT = dt;

                    spInfo.IsSaveSP = this.IsSaveUsp ? true : isCUD;
                    this.IsSaveUsp = spInfo.IsSaveSP;

                    return spInfo;
                }

                DataTable dt2 = DBObject.GetSPParames(spInfo.DbConnectionString, spInfo.SPName);
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


                string query = @"SELECT definition FROM sys.sql_modules WHERE object_id = (OBJECT_ID(N'[" + spInfo.SPName + "]'));";
                DataSet ds = SC.WPF.Tools.CodeHelper.DataAccess.GetDataSet2(spInfo.DbConnectionString, query, CommandType.Text);
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    spInfo.SPCodeText = (ds.Tables[0].Rows[0][0] + "").Trim();
                }
                else
                {
                    MessageBox.Show("선택된 DB에서 SP를 찾을 수 없습니다. 다른 DB를 선택한후 가져오기를 해보세요.");
                    return null;
                }

                spInfo.SPParamsDT = dt2;

                #region CUD 체크
                string spUperName = spInfo.SPName.ToUpper();
                string tempSPCodeText = (spInfo.SPCodeText + "").ToUpper();

                string prefix = string.Empty;

                bool isCUD2 = (spUperName.Contains("INSERT") || spUperName.Contains("UPDATE") ||
                         spUperName.Contains("DELETE") || spUperName.Contains("SAVE"))
                        &&
                        (tempSPCodeText.Contains("INSERT") || tempSPCodeText.Contains("UPDATE") ||
                         tempSPCodeText.Contains("DELETE"));

                spInfo.IsSaveSP = this.IsSaveUsp ? true : isCUD2;
                this.IsSaveUsp = spInfo.IsSaveSP;

                #endregion

                //if (string.IsNullOrEmpty(spInfo.SPCodeText))
                //{
                //    DataTable dt = DBObject.GetSPParames(spInfo.DbConnectionString, spInfo.SPName);
                //    dt.Columns.Add("Value");
                //    foreach (DataRow dr in dt.Rows)
                //    {
                //        int len = int.Parse(dr["length"].ToString());
                //        if (len == 8 && dr["parameter"].ToString().Contains("Ymd"))
                //        {
                //            dr["value"] = "20080909";
                //        }
                //        else
                //        {
                //            dr["value"] = "1".PadRight(len - 1, '0');
                //        }
                //    }

                //    string query = @"SELECT definition FROM sys.sql_modules WHERE object_id = (OBJECT_ID(N'[" + spInfo.SPName + "]'));";
                //    DataSet ds = SC.WPF.Tools.CodeHelper.DataAccess.GetDataSet2(spInfo.DbConnectionString, query, CommandType.Text);
                //    if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                //    {
                //        spInfo.SPCodeText = (ds.Tables[0].Rows[0][0] + "").Trim();
                //    }
                //    else
                //    {
                //        MessageBox.Show("선택된 DB에서 SP를 찾을 수 없습니다. 다른 DB를 선택한후 가져오기를 해보세요.");
                //        return null;
                //    }

                //    spInfo.SPParamsDT = dt;

                //    #region CUD 체크

                //    string spUperName = spInfo.SPName.ToUpper();
                //    string tempSPCodeText = (spInfo.SPCodeText + "").ToUpper();

                //    string prefix = string.Empty;

                //    bool isCUD = (spUperName.Contains("INSERT") || spUperName.Contains("UPDATE") ||
                //             spUperName.Contains("DELETE") || spUperName.Contains("SAVE"))
                //            &&
                //            (tempSPCodeText.Contains("INSERT") || tempSPCodeText.Contains("UPDATE") ||
                //             tempSPCodeText.Contains("DELETE"));

                //    spInfo.IsSaveSP = isCUD;

                //    #endregion
                //}

                return spInfo;
            }
            catch 
            {
                return null;
            }
        }
        
        private string GetConnectionString(string dbName)
        {
            string connectionString;

            if (dbName == "HOOH") connectionString = "Data Source=10.10.11.20;Initial Catalog=HOOHTESTDB;User Id=dev_user;Password=password1!;MultipleActiveResultSets=True;";
            else if (dbName == "CDR") connectionString = "Data Source=10.10.11.20;Initial Catalog=HISZ;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;";
            else if (dbName == "CDR30") connectionString = "Data Source=10.10.11.20;Initial Catalog=HIS_TEST;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;";
            else if (dbName == "CDRS") connectionString = "Data Source=10.10.11.20;Initial Catalog=CDR;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;";
            else if (dbName == "CM") connectionString = "Data Source=10.10.11.20;Initial Catalog=HISZ;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;";
            else if (dbName == "MR") connectionString = "Data Source=10.10.11.20;Initial Catalog=HISE;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;";
            else if (dbName == "HP") connectionString = "Data Source=10.10.11.20;Initial Catalog=HISH;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;";
            else if (dbName == "MD") connectionString = "Data Source=10.10.11.20;Initial Catalog=HISM;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;";
            else if (dbName == "SP") connectionString = "Data Source=10.10.11.20;Initial Catalog=HISS;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;";
            else if (dbName == "TEST") connectionString = "Data Source=10.10.11.20;Initial Catalog=HIS_TEST;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;";
            else if (dbName == "BizTalk") connectionString = "Data Source=10.10.11.20;Initial Catalog=Yumc_EMR;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;";
            else if (dbName == "Cache") connectionString = "his@10.26.10.151:6379;";
            else if (dbName == "HISRS") connectionString = "Data Source=10.10.11.20;Initial Catalog=HISRS;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;";
            else if (dbName == "HISR") connectionString = "Data Source=10.10.11.20;Initial Catalog=HISR;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;";
            else if (dbName == "HISU") connectionString = "Data Source=10.10.11.20;Initial Catalog=HISU;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;";
            else if (dbName == "CDWE") connectionString = "Data Source=10.10.11.20;Initial Catalog=HISE;User Id=dev_user;Password=password1!;MultipleActiveResultSets=True;Connection Timeout=120";
            else if (dbName == "강남HP") connectionString = @"Data Source=172.31.7.49\DEV08;Initial Catalog=HISH;User ID=his_user;Password=cns0909;";
            else if (dbName == "강남SP") connectionString = @"Data Source=172.31.7.49\DEV08;Initial Catalog=HISS;User ID=his_user;Password=cns0909;";
            else connectionString = string.Format("Data Source=10.10.11.20;Initial Catalog={0};User Id=dev_user;Password=password1!;MultipleActiveResultSets=True;Connection Timeout=120", dbName);

            return connectionString;
        }

        private bool ExecuteSp()
        {            
            if (string.IsNullOrEmpty(ExecuteSpCode) && CurrentSpInfo.SPParamsDT == null)
            {
                return false;
            }
            
            List<Model> modelList = new List<Model>();
            // Model 정보
            CurrentSpInfo.IsSaveSP = this.IsSaveUsp;
            if (!CurrentSpInfo.IsSaveSP)
            {
                DataSet dsColumn = GetSPExcuteDataSet(ExecuteSpCode);
                if (dsColumn == null || dsColumn.Tables.Count == 0)
                {
                    MessageBox.Show("결과값을 조회 할 수 없습니다. 파라메터 정보를 다시 지정해보세요.");
                    return false;
                }

                string[] spNameSplit = CurrentSpInfo.SPName.Split('_');
                string modelBaseName = string.Empty;
                modelBaseName = spNameSplit[spNameSplit.Length - 1].RegexReplace("(Select|Save|Insert|Update)", "");
                if (modelBaseName.StartsWith("By"))
                {
                    modelBaseName = spNameSplit[spNameSplit.Length - 2].RegexReplace("(Select|Save|Insert|Update)", "") + modelBaseName;
                }

                ResultSpItems.Clear();
                for (int i = 0; i < dsColumn.Tables.Count; i++)
                {
                    var dt = dsColumn.Tables[i].Copy();
                    var modelName = modelBaseName;
                    if (dsColumn.Tables.Count > 1)
                        modelName = modelName + i.ToString() + "Model";
                    else
                        modelName = modelName + "Model";

                    ResultSpItems.Add(new ResultSpItem() { SpInfo = this.CurrentSpInfo, ModelName = modelName, Table = dt });
                }
            }
            else
            {
                ResultSpItems.Clear();
                ResultSpItems.Add(new ResultSpItem() { SpInfo = this.CurrentSpInfo, ModelName = string.Empty, Table = null });
            }
            
            return true;
        }

        private DataSet GetSPExcuteDataSet(string excuteSPQuery)
        {            
            if (!String.IsNullOrEmpty(excuteSPQuery))
            {
                return DataAccess.GetDataSet2(CurrentSpInfo.DbConnectionString, excuteSPQuery.Trim(), CommandType.Text);
            }
            else
            {
                SqlParameter[] sqlparams = new SqlParameter[CurrentSpInfo.SPParamsDT.Rows.Count];
                for (int i = 0; i < CurrentSpInfo.SPParamsDT.Rows.Count; i++)
                {
                    if (CurrentSpInfo.SPParamsDT.Rows[i]["Value"] == null || CurrentSpInfo.SPParamsDT.Rows[i]["Value"].ToString() == "")
                    {
                        MessageBox.Show(CurrentSpInfo.SPParamsDT.Rows[i]["parameter"] + "  parameter 의 Value 값을 지정 하셔야 합니다!");
                        return null;
                    }
                    sqlparams[i] = new SqlParameter(CurrentSpInfo.SPParamsDT.Rows[i]["parameter"] + "", CurrentSpInfo.SPParamsDT.Rows[i]["Value"] + "");
                }

                DataSet ds = null;
                if (CurrentSpInfo.IsQueryText)
                {
                    ds = DataAccess.GetDataSet2(CurrentSpInfo.DbConnectionString, CurrentSpInfo.SPCodeText, sqlparams, CommandType.Text);
                    CurrentSpInfo.SPName = CurrentSpInfo.SPCodeText;
                }
                else
                    ds = DataAccess.GetDataSet2(CurrentSpInfo.DbConnectionString, CurrentSpInfo.SPName, sqlparams, CommandType.StoredProcedure);

                return ds;
            }
        }

        #region Code Generate...

        private string GetModelCode()
        {
            var modelList = GetModelList();

            var sbModelCode = new StringBuilder();
            foreach (var m in modelList)
            {
                sbModelCode.AppendLine("///<summary>");
                sbModelCode.AppendLine("///" + CurrentSpInfo.SPName + " 에서 생성");
                sbModelCode.AppendLine("///</summary>");
                sbModelCode.AppendLine(" public class " + m.ModelName + " : SAFModel");
                sbModelCode.AppendLine(" {");

                foreach (string pName in m.ModelPropertyList)
                {
                    string pLocalName = "_" + pName.Left(1).ToLower() + pName.Substring(1);
                    sbModelCode.AppendLine("\t#region " + pName);
                    sbModelCode.AppendLine("\tstring " + pLocalName + ";");
                    sbModelCode.AppendLine("\tpublic string " + pName);

                    sbModelCode.AppendLine("\t{");
                    sbModelCode.AppendLine("\t\tget { return " + pLocalName + "; }");
                    sbModelCode.AppendLine("\t\tset { RaiseAndSetIfChanged(ref " + pLocalName + ", value); }");

                    sbModelCode.AppendLine("\t}");
                    sbModelCode.AppendLine("\t#endregion");
                    sbModelCode.AppendLine();
                }
                sbModelCode.AppendLine(" }");
            }

            return sbModelCode.ToString();
        }

        private string GetRequestCode()
        {
            var sb = new StringBuilder();
            var sbProperty = new StringBuilder();
            var sbPropertyInit = new StringBuilder();
            var sbResponseProperty = new StringBuilder();
            var name = this.BaseName;

            var parameters = GetRequestParameters(CurrentSpInfo);

            parameters.Distinct().ToList().ForEach(p =>
            {
                sbProperty.Append(string.Format(@"
        public string {0};", p));
                sbPropertyInit.Append(string.Format(@"
            {0} = string.Empty;", p));
            });

            if (this.IsSaveUsp)
            {
                sbResponseProperty.Append(string.Format(@"
        public int ReturnValue {{ get; set; }}"));
            }
            else
            {
                foreach (string modelName in this.ResultSpItems.Select(p => p.ModelName))
                {
                    sbResponseProperty.Append(string.Format(@"
        public IList<{0}> {0}List {{ get; set; }}", modelName));
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
    }}
", name, sbProperty.ToString(), sbPropertyInit.ToString(), sbResponseProperty.ToString()));

            return sb.ToString();

            //var sbRequestCode = new StringBuilder();
            //var sbResponseCode = new StringBuilder();

            //string _RequestName = this.BaseName + "Request";
            //string _ResponseName = this.BaseName + "Response";

            //#region Request
            //// Model Request 
            //sbRequestCode.AppendLine(" public class " + _RequestName + " : SAF.Model.ISAFReturn<" + _ResponseName + ">");
            //sbRequestCode.AppendLine(" {");

            //DataSet dsParam = DBObject.GetSPParameters(CurrentSpInfo.DBName, CurrentSpInfo.SPName);
            //foreach (DataRow r in dsParam.Tables[0].Rows)
            //{
            //    //get { return _IndicatorGb; } set { RaiseAndSetIfChanged(ref _IndicatorGb, value); }
            //    string pName = r[0].ToString().Replace("@", "");
            //    sbRequestCode.AppendLine("\tpublic string " + pName + " { get; set; }");

            //}
            //sbRequestCode.AppendLine(" }");
            //sbRequestCode.AppendLine();
            //#endregion

            //#region Response
            //// Model Response
            //sbResponseCode.AppendLine(" public class " + _ResponseName);
            //sbResponseCode.AppendLine(" {");

            //if (this.IsSaveUsp)
            //{
            //    sbResponseCode.AppendLine("\tpublic int ReturnValue { get; set; }");
            //}
            //else
            //{
            //    foreach (string modelName in this.ResultSpItems.Select(p => p.ModelName))
            //    {
            //        sbResponseCode.AppendLine("\tpublic IList<" + modelName + "> " + modelName + "List { get; set; }");
            //    }
            //}
            //sbResponseCode.AppendLine(" }");
            //sbResponseCode.AppendLine();
            //#endregion

            //var sbResult = new StringBuilder();
            //sbResult.AppendLine(sbRequestCode.ToString());
            //sbResult.AppendLine(sbResponseCode.ToString());

            //return sbResult.ToString();
        }

        private IEnumerable<string> GetRequestParameters(SPInfo spInfo)
        {
            if (spInfo.IsQueryText)
            {   
                foreach(var p in spInfo.RequestPropertyList)
                {
                    yield return p;
                }
            }
            else
            {
                DataSet dsParam = DBObject.GetSPParameters(spInfo.DbConnectionString, spInfo.SPName);
                foreach (DataRow r in dsParam.Tables[0].Rows)
                {
                    //get { return _IndicatorGb; } set { RaiseAndSetIfChanged(ref _IndicatorGb, value); }
                    string pName = r[0].ToString().Replace("@", "");
                    yield return pName;
                }
            }
        }

        private string GetServiceCode()
        {
            var sb = new StringBuilder();
            var sbAny = new StringBuilder();
            var sbPropertyInit = new StringBuilder();
            var name = this.BaseName;
            //var ns = TobeNamespace.Replace("Model", "Server");
            var dbName = CurrentSpInfo.DBName;

            var parameters = GetRequestParameters(CurrentSpInfo);
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

            var modelList = GetModelList();

            var callMethodName = this.IsSaveUsp
                ? "ExecuteAsync"
                : modelList.Count() > 1
                ? "QueryMultipleAsync"
                : string.Format(@"QueryAsync<""{0}"">", this.CurrentSpInfo.SPName);

            sbAny.AppendLine(string.Format(@"

            var data{0} = await DataProvider.{1}(""{2}"",
                {3}, null, System.Data.CommandType.StoredProcedure);"
                , string.Empty,
                callMethodName,
                this.CurrentSpInfo.SPName,
                propertyInit));

            if (this.IsSaveUsp)
            {
                        sbAny.Append(string.Format(@"
            ret.ReturnValue = data;"));
            }
            else
            {
                int cnt = 0;
                foreach (var model in modelList)
                {
                    if (modelList.Count() > 1)
                    {
                        sbAny.Append(string.Format(@"
            if (data{0}.HasMoreResults) ret.{1}List = data{0}.NextResult<{1}>();"
                            , cnt < 1 ? string.Empty : cnt.ToString(),
                            model.ModelName));
                    }
                    else
                    {
                        sbAny.Append(string.Format(@"
            ret.{1}List = data{0};"
                            , cnt < 1 ? string.Empty : cnt.ToString(),
                            model.ModelName));
                    }
                    cnt++;
                }
            }

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


            return sb.ToString();

            //var _Name = this.BaseName;
            //var _SPName = this.BaseName;
            //var modelNames = GetModelList().Select(p => p.ModelName);

            //var dbConnectionList = new Dictionary<string, string>();
            //foreach (System.Configuration.ConnectionStringSettings item in System.Configuration.ConfigurationManager.ConnectionStrings)
            //{
            //    dbConnectionList.Add(item.Name, item.ConnectionString);
            //}

            //StringBuilder sbServerCode = new StringBuilder();

            //sbServerCode.AppendLine(" public class " + _Name + "Service : SAF.Server.SAFService");
            //sbServerCode.AppendLine(" {");
            //sbServerCode.AppendLine("\tpublic override string DefaultConnectionName");
            //sbServerCode.AppendLine("\t{");
            //sbServerCode.AppendLine("\t\tget");
            //sbServerCode.AppendLine("\t\t{");
            //sbServerCode.AppendLine("\t\t\treturn \"" + (dbConnectionList.FirstOrDefault(x => x.Value == this.DbName).Key + "").Replace("강남", "") + "\";");
            //sbServerCode.AppendLine("\t\t}");
            //sbServerCode.AppendLine("\t}");
            //sbServerCode.AppendLine();
            //sbServerCode.AppendLine("\tpublic async Task<" + _Name + "Response> Any(" + _Name + "Request request)");
            //sbServerCode.AppendLine("\t{");
            //sbServerCode.AppendLine("\t\tvar ret = new " + _Name + "Response();");

            //foreach (var modelName in modelNames)
            //{
            //    var _ModelName = modelName;

            //    if (!this.IsSaveUsp)
            //    {
            //        if (modelNames.Count() > 1)
            //        {
            //            sbServerCode.AppendLine("\t\tvar data = await DataProvider.QueryMultipleAsync( \"" + _SPName + "\",");
            //        }
            //        else
            //        {
            //            sbServerCode.AppendLine("\t\tvar data = await DataProvider.QueryAsync<" + _ModelName + ">( \"" + _SPName + "\",");
            //        }
            //    }
            //    else
            //    {
            //        sbServerCode.AppendLine("\t\tret.ReturnValue = await DataProvider.ExecuteAsync( \"" + _SPName + "\",");
            //    }

            //    sbServerCode.AppendLine("\t\tnew");
            //    sbServerCode.AppendLine("\t\t\t {");

            //    DataSet dsParam = DBObject.GetSPParameters(CurrentSpInfo.DBName, CurrentSpInfo.SPName);
            //    foreach (DataRow r in dsParam.Tables[0].Rows)
            //    {
            //        string paramName = r[0].ToString().Replace("@", "");
            //        sbServerCode.AppendLine("\t\t\t" + paramName + "= request." + paramName + ",");
            //    }
            //    sbServerCode.Remove(sbServerCode.Length - 3, 3);
            //    sbServerCode.AppendLine();
            //    sbServerCode.AppendLine("\t\t\t }");
            //    sbServerCode.AppendLine("\t\t,null, System.Data.CommandType.StoredProcedure);");
            //    sbServerCode.AppendLine();
            //}

            //if (!this.IsSaveUsp)
            //{
            //    if (modelNames.Count() == 1)
            //    {
            //        sbServerCode.AppendLine("\t\tret." + modelNames.First() + "List = data;");
            //    }
            //    else
            //    {
            //        foreach (var modelName in modelNames)
            //        {
            //            sbServerCode.AppendLine("\t\tret." + modelName + "List = data.NextResult<" + modelName + ">();");
            //        }                    
            //    }
            //}

            //sbServerCode.AppendLine("\t\treturn ret;");
            //sbServerCode.AppendLine("\t}");
            //sbServerCode.AppendLine(" }");
            //return sbServerCode.ToString();
        }

        private IEnumerable<Model> GetModelList()
        {
            foreach (var resultSpItem in ResultSpItems)
            {
                if (resultSpItem.SpInfo.IsSaveSP) continue;

                Model model = new Model();
                model.ModelName = resultSpItem.ModelName;
                List<string> modelPropertyList = new List<string>();
                foreach (DataColumn col in resultSpItem.Table.Columns)
                {
                    modelPropertyList.Add(col.ColumnName);
                }
                model.ModelPropertyList = modelPropertyList;
                yield return model;
            }
        }

        #endregion

        #endregion
    }

    public partial class ModelGeneratorByUspVM : ViewModelBase
    {
        public class ResultSpItem : PropertyNotifier
        {
            public SPInfo SpInfo { get; set; }
            public string ModelName { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public DataTable Table { get; set; }            
        }
    }
}
