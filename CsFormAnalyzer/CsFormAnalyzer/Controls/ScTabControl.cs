using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CsFormAnalyzer.Controls
{
    public class ScTabControl : TabControl
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            var item = new ScTabItem();
            item.AddHandler(Button.ClickEvent, new RoutedEventHandler(TabItemButtonClick));
            return item;
        }

        private void TabItemButtonClick(object sender, RoutedEventArgs e)
        {
            var tabItem = sender as ScTabItem;
            if (tabItem == null) return;

            var button = e.OriginalSource as Button;
            if (button == null) return;

            if (button.Name.Equals("PART_CloseButton"))
            {
                CsFormAnalyzer.Utils.ControlHelper.RemoveSelectedItem(this);
            }
        }
    }
}
