using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CsFormAnalyzer.Behaviors
{
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
			typeof(string), typeof(TextBoxExtendBehavior),
			new UIPropertyMetadata(default(string), OnWindowChanged));


		private static void OnWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (DesignerProperties.GetIsInDesignMode(d)) return;

			var win = d as Window;
			var key = e.NewValue.ToString();
			if (string.IsNullOrEmpty(key)) return;

			var width = AppManager.Current.Settings.Get(key + ".Width");
			var height = AppManager.Current.Settings.Get(key + ".Height");
			if (width != null) win.Width = Convert.ToDouble(width);
			if (height != null) win.Height = Convert.ToDouble(height);

			var windowState = AppManager.Current.Settings.Get(key + ".WindowState");
			if (windowState != null) win.WindowState = (WindowState)Enum.Parse(typeof(WindowState), windowState);

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
			typeof(string), typeof(TextBoxExtendBehavior),
			new UIPropertyMetadata(default(string), OnGridSplitterChanged));


		private static void OnGridSplitterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (DesignerProperties.GetIsInDesignMode(d)) return;

			var gs = d as GridSplitter;
			var key = e.NewValue.ToString();
			if (string.IsNullOrEmpty(key)) return;

			gs.DragCompleted += delegate { Save(gs, key); };
			gs.Loaded += delegate { Load(gs, key); };			
		}

		private static void Save(GridSplitter gs, string key)
		{
			var grid = gs.Parent as Grid;			
			

			if (gs.ActualWidth < gs.ActualHeight) // 가로
			{
				var col = Grid.GetColumn(gs);
				var width_1 = grid.ColumnDefinitions[col - 1].Width;
				var width_2 = grid.ColumnDefinitions[col + 1].Width;

				AppManager.Current.Settings.Set(key + "Width_1", width_1.ToString());
				AppManager.Current.Settings.Set(key + "Width_2", width_2.ToString());
			}
			else
			{
				var row = Grid.GetRow(gs);
				var height_1 = grid.RowDefinitions[row - 1].Height;
				var height_2 = grid.RowDefinitions[row + 1].Height;

				AppManager.Current.Settings.Set(key + "Height_1", height_1.ToString());
				AppManager.Current.Settings.Set(key + "Height_2", height_2.ToString());
			}
		}

		private static void Load(GridSplitter gs, string key)
		{
			var grid = gs.Parent as Grid;

			if (gs.ActualWidth < gs.ActualHeight) // 가로
			{
				var col = Grid.GetColumn(gs);
				var width_1 = AppManager.Current.Settings.Get(key + "Width_1");
				var width_2 = AppManager.Current.Settings.Get(key + "Width_2");
				if (width_1 != null || width_2 != null)
				{
					grid.ColumnDefinitions[col - 1].Width = ToGridLength(width_1, false);
					grid.ColumnDefinitions[col + 1].Width = ToGridLength(width_2, false);
				}
			}
			else
			{
				var row = Grid.GetRow(gs);
				var height_1 = AppManager.Current.Settings.Get(key + "Height_1");
				var height_2 = AppManager.Current.Settings.Get(key + "Height_2");
				if (height_1 != null || height_2 != null)
				{
					grid.RowDefinitions[row - 1].Height = ToGridLength(height_1, false);
					grid.RowDefinitions[row + 1].Height = ToGridLength(height_2, false);
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
				string result = System.Text.RegularExpressions.Regex.Replace(value, @"[^\d]", "");
				return new GridLength(System.Convert.ToDouble(result), GridUnitType.Star);
			}	
			else
				return new GridLength(System.Convert.ToDouble(value), GridUnitType.Pixel);
		}

		#endregion
	}
}
