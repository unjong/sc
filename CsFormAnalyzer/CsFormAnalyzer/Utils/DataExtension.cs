using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Utils
{
	public static class DataExtension
	{
		public static string ToStr(this DataRow dr, string column, string defaultValue = default(string))
		{
			if (dr == null) return defaultValue;

			try
			{
				return Convert.ToString(dr[column]) ?? defaultValue;
			}
			catch (Exception ex)
			{
				if (dr.Table.Columns.Contains(column) != true)
					return defaultValue;

				throw ex;
			}
		}

		public static int ToInt(this DataRow dr, string column, int defaultValue = default(int))
		{
			if (dr == null) return defaultValue;

			try
			{
				return dr[column] == DBNull.Value ? defaultValue : (int)dr[column];
			}
			catch (Exception ex)
			{
				if (dr.Table.Columns.Contains(column) != true)
					return defaultValue;

				throw ex;
			}
		}

		public static DataTable Distinct(this DataTable dt, params string[] columnNames)
		{			
			var dv = new DataView(dt);
			return dv.ToTable(true, columnNames);
		}
	}
}
