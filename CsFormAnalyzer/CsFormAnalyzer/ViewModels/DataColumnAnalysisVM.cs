using CsFormAnalyzer.Core;
using CsFormAnalyzer.Mvvm;
using CsFormAnalyzer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
#if !DEBUG
			try
			{
#endif
				//var path = @"D:\720 연세의료원\His2\HIS2.0\Source\WinUI\HP\ZZZ\HIS.WinUI.HP.ZZZ.SOC\SocSupPatDF.cs";
				//var path = @"D:\720 연세의료원\His2\HIS2.0\Source\WinUI\SP\PHA\HIS.WinUI.SP.PHA.CM.Com\DrgSearchDF.cs";
				var path = TargetFile;
				var gp = new GridColumnInfoParser(path);
				var resultTables = gp.Parse();
								
				var ssa = new SpreadSheetAnalysis(path);
				ssa.TableSchema = gp.GetDataTable();
				var bSuccess = ssa.Parse();
				if (bSuccess)
				{
					resultTables.AddRange(ssa.ResultTables);
				}

				foreach (var dt in resultTables.ToArray())
				{
					// Row가 없는 테이블은 제거합니다.
					if (dt.Rows.Count < 1)
					{
						resultTables.Remove(dt);
						continue;
					}

					// 값을 보정합니다.
					foreach(DataRow row in dt.Rows)
					{
						if (row.IsNull("Binding")) row["Binding"] = row.ToStr("Title").Replace(" ", "");
						row["CellType"] = row.ToStr("CellType").Replace("HIS.WinUI.Controls.Spread.", "");
						row["HorizontalAlignment"] = row.ToStr("HorizontalAlignment").Replace("HIS.WinUI.Controls.Spread.", "");
						row["DataType"] = row.ToStr("DataType").Replace("System.", "");
					}
				}

				this.ResultTables = resultTables;
#if !DEBUG
			}
			catch (Exception ex)
			{
                System.Windows.MessageBox.Show(ex.SxGetErrorMessage());
			}
#endif
		}
	}
}
