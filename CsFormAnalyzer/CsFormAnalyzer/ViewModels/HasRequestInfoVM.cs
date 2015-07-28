using CsFormAnalyzer.Mvvm;
using SC.WPF.Tools.CodeHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CsFormAnalyzer.ViewModels
{
    public class HasRequestInfoVM : ViewModelBase
    {
        #region RequestInfoDT
        DataSet requestInfoDataSet;
        public DataSet RequestInfoDataSet
        {
            get
            {
                return requestInfoDataSet;
            }
            set
            {
                this.requestInfoDataSet = value;
                OnPropertyChanged("RequestInfoDataSet");

                this.FacadeReqInfoModelList = new List<ReqInfoModel>();
                foreach (DataRow row in RequestInfoDataSet.Tables[0].Rows)
                if (SpInfomation != null)
                {

                   var facadeReq = FacadeReqInfoModelList.FirstOrDefault(x => x.Location == row["Location"] + "");
                    if(facadeReq == null)
                    {
                      facadeReq = new ReqInfoModel();
                    }
                    facadeReq.ID = row["id"] + "";
                    string modelName = row["ModelName"] + "";
                    if (string.IsNullOrEmpty(modelName))
                        modelName = "ReturnValue";
                    facadeReq.ModelNameList.Add(modelName);
                    facadeReq.ResponseName = row["ResponseName"] + "";
                    facadeReq.RequestName = row["RequestName"] + "";
                    facadeReq.ResponseName = row["ResponseName"] + "";
                    facadeReq.Location = row["Location"] + "";
                    facadeReq.UserName = row["UserName"] + "";
                    this.FacadeReqInfoModelList.Add(facadeReq);

                }
                this.SPReqInfoModelList = new List<SPReqInfoModel>();
                foreach (DataRow row in RequestInfoDataSet.Tables[1].Rows)
                {

                    var spReqInfoModel = SPReqInfoModelList.FirstOrDefault(x => x.FacadeNameSpace == row["FacadeNameSpace"] + "" 
                                                                                    && x.FacadeClassNM == row["FacadeClassNM"]+""
                                                                                    && x.FacadeMethodNM == row["FacadeMethodNM"] + "");
                    if (spReqInfoModel == null)
                    {
                        spReqInfoModel = new SPReqInfoModel();
                        spReqInfoModel.FacadeLocation = row["Location"] + "";
                        spReqInfoModel.FacadeMethodNM = row["FacadeMethodNM"] + "";
                        spReqInfoModel.FacadeClassNM = row["FacadeClassNM"] + "";
                        spReqInfoModel.FacadeNameSpace = row["FacadeNameSpace"] + "";
                        SPReqInfoModelList.Add(spReqInfoModel);
                    }
                    var reqInfo = new ReqInfoModel();
                    reqInfo.ID = row["id"] + "";
                    reqInfo.ModelNameList.Add( row["ModelName"] + "");
                    reqInfo.ResponseName = row["ResponseName"] + "";
                    reqInfo.RequestName = row["RequestName"] + "";
                    reqInfo.ResponseName = row["ResponseName"] + "";
                    reqInfo.Location = row["Location"] + "";
                    reqInfo.UserName = row["UserName"] + "";
                    reqInfo.SPName = row["SPName"] + "";
                    spReqInfoModel.ReqInfoModelList.Add(reqInfo);
                }
            }
        } 
        #endregion


        #region RequestCode
        string msg;
        public string RequestCode
        {
            get { return msg; }
            set
            {
                msg = value; OnPropertyChanged("RequestCode");
            }
        } 
        #endregion

        public List<ReqInfoModel> FacadeReqInfoModelList { get; set; }
        public List<SPReqInfoModel> SPReqInfoModelList { get; set; }

        public SPInfo SpInfomation { get; set; }

        #region ScmdShowReqCode
        ICommand _ScmdShowReqCode;
        public ICommand ScmdShowReqCode
        {
            get
            {
                if (_ScmdShowReqCode == null)
                    _ScmdShowReqCode = CreateCommand(ExecScmdShowReqCode);
                return _ScmdShowReqCode;
            }
        }

        internal void ExecScmdShowReqCode(object param)
        {
            try
            {
                if(SpInfomation.RequestPropertyList == null)
                {
                    if (SpInfomation.RequestPropertyList == null)
                    {
                        ObservableCollection<string> paramList = new ObservableCollection<string>();
                        // 1.request 정보
                        DataSet dsParam = DBObject.GetSPParameters(SpInfomation.DBName, SpInfomation.SPName);
                        foreach (DataRow r in dsParam.Tables[0].Rows)
                        {
                            string pName = r[0].ToString().Replace("@", "");
                            if (paramList.Count(x => x == pName) == 0)
                                paramList.Add(pName);
                            //sbRequestCode.AppendLine("\tpublic string " + pName + " { get; set; }");
                        }
                        SpInfomation.RequestPropertyList = paramList;
                    }
                }



                if (param is ReqInfoModel)
                {
                    var reqInfoModel = param as ReqInfoModel;
                    StringBuilder sbmessage = new StringBuilder();
                    sbmessage.AppendLine("\tvar req = new " + reqInfoModel.RequestName + "();");
                    foreach (string properynm in SpInfomation.RequestPropertyList)
                    {
                        sbmessage.AppendLine("\treq." + properynm + " = " + "p" + properynm +";");
                    }
                    sbmessage.AppendLine("\tvar data = Context.Proxy.Post<" + reqInfoModel.ResponseName + ">(req);");
                    foreach (var modelName in reqInfoModel.ModelNameList)
                        sbmessage.AppendLine("\t" + modelName + "ListProprety =  new List<" + modelName + ">(data." + modelName + "List );");
                    this.RequestCode = sbmessage.ToString();
                    return;
                }

                if ( param is List<ReqInfoModel> )
                {
                    var reqInfoModelList = param as List<ReqInfoModel>;
                    if (reqInfoModelList.Count == 0) return;
                    StringBuilder sbmessage = new StringBuilder();

                    sbmessage.AppendLine("\tvar req = new " + reqInfoModelList[0].RequestName + "();");
                    foreach (string properynm in SpInfomation.RequestPropertyList)
                    {
                        sbmessage.AppendLine("\treq." + properynm + " = " + "p" + properynm+";");
                    }
                    sbmessage.AppendLine("\tvar data = Context.Proxy.Post<" + reqInfoModelList[0].ResponseName + ">(req);");
                    foreach (var reqinfo in reqInfoModelList)
                    {
                        foreach (var modelName in reqinfo.ModelNameList)
                        {
                            if(string.IsNullOrEmpty(modelName))
                                sbmessage.AppendLine("\tint rowCnt = data.ReturnValue);");
                            else
                                sbmessage.AppendLine("\t" + modelName + "ListProprety =  new List<" + modelName + ">(data." + modelName + "List );");
                        }
                    }
                    this.RequestCode = sbmessage.ToString();
                    return;

                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }


        } 
        #endregion
    }

    public class SPReqInfoModel
    {
        public string FacadeLocation { get; set; }
        public string FacadeNameSpace { get; set; }
        public string FacadeClassNM { get; set; }
        public string FacadeMethodNM { get; set; }

        List<ReqInfoModel> _ReqInfoModelList = new List<ReqInfoModel>();
        public List<ReqInfoModel> ReqInfoModelList {
            get { return _ReqInfoModelList; }
            set
            {
                _ReqInfoModelList = value; 
            }
        }
    }
    public class ReqInfoModel
    {
       public string ID{get;set;}
       public string SPName{get;set;}
       private List<string> modelName = new List<string>();
       public List<string> ModelNameList { get { return modelName; } }
        public string RequestName	{get;set;}
        public string ResponseName	{get;set;}
        public string Location	{get;set;}
        public string UserName	{get;set;}
        public string FacadeNameSpace { get; set; }
        public string FacadeClassNM { get; set; }
        public string FacadeMethodNM { get; set; }
    }
}
