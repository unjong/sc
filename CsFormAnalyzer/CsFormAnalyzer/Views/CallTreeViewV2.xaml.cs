using CsFormAnalyzer.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using CsFormAnalyzer.Utils;

namespace CsFormAnalyzer.Views
{
	/// <summary>
	/// Interaction logic for CallTreeView.xaml
	/// </summary>
	public partial class CallTreeViewV2 : UserControl
	{
        public CallTreeViewV2()
		{
			InitializeComponent();

            //AddHandler(UIElement.MouseLeftButtonUpEvent, new RoutedEventHandler(delegate
            //    {
            //        Console.Write(1);
            //    }));
		}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BizCallTreeAnalysisView pv = new BizCallTreeAnalysisView();
            pv.DataContext = new BizCallTreeAnalysisVM();
            pv.ShowDialog();
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            //{
            //    var item = ((FrameworkElement)sender).DataContext as CsFormAnalyzer.ViewModels.CallTreeVM.CallTreeObjectItem;                
            //    Console.Write(1);
            //}
        }        
    }

    public class CallTreeItemToBackground : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var isFind = System.Convert.ToBoolean(values.ElementAt(3));            

            if (isFind)
            {
                var rowItem = values.ElementAt(1) as CsFormAnalyzer.ViewModels.CallTreeVM.CallTreeItem;
                if (rowItem == null) return new SolidColorBrush(Colors.White);
                var find = System.Convert.ToString(values.ElementAt(4));
                for (int i = 0; i < 10; i++)
                {
                    var depth = rowItem.Row.ToStr(string.Format("Depth{0}", i));
                    if (string.IsNullOrEmpty(depth)) break;

                    if (depth.IndexOf(find) >= 0)
                        return new SolidColorBrush(Colors.IndianRed);
                }
            }
            else
            {
                if (values.ElementAt(0) == null) return new SolidColorBrush(Colors.White);

                var selectedItem = values.ElementAt(0) as CsFormAnalyzer.ViewModels.CallTreeVM.CallTreeObjectItem;
                var rowItem = values.ElementAt(1) as CsFormAnalyzer.ViewModels.CallTreeVM.CallTreeItem;
                if (rowItem == null) return new SolidColorBrush(Colors.White);

                if (selectedItem.Layer.Equals("SP"))
                {
                    var depth = rowItem.Row.ToStr("SP");
                    if (selectedItem.Key.Equals(depth))
                    {
                        var parent = selectedItem;
                        var bSuccess = true;
                        for (int j = 10 - 1; j >= 0; j--)
                        {
                            depth = rowItem.Row.ToStr(string.Format("Depth{0}", j));
                            if (string.IsNullOrEmpty(depth)) continue;

                            parent = parent.Parent;                            
                            if (parent.Key.Equals(depth) != true)
                            {
                                bSuccess = false;
                                break;
                            }
                        }

                        if (bSuccess) return new SolidColorBrush(Colors.IndianRed);
                    }     
                }

                for (int i = 0; i < 10; i++)
                {
                    var depth = rowItem.Row.ToStr(string.Format("Depth{0}", i));
                    if (string.IsNullOrEmpty(depth)) break;

                    if (selectedItem.Key.Equals(depth))
                    {
                        var parent = selectedItem;
                        var bSuccess = true;
                        for (int j = i - 1; j >= 0; j--)
                        {
                            parent = parent.Parent;
                            depth = rowItem.Row.ToStr(string.Format("Depth{0}", j));
                            if (parent.Key.Equals(depth) != true)
                            {
                                bSuccess = false;
                                break;
                            }
                        }

                        if (bSuccess) return new SolidColorBrush(Colors.IndianRed);
                    }                        
                }
            }

            return new SolidColorBrush(Colors.White);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}