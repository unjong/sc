using CsFormAnalyzer.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace CsFormAnalyzer.Controls
{
    public class SCContentControl : ContentControl
    {
        const string TargetControlName = "PART_Indicator";

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ViewModelBase), typeof(SCContentControl), new PropertyMetadata(new PropertyChangedCallback(OnViewModelChanged)));

        public static readonly DependencyProperty IsAccessDeniedProperty =
            DependencyProperty.Register("IsAccessDenied", typeof(bool), typeof(SCContentControl));

        public static readonly DependencyProperty SCViewProperty =
            DependencyProperty.Register("SCView", typeof(FrameworkElement), typeof(SCContentControl));

        static SCContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SCContentControl), new FrameworkPropertyMetadata(typeof(SCContentControl)));
        }

        public ViewModelBase ViewModel
        {
            get { return (ViewModelBase)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public bool IsAccessDenied
        {
            get { return (bool)GetValue(IsAccessDeniedProperty); }
            set { SetValue(IsAccessDeniedProperty, value); }
        }

        public FrameworkElement SCView
        {
            get
            {
                return (FrameworkElement)GetValue(SCViewProperty); 
            }
            set { SetValue(SCViewProperty, value); }
        }

        static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var sc = d as SCContentControl;
            sc.OnViewModelChanged(args.NewValue as ViewModelBase);
        }

        void OnViewModelChanged(ViewModelBase vModel)
        {
            if (vModel != null)
            {

            }
            else
            {
                SCView = null;
            }
        }
    }
}
