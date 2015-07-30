using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Types
{
    public class FullyDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public TValue DefaultValue { get; set; }

        public FullyDictionary() { }

        public FullyDictionary(TValue defaultValue)
        {
            this.DefaultValue = defaultValue;
        }

        new public TValue this[TKey key]
        {
            get
            {
                if (base.ContainsKey(key))
                    return base[key];
                else if (DefaultValue == null)
                    return CsFormAnalyzer.Utils.ScExtensions.GetDefaultValue<TValue>();
                else
                {
                    TValue value;
                    return TryGetValue(key, out value) ? value : DefaultValue;
                }   
            }
            set
            {
                if (base.ContainsKey(key))
                    base[key] = value;
                else
                    base.Add(key, value);
            }
        }
    }
}
