using CsFormAnalyzer.Data;
using CsFormAnalyzer.Utils;
using CsFormAnalyzer.Mvvm;
using CsFormAnalyzer.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using SC.WPF.Tools.CodeHelper;
using System.Text.RegularExpressions;

namespace CsFormAnalyzer.Core
{
    public class WorkService : Singleton<WorkService>
    {
        private WorkService() { }

        public DataTable GetViewBizInfo()
        {
            var conn = SqlHelper.GetConnection(AppManager.DataConnectionString);
            var query = "SELECT * FROM VIEW_BIZ_INFO";
            return SqlHelper.GetDataTable(conn, query);
        }

        internal DataTable GetTblSpModel()
        {
            var conn = SqlHelper.GetConnection(AppManager.DataConnectionString);
            var query = "SELECT * FROM TBL_SP_MODEL";
            return SqlHelper.GetDataTable(conn, query);
        }

        public string GetFullFilePath(string ns, string className)
        {
            if (string.IsNullOrEmpty(ns)) return null;
            var targetFile = ViewModelLocator.Current.CallTreeVM.TargetFile;
            if (string.IsNullOrEmpty(targetFile))
                throw new Exception("타겟 파일을 지정해야 합니다.");

            var basePath = targetFile.LeftBySearch("WinUI");

            // D:\720.YUHS\His2\HIS2.0\Source\WinUI\SP\PHA\HIS.WinUI.SP.PHA.CL.Rs
            var nsArray = ns.Split('.');            
            string path;
            if (nsArray.Where(p => p.Contains("WinUI", "Controller")).Count() > 0)
            {
                path = string.Format(@"{0}\{1}\{2}\{3}\{4}.cs",
                    nsArray.ElementAt(1),
                    nsArray.ElementAt(2),
                    nsArray.ElementAt(3),
                    ns,
                    className);
            }
            else if (nsArray.Where(p => p.Equals("FACADE", StringComparison.CurrentCultureIgnoreCase)).Count() > 0)
            {
                path = string.Format(@"{0}\{1}\{2}\{3}.cs",
                    nsArray.ElementAt(1),
                    nsArray.ElementAt(2),
                    ns,
                    className);
            }
            else // BIZ, DA
            {
                path = string.Format(@"COM\{0}\{1}\{2}\{3}\{4}.cs",
                    nsArray.ElementAt(1),
                    nsArray.ElementAt(2),
                    nsArray.ElementAt(3),
                    ns,
                    className);
            }

            path = System.IO.Path.Combine(basePath, path);

            return path;
        }

        internal void SaveSpModel(string spName, string modelName, string requestName, string responseName, string location, string facadeNamespace, string facadeClassNm, string facadeMethodNm)
        {
            var sqlparams = new SqlParameter[9];
            sqlparams[0] = new SqlParameter("@SPName", spName);
            sqlparams[1] = new SqlParameter("@ModelName", modelName);
            sqlparams[2] = new SqlParameter("@RequestName", requestName);
            sqlparams[3] = new SqlParameter("@ResponseName", responseName);
            sqlparams[4] = new SqlParameter("@Location", location);
            sqlparams[5] = new SqlParameter("@facadeNMSPace", facadeNamespace);
            sqlparams[6] = new SqlParameter("@facadeCLSNM", facadeClassNm);
            sqlparams[7] = new SqlParameter("@facadeMethodNM", facadeMethodNm);
            sqlparams[8] = new SqlParameter("@UserName", System.Security.Principal.WindowsIdentity.GetCurrent().Name);

            DataAccess.GetDataSet2(AppManager.DataConnectionString, @"[USP_SAVE_SP_MODEL]", sqlparams, CommandType.StoredProcedure);
        }

        public string GetHis3DbName(string dbCategoryName)
        {
            string dbName = dbCategoryName;

            switch (dbCategoryName)
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

            return dbName;
        }

        public string GetNamespaceByFilePath(string path)
        {
            var match = Regex.Match(path, @"\\(HIS\.[^\\]+)");
            if (match.Success != true)
                throw new NotSupportedException("Namespace 를 추측 할 수 없습니다.");

            return match.Groups[1].Value;
        }

        internal IEnumerable<string> GetPropertyNames(string code)
        {
            var pattern = @"public\s+[\w\.]+\s+(\w+)\s+{";
            foreach (Match m in Regex.Matches(code, pattern))
            {
                yield return m.Groups[1].Value;
            }
        }
    }
}
