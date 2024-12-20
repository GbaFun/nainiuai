using CefSharp;
using CefSharp.DevTools.Network;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IdleAuto
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var Url = @"https://www.idleinfinity.cn/Home/Index";//url网址
            ChromiumWebBrowser ch = new ChromiumWebBrowser(Url);
            ch.Dock = DockStyle.Fill;
            //浏览器当前地址
            //cr.CurrentUrl = open.Address;
            this.Controls.Add(ch);

        }
        public class CustomRequestHandler : IRequestHandler
        {
            public bool OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
            {
                var url = request.Url;

                // 假设你的自定义JS文件名为 custom.js
                if (request.ResourceType == ResourceType.Script && url.Contains("custom.js"))
                {
                    // 读取你的自定义JS内容
                    var jsContent = System.IO.File.ReadAllText("path/to/your/custom.js");

                    // 使用回调返回自定义的JS内容
                    callback.Continue(new Response { MimeType = "application/javascript", StatusCode = 200, Body = jsContent });

                    return true; // 表示处理已完成
                }

                return false; // 继续加载资源
            }

            // 其他必须实现的接口方法...
        }

        // 在你的CefSharp初始化代码中使用这个自定义的RequestHandler
        var browser = new ChromiumWebBrowser("http://yourwebsite.com");
        browser.RequestHandler = new CustomRequestHandler();
    }
}
