using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Foundation
{
    public class RestoreValueAttribute : Attribute
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public RestoreValueAttribute(string key, string defaultValue)
        {
            this.Key = key;
            this.Value = AppManager.Current.Settings.Get(key, defaultValue);
        }

        public void Save(dynamic value)
        {
            AppManager.Current.Settings.Set(this.Key, Convert.ToString(value));
        }
    }
}
