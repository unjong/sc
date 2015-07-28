using CsFormAnalyzer.Data;
using CsFormAnalyzer.Mvvm;
using CsFormAnalyzer.Utils;
using CsFormAnalyzer.Views;
using SC.WPF.Tools.CodeHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CsFormAnalyzer.ViewModels
{
    public partial class CodeGeneratorByMultiUspVM : ViewModelBase
    {
        #region Fields

        private int maxStep = 1;

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

        public string BaseNamespace { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string BaseName { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        
        [InstanceNew]
        public ObservableCollection<UspItem> UspItems { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public string ModelCode { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string RequestCode { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string ServiceCode { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        #endregion

        #region Commands

        public ICommand PreviousCommand { get; private set; }
        public ICommand NextCommand { get; private set; }

        public ICommand AddNewUspItemCommand { get; private set; }

        public override void InitCommands()
        {
            PreviousCommand = base.CreateCommand(delegate { Step -= 1; }, delegate { return Step > 0; });
            NextCommand = base.CreateCommand(delegate { if (CanNext(Step)) Step += 1; }, delegate { return Step < maxStep; });

            this.AddNewUspItemCommand = new SimpleCommand(OnExecuteAddNewUspItemCommand);
        }

        private void OnExecuteAddNewUspItemCommand(object obj)
        {
            var vm = ViewModelLocator.Current.GetInstance<ModelGeneratorByUspVM>(true);
            if (AppManager.Current.ShowDialogView(typeof(ModelGeneratorByUspView), vm))
            {
                var uspItem = new UspItem()
                {
                    Name = vm.UspName,
                    SpItems = vm.ResultSpItems,
                };

                UspItems.Add(uspItem);
            }
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
                    //if (string.IsNullOrEmpty(BaseNamespace)) throw new ArgumentNullException("Namespace");
                    if (string.IsNullOrEmpty(BaseName)) throw new ArgumentNullException("Name");
                                        
                    RequestCode = GetRequestCode();
                    ServiceCode = GetServiceCode();
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
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
                sbModelCode.AppendLine("///" + m.SPName + " 에서 생성");
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
            var sbRequestCode = new StringBuilder();
            var sbResponseCode = new StringBuilder();

            var _RequestName = BaseName + "Request";
            var _ResponseName = BaseName + "Response";

            // 주석
            sbRequestCode.AppendLine(GetClassComment(false));

            #region Request
            // Model Request 
            sbRequestCode.AppendLine(" public class " + _RequestName + " : SAF.Model.ISAFReturn<" + _ResponseName + ">");
            sbRequestCode.AppendLine(" {");

            foreach (var item in this.UspItems)
            {
                var _DbName = item.SpItems.First().SpInfo.DBName;
                var _SpName = item.SpItems.First().SpInfo.SPName;
                //var _IsSaveSP = this.UspItems.First().SpItem.SpInfo.IsSaveSP;
                DataSet dsParam = DBObject.GetSPParameters(_DbName, _SpName);
                foreach (DataRow r in dsParam.Tables[0].Rows)
                {
                    //get { return _IndicatorGb; } set { RaiseAndSetIfChanged(ref _IndicatorGb, value); }
                    string pName = r[0].ToString().Replace("@", "");
                    sbRequestCode.AppendLine("\tpublic string " + pName + " { get; set; }");
                }
            }

            sbRequestCode.AppendLine(" }");
            sbRequestCode.AppendLine();
            #endregion

            #region Response
            // Model Response
            sbResponseCode.AppendLine(" public class " + _ResponseName);
            sbResponseCode.AppendLine(" {");

            foreach (var uspItem in this.UspItems)
            {
                foreach (var spItem in uspItem.SpItems)
                {
                    if (spItem.SpInfo.IsSaveSP)
                    {
                        sbResponseCode.AppendLine("\tpublic int ReturnValue { get; set; }");
                    }
                    else
                    {
                        var modelName = spItem.ModelName;
                        sbResponseCode.AppendLine("\tpublic IList<" + modelName + "> " + modelName + "List { get; set; }");
                    }
                }
            }
            sbResponseCode.AppendLine(" }");
            sbResponseCode.AppendLine();
            #endregion

            var sbResult = new StringBuilder();
            sbResult.AppendLine(sbRequestCode.ToString());
            sbResult.AppendLine(sbResponseCode.ToString());
            sbResult.AppendLine(GetModelCode());
            
            return sbResult.ToString();
        }

        private string GetServiceCode()
        {
            var _Name = this.BaseName;
            var _DbName = this.UspItems.First().SpItems.First().SpInfo.DBName;

            var modelNames = GetModelList().Select(p => p.ModelName);

            var dbConnectionList = new Dictionary<string, string>();
            foreach (System.Configuration.ConnectionStringSettings item in System.Configuration.ConfigurationManager.ConnectionStrings)
            {
                dbConnectionList.Add(item.Name, item.ConnectionString);
            }

            StringBuilder sbServerCode = new StringBuilder();

            // 주석
            sbServerCode.AppendLine(GetClassComment(false));

            sbServerCode.AppendLine(" public class " + _Name +"Service : SAF.Server.SAFService");
            sbServerCode.AppendLine(" {");
            sbServerCode.AppendLine("\tpublic override string DefaultConnectionName");
            sbServerCode.AppendLine("\t{");
            sbServerCode.AppendLine("\t\tget");
            sbServerCode.AppendLine("\t\t{");
            sbServerCode.AppendLine("\t\t\treturn \"" + (dbConnectionList.FirstOrDefault(x => x.Value == _DbName).Key + "").Replace("강남", "") + "\";");
            sbServerCode.AppendLine("\t\t}");
            sbServerCode.AppendLine("\t}");
            sbServerCode.AppendLine();
            sbServerCode.AppendLine("\tpublic async Task<" + _Name + "Response> Any(" + _Name + "Request request)");
            sbServerCode.AppendLine("\t{");
            sbServerCode.AppendLine("\t\tvar ret = new " + _Name + "Response();");

            foreach (var uspItem in this.UspItems)
            {
                foreach (var spItem in uspItem.SpItems)
                {
                    var _SpName = spItem.SpInfo.SPName;
                    var _IsSaveSP = spItem.SpInfo.IsSaveSP;

                    if (!_IsSaveSP)
                    {
                        if (modelNames.Count() > 1)
                        {
                            sbServerCode.AppendLine("\t\tvar data = await DataProvider.QueryMultipleAsync( \"" + _SpName + "\",");
                        }
                        else
                        {
                            sbServerCode.AppendLine("\t\tvar data = await DataProvider.QueryAsync<" + _Name + ">( \"" + _SpName + "\",");
                        }
                    }
                    else
                    {
                        sbServerCode.AppendLine("\t\tret.ReturnValue = await DataProvider.ExecuteAsync( \"" + _SpName + "\",");
                    }

                    sbServerCode.AppendLine("\t\tnew");
                    sbServerCode.AppendLine("\t\t\t {");

                    DataSet dsParam = DBObject.GetSPParameters(_DbName, _SpName);
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
                }
            }

            foreach (var uspItem in this.UspItems)
            {
                foreach (var spItem in uspItem.SpItems)
                {
                    if (modelNames.Count() == 1)
                    {
                        sbServerCode.AppendLine("\t\tret." + _Name + "List = data;");
                    }
                    else
                    {
                        foreach (string modelName in modelNames)
                        {
                            sbServerCode.AppendLine("\t\tret." + modelName + "List = data.NextResult<" + modelName + ">();");
                        }
                    }
                }
            }

            sbServerCode.AppendLine("\t\treturn ret;");
            sbServerCode.AppendLine("\t}");
            sbServerCode.AppendLine(" }");
            return sbServerCode.ToString();
        }

        private IEnumerable<Model> GetModelList()
        {
            foreach (var uspItem in this.UspItems)
            {
                foreach (var spItem in uspItem.SpItems)
                {
                    Model model = new Model();
                    model.ModelName = spItem.ModelName;
                    model.SPName = spItem.SpInfo.SPName;
                    List<string> modelPropertyList = new List<string>();
                    foreach (DataColumn col in spItem.Table.Columns)
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
            var _Namespace = BaseNamespace;
            var _RequestName = BaseName + "Request";

            StringBuilder sbServerCode = new StringBuilder();
            sbServerCode.AppendLine("// ================================================");
            if (isService)
                sbServerCode.AppendLine("// " + _RequestName.Replace("Request", "Service"));
            else
                sbServerCode.AppendLine("// " + _RequestName);
            sbServerCode.AppendLine("// -----------------------------------------------");
            sbServerCode.AppendLine("// " + DateTime.Today.Date.ToShortDateString() + " 작성자(" + Environment.UserName + ")");
            sbServerCode.AppendLine("// ===============================================");
            return sbServerCode.ToString();
        }

        #endregion
        
        #endregion        
    }

    public partial class CodeGeneratorByMultiUspVM : ViewModelBase
    {
        public class UspItem : PropertyNotifier, IConditionalTemplateItem
        {
            #region Interfaces...

            public string MatchingKey { get; set; }

            #endregion

            #region Properteis

            public string Name { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
            public IEnumerable<ModelGeneratorByUspVM.ResultSpItem> SpItems { get; set; }

            #endregion                        
        }
    }
}
