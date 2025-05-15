using CefSharp;
using CefSharp.WinForms;
using IdleAuto.Db;
using IdleAuto.Scripts.View;
using IdleAuto.Scripts.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IdleAuto
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //设置全局异常处理
            var settings = new CefSettings
            {
                MultiThreadedMessageLoop = true,
                ExternalMessagePump = false,
                // 禁用 GPU 加速
                CefCommandLineArgs = { ["disable-webgl"] = "1", ["mute-audio"] = "1" },

            };
            if (Cef.IsInitialized == null || !Cef.IsInitialized.Value)
            {
                Cef.Initialize(settings);
            }
            EquipTradeQueue.AppDomainInitializer();
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
         
        }
        // 处理 UI 线程中的未处理异常
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        // 处理非 UI 线程中的未处理异常
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        // 异常处理方法
        static void HandleException(Exception ex)
        {
            if (ex != null)
            {
                // 记录异常信息（例如，写入日志文件）
                // 显示友好的错误消息
                MessageBox.Show($"发生未处理的异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                P.Log(ex.StackTrace, emLogType.Error);
            }
        }
    }
}
