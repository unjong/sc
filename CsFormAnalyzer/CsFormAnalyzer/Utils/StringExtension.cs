using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Utils
{
	/// <summary>
	/// string의 확장메서드를 제공합니다.
	/// </summary>
	public static class StringExtension
	{
		public static bool IsNumeric(this string value)
		{
			
			double test;
			return double.TryParse(value, out test);
		}

		public static bool IsBoolean(this string value)
		{
			
			bool test;
			return bool.TryParse(value, out test);
		}

		public static string Left(this string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value)) return value;
			maxLength = Math.Abs(maxLength);

			return value.Length <= maxLength
				? value
				: value.Substring(0, maxLength);
		}
	}
}
