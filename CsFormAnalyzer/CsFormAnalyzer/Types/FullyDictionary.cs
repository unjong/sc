using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Types
{
    public class FullyDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public TValue this[TKey key]
        {
            get
            {
                if (base.ContainsKey(key))
                    return base[key];
                else
                    return CsFormAnalyzer.Utils.ScExtensions.GetDefaultValue<TValue>();
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
