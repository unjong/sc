using CsFormAnalyzer.Controls;
using CsFormAnalyzer.Data;
using CsFormAnalyzer.Mvvm;
using CsFormAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace CsFormAnalyzer
{
    public class AppManager : Singleton<AppManager>
	{
        private AppManager() 
        { 
        }
        
        public static string DataConnectionString = @"Data Source=10.28.16.54\scproject;Initial Catalog=HISBIZ;Integrated Security=False;User ID=sa;Password=password!234;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";
        public static string ServerConnectionString = @"Data Source=10.10.11.20;Initial Catalog=HISS;User Id=dev_user;Password=password1!;Application Name=SAF30;MultipleActiveResultSets=True;";

		public LocalSettings Settings = new LocalSettings(@"UserSetting.config");
        public DbUnit MainDbUnit { get; set; }

        #region Show View

        internal void ShowView(Type viewType, Mvvm.ViewModelBase vm, Window owner = null)
        {
            var win = LoadWindow(viewType, vm, owner);
            win.Show();
        }

        internal bool ShowDialogView(Type viewType, Mvvm.ViewModelBase vm, Window owner = null)
        {
            var win = LoadWindow(viewType, vm, owner);
            return (bool)win.ShowDialog();
        }

        private Window LoadWindow(Type viewType, Mvvm.ViewModelBase vm, Window owner)
        {
            var view = ReflectionHelper.CreateNewObjectFactory<FrameworkElement>(viewType);
            view.DataContext = vm;

            var win = new SCPopWinBase(vm);
            if (win != Application.Current.MainWindow)
            {
                win.Owner = owner ?? Application.Current.MainWindow;
            }            
            win.layoutRootGrid.Children.Add(view);
            
            Behaviours.StateRestoreBehavior.SetWindow(win, vm.GetType().Name);

            if (string.IsNullOrEmpty(vm.Title) != true) win.Title = vm.Title;

            win.Activate();
            win.Topmost = true;
            win.Topmost = false;
            win.Focus(); 

            return win;
        }

        #endregion

        #region Show Popup

        internal void ShowPopup(UIElement element, UIElement parent = null)
        {
            // var settings = new PopupSettings();
            var border = new Border();
            border.Padding = new Thickness(4);
            border.Background = new SolidColorBrush(Colors.White);
            border.BorderBrush = new SolidColorBrush(Colors.Blue);
            border.BorderThickness = new Thickness(1);
            border.Child = element;

            var popup = new Popup();
            popup.Child = border;
            popup.PlacementTarget = parent;
            popup.Placement = PlacementMode.MousePoint;
            popup.StaysOpen = false;
            popup.IsOpen = true;
        }

        #endregion

        #region Show Window
        #endregion

        public void BeginInvoke(DispatcherPriority dispatcherPriority, Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(dispatcherPriority, (System.Threading.ThreadStart)delegate()
            {
                action.Invoke();
            });
        }
    }
}
