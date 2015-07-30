using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace CsFormAnalyzer.Utils
{
	public class IOHelper
	{
		/// <summary>
		/// 경로의 파일을 읽어 문자열로 반환합니다.
		/// </summary>
		public static string ReadFileToString(string path)
		{
            if (File.Exists(path) != true) return string.Empty;

			var sb = new StringBuilder();
            using (var sr = new StreamReader(path, Encoding.Default, true))
			{
				var line = sr.ReadToEnd();
				sb.Append(line);
			}
			return sb.ToString();
		}

        public static Encoding GetEncoding(string filename, Encoding defaultEncoding = null)
        {
            // *** Use Default of Encoding.Default (Ansi CodePage)
            Encoding enc;

            // *** Detect byte order mark if any - otherwise assume default
            byte[] buffer = new byte[5];
            FileStream file = new FileStream(filename, FileMode.Open);
            file.Read(buffer, 0, 5);
            file.Close();

            if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                enc = Encoding.UTF8;
            else if (buffer[0] == 0xfe && buffer[1] == 0xff)
                enc = Encoding.Unicode;
            else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
                enc = Encoding.UTF32;
            else if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
                enc = Encoding.UTF7;
            else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                // 1201 unicodeFFFE Unicode (Big-Endian)
                enc = Encoding.GetEncoding(1201);
            else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                // 1200 utf-16 Unicode
                enc = Encoding.GetEncoding(1200);
            else
                enc = defaultEncoding ?? Encoding.Default;

            return enc;
        }


        /// <summary>
        /// 리스트의 프로퍼티를 소스로하는 파일을 생성합니다.
        /// </summary>
        public static void ListToFile(IList coll, string filePath, string delimter = ",")
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }
            int length = coll.Count;

            using (System.IO.TextWriter writer = File.CreateText(filePath))
            {
                var list = new List<string>();
                for (int index = 0; index < length; index++)
                {
                    var item = coll[index];
                    var properties = item.GetType().GetProperties();
                    list.Clear();
                    foreach (var p in properties)
                    {
                        list.Add(Convert.ToString(p.GetValue(item)));
                    }
                    writer.WriteLine(string.Join(delimter, list.ToArray()));
                }
            }
        }

        public static void SaveFile(string filePath, string value)
        {
            System.IO.File.WriteAllText(filePath, value, Encoding.UTF8);

            //using (System.IO.TextWriter writer = File.CreateText(filePath))
            //{
            //    writer.Write(value);
            //}
        }

        public static string[] GetFiles(string path, params string[] patterns)
        {
            return patterns.SelectMany(p => Directory.GetFiles(path, p)).ToArray();
        }

        public static void Copy(string from, string to, bool overwrite = true)
        {
            if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(to)) != true)
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(to));

            System.IO.File.Copy(from, to, overwrite);
        }

        public static void Xcopy(string from, string to)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardInput = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;            

            startInfo.FileName = "xcopy";
            startInfo.Arguments = "\"" + from + "\"" + " " + "\"" + to + "\"" + @" /e /y /I";
            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public static bool RunCommand(string command)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.Verb = "runas";
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardInput = true;
            processInfo.RedirectStandardInput = false;
            processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        
            processInfo.FileName = "cmd.exe";
            processInfo.Arguments = command;

            try
            {
                using (Process exeProcess = Process.Start(processInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return true;
        }
    }
}
