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
using System.Windows.Shapes;

namespace CsFormAnalyzer.Controls
{
    /// <summary>
    /// Interaction logic for SCPopWinBase.xaml
    /// </summary>
    public partial class SCPopWinBase : Window
    {
        public SCPopWinBase(Mvvm.ViewModelBase vm)
        {
            InitializeComponent();

            vm.ViewControl = this;
            vm.Close = () =>
            {
                this.Close();
            };
        }
    }
}
