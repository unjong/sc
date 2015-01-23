using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Utils
{
	/// <summary>
	/// string의 확장메서드를 제공합니다.
	/// </summary>
	public static class StringExtension
	{
		/// <summary>
		/// 값이 숫자형식인지 확인합니다.
		/// </summary>
		public static bool IsNumeric(this string value)
		{	
			double test;
			return double.TryParse(value, out test);
		}

		/// <summary>
		/// 값이 boolean형식인지 확인합니다.
		/// </summary>
		public static bool IsBoolean(this string value)
		{
			
			bool test;
			return bool.TryParse(value, out test);
		}

		/// <summary>
		/// 문자열의 왼쪽부터 갯수만큼 가져옵니다.
		/// </summary>
		public static string Left(this string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value)) return value;
			maxLength = Math.Abs(maxLength);

			return value.Length <= maxLength
				? value
				: value.Substring(0, maxLength);
		}

		/// <summary>
		/// 문자열을 연결하여 가져옵니다.
		/// 최초값이 null, empty의 경우 separator을 붙이지 않습니다.
		/// JoinPost("aa", ".", "bb", "cc") return "aa.bb.cc"
		/// JoinPost(null, ".", "bb", "cc") return "bb.cc"		
		/// </summary>
		public static string JoinPost(this string value1, string separator, string value2)
		{
			return string.IsNullOrEmpty(value1)
				? string.Join(separator, value1, value2).Substring(separator.Length)
				: string.Join(separator, value1, value2);
		}

		/// <summary>
		/// sources 배열요소에 이 값의 포함여부를 확인합니다.
		/// </summary>
		public static bool Contains(this string value, params string[] sources)
		{
			return sources.Contains(value);
		}


		/// <summary>
		/// a와 b 사이의 문자열을 가져옵니다.
		/// </summary>
		public static string Between(this string value, string a, string b)
		{
			//var regex = string.Format(@"{0}[\w\.\d\(\)]+{1}", Regex.Escape(a), Regex.Escape(b));
			//var match = Regex.Match(value, regex);
			//if (match.Success != true) return null;
			//return match.Value.Substring(a.Length, match.Value.Length - (a.Length + b.Length));
			if (value.IndexOf(a) < 0) return null;

			var start = value.IndexOf(a) + a.Length;
			var end = value.IndexOf(b, start);

			if (start < 0 || end < 0) return null;			
			return value.Substring(start, end - start);
		}


		/// <summary>
		/// 뒤에서부터 문자열을 찾아 그 이전까지의 문자열을 가져옵니다.
		/// </summary>
		public static string Last(this string value, string search)
		{
			return value = value.Substring(value.LastIndexOf(search) + search.Length);
		}

		/// <summary>
		/// 정규식을 적용하여 결과 문자열을 가져옵니다.
		/// </summary>
		public static string RegexReturn(this string value, string pattern)
		{
			var match = Regex.Match(value, pattern);
			return match.Success ? match.Value : null;
		}

		public static int ToInt(this string value, int defaultValue = default(int))
		{
			if (string.IsNullOrEmpty(value)) return defaultValue;

			try
			{
				return Convert.ToInt32(value);
			}
			catch
			{
				return defaultValue;
			}
			
		}

	}
}
