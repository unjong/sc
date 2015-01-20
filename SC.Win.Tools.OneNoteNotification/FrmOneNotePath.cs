using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SC.Win.Tools.OneNoteNotification
{
    public partial class FrmOneNotePath : Form
    {
        public FrmOneNotePath()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(!Directory.Exists(txtFilePath.Text))
            {
                MessageBox.Show(@"경로가 존재 하지 않습니다.\r\n C:\Users\사용자계정\AppData\Local\Microsoft\OneNote  에서 찾아서 입력해주세요.");
                return;
            }
            Properties.Settings.Default.OneNotePath = txtFilePath.Text;
            Properties.Settings.Default.Save();
            this.Close();
        }
    }
}
