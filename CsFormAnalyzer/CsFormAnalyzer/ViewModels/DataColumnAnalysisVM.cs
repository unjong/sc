using CsFormAnalyzer.Core;
using CsFormAnalyzer.Mvvm;
using CsFormAnalyzer.Utils;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CsFormAnalyzer.ViewModels
{
	class DataColumnAnalysisVM : ViewModelBase
    {
        #region Properties

        public string TargetFile { get { return _TargetFile; } set { _TargetFile = value; OnPropertyChanged(); } }
        private string _TargetFile;

        public List<System.Data.DataTable> ResultTables { get { return _ResultTables; } set { _ResultTables = value; OnPropertyChanged(); } }
        private List<System.Data.DataTable> _ResultTables;

        public string ColumnInitCode { get { return _ColumnInitCode; } set { _ColumnInitCode = value; OnPropertyChanged(); } }
        private string _ColumnInitCode;

        public List<System.Data.DataTable> ColumnSplitResult { get { return _ColumnSplitResult; } set { _ColumnSplitResult = value; OnPropertyChanged(); } }
        private List<System.Data.DataTable> _ColumnSplitResult;

        #endregion

        #region Commands

        public ICommand ColumnSplitCommand { get; private set; }

        public override void InitCommands()
        {
            this.ColumnSplitCommand = base.CreateCommand(delegate
            {
                ParseColumnInitCode();
            });
        }
        
        #endregion

        #region Methods

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

            AmendTable(resultTables);

            this.ResultTables = resultTables;
#if !DEBUG
			}
			catch (Exception ex)
			{
                System.Windows.MessageBox.Show(ex.SxGetErrorMessage());
			}
#endif
        }

        private void AmendTable(List<DataTable> tables)
        {
            foreach (var dt in tables.ToArray())
            {
                // Row가 없는 테이블은 제거합니다.
                if (dt.Rows.Count < 1)
                {
                    tables.Remove(dt);
                    continue;
                }

                // 값을 보정합니다.
                foreach (DataRow row in dt.Rows)
                {
                    if (row.IsNull("Binding") || string.IsNullOrEmpty(row.ToStr("Binding")))
                        row["Binding"] = row.ToStr("Title").Replace(" ", "");

                    row["Binding"] = row.ToStr("Binding").Replace(@"""", @"");
                    row["Title"] = row.ToStr("Title").Replace(@"""", @"");

                    row["CellType"] = row.ToStr("CellType").RightBySearch(".", true);
                    row["HorizontalAlignment"] = row.ToStr("HorizontalAlignment").RightBySearch(".", true);
                    row["DataType"] = row.ToStr("DataType").RightBySearch(".", true);
                    row["BackColor"] = row.ToStr("BackColor").RightBySearch(".", true);

                    foreach (DataColumn col in dt.Columns)
                    {
                        row[col.ColumnName] = row.ToStr(col.ColumnName).Trim();
                    }
                }
            }
        }

        private void ParseColumnInitCode()
        {
            if (string.IsNullOrEmpty(this.ColumnInitCode)) return;

            var resultTables = new List<DataTable>();

            var code = this.ColumnInitCode;
            if (code.IndexOf(".Get(") >= 0
                || code.IndexOf(".Cells[") >= 0)
            {
                var ssa = new SpreadSheetAnalysis();
                ssa.TableSchema = new GridColumnInfoParser("").GetDataTable();
                ssa.Parse(code);
                resultTables.AddRange(ssa.ResultTables);
            }
            else // line 단위 분석
            {
                var dt = new GridColumnInfoParser("").GetDataTable();
                resultTables.Add(dt);

                foreach (var codeLine in code.Split(';'))
                {
                    var line = codeLine.RegexReplace("[\t]+", " ")
                        .RegexReplace(@"[\s]+\(", "(")
                        .Trim();
                    if (line.StartsWith("//") && line.IndexOf("\r\n") >= 0)
                        line = line.Substring(line.IndexOf("\r\n") + "\r\n".Length);
                    if (string.IsNullOrEmpty(line) || line.StartsWith("//")) continue;

                    if (line.IndexOf(".Header(") > 0)
                    {
                        #region .Header(
                        var rLine = line.Between(".Header(", ")", false, true);
                        var parameters = rLine.Split(',');
                        if (parameters.Length == 3)
                        {
                            //public Header(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName);
                            var row = dt.NewRow();
                            dt.Rows.Add(row);
                            if (Utils.StringExtensions.IsNumeric(parameters[0]))
                                row["Binding"] = parameters[1];
                            else
                                row["Binding"] = parameters[0].IndexOf("_") >= 0 ? parameters[0].RightBySearch("_") : parameters[0];
                            row["Title"] = parameters[1];
                            row["CellType"] = parameters[2];
                        }
                        else if (parameters.Length == 4)
                        {
                            //public Header(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName, string DataField);
                            var row = dt.NewRow();
                            dt.Rows.Add(row);
                            row["Title"] = parameters[1];
                            row["CellType"] = parameters[2];
                            row["Binding"] = parameters[3];
                        }
                        else if (parameters.Length == 5)
                        {
                            //public Header(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName, int width, string DataField);
                            var row = dt.NewRow();
                            dt.Rows.Add(row);
                            row["Title"] = parameters[1];
                            row["CellType"] = parameters[2];
                            row["Width"] = parameters[3];
                            row["Binding"] = parameters[4];
                        }
                        else if (parameters.Length == 7)
                        {
                            //public Header(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden);
                            var row = dt.NewRow();
                            dt.Rows.Add(row);
                            if (Utils.StringExtensions.IsNumeric(parameters[0]))
                                row["Binding"] = parameters[1];
                            else
                                row["Binding"] = parameters[0].IndexOf("_") >= 0 ? parameters[0].RightBySearch("_") : parameters[0];
                            row["Title"] = parameters[1];
                            row["CellType"] = parameters[2];
                            row["ReadOnly"] = parameters[3];
                            row["Width"] = parameters[4];
                            row["HorizontalAlignment"] = parameters[5];
                            row["Hidden"] = parameters[6];
                        }
                        else if (parameters.Length == 8)
                        {
                            //public Header(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, string DataField);
                            var row = dt.NewRow();
                            dt.Rows.Add(row);
                            row["Title"] = parameters[1];
                            row["CellType"] = parameters[2];
                            row["ReadOnly"] = parameters[3];
                            row["Width"] = parameters[4];
                            row["HorizontalAlignment"] = parameters[5];
                            row["Hidden"] = parameters[6];
                            row["Binding"] = parameters[7];
                        }
                        else if (parameters.Length == 13)
                        {
                            //public Header(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName, Color BackColor, bool Lock, float Width, int DataLength, int Precision, int NumericScale, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden);
                            var row = dt.NewRow();
                            dt.Rows.Add(row);
                            if (Utils.StringExtensions.IsNumeric(parameters[0]))
                                row["Binding"] = parameters[1];
                            else
                                row["Binding"] = parameters[0].IndexOf("_") >= 0 ? parameters[0].RightBySearch("_") : parameters[0];
                            row["Title"] = parameters[1];
                            row["CellType"] = parameters[2];
                            row["BackColor"] = parameters[3];
                            row["ReadOnly"] = parameters[4];
                            row["Width"] = parameters[5];
                            row["Length"] = parameters[6];
                            row["HorizontalAlignment"] = parameters[11];
                            row["Hidden"] = parameters[12];
                        }
                        else if (parameters.Length == 14)
                        {
                            //public Header(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName, Color BackColor, bool Lock, float Width, int DataLength, int Precision, int NumericScale, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, string DataField);
                            var row = dt.NewRow();
                            dt.Rows.Add(row);
                            row["Title"] = parameters[1];
                            row["CellType"] = parameters[2];
                            row["BackColor"] = parameters[3];
                            row["ReadOnly"] = parameters[4];
                            row["Width"] = parameters[5];
                            row["Length"] = parameters[6];
                            row["HorizontalAlignment"] = parameters[11];
                            row["Hidden"] = parameters[12];
                            row["Binding"] = parameters[13];
                        }
                        #endregion
                    }
                    else if (line.IndexOf(".InitialHeader(") > 0)
                    {
                        #region .InitialHeader(
                        var rLine = line.Between(".InitialHeader(", ")", false, true);
                        var parameters = rLine.Split(',');

                        if (parameters.Length == 3)
                        {
                            if (parameters[2].IndexOf("Cell", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                //public void InitialHeader(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName);
                                var row = dt.NewRow();
                                dt.Rows.Add(row);
                                if (Utils.StringExtensions.IsNumeric(parameters[0]))
                                    row["Binding"] = parameters[1];
                                else
                                    row["Binding"] = parameters[0].IndexOf("_") >= 0 ? parameters[0].RightBySearch("_") : parameters[0];
                                row["Title"] = parameters[1];
                                row["CellType"] = parameters[2];
                            }
                            else if (parameters[1].IndexOf("Cell", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                //public void InitialHeader(string Title, CommonModule.CellTypeProperty CellTypeName, string DataField);
                                var row = dt.NewRow();
                                dt.Rows.Add(row);
                                row["Title"] = parameters[0];
                                row["CellType"] = parameters[1];
                                row["Binding"] = parameters[2];
                            }
                        }
                        else if (parameters.Length == 4)
                        {
                            //public void InitialHeader(string Title, CommonModule.CellTypeProperty CellTypeName, int width, string DataField);
                            var row = dt.NewRow();
                            dt.Rows.Add(row);
                            row["Title"] = parameters[0];
                            row["CellType"] = parameters[1];
                            row["Width"] = parameters[2];
                            row["Binding"] = parameters[3];
                        }
                        else if (parameters.Length == 7)
                        {
                            if (parameters[2].IndexOf("Cell", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                //public void InitialHeader(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden);
                                var row = dt.NewRow();
                                dt.Rows.Add(row);
                                if (Utils.StringExtensions.IsNumeric(parameters[0]))
                                    row["Binding"] = parameters[1];
                                else
                                    row["Binding"] = parameters[0].IndexOf("_") >= 0 ? parameters[0].RightBySearch("_") : parameters[0];
                                row["Title"] = parameters[1];
                                row["CellType"] = parameters[2];
                                row["ReadOnly"] = parameters[3];
                                row["Width"] = parameters[4];
                                row["HorizontalAlignment"] = parameters[5];
                                row["Hidden"] = parameters[6];
                            }
                            else if (parameters[1].IndexOf("Cell", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                //public void InitialHeader(string Title, CommonModule.CellTypeProperty CellTypeName, bool Lock, float Width, CommonModule.Alignment TextAlignment, bool Hidden, string DataField);
                                var row = dt.NewRow();
                                dt.Rows.Add(row);
                                row["Title"] = parameters[0];
                                row["CellType"] = parameters[1];
                                row["ReadOnly"] = parameters[2];
                                row["Width"] = parameters[3];
                                row["HorizontalAlignment"] = parameters[4];
                                row["Hidden"] = parameters[5];
                                row["Binding"] = parameters[6];
                            }
                        }
                        else if (parameters.Length == 13)
                        {
                            if (parameters[2].IndexOf("Cell", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                //public void InitialHeader(int Seq, string Title, CommonModule.CellTypeProperty CellTypeName, Color BackColor, bool Lock, float Width, int DataLength, int Precision, int NumericScale, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden);
                                var row = dt.NewRow();
                                dt.Rows.Add(row);
                                if (Utils.StringExtensions.IsNumeric(parameters[0]))
                                    row["Binding"] = parameters[1];
                                else
                                    row["Binding"] = parameters[0].IndexOf("_") >= 0 ? parameters[0].RightBySearch("_") : parameters[0];
                                row["Title"] = parameters[1];
                                row["CellType"] = parameters[2];
                                row["BackColor"] = parameters[3];
                                row["ReadOnly"] = parameters[4];
                                row["Width"] = parameters[5];
                                row["Length"] = parameters[6];
                                row["HorizontalAlignment"] = parameters[11];
                                row["Hidden"] = parameters[12];
                            }
                            else if (parameters[1].IndexOf("Cell", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                //public void InitialHeader(string Title, CommonModule.CellTypeProperty CellTypeName, Color BackColor, bool Lock, float Width, int DataLength, int Precision, int NumericScale, CommonModule.SortOrder Asc, int KeyCol, CommonModule.Alignment TextAlignment, bool Hidden, string DataField);
                                var row = dt.NewRow();
                                dt.Rows.Add(row);
                                row["Title"] = parameters[0];
                                row["CellType"] = parameters[1];
                                row["BackColor"] = parameters[2];
                                row["ReadOnly"] = parameters[3];
                                row["Width"] = parameters[4];
                                row["Length"] = parameters[5];
                                row["HorizontalAlignment"] = parameters[10];
                                row["Hidden"] = parameters[11];
                                row["Binding"] = parameters[12];
                            }
                        }
                        #endregion
                    }
                    else if (line.IndexOf("new string[,]") > 0 || line.Count(f => f == '{') > 3)
                    {
                        #region new string[,]
                        var regex = @"{[^}{]+";
                        foreach (var match in Regex.Matches(line, regex))
                        {
                            // 0.ColumnName_1.Label_2.DataType_3.Width_4.Hidden_5.CellType _6.Unique_7.NotNull_8.Length_9.ReadOnly_10.SaveNo_11.DefaultValue_
                            var pLine = match.ToString().Substring(1).Trim();
                            if (string.IsNullOrEmpty(pLine) || pLine.StartsWith("//")) continue;

                            var parameters = pLine.Split(',');
                            if (parameters.Length < 1) continue;

                            parameters = parameters.Replace(@"""", @"");
                            var row = dt.NewRow();
                            dt.Rows.Add(row);
                            row["Binding"] = parameters[0].Trim();
                            row["Title"] = parameters.Length > 1 ? parameters[1].Trim() : null;
                            row["DataType"] = parameters.Length > 2 ? parameters[2].Trim() : null;
                            row["Width"] = parameters.Length > 3 ? parameters[3].Trim() : null;
                            row["Hidden"] = parameters.Length > 4 ? parameters[4].Trim() : null;
                            row["CellType"] = parameters.Length > 5 ? parameters[5].Trim() : null;
                            //row["Unique"] = parameters.Length > 6 ? parameters[6].Trim() : null;
                            row["NotNull"] = parameters.Length > 7 ? parameters[7].Trim() : null;
                            row["Length"] = parameters.Length > 8 ? parameters[8].Trim() : null;
                            row["ReadOnly"] = parameters.Length > 9 ? parameters[9].Trim() : null;
                            //row["SaveNo"] = parameters.Length > 10 ? parameters[10].Trim() : null;
                            row["DefaultValue"] = parameters.Length > 11 ? parameters[11].Trim() : null;
                            row["HorizontalAlignment"] = parameters.Length > 12 ? parameters[12].Trim() : null;
                        }
                        #endregion
                    }
                    else if (line.IndexOf(".DataField =") > 0)
                    {
                        var rLine = line.Substring(line.IndexOf(".DataField =") + ".DataField =".Length).Trim();

                        var row = dt.NewRow();
                        dt.Rows.Add(row);
                        row["Binding"] = rLine;
                    }
                }

                if (dt.Rows.Count < 1)
                {
                    var mColumns = Regex.Match(code, @"<Columns>.*<\/Columns>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    var mCells = Regex.Match(code, @"<Cells>.*<\/Cells>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                    var list = new List<ColumnInfo>();
                    if (mColumns.Success)
                    {
                        foreach (Match m in Regex.Matches(mColumns.Value, @"<Column .*?<\/Column>", RegexOptions.Singleline | RegexOptions.IgnoreCase))
                        {                            
                            var index = Convert.ToInt32(Regex.Match(m.Value, @"index=""(\d+)""").Groups[1].Value);
                            var name = Regex.Match(m.Value, @"<ColumnName>(\w+)<\/ColumnName>").Groups[1];

                            var item = list.Where(p => p.Index.Equals(index)).FirstOrDefault();
                            if (item == null) { item = new ColumnInfo(); list.Add(item); }
                            item.Index = index;
                            item.Name = name;
                        }
                    }
                    if (mCells.Success)
                    {
                        foreach (Match m in Regex.Matches(mCells.Value, @"<Cell .*?<\/Cell>", RegexOptions.Singleline | RegexOptions.IgnoreCase))
                        {
                            var index = Convert.ToInt32(Regex.Match(m.Value, @"column=""(\d+)""").Groups[1].Value);
                            var display = Regex.Match(m.Value, @"<Data .*?>(.*?)<\/Data>").Groups[1];

                            var item = list.Where(p => p.Index.Equals(index)).FirstOrDefault();
                            if (item == null) { item = new ColumnInfo(); list.Add(item); }
                            item.Index = index;
                            item.Display = display;
                        }
                    }

                    foreach(var item in list.OrderBy(p => p.Index))
                    {                        
                        var row = dt.NewRow();
                        dt.Rows.Add(row);
                        row["Title"] = item.Display ?? item.Name;
                        row["Binding"] = item.Name ?? item.Display;
                    }
                }
            }

            this.AmendTable(resultTables);
            this.ColumnSplitResult = resultTables;
        }

        #endregion

        public class ColumnInfo
        {
            public int Index { get; set; }
            public Group Name { get; set; }

            public Group Display { get; set; }
        }
	}
}
