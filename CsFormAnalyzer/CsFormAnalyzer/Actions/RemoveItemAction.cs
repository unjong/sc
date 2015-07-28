using CsFormAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace CsFormAnalyzer.Actions
{
    public class RemoveItemAction : TriggerAction<DependencyObject>
    {
        protected override void Invoke(object parameter)
        {
            var itemsControl = ControlHelper.FindVisualParent<ItemsControl>(this.AssociatedObject);
            ControlHelper.RemoveItem(itemsControl, ((FrameworkElement)this.AssociatedObject).DataContext);            
        }
    }
}
