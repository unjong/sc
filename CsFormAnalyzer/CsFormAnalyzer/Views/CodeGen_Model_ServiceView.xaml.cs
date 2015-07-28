using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit;
using CsFormAnalyzer.lib;
using ICSharpCode.AvalonEdit.Folding;
using CsFormAnalyzer.ViewModels;

namespace CsFormAnalyzer.Views
{
    /// <summary>
    /// Interaction logic for CodeGen_Model_ServiceView.xaml
    /// </summary>
    public partial class CodeGen_Model_ServiceView : UserControl
    {

        ICSharpCode.AvalonEdit.Folding.AbstractFoldingStrategy foldingStrategy;
        public CodeGen_Model_ServiceView()
        {
            InitializeComponent();
            foldingStrategy = new BraceFoldingStrategy();
            ((INotifyCollectionChanged)tabControl1.Items).CollectionChanged += (s, e) =>
                {
                    tabControl1.SelectedIndex = tabControl1.Items.Count - 1;
                };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.bdrSPParamInfo.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            btn.CommandParameter = this.textEditor3.SelectedText;
        }

       
        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var txtBox = sender as TextBox;

                ContentPresenter cp = tabControl1.Template.FindName("PART_SelectedContentHost", tabControl1) as ContentPresenter;
                // Finding textBlock from the DataTemplate that is set on that ContentPresenter
                DataTemplate myDataTemplate = tabControl1.SelectedContentTemplate;
                ICSharpCode.AvalonEdit.TextEditor textEditor = myDataTemplate.FindName("textEditor", cp) as ICSharpCode.AvalonEdit.TextEditor;
                textEditor.Text = txtBox.Text;
                textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
                var foldingManager = FoldingManager.Install(textEditor.TextArea);
                foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
            }
            catch
            {
            }
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var txtBox = sender as TextBox;

                ContentPresenter cp = tabControl1.Template.FindName("PART_SelectedContentHost", tabControl1) as ContentPresenter;
                // Finding textBlock from the DataTemplate that is set on that ContentPresenter
                DataTemplate myDataTemplate = tabControl1.SelectedContentTemplate;
                ICSharpCode.AvalonEdit.TextEditor textEditor = myDataTemplate.FindName("textEditor", cp) as ICSharpCode.AvalonEdit.TextEditor;
                textEditor.Text = txtBox.Text;
                //textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
                //var foldingManager = FoldingManager.Install(textEditor.TextArea);
                //foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
            }
            catch
            {
            }
        }

        private void TextBox2_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var txtBox = sender as TextBox;

                ContentPresenter cp = tabControl1.Template.FindName("PART_SelectedContentHost", tabControl1) as ContentPresenter;
                // Finding textBlock from the DataTemplate that is set on that ContentPresenter
                DataTemplate myDataTemplate = tabControl1.SelectedContentTemplate;
                ICSharpCode.AvalonEdit.TextEditor textEditor = myDataTemplate.FindName("textEditor2", cp) as ICSharpCode.AvalonEdit.TextEditor;
                textEditor.Text = txtBox.Text;
                textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
                var foldingManager = FoldingManager.Install(textEditor.TextArea);
                foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
            }
            catch
            { }
        }

        private void TextBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var txtBox = sender as TextBox;

                ContentPresenter cp = tabControl1.Template.FindName("PART_SelectedContentHost", tabControl1) as ContentPresenter;
                // Finding textBlock from the DataTemplate that is set on that ContentPresenter
                DataTemplate myDataTemplate = tabControl1.SelectedContentTemplate;
                ICSharpCode.AvalonEdit.TextEditor textEditor = myDataTemplate.FindName("textEditor2", cp) as ICSharpCode.AvalonEdit.TextEditor;
                textEditor.Text = txtBox.Text;
                //textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
                //var foldingManager = FoldingManager.Install(textEditor.TextArea);
                //foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
            }
            catch
            { }
        }

        private void txtSPCode_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            try
            {
                var txtBox = sender as TextBox;
                textEditor3.Text = txtBox.Text;
            }
            catch
            {
            }
        }

        private void textEditor3_TextChanged(object sender, EventArgs e)
        {
            var vm = this.grdRoot.DataContext as CodeGen_Model_ServiceVM;
            if(vm.SelectedSPInfo.IsQueryText &&  vm.SelectedSPInfo.SPCodeText != textEditor3.Text)
            {
                vm.SelectedSPInfo.SPCodeText = textEditor3.Text;
            }
        }

        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var listBox = sender as ListBox;
                var item = listBox.SelectedItem;
                var text = ((dynamic)item).SPName;
                Clipboard.SetText(text);
            }
            catch (Exception)
            {
            }
        }
    }
}
