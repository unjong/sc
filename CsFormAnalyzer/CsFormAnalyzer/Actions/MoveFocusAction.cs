using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace CsFormAnalyzer.Actions
{
    public class MoveFocusAction : TriggerAction<DependencyObject>
    {
        /*
         xmlns:i="clr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity"
         xmlns:ii="clr-namespace:Microsoft.Expression.Interactivity.Input;assembly=Microsoft.Expression.Interactions"       
         
                <i:Interaction.Triggers>                    
                    <ii:KeyTrigger Key="Enter" ActiveOnFocus="True">
                        <a:MoveFocusAction />
                    </ii:KeyTrigger>
                </i:Interaction.Triggers>
         */

        protected override void Invoke(object parameter)
        {
            if (FocusElement == null)
                ((UIElement)base.AssociatedObject).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            else
                FocusManager.SetFocusedElement(base.AssociatedObject, this.FocusElement);
        }

        public UIElement FocusElement
        {
            get { return (UIElement)GetValue(FocusElementProperty); }
            set { SetValue(FocusElementProperty, value); }
        }

        public static readonly DependencyProperty FocusElementProperty = DependencyProperty.Register("FocusElement", typeof(UIElement), typeof(MoveFocusAction), new PropertyMetadata());
    }
}
