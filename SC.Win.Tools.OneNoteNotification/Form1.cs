using Microsoft.Win32;
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
    public partial class Form1 : Form
    {
        
        AlertWindows alertWin = new AlertWindows();
        public Form1()
        {
            InitializeComponent();

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
                                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            //레지스트리 등록 할때
            if (registryKey.GetValue("SCOneNoteNotification") == null)
            {
                MessageBox.Show("원노트 알리미가 설치 되었습니다. 감사합니다.!!!");
                registryKey.SetValue("SCOneNoteNotification", Application.ExecutablePath.ToString());
            }




            //if (string.IsNullOrEmpty(Properties.Settings.Default.OneNotePath))
            //{
            //    var saver = new FrmOneNotePath();
            //    saver.ShowDialog();
            //    this.fileSystemWatcher1.Path = Properties.Settings.Default.OneNotePath;
            //}
            //else
            //    this.fileSystemWatcher1.Path = Properties.Settings.Default.OneNotePath;

            this.fileSystemWatcher1.Path = @"\\10.28.16.54\share\onenote\socen의 전자 필기장\99.메시지전달용";

        }

        private void fileSystemWatcher1_Changed(object sender, System.IO.FileSystemEventArgs e)
        {

            //string txtmsg = e.FullPath.Replace(@"\\10.28.16.54\share\onenote\socen의 전자 필기장\00.Sevrance\메시지전달용\", "").Replace("\\", ">");

            //var attrs = File.GetAttributes(e.FullPath);
            
            alertWin.ShowMessage("새로운 알림이 있습니다", e.FullPath);
        }

        private void fileSystemWatcher1_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            //if (!e.Name.Contains(".one")) return;
            //string txtmsg = e.FullPath.Replace(@"\\10.28.16.54\share\onenote\socen의 전자 필기장\00.Sevrance\메시지전달용\", "").Replace("\\", ">");
            alertWin.ShowMessage("새로운 알림이 있습니다", e.FullPath);
        }
    }
}
