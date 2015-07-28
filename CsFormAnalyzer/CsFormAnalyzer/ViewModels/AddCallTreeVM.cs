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
    public class AddCallTreeVM : ViewModelBase
    {

        #region CallTreeItem

        private CsFormAnalyzer.ViewModels.CallTreeVM.CallTreeObjectItem _CallTreeItem;
        ///
        public CsFormAnalyzer.ViewModels.CallTreeVM.CallTreeObjectItem CallTreeItem
        {
            get
            {
                return _CallTreeItem;
            }
            set
            {
                _CallTreeItem = value;
               
                PARAMSCNT = BizCallTreeAnalysisVM.GetParmasCount(PARAMS).ToString();

                if (string.IsNullOrEmpty(_CallTreeItem.ID))
                    return;

                string query = "SELECT * from TBL_BIZ_INFO where ID = '" + _CallTreeItem.ID + "'";
                                    
               DataSet ds= DataAccess.GetDataSet2(AppManager.DataConnectionString, query, CommandType.Text);

                if(ds.Tables.Count > 0 && ds.Tables[0].Rows.Count>0)
                {
                    NS =  ds.Tables[0].Rows[0]["NAMESPACE"] + "";
                    CLASSNM = ds.Tables[0].Rows[0]["CLASSNM"] + "";
                    Method = ds.Tables[0].Rows[0]["METHODNM"] + "";
                    LAYER = ds.Tables[0].Rows[0]["LAYER"] + "";
                    PARAMS = ds.Tables[0].Rows[0]["PARAMS"] + "";
                    RTNVALUE = ds.Tables[0].Rows[0]["RTNVALUE"] + "";
                    CALLOBJNS = ds.Tables[0].Rows[0]["CALLOBJNS"] + "";
                    CALLOBJNM = ds.Tables[0].Rows[0]["CALLOBJNM"] + "";
                    CALLFUNCNM = ds.Tables[0].Rows[0]["CALLFUNCNM"] + "";
                    CALLFUNCPARAMS = ds.Tables[0].Rows[0]["CALLFUNCPARAMS"] + "";
                    ID = _CallTreeItem.ID + "";
                }

                OnPropertyChanged("CallTreeItem");
            }
        }

        #endregion

        public string ID { get; set; }
        public string CLASSNM { get; set; }
        public string NS { get; set; }
        public string Method { get; set; }
        public string LAYER {get;set;}

        public string PARAMS {get;set;}

        public string RTNVALUE {get;set;}

        public string CALLOBJNS {get;set;}
        public string CALLOBJNM {get;set;}
        public string CALLFUNCNM {get;set;}
        public string CALLFUNCPARAMS {get;set;}

        public string PARAMSCNT { get;set;}

        public string CALLPARAMCNT { get; set; }

        #region ScmdSave
        ICommand _scmdSave;
        public ICommand ScmdSave
        {
            get
            {
                if (_scmdSave == null)
                    _scmdSave = CreateCommand(ExecscmdSave);
                return _scmdSave;
            }
        }

        internal void ExecscmdSave(object param)
        {
            PARAMSCNT = BizCallTreeAnalysisVM.GetParmasCount(PARAMS).ToString();
            CALLPARAMCNT = BizCallTreeAnalysisVM.GetParmasCount(CALLFUNCPARAMS).ToString();
            Save();
        } 
        #endregion

        #region ScmdAddNew
        ICommand _ScmdAddNew;
        public ICommand ScmdAddNew
        {
            get
            {
                if (_ScmdAddNew == null)
                    _scmdSave = CreateCommand(ExecScmdAddNew);
                return _scmdSave;
            }
        }

        internal void ExecScmdAddNew(object param)
        {
            PARAMSCNT = BizCallTreeAnalysisVM.GetParmasCount(PARAMS).ToString();
            CALLPARAMCNT = BizCallTreeAnalysisVM.GetParmasCount(CALLFUNCPARAMS).ToString();
            ID = "";
            Save();
        }

        private void Save()
        {
            using (SqlConnection conn = new SqlConnection(AppManager.DataConnectionString))
            {
                SqlCommand sqlCmd = new SqlCommand("USP_SAVE_CALLTREE", conn);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("@id", ID);
                sqlCmd.Parameters.AddWithValue("@layer", LAYER);
                sqlCmd.Parameters.AddWithValue("@namespace", NS);
                sqlCmd.Parameters.AddWithValue("@className", CLASSNM);
                sqlCmd.Parameters.AddWithValue("@method", Method);
                sqlCmd.Parameters.AddWithValue("@params", PARAMS);
                sqlCmd.Parameters.AddWithValue("@paramcnt", PARAMSCNT);
                sqlCmd.Parameters.AddWithValue("@rtnvalue", RTNVALUE);
                sqlCmd.Parameters.AddWithValue("@callobjNS", "" + CALLOBJNS);
                sqlCmd.Parameters.AddWithValue("@callobjNM", CALLOBJNM + "");
                sqlCmd.Parameters.AddWithValue("@callFuncNM", "" + CALLFUNCNM);
                sqlCmd.Parameters.AddWithValue("@callFuncParams", "" + CALLFUNCPARAMS);
                sqlCmd.Parameters.AddWithValue("@callparamcnt", "" + CALLPARAMCNT);
                conn.Open();
                sqlCmd.ExecuteNonQuery();
                conn.Close();
            }

            MessageBox.Show("저장 되었습니다.");
        }  
        #endregion
    }
}
