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
    public partial class Form1 : Form
    {
   

        private void Form1_Load(object sender, EventArgs e)
        {
  

        }
        private ChromiumWebBrowser browser;

        public Form1()
        {
            InitializeComponent();
            InitializeChromium();
        }

        private void InitializeChromium()
        {
            CefSettings settings = new CefSettings();
            Cef.Initialize(settings);
            browser = new ChromiumWebBrowser("https://www.idleinfinity.cn/Home/Index");
            this.Controls.Add(browser);
            browser.Dock = DockStyle.Fill;

            // 等待页面加载完成后执行脚本
            browser.FrameLoadEnd += OnFrameLoadEnd;
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
            browser.ExecuteScriptAsync(script);

        }
    }
}
