using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CsFormAnalyzer.Foundation
{
    public class AutoTemplateSelector : DataTemplateSelector
    {
        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            if (item == null) return base.SelectTemplate(item, container);

            var key = string.Join(".", item.GetType().Name, container.GetType().Name);
            return Application.Current.Resources[key] as DataTemplate 
                ?? base.SelectTemplate(item, container);
        }
    }
}
