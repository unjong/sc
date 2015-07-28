using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xaml;

namespace CsFormAnalyzer.Behaviours
{
    public class BindAssist
    {
        #region DependencyProperties

        public static readonly DependencyProperty BindProperty =
            DependencyProperty.RegisterAttached("Bind",
            typeof(string), typeof(BindAssist),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBindChanged));

        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach",
            typeof(bool), typeof(BindAssist), new PropertyMetadata(false, Attach));

        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached("IsUpdating", typeof(bool),
            typeof(BindAssist));

        public static void SetAttach(DependencyObject dp, bool value)
        {
            dp.SetValue(AttachProperty, value);
        }

        public static bool GetAttach(DependencyObject dp)
        {
            return (bool)dp.GetValue(AttachProperty);
        }

        public static string GetBind(DependencyObject dp)
        {
            return (string)dp.GetValue(BindProperty);
        }

        public static void SetBind(DependencyObject dp, string value)
        {
            dp.SetValue(BindProperty, value);
        }

        private static bool GetIsUpdating(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }

        private static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }

        #endregion

        private static void Attach(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox)
            {
                var target = d as PasswordBox;
                if ((bool)e.OldValue)
                    target.PasswordChanged -= OnValueChanged;

                if ((bool)e.NewValue)
                    target.PasswordChanged += OnValueChanged;
            }
            else if (d is ICSharpCode.AvalonEdit.TextEditor)
            {
                var target = d as ICSharpCode.AvalonEdit.TextEditor;
                if ((bool)e.OldValue)
                    target.TextChanged -= OnValueChanged;

                if ((bool)e.NewValue)
                    target.TextChanged += OnValueChanged;
            }
        }

        static void OnValueChanged(object sender, EventArgs e)
        {
            var d = sender as DependencyObject;
            SetIsUpdating(d, true);

            if (d is PasswordBox)
                d.SetCurrentValue(BindProperty, ((PasswordBox)d).Password);

            else if (d is ICSharpCode.AvalonEdit.TextEditor)
                d.SetCurrentValue(BindProperty, ((ICSharpCode.AvalonEdit.TextEditor)d).Text);

            SetIsUpdating(d, false);
        }

        private static void OnBindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox)
            {
                var target = d as PasswordBox;
                target.PasswordChanged -= OnValueChanged;
                if (!(bool)GetIsUpdating(target))
                    target.Password = (string)e.NewValue;

                target.PasswordChanged += OnValueChanged;
            }
            else if (d is ICSharpCode.AvalonEdit.TextEditor)
            {
                var target = d as ICSharpCode.AvalonEdit.TextEditor;
                target.TextChanged -= OnValueChanged;
                if (!(bool)GetIsUpdating(target))
                    target.Text = (string)e.NewValue;

                target.TextChanged += OnValueChanged;
            }
        }

        #region SelectedText

        public static readonly DependencyProperty SelectedTextProperty =
            DependencyProperty.RegisterAttached("SelectedText",
            typeof(string), typeof(BindAssist),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedTextChanged));

        public static string GetSelectedText(DependencyObject dp)
        {
            return (string)dp.GetValue(SelectedTextProperty);
        }

        public static void SetSelectedText(DependencyObject dp, string value)
        {
            dp.SetValue(SelectedTextProperty, value);
        }

        private static void OnSelectedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as ICSharpCode.AvalonEdit.TextEditor;
            target.LostFocus += delegate
            {
                SetSelectedText(d, target.SelectedText);
            };
        }

        #endregion
        
    }
}