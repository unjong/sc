using CsFormAnalyzer.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CsFormAnalyzer.Behaviours
{
    public static class MvvmBehavior
    {
        #region Link View-ViewModel // 뷰모델이 뷰를 알 수 있도록 합니다.

        public static bool GetView(DependencyObject obj)
        {
            return (bool)obj.GetValue(ViewProperty);
        }

        public static void SetView(DependencyObject obj, bool value)
        {
            obj.SetValue(ViewProperty, value);
        }

        public static readonly DependencyProperty ViewProperty =
            DependencyProperty.RegisterAttached("View",
            typeof(bool), typeof(MvvmBehavior),
            new FrameworkPropertyMetadata(false, OnViewChanged));
            //new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, OnViewChanged));

        private static void OnViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {            
            var view = d as FrameworkElement;
            if (view == null) return;

            view.DataContextChanged += delegate
            {
                var vm = view.DataContext as ViewModelBase;
                if (vm == null || view.Equals(vm.ViewControl)) return;
                vm.ViewControl = view;
            };
        }

        #endregion
    }
}
