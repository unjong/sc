using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace CsFormAnalyzer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

            //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ko-kr");
            //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ko-kr");
		}

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var cbo = sender as System.Windows.Controls.ComboBox;
                if (cbo.Text == "한글")
                {
                    Mvvm.ViewModelBase.ScRsLocator.RaiseCultureChanged(new System.Globalization.CultureInfo("ko-kr"));
                }
                else
                {
                    Mvvm.ViewModelBase.ScRsLocator.RaiseCultureChanged(new System.Globalization.CultureInfo("en-US"));
                }
            }
            catch (Exception)
            {

            }
        }
	}
}

