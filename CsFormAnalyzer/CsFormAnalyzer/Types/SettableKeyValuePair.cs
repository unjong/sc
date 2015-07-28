using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace CsFormAnalyzer.Data
namespace CsFormAnalyzer.Types
{    

    /// <summary>
    /// Setter 가 있는 KeyValuePair 입니다.
    /// </summary>
    public class SettableKeyValuePair<TKey, TValue>
    {
        public SettableKeyValuePair()
        {
        }

        public SettableKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public TKey Key { get; set; }
        public TValue Value { get; set; }

    }
}
