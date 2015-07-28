using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Utils
{
	public static class DataExtensions
	{
        private const string string_empty = "";
		/// <summary>
		/// 문자열 형식으로 변환하여 가져옵니다.
		/// </summary>
		public static string ToStr(this DataRow dr, string column, string defaultValue = string_empty)
		{
			if (dr == null) return defaultValue;

			try
			{
                var value = Convert.ToString(dr[column]) ?? defaultValue;
                return value;
			}
			catch (Exception ex)
			{
				if (dr.Table.Columns.Contains(column) != true)
					return defaultValue;

				throw ex;
			}
		}

		/// <summary>
		/// 숫자 형식으로 변환하여 가져옵니다.
		/// </summary>
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

		/// <summary>
		/// 지정된 컬럼열로 Distinct 결과 테이블을 가져옵니다.
		/// </summary>
		public static DataTable Distinct(this DataTable dt, params string[] columnNames)
		{			
			var dv = new DataView(dt);
			return dv.ToTable(true, columnNames);
		}

		/// <summary>
		/// 지정된 컬럼열로 Distinct 결과 테이블을 가져옵니다.
		/// </summary>
		public static DataTable Distinct(this IEnumerable<DataRow> rows, params string[] columnNames)
		{			
            return rows.CopyToDataTable().Distinct(columnNames);
		}
	}
}
