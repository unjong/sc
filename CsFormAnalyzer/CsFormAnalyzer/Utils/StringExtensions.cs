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
	public static class StringExtensions
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
		/// 문자열 배열을 연결하고 각 원소 사이에 구분자를 포함하여 가져옵니다.
		/// 최초값이 null, empty의 경우 separator을 붙이지 않습니다.
        /// "aa".ConcatDiv("/", "bb", "cc") return "aa/bb/cc"
        /// "".ConcatDiv("/", "bb", "cc") return "bb/cc"		
		/// </summary>
        public static string ConcatDiv(this string value1, string separator, params string[] value2)
		{
            var items = value2.Where(p => string.IsNullOrEmpty(p) != true);

            var result = string.IsNullOrEmpty(value1)
                ? string.Join(separator, items)
                : string.Join(separator, new string[] { value1 }.Concat<string>(items));

            return result;
		}

		/// <summary>
		/// 배열요소중 이 값이 포함되어 있는지 확인합니다.
		/// </summary>
		public static bool Contains(this string value, params string[] sources)
		{
			return sources.Contains(value);
		}

        public static bool InnerText(this string value, params string[] sources)
        {
            foreach (var source in sources)
            {
                if (value.IndexOf(source) >= 0)
                    return true;
            }

            return false;
        }

        /// <summary>
		/// (처음발견되는) a와 b 사이의 문자열을 가져옵니다.
        /// </summary>
        /// <param name="includeAB">a, b를 반환에 포함합니다.</param>
        /// <param name="lastB">b를 뒤에서 부터 판정합니다.</param>
        /// <returns>a~b 사이의 문자열</returns>
        public static string Between(this string value, string a, string b, bool includeAB = false, bool lastB = false)
		{
			if (value.IndexOf(a) < 0) return string.Empty;

            int start, end;
			start = value.IndexOf(a) + a.Length;
            if (lastB)
                end = value.LastIndexOf(b) - start;
            else
                end = value.IndexOf(b, start) - start;

            if (start < 0 || end < 0) return string.Empty;
            if (includeAB)
                return value.Substring(start - a.Length, end + (a.Length + b.Length));
            else
                return value.Substring(start, end);
		}
        
        public static string BetweenWithEmpty(this string value, string a, string b, bool includeAB = false)
        {
            var start = 0; 
            if (value.IndexOf(a) >= 0)
                start = value.IndexOf(a) + a.Length;


            var end = value.IndexOf(b, start);
            if(end<0)
            {
                end = value.IndexOf(b);
            }

            // 뒤에서 발견
            if (start > end)
                start = 0;

            if (start < 0 || end < 0) return null;
            if (includeAB)
                return value.Substring(start - a.Length, (end -start) + (a.Length + b.Length)).Trim();
            else
                return value.Substring(start, end - start).Trim();
        }

		/// <summary>
		/// (뒤에서 처음 발견되는) a와 b 사이의 문자열을 가져옵니다.
		/// </summary>
        public static string LastBetween(this string value, string a, string b, bool includeAB = false)
		{
            if (value.IndexOf(a) < 0) return string.Empty;

			var start = value.LastIndexOf(a) + a.Length;
			var end = value.IndexOf(b, start) - start;

            if (start < 0 || end < 0) return string.Empty;

            if (includeAB)
                return value.Substring(start - a.Length, end + (a.Length + b.Length));
            else
                return value.Substring(start, end);
		}

		/// <summary>
		/// 찾는 문자열을 기준으로 왼쪽 문자열을 가져옵니다.
        /// failedDefault = true : 실패시 value를 기본값으로 반환합니다.
        /// includeSearch = true : 검색문자열을 반환에 포함합니다.
		/// </summary>
        public static string LeftBySearch(this string value, string searchText, bool failedDefault = false, bool includeSearch = false)
		{
            var start = value.IndexOf(searchText);
            if (string.IsNullOrEmpty(value) || start < 0)
                return failedDefault ? value : string.Empty;

            if (includeSearch)
                return value = value.Substring(0, start + searchText.Length);
            else
                return value = value.Substring(0, start);
		}

        /// <summary>
        /// 찾는 문자열을 기준으로 왼쪽 문자열을 가져옵니다. (뒤에서부터 찾습니다.)
        /// </summary>
        public static string LastLeftBySearch(this string value, string searchText, bool failedDefault = false, bool includeSearch = false)
		{
            var start = value.LastIndexOf(searchText);
            if (string.IsNullOrEmpty(value) || start < 0)
                return failedDefault ? value : string.Empty;

            if (includeSearch)
                return value = value.Substring(0, start + searchText.Length);
            else
                return value = value.Substring(0, start);
		}

		/// <summary>
		/// 찾는 문자열을 기준으로 오른쪽 문자열을 가져옵니다. (오른쪽 부터 탐색합니다.)
        /// failedDefault = true : 실패시 value를 기본값으로 반환합니다.
        /// includeSearch = true : 검색문자열을 반환에 포함합니다.
		/// </summary>
        public static string RightBySearch(this string value, string searchText, bool failedDefault = false, bool includeSearch = false)
		{
            if (string.IsNullOrEmpty(value) || value.LastIndexOf(searchText) < 0)
                return failedDefault ? value : string.Empty;

            if (includeSearch)
                return value = value.Substring(value.LastIndexOf(searchText));
            else
                return value = value.Substring(value.LastIndexOf(searchText) + searchText.Length);
		}
        
		/// <summary>
		/// 정규식을 적용하여 결과 문자열을 가져옵니다.
		/// </summary>
		public static string RegexReturn(this string value, string pattern, int index = 0)
		{
            if (string.IsNullOrEmpty(value)) return string.Empty;

			var match = Regex.Match(value, pattern);
            return match.Success ? match.Groups[index].Value : string.Empty;
		}

        /// <summary>
        /// 지정된 입력 문자열에서 지정된 정규식과 일치하는 모든 문자열을 지정된 대체 문자열로 바꿉니다.
        /// </summary>
        public static string RegexReplace(this string value, string pattern, string replacement, RegexOptions regexOptions = RegexOptions.None)
		{
            if (string.IsNullOrEmpty(value)) return string.Empty;

            return Regex.Replace(value, pattern, replacement, regexOptions);
		}

        /// <summary>
        /// 정규식에 의한 A와B사이의 문자열을 가져옵니다.
        /// </summary>
        public static IEnumerable<string> RegexBetween(this string value, string a, string b, bool includeAB = false, bool useEscape = true)
        {
            // 포함 (?:)
            // 전방탐색 (.*?)(?=a)
            // 후방탐색 (?<=a)(.*)
            if (useEscape)
            {
                a = Regex.Escape(a);
                b = Regex.Escape(b);
            }
            string pattern = string.Empty;
            if (includeAB)
                pattern = string.Format(@"(?:{0})(.*?)(?:{1})", a, b);
            else
                pattern = string.Format(@"(?<={0})(.*?)(?={1})", a, b);

            var matches = Regex.Matches(value, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return matches.Cast<Match>().Select(p => p.Value);
        }


        /// <summary>
        /// 정규식을 이용한 전방탐색 결과를 가져옵니다.
        /// </summary>
        public static string RegexLeft(this string value, string searchText, bool includeSearchText = false)
        {
            string pattern = string.Empty;
            if (includeSearchText)
                pattern = string.Format(@"(.*)(?:{0})", Regex.Escape(searchText));
            else
                pattern = string.Format(@"(.*)(?={0})", Regex.Escape(searchText));

            var matches = Regex.Matches(value, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var match = matches.Cast<Match>().FirstOrDefault();
            if (match == null) return string.Empty;

            return match.Value;
        }

        /// <summary>
        /// 정규식을 이용한 후방탐색 결과를 가져옵니다.
        /// </summary>
        public static string RegexRight(this string value, string searchText, bool includeSearchText = false)
        {
            string pattern = string.Empty;
            if (includeSearchText)
                pattern = string.Format(@"(?:{0})(.*)", Regex.Escape(searchText));
            else
                pattern = string.Format(@"(?<={0})(.*)", Regex.Escape(searchText));

            var matches = Regex.Matches(value, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var match = matches.Cast<Match>().FirstOrDefault();
            if (match == null) return string.Empty;

            return match.Value;
        }

        public static IEnumerable<string> RegexMatches(this string target, string pattern, RegexOptions regexOptions = RegexOptions.Singleline | RegexOptions.IgnoreCase)
        {
            return Regex.Matches(target, pattern, regexOptions).Cast<Match>().Select(p => p.Value);
        }

        /// <summary>
        /// 문자열에서 숫자만 가져옵니다.
        /// </summary>
        public static int GetNumeric(this string value, bool onlyDigit = true)
        {
            //if (value.All(p => char.IsDigit(p)))
            //@"^-?\d*(\.\d+)?$"
            var numeric = value.RegexReturn(@"\d+");
            return ToInt(numeric);
        }

		/// <summary>
		/// 숫자형식으로 변환하여 가져옵니다.
		/// </summary>
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
        
        /// <summary>
        /// 지정된 인덱스가 포함된 라인을 가져옵니다.
        /// </summary>
        public static string ReadLine(this string value, int startIndex)
        {
            if (startIndex < 0) return string.Empty;

            var start = value.LastIndexOf("\n", startIndex);
            var end = value.IndexOf("\n", startIndex);
            return value.Substring(start, end - start).Trim();
        }

        public static string[] Replace(this string[] values, string oldValue, string newValue)
        {
            var list = new List<string>();
            foreach(var value in values)
            {
                list.Add(value.Replace(oldValue, newValue));
            }
            return list.ToArray();
        }

        /// <summary>
        /// Block 사이의 문자열을 가져옵니다.
        /// "aaa(bbb)cc" => "(bbb)"
        /// "aaa(bbb(cc))dd(ff)" => [0]: "(bbb(cc))" [1]: "(ff)"
        /// </summary>
        public static string[] GetBlocks(this string value, bool includeBlock = true, string blockStart = "(", string blockEnd = ")")
        {
            if (value.IndexOf(blockStart) < 0) return new string[] { };

            var list = new List<string>();
            var inline = new StringBuilder();
            int blockCount = 0;
            foreach (var c in value)
            {
                if (c.ToString().Equals(blockStart))
                {
                    if (blockCount == 0) inline.Clear();
                    if (blockCount > 0 || includeBlock && blockCount == 0) inline.Append(c);
                    blockCount++;
                }
                else if (c.ToString().Equals(blockEnd))
                {
                    blockCount--;
                    if (blockCount > 0 || includeBlock && blockCount == 0) inline.Append(c);
                    if (inline.Length < 1) list.Add("");
                }
                else
                {
                    if (blockCount > 0)
                        inline.Append(c);
                }

                if (inline.Length > 0 && blockCount == 0)
                {
                    list.Add(inline.ToString());
                    inline.Clear();
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// 카멜 표기법으로 치환하여 가져옵니다.
        /// </summary>
        public static string ToCamel(this string value, bool bStrongConvert = false)
        {
            if (string.IsNullOrEmpty(value)) return value;

            var firstString = value.First().ToString();
            if (bStrongConvert) return firstString.ToUpper() + value.Substring(1).ToLower();

            var isFirstUpper = firstString.ToUpper().Equals(firstString);
            var isAllUpper = value.ToUpper().Equals(value);

            if (isAllUpper)
            {
                // first Upper - rest Lower
                /* STRINGFORMAT > Stringformat */                
                return firstString.ToUpper() + value.Substring(1).ToLower();
            }
            else
            {
                // only first upper
                /* stringFormat > StringFormat */
                return firstString.ToUpper() + value.Substring(1);
            }

        }

        /// <summary>
        /// 지정된 문자열로 구분된 이문자열의 부분 문자열이 들어있는 문자열 배열을 반환합니다.
        /// </summary>
        public static string[] Split(this string value, string separator, StringSplitOptions stringSplitOptions = StringSplitOptions.None)
        {
            if (string.IsNullOrEmpty(value)) return new string[] { };

            return value.Split(new string[] { separator }, stringSplitOptions);
        }

        /// <summary>
        /// 지정된 문자열로 구분된 부분문자열배열의 지정된 인덱스 요소를 가져옵니다.
        /// </summary>
        public static string SplitAt(this string value, string separator, int index)
        {
            var array = value.Split(separator);
            if (array.Length <= index) return string.Empty;

            return array.ElementAt(index);
        }

        public static string Escape(this string value)
        {
            return System.Security.SecurityElement.Escape(value);
        }
    }
}
