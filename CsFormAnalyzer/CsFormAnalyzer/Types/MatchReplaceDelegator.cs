using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsFormAnalyzer.Utils;

//namespace CsFormAnalyzer.Regular
namespace CsFormAnalyzer.Types
{
    public class MatchReplaceDelegator : Mvvm.PropertyNotifier
    {   
        public MatchReplaceDelegator()
        {
        }

        public MatchReplaceDelegator(string pattern, string replacement)
        {
            this.Pattern = pattern;
            this.Replacement = replacement;
        }

        public string Pattern { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public string Replacement { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        public RegexOptions RegexOptions { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }

        public string Replace(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            if (string.IsNullOrEmpty(Replacement))
            {
                if (Regex.IsMatch(value, Pattern, RegexOptions))
                    return Regex.Replace(value, Pattern, m => MatchLogic(m, value), RegexOptions);
                else
                    return value;
            }                
            else
            {
                try
                {
                    return Regex.Replace(value, Pattern, Replacement, RegexOptions);
                }
                catch (Exception)
                {
                    return value.Replace(Pattern, Replacement);
                }
            }
        }

        public delegate string MatchLogicDelegate(Match match, string value);
        public MatchLogicDelegate MatchLogic { get; set; }
    }
}
