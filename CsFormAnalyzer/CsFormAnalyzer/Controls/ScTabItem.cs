using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CsFormAnalyzer.Controls
{
    [TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
    public class ScTabItem : TabItem
    {
        static ScTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScTabItem), new FrameworkPropertyMetadata(typeof(ScTabItem)));
        }

        private Button PART_CloseButton;

        public ScTabItem()
        {
            DefaultStyleKey = typeof(ScTabItem);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_CloseButton = GetTemplateChild("PART_CloseButton") as Button;
        }
    }
}
