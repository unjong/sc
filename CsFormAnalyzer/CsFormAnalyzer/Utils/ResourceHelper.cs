using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Utils
{
    public static class ResourceHelper
    {
        public static IEnumerable<DictionaryEntry> ReadResourceFile(string path)
        {
            ResXResourceReader reader = new ResXResourceReader(path);
            reader.UseResXDataNodes = true;
            return reader.Cast<DictionaryEntry>();
        }

        /// <summary>
        /// 리소스파일을 업데이트 합니다.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="path"></param>
        public static void UpdateResourceFile(Dictionary<string, string> data, string path)
        {
            var resourceEntries = new Dictionary<string, string>();

            ResXResourceReader reader = new ResXResourceReader(path);            
            reader.UseResXDataNodes = true;
            ResXResourceWriter resourceWriter = new ResXResourceWriter(path);
            System.ComponentModel.Design.ITypeResolutionService typeres = null;
            if (reader != null)
            {
                IDictionaryEnumerator id = reader.GetEnumerator();
                foreach (DictionaryEntry d in reader)
                {                    
                    string val = "";
                    if (d.Value == null)
                        resourceEntries.Add(d.Key.ToString(), "");
                    else
                    {
                        val = ((ResXDataNode)d.Value).GetValue(typeres).ToString();
                        resourceEntries.Add(d.Key.ToString(), val);
                    }

                    ResXDataNode dataNode = (ResXDataNode)d.Value;

                    resourceWriter.AddResource(dataNode);

                }
                reader.Close();
            }

            Hashtable newRes = new Hashtable();
            foreach (String key in data.Keys)
            {
                if (!resourceEntries.ContainsKey(key))
                {

                    String value = data[key].ToString();
                    if (value == null) value = "";

                    resourceWriter.AddResource(key, value);
                }
            }

            resourceWriter.Generate();
            resourceWriter.Close();

        }
    }
}
