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
		private string code;
		private string path;

		public List<DataTable> ResultTables { get; set; }
		public DataTable TableSchema { get; set; }

		public SpreadSheetAnalysis(string path)
		{
			this.path = path;
			this.code = IOHelper.ReadFileToString(path);
			this.ResultTables = new List<DataTable>();
		}

		public bool Parse()
		{

			ParseInitializeCode();
			ParseXmlResource();

			return true;
		}

		private void ParseInitializeCode()
		{
			var list = new List<ColumnInfo>();

			var regex = @"[^\s\n]*Columns.Get\([\d]+\)[^\n]*;";
			foreach (var match in Regex.Matches(code, regex))
			{
				var line = match.ToString().Trim();

				//string name. property, value;
				var name = line.Between("this.", ".Columns.Get");
				var columnIndex = line.Between(".Get(", ").").ToInt();
				var property = line.Between(").", " = ");
				var value = line.Between(" = ", ";");

				if (value.IndexOf(@"""") < 0 && value.IndexOf(".") > 0)
					value = value.Last(".");
				else
					value = value.Replace(@"""", @"");

				if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(property)) continue;

				list.Add(new ColumnInfo()
				{
					Name = name.Trim(),
					ColumnIndex = columnIndex,
					Property = property.Trim(),
					Value = value.Trim()
				});
			}

			FillResultTableByColuminfoList(list);
		}
		
		private void ParseXmlResource()
		{
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
				if (name.IndexOf(".Models") < 0) continue;
				name = name.Substring(0, name.IndexOf(".Models"));

				var cells = dataXml.GetElementsByTagName("Cells").Cast<XmlElement>().FirstOrDefault();
				if (cells == null) continue;

				for (int i = 0; i < cells.ChildNodes.Count; i++)
				{
					var cell = cells.ChildNodes[i];
					var data = cell.SelectNodes("Data").Item(0);

					var column = cell.Attributes["column"].Value;
					var row = cell.Attributes["row"].Value;
					var type = data.Attributes["type"].Value;
					var value = data.InnerText;

					list.Add(new ColumnInfo() { Name = name.Trim(), ColumnIndex = column.ToInt(), Property = "Label", Value = value.Trim() });
					list.Add(new ColumnInfo() { Name = name.Trim(), ColumnIndex = column.ToInt(), Property = "DataType", Value = type.Trim() });
				}
			}


			FillResultTableByColuminfoList(list);
		}

		private DataTable CastToReturnTable(IEnumerable<ColumnInfo> list)
		{
			var dt = TableSchema.Clone();

			for(int i = 0; i <= list.Max(p => p.ColumnIndex); i++)
			{
				var dr = dt.NewRow();			

				foreach(var item in list.Where(p => p.ColumnIndex.Equals(i)))
				{
					if (item.Property.Equals("Label"))
					{
						dr["Title"] = item.Value;
						if (dr.IsNull("Binding"))
							dr["Binding"] = item.Value.Replace(" ", "");
					}
					else if (item.Property.Contains("VerticalAlignment", "Font"))
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
					else if (item.Property.Equals("DataField"))
						dr["Binding"] = item.Value;
#if DEBUG
					else
						MessageBox.Show(string.Format("{0} 유형이 발견되었습니다. 저한테 알려주세요(언종)", item.Property));
						Console.WriteLine(string.Join(" / ", item.Property, item.Value));
#endif					
				}

				if (string.IsNullOrEmpty(dr.ToStr("Title"))) continue;

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

		class ColumnInfo
		{
			public string Name { get; set; }
			public int ColumnIndex { get; set; }
			public string Property { get; set; }
			public string Value { get; set; }
		}
		
	}
}
