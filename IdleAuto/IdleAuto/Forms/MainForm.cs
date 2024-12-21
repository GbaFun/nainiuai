using CefSharp;
using CefSharp.DevTools.Network;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IdleAuto
{
    public partial class MainForm : Form
    {


        private void MainForm_Load(object sender, EventArgs e)
        {


        }
        private ChromiumWebBrowser browser;
        private ChromiumWebBrowser browser2;

        public MainForm()
        {
            InitializeComponent();
            InitializeChromium();
            this.Resize += Form1_Resize; // 订阅窗口大小变化事件
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            // 调整浏览器控件的大小
            browser.Width = this.ClientSize.Width / 2;
            browser2.Width = this.ClientSize.Width / 2;
            browser.Height = this.ClientSize.Height;
            browser2.Height = this.ClientSize.Height;
        }

        private void InitializeChromium()
        {
            //var settings = new CefSettings();

            //// Increase the log severity so CEF outputs detailed information, useful for debugging
            //settings.LogSeverity = LogSeverity.Verbose;
            //// By default CEF uses an in memory cache, to save cached data e.g. to persist cookies you need to specify a cache path
            //// NOTE: The executing user must have sufficient privileges to write to this folder.
            //settings.CachePath = AppDomain.CurrentDomain.BaseDirectory + "idle\\caches";

            //Cef.Initialize(settings);

            // 创建第一个浏览器的请求上下文
            var requestContext1 = new RequestContext(new RequestContextSettings { CachePath = AppDomain.CurrentDomain.BaseDirectory + "idle\\caches\\cache1" });


            // 初始化第一个浏览器
            browser = new ChromiumWebBrowser("https://www.idleinfinity.cn/Home/Index", requestContext1);
            this.Controls.Add(browser);
            browser.Dock = DockStyle.Left;
            browser.Width = this.ClientSize.Width / 2;

            // 创建第二个浏览器的请求上下文
            var requestContext2 = new RequestContext(new RequestContextSettings { CachePath = AppDomain.CurrentDomain.BaseDirectory + "idle\\caches\\cache2" });

            // 初始化第二个浏览器
            browser2 = new ChromiumWebBrowser("https://www.idleinfinity.cn/Home/Index", requestContext2);
            this.Controls.Add(browser2);
            browser2.Dock = DockStyle.Right;
            browser2.Width = this.ClientSize.Width / 2;

            // 等待页面加载完成后执行脚本
            browser.FrameLoadEnd += OnFrameLoadEnd;
            browser2.FrameLoadEnd += OnFrameLoadEnd;
        }

        private void OnFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {

            // 在主框架中执行自定义脚本
            // 获取WinForms程序目录下的JavaScript文件路径
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "js", "ah.js");
            string scriptContent = File.ReadAllText(scriptPath);

            // 在主框架中执行自定义脚本
            string script = $@"
                    (function() {{
                        var script = document.createElement('script');
                        script.type = 'text/javascript';
                        script.text = {scriptContent};
                        document.head.appendChild(script);
                    }})();
                ";
            (sender as ChromiumWebBrowser).ExecuteScriptAsync(script);
            //browser.ExecuteScriptAsync(script);
            //browser2.ExecuteScriptAsync(script);

        }
    }
}
