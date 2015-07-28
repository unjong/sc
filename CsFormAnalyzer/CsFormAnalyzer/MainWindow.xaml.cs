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
                        
            cTabContorl.SelectedIndex = Convert.ToInt32(AppManager.Current.Settings.Get(cTabContorl.Name + ".SelectedIndex"));
            cTabContorl.SelectionChanged += cTabContorl_SelectionChanged;            
            //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ko-kr");
            //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ko-kr");

            //folderExplorer.SelectedPathChanged += folderExplorer_SelectedPathChanged;

            //var selectedPath = AppManager.Current.Settings.Get("folderExplorer.SelectedPath");
            //if (selectedPath != null)
            //    folderExplorer.SelectedPath = selectedPath;                        
		}

        void cTabContorl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppManager.Current.Settings.Set(cTabContorl.Name + ".SelectedIndex", cTabContorl.SelectedIndex.ToString());
        }

        //private void folderExplorer_SelectedPathChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    AppManager.Current.Settings.Set("folderExplorer.SelectedPath", e.NewValue as string);
        //}

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

