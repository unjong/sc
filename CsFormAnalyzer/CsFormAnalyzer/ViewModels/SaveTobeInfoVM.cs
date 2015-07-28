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

namespace CsFormAnalyzer.ViewModels
{
    class SaveTobeInfoVM : ViewModelBase
    {

        #region RequestName

        private string _RequestName;
        ///
        public string RequestName
        {
            get
            {
                return _RequestName;
            }
            set
            {
                _RequestName = value;
                OnPropertyChanged("RequestName");
            }
        }

        #endregion


        #region ResponseName

        private string _ResponseName;
        ///
        public string ResponseName
        {
            get
            {
                return _ResponseName;
            }
            set
            {
                _ResponseName = value;
                OnPropertyChanged("ResponseName");
            }
        }

        #endregion


        #region ServiceName

        private string _ServiceName;
        ///
        public string ServiceName
        {
            get
            {
                return _ServiceName;
            }
            set
            {
                _ServiceName = value;
                OnPropertyChanged("Servicename");
            }
        }

        #endregion

        public string SPName { get; set; }
        //public GenCode GenCodeProperty
        //{
        //    get
        //    {
        //        return _gencode;
        //    }
        //    set
        //    {
        //        _gencode = new GenCode();
        //        _gencode.SPInfoList = value.SPInfoList;
        //        _gencode.RequestName = value.RequestName;
        //        _gencode.Location = value.Location;
        //        _gencode.ResponseName = value.ResponseName;

        //        List<SaveModel> modelNameList = new List<SaveModel>();
        //        foreach(var p in _gencode.SPInfoList)
        //        {
        //            foreach (var m in p.ModelList)
        //            {
        //                if(modelNameList.Count(x=>x.ModelName == m.ModelName) ==0)
        //                    modelNameList.Add(new SaveModel(){ModelName = m.ModelName,SPName=p.SPName});
        //            }
        //        }
        //        this.ModelNameList = modelNameList;
        //    }
        //}

        public string Location { get; set; }

        #region ModelNameList
        List<Model> _modelNameList;
        public List<Model> ModelNameList
        {
            get
            {
                return _modelNameList;
            }
            set
            {
                _modelNameList = value;
                OnPropertyChanged("ModelNameList");
            }
        } 
        #endregion

        //#region ScmdSave
        //ICommand _ScmdSave;
        //public ICommand ScmdSave
        //{
        //    get
        //    {
        //        if (_ScmdSave == null)
        //            _ScmdSave = CreateCommand(ExecScmdSave);
        //        return _ScmdSave;
        //    }
        //}

        //internal void ExecScmdSave(object param)
        //{
        //    var genCode = _gencode;
        //    foreach (var sp in genCode.SPInfoList)
        //    {
        //        SqlParameter[] sqlparams = new SqlParameter[9];
        //        sqlparams[0] = new SqlParameter("@SPName", sp.SPName);
        //        string modelName = string.Empty;
        //        foreach(var m in ModelNameList.Where(x=>x.SPName == sp.SPName))
        //            modelName += m.ModelName+(", ");
        //        sqlparams[1] = new SqlParameter("@ModelName", modelName.Trim(new char[]{',',' '} ) );
        //        sqlparams[2] = new SqlParameter("@RequestName", genCode.RequestName);
        //        sqlparams[3] = new SqlParameter("@ResponseName", genCode.ResponseName);
        //        sqlparams[4] = new SqlParameter("@Location", genCode.Location);
        //        var methodnm = sp.FacadePath.RightBySearch(".");
        //        var clsnm =  sp.FacadePath.LeftBySearch("."+methodnm).RightBySearch(".");
        //        var nsname = sp.FacadePath.LeftBySearch("."+clsnm);

        //        sqlparams[5] = new SqlParameter("@facadeNMSPace", nsname);
        //        sqlparams[6] = new SqlParameter("@facadeCLSNM", clsnm);
        //        sqlparams[7] = new SqlParameter("@facadeMethodNM", methodnm);
        //        sqlparams[8] = new SqlParameter("@UserName", WindowsIdentity.GetCurrent().Name);

        //        DataAccess.GetDataSet2(AppManager.DataConnectionString, @"[USP_SAVE_SP_MODEL]", sqlparams, CommandType.StoredProcedure);
        //    }
        //    MessageBox.Show("저장완료");

        //    genCode.IsNotSaved = false;
        //} 
        //#endregion


        ICommand _ScmdClose;
        public ICommand ScmdClose
        {
            get
            {
                if (_ScmdClose == null)
                    _ScmdClose = CreateCommand(ExecScmdClose);
                return _ScmdClose;
            }
        }

        internal void ExecScmdClose(object param)
        {
            this.Close();
        }
    }

    public class SaveModel
    {
        public string ModelName { get; set; }
        public string SPName { get; set; }
    }
}
