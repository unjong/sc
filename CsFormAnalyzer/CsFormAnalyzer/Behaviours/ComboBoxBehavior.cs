using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace CsFormAnalyzer.Behaviours
{
    public static class ComboBoxBehavior
    {
        #region AutoComplateItemsSourceProperty

        public static IEnumerable GetAutoComplateItemsSource(ComboBox obj)
        {
            return (IEnumerable)obj.GetValue(AutoComplateItemsSourceProperty);
        }

        public static void SetAutoComplateItemsSource(ComboBox obj, IEnumerable value)
        {
            obj.SetValue(AutoComplateItemsSourceProperty, value);
        }

        /// <summary>
        /// 콤보박스의 자동완성기능을 지원합니다.
        /// ItemsSource 대신 이 프로퍼티에 소스를 바인딩 합니다.
        /// (ItemsSourceProperty, TextProperty 를 같이 사용 할 수 없습니다.)
        /// </summary>
        public static readonly DependencyProperty AutoComplateItemsSourceProperty =
            DependencyProperty.RegisterAttached("AutoComplateItemsSource",
            typeof(IEnumerable), typeof(ComboBoxBehavior),
            new FrameworkPropertyMetadata(null, OnAutoComplateItemsSourceChanged));

        private static void OnAutoComplateItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            new ComboBoxAutoComplateAssist(d as ComboBox, e.NewValue);
        }

        public class ComboBoxAutoComplateAssist
        {
            private ComboBox comboBox;
            private bool _IsRefresh;
            public ICollectionView SourceView { get; set; }
            public string ComboText
            {
                get { throw new NotImplementedException(); }
                set 
                { 
                    if (_IsRefresh) return; 
                    SourceView.Filter = item => item.ToString().ToLower().Contains(value.ToLower()); 
                }
            }

            public ComboBoxAutoComplateAssist(ComboBox comboBox, object source)
            {
                comboBox.IsEditable = true;
                comboBox.IsTextSearchEnabled = false;

                this.comboBox = comboBox;
                this.SourceView = CollectionViewSource.GetDefaultView(source);

                comboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding()
                {
                    Source = this,
                    Path = new PropertyPath("SourceView"),
                });
                comboBox.SetBinding(ComboBox.TextProperty, new Binding()
                {
                    Source = this,
                    Path = new PropertyPath("ComboText"),
                    Mode = BindingMode.OneWayToSource,
                });

                comboBox.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(OnTextChanged));
                comboBox.AddHandler(TextBoxBase.KeyUpEvent, new KeyEventHandler(OnPreviewKeyDown));
                comboBox.AddHandler(Button.ClickEvent, new RoutedEventHandler(OnClick));
            }

            private void ClearFilter()
            {
                _IsRefresh = true;
                SourceView.Filter = null;
                //SourceView.Refresh();
                _IsRefresh = false;
            }


            private void OnTextChanged(object sender, TextChangedEventArgs e)
            {                
                comboBox.IsDropDownOpen = true;
            }

            private void OnPreviewKeyDown(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Up || e.Key == Key.Down)
                {
                    ClearFilter();
                }
            }

            private void OnClick(object sender, RoutedEventArgs e)
            {
                ClearFilter();
            }

        }

        #endregion
    }
}
