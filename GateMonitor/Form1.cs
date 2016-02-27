using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace GateMonitor
{
    public partial class Form1 : Form
    {
        private string FConnStr;
        private string FAvatarPath;
        private string FAvatarExtension;

        public Form1()
        {
            InitializeComponent();

            var s = new Settings1();
            FConnStr = s.ConnStr;
            FAvatarPath = s.AvatarPath;
            FAvatarExtension = s.AvatarExtension;
            FGate = s.DefaultGate;
            
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dataGridView1.RowCount > 0)
            {
                dataGridView1.Columns[1].Visible = false;
                label1.Text = (string)dataGridView1.Rows[0].Cells["username"].Value;
                label2.Text = (string)dataGridView1.Rows[0].Cells["job"].Value;
                label3.Text = (string)dataGridView1.Rows[0].Cells["company"].Value;
                label4.Text = (string)dataGridView1.Rows[0].Cells["dir"].Value;
                switch (label4.Text)
                {
                    case "进":
                        label4.BackColor = Color.Yellow;
                        break;
                    case "出":
                        label4.BackColor = Color.Cyan;
                        break;
                }
                pictureBox1.ImageLocation = FAvatarPath + ((string)dataGridView1.Rows[0].Cells["id_card"].Value).Trim() + FAvatarExtension;
                label6.Text = (string)dataGridView1.Rows[0].Cells["edu_time"].Value;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                var conn = new MySql.Data.MySqlClient.MySqlConnection(FConnStr);
                conn.Open();

                var da = new MySql.Data.MySqlClient.MySqlDataAdapter("SELECT IF(u.user_name IS NULL, '', u.user_name) AS username, IF(u.edu_time IS NULL, '', u.edu_time) AS edu_time, IF(u.user_identity IS NULL, '1', u.user_identity) AS id_card, IF(i.dotime IS NULL, '0000-00-00 00:00:00', i.dotime) AS time, IF(i.flag=0, '进', '出') AS dir, IF(j.job_name IS NULL, '', j.job_name) AS job, IF(c.company_name IS NULL, '', c.company_name) AS company FROM iolog i, user u, job j, company c WHERE i.gate = " + FGate + " AND i.card_id = u.card_id AND u.job_id = j.job_id AND j.company_id = c.company_id ORDER BY i.dotime DESC, u.user_name ASC LIMIT 30", conn);
                var ds = new DataSet();
                da.Fill(ds);
                DateTime time = new DateTime();
                this.dataGridView1.AutoGenerateColumns = true;
                //添加的地方
                int m = 0;
                int n = 0;
                int h = ds.Tables[0].Rows.Count;
                //  Console.Write(a);
                for (m = 0; m < h; m++)
                {
                    for (n = m + 1; n < h; n++)
                        if (Convert.ToString(ds.Tables[0].Rows[m][0]) == Convert.ToString(ds.Tables[0].Rows[n][0]) && Convert.ToString(ds.Tables[0].Rows[m][4]) == Convert.ToString(ds.Tables[0].Rows[n][4]))
                        {
                            TimeSpan a = Convert.ToDateTime(ds.Tables[0].Rows[m][3]) - Convert.ToDateTime(ds.Tables[0].Rows[n][3]);
                            double b = a.TotalSeconds;
                            if (b > 0 && b < 1000)
                            {
                                ds.Tables[0].Rows.RemoveAt(m);
                                //  Console.Write(ds.Tables[0].Rows[0][0]);
                                //  m = m - 1;
                                n = 0;
                                h = h - 1;
                            }
                            //Console.Write("大");
                            // ds.Tables[0].Rows.RemoveAt(0);
                        }
                    //m = m - 1;
                }
                this.dataGridView1.DataSource = ds.Tables[0];
                this.dataGridView1.AutoResizeColumns();
                this.dataGridView1.AutoResizeRows();
                int sum = 0;
                int i = 0;
                for (i = 2; i < dataGridView1.Columns.Count; ++i)
                {
                    sum += dataGridView1.Columns[i].Width;
                }
                dataGridView1.Columns[0].Width = dataGridView1.Width - sum;

                da.Dispose();
                conn.Close();
                conn.Dispose();
            }
            catch (Exception ex)
            {

            }
        }

        public Boolean SetFormFullScreen(Boolean Fullscreen, ref Rectangle OldRect)
        {
            Int32 hwnd = 0;
            hwnd = FindWindow("Shell_TrayWnd", null);

            if (hwnd == 0) return false;
            //this.SuspendLayout();
            if (Fullscreen)
            {
                ShowWindow(hwnd, SW_HIDE);

                SystemParametersInfo(SPI_GETWORKAREA, 0, ref OldRect, SPIF_UPDATEINIFILE);
                Rectangle rectFull = Screen.PrimaryScreen.Bounds;
                SystemParametersInfo(SPI_SETWORKAREA, 0, ref rectFull, SPIF_UPDATEINIFILE);

                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                Activate();
            }
            else
            {
                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.Sizable;

                ShowWindow(hwnd, SW_SHOW);

                SystemParametersInfo(SPI_SETWORKAREA, 0, ref OldRect, SPIF_UPDATEINIFILE);

                Activate();
            }
            return true;
        }
        #region user32.dll

        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        public static extern Int32 ShowWindow(Int32 hwnd, Int32 nCmdShow);
        public const Int32 SW_SHOW = 5; public const Int32 SW_HIDE = 0;

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        private static extern Int32 SystemParametersInfo(Int32 uAction, Int32 uParam, ref Rectangle lpvParam, Int32 fuWinIni);
        public const Int32 SPIF_UPDATEINIFILE = 0x1;
        public const Int32 SPI_SETWORKAREA = 47;
        public const Int32 SPI_GETWORKAREA = 48;

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern Int32 FindWindow(string lpClassName, string lpWindowName);

        #endregion

        private bool FIsFullScreen = true;
        private string FGate = "0";

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                Rectangle r;
                r = Rectangle.Empty;
                FIsFullScreen = !FIsFullScreen;
                this.SetFormFullScreen(FIsFullScreen, ref r);
            }
            else
            {
                MessageBox.Show("请按F11切换全屏/非全屏模式！");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Form1_SizeChanged(sender, e);
            if (FIsFullScreen)
            {
                Rectangle r;
                r = Rectangle.Empty;
                this.SetFormFullScreen(true, ref r);
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            label1.Font = new Font(label1.Font.FontFamily.Name, (int) (label1.Height * 0.8));
            label2.Font = new Font(label2.Font.FontFamily.Name, (int) (label2.Height * 0.5));
            label3.Font = new Font(label3.Font.FontFamily.Name, (int) (label3.Height * 0.5));
            label4.Font = new Font(label4.Font.FontFamily.Name, (int) (label4.Height * 0.75));
            label6.Font = new Font(label6.Font.FontFamily.Name, (int) (label6.Height * 0.8));
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FGate = comboBox1.Text;
        }
    }


}
