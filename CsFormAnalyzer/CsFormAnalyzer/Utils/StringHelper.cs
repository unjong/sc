using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Utils
{
    public static class StringHelper
    {
        public static string[] GetParams(string value)
        {
            var sb = new StringBuilder();
            char ignoreSwitchChar = default(char);

            foreach (var c in value)
            {
                if (c.Equals('"') || c.Equals('(') || c.Equals(')') || c.Equals('{') || c.Equals('}') || c.Equals('[') || c.Equals(']'))
                {
                    if (c.Equals('"'))
                    {
                        if (ignoreSwitchChar == default(char))
                            ignoreSwitchChar = c;
                        else if (ignoreSwitchChar.Equals('"'))
                            ignoreSwitchChar = default(char);
                    }
                    else if (c.Equals('(') || c.Equals(')') || c.Equals('{') || c.Equals('}') || c.Equals('[') || c.Equals(']'))
                    {
                        if (ignoreSwitchChar == default(char))
                            ignoreSwitchChar = c;
                        else if (ignoreSwitchChar.Equals('(') && c.Equals(')'))
                            ignoreSwitchChar = default(char);
                        else if (ignoreSwitchChar.Equals('{') && c.Equals('}'))
                            ignoreSwitchChar = default(char);
                        else if (ignoreSwitchChar.Equals('[') && c.Equals(']'))
                            ignoreSwitchChar = default(char);
                    }

                    sb.Append(c);
                }
                else if (c.Equals(','))
                {
                    if (ignoreSwitchChar != default(char))
                        sb.Append("@#$");
                    else
                        sb.Append(c);
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Split(',');
        }
    }
}
