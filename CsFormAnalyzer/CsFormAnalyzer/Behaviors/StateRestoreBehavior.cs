using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
			var guid = e.NewValue.ToString();

			var width = AppManager.Current.Settings.Get(guid + ".Width");
			var height = AppManager.Current.Settings.Get(guid + ".Height");
			if (width != null) win.Width = Convert.ToDouble(width);
			if (height != null) win.Height = Convert.ToDouble(height);

			var windowState = AppManager.Current.Settings.Get(guid + ".WindowState");
			if (windowState != null) win.WindowState = (WindowState)Enum.Parse(typeof(WindowState), windowState);

			win.SizeChanged += delegate
			{
				if (win.IsLoaded != true) return;
				AppManager.Current.Settings.Set(guid + ".Width", win.Width.ToString());
				AppManager.Current.Settings.Set(guid + ".Height", win.Height.ToString());
			};

			win.StateChanged += delegate
			{
				if (win.IsLoaded != true) return;
				AppManager.Current.Settings.Set(guid + ".WindowState", win.WindowState.ToString());
			};
		}

		#endregion
	}
}
