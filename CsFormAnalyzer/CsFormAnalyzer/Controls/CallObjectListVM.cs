using CsFormAnalyzer.Mvvm;
using CsFormAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Controls
{
    public class CallObjectListVM : ViewModelBase
    {
        private DataTable _Source;
        public DataTable Source 
        {
            get
            {
                return _Source;
            }
            set
            {
                _Source = value;
                RootCallObjectItem = ConvertToCallObjectItem(value);
            }
        }


        private CallObjectItem _RootCallObjectItem;
        public CallObjectItem RootCallObjectItem { get { return _RootCallObjectItem; } set { _RootCallObjectItem = value; OnPropertyChanged(); } }

        private CallObjectItem ConvertToCallObjectItem(object source)
        {
#if DEBUG
            var dt = source as DataTable;
            if (dt == null) return null;

            var mapper = new Dictionary<string, CallObjectItem>();

            var root = new CallObjectItem();
            root.Layer = "ROOT";
            mapper.Add("ROOT", root);

            foreach (var row in dt.AsEnumerable().OrderBy(p => p.Field<int>("Lvl")))
            {
                var item = new CallObjectItem()
                {
                    ClassNM = row.ToStr("CLASSNM"),
                    MethodNM = row.ToStr("METHODNM"),
                    Layer = row.ToStr("LAYER"),
                    NMSpace = row.ToStr("NAMESPACE"),
                    Params = row.ToStr("PARAMS"),
                    CallObjName = row.ToStr("CALLOBJNM"),
                    CallObjMethod = row.ToStr("CALLFUNCNM")
                };
                
                int level = row.Field<int>("Lvl");                
                var parentKey = level == 0
                    ? "ROOT"
                    : string.Join(".", item.ClassNM, item.MethodNM).ToUpper();
                if (mapper.ContainsKey(parentKey) != true) continue;

                var key = string.Join(".", item.CallObjName, item.CallObjMethod).ToUpper();
                if (!mapper.ContainsKey(key))
                {
                    mapper.Add(key, item);
                    mapper[parentKey].Children.Add(item);
                }

                if (item.Layer.Equals("DA"))
                {
                    var spItem = new CallObjectItem()
                    {
                        Layer = "SP",
                        ClassNM = row.ToStr("CALLOBJNM")
                    };
                    item.Children.Add(spItem);
                }
            }

            return root;
#else
            return null;
#endif
        }

        public class CallObjectItem : PropertyNotifier
        {
            public CallObjectItem()
            {
                this.Children = new List<CallObjectItem>();
            }

            public string Depth
            {
                get
                {
                    return string.Join(".", ClassNM, MethodNM);
                }
            }

            public CallObjectItem Parent { get; set; }

            public List<CallObjectItem> Children { get; set; }

            public string CallObjName { get; set; }
            public string CallObjMethod { get; set; }

            public string ClassNM { get; set; }

            public string MethodNM { get; set; }

            public string NMSpace { get; set; }

            public string Params { get; set; }

            public string Layer { get; set; }

            public string SourcePath { get; set; }

            public bool IsExpanded 
            { 
                get { return GetPropertyValue(); } 
                set 
                { 
                    SetPropertyValue(value); 

                    foreach(var item in Children)
                    {
                        item.IsExpanded = value;
                    }
                } 
            }
        }
    }
}
