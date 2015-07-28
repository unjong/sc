using CsFormAnalyzer.Mvvm;
using System.Windows.Input;
using System.Data;
using System.Collections.ObjectModel;
using SC.WPF.Tools.CodeHelper;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
namespace CsFormAnalyzer.ViewModels
{
    public partial class FacadeGenVM : ViewModelBase
    {
        #region FacadeList

        private DataTable _FacadeListDT;
        ///
        public DataTable FacadeListDT
        {
            get
            {
                if (_FacadeListDT == null)
                {
                    SqlParameter[] salParams = new SqlParameter[2];
                    salParams[0] = new SqlParameter("@ns", DBNull.Value);
                    salParams[1] = new SqlParameter("@classNM", DBNull.Value);
                    DataSet ds = DataAccess.GetDataSet2(AppManager.DataConnectionString, "USP_SELECT_FACADE",salParams, CommandType.StoredProcedure);
                    _FacadeListDT = ds.Tables[0];
                }
                return _FacadeListDT;
            }
            set
            {
                _FacadeListDT = value;
                OnPropertyChanged("FacadeListDT");
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
                    mCodeGenModelVM.PropertyChanged += (s, e) =>
                        {
                            if(e.PropertyName == "SelectedFacadeFunc")
                            {
                                SetSPInfos();
                            }
                            else if (e.PropertyName == "SelectedSPInfo")
                            {
                                CodeGenModelVM.UINameSpace = CodeGenModelVM.SelectedSPInfo.FacadePath;
                            }
                        };
                }
                return mCodeGenModelVM;
            }
            set
            {
                mCodeGenModelVM = value;
                OnPropertyChanged("CodeGenModelVM");
            }
        }

        private void SetSPInfos()
        {
            string ns = "";
            string clsNM = "";
            string method = "";
            string facadepath = CodeGenModelVM.SelectedFacadeFunc;
            foreach (DataRow dr in FacadeMethodDT.Rows)
            {
                if (dr["FacadePath"] + "" == CodeGenModelVM.SelectedFacadeFunc)
                {
                    ns = dr["NAMESPACE"] + "";
                    clsNM = dr["CLASSNM"] + "";
                    method = dr["METHODNM"] + "";
                    break;
                }
            }


            string query = "exec USP_SEARCH_CALLTREE_FACADE '" + ns + "', '" + clsNM + "', '" + method + "', 'Facade'";
            DataSet ds = DataAccess.GetDataSet2(AppManager.DataConnectionString, query, CommandType.Text);

            var spInfos = new ObservableCollection<SPInfo>();

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                string spName = dr["SP"] + "";
                if (string.IsNullOrEmpty(spName)) continue;
                string[] spNameSplit = spName.Split('_');
                string dbName = string.Empty;
                if (spNameSplit.Length > 2)
                {
                    if (CodeGenModelVM.DBConnectionList.ContainsKey(spNameSplit[1]))
                        dbName = CodeGenModelVM.DBConnectionList[spNameSplit[1]];
                    else
                        dbName = CodeGenModelVM.DBConnectionList["SP"];
                }

                string spuperName = spName.ToUpper();
                bool isCUD = (spuperName.Contains("INSERT") || spuperName.Contains("UPDATE") ||
                spuperName.Contains("DELETE") || spuperName.Contains("SAVE"));

                if (spInfos.Count(x => x.SPName == spName) > 0) continue;
                spInfos.Add(new SPInfo()
                {
                    DBName = dbName,
                    SPName = spName,
                    IsSaveSP = isCUD,
                    FacadeMethodName = method,
                    FacadePath = facadepath
                });
            }

            CodeGenModelVM.UINameSpace = ds.Tables[0].Rows[0][1] + "";
            string reqName = method.Replace("NTx", "").Replace("Tx", "");
            CodeGenModelVM.RequestName = reqName+"Request";
            CodeGenModelVM.ResponseName = reqName+"Response";
            CodeGenModelVM.FacadeSPList = spInfos;
        }
        #endregion

        #region SelectedRow

        private DataRowView _SelectedRow;
        ///
        public DataRowView SelectedRow
        {
            get
            {
                return _SelectedRow;
            }
            set
            {
                _SelectedRow = value;
            }
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
            if (SelectedRow == null) return;
            base.InvokeAsyncAction(()=>
                {
                DataRow dr = SelectedRow.Row as DataRow;
                SqlParameter[] salParams = new SqlParameter[2];
                salParams[0] = new SqlParameter("@ns", dr["NAMESPACE"] + "");
                salParams[1] = new SqlParameter("@classNM", dr["CLASSNM"] + "");
                DataSet ds = DataAccess.GetDataSet2(AppManager.DataConnectionString, "USP_SELECT_FACADE", salParams, CommandType.StoredProcedure);

                Dictionary<string, string> facadeMtList = new Dictionary<string, string>();
                //facadeMtList.Add( )
                foreach(DataRow r in ds.Tables[0].Rows)
                {
                    if (!facadeMtList.ContainsKey(r["FacadePath"]+""))
                        facadeMtList.Add(r["FacadePath"] + "", r["Method"] + "");
                }
                FacadeMethodDT = ds.Tables[0];
                this.CodeGenModelVM.FacadeFuncList = facadeMtList;
                });
        }
        #endregion

        private DataTable FacadeMethodDT;

    }
}
