using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Data
{
    public class DbUnit
    {
        public System.Data.SqlClient.SqlConnection Connection { get; set; }

        public DbUnit(System.Data.SqlClient.SqlConnection conn)
        {
            this.Connection = conn;
        }

        public int ExecuteNonQuery(string query)
        {
            return SqlHelper.ExecuteNonQuery(this.Connection, query);
        }
    }
}
