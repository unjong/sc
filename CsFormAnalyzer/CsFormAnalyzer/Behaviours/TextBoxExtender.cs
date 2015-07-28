using CsFormAnalyzer.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CsFormAnalyzer.Behaviours
{
	public static class TextBoxExtender
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
			typeof(string), typeof(TextBoxExtender),
            new FrameworkPropertyMetadata(string.Empty, OnSearchTextChanged));

		private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var searchText = Convert.ToString(e.NewValue);
            if (string.IsNullOrEmpty(searchText)) return;

			try
			{
                if (d is TextBox)
                {
                    var target = d as TextBox;
                    target.Dispatcher.BeginInvoke(DispatcherPriority.Input, (Action)(() =>
                    {
                        target.SelectionStart = target.Text.IndexOf(searchText);
                        target.SelectionLength = searchText.Length;
                        target.ScrollToLine(target.GetLineIndexFromCharacterIndex(target.SelectionStart));
                    }));
                }
                else if (d is ICSharpCode.AvalonEdit.TextEditor)
                {
                    var target = d as ICSharpCode.AvalonEdit.TextEditor;
                    target.Dispatcher.BeginInvoke(DispatcherPriority.Input, (Action)(() =>
                    {
                        var start = target.Text.IndexOf(searchText);
                        target.Select(start, searchText.Length);

                        var line = target.Document.GetLineByOffset(target.CaretOffset);
                        target.Select(line.Offset, line.Length);
                        target.ScrollToLine(line.LineNumber);

                        target.TextArea.Caret.BringCaretToView();
                    }));
                }
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + "OnSearchTextChanged");
			}
		}

		#endregion

        #region SelectAllOnFocus

        public static bool GetSelectAllOnFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(SelectAllOnFocusProperty);
        }

        public static void SetSelectAllOnFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(SelectAllOnFocusProperty, value);
        }

        public static readonly DependencyProperty SelectAllOnFocusProperty =
            DependencyProperty.RegisterAttached("SelectAllOnFocus",
            typeof(bool), typeof(TextBoxExtender),
            new FrameworkPropertyMetadata(false, OnSelectAllOnFocusChanged));

        private static void OnSelectAllOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var txtBox = d as TextBox;

            if ((bool)e.NewValue)
            {
                txtBox.GotFocus += TextBoxGotFocus;
            }
            else
            {
                txtBox.GotFocus -= TextBoxGotFocus;
            }
        }

        static void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            var txtBox = sender as TextBox;
            if (txtBox == null)
                return;
            if (GetSelectAllOnFocus(txtBox))
            {
                txtBox.Dispatcher.BeginInvoke((Action)(txtBox.SelectAll));
            }
        }

        static void PasswordGotFocus(object sender, RoutedEventArgs e)
        {
            var passBox = sender as PasswordBox;
            if (passBox == null)
                return;
            if (GetSelectAllOnFocus(passBox))
            {
                passBox.Dispatcher.BeginInvoke((Action)(passBox.SelectAll));
            }
        }

        #endregion

        #region MoveFocus

        public static object GetMoveFocus(TextBox obj)
        {
            return (object)obj.GetValue(MoveFocus);
        }

        public static void SetMoveFocus(TextBox obj, Key value)
        {
            obj.SetValue(MoveFocus, value);
        }

        public static readonly DependencyProperty MoveFocus =
            DependencyProperty.RegisterAttached("MoveFocus",
            typeof(object), typeof(TextBoxExtender),
            new FrameworkPropertyMetadata(Key.None, OnMoveFocusChanged));

        private static void OnMoveFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var key = e.NewValue is Key ? (Key)e.NewValue : Key.Enter;
            var focusElement = e.NewValue as UIElement;

            var keyTrigger = new Microsoft.Expression.Interactivity.Input.KeyTrigger()
            {
                Key = key,                
                ActiveOnFocus = true,
            };
            keyTrigger.Actions.Add(new MoveFocusAction() { FocusElement = focusElement });

            var triggers = System.Windows.Interactivity.Interaction.GetTriggers(d);
            triggers.Add(keyTrigger);
        }

        #endregion
	}
}
