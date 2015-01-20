using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace SC.WPF.Tools.CodeHelper
{
    /// <summary>
    /// DataAccess에 대한 요약 설명입니다.
    /// </summary>
    public class DataAccess
    {

        public DataAccess()
        {
            //
            // TODO: 여기에 생성자 논리를 추가합니다.
            //
        }

        public static void SetConnectionInfo(string DatabaseName, string strcon)
        {
            Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\SOCEN\\CodeHelper\\CONNECTION_STRING", DatabaseName, strcon);
        }
        public static void DeleteConnectionInfo(string DataBaseName)
        {
            Microsoft.Win32.RegistryKey SoftwareKey = null;
            Microsoft.Win32.RegistryKey INTERDEV = null;
            Microsoft.Win32.RegistryKey CodeHelper = null;
            Microsoft.Win32.RegistryKey CONNECTION_STRING = null;

            try
            {
                SoftwareKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software");
                INTERDEV = SoftwareKey.OpenSubKey("SOCEN");
                if (INTERDEV != null)
                {
                    CodeHelper = INTERDEV.OpenSubKey("CodeHelper");
                    if (CodeHelper != null)
                    {
                        CONNECTION_STRING = CodeHelper.OpenSubKey("CONNECTION_STRING", Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree);
                        if (CONNECTION_STRING != null)
                        {
                            CONNECTION_STRING.DeleteValue(DataBaseName, false);
                        }
                    }
                }

            }
            finally
            {
                if (CONNECTION_STRING != null) CONNECTION_STRING.Close();
                if (CodeHelper != null) CodeHelper.Close();
                if (INTERDEV != null) INTERDEV.Close();
                if (SoftwareKey != null) SoftwareKey.Close();
            }
        }
        public static string[] GetDBList()
        {
            Microsoft.Win32.RegistryKey SoftwareKey = null;
            Microsoft.Win32.RegistryKey INTERDEV = null;
            Microsoft.Win32.RegistryKey CodeHelper = null;
            Microsoft.Win32.RegistryKey CONNECTION_STRING = null;

            string[] strReturn = null;

            try
            {
                SoftwareKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software");
                INTERDEV = SoftwareKey.OpenSubKey("SOCEN");
                if (INTERDEV != null)
                {
                    CodeHelper = INTERDEV.OpenSubKey("CodeHelper");
                    if (CodeHelper != null)
                    {
                        CONNECTION_STRING = CodeHelper.OpenSubKey("CONNECTION_STRING", Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree);
                        if (CONNECTION_STRING != null)
                        {
                            strReturn = CONNECTION_STRING.GetValueNames();
                        }
                    }
                }
            }
            finally
            {
                if (CONNECTION_STRING != null) CONNECTION_STRING.Close();
                if (CodeHelper != null) CodeHelper.Close();
                if (INTERDEV != null) INTERDEV.Close();
                if (SoftwareKey != null) SoftwareKey.Close();
            }

            //INHA.ITIS.Framework.Common.Log.LogToFile("d:\\inhaportal\\home\\itis\\", "rhLog22.log", alias + ", " + strReturn);

            return strReturn;
        }

        public static string GetConnStr(string DatabaseName)
        {
            string rtnValue = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Interdev\\CodeHelper\\CONNECTION_STRING", DatabaseName, "").ToString();
            string[] spliter = { "|^|" };
            return rtnValue.Split(spliter, StringSplitOptions.None)[0];
        }
        public static string GetDBType(string databaseName)
        {
            string rtnValue =Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Interdev\\CodeHelper\\CONNECTION_STRING", databaseName, "").ToString();
            string[] spliter = { "|^|" };
            return rtnValue.Split(spliter, StringSplitOptions.None)[1];
        }
        public static DataSet GetDataSet(string DB, string commandText, SqlParameter[] sqlParameters, CommandType commandType)
        {
            string constring = GetConnStr(DB);
            return GetDataSet2(constring, commandText, sqlParameters, commandType);
        }

        public static DataSet GetDataSet2(string connectionString, string commandText, SqlParameter[] sqlParameters, CommandType commandType)
        {
            SqlConnection con = null;
            SqlCommand cmd = null;
            SqlDataAdapter da = null;
            DataSet dsReturn = null;

            try
            {
                cmd = new SqlCommand();
                cmd.CommandText = commandText;
                cmd.CommandType = commandType;
                if (sqlParameters != null)
                {
                    foreach (SqlParameter param in sqlParameters)
                    {
                        AddParameter(cmd, param);
                    }
                }

                con = new SqlConnection(connectionString);
                con.Open();

                cmd.Connection = con;

                dsReturn = new DataSet();
                da = new SqlDataAdapter(cmd);
                da.Fill(dsReturn);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                {
                    if (con.State == ConnectionState.Open)
                        con.Close();
                    con.Dispose();
                }
                if (cmd != null) cmd.Dispose();
                if (da != null) da.Dispose();
            }

            return dsReturn;
        }
        public static DataSet GetDataSet(string DB, string commandText, CommandType commandType)
        {
            return DataAccess.GetDataSet(DB, commandText, null, commandType);
        }
        public static DataSet GetDataSet2(string constring, string commandText, CommandType commandType)
        {
            return DataAccess.GetDataSet2(constring, commandText, null, commandType);
        }
        private static void AddParameter(SqlCommand cmd, SqlParameter param)
        {
            if ((param.Value == null) || ((param.Value.GetType().ToString().Equals("System.String")) && ((string)param.Value).Length == 0))
                param.Value = System.DBNull.Value;
            cmd.Parameters.Add(param);

        }
    }
}

