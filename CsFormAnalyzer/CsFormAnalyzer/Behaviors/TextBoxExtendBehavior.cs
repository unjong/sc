using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CsFormAnalyzer.Behaviors
{
	public static class TextBoxExtendBehavior
	{
		#region SearchText

		public static string GetSearchText(DependencyObject obj)
		{
			return (string)obj.GetValue(SearchTextProperty);
		}

		public static void SetSearchText(DependencyObject obj, string value)
		{
			obj.SetValue(SearchTextProperty, value);
		}

		public static readonly DependencyProperty SearchTextProperty =
			DependencyProperty.RegisterAttached("SearchText",
			typeof(string), typeof(TextBoxExtendBehavior),
			new UIPropertyMetadata(default(string), OnSearchTextChanged));


		private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var searchText = Convert.ToString(e.NewValue);

			try
			{
				var txtBox = d as TextBox;
				txtBox.SelectionStart = txtBox.Text.IndexOf(searchText);
				txtBox.SelectionLength = searchText.Length;
				txtBox.ScrollToLine(txtBox.GetLineIndexFromCharacterIndex(txtBox.SelectionStart));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + "OnSearchTextChanged");
			}
		}

		#endregion
	}
}
