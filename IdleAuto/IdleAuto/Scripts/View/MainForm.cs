using CefSharp;
using CefSharp.DevTools.Network;
using CefSharp.WinForms;
using CefSharp.WinForms.Internals;
using IdleAuto.Db;
using IdleAuto.Scripts.Controller;
using IdleAuto.Scripts.View;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
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
    public MaskForm maskForm
    {
        get;
        private set;
    }
    private emMaskType maskType;
    private void MainForm_Load(object sender, EventArgs e)
    {
    }

    public MainForm()
    {
        InitializeComponent();
        InitializeChromium();
        ShowAccountCombo();
        HideLoadingPanel();
        EventManager.Instance.SubscribeEvent(emEventType.OnAccountDirty, OnAccountDirty);
        EventManager.Instance.SubscribeEvent(emEventType.OnCharLoaded, CharacterController.Instance.OnCharLoaded);
        OnAccountDirty(null);
        Instance = this;
    }
    private void ShowLoginMenu()
    {
        // 显示登录菜单
        this.menuPanel.Controls.Clear();
        this.menuPanel.Controls.Add(this.AccountCombo);
        this.menuPanel.Controls.Add(this.LoginGroup);
        //this.menuPanel.Controls.Add(this.JumpGroup);

    }
    private void ShowMainMenu()
    {
        this.menuPanel.Controls.Clear();
        this.menuPanel.Controls.Add(this.AccountCombo);
        this.menuPanel.Controls.Add(this.HomeGroup);
        this.menuPanel.Controls.Add(this.JumpGroup);
        ShowRoleCombo();
    }
    private void ShowRoleMenu()
    {
        this.menuPanel.Controls.Clear();
        this.menuPanel.Controls.Add(this.AccountCombo);
        this.menuPanel.Controls.Add(this.RoleGroup);
        this.menuPanel.Controls.Add(this.JumpGroup);
    }
    private void ShowMaterialMenu()
    {
        this.menuPanel.Controls.Clear();
        this.menuPanel.Controls.Add(this.AccountCombo);
        this.menuPanel.Controls.Add(this.RuneGroup);
        this.menuPanel.Controls.Add(this.JumpGroup);
    }
    private void ShowAhMenu()
    {
        this.menuPanel.Controls.Clear();
        this.menuPanel.Controls.Add(this.AccountCombo);
        this.menuPanel.Controls.Add(this.AhGroup);
        this.menuPanel.Controls.Add(this.JumpGroup);
    }
    public void ShowLoadingPanel(string content = "", emMaskType mType = emMaskType.WEB_LOADING)
    {
        P.Log($"ShowLoadingPanel--From:{mType}", emLogType.Warning);
        if (maskForm == null)
        {
            maskForm = new MaskForm(this);
        }
        if (!maskForm.Visible)
        {
            maskForm.Show();
            maskType = mType;
        }
        if (!string.IsNullOrEmpty(content) && mType == maskType)
        {
            maskForm.SetLoadContent(content);
        }
        //this.LoadingPanel.Visible = true;
    }
    public void HideLoadingPanel(emMaskType mType = emMaskType.WEB_LOADING)
    {
        P.Log($"HideLoadingPanel--From:{mType}", emLogType.Warning);
        if (mType != maskType)
        {
            return;
        }
        maskForm?.Hide();
        //if (this.LoadingPanel.Visible)
        //    this.LoadingPanel.Visible = false;
    }

    public void SetLoadContent(string content)
    {
        if (maskForm == null)
        {
            maskForm = new MaskForm(this);
        }
        if (!maskForm.Visible)
            maskForm.Show();
        if (!string.IsNullOrEmpty(content))
        {
            maskForm.SetLoadContent(content);
        }
    }

    private void ShowAccountCombo()
    {
        foreach (var account in AccountCfg.Instance.Accounts)
        {
            AccountCombo.Items.Add(account);
        }

        if (AccountCombo.Items.Count > 0)
        {
            AccountCombo.SelectedIndex = 0; // Select the first item by default
        }
    }
    private void ShowRoleCombo()
    {
        if (RoleCombo.Created)
        {
            RoleCombo.Items.Clear();
            RoleCombo.SelectedIndex = -1;
            RoleCombo.Text = "";
            foreach (var role in AccountController.Instance.User.Roles)
            {
                RoleCombo.Items.Add(role);
            }
        }
    }
    private void RefreshRole(RoleModel role)
    {
        if (RoleCombo.Created)
        {
            RoleCombo.SelectedItem = role;
            RoleCombo.Text = role.RoleName;
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
        browser.FrameLoadStart += OnFrameLoadStart;
    }

    private void AccountCombo_SelectedIndexChanged(object sender, EventArgs e)
    {

        //存储当前用户
        if (AccountController.Instance.User != null && !PageLoadHandler.ContainsUrl(browser.Address, PageLoadHandler.LoginPage)) PageLoadHandler.SaveCookieAndCache(browser, true);
        // 获取选中的项
        Account item = this.AccountCombo.SelectedItem as Account;
        //var item = AccountCfg.Instance.Accounts.Where(s => s.Username == selectedItem).FirstOrDefault();
        AccountController.Instance.User = new UserModel(item);
        if (browser != null)
        {
            Task.Run(async () =>
            {
                await DevToolUtil.ClearCookiesAsync(browser);
            });
            browser.Load("https://www.idleinfinity.cn/Home/Login");
        }
    }

    private void RoleCombo_SelectedIndexChanged(object sender, EventArgs e)
    {
        RoleModel role = this.RoleCombo.SelectedItem as RoleModel;

        string url = browser.Address;
        Match result = RegexRoleUrl(url);

        if (result.Success)
        {
            string id = result.Groups[1].Value;
            if (role.RoleId == int.Parse(id))
            {
                return;
            }
            else
            {
                url = url.Replace(id, role.RoleId.ToString());
                browser.Load(url);
            }
        }
        else
        {
            browser.Load($"https://www.idleinfinity.cn/Character/Detail?id={role.RoleId}");
        }
    }

    private void OnFrameLoadStart(object sender, FrameLoadStartEventArgs e)
    {
        var bro = sender as ChromiumWebBrowser;
        if (bro != null)
        {
            this.Invoke(new Action(() => ShowLoadingPanel("页面加载中...")));
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
            Task.Run(async () =>
            {
                await PageLoadHandler.LoadCookieAndCache(browser);
            });
        }
        else if (PageLoadHandler.ContainsUrl(url, PageLoadHandler.HomePage))
        {
            this.Invoke(new Action(() => ShowMainMenu()));
        }
        else if (PageLoadHandler.ContainsUrl(url, PageLoadHandler.RolePage))
        {
            this.Invoke(new Action(() => ShowRoleMenu()));
        }
        else if (PageLoadHandler.ContainsUrl(url, PageLoadHandler.MaterialPage))
        {
            this.Invoke(new Action(() => ShowMaterialMenu()));
        }
        else if (PageLoadHandler.ContainsUrl(url, PageLoadHandler.AhPage))
        {
            this.Invoke(new Action(() => ShowAhMenu()));
        }
        Task.Run(async () =>
        {
            await PageLoadHandler.LoadJsByUrl(browser);
        });

        if (!PageLoadHandler.ContainsUrl(url, PageLoadHandler.LoginPage))
        {
            PageLoadHandler.SaveCookieAndCache(browser);
        }

        //检查url是否包含角色id，刷新角色选则内容
        Match result = RegexRoleUrl(url);
        if (result.Success)
        {
            if (int.TryParse(result.Groups[1].Value, out int id))
            {
                id = int.Parse(result.Groups[1].Value);
                RoleModel role = RoleCombo.Items.Cast<RoleModel>().FirstOrDefault(s => s.RoleId == id);
                this.Invoke(new Action(() => RefreshRole(role)));
            }
        }
        this.Invoke(new Action(() => HideLoadingPanel()));
    }

    private void OnAccountDirty(params object[] args)
    {
        if (this.IsHandleCreated)
        {
            this.Invoke(new Action(() => ShowRoleCombo()));
        }
        else
        {
            // Handle the case where the handle is not created yet
            // For example, you can directly call the method
            ShowRoleCombo();
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

    private int GetCurRoleId()
    {
        int roleId;
        RoleModel role = this.RoleCombo.SelectedItem as RoleModel;
        if (role == null)
            roleId = AccountController.Instance.User.FirstRole.RoleId;
        else
            roleId = role.RoleId;

        return roleId;
    }

    private void BtnAutoRune_Click(object sender, EventArgs e)
    {
        var result = MessageBox.Show("是否需要确认符文保留数量", "提示", MessageBoxButtons.OKCancel);
        if (result == DialogResult.OK)
        {
            var form = new RuneConfigForm();
            form.Show();
        }
        else
        {
            RuneController.Instance.AutoUpgradeRune();
        }
        //RuneController.Instance.AutoUpgradeRune();
    }


    private void BtnHome_Click(object sender, EventArgs e)
    {
        browser.Load("https://www.idleinfinity.cn/Home/Index");
    }

    private void BtnRank_Click(object sender, EventArgs e)
    {
        browser.Load($"https://www.idleinfinity.cn/Character/RankingList?id={GetCurRoleId()}");
    }

    private void BtnMaterial_Click(object sender, EventArgs e)
    {
        browser.Load($"https://www.idleinfinity.cn/Equipment/Material?id={GetCurRoleId()}");
    }

    private void BtnRuneLog_Click(object sender, EventArgs e)
    {
        //https://www.idleinfinity.cn/Character/RuneLog?uid=13826420&id=10728
        //https://www.idleinfinity.cn/Character/RuneLog?uid=13846682&id=10728
    }

    private void BtnAutoAh_Click(object sender, EventArgs e)
    {
        //todo 自动拍卖
        if (!AuctionController.Instance.IsStart)
        {
            AuctionController.Instance.StartScan();
            this.BtnAutoAh.Text = "停止扫拍";
        }
        else
        {
            AuctionController.Instance.StopScan();
            this.BtnAutoAh.Text = "开始扫拍";
        }
    }

    private Match RegexRoleUrl(string url)
    {
        Regex reg = new Regex(@".*\\?id=(\d*)");
        Match result = reg.Match(url);
        return result;
    }

    private void BtnAutoEquip_Click(object sender, EventArgs e)
    {
        EquipController.Instance.StartAutoEquip();
    }

    private void BtnInit_Click(object sender, EventArgs e)
    {
        EquipController.Instance.SaveAllEquips();

        //todo 账号初始化
        // 根据配置创建角色(自动命名,选择种族,职业)
        // 自动创建工会
        // 自动进行组队
    }
}