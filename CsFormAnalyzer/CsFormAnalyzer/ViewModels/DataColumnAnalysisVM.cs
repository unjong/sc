using CsFormAnalyzer.Core;
using CsFormAnalyzer.Mvvm;
using CsFormAnalyzer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CsFormAnalyzer.ViewModels
{
	class DataColumnAnalysisVM : ViewModelBase
	{
		public string TargetFile { get { return _TargetFile; } set { _TargetFile = value; OnPropertyChanged(); } }
		private string _TargetFile;

		public List<System.Data.DataTable> ResultTables { get { return _ResultTables; } set { _ResultTables = value; OnPropertyChanged(); } }
		private List<System.Data.DataTable> _ResultTables;

		public void Run()
		{
			try
			{
				//var path = @"D:\720 연세의료원\His2\HIS2.0\Source\WinUI\HP\ZZZ\HIS.WinUI.HP.ZZZ.SOC\SocSupPatDF.cs";
				//var path = @"D:\720 연세의료원\His2\HIS2.0\Source\WinUI\SP\PHA\HIS.WinUI.SP.PHA.CM.Com\DrgSearchDF.cs";
				var path = TargetFile;
				var gp = new GridColumnInfoParser(path);				
				this.ResultTables = gp.Parse();
			}
			catch (Exception ex)
			{
                System.Windows.MessageBox.Show(ex.SxGetErrorMessage());
			}
		}
	}
}
