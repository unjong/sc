using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Utils
{
	public class IOHelper
	{
		public static string ReadFile(string path)
		{
			var sb = new StringBuilder();
			using (var sr = new StreamReader(path, Encoding.Default, true))
			{
				var line = sr.ReadToEnd();
				sb.Append(line);
			}
			return sb.ToString();
		}
	}
}
