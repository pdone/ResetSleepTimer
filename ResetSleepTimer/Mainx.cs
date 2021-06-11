using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ResetSleepTimer
{
    public partial class Mainx : Form
    {
        BackgroundWorker Worker;

        /// <summary>
        /// 重置间隔
        /// </summary>
        const int Interval = 1000 * 30;

        /// <summary>
        /// 启动项注册表路径
        /// </summary>
        const string RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public Mainx()
        {
            InitializeComponent();
        }

        private void Mainx_Load(object sender, EventArgs e)
        {
            string path = Application.ExecutablePath;

            notifyIcon.Icon = Properties.Resources.logo;

            notifyIcon.ShowBalloonTip(1000);
            Worker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };

            Worker.DoWork += (ss, ee) =>
            {
                while (true)
                {
                    if (Worker.CancellationPending)
                    {
                        ee.Cancel = true;
                        return;
                    }

                    Thread.Sleep(Interval);
                    SystemSleepManagement.ResetSleepTimer(true);
                }
            };
            Worker.RunWorkerAsync();

            PauseStripMenuItem.CheckedChanged += (ss, ee) =>
            {
                if (PauseStripMenuItem.Checked)
                {
                    if (Worker.IsBusy)
                    {
                        Worker.CancelAsync();
                    }
                }
                else
                {
                    if (!Worker.IsBusy)
                    {
                        Worker.RunWorkerAsync();
                    }
                }
            };

            using (RegistryKey rk = Registry.LocalMachine)
            {
                using (RegistryKey rk2 = rk.OpenSubKey(RegistryPath))
                {
                    var path2 = rk2.GetValue("ResetSleepTimer") as string;
                    if (path == path2)
                    {
                        AutobootStripMenuItem.Checked = true;
                    }
                }
            }


            AutobootStripMenuItem.CheckedChanged += (ss, ee) =>
            {
                if (AutobootStripMenuItem.Checked)
                {
                    using (RegistryKey rk = Registry.LocalMachine)
                    {
                        using (RegistryKey rk2 = rk.CreateSubKey(RegistryPath))
                        {
                            rk2.SetValue("ResetSleepTimer", path);
                        }
                    }
                }
                else
                {
                    using (RegistryKey rk = Registry.LocalMachine)
                    {
                        using (RegistryKey rk2 = rk.CreateSubKey(RegistryPath))
                        {
                            rk2.SetValue("ResetSleepTimer", false);
                        }
                    }
                }
            };

            ExitToolStripMenuItem.Click += (ss, ee) =>
            {
                Worker.CancelAsync();
                Application.Exit();
            };
        }


    }

    class SystemSleepManagement
    {
        //定义API函数
        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(ExecutionFlag flags);

        [Flags]
        enum ExecutionFlag : uint
        {
            System = 0x00000001,
            Display = 0x00000002,
            Continus = 0x80000000,
        }

        /// <summary>
        /// 阻止系统休眠，直到线程结束恢复休眠策略
        /// </summary>
        /// <param name="includeDisplay">是否阻止关闭显示器</param>
        public static void PreventSleep(bool includeDisplay = false)
        {
            if (includeDisplay)
                SetThreadExecutionState(ExecutionFlag.System | ExecutionFlag.Display | ExecutionFlag.Continus);
            else
                SetThreadExecutionState(ExecutionFlag.System | ExecutionFlag.Continus);
        }

        /// <summary>
        /// 恢复系统休眠策略
        /// </summary>
        public static void ResotreSleep()
        {
            SetThreadExecutionState(ExecutionFlag.Continus);
        }

        /// <summary>
        /// 重置系统休眠计时器
        /// </summary>
        /// <param name="includeDisplay">是否阻止关闭显示器</param>
        public static void ResetSleepTimer(bool includeDisplay = false)
        {
            if (includeDisplay)
                SetThreadExecutionState(ExecutionFlag.System | ExecutionFlag.Display);
            else
                SetThreadExecutionState(ExecutionFlag.System);
        }
    }
}
