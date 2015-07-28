using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CsFormAnalyzer.Behaviours
{
	/// <summary>
	/// 컨트롤의 상태 유지,복원에 대한 Behavior를 지원합니다.
	/// </summary>
	public static class StateRestoreBehavior
	{
		#region Window StateRestore

		public static string GetWindow(DependencyObject obj)
		{
			return (string)obj.GetValue(WindowProperty);
		}

		public static void SetWindow(DependencyObject obj, string value)
		{
			obj.SetValue(WindowProperty, value);
		}

		public static readonly DependencyProperty WindowProperty =
			DependencyProperty.RegisterAttached("Window",
			typeof(string), typeof(TextBoxExtender),
			new UIPropertyMetadata(default(string), OnWindowChanged));


		private static void OnWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (DesignerProperties.GetIsInDesignMode(d)) return;

			var win = d as Window;
			var key = e.NewValue.ToString();
			if (string.IsNullOrEmpty(key)) return;

            var top = AppManager.Current.Settings.Get(key + ".Top");
            var left = AppManager.Current.Settings.Get(key + ".Left");
            if (top != null) win.Top = Convert.ToDouble(top);
            if (left != null) win.Left = Convert.ToDouble(left);

			var width = AppManager.Current.Settings.Get(key + ".Width");
			var height = AppManager.Current.Settings.Get(key + ".Height");
			if (width != null) win.Width = Convert.ToDouble(width);
			if (height != null) win.Height = Convert.ToDouble(height);

			var windowState = AppManager.Current.Settings.Get(key + ".WindowState");
			if (windowState != null) win.WindowState = (WindowState)Enum.Parse(typeof(WindowState), windowState);

            win.LocationChanged += delegate
            {
                if (win.IsLoaded != true) return;
                AppManager.Current.Settings.Set(key + ".Top", win.Top.ToString());
                AppManager.Current.Settings.Set(key + ".Left", win.Left.ToString());
            };

			win.SizeChanged += delegate
			{
				if (win.IsLoaded != true) return;
				AppManager.Current.Settings.Set(key + ".Width", win.Width.ToString());
				AppManager.Current.Settings.Set(key + ".Height", win.Height.ToString());
			};

			win.StateChanged += delegate
			{
				if (win.IsLoaded != true) return;
				AppManager.Current.Settings.Set(key + ".WindowState", win.WindowState.ToString());
			};
		}

		#endregion

		#region GridSplitter StateRestore

		public static string GetGridSplitter(DependencyObject obj)
		{
			return (string)obj.GetValue(GridSplitterProperty);
		}

		public static void SetGridSplitter(DependencyObject obj, string value)
		{
			obj.SetValue(GridSplitterProperty, value);
		}

		public static readonly DependencyProperty GridSplitterProperty =
			DependencyProperty.RegisterAttached("GridSplitter",
			typeof(string), typeof(TextBoxExtender),
			new UIPropertyMetadata(default(string), OnGridSplitterChanged));


		private static void OnGridSplitterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (DesignerProperties.GetIsInDesignMode(d)) return;

			var gs = d as GridSplitter;
			var key = e.NewValue.ToString();
			if (string.IsNullOrEmpty(key)) return;

			gs.DragCompleted += delegate { Save(gs, key); };
            gs.SizeChanged += gs_SizeChanged;
		}

        static void gs_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var gs = sender as GridSplitter;
            gs.SizeChanged -= gs_SizeChanged;

            gs.Dispatcher.BeginInvoke(DispatcherPriority.Input, (Action)(() =>
            {
                Load(gs, StateRestoreBehavior.GetGridSplitter(gs));
            }));
        }

		private static void Save(GridSplitter gs, string key)
		{
			var grid = gs.Parent as Grid;			
			
			if (gs.ActualWidth < gs.ActualHeight) // 가로
			{
                for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
                {
                    var setKey = string.Join(".", key, i, "Width");
                    var value = grid.ColumnDefinitions[i].Width.ToString();
                    AppManager.Current.Settings.Set(setKey, value);
                }
			}
			else
			{
                for (int i = 0; i < grid.RowDefinitions.Count; i++)
                {
                    var setKey = string.Join(".", key, i, "Height");
                    var value = grid.RowDefinitions[i].Height.ToString();
                    AppManager.Current.Settings.Set(setKey, value);
                }
			}
		}

		private static void Load(GridSplitter gs, string key)
		{
			var grid = gs.Parent as Grid;

            if (gs.ActualWidth < gs.ActualHeight) // 가로
            {
                var v = AppManager.Current.Settings.Get(string.Join(".", key, 0, "Width"));
                if (v != null)
                {
                    for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
                    {
                        var getKey = string.Join(".", key, i, "Width");
                        var width = AppManager.Current.Settings.Get(getKey);
                        if (width != null)
                            grid.ColumnDefinitions[i].Width = ToGridLength(width, false);
                    }
                }
            }
            else
            {
                var v = AppManager.Current.Settings.Get(string.Join(".", key, 0, "Height"));
                if (v != null)
                {
                    for (int i = 0; i < grid.RowDefinitions.Count; i++)
                    {
                        var getKey = string.Join(".", key, i, "Height");
                        var height = AppManager.Current.Settings.Get(getKey);
                        if (height != null)
                            grid.RowDefinitions[i].Height = ToGridLength(height, false);
                    }
                }
            }
		}

		private static GridLength ToGridLength(string value, bool IsStarReturn = false)
		{
			if (value == "Auto")
				return GridLength.Auto;
			else if (value == "*")
				return new GridLength(1, GridUnitType.Star);
			else if (IsStarReturn)
				return new GridLength(System.Convert.ToDouble(value), GridUnitType.Star);
			else if (value.IndexOf("*") > 0)
			{
				string result = System.Text.RegularExpressions.Regex.Replace(value, @"[^\d.]", "");
				return new GridLength(System.Convert.ToDouble(result), GridUnitType.Star);
			}	
			else
				return new GridLength(System.Convert.ToDouble(value), GridUnitType.Pixel);
		}

		#endregion

        #region TextBox StateRestore

        public static string GetTextBox(DependencyObject obj)
        {
            return (string)obj.GetValue(TextBoxProperty);
        }

        public static void SetTextBox(DependencyObject obj, string value)
        {
            obj.SetValue(TextBoxProperty, value);
        }

        public static readonly DependencyProperty TextBoxProperty =
            DependencyProperty.RegisterAttached("TextBox",
            typeof(string), typeof(TextBoxExtender),
            new UIPropertyMetadata(default(string), OnTextBoxChanged));
        
        private static void OnTextBoxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(d)) return;

            var txt = d as TextBox;
            var key = e.NewValue.ToString();
            if (string.IsNullOrEmpty(key)) return;

            string oldValue = null, newValue = null;

            txt.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (Action)(() => 
            {
                var restoreText = AppManager.Current.Settings.Get(key + ".Text");
                if (restoreText != null)
                {
                    txt.SetCurrentValue(TextBox.TextProperty, restoreText);
                    var be = txt.GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();
                    
                }                    
            }));

            txt.GotFocus += delegate
            {
                oldValue = txt.GetValue(TextBox.TextProperty) as string;
            };
            txt.LostFocus += delegate
            {
                newValue = txt.GetValue(TextBox.TextProperty) as string;

                if (oldValue != newValue)
                {
                    AppManager.Current.Settings.Set(key + ".Text", newValue);
                }
            };
        }
        
        #endregion
	}
}
