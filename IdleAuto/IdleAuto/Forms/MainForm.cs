using CefSharp;
using CefSharp.DevTools.Network;
using CefSharp.WinForms;
using Newtonsoft.Json.Linq;
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
        private ChromiumWebBrowser browser;
        private ComboBox optSelect;
        private Panel topPanel = new Panel();
        private Panel browserPanel;

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        public MainForm()
        {
            InitializeComponent();
            InitOptAccount();
            InitializeChromium();
        }

        private void InitializeChromium()
        {
            // 初始化CefSharp前设置路径和方案
            var settings = new CefSettings();
            settings.CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache"); // 确保这是一个有效的、可写的路径
            Cef.Initialize(settings);

            // 在顶部面板中添加按钮
            Button btnLogin = new Button();
            btnLogin.Text = "登录";
            btnLogin.Dock = DockStyle.Left;
            btnLogin.Click += LoadButton_Click;
            topPanel.Controls.Add(btnLogin);

            Button btnSave = new Button();
            btnSave.Text = "保存登录信息";
            btnSave.Dock = DockStyle.Left;
            btnSave.Click += SaveButton_Click;
            topPanel.Controls.Add(btnSave);

            // 创建浏览器面板用于放置浏览器控件
            browserPanel = new Panel();
            browserPanel.Top = 100;
            browserPanel.Width = 1000;
            browserPanel.Height = 800;
            this.Controls.Add(browserPanel);

            // 初始化第一个浏览器
            browser = new ChromiumWebBrowser("https://www.idleinfinity.cn/Home/Index");
            // 绑定对象
            browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            browser.JavascriptObjectRepository.Register("Bridge", new Bridge(), isAsync: true, options: BindingOptions.DefaultBinder);
            browser.KeyboardHandler = new CEFKeyBoardHander();
            browserPanel.Controls.Add(browser);

            // 等待页面加载完成后执行脚本
            browser.FrameLoadEnd += OnFrameLoadEnd;
        }

        private void InitOptAccount()
        {

            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 50; // 设置顶部面板高度
            this.Controls.Add(topPanel);
            optSelect = new ComboBox();
            optSelect.Dock = DockStyle.Left;
            optSelect.Font = new Font("微软雅黑", 18);
            // 设置ComboBox的属性
            topPanel.Controls.Add(optSelect);
            optSelect.Items.AddRange(AccountCfg.Instance.Accounts.Select(s => s.Username).ToArray());
            // 设置默认选中的项
            optSelect.SelectedIndex = 0;
            // 添加一个事件处理器来处理选中项变化事件
            optSelect.SelectedIndexChanged += new EventHandler(AccountChanged);
            // 将ComboBox添加到窗体控件集合中
            this.Controls.Add(optSelect);
        }

        private void AccountChanged(object sender, EventArgs e)
        {
            // 获取选中的项
            string selectedItem = optSelect.SelectedItem.ToString();
            // 可以在这里添加代码来处理选中项变化
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            await SaveCookiesAsync(browser);
            await SaveLocalStorageAsync(browser);
        }

        private async void LoadButton_Click(object sender, EventArgs e)
        {
            await ClearCookiesAsync(browser);
            await ClearLocalStorageAsync(browser);
            await LoadCookiesAsync(browser);
            await LoadLocalStorageAsync(browser);
            await PrintCookiesAsync(browser); // 打印Cookie以验证写入
            ReloadPage();
        }

        private async Task SaveCookiesAsync(ChromiumWebBrowser browser)
        {
            var cookieManager = browser.GetCookieManager();
            var cookies = await cookieManager.VisitAllCookiesAsync();
            using (var writer = new StreamWriter(optSelect.SelectedItem.ToString() + "cookies.txt"))
            {
                foreach (var cookie in cookies)
                {
                    writer.WriteLine($"{cookie.Domain}\t{cookie.Name}\t{cookie.Value}\t{cookie.Path}\t{cookie.Expires}");
                }
            }
            Console.WriteLine("Cookies saved.");
        }

        private async Task LoadCookiesAsync(ChromiumWebBrowser browser)
        {
            var cookieManager = browser.GetCookieManager();
            using (var reader = new StreamReader(optSelect.SelectedItem.ToString() + "cookies.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split('\t');
                    var cookie = new CefSharp.Cookie
                    {
                        Domain = parts[0],
                        Name = parts[1],
                        Value = parts[2],
                        Path = parts[3],
                        Expires = parts[4] == "" ? DateTime.Now.AddDays(1) : DateTime.Parse(parts[4])
                    };
                    var url = "https://www.idleinfinity.cn/";
                    bool success = await cookieManager.SetCookieAsync(url, cookie);
                    if (!success)
                    {
                        Console.WriteLine($"Failed to set cookie: {cookie.Name} for domain: {cookie.Domain}");
                    }
                }
            }
            Console.WriteLine("Cookies loaded.");
        }

        private async Task ClearCookiesAsync(ChromiumWebBrowser browser)
        {
            var cookieManager = browser.GetCookieManager();
            await cookieManager.DeleteCookiesAsync("", "");
            Console.WriteLine("Cookies cleared.");
        }

        private async Task PrintCookiesAsync(ChromiumWebBrowser browser)
        {
            var cookieManager = browser.GetCookieManager();
            var cookies = await cookieManager.VisitAllCookiesAsync();
            foreach (var cookie in cookies)
            {
                Console.WriteLine($"{cookie.Domain}\t{cookie.Name}\t{cookie.Value}\t{cookie.Path}\t{cookie.Expires}");
            }
        }

        private async Task SaveLocalStorageAsync(ChromiumWebBrowser browser)
        {
            var script = @"
                (function() {
                    var items = {};
                    for (var i = 0; i < localStorage.length; i++) {
                        var key = localStorage.key(i);
                        items[key] = localStorage.getItem(key);
                    }
                    return JSON.stringify(items);
                })();";
            var response = await browser.EvaluateScriptAsync(script);
            if (response.Success && response.Result != null)
            {
                File.WriteAllText(optSelect.SelectedItem.ToString() + ".json", response.Result.ToString());
                Console.WriteLine("LocalStorage saved.");
            }
            else
            {
                Console.WriteLine("Failed to save LocalStorage.");
            }
        }

        private async Task LoadLocalStorageAsync(ChromiumWebBrowser browser)
        {
            if (File.Exists(optSelect.SelectedItem.ToString() + "localstorage.json"))
            {
                var json = File.ReadAllText("localstorage.json");
                var script = $@"
                    (function() {{
                        var items = {json};
                        for (var key in items) {{
                            localStorage.setItem(key, items[key]);
                        }}
                    }})();";
                var response = await browser.EvaluateScriptAsync(script);
                if (response.Success)
                {
                    Console.WriteLine("LocalStorage loaded.");
                }
                else
                {
                    Console.WriteLine("Failed to load LocalStorage.");
                }
            }
        }

        private async Task ClearLocalStorageAsync(ChromiumWebBrowser browser)
        {
            var script = @"
                (function() {
                    localStorage.clear();
                })();";
            var response = await browser.EvaluateScriptAsync(script);
            if (response.Success)
            {
                Console.WriteLine("LocalStorage cleared.");
            }
            else
            {
                Console.WriteLine("Failed to clear LocalStorage.");
            }
        }

        private void OnFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            
            // 在主框架中执行自定义脚本
            // 获取WinForms程序目录下的JavaScript文件路径
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "login.js");
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
            var bro = sender as ChromiumWebBrowser;
            if (bro.Address.ToLower().IndexOf("login") > -1)
            {
                bro.ExecuteScriptAsync(script);
            }

        }

        private void ReloadPage(string url = "")
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                var script = $@"location.reload();";
                browser.EvaluateScriptAsync(script);
            }
        }


    }
}