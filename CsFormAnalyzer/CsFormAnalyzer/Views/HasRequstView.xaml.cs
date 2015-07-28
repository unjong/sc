using CsFormAnalyzer.Controls;
using System;
using System.Collections.Generic;
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

namespace CsFormAnalyzer.Views
{
    /// <summary>
    /// Interaction logic for HasRequstView.xaml
    /// </summary>
    public partial class HasRequstView : UserControl
    {
        public HasRequstView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SCPopWinBase win = this.Tag as SCPopWinBase;
            win.DialogResult = true;
            win.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SCPopWinBase win = this.Tag as SCPopWinBase;
            win.Close();
        }
    }
}
