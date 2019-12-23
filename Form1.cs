using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DmSoft;


namespace 窗口调用
{

 



    public partial class Form1 : Form
    {
        private Dm dm;
        public Form1()
        {
            InitializeComponent();
            this.dataGridView1.ColumnHeadersHeight = 28;
            this.dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; //只允许选择一整行
            this.dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.MultiSelect = false;
            Bind();
            dataGridView1_CellClick(null, null);
        }

        //连接
        public void  Connect(Object model)
        {

            Model _model = (Model)model;
            
            string filepath =AppDomain.CurrentDomain.BaseDirectory+"\\Radmin.exe";
            string arg = "/connect:" + _model.IP + ":" + _model.Port + " " + _model.ConnectType;

            Runapp(filepath, arg);
            dm = new Dm();
            int hwnd = 0;
            while (hwnd == 0)
            {
                hwnd = dm.FindWindow("", "Radmin 安全性: " + _model.IP);
                Thread.Sleep(10);
            }
            dm.SetWindowState(hwnd, 1);

            //输入账号和密码的窗口
            string editHwnd = string.Empty;
            while (editHwnd == string.Empty)
            {
                editHwnd = dm.EnumWindow(hwnd, "", "Edit", 2 + 4 + 8 + 16);
                if (editHwnd != "" && editHwnd.Split(',').Length == 2)
                {
                    dm.SendString(Convert.ToInt32(editHwnd.Split(',')[0]), _model.Account);
                    dm.SendString(Convert.ToInt32(editHwnd.Split(',')[1]), _model.PassWord);
                }
                Thread.Sleep(10);
            }

            //点击确定
            string SubButton = string.Empty;
            while (SubButton == string.Empty)
            {
                SubButton = dm.EnumWindow(hwnd, "确定", "", 1 + 4 + 8 + 16);
                if (SubButton != "")
                {
                    dm.BindWindow(Convert.ToInt32(SubButton), "normal", "windows", "windows", 0);
                    dm.MoveTo(20, 10);
                    dm.LeftDoubleClick();
                }
                Thread.Sleep(10);
            }

            //以免木有点击到...
            Thread.Sleep(500);

            while (Convert.ToInt32(SubButton) > 0 )
            {
                SubButton = dm.EnumWindow(hwnd, "确定", "", 1 + 4 + 8 + 16);
                if (SubButton != "")
                {
                    dm.LeftClick();
                }
                if (SubButton=="")
                {
                    SubButton = "0";
                }
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 运行一个指定文件或者程序
        /// </summary>
        /// <param name="Path">文件路径</param>
        /// <returns>失败返回 false / 成功返回 true</returns>
        private void Runapp(string Path ,string arg)
        {

            Process pro = new Process();
            pro.StartInfo.UseShellExecute = false;
            pro.StartInfo.CreateNoWindow = false;
            pro.StartInfo.FileName = @Path;
            pro.StartInfo.Arguments = arg;
            pro.Start();
        }


        //绑定数据
        public void Bind()
        {
            if (File.Exists("radmin.data"))
            {
                string[] fileStrings = File.ReadAllLines("radmin.data");
                foreach (var dr in fileStrings)
                {
                    this.dataGridView1.Rows.Add(dr.Split(',')[0],
                        dr.Split(',')[1], dr.Split(',')[2], dr.Split(',')[3],
                        dr.Split(',')[4], dr.Split(',')[5].Replace(";",""));
                }
            }
        }

        //连接
        private void button4_Click(object sender, EventArgs e)
        {
            DataGridViewRow dr = this.dataGridView1.CurrentRow;
            string ip = dr.Cells[1].Value.ToString();
            string 端口 = dr.Cells[4].Value.ToString();
            string 账号 = dr.Cells[2].Value.ToString();
            string 密码 = dr.Cells[5].Value.ToString();

            Thread  connecThread = new Thread(new ParameterizedThreadStart(Connect));
            Model model = new Model { IP = ip, PassWord = 密码, Port = 端口, Account = 账号, ConnectType = radioButton2.Checked ? "/file" : "" };
            connecThread.Start(model);
        }


        //任务栏
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;
            this.Activate();
            this.ShowInTaskbar = true;
        }

        //最小化
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)//最小化　　　　　 
            {
                this.ShowInTaskbar = false;
                this.notifyIcon1.Visible = true;
            }
        }

