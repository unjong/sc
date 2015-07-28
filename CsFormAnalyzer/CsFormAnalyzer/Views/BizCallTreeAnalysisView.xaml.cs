using CsFormAnalyzer.Mvvm;
using CsFormAnalyzer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CsFormAnalyzer.Views
{
    /// <summary>
    /// Interaction logic for BizDataParsingView.xaml
    /// </summary>
    public partial class BizCallTreeAnalysisView : Window
    {
        public BizCallTreeAnalysisView()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtBox = sender as TextBox;
            if(txtBox!=null)
            {
                txtBox.ScrollToEnd();
            }
        }

        private void DataGrid_CollectionChanged(object sender, AddingNewItemEventArgs e)
        {
           
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var dg = (DataGrid)sender;
            if (dg == null || dg.ItemsSource == null) return;

            var sourceCollection = dg.ItemsSource as ObservableCollection<CallTreeData>;
            if (sourceCollection == null) return;

            sourceCollection.CollectionChanged += (s, e2) =>
                {
                    if (dg != null)
                        dg.ScrollIntoView(dg.Items[dg.Items.Count - 1]);
                };
                
        }
    }
}
