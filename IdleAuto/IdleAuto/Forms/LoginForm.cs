using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IdleAuto
{
    public partial class LoginForm : Form
    {
        public static LoginForm Instance { get; private set; }
        private ChromiumWebBrowser browser;

        public string LoginName;
        public string LoginPassword;
        public LoginForm()
        {
            Instance = this;
            InitializeComponent();

            var settings = new CefSettings();

            // Increase the log severity so CEF outputs detailed information, useful for debugging
            settings.LogSeverity = LogSeverity.Verbose;
            // By default CEF uses an in memory cache, to save cached data e.g. to persist cookies you need to specify a cache path
            // NOTE: The executing user must have sufficient privileges to write to this folder.
            settings.CachePath = AppDomain.CurrentDomain.BaseDirectory + "idle\\caches";

            Cef.Initialize(settings);

            // 创建第一个浏览器的请求上下文
            var requestContext1 = new RequestContext(new RequestContextSettings { CachePath = AppDomain.CurrentDomain.BaseDirectory + $"idle\\caches\\{LoginName}" });

            // 初始化第一个浏览器
            //browser = new ChromiumWebBrowser("https://www.idleinfinity.cn/Home/Login", requestContext1);
            browser = new ChromiumWebBrowser("https://www.idleinfinity.cn/Home/Login");
            //注册方法//注册JsObj对象JS调用C#
            browser.JavascriptObjectRepository.Register("Bridge", new Bridge(), isAsync: false);
            browser.KeyboardHandler = new CEFKeyBoardHander();
            this.Controls.Add(browser);

            browser.FrameLoadEnd += OnWebBrowserLoaded;
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Console.WriteLine("LoginForm closed");
            TestForm.Instance.Show();
        }

        private async void OnWebBrowserLoaded(object sender, FrameLoadEndEventArgs e)
        {
            // 在主框架中执行自定义脚本                   
            // 获取WinForms程序目录下的JavaScript文件路径
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts\\js", "LoginLogic.js");
            string scriptContent = File.ReadAllText(scriptPath);

            // 在主框架中执行自定义脚本
            string script = $@"{scriptContent}";
            browser.ExecuteScriptAsync(script);
            //browser.ExecuteScriptAsync($@"showMessage('loginName:{LoginName}---loginPassword:{LoginPassword}')");
            browser.ExecuteScriptAsync(@"getMessage()");

            Console.WriteLine($"({DateTime.Now})Do ExecuteScriptAsync LoginLogic.js");
            //await browser.EvaluateScriptAsync($"showMessage('loginName:{LoginName}---loginPassword:{LoginPassword}')");
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cef.Shutdown();

        }
    }
}