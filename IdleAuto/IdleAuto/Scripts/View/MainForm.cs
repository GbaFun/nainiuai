using CefSharp;
using CefSharp.DevTools.Network;
using CefSharp.WinForms;
using CefSharp.WinForms.Internals;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm : Form
{
    public static MainForm Instance;

    public ChromiumWebBrowser browser
    {
        get;
        private set;
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
    }

    public MainForm()
    {
        InitializeComponent();
        InitializeChromium();
        ShowAccountCombo();
        Instance = this;
    }
    private void ShowLoginMenu()
    {
        // 显示登录菜单
        this.menuPanel.Controls.Clear();
        this.menuPanel.Controls.Add(this.AccountCombo);
        this.menuPanel.Controls.Add(this.LoginGroup);

    }
    private void ShowMainMenu()
    {
        this.menuPanel.Controls.Clear();
        this.menuPanel.Controls.Add(this.AccountCombo);
        this.menuPanel.Controls.Add(this.HomeGroup);
    }
    private void ShowMaterialMenu()
    {
        this.menuPanel.Controls.Clear();
        this.menuPanel.Controls.Add(this.AccountCombo);
        this.menuPanel.Controls.Add(this.RuneGroup);
    }


    private void ShowAccountCombo()
    {
        foreach (var account in AccountCfg.Instance.Accounts)
        {
            AccountCombo.Items.Add(account.Username);
        }

        if (AccountCombo.Items.Count > 0)
        {
            AccountCombo.SelectedIndex = 0; // Select the first item by default
        }
    }


    private void InitializeChromium()
    {
        // 初始化CefSharp前设置路径和方案
        var settings = new CefSettings();
        settings.CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache"); // 确保这是一个有效的、可写的路径
        Cef.Initialize(settings);
        // 初始化第一个浏览器
        browser = new ChromiumWebBrowser("https://www.idleinfinity.cn/Home/Index");
        // 绑定对象
        browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
        browser.JavascriptObjectRepository.Register("Bridge", new Bridge(), isAsync: true, options: BindingOptions.DefaultBinder);
        browser.KeyboardHandler = new CEFKeyBoardHandler();
        this.browserPanel.Controls.Add(browser);
        // 等待页面加载完成后执行脚本
        browser.FrameLoadEnd += OnFrameLoadEnd;
    }

    private void AccountCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
        //存储当前用户
        if (AccountController.User != null && !PageLoadHandler.ContainsUrl(browser.Address, PageLoadHandler.LoginPage)) PageLoadHandler.SaveCookieAndCache(browser, true);
        // 获取选中的项
        string selectedItem = this.AccountCombo.SelectedItem.ToString();
        var item = AccountCfg.Instance.Accounts.Where(s => s.Username == selectedItem).FirstOrDefault();
        AccountController.User = new UserModel { Username = selectedItem, Password = item.Password };
        if (browser != null)
        {
            browser.Load("https://www.idleinfinity.cn/Home/Login");
        }
    }




    private void OnFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
    {
        var bro = sender as ChromiumWebBrowser;
        string url = bro.Address;
        Console.WriteLine(url);
        if (PageLoadHandler.ContainsUrl(url, PageLoadHandler.LoginPage))
        {
            this.Invoke(new Action(() => ShowLoginMenu()));
            PageLoadHandler.LoadCookieAndCache(browser);
        }
        else if (PageLoadHandler.ContainsUrl(url, PageLoadHandler.HomePage))
        {
            this.Invoke(new Action(() => ShowMainMenu()));
        }
        Task.Run(async () =>
        {
            await PageLoadHandler.LoadJsByUrl(browser);
        });

        if (!PageLoadHandler.ContainsUrl(url, PageLoadHandler.LoginPage))
        {
            PageLoadHandler.SaveCookieAndCache(browser);
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

    private void BtnAutoRune_Click(object sender, EventArgs e)
    {
        RuneController.Instance.AutoUpgradeRune();
    }
}