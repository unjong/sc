using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CsFormAnalyzer
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
        public App()
        {
            Application.Current.Dispatcher.Thread.CurrentUICulture = new System.Globalization.CultureInfo("ko-kr");
            
#if !DEBUG
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif
            // CsFormAnalyzer 를 시작합니다.
            StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);

            //// Project Counter 를 시작합니다.
            //var vm = new CsFormAnalyzer.ViewModels.ProjectConterVM();
            //AppManager.Current.ShowView(typeof(CsFormAnalyzer.Views.ProjectConterView), vm);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            MessageBox.Show(exception.Message);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {            
            MessageBox.Show(e.Exception.Message);
            e.Handled = true;
        }
	}
}
