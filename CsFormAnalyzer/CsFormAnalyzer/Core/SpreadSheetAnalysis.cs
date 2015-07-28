using CsFormAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace CsFormAnalyzer.Core
{
	public class SpreadSheetAnalysis
    {
        #region fields & properties

        private string code;
		private string path;

		public List<DataTable> ResultTables { get; set; }
		public DataTable TableSchema { get; set; }

        #endregion

        #region constructor & initialize...

        public SpreadSheetAnalysis()
        {
            this.ResultTables = new List<DataTable>();
        }

		public SpreadSheetAnalysis(string path) : this()
		{
			this.path = path;
			this.code = IOHelper.ReadFileToString(path);			
		}

        #endregion

        #region methods

        public bool Parse()
		{			
			ParseInitializeCode();
			ParseXmlResource();

			return true;
		}

        public bool Parse(string code)
        {
            this.code = code;

            ParseInitializeCode();
            ParseXmlResource();

            return true;
        }

		private void ParseInitializeCode()
		{
			var list = new List<ColumnInfo>();

            #region ColumnsHeader.Cells.Get
            {
                var regex = @"[^\s\n]*ColumnHeader.Cells.Get\([0-9, ]+\)[^\n]*;";
                foreach (var match in Regex.Matches(code, regex))
                {
                    var line = match.ToString().Trim();
                    if (line.StartsWith("//")) continue;

                    int col = 0, row = 0;
                    var name = line.Between("this.", ".ColumnHeader.Cells.Get");
                    if (string.IsNullOrEmpty(name))
                    {
                        var codeArray = line.Split('.');
                        var ndx = Array.IndexOf(codeArray, "ColumnHeader") - 2;
                        if (ndx >= 0)
                            name = codeArray[ndx];
                    }
                    if (string.IsNullOrEmpty(name))
                    {
#if DEBUG
                        Debugger.Break();
#endif
                        continue;
                    }
                    var matrix = line.Between(".Get(", ").");
                    var property = line.Between(").", " = ");
                    var value = line.Between(" = ", ";");

                    var p = (Point)new PointConverter().ConvertFromString(matrix);
                    row = (int)p.X;
                    col = (int)p.Y;

                    if (name == null || name.IndexOf(".") > 0
                        || string.IsNullOrEmpty(property) || string.IsNullOrEmpty(value)) continue;

                    if (value.IndexOf(@"""") < 0 && value.IndexOf(".") > 0)
                        value = value.RightBySearch(".");
                    else
                        value = value.Replace(@"""", @"");

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(property)) continue;

                    list.Add(new ColumnInfo()
                    {
                        Name = name.Trim(),
                        Row = row,
                        Column = col,
                        Property = property.Trim(),
                        Value = value.Trim()
                    });
                }
            }
            #endregion

            #region ColumnsHeader.Cells[
            {
                var regex = @"[^\s\n]*ColumnHeader.Cells\[[\d]+[^\n]*;";
                foreach (var match in Regex.Matches(code, regex))
                {
                    var line = match.ToString().Trim();
                    if (line.StartsWith("//")) continue;

                    int col = 0, row = 0;
                    var name = line.Between("this.", ".ColumnHeader.Cells.Get");
                    if (string.IsNullOrEmpty(name))
                    {
                        var codeArray = line.Split('.');
                        var ndx = Array.IndexOf(codeArray, "ColumnHeader") - 2;
                        if (ndx >= 0)
                            name = codeArray[ndx];
                    }
                    if (string.IsNullOrEmpty(name))
                    {
#if DEBUG
                        Debugger.Break();
#endif
                        continue;
                    }
                    var matrix = line.Between(".Cells[", "]");
                    var property = line.LastBetween("].", " = ");
                    var value = line.Between(" = ", ";");

                    try
                    {
                        var p = (Point)new PointConverter().ConvertFromString(matrix);
                        row = (int)p.X;
                        col = (int)p.Y;
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (name == null || name.IndexOf(".") > 0
                        || string.IsNullOrEmpty(property) || string.IsNullOrEmpty(value)) continue;

                    if (value.IndexOf(@"""") < 0 && value.IndexOf(".") > 0)
                        value = value.RightBySearch(".");
                    else
                        value = value.Replace(@"""", @"");

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(property)) continue;

                    list.Add(new ColumnInfo()
                    {
                        Name = name.Trim(),
                        Row = row,
                        Column = col,
                        Property = property.Trim(),
                        Value = value.Trim()
                    });
                }
            }
            #endregion

            #region Columns.Get
            { // Columns
                var regex = @"[^\s\n]*Columns.Get\([\d]+\)[^\n]*;";
                foreach (var match in Regex.Matches(code, regex))
                {
                    var line = match.ToString().Trim();
                    if (line.StartsWith("//")) continue;

                    //string name. property, value;
                    var name = line.Between("this.", ".Columns.Get");
                    if (string.IsNullOrEmpty(name))
                    {
                        var codeArray = line.Split('.');
                        var ndx = Array.IndexOf(codeArray, "Columns") - 2;
                        if (ndx >= 0)
                            name = codeArray[ndx];
                    }
                    if (string.IsNullOrEmpty(name))
                    {
#if DEBUG
                        Debugger.Break();
#endif
                        continue;
                    }

                    var col = line.Between(".Get(", ").").ToInt();
                    var property = line.Between(").", " = ");
                    var value = line.Between(" = ", ";");

                    if (name == null || name.IndexOf(".") > 0
                        || string.IsNullOrEmpty(property) || string.IsNullOrEmpty(value)) continue;

                    if (value.IndexOf(@"""") < 0 && value.IndexOf(".") > 0)
                        value = value.RightBySearch(".");
                    else
                        value = value.Replace(@"""", @"");

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(property)) continue;

                    list.Add(new ColumnInfo()
                    {
                        Name = name.Trim(),
                        Column = col,
                        Property = property.Trim(),
                        Value = value.Trim()
                    });
                }
            }
            #endregion

            #region Columns[
            { // Columns
                var regex = @"[^\s\n]*Columns\[[\d]+\]\.[^\n]*;";
                foreach (var match in Regex.Matches(code, regex))
                {
                    var line = match.ToString().Trim();                    
                    if (line.StartsWith("//")) continue;

                    //string name. property, value;
                    var name = line.Between("this.", ".Columns[");
                    if (string.IsNullOrEmpty(name) || name.IndexOf('.') >= 0)
                    {
                        var codeArray = line.Split('.');
                        var columnsItem = codeArray.Where(p => p.IndexOf("Columns") >= 0).FirstOrDefault();
                        var ndx = Array.IndexOf(codeArray, columnsItem) - 2;
                        if (ndx >= 0)
                            name = codeArray[ndx];
                        else
                            name = codeArray[0];
                    }
                    if (string.IsNullOrEmpty(name))
                    {
#if DEBUG
                        Debugger.Break();
#endif
                        continue;
                    }

                    var col = line.Between(".Columns[", "]").ToInt();
                    var property = line.Between("].", "=").Trim();
                    var value = line.Between("=", ";").Trim();

                    if (name == null || name.IndexOf(".") > 0
                        || string.IsNullOrEmpty(property) || string.IsNullOrEmpty(value)) continue;

                    if (value.IndexOf(@"""") < 0 && value.IndexOf(".") > 0)
                        value = value.RightBySearch(".");
                    else
                        value = value.Replace(@"""", @"");

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(property)) continue;

                    list.Add(new ColumnInfo()
                    {
                        Name = name.Trim(),
                        Column = col,
                        Property = property.Trim(),
                        Value = value.Trim()
                    });
                }
            }
            #endregion

			FillResultTableByColuminfoList(list);
		}
		
		private void ParseXmlResource()
		{
            if (this.path == null) return;

			var path = this.path.Replace(".cs", ".resx");
			if (System.IO.File.Exists(path) != true) return;

			var list = new List<ColumnInfo>();

			var xmldoc = new XmlDocument();
			xmldoc.Load(path);
						
			var dataNodes = xmldoc.GetElementsByTagName("data").Cast<XmlElement>();
			foreach(var dataNode in dataNodes)
			{
				var dataXml = new XmlDocument();
				try
				{
					dataXml.LoadXml(dataNode.InnerText);
				}
				catch (Exception)
				{
					continue;
				}

				var name = dataNode.Attributes["name"].Value;
				if (name.IndexOf(".Model") < 0) continue;
				name = name.Substring(0, name.IndexOf(".Model"));
                
                #region ColumnName(Binding)

                var dm = dataXml.GetElementsByTagName("DataModel").Cast<XmlElement>().FirstOrDefault();
                if (dm != null)
                {
                    var columns = dm.SelectSingleNode("Columns");
                    if (columns != null)
                    {
                        for (int i = 0; i < columns.ChildNodes.Count; i++)
                        {
                            var column = columns.ChildNodes[i];
                            var columnInfo = column.SelectNodes("ColumnInfo").Item(0);

                            var col = column.Attributes["index"].Value;
                            var property = "ColumnName";
                            var value = columnInfo.SelectSingleNode("ColumnName").InnerText;

                            list.Add(new ColumnInfo() { Name = name.Trim(), Column = col.ToInt(), Property = property, Value = value.Trim() });
                        }
                    }
                }                
                
                #endregion
                
                #region Label, DataType
                
                var chdm = dataXml.GetElementsByTagName("ColumnHeaderDataModel").Cast<XmlElement>().FirstOrDefault();
                if (chdm != null)
                {
                    var cells = chdm.SelectSingleNode("Cells");
                    if (cells != null)
                    {
                        for (int i = 0; i < cells.ChildNodes.Count; i++)
                        {
                            var cell = cells.ChildNodes[i];
                            var data = cell.SelectNodes("Data").Item(0);

                            var column = cell.Attributes["column"].Value;
                            var row = cell.Attributes["row"].Value;
                            var type = data.Attributes["type"].Value;
                            var value = data.InnerText;

                            list.Add(new ColumnInfo() { Name = name.Trim(), Column = column.ToInt(), Property = "Label", Value = value.Trim() });
                            list.Add(new ColumnInfo() { Name = name.Trim(), Column = column.ToInt(), Property = "DataType", Value = type.Trim() });
                        }
                    }
                }
                
                #endregion
                
                var columnStyle = dataXml.GetElementsByTagName("ColumnStyles").Cast<XmlElement>().FirstOrDefault();
                ListFillByXmlElement(list, columnStyle, name, "Locked", "VerticalAlignment");

                var model = dataXml.GetElementsByTagName("ColumnAxisModel").Cast<XmlElement>().FirstOrDefault();
                if (model != null)
                {
                    var items = model.GetElementsByTagName("Items").Cast<XmlElement>().FirstOrDefault();
                    ListFillByXmlElement(list, items, name, "Size");
                }
			}


			FillResultTableByColuminfoList(list);
		}

        private void ListFillByXmlElement(List<ColumnInfo> list, XmlElement node, string name, params string[] properties)
        {
            if (node == null) return;

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                var item = node.ChildNodes[i];

                var index = item.Attributes.Cast<XmlAttribute>().Where(p => p.Name.Equals("Index", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (index == null) continue;

                var col = index.Value;
                foreach (var property in properties)
                {
                    if (item.ChildNodes.Cast<XmlElement>().Where(p => p.Name.Equals(property)).FirstOrDefault() == null) continue;

                    var value = item.SelectSingleNode(property).InnerText;
                    list.Add(new ColumnInfo() { Name = name.Trim(), Column = col.ToInt(), Property = property, Value = value.Trim() });
                }
            }
        }

		private DataTable CastToReturnTable(IEnumerable<ColumnInfo> list)
		{
			var dt = TableSchema.Clone();

			for (int i = 0; i <= list.Max(p => p.Column); i++)
			{
				var targetItem = list.Where(p => p.Column == i && p.Property.Contains("Label", "Text"))
					.OrderByDescending(p => p.Row) // 마지막 Row 가 바인딩 되는 컬럼 요소이다.
					.FirstOrDefault();
				if (targetItem == null) continue;

				var dr = dt.NewRow();

				dr["Title"] = targetItem.Value;
				dr["Binding"] = targetItem.Value.Replace(" ", "");
				
				//foreach(var item in list.Where(p => p.Row.Equals(targetItem.Row) && p.Column.Equals(targetItem.Column)))
                foreach (var item in list.Where(p => p.Column.Equals(targetItem.Column)))
				{
					if (item.Property.Contains("Label", "Text"))
						dr["Title"] = item.Value;

					else if (item.Property.Contains("VerticalAlignment", "Font", "ForeColor", "ColumnSpan", "RowSpan", "ParseFormatString", "Border",
                        "MergePolicy", "AllowAutoSort"))
						continue;

					else if (dt.Columns.Contains(item.Property))
					{
						if (item.Property.Equals("CellType"))
							dr[item.Property] = Regex.Replace(item.Value, @"[\d]+", "").Replace("CellType", "");
						else
							dr[item.Property] = item.Value;
					}
					else if (item.Property.Equals("Locked"))
						dr["ReadOnly"] = item.Value;

					else if (item.Property.Contains("DataField", "ColumnName"))
						dr["Binding"] = item.Value;

					else if (item.Property.Equals("Visible"))
						dr["Hidden"] = item.Value;

                    else if (item.Property.Equals("Size"))
                        dr["Width"] = item.Value;
#if DEBUG
					else
					{
						MessageBox.Show(string.Format("{0} 유형이 발견되었습니다. 저한테 알려주세요(언종)", item.Property));
						Console.WriteLine(string.Join(" / ", item.Property, item.Value));
					}
#endif					
				}

				dt.Rows.Add(dr);
			}

			return dt;

			//dt.Columns.Add("Binding");
			//dt.Columns.Add("Title");
			//dt.Columns.Add("DefaultValue");
			//dt.Columns.Add("CellType");
			//dt.Columns.Add("DataType");
			//dt.Columns.Add("Hidden");
			//dt.Columns.Add("NotNull");
			//dt.Columns.Add("ReadOnly");
			////dt.Columns.Add("Unique");
			//dt.Columns.Add("Length");
			//dt.Columns.Add("Width");
			//dt.Columns.Add("HorizontalAlignment");
			//dt.Columns.Add("BackColor");
			//dt.Columns.Add("Format");
		}

		private void FillResultTableByColuminfoList(List<ColumnInfo> list)
		{
			foreach (var name in list.GroupBy(p => p.Name).Select(p => p.Key))
			{
				var dt = CastToReturnTable(list.Where(p => p.Name.Equals(name)));
				this.ResultTables.Add(dt);
			}
		}

        #endregion

        class ColumnInfo
		{
			public string Name { get; set; }
			public int Row { get; set; }
			public int Column { get; set; }
			public string Property { get; set; }
			public string Value { get; set; }
		}
		
	}
}
