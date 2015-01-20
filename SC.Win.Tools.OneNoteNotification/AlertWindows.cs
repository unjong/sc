using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SC.Win.Tools.OneNoteNotification
{
    public partial class AlertWindows : Form
    {

        bool isShow = false;

        Timer tm = new Timer();
                
        public AlertWindows()
        {
            InitializeComponent();
            this.ControlBox = false;
            this.ShowInTaskbar = false;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tm.Interval = 30;
            tm.Tick += delegate(object se, EventArgs ea)
            {
                this.TopMost = true;
                isShow = true;
                // 효과 종료조건
                if (this.Top > (Screen.PrimaryScreen.WorkingArea.Bottom - this.Height + 5))
                    this.Top -= (this.Height / 15);
                // 효과 종료 동작
                else
                {
                    this.Top = Screen.PrimaryScreen.WorkingArea.Bottom - this.Height;

                    tm.Stop();
                }
            };
        }

        public void ShowMessage(string message, string linkText)
        {
            try
            {
                if(isShow)
                {
                    if(this.panel2.Controls.Count * 20 > this.panel2.Height)
                        this.Height += 10;
                    //var colorArr = new Color[] { System.Drawing.Color.Aqua, System.Drawing.Color.Wheat, Color.Yellow, Color.WhiteSmoke, Color.LightPink };
                    //Random r = new Random();
                    //this.panel2.BackColor =  colorArr[r.Next(0, colorArr.Length)];
                }

                // 메시지 추가
                AddMessage(message, linkText);


                this.Show();
                this.Left = Screen.PrimaryScreen.WorkingArea.Right - this.Width;
                this.Top = Screen.PrimaryScreen.WorkingArea.Bottom;
                SystemSounds.Exclamation.Play();

                tm.Start();
            }
            catch (Exception)
            {

            }
        }

        private void AddMessage(string message, string linkText)
        {
            bool hasMessage = false;
            foreach (var ctl in panel2.Controls)
            {
                var lbl = ctl as LinkLabel;
                if (lbl != null)
                {
                    if (lbl.Text.Equals(message))
                    {
                        hasMessage = true;
                        break;
                    }
                }
            }

            if (!hasMessage)
            {
                // 표시 위치 지정
                var linkLabel = new LinkLabel();
                linkLabel.Text = message;
                linkLabel.Dock = DockStyle.Top;
                linkLabel.Height = 20;
                linkLabel.LinkClicked += (s, e) =>
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
                            this.panel2.Controls.Remove(linkLabel);
                            if(panel2.Controls.Count == 0)
                                HideWindow();
                        }
                        catch
                        { }
                    };
                linkLabel.Padding = new System.Windows.Forms.Padding(3);
                linkLabel.Links.Add(0, message.Length, linkText);
                this.panel2.Controls.Add(linkLabel);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HideWindow();
        }

        private void HideWindow()
        {
            this.Hide();
            this.isShow = false;
            this.panel2.Controls.Clear();
            this.Size = new Size(347, 252);
        }
    }
}
