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
        public string ExceptValues { get; set; }

		internal void LoadDefaultConfig()
		{
            this.SearchTypes = "Label, Button, RadioButton, CheckBox, LDateTimePicker, DateTimePicker, ComboBox, ListBox, TextBox, RichTextBox, FpSpread, TabControl, TabPage, GroupBox, Panel, CodeCombo, LMaskEdBox, PictureBox, TreeView, CheckedListBox, SheetView, FrameCtrl, Splitter, LFloatTextBox, CrystalReportViewer, NumericUpDown, StatusBa, StatusBarPanel, FlowLayoutPanel, PatientAlarm, AxWebBrowser, LinkLabel, ScheduleMonthCalendarUC, MonthCalendar, ProgressBar";
            this.SearchProperties = "ReadOnly, Text, Visible, MaxLength, Multiline,IsChecked, Value, Enabled, Checked,DisplayMember, ValueMember, Value, MaxDate, MinDate, Format, SelectedIndex, CustomFormat, ContextMenu, TipAppearance, Maximum, Image";
            this.SearchEvents = "Click, CellClick, KeyPress, KeyDown, TextChanged, ButtonClicked, CheckedChanged, CellDoubleClick, KeyUp, TextTipFetch, SelectedIndexChanged,ValueChanged, MouseUp, EditModeOff, EditModeOn, EnterCell, Leave, ComboSelChange, EditChange, AfterCheck, TextChanged, SelectionChangeCommitted,ButtonClicked,LinkClicked, ClickDate, CurrentViewMonthChanged, MouseHover, SelectionChangeCommitted";
            this.RemovePrefixs = "lbl, btn, rdo, chk, dtp, cbo, lst, txt, cbo, chb, cbx, spd, chx, fps, fpSpread, pbx, pic, txb, prog";
            this.SelectorTypes = "ComboBox, ListBox, CodeCombo, FpSpread, TreeView, CheckedListBox";
            this.ExceptValues = "#FFFFFF, #000000, #333333";
		}
    }
}
