using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DmSoft;

namespace 窗口调用
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Radmin.exe";
            if (!File.Exists(filepath))
            {
                MessageBox.Show("请把程序放在RadminView目录");
                return;
            }


            bool create;
            using (Mutex mu = new Mutex(true, Application.ProductName, out create))
            {
               
                if (create)
                {
                    Run();
                }
                else
                {
                    MessageBox.Show("该程序已运行.");
                }
            }
        }
        static void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

    }
}
