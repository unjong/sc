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
        }
	}
}
