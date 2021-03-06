﻿using CsFormAnalyzer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace CsFormAnalyzer.Core
{
	public partial class ComponentAnalysis
    {
        #region Fields

        private int initIndexStart;
        private int initIndexEnd;
        private string initCode;
        private string[] panelTypes = new string[] { "TabControl", "TabPage", "Panel", "GroupBox", "FlowLayoutPanel", "TableLayoutPanel", "FrameCtrl" };
        private string[] codeLines;

        #endregion

        #region Properties

		public string Path { get; set; }
		public string[] SearchTypes { get; set; }
		public string[] SearchProperties { get; set; }
		public string[] SearchEvents { get; set; }
		public string[] RemovePrefixs { get; set; }
		public string[] SelectorTypes { get; set; }
        public string[] ExceptValues { get; set; }

		public DataTable ResultTable { get; private set; }
		public List<CheckLineItem> CheckLines { get; private set; }
		public string Code { get; set; }

		public List<ComponentPropertyInfo> PropertyList { get; set; }


		#endregion

		#region Initialize...

		public ComponentAnalysis()
		{
			Initialize();
		}

		private void Initialize()
		{
			this.CheckLines = new List<CheckLineItem>();
			this.ResultTable = null;
			this.Code = null;

			this.PropertyList = new List<ComponentPropertyInfo>();
		}

		#endregion

		#region Public Methods

		public bool Run()
		{
#if !DEBUG
			try
			{				
#endif
			Initialize();

			if (File.Exists(this.Path) != true) return false;

			var code = FindCode(this.Path);

			code = code.Replace("\t", " ");
			code = Regex.Replace(code, "[ ]+", " ");
			this.codeLines = code.Split('\n');
			this.Code = code;

			this.initIndexStart = code.IndexOf("private void InitializeComponent()");
			this.initIndexEnd = code.IndexOf("this.ResumeLayout(false);", initIndexStart);
			if (this.initIndexStart > 0 && this.initIndexEnd > 0)
				this.initCode = code.Substring(initIndexStart, initIndexEnd - initIndexStart);

			var dt = MakeReturnTable();

			AnalysisBeforeAmend(code, dt);

			AnalysisTypes(code, dt, this.SearchTypes);
			AnalysisProperties(code, dt, this.SearchProperties);
			AnalysisEvents(code, dt, this.SearchEvents);

            AnalysisResource(this.Path, dt);

			AnalysisAfterAmend(code, dt);

			AnalysisDepth(code, dt);

			FillPropertyList(code, dt);

			this.ResultTable = GetAmendTable(code, dt);
#if !DEBUG
			}
			catch(Exception ex)
			{
                
				Console.Write(ex);
				System.Windows.MessageBox.Show(ex.SxGetErrorMessage());
				return false;
			}		
#endif

			return true;
		}

		#endregion

        #region Analysis

        /// <summary>
		/// 코드에서 SearchTypes에 해당되는 컴포넌트 정보(Type, Name)를 추가합니다.
		/// </summary>
		private void AnalysisTypes(string code, DataTable dt, string[] searchTypes)
		{
			var regex = @"([^)\s][^\n]* new [a-zA-Z.]*\(\)[^\n]*;)";

			foreach (var match in Regex.Matches(code, regex))
			{
				var line = match.ToString().Trim();
				if (line.StartsWith("//")) continue;
				
				var type = line.LastBetween(".", "()");
				if (searchTypes.Contains(type) != true) continue;

				var name = line.Between("this.", " ");

				if (string.IsNullOrEmpty(type) != true && string.IsNullOrEmpty(name) != true)
				{
					var item = new ComponentPropertyInfo()
					{
						Name = name,
						Line = line
					};
					this.PropertyList.Add(item);

					var row = GetNewRow(dt);					
					row["type"] = type;
					row["Name"] = name;
					row["컨트롤명"] = GetMappingType(type);
                    row["T"] = "N";
					row["line"] = line;

					ApplyMacroProperties(dt, type, name);
				}
			}
		}

		/// <summary>
		/// 코드에서 SearchProperties에 해당되는 컴포넌트 정보를 추가합니다.
		/// </summary>
		private void AnalysisProperties(string code, DataTable dt, string[] searchProperties)
		{
			var list = searchProperties.ToList();
			if (list.Contains("TabIndex") != true) list.Add("TabIndex");
			if (list.Contains("Location") != true) list.Add("Location");
			searchProperties = list.ToArray();
			
			var regex = @"([^)\s][^)\n]*\.[a-zA-Z.]*[ ]+= [^\n]*;)";

			foreach (var match in Regex.Matches(code, regex))
			{
				var line = match.ToString().Trim();
				if (line.StartsWith("//")) continue;
								
				string name, check = null;

				if (line.Left(5).Equals("this."))
					name = line.Between("this.", ".");
				else
					name = line.Between("", ".");
				if (name == null) continue;

				var propertyName = line.Between(string.Format("{0}.", name), " = ");
				if (propertyName == null) continue;
				propertyName = propertyName.Trim();
				if (propertyName.LastIndexOf(".") > 0)
				{
					check = "child";
					propertyName = propertyName.Substring(propertyName.LastIndexOf(".") + 1);
				}

				var value = line.Between(" = ", ";");

				if (string.IsNullOrEmpty(name) != true && string.IsNullOrEmpty(propertyName) != true && string.IsNullOrEmpty(value) != true)
				{
                    if (value.IndexOf("resources.Get") >= 0) continue;

					var item = new ComponentPropertyInfo()
					{
						Name = name,
						Target = propertyName,
						Value = value,
						Line = line
					};
					this.PropertyList.Add(item);

					if (searchProperties.Contains(propertyName) != true) continue;

					if ("TabIndex".Equals(propertyName))
					{
						if (value.IsNumeric() != true) continue;

						var tabIndex = Convert.ToInt32(value);
						var rows = dt.Select(string.Format("Name = '{0}'", name));
						foreach(var row in rows)
						{
							row["index"] = tabIndex;
						}						
						continue;
					}
					else if ("Location".Equals(propertyName))
					{
						var matches = Regex.Matches(value, @"[\d]+").Cast<Match>().Select(p => p.Value).ToArray();						
						if (matches.Count() > 1)
						{
							var left = matches.ElementAt(0);
							var top = matches.ElementAt(1);
							var rows = dt.AsEnumerable().Where(p => p.ToStr("name").Equals(name) && p.IsNull("top"));
							foreach (var row in rows)
							{
								row["top"] = top;
								row["left"] = left;
							}
						}
						continue;
					}
					else
					{
                        // value 보정
                        if (string.IsNullOrEmpty(value) != true)
                        {
                            if (propertyName.Contains("ForeColor", "BackColor"))
                            {
                                var matches = Regex.Matches(value, @"[\d]+");
                                if (matches.Count == 3)
                                {
                                    var arr = matches.Cast<Match>();
                                    value = String.Format("#{0}{1}{2}",
                                        Convert.ToInt32(arr.ElementAt(0).ToString()).ToString("x"), // r
                                        Convert.ToInt32(arr.ElementAt(1).ToString()).ToString("x"), // g
                                        Convert.ToInt32(arr.ElementAt(2).ToString()).ToString("x")) // b
                                        .ToUpper();
                                }
                            }

                            if (value.FirstOrDefault().Equals('"') && value.LastOrDefault().Equals('"'))
                                value = value.Replace(@"""", "");

                            else if (value.IndexOf("System.") == 0)
                                value = value.Substring(value.LastIndexOf('.') + 1);

                            else if ((value.IsNumeric() || value.IsBoolean()) != true
                                && IsInit(code, line) != true)
                                value = null;

                            if (ExceptValues.Contains(value)) continue;
                        }

						var row = GetTargetRowByName(dt, name);
						if (row != null)
						{
							row["target"] = propertyName;
							var mapPropertyName = GetMappingProperty(row["컨트롤명"].ToString(), propertyName);
							row["대상"] = mapPropertyName;
                            row["값"] = value;
							if (propertyName.Contains("DisplayMember", "ValueMember") != true)
							{
								row["바인딩"] = string.Format("{0}{1}Property", GetRemovedPrefixName(name), mapPropertyName);
							}							
							row["Check"] = check;
							row["line"] = line;
							//row["IsInit"] = IsInit(code, line);
							row["T"] = "P";
						}
#if DEBUG
						else
						{
							Console.WriteLine(line);
							Console.WriteLine(string.Join(" / ", name, propertyName, value));
							//System.Diagnostics.Debugger.Break();
						}
#endif
					}
				}
			}
		}

		/// <summary>
		/// 코드에서 SearchEvents에 해당되는 컴포넌트 정보를 추가합니다.
		/// </summary>
		private void AnalysisEvents(string code, DataTable dt, string[] searchEvents)
		{
			var regex = @"([^)\s][^)\n]*\.[a-zA-Z.]* \+= new [^\n]*\);)";

			foreach (var match in Regex.Matches(code, regex))
			{
				var line = match.ToString().Trim();
				if (line.StartsWith("//")) continue;

				string name, eventName, eventHandler;

				if (line.Left(5).Equals("this."))
					name = line.Between("this.", ".");
				else
					name = line.Between("", ".");
				if (name == null) continue;

				eventName = line.Between(string.Format("{0}.", name), " += ");
				if (eventName == null) continue;

				eventHandler = line.LastBetween("(", ");");
				if (eventHandler == null) continue;

				if (string.IsNullOrEmpty(name) != true && string.IsNullOrEmpty(eventName) != true && string.IsNullOrEmpty(eventHandler) != true)
				{
					var item = new ComponentPropertyInfo()
					{
						Name = name,
						Target = eventName,
						Value = eventHandler,
						Line = line
					};
					this.PropertyList.Add(item);

					if (searchEvents.Contains(eventName) != true) continue;

					var row = GetTargetRowByName(dt, name);
					if (row != null)
					{
						row["대상"] = "Command";
						row["바인딩"] = string.Format("{0}Command", GetRemovedPrefixName(name));
						row["비고"] = eventName;
						row["info"] = eventHandler;
						row["target"] = eventName;
						row["line"] = line;
						//row["IsInit"] = IsInit(code, line);
						row["T"] = "E";
					}
#if DEBUG
					else
					{
						Console.WriteLine(string.Join(" / ", name, eventName, eventHandler));
						//System.Diagnostics.Debugger.Break();
					}
#endif
				}
			}
		}

        private void AnalysisResource(string path, DataTable dt)
        {
            path = path.Replace(".cs", ".resx");
            if (System.IO.File.Exists(path) != true) return;

            var xmldoc = new XmlDocument();
            xmldoc.Load(path);

            var dataNodes = xmldoc.GetElementsByTagName("data").Cast<XmlElement>();
            foreach (var dataNode in dataNodes)
            {
                var nameValue = dataNode.Attributes["name"].Value;
                if (nameValue.IndexOf(".") < 0) continue;

                var name = nameValue.Substring(0, nameValue.IndexOf("."));
                var property = nameValue.Substring(nameValue.IndexOf(".") + 1);
                var value = dataNode.InnerText.Trim();

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(property) || string.IsNullOrEmpty(value)) continue;

                // 리소스에서 무시할 프로퍼티
                if (property.Contains("Image")) continue;

                if ("TabIndex".Equals(property))
                {
                    if (value.IsNumeric() != true) continue;

                    var tabIndex = Convert.ToInt32(value);
                    var rows = dt.Select(string.Format("Name = '{0}'", name));
                    foreach (var row in rows)
                    {
                        row["index"] = tabIndex;
                    }
                    continue;
                }
                else if ("Location".Equals(property))
                {
                    var matches = Regex.Matches(value, @"[\d]+").Cast<Match>().Select(p => p.Value).ToArray();
                    if (matches.Count() > 1)
                    {
                        var left = matches.ElementAt(0);
                        var top = matches.ElementAt(1);
                        var rows = dt.AsEnumerable().Where(p => p.ToStr("name").Equals(name) && p.IsNull("top"));
                        foreach (var row in rows)
                        {
                            row["top"] = top;
                            row["left"] = left;
                        }
                    }
                    continue;
                }

                if (this.SearchProperties.Contains(property))
                {
                    var row = GetTargetRowByName(dt, name);
                    if (row == null) continue;

                    row["target"] = property;
                    var mapPropertyName = GetMappingProperty(row["컨트롤명"].ToString(), property);
                    row["대상"] = mapPropertyName;
                    row["값"] = value.Trim();

                    if (property.Contains("DisplayMember", "ValueMember") != true)
                    {
                        row["바인딩"] = string.Format("{0}{1}Property", GetRemovedPrefixName(name), mapPropertyName);
                    }
                    row["info"] = "resx";
                    row["line"] = "code from DesignResourceFile (.Resx)";
                    row["T"] = "P";
                }
            }
        }

		/// <summary>
		/// 분석 이전 보정을 수행합니다.
		/// </summary>
		private void AnalysisBeforeAmend(string code, DataTable dt)
		{
			if (code.IndexOf("pnlLeft") > 0)
			{
				var row1 = GetNewRow(dt);
				row1["type"] = "Panel";
				row1["Name"] = "pnlLeft";
				row1["컨트롤명"] = "Panel";
                row1["T"] = "N";
				row1["line"] = "generator macro apply";

				var row2 = GetNewRow(dt);
				row2["type"] = "Panel";
				row2["Name"] = "pnlRight";
				row2["컨트롤명"] = "Panel";
                row2["T"] = "N";
				row2["line"] = "generator macro apply";
			}

			if (code.IndexOf("pnlTop") > 0)
			{
				var row1 = GetNewRow(dt);
				row1["type"] = "Panel";
				row1["Name"] = "pnlTop";
				row1["컨트롤명"] = "Panel";
                row1["T"] = "N";
				row1["line"] = "generator macro apply";

				var row2 = GetNewRow(dt);
				row2["type"] = "Panel";
				row2["Name"] = "pnlBottom";
				row2["컨트롤명"] = "Panel";
                row2["T"] = "N";
				row2["line"] = "generator macro apply";
			}
		}

		/// <summary>
		/// 분석 이후 보정을 수행합니다.
		/// </summary>
		private void AnalysisAfterAmend(string code, DataTable dt)
		{
			var dtRows = dt.AsEnumerable();

			{	// Button 에 Click 커맨드가 바인딩되어있지 않을경우 추가합니다.
				var buttonRows = dtRows
					.Where(p => p.ToStr("type").Equals("Button"))
					.GroupBy(item => new { Name = item.Field<string>("Name") })
					.Select(group => new { Name = group.Key.Name });

				foreach (var buttonRow in buttonRows)
				{
					var clickRow = dtRows.Where(p => p.ToStr("name").Equals(buttonRow.Name)
						&& p.ToStr("target").Equals("Click")).FirstOrDefault();

					if (clickRow != null) continue;

					var row = GetTargetRowByName(dt, buttonRow.Name);
					row["대상"] = "Command";
					row["바인딩"] = string.Format("{0}Command", GetRemovedPrefixName(buttonRow.Name));
					row["비고"] = "Click";
					row["target"] = "Click";
                    row["T"] = "E";
					row["line"] = "generator macro apply";
				}
			}
		}

		/// <summary>
		/// 콤포넌트의 VisualTree 상의 깊이를 분석하여 정렬번호를 부여합니다.
		/// </summary>
		private void AnalysisDepth(string code, DataTable dt)
		{
			var init_s = code.IndexOf("private void InitializeComponent()");
			var init_e = code.IndexOf("this.ResumeLayout(false);", initIndexStart);
			if (init_e < 0)
				init_e = code.IndexOf("#endregion", initIndexStart);

			var initCode = code.Substring(init_s, init_e - init_s);
					
			
			var tree = new List<DepthItem>();
			tree.Add(new DepthItem() { Name = "root", Top = 0, Left = 0 });
			{
				var row = dt.Select(string.Format("Name='{0}'", "pnlLeft")).FirstOrDefault();
				if (row != null)
					tree.Add(new DepthItem() { Name = "pnlLeft", Top = row.ToInt("Top"), Left = row.ToInt("Left") });
			}
			{
				var row = dt.Select(string.Format("Name='{0}'", "pnlRight")).FirstOrDefault();
				if (row != null)
					tree.Add(new DepthItem() { Name = "pnlRight", Top = row.ToInt("Top"), Left = row.ToInt("Left") });
			}
			{
				var row = dt.Select(string.Format("Name='{0}'", "pnlTop")).FirstOrDefault();
				if (row != null)
					tree.Add(new DepthItem() { Name = "pnlTop", Top = row.ToInt("Top"), Left = row.ToInt("Left") });
			}
			{
				var row = dt.Select(string.Format("Name='{0}'", "pnlBottom")).FirstOrDefault();
				if (row != null)
					tree.Add(new DepthItem() { Name = "pnlBottom", Top = row.ToInt("Top"), Left = row.ToInt("Left") });
			}

			var regex = @"([^)\s][^\n]* new [a-zA-Z.]*\(\)[^\n]*;)";

			// 패널의 트리 구성
			var tabPageList = new Dictionary<string, int>();
			int groupNum = 0;
			foreach (var match in Regex.Matches(initCode, regex))
			{
				var line = match.ToString().Trim();
				string type, name;
				type = line.LastBetween(".", "()");
				if (this.panelTypes.Contains(type) != true) continue;

				name = line.LastBetween("this.", " ");				
				
				if ("TabPage".Equals(type))
				{
					tabPageList.Add(name, ++groupNum);
				}

				var row = dt.Select(string.Format("Name='{0}'", name)).FirstOrDefault();
				tree.Add(new DepthItem() { Name = name, Top = row.ToInt("Top"), Left = row.ToInt("Left") });
			}

			// 패널트리에 아이템 채우기
			foreach(var match in Regex.Matches(initCode, @"([^)\s][^)\n]*\.Controls\.Add[^\n]*;)"))
			{
				var line = match.ToString().Trim();
				string parent = null, target;

				parent = line.Between("this.", ".Controls");
				target = line.Between("(this.", ");");

				var treeItem = string.IsNullOrEmpty(parent)
					? tree.Where(p => p.Name.Equals("root")).FirstOrDefault()
					: tree.Where(p => p.Name.Equals(parent)).FirstOrDefault();

                if (treeItem == null) continue;

				var row = dt.Select(string.Format("Name='{0}'", target)).FirstOrDefault();				
                treeItem.Items.Add(new DepthItem() { Name = target, Top = row.ToInt("Top"), Left = row.ToInt("Left") });
			}


			var root = tree.Where(p => p.Name.Equals("root")).FirstOrDefault();
			var pnlTop = tree.Where(p => p.Name.Equals("pnlTop")).FirstOrDefault();			
			var pnlLeft = tree.Where(p => p.Name.Equals("pnlLeft")).FirstOrDefault();
			var pnlRight = tree.Where(p => p.Name.Equals("pnlRight")).FirstOrDefault();
			var pnlBottom = tree.Where(p => p.Name.Equals("pnlBottom")).FirstOrDefault();

			if (pnlTop != null) root.Items.Add(pnlTop);
			if (pnlLeft != null) root.Items.Add(pnlLeft);
			if (pnlRight != null) root.Items.Add(pnlRight);
			if (pnlBottom != null) root.Items.Add(pnlBottom);

			if (root != null && root.Items.Count > 0) ReqursiveApplySortIndex(tree, dt, root, "root", "");
			
		}

		private void ReqursiveApplySortIndex(List<DepthItem> tree, DataTable dt, DepthItem depthItem, string depthAdd, string depth)
		{
			int seq = 0;

			foreach (var item in depthItem.Items.OrderBy(p => p.Top).ThenBy(p => p.Left))
			{
				seq++;
                var depthStr = depth.ConcatDiv(".", seq.ToString().PadLeft(2, '0'));
				var rows = dt.AsEnumerable().Where(p => p.ToStr("Name").Equals(item.Name));
				foreach (var row in rows)
				{
					row["depth"] = depthStr;
					row["depthAdd"] = depthAdd;
				}

				var treeItem = tree.Where(p => p.Name.Equals(item.Name)).FirstOrDefault();
				if (treeItem != null)
				{	// 패널요소 입니다.					
					ReqursiveApplySortIndex(tree, dt, treeItem, string.Join(".", depthAdd, item.Name), depthStr);
					continue;
				}
			}
		}

		private string FindCode(string path, string cs = null)
		{
			var code = IOHelper.ReadFileToString(path);
			if (cs != null) code = string.Join("\n", cs, code);


			var indexStart = code.IndexOf("private void InitializeComponent()");
			if (indexStart < 0)
			{
				if (path.IndexOf("Designer.cs") < 0)
				{
					path = path.Replace(".cs", ".Designer.cs");
					return FindCode(path, code);
				}
				else
				{
					throw new Exception("윈도우 폼이 아닙니다.");
				}
			}
			return code;
		}

		private void FillPropertyList(string code, DataTable dt)
		{
			var names = dt.Distinct("Name").AsEnumerable().Select(p => p.ToStr("Name"));
			
			foreach (var name in names)
			{
				foreach(var codeline in codeLines.Where(p => p.IndexOf(name) >= 0))
				{
					var line = codeline.Trim();
					var has = PropertyList.Where(p => p.Line.Equals(line)).FirstOrDefault() != null;
					if (has == true) continue;

                    // name 이 정확히 일치하지 않음 label1 > label11 거르기
                    var postLine = line.Substring(line.IndexOf(name) + name.Length);
                    if (postLine.Length > 0 && postLine.Substring(0, 1).IsNumeric()) continue;


					var item = new ComponentPropertyInfo()
					{
						Name = name,
						Line = line
					};
					PropertyList.Add(item);
				}
			}
		}

		/// <summary>
		/// prefix 를 제거한 컨트롤 이름을 가져옵니다.
		/// </summary>
		private string GetRemovedPrefixName(string name)
		{
			foreach (var prefix in RemovePrefixs)
			{
				if (name.Length >= prefix.Length
					&& name.Substring(0, prefix.Length).Equals(prefix))
				{
					return name.Substring(prefix.Length);
				}
			}

			return name;
		}

		/// <summary>
		/// 컨트롤이름에 해당하는 추가될 행을 가져옵니다.
		/// </summary>
		private DataRow GetTargetRowByName(DataTable dt, string name)
		{
			var row = dt.Select(string.Format("Name = '{0}'", name)).FirstOrDefault();
			if (row != null)
			{
				if (string.IsNullOrEmpty(row.ToStr("대상")) != true)
				{
					var newRow = GetNewRow(dt);
					newRow["type"] = row["type"];
					newRow["Name"] = row["Name"];
					newRow["컨트롤명"] = row["컨트롤명"];
					newRow["index"] = row["index"];
					newRow["top"] = row["top"];
					newRow["left"] = row["left"];
					row = newRow;
				}
			}

			return row;
		}

		/// <summary>
		/// 컨트롤 타입에 따라 기본적으로 필요한 프로퍼티를 추가합니다.
		/// </summary>
		private void ApplyMacroProperties(DataTable dt, string type, string name)
		{
			// Selector Type 은 ItemsSource와 SelectedItem 프로퍼티를 추가합니다.
			//var selectorTypes = new string[] { "ComboBox", "ListBox", "CodeCombo" };			
			if (SelectorTypes.Contains(type))
			{
				{ // ItemsSource
					var row = GetTargetRowByName(dt, name);
					row["대상"] = "ItemsSource";					
					row["바인딩"] = string.Format("{0}ListProperty", GetRemovedPrefixName(name));
					row["T"] = "P";
					row["line"] = "generator macro apply";
				}
				{ // SelectedItem
					var row = GetTargetRowByName(dt, name);
					row["대상"] = "SelectedItem";
					row["바인딩"] = string.Format("Selected{0}Property", GetRemovedPrefixName(name));
					row["T"] = "P";
					row["line"] = "generator macro apply";
				}
			}

			if (type.Equals("DateTimePicker"))
			{
				var row = GetTargetRowByName(dt, name);
				row["대상"] = "Date";
				row["바인딩"] = string.Format("{0}DateProperty", GetRemovedPrefixName(name));
				row["T"] = "P";
				row["line"] = "generator macro apply";
			}
			else if(type.Equals("NumericUpDown"))
			{
				{ // Minimum
					var row = GetTargetRowByName(dt, name);
					row["대상"] = "Min";
					row["target"] = "Minimum";
					row["값"] = "0";
					row["T"] = "P";
					row["line"] = "generator macro apply";
				}
				{ // Maximum
					var row = GetTargetRowByName(dt, name);
					row["대상"] = "Max";
					row["target"] = "Maximum";
					row["값"] = "100";
					row["T"] = "P";
					row["line"] = "generator macro apply";
				}
				{ // Value
					var row = GetTargetRowByName(dt, name);
					row["대상"] = "Value";
					row["target"] = "Value";
					row["바인딩"] = string.Format("{0}ValueProperty", GetRemovedPrefixName(name));
					row["T"] = "P";
					row["line"] = "generator macro apply";
				}
			}


			// DataBindingManager 에 의한 기본 프로퍼티 추가
			if (type.Equals("TextBox") || type.Equals("RichTextBox"))
			{
				var row = GetTargetRowByName(dt, name);
				row["대상"] = "Text";
				row["target"] = "Text";
				row["바인딩"] = string.Format("{0}TextProperty", GetRemovedPrefixName(name));
				row["T"] = "P";
				row["line"] = "generator macro apply";
			}
			else if (type.Equals("CheckBox"))
			{
				var row = GetTargetRowByName(dt, name);
				row["대상"] = "IsChecked";
				row["target"] = "Checked";
				row["바인딩"] = string.Format("{0}IsCheckedProperty", GetRemovedPrefixName(name));
				row["T"] = "P";
				row["line"] = "generator macro apply";
			}
			else if (type.Equals("LMaskEdBox") || type.Equals("LFloatTextBox"))
			{
				var row = GetTargetRowByName(dt, name);
				row["대상"] = "Value";
				row["target"] = "Value";
				row["바인딩"] = string.Format("{0}ValueProperty", GetRemovedPrefixName(name));
				row["T"] = "P";
				row["line"] = "generator macro apply";
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// 반환테이블을 만들어 가져옵니다.
		/// </summary>
		private DataTable MakeReturnTable()
		{
			var dt = new DataTable();

			dt.Columns.Add(new DataColumn("컨트롤명", typeof(string)));
			dt.Columns.Add(new DataColumn("대상", typeof(string)));
			dt.Columns.Add(new DataColumn("값", typeof(string))); // 기본값
			dt.Columns.Add(new DataColumn("리소스", typeof(string)));
			dt.Columns.Add(new DataColumn("바인딩", typeof(string)));
			dt.Columns.Add(new DataColumn("비고", typeof(string))); // 이벤트

			dt.Columns.Add(new DataColumn("blank1", typeof(string)));
			dt.Columns.Add(new DataColumn("Name", typeof(string)));
			dt.Columns.Add(new DataColumn("type", typeof(string)));
			dt.Columns.Add(new DataColumn("target", typeof(string)));
			dt.Columns.Add(new DataColumn("info", typeof(string)));

			dt.Columns.Add(new DataColumn("check", typeof(string)));
			dt.Columns.Add(new DataColumn("T", typeof(string))); // property=P, event=E			
			//dt.Columns.Add(new DataColumn("IsInit", typeof(bool))); // InitializeComponent() 내에 있는 코드입니다.
			dt.Columns.Add(new DataColumn("top", typeof(int)));
			dt.Columns.Add(new DataColumn("left", typeof(int)));
			dt.Columns.Add(new DataColumn("index", typeof(int))); // index
			dt.Columns.Add(new DataColumn("sort", typeof(int)));
			dt.Columns.Add(new DataColumn("depth", typeof(string)));
			dt.Columns.Add(new DataColumn("depthAdd", typeof(string)));
			dt.Columns.Add(new DataColumn("line", typeof(string)));

			return dt;
		}

		/// <summary>
		/// 새 행을 초기화하여 반환합니다.
		/// </summary>
		private DataRow GetNewRow(DataTable dt)
		{
			var row = dt.NewRow();
			//row["IsInit"] = false;			
			dt.Rows.Add(row);
			return row;
		}

		/// <summary>
		/// 반환형식에 맞게 테이블을 보정하여 가져옵니다.
		/// </summary>
		private DataTable GetAmendTable(string code, DataTable dt)
		{
			if (dt.Rows.Count < 1) return dt;

			var dtRows = dt.AsEnumerable();

			{	// Depth 가 null 인 UI 요소는 화면에 배치되지 않은 컨트롤 이므로 제거됩니다.
				var targetRows = dtRows
					.Where(p => p.IsNull("depth"));

				foreach (var targetRow in targetRows.ToArray())
				{
					dt.Rows.Remove(targetRow);
				}
			}

            {	// 제거대상 로우
                var targetRows = dtRows
                    .Where(p => 
                        string.IsNullOrEmpty(p.ToStr("대상"))
                        && string.IsNullOrEmpty(p.ToStr("값"))
                        && string.IsNullOrEmpty(p.ToStr("바인딩")));

                foreach (var targetRow in targetRows.ToArray())
                {
                    if (dtRows.Where(p => p.ToStr("Name").Equals(targetRow.ToStr("Name"))).Count() > 1)
                        dt.Rows.Remove(targetRow);
                }
            }

			if (string.IsNullOrEmpty(this.initCode) != true)
			{	// NumericUpDown의 Minimum, Maximum, Value 는 디자인타임에서 개행되어 표기되므로 보정합니다.				
				var targetRows = dtRows
					.Where(p => p.ToStr("type").Equals("NumericUpDown")
						&& p.ToStr("target").Contains("Minimum", "Maximum", "Value")
					);

				foreach (var row in targetRows)
				{
					var startStr = string.Format("this.{0}.{1} = ", row.ToStr("name"), row.ToStr("target"));
					var line = this.initCode.Between(startStr, ";");
					if (string.IsNullOrEmpty(line)) continue;

					var value = Regex.Matches(line, @"[\d]+").Cast<Match>().FirstOrDefault();
					if (value == null) continue;

					row["값"] = value.ToString();
				}

			}
			{	// DateTimePicker 타입의 Text, Value 프로퍼티는 수집목록에서 제거됩니다.
				var targetRows = dtRows
					.Where(p => p.ToStr("type").Equals("DateTimePicker")
						&& p.ToStr("target").Contains("Text", "Value")
					);

				foreach (var targetRow in targetRows.ToArray())
				{
					dt.Rows.Remove(targetRow);
				}
			}
			{	// 속성 SelectedIndex, 값 0 은 목록에서 제거됩니다.
				var targetRows = dtRows
					.Where(p => p.ToStr("target").Equals("SelectedIndex")
						&& (p.IsNull("값") != true && p.ToStr("값").Equals("0"))
					);

				foreach (var targetRow in targetRows.ToArray())
				{
					dt.Rows.Remove(targetRow);
				}
			}
			{	// 같은 Name 으로 단일 프로퍼티가 적용된 속성은 바인딩을 제거합니다.
				var targetRows = dtRows
					.Where(p => "P".Equals(p.ToStr("T")))
					.GroupBy(item => new { Name = item.Field<string>("Name"), Target = item.Field<string>("대상") })
					.Select(group => new { Name = group.Key.Name, Target = group.Key.Target, Count = group.Count() });

				// 속성 설정이 1개 행일 경우 바인딩 대상이 아니므로 바인딩을 제거합니다.
				foreach (var targetRow in targetRows.Where(p => p.Count < 2))
				{
					var rows = dtRows.Where(p => p.ToStr("Name").Equals(targetRow.Name)
						&& p.ToStr("대상").Equals(targetRow.Target)
						//&& p.Field<bool>("IsInit")
						);

					foreach (var row in rows)
					{
						if (row.ToStr("info") == "resx" || IsInit(code, row.ToStr("line")))
							row["바인딩"] = null;
					}
				}

				// 속성 설정이 2개 이상일 경우 한개행만 남겨두고 모두 제거합니다.
				foreach (var targetRow in targetRows.Where(p => p.Count > 1).ToArray())
				{
					var rows = dtRows.Where(p => p.ToStr("Name").Equals(targetRow.Name)
						&& p.ToStr("대상").Equals(targetRow.Target)						
						);
                    //&& string.IsNullOrEmpty(p.ToStr("check"))
                    //&& p.Field<bool>("IsInit") != true

					//var skipRow = rows.LastOrDefault();
                    var skipRow = rows.OrderByDescending(p => p.ToStr("값")).ThenByDescending(p => p.ToStr("info")).FirstOrDefault();                    
                    if (IsInit(code, skipRow.ToStr("line")))
                        skipRow["바인딩"] = null;

                    bool bInitValueApply = false;
					foreach (var row in rows.ToArray())
					{
						if (row.Equals(skipRow)) continue;

                        //// 2개행이 유일하고 제거대상이 디자인라인일 경우 바인딩 대상이 아님
                        //if (rows.Count() == 2 && IsInit(code, row.ToStr("line")))
                        //    skipRow["바인딩"] = null;

                        if (bInitValueApply != true && IsInit(code, row.ToStr("line")) && string.IsNullOrEmpty(row.ToStr("값")) != true)
                        {
                            bInitValueApply = true;
                            skipRow["값"] = row.ToStr("값");
                        }

						dt.Rows.Remove(row);
					}
				}
			}
			{	// FpSpread 의 Text속성을 마킹 하고 제거합니다.
				var targetRows = dtRows
					.Where(p => "Text".Equals(p.ToStr("대상"))
						&& "FpSpread".Equals(p.ToStr("type")));

				foreach (var targetRow in targetRows.ToArray())
				{
					WriteCheckLine(targetRow.ToStr("line"));
					dt.Rows.Remove(targetRow);
				}
			}
			{	// SheetView 타입을 마킹 하고 제거합니다.
				var targetRows = dtRows
					.Where(p => "SheetView".Equals(p.ToStr("type")));

				foreach (var targetRow in targetRows.ToArray())
				{
					WriteCheckLine(targetRow.ToStr("line"));
					dt.Rows.Remove(targetRow);
				}
			}
			{	// Sheets를 엑세스 하는 라인을 마킹 하고 제거합니다.
				var targetRows = dtRows
					.Where(p => string.IsNullOrEmpty(p.ToStr("line")) != true
						&& (p.ToStr("line").IndexOf("ActiveSheet") > 0 || p.ToStr("line").IndexOf("Sheets") > 0));

				foreach (var targetRow in targetRows.ToArray())
				{
					var line = targetRow.ToStr("line");

					if (line.IndexOf("=") > 0)
					{
						var target = line.Substring(0, line.IndexOf("="));
						if ((target.IndexOf("ActiveSheet") > 0 || target.IndexOf("Sheets") > 0) != true)
							continue;

					}

					WriteCheckLine(targetRow.ToStr("line"));
					dt.Rows.Remove(targetRow);
				}
			}

			foreach (var row in dt.AsEnumerable())
			{
				if (row.ToStr("대상").Contains("Content", "Header", "Text"))
					row["sort"] = 1;
				else
					row["sort"] = 99;
			}


			dt.DefaultView.Sort = "depth, top, left, index, sort, 대상";
			string name = null;
			foreach (DataRowView rowView in dt.DefaultView)
			{
				var row = rowView.Row;
				if (row.ToStr("name").Equals(name))
					row["컨트롤명"] = null;
				else
					name = row.ToStr("name");
			}

			return dt;
		}

		private void WriteCheckLine(string line)
		{
			if (string.IsNullOrEmpty(line)) return;

			if (string.IsNullOrEmpty(line) != true)
			{
				CheckLines.Add(new CheckLineItem() { Line = line });
			}
		}

		/// <summary>
		/// 이 코드라인이 InitializeComponent() 함수 내에 위치한 코드일 경우 True
		/// </summary>
		private bool IsInit(string code, string line)
		{
            if (string.IsNullOrEmpty(line)) return false;

			var index = code.IndexOf(line);
			return index > initIndexStart && index < initIndexEnd;
		}

		/// <summary>
		/// 프로퍼티이름으로 매핑되는 컨트롤명을 가져옵니다.
		/// </summary>
		private string GetMappingType(string typeName)
		{
			if (mappingTypeDictionary == null)
				mappingTypeDictionary = GetMappingTypeDictionary();

			var dic = mappingTypeDictionary;
			if (dic.ContainsKey(typeName))
			{
				return dic[typeName];
			}

			return typeName;
		}

		private Dictionary<string, string> mappingTypeDictionary;
		private Dictionary<string, string> GetMappingTypeDictionary()
		{
			var dic = new Dictionary<string, string>();

			dic.Add("FpSpread", "DataGrid");
			dic.Add("DateTimePicker", "DateTime");
			dic.Add("TabPage", "TabItem");
			dic.Add("CodeCombo", "ComboBox");
			dic.Add("FrameCtrl", "Container");
			dic.Add("CrystalReportViewer", "OzViewer");

			return dic;
		}

		/// <summary>
		/// 프로퍼티이름으로 매핑되는 프로퍼티명을 가져옵니다.
		/// </summary>
		private string GetMappingProperty(string typeName, string propertyName)
		{
			if (mappingPropertyDictionary == null)
				mappingPropertyDictionary = GetMappingPropertyDictionary();

			var dic = mappingPropertyDictionary;
			if (dic.ContainsKey(typeName)
				&& dic[typeName].ContainsKey(propertyName))
			{
				return dic[typeName][propertyName];
			}

			return propertyName;
		}

		private Dictionary<string, Dictionary<string, string>> mappingPropertyDictionary;
		private Dictionary<string, Dictionary<string, string>> GetMappingPropertyDictionary()
		{
			var dic = new Dictionary<string, Dictionary<string, string>>();
			{	// Button
				var list = new Dictionary<string, string>();
				list.Add("Text", "Content");
				dic.Add("Button", list);
			}
			{	// CheckBox
				var list = new Dictionary<string, string>();
				list.Add("Text", "Content");
				list.Add("Checked", "IsChecked");
				dic.Add("CheckBox", list);
			}
			{	// ComboBox
				var list = new Dictionary<string, string>();
				list.Add("DataSource", "ItemsSource");
				list.Add("DataMember", "SelectedItem");
				list.Add("DisplayMember", "Content");
				dic.Add("ComboBox", list);
			}
			{	// ComboBoxItem
				var list = new Dictionary<string, string>();
				list.Add("DisplayMember", "Content");
				dic.Add("ComboBoxItem", list);
			}
			{	// ListBox
				var list = new Dictionary<string, string>();
				list.Add("DataSource", "ItemsSource");
				list.Add("DataMember", "SelectedItem");
				list.Add("DisplayMember", "Content");
				dic.Add("ListBox", list);
			}
			{	// ListBoxItem
				var list = new Dictionary<string, string>();
				list.Add("DisplayMember", "Content");
				dic.Add("ListBoxItem", list);
			}
			{	// DateTimePicker
				var list = new Dictionary<string, string>();
				// Value, Text 는 수집 대상이 아니며, DateTimePicker는 Date가 기본적으로 바인딩요소로 추가되어 있다.
				dic.Add("DateTimePicker", list);
			}
			{	// GroupBox
				var list = new Dictionary<string, string>();
				list.Add("Text", "Header");
				dic.Add("GroupBox", list);
			}
			{	// RadioButton
				var list = new Dictionary<string, string>();
				list.Add("Text", "Content");
				list.Add("Checked", "IsChecked");
				dic.Add("RadioButton", list);
			}
			{	// TabItem
				var list = new Dictionary<string, string>();
				list.Add("Text", "Header");
				dic.Add("TabItem", list);
			}
			{	// ToggleButton
				var list = new Dictionary<string, string>();
				list.Add("Text", "Content");
				dic.Add("ToggleButton", list);
			}
			{	// NumericUpDown
				var list = new Dictionary<string, string>();
				list.Add("Minimum", "Min");
				list.Add("Maximum", "Max");
				dic.Add("NumericUpDown", list);
			}

			return dic;
		}		

		#endregion        
    }

    public partial class ComponentAnalysis
    {
        public class ComponentPropertyInfo
        {
            public string Name { get; set; }
            public string Target { get; set; }
            public string Value { get; set; }
            public string Line { get; set; }
        }

        public class CheckLineItem
        {
            public string Line { get; set; }
        }

        public class DepthItem
        {
            public string Name { get; set; }
            public int Left { get; set; }
            public int Top { get; set; }
            public List<DepthItem> Items { get; set; }

            public DepthItem()
            {
                this.Items = new List<DepthItem>();
            }
        }
    }
}