using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CsFormAnalyzer.Utils;

namespace CsFormAnalyzer.ViewModels
{
    partial class CallTreeCodeVM : Mvvm.ViewModelBase
    {
        public override string Title
        {
            get
            {
                return "CallTree Code Viewer";
            }
            set
            {
                base.Title = value;
            }
        }
        [Mvvm.InstanceNew]
        public List<ItemContext> ItemsContext { get { return GetPropertyValue(); } set { SetPropertyValue(value); } }
        
        public ItemContext SelectedItem 
        {
            get
            {
                return GetPropertyValue(); 
            }
            set
            {
                SetPropertyValue(value);
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, (Action)(() =>
                {
                    value.SearchText = string.Empty;
                    value.OnPropertyChanged("SearchText");

                    value.SearchText = value.GetSearchText();
                    value.OnPropertyChanged("SearchText");
                }));                
            }
        }
    }

    partial class CallTreeCodeVM : Mvvm.ViewModelBase
    {
        public class ItemContext : Mvvm.PropertyNotifier
        {
            public CallTreeVM.CallTreeObjectItem Item { get; set; }

            public string Header
            {
                get
                {
                    return string.Format("[{0}]{1}.{2}", Item.Layer, Item.ClassName, Item.MethodName);
                }
            }

            public string Code 
            { 
                get 
                {
                    return Item.GetFullCode();
                }
                set { }
            }

            public string SearchText { get; set; }

            public string GetSearchText()
            {
                var pattern = @"(public|private) [\w]+ " + Item.MethodName + @"\(";
                var search = StringExtensions.RegexReturn(Code, pattern);
                return search;
            }
        }      
    }
}