        //选中行
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
        
            DataGridViewRow dr = this.dataGridView1.CurrentRow;
            if (dr!=null)
            {
                textBox1.Text = dr.Cells[1].Value.ToString();
                textBox2.Text = dr.Cells[2].Value.ToString();
                textBox3.Text = dr.Cells[5].Value.ToString();
                textBox4.Text = dr.Cells[4].Value.ToString();
            }
        }


        //添加数据
        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim() == "")
            {
                MessageBox.Show("IP地址不能为空!");
                return;
            }

            if (textBox2.Text.Trim() == "")
            {
                MessageBox.Show("账号不能为空!");
                return;
            }

            if (textBox3.Text.Trim() == "")
            {
                MessageBox.Show("密码不能为空!");
                return;
            }

            if (textBox4.Text.Trim() == "")
            {
                MessageBox.Show("端口号不能为空!");
                return;
            }

            var exits = false;

            foreach (DataGridViewRow dr in dataGridView1.Rows)
            {
                if (dr.Cells[1].Value.ToString() == textBox1.Text.Trim())
                {
                    exits = true;
                    break;
                }
            }
            if (!exits)
            {
                this.dataGridView1.Rows.Add(this.dataGridView1.Rows.Count + 1, textBox1.Text.Trim(), textBox2.Text.Trim(), "******",
                    textBox4.Text.Trim(), textBox3.Text.Trim());
                Save();
            }
        }

        //修改
        private void button3_Click(object sender, EventArgs e)
        {
           DataGridViewRow dr = this.dataGridView1.CurrentRow;
            if (dr != null)
            {

                if (textBox1.Text.Trim() == "")
                {
                    MessageBox.Show("IP地址不能为空!");
                    return;
                }

                if (textBox2.Text.Trim() == "")
                {
                    MessageBox.Show("账号不能为空!");
                    return;
                }

                if (textBox3.Text.Trim() == "")
                {
                    MessageBox.Show("密码不能为空!");
                    return;
                }

                if (textBox4.Text.Trim() == "")
                {
                    MessageBox.Show("端口号不能为空!");
                    return;
                }

                this.dataGridView1.CurrentRow.Cells[1].Value = textBox1.Text.Trim();
                this.dataGridView1.CurrentRow.Cells[2].Value = textBox2.Text.Trim();
                this.dataGridView1.CurrentRow.Cells[3].Value = "******";
                this.dataGridView1.CurrentRow.Cells[4].Value = textBox4.Text.Trim();
                this.dataGridView1.CurrentRow.Cells[5].Value = textBox3.Text.Trim();
                Save();
            }
            else
            {
                MessageBox.Show("请选择需要修改的行!");
            }
        }


        //删除行
        private void button1_Click(object sender, EventArgs e)
        {
            this.dataGridView1.Rows.Remove(this.dataGridView1.CurrentRow);
            Save();
        }

        //保存至文件
        public void Save()
        {
            string text = "";
            foreach (DataGridViewRow dr in dataGridView1.Rows)
            {
                text += dr.Cells[0].Value + "," + dr.Cells[1].Value + "," + dr.Cells[2].Value + "," + dr.Cells[3].Value +
                        "," + dr.Cells[4].Value + "," + dr.Cells[5].Value.ToString() + ";\r\n";
            }
            File.WriteAllText("radmin.data", text);
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {

                contextMenuStrip1.Show();
            }       
        }

        private void contextMenuStrip1_MouseClick(object sender, MouseEventArgs e)
        {
            Application.Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
        }
    }



    public class Model
    {

        public string IP
        {
            get;
            set;
        }
        public string Port
        {
            get;
            set;
        }

        public string Account
        {
            get;
            set;
        }

        public string PassWord
        {
            get;
            set;
        }

        public string ConnectType
        {
            get;
            set;
        }
    }
}
