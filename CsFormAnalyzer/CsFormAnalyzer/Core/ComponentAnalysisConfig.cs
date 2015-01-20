using CsFormAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Core
{
	public class ComponentAnalysisConfig
	{
		public string SearchTypes { get; set; }
		public string SearchProperties { get; set; }
		public string SearchEvents { get; set; }
		public string RemovePrefixs { get; set; }
		public string SelectorTypes { get; set; }

		internal void LoadDefaultConfig()
		{
			this.SearchTypes = "Label, Button, RadioButton, CheckBox, DateTimePicker, ComboBox, ListBox, TextBox, RichTextBox, FpSpread, TabControl, TabPage, GroupBox, Panel, CodeCombo, LMaskEdBox, PictureBox, TreeView, CheckedListBox, SheetView, FrameCtrl, Splitter";
			this.SearchProperties = "ReadOnly, Text, Visible, MaxLength, Multiline,IsChecked, Value, Enabled, Checked,DisplayMember, ValueMember, Value, MaxDate, MinDate, Format, SelectedIndex";
			this.SearchEvents = "Click, CellClick, KeyPress, KeyDown, TextChanged, ButtonClicked, CheckedChanged, CellDoubleClick, KeyUp, TextTipFetch, SelectedIndexChanged,ValueChanged";
			this.RemovePrefixs = "lbl, btn, rdo, chk, dtp, cbo, lst, txt, cbo, chb, cbx, spd";
			this.SelectorTypes = "ComboBox, ListBox, CodeCombo, FpSpread, TreeView, CheckedListBox";
		}
	}
}
