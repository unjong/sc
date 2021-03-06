﻿using CsFormAnalyzer.ViewModels;
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

namespace CsFormAnalyzer.Views
{
	/// <summary>
	/// Interaction logic for CallTreeView.xaml
	/// </summary>
	public partial class CallTreeView : UserControl
	{
		public CallTreeView()
		{
			InitializeComponent();
		}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BizCallTreeAnalysisView pv = new BizCallTreeAnalysisView();
            pv.DataContext = new BizCallTreeAnalysisVM();
            pv.ShowDialog();
        }
	}
}
