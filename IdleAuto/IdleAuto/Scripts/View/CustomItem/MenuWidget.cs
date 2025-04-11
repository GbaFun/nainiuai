using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IdleAuto.Scripts.Controller;
using System.Text.RegularExpressions;
using IdleAuto.Scripts.View;
using CefSharp.DevTools.FedCm;
using CefSharp;
using System.Threading;
using System.Security.Principal;

namespace IdleAuto.Scripts.View
{
    public partial class MenuWidget : UserControl
    {
        #region UI方法

        private System.Threading.Timer refreshTimer;
        private System.Threading.Timer autoTimer;
        public MenuWidget()
        {
            InitializeComponent();
            ShowAccountCombo();
            SetDailyTimer();
        }

        private void MenuWidget_Load(object sender, EventArgs e)
        {

        }
        private void ShowAccountCombo()
        {
            foreach (var account in AccountCfg.Instance.Accounts)
            {
                AccountCombo.Items.Add(account);
            }
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
        }
        private void ShowRoleMenu()
        {
            this.menuPanel.Controls.Clear();
            this.menuPanel.Controls.Add(this.AccountCombo);
            this.menuPanel.Controls.Add(this.RoleGroup);
        }
        private void ShowMaterialMenu()
        {
            this.menuPanel.Controls.Clear();
            this.menuPanel.Controls.Add(this.AccountCombo);
            this.menuPanel.Controls.Add(this.RuneGroup);
        }
        private void ShowAhMenu()
        {
            this.menuPanel.Controls.Clear();
            this.menuPanel.Controls.Add(this.AccountCombo);
            this.AhGroup.Controls.Add(this.BtnAutoAh);
            this.menuPanel.Controls.Add(this.AhGroup);
        }
        private void ShowNoneMenu()
        {
            this.menuPanel.Controls.Clear();
        }


        #endregion

        #region 菜单交互事件

        private async void BtnInit_Click(object sender, EventArgs e)
        {
            FlowController.StartInit();
        }

        private void BtnClean_Click(object sender, EventArgs e)
        {
            RepairManager.Instance.UpdateEquips(AccountController.Instance.User);
        }

        private void BtnAutoEquip_Click(object sender, EventArgs e)
        {
            RepairManager.Instance.AutoRepair(AccountController.Instance.User);
        }
        public void BtnAutoAh_Click(object sender, EventArgs e)
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

        private async void AccountCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            //切换角色将不再打开页面
            Account item = this.AccountCombo.SelectedItem as Account;
            AccountController.Instance.User = new UserModel(item);
            //await TabManager.Instance.TriggerAddBroToTap(AccountController.Instance.User);
        }

        private async void BtnClear_Click(object sender, EventArgs e)
        {
            TabManager.Instance.GetWindow().Close();
        }

