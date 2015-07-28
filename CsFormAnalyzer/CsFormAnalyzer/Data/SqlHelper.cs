using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsFormAnalyzer.Utils;

namespace CsFormAnalyzer.Data
{
    public static class SqlHelper
    {
        public static SqlConnection GetConnection(string connectionString)
        {
            var sqlConnection = new SqlConnection(connectionString);
            return sqlConnection;
        }

        public static void Fill(this DataTable dt, SqlConnection conn, string query)
        {
            var sqlCommand = new SqlCommand(query, conn);
            var sqlDataAdapter = new SqlDataAdapter(sqlCommand);

            sqlDataAdapter.Fill(dt);
        }

        public static int Update(this DataTable dt, SqlConnection conn, string updateQuery, string insertQuery = null, string deleteQuery = null)
        {
            try
            {
                var sqlDataAdapter = new SqlDataAdapter();
                sqlDataAdapter.UpdateCommand = GetCommand(updateQuery, conn);
                sqlDataAdapter.InsertCommand = GetCommand(insertQuery, conn);
                sqlDataAdapter.DeleteCommand = GetCommand(deleteQuery, conn);

                var result = sqlDataAdapter.Update(dt);
                dt.AcceptChanges();
                return result;
            }
            catch(Exception ex)
            {                
                return -1;
            }
        }

        private static SqlCommand GetCommand(string query, SqlConnection conn)
        {
            if (string.IsNullOrEmpty(query)) return null;

            var sqlCommand = new SqlCommand(query, conn);
            var parameters = query.RegexMatches(@"@[\w]+");
            foreach (var p in parameters)
            {
                sqlCommand.Parameters.Add(new SqlParameter() { ParameterName = p, SourceColumn = p.Substring(1) });
            }

            return sqlCommand;
            
        }

        public static DataTable GetDataTable(SqlConnection conn, string query)
        {
            var dt = new DataTable();
            dt.Fill(conn, query);
            return dt;
        }

        public static int ExecuteNonQuery(SqlConnection sqlConnection, string cmdText)
        {
            sqlConnection.Open();

            var cmd = new SqlCommand(cmdText, sqlConnection);
            var result = cmd.ExecuteNonQuery();

            sqlConnection.Close();
            return result;
        }
    }
}
