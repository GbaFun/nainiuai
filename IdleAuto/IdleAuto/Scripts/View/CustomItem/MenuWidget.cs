﻿using System;
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
using IdleAuto.Scripts.Utils;
using IdleAuto.Db;
using IdleAuto.Scripts.Wrap;
using System.IO;
using System.Linq.Expressions;
using IdleAuto.Scripts.Model;

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
            LoadComBoxData();
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

            FreeDb.Sqlite.Delete<TradeModel>().Where(p => 1 == 1).ExecuteAffrows();
            FreeDb.Sqlite.Delete<LockEquipModel>().Where(p => 1 == 1).ExecuteAffrows();
            RepairManager.Instance.AutoRepair(null);
        }
        public void BtnAutoAh_Click(object sender, EventArgs e)
        {
            //todo 自动拍卖
            Task.Run(async () =>
            {
                var win = await TabManager.Instance.TriggerAddBroToTap(AccountController.Instance.User);
                var a = new AuctionController(win);
                await a.StartScan(win.User.FirstRole);
            });

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
            string[] MapSwitchAccounts = RepairManager.NanfangAccounts.ToArray();

            FlowController.GroupWork(4, 1, FlowController.StartMapSwitch, MapSwitchAccounts);
        }
        private void BtnSkillPoint_Click(object sender, EventArgs e)
        {


            FlowController.GroupWork(4, 1, FlowController.StartAddSkill);
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            string[] accounts = null;
            FreeDb.Sqlite.Delete<TradeModel>().Where(p => 1 == 1).ExecuteAffrows();
            FreeDb.Sqlite.Delete<LockEquipModel>().Where(p => 1 == 1).ExecuteAffrows();
            var acc = RepairManager.ActiveAcc.Except(RepairManager.AccDone).ToArray();
            //accounts = new string[] {"南方工具人7" };
            Task.Run(async () =>
            {
                try
                {

                    await FlowController.GroupWork(2, 1, RepairManager.Instance.AutoRepair, acc);


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

        public void LoadComBoxData()
        {
            comboJob.DataSource = RepairManager.Jobs;
        }

        private void SetDailyTimer()
        {

            //定时任务测试
            // refreshTimer = new System.Threading.Timer(AutoMonitorElapsed, null, TimeSpan.FromMinutes(60), TimeSpan.FromMinutes(120));
        }
        private void AutoMonitorElapsed(object state)
        {
            // 执行BtnRefresh的点击事件
            if (InvokeRequired)
            {
                Invoke(new Action(() => BtnTest_Click(null, EventArgs.Empty)));
            }
            else
            {
                BtnTest_Click(null, EventArgs.Empty);
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
            var acc = RepairManager.NainiuAccounts.Concat(RepairManager.NanfangAccounts).Where(p => p != "RasdGch").ToArray();
           //  var acc = RepairManager.BudingAccounts.Where(p => p != "010").ToArray();
            FlowController.GroupWork(4, 1, FlowController.SyncFilter, acc);
        }

        private void btnMonitor_Click(object sender, EventArgs e)
        {
            FlowController.GroupWork(3, 1, FlowController.StartEfficencyMonitor);
        }

        private void BtnInventory_Click(object sender, EventArgs e)
        {
        }

        private async void BtnTest_Click(object sender, EventArgs e)
        {
            //await FlowController.RegisterColdConversion();
            //await FlowController.MakeYongheng();
            //FlowController.RegisterYongheng();

            //await FlowController.MakeMori();
            //FlowController.RegisterMori();
            // //await FlowController.MoveTaGeAo();
            ////  await FlowController.SendXianji();
            // // await FlowController.SaveRuneMap();
            //  await FlowController.PassDungeon(91, 90, 80);
            // FlowController.GroupWork(3, 1, FlowController.ReformMageNecklace);
            //Expression<Func<EquipModel,EquipSuitModel, bool>> exp = (a,b) => a.EquipName.Contains("永恒");
            //EquipUtil.QueryEquipInRepo(exp, 2268);

            //  FlowController.GroupWork(3, 1, FlowController.StartDailyDungeon, RepairManager.NainiuAccounts);

            // FlowController.FightWorldBoss();

            //FlowController.SwitchYongheng();
            // FlowController.ReformShengyi();
            FlowController.UseBox();
            // FlowController.ReformBaseEq();
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

            FlowController.GroupWork(3, 1, FlowController.StartDailyDungeon, RepairManager.ActiveAcc);

        }

        private void btnProxy_Click(object sender, EventArgs e)
        {
            var user = AccountController.Instance.User;
            TabManager.Instance.TriggerAddBroToTap(user, true);
        }

        private void BtnAutoRune_Click(object sender, EventArgs e)
        {
            FlowController.InitializeRuneCfgItems();
            FlowController.GroupWork(3, 0, FlowController.RuneUpgrade);
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

        private void BtnDealTrade_Click(object sender, EventArgs e)
        {
            FlowController.DealDemandEquip();
        }

        private void btnAuction_Click(object sender, EventArgs e)
        {
            FlowController.SellEquipToAuction();
        }

        private void btnRollArtifact_Click(object sender, EventArgs e)
        {
            FlowController.ContinueJob(emTaskType.RollTianzai, FlowController.RollTianzai);
        }

        private void btnNec_Click(object sender, EventArgs e)
        {
            //FlowController.RepairNec();
            FlowController.RepairXianji();
        }

        private void BtnReform_Click(object sender, EventArgs e)
        {
            FlowController.ReformDungeonAndRings();
        }

        private void btnCookie_Click(object sender, EventArgs e)
        {
            var target = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie");
            var des = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "backup"));
            ;
            FileUtil.CopyDirectory(target, des);

        }

        private void btnGem_Click(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                await FlowController.GroupWork(3, 1, FlowController.AutoUpgradeGem);
            });


        }

        private void btnMf_Click(object sender, EventArgs e)
        {
            if (AccountController.Instance.User != null)
            {
                Task.Run(async () =>
                {
                    var win = await TabManager.Instance.TriggerAddBroToTap(AccountController.Instance.User);
                    await FlowController.UpdateMfEquip(win);
                });
            }
            else FlowController.GroupWork(3, 1, FlowController.UpdateMfEquip, RepairManager.ActiveAcc);
        }

        private void btnPreDel_Click(object sender, EventArgs e)
        {
            FlowController.ClearRepoPre();
        }

        private void btnConfirmDel_Click(object sender, EventArgs e)
        {
            FlowController.ConfirmDelEquip();
        }

        private void btnSanBoss_Click(object sender, EventArgs e)
        {
            FlowController.FightWorldBoss();
        }

        private void btnHunterSkill_Click(object sender, EventArgs e)
        {
            FlowController.SetHunterAndPastor();
        }

        private void btnRecovery_Click(object sender, EventArgs e)
        {
            FlowController.RecoverHunterAndPastor();
        }

        private void btnShengyi_Click(object sender, EventArgs e)
        {
            FlowController.ReformShengyi();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MenuInstance.SecondForm = MenuInstance.SecondForm == null ? new SecondMenuForm() : MenuInstance.SecondForm;

            MenuInstance.SecondForm.ShowDialog();
        }

        private void comboJob_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 获取当前选中项的值
            string selectedValue = comboJob.SelectedItem?.ToString();
            // 获取当前选中项的索引
            int selectedIndex = comboJob.SelectedIndex;

            // 执行相关操作，如更新其他控件数据
            RepairManager.RepairJob = selectedValue;
        }

        private void btnGroupInit_Click(object sender, EventArgs e)
        {
            FlowController.InitGroup();
        }
    }
}
