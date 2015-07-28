using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Server;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace SC.WPF.Tools.CodeHelper
{
    class DBObject
    {
        private const string CONST_SPPREFIX = "UP_Save";
        private static string strDbName = string.Empty;
        private static string strSqlVersion = string.Empty;
        private static string strSpCode = string.Empty;
        private static string strSpName = string.Empty;

        public enum DBServerType
        {
            SQL2005,
            SQL2000,
            ORACLE
        }
        public static DataSet GetTableColumns(string dbName, string tableName, string sqlver)
        {

            DataSet ds = new DataSet();
            StringBuilder sbQry = new StringBuilder();
            SqlParameter[] sqlParams = new SqlParameter[1];

            if (sqlver == DBServerType.SQL2005.ToString())
            {
                sbQry.Append("SELECT a.[name] as paramName, b.[name] as DataType , a.max_length as length, status='', a.is_nullable as isnullable");
                sbQry.Append(" FROM sys.columns a JOIN sys.types b ON a.system_type_id = b.system_type_id AND a.user_type_id = b.user_type_id ");
                sbQry.Append(" AND a.system_type_id = b.system_type_id AND a.user_type_id = b.user_type_id ");
                sbQry.Append(" WHERE [object_id]  = object_id(@id)");
                sbQry.Append(" ORDER BY a.column_id");
            }
            else if (sqlver == DBServerType.SQL2000.ToString())
            {
                sbQry.Append("SELECT a.[name] as paramName, b.[name] as DataType, a.length, a.status, a.isnullable");
                sbQry.Append(" FROM dbo.syscolumns a JOIN dbo.systypes b ON a.xtype = b.xtype AND a.xtype = b.xusertype");
                sbQry.Append(" WHERE a.[id] = (select  id   from dbo.sysobjects where name=@id)");
                sbQry.Append(" ORDER BY a.colorder");
                tableName = tableName.Replace("dbo.", "");
            }
            sqlParams[0] = new SqlParameter("@id", SqlDbType.VarChar, 100);
            sqlParams[0].Direction = ParameterDirection.Input;
            sqlParams[0].Value = tableName;

            ds = DataAccess.GetDataSet(dbName, sbQry.ToString(), sqlParams, CommandType.Text);

            return ds;

        }

        public static DataSet GetSPParameters(string dbName, string spName, string sqlver)
        {
            DataSet ds = new DataSet();
            StringBuilder sbQry = new StringBuilder();

            if (sqlver == DBServerType.SQL2000.ToString())
            {
                sbQry.Append("SELECT a.[name] as paramName, b.[name] as DataType , a.length, a.status, a.isnullable");
                sbQry.Append(" FROM dbo.syscolumns a JOIN dbo.systypes b ON a.xtype = b.xtype AND a.xusertype = b.xusertype");
                sbQry.Append(" WHERE a.[id] = (select  id   from dbo.sysobjects where name=@id)");
                sbQry.Append(" ORDER BY a.colorder");
                spName = spName.Replace("dbo.", "");
            }
            else if (sqlver == DBServerType.SQL2005.ToString())
            {

                sbQry.Append("SELECT a.[name] as paramName, b.[name] as DataType, a.length, a.status, a.isnullable");
                sbQry.Append(" FROM dbo.syscolumns a JOIN dbo.systypes b ON a.xtype = b.xtype AND a.xusertype = b.xusertype");
                sbQry.Append(" WHERE a.[id] = object_id(@id)");
                sbQry.Append(" ORDER BY a.colorder");
            }
            SqlParameter[] sqlParams = new SqlParameter[1];
            sqlParams[0] = new SqlParameter("@id", SqlDbType.VarChar, 100);
            sqlParams[0].Direction = ParameterDirection.Input;
            sqlParams[0].Value = spName;

            ds = DataAccess.GetDataSet(dbName, sbQry.ToString(), sqlParams, CommandType.Text);

            return ds;
        }

        public static DataTable GetSPParames(string dbconstr, string spName)
        {
            System.Text.StringBuilder sbQry = new System.Text.StringBuilder();
            System.Text.StringBuilder sbResult = new System.Text.StringBuilder();
            SqlParameter[] sqlParams = new SqlParameter[1];
            DataSet ds = DBObject.GetTables(dbconstr);

            sbQry.Append("SELECT a.[name] as [parameter], b.[name] as DataType , a.length, (case a.status when 40 then 'output' when 8 then 'input' end) as direction");
            sbQry.Append(" FROM dbo.syscolumns a JOIN dbo.systypes b on a.xtype = b.xtype and a.xusertype = b.xusertype");
            sbQry.Append(" WHERE a.[id] = object_id(@objname)");
            sbQry.Append(" ORDER BY a.colorder");

            sqlParams[0] = new SqlParameter("@objname", SqlDbType.NVarChar, 50);
            sqlParams[0].Direction = ParameterDirection.Input;
            sqlParams[0].Value = spName;

            ds = DataAccess.GetDataSet2(dbconstr, sbQry.ToString(), sqlParams, CommandType.Text);
            return ds.Tables[0];
        }



        public static DataSet GetSPParameters(string connectionString, string spName)
        {
            DataSet ds = new DataSet();
            StringBuilder sbQry = new StringBuilder();

            sbQry.Append("SELECT a.[name] as paramName, b.[name] as DataType, a.length, a.status, a.isnullable");
            sbQry.Append(" FROM dbo.syscolumns a JOIN dbo.systypes b ON a.xtype = b.xtype AND a.xusertype = b.xusertype");
            sbQry.Append(" WHERE a.[id] = object_id(@id)");
            sbQry.Append(" ORDER BY a.colorder");

            SqlParameter[] sqlParams = new SqlParameter[1];
            sqlParams[0] = new SqlParameter("@id", SqlDbType.VarChar, 100);
            sqlParams[0].Direction = ParameterDirection.Input;
            sqlParams[0].Value = spName;

            ds = DataAccess.GetDataSet2(connectionString, sbQry.ToString(), sqlParams, CommandType.Text);

            return ds;
        }


        public static DataSet GetFirstTables(string connectionstr, DBServerType sqlver)
        {

            System.Text.StringBuilder sbQry = new System.Text.StringBuilder();
            if (sqlver == DBServerType.SQL2000)
            {
                sbQry.Append("SELECT 'dbo' AS [schema_id], [name],  [id] as [object_id] FROM dbo.sysobjects");
                sbQry.Append(" WHERE [xtype] ='U'");
                sbQry.Append(" ORDER BY schema_id, name ASC ");
            }
            else if (sqlver == DBServerType.SQL2005)
            {
                sbQry.Append("SELECT SCHEMA_NAME([schema_id]) AS [schema_id], [name], [object_id] FROM sys.objects");
                sbQry.Append(" WHERE [type] = 'U'");
                sbQry.Append(" ORDER BY schema_id, name ASC");
            }
            return DataAccess.GetDataSet2(connectionstr, sbQry.ToString(), CommandType.Text);

        }

        public static DataSet GetFirstSPs(string connectionstr, DBServerType sqlver)
        {
            System.Text.StringBuilder sbQry = new System.Text.StringBuilder();
            if (sqlver == DBServerType.SQL2000)
            {
                sbQry.Append("SELECT 'dbo' AS [schema_id], [name],  [id] as [object_id] FROM dbo.sysobjects");
                sbQry.Append(" WHERE [xtype] ='P'");
                sbQry.Append(" ORDER BY schema_id, name ASC");
            }
            else if (sqlver == DBServerType.SQL2005)
            {
                sbQry.Append("SELECT SCHEMA_NAME(schema_id) as [schema_id] ,[name], [object_id] FROM sys.objects");
                sbQry.Append(" WHERE [type] = 'P'");
                sbQry.Append(" ORDER BY schema_id, name ASC");
            }
            return DataAccess.GetDataSet2(connectionstr, sbQry.ToString(), CommandType.Text);
        }

        public static DataSet GetTables(string db, string sqlver)
        {

            System.Text.StringBuilder sbQry = new System.Text.StringBuilder();
            if (sqlver == DBServerType.SQL2000.ToString())
            {
                sbQry.Append("SELECT 'dbo' AS [schema_id], [name],  [id] as [object_id] FROM dbo.sysobjects");
                sbQry.Append(" WHERE [xtype] ='U'");
                sbQry.Append(" ORDER BY schema_id, name ASC");

            }
            else if (sqlver == DBServerType.SQL2005.ToString())
            {

                sbQry.Append("SELECT SCHEMA_NAME([schema_id]), [name], [object_id] FROM sys.objects");
                sbQry.Append(" WHERE [type] = 'U'");
                sbQry.Append(" ORDER BY schema_id, name ASC");


            }
            return DataAccess.GetDataSet(db, sbQry.ToString(), CommandType.Text);

        }

        public static DataSet GetTables(string connecString)
        {
            System.Text.StringBuilder sbQry = new System.Text.StringBuilder();
            sbQry.Append("SELECT SCHEMA_NAME([schema_id]), [name], [object_id] FROM sys.objects");
            sbQry.Append(" WHERE [type] = 'U'");
            sbQry.Append(" ORDER BY schema_id, name ASC");

            return DataAccess.GetDataSet2(connecString, sbQry.ToString(), CommandType.Text);
        }


        public static DataSet GetSPs(string db, string sqlver)
        {
            System.Text.StringBuilder sbQry = new System.Text.StringBuilder();
            if (sqlver == DBServerType.SQL2000.ToString())
            {
                sbQry.Append("SELECT 'dbo' AS [schema_id], [name],  [id] as [object_id] FROM dbo.sysobjects");
                sbQry.Append(" WHERE [xtype] ='P'");
                sbQry.Append(" ORDER BY schema_id, name ASC");
            }
            else if (sqlver == DBServerType.SQL2005.ToString())
            {
                sbQry.Append("SELECT SCHEMA_NAME(schema_id),[name],[object_id] FROM sys.objects");
                sbQry.Append(" WHERE [type] = 'P'");
                sbQry.Append(" ORDER BY schema_id, name ASC");
            }
            return DataAccess.GetDataSet(db, sbQry.ToString(), CommandType.Text);
        }

        public static DataSet GetSPs(string connectionString)
        {
            System.Text.StringBuilder sbQry = new System.Text.StringBuilder();

                sbQry.Append("SELECT SCHEMA_NAME(schema_id),[name],[object_id] FROM sys.objects");
                sbQry.Append(" WHERE [type] = 'P'");
                sbQry.Append(" ORDER BY schema_id, name ASC");
            return DataAccess.GetDataSet2(connectionString, sbQry.ToString(), CommandType.Text);
        }



        public static void MakeSP(string dbName, string sqlver, string spcode, string spName)
        {
            string sQry = string.Empty;
            string strName = string.Empty;

            string[] findSPName = spName.Split('.');
            strName = findSPName[findSPName.Length - 1];

            if (sqlver == DBServerType.SQL2005.ToString())
            {
                // SP 존재여부 확인 
                sQry = "SELECT [name] FROM sys.objects WHERE NAME = @name AND TYPE = 'P'";
                SqlParameter[] sqlParams = new SqlParameter[1];
                DataSet ds = null;
                sqlParams[0] = new SqlParameter("@name", SqlDbType.VarChar, 50);
                sqlParams[0].Value = strName;



                ds = DataAccess.GetDataSet(dbName, sQry, sqlParams, CommandType.Text);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    MessageBox.Show("해당 SP가 존재합니다.");
                    return;
                }
                else
                {
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(DataAccess.GetConnStr(dbName)))
                        {
                            SqlCommand command = new SqlCommand(spcode, connection);
                            command.Connection.Open();
                            command.ExecuteNonQuery();
                        }
                        MessageBox.Show("SP가 정상적으로 생성되었습니다.");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("SP 생성 중 오류가 발생하였습니다.");
                    }
                    finally
                    {
                    }
                }
            }
            else if (sqlver == DBServerType.SQL2000.ToString())
            {
                sQry = "SELECT [name] FROM dbo.sysobjects WHERE NAME = @name AND XTYPE = 'P'";
                SqlParameter[] sqlParams = new SqlParameter[1];
                DataSet ds = null;
                sqlParams[0] = new SqlParameter("@name", SqlDbType.VarChar, 50);
                sqlParams[0].Value = spName;



                ds = DataAccess.GetDataSet(dbName, sQry, sqlParams, CommandType.Text);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    MessageBox.Show("해당 SP가 존재합니다.");
                    return;
                }
                else
                {
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(DataAccess.GetConnStr(dbName)))
                        {
                            SqlCommand command = new SqlCommand(spcode, connection);
                            command.Connection.Open();
                            command.ExecuteNonQuery();
                        }
                        MessageBox.Show("SP가 정상적으로 생성되었습니다.");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("SP 생성 중 오류가 발생하였습니다.");
                    }
                    finally
                    {
                    }
                }
            }

        }


        private static void SetDbConnetion(string dbName, string sqlver, string spcode, string spName)
        {
            strDbName = dbName;
            strSqlVersion = sqlver;
            strSpCode = spcode;
            strSpCode = spName;
        }

        public static DataSet GetColumnProperties(string dbName, string sqlver, string sTableName)
        {
            StringBuilder sbQry_Col = new StringBuilder();


            if (sqlver == DBServerType.SQL2005.ToString())
            {

                // Column 및 속성 값 가져오기 
                sbQry_Col.Append("SELECT a.[name], b.[name], a.max_length as length,");
                sbQry_Col.Append(" CASE a.is_identity WHEN 1 THEN 128 END  as identityColum");
                sbQry_Col.Append(" FROM sys.columns a JOIN sys.types b ON");
                sbQry_Col.Append(" a.system_type_id = b.system_type_id AND a.user_type_id = b.user_type_id");
                sbQry_Col.Append(" WHERE a.[object_id] = object_id(@objectname)");
                sbQry_Col.Append(" ORDER BY a.column_id ");


            }
            else if (sqlver == DBServerType.SQL2000.ToString())
            {
                // Column 및 속성 값 가져오기 identity column제외 a.status = 0x80은 identity 컬럼
                sbQry_Col.Append("SELECT a.[name], b.[name], a.length, a.status as identityColum");
                sbQry_Col.Append(" FROM dbo.syscolumns a JOIN dbo.systypes b ON");
                sbQry_Col.Append(" a.xtype = b.xtype AND a.xusertype = b.xusertype");
                sbQry_Col.Append(" WHERE a.[id] = (select  id   from dbo.sysobjects where name=@objectname)");
                sbQry_Col.Append(" ORDER BY a.colid ");
                sTableName = sTableName.Replace("dbo.", "");
            }
            SqlParameter[] sqlParams_Col = new SqlParameter[1];
            sqlParams_Col[0] = new SqlParameter("@objectname", SqlDbType.VarChar, 100);
            sqlParams_Col[0].Direction = ParameterDirection.Input;
            sqlParams_Col[0].Value = sTableName;

            return DataAccess.GetDataSet(dbName, sbQry_Col.ToString(), sqlParams_Col, CommandType.Text);
        }

        public static DataSet GetPrimaryKey(string dbName, string sqlver, string sTableName)
        {
            StringBuilder sbQry_PK = new StringBuilder();

            if (sqlver == DBServerType.SQL2005.ToString())
            {
                sbQry_PK.Append("select name from sys.columns where [object_id] = object_id(@objectname) and column_id in ");
                sbQry_PK.Append(" (select b.column_id from sys.key_constraints a  join sys.index_columns b on");
                sbQry_PK.Append(" a.unique_index_id = b.index_id and a.parent_object_id = b.[object_id]");
                sbQry_PK.Append(" where a.type = 'PK' and b.[object_id] = object_id(@objectname))");

            }
            else if (sqlver == DBServerType.SQL2000.ToString())
            {
                sbQry_PK.Append("SELECT column_name as name");
                sbQry_PK.Append(" FROM  INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE a JOIN dbo.sysobjects b");
                sbQry_PK.Append(" ON a.constraint_name = b.name");
                sbQry_PK.Append(" WHERE xtype='PK' and  table_name =@objectname ");
                sTableName = sTableName.Replace("dbo.", "");

            }
            SqlParameter[] sqlParams_PK = new SqlParameter[1];
            sqlParams_PK[0] = new SqlParameter("@objectname", SqlDbType.VarChar, 100);
            sqlParams_PK[0].Direction = ParameterDirection.Input;
            sqlParams_PK[0].Value = sTableName;

            return DataAccess.GetDataSet(dbName, sbQry_PK.ToString(), sqlParams_PK, CommandType.Text);
        }



    }
}