        private void btnMap_Click(object sender, EventArgs e)
        {
            string[] MapSwitchAccounts = null;

            FlowController.GroupWork(4, 1, FlowController.StartMapSwitch);
        }
        private void BtnSkillPoint_Click(object sender, EventArgs e)
        {
            if (AccountController.Instance.User != null)
            {
                Task.Run(async () =>
                {
                    var window = await TabManager.Instance.TriggerAddBroToTap(AccountController.Instance.User);
                    await FlowController.StartAddSkill(window);

                });
            }

            else FlowController.GroupWork(4, 1, FlowController.StartAddSkill);
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            string[] accounts = null;
            //accounts = new string[] {"南方工具人7" };
            Task.Run(async () =>
            {
                try
                {
                    await FlowController.GroupWork(3, 1, RepairManager.Instance.ClearEquips, accounts);
                    await FlowController.GroupWork(3, 1, RepairManager.Instance.UpdateEquips);
                    await FlowController.GroupWork(3, 1, RepairManager.Instance.AutoRepair);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });
        }

        #endregion

        #region 监听事件
        private void OnBrowserFrameLoadStart(params object[] args)
        { }

        private void OnBrowserFrameLoadEnd(params object[] args)
        {
            string url = args[0] as string;
            if (PageLoadHandler.ContainsUrl(url, PageLoadHandler.LoginPage))
            {
                this.Invoke(new Action(() => ShowLoginMenu()));
            }
            else if (PageLoadHandler.ContainsUrl(url, PageLoadHandler.HomePage))
            {
                this.Invoke(new Action(() => ShowMainMenu()));
            }
            else if (PageLoadHandler.ContainsUrl(url, PageLoadHandler.RolePage))
            {
                this.Invoke(new Action(() => ShowRoleMenu()));
            }
            else if (PageLoadHandler.ContainsUrl(url, PageLoadHandler.EquipPage))
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
            else
            {
                this.Invoke(new Action(() => ShowNoneMenu()));
            }

            //检查url是否包含角色id，刷新角色选则内容
            //Match result = RegexRoleUrl(url);
            //if (result.Success)
            //{
            //    if (int.TryParse(result.Groups[1].Value, out int id))
            //    {
            //        id = int.Parse(result.Groups[1].Value);
            //        RoleModel role = RoleCombo.Items.Cast<RoleModel>().FirstOrDefault(s => s.RoleId == id);
            //        if (role == null) return;//先跳过一下
            //        this.Invoke(new Action(() => RefreshRole(role)));
            //    }
            //}
            //this.Invoke(new Action(() => HideLoadingPanel()));
        }
        private Match RegexRoleUrl(string url)
        {
            Regex reg = new Regex(@".*\\?id=(\d*)");
            Match result = reg.Match(url);
            return result;
        }


        #endregion

        private void SetDailyTimer()
        {
            DateTime now = DateTime.Now;
            // 每两小时自动运行效率监控
            //  refreshTimer = new System.Threading.Timer(AutoMonitorElapsed, null, TimeSpan.FromHours(1), TimeSpan.FromHours(6));

            // 每天凌晨6点自动运行全部账号清库盘库修车指令
            DateTime nextRun2 = now.Date.AddDays(1).AddHours(6);
            TimeSpan initialDelay2 = nextRun2 - now;
            // autoTimer = new System.Threading.Timer(AutoEquipElapsed, null, initialDelay2, TimeSpan.FromHours(24));
        }
        private void AutoMonitorElapsed(object state)
        {
            // 执行BtnRefresh的点击事件
            if (InvokeRequired)
            {
                Invoke(new Action(() => btnMonitor_Click(null, EventArgs.Empty)));
            }
            else
            {
                btnMonitor_Click(null, EventArgs.Empty);
            }
        }
        private void AutoEquipElapsed(object state)
        {
            // 执行BtnRefresh的点击事件
            if (InvokeRequired)
            {
                Invoke(new Action(() => { RepairManager.Instance.AutoRepair(); }));
            }
            else
            {
                RepairManager.Instance.AutoRepair();
            }
        }

        private async void btnHomePage_Click(object sender, EventArgs e)
        {
            Account item = this.AccountCombo.SelectedItem as Account;
            AccountController.Instance.User = new UserModel(item);
            var user = AccountController.Instance.User;
            var window = await TabManager.Instance.TriggerAddBroToTap(user);
        }

        private void btnSyncFilter_Click(object sender, EventArgs e)
        {
            FlowController.GroupWork(4, 1, FlowController.SyncFilter);
        }

        private void btnMonitor_Click(object sender, EventArgs e)
        {
            FlowController.GroupWork(4, 1, FlowController.StartEfficencyMonitor);
        }

        private void BtnInventory_Click(object sender, EventArgs e)
        {
        }

        private async void BtnTest_Click(object sender, EventArgs e)
        {
            //Account item = this.AccountCombo.SelectedItem as Account;
            //AccountController.Instance.User = new UserModel(item);
            //var user = AccountController.Instance.User;
            //var window = TabManager.Instance.GetWindow();
            //TradeController controller = new TradeController();
            //await controller.AcceptAll(window, user);
        }

        private void btnTestArtifact_Click(object sender, EventArgs e)
        {
            FlowController.MakeArtifactTest();
        }

        private void Button_Inventory_Click(object sender, EventArgs e)
        {
            Account item = this.AccountCombo.SelectedItem as Account;
            AccountController.Instance.User = new UserModel(item);
            var user = AccountController.Instance.User;
            RepairManager.Instance.ClearEquips(user);
        }

        private void btnDungeon_Click(object sender, EventArgs e)
        {

            FlowController.GroupWork(4, 1, FlowController.StartDailyDungeon);

        }

        private void btnProxy_Click(object sender, EventArgs e)
        {
            var user = AccountController.Instance.User;
            TabManager.Instance.TriggerAddBroToTap(user, true);
        }

        private void BtnAutoRune_Click(object sender, EventArgs e)
        {
            RuneConfigForm runeForm = new RuneConfigForm();
            runeForm.ShowDialog();
        }

        private void HomeGroup_Enter(object sender, EventArgs e)
        {

        }

        private void btnSendRune_Click(object sender, EventArgs e)
        {
            FlowController.SendRune();
        }

        private void btnSendEquip_Click(object sender, EventArgs e)
        {
            FlowController.SendEquip();
        }
    }
}
