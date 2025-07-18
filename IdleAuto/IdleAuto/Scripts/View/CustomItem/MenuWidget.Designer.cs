using IdleAuto.Properties;

namespace IdleAuto.Scripts.View
{
    partial class MenuWidget
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.menuPanel = new System.Windows.Forms.Panel();
            this.AccountCombo = new System.Windows.Forms.ComboBox();
            this.HomeGroup = new System.Windows.Forms.GroupBox();
            this.btnShengyi = new System.Windows.Forms.Button();
            this.btnRecovery = new System.Windows.Forms.Button();
            this.btnHunterSkill = new System.Windows.Forms.Button();
            this.btnSanBoss = new System.Windows.Forms.Button();
            this.btnConfirmDel = new System.Windows.Forms.Button();
            this.btnPreDel = new System.Windows.Forms.Button();
            this.btnMf = new System.Windows.Forms.Button();
            this.btnGem = new System.Windows.Forms.Button();
            this.btnCookie = new System.Windows.Forms.Button();
            this.btnNec = new System.Windows.Forms.Button();
            this.btnRollArtifact = new System.Windows.Forms.Button();
            this.btnAuction = new System.Windows.Forms.Button();
            this.btnDealTrade = new System.Windows.Forms.Button();
            this.btnSendEquip = new System.Windows.Forms.Button();
            this.btnSendRune = new System.Windows.Forms.Button();
            this.btnProxy = new System.Windows.Forms.Button();
            this.btnDungeon = new System.Windows.Forms.Button();
            this.Button_Inventory = new System.Windows.Forms.Button();
            this.btnReform = new System.Windows.Forms.Button();
            this.BtnTest = new System.Windows.Forms.Button();
            this.btnHomePage = new System.Windows.Forms.Button();
            this.BtnInit = new System.Windows.Forms.Button();
            this.BtnClean = new System.Windows.Forms.Button();
            this.BtnAutoEquip = new System.Windows.Forms.Button();
            this.BtnSkillPoint = new System.Windows.Forms.Button();
            this.btnMap = new System.Windows.Forms.Button();
            this.btnSyncFilter = new System.Windows.Forms.Button();
            this.BtnAutoAh = new System.Windows.Forms.Button();
            this.BtnTODO2 = new System.Windows.Forms.Button();
            this.btnMonitor = new System.Windows.Forms.Button();
            this.BtnClear = new System.Windows.Forms.Button();
            this.BtnAutoRune = new System.Windows.Forms.Button();
            this.BtnInventory = new System.Windows.Forms.Button();
            this.RuneGroup = new System.Windows.Forms.GroupBox();
            this.AhGroup = new System.Windows.Forms.GroupBox();
            this.RoleGroup = new System.Windows.Forms.GroupBox();
            this.LoginGroup = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.CurLoginAccount = new System.Windows.Forms.Label();
            this.BtnLogin = new System.Windows.Forms.Button();
            this.menuPanel.SuspendLayout();
            this.HomeGroup.SuspendLayout();
            this.LoginGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuPanel
            // 
            this.menuPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.menuPanel.AutoScroll = true;
            this.menuPanel.Controls.Add(this.AccountCombo);
            this.menuPanel.Controls.Add(this.HomeGroup);
            this.menuPanel.Location = new System.Drawing.Point(0, 0);
            this.menuPanel.Margin = new System.Windows.Forms.Padding(0);
            this.menuPanel.Name = "menuPanel";
            this.menuPanel.Size = new System.Drawing.Size(300, 754);
            this.menuPanel.TabIndex = 0;
            // 
            // AccountCombo
            // 
            this.AccountCombo.DisplayMember = "Username";
            this.AccountCombo.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.AccountCombo.FormattingEnabled = true;
            this.AccountCombo.Location = new System.Drawing.Point(0, 0);
            this.AccountCombo.Margin = new System.Windows.Forms.Padding(0);
            this.AccountCombo.Name = "AccountCombo";
            this.AccountCombo.Size = new System.Drawing.Size(265, 32);
            this.AccountCombo.TabIndex = 2;
            this.AccountCombo.SelectedIndexChanged += new System.EventHandler(this.AccountCombo_SelectedIndexChanged);
            // 
            // HomeGroup
            // 
            this.HomeGroup.Controls.Add(this.btnShengyi);
            this.HomeGroup.Controls.Add(this.btnRecovery);
            this.HomeGroup.Controls.Add(this.btnHunterSkill);
            this.HomeGroup.Controls.Add(this.btnSanBoss);
            this.HomeGroup.Controls.Add(this.btnConfirmDel);
            this.HomeGroup.Controls.Add(this.btnPreDel);
            this.HomeGroup.Controls.Add(this.btnMf);
            this.HomeGroup.Controls.Add(this.btnGem);
            this.HomeGroup.Controls.Add(this.btnCookie);
            this.HomeGroup.Controls.Add(this.btnNec);
            this.HomeGroup.Controls.Add(this.btnRollArtifact);
            this.HomeGroup.Controls.Add(this.btnAuction);
            this.HomeGroup.Controls.Add(this.btnDealTrade);
            this.HomeGroup.Controls.Add(this.btnSendEquip);
            this.HomeGroup.Controls.Add(this.btnSendRune);
            this.HomeGroup.Controls.Add(this.btnProxy);
            this.HomeGroup.Controls.Add(this.btnDungeon);
            this.HomeGroup.Controls.Add(this.Button_Inventory);
            this.HomeGroup.Controls.Add(this.btnReform);
            this.HomeGroup.Controls.Add(this.BtnTest);
            this.HomeGroup.Controls.Add(this.btnHomePage);
            this.HomeGroup.Controls.Add(this.BtnInit);
            this.HomeGroup.Controls.Add(this.BtnClean);
            this.HomeGroup.Controls.Add(this.BtnAutoEquip);
            this.HomeGroup.Controls.Add(this.BtnSkillPoint);
            this.HomeGroup.Controls.Add(this.btnMap);
            this.HomeGroup.Controls.Add(this.btnSyncFilter);
            this.HomeGroup.Controls.Add(this.BtnAutoAh);
            this.HomeGroup.Controls.Add(this.BtnTODO2);
            this.HomeGroup.Controls.Add(this.btnMonitor);
            this.HomeGroup.Controls.Add(this.BtnClear);
            this.HomeGroup.Controls.Add(this.BtnAutoRune);
            this.HomeGroup.Location = new System.Drawing.Point(0, 44);
            this.HomeGroup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.HomeGroup.Name = "HomeGroup";
            this.HomeGroup.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.HomeGroup.Size = new System.Drawing.Size(300, 710);
            this.HomeGroup.TabIndex = 4;
            this.HomeGroup.TabStop = false;
            this.HomeGroup.Text = "主页菜单";
            this.HomeGroup.Enter += new System.EventHandler(this.HomeGroup_Enter);
            // 
            // btnShengyi
            // 
            this.btnShengyi.Location = new System.Drawing.Point(20, 566);
            this.btnShengyi.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnShengyi.Name = "btnShengyi";
            this.btnShengyi.Size = new System.Drawing.Size(110, 28);
            this.btnShengyi.TabIndex = 25;
            this.btnShengyi.Text = "洗圣衣";
            this.btnShengyi.UseVisualStyleBackColor = true;
            this.btnShengyi.Click += new System.EventHandler(this.btnShengyi_Click);
            // 
            // btnRecovery
            // 
            this.btnRecovery.Location = new System.Drawing.Point(141, 529);
            this.btnRecovery.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRecovery.Name = "btnRecovery";
            this.btnRecovery.Size = new System.Drawing.Size(110, 28);
            this.btnRecovery.TabIndex = 24;
            this.btnRecovery.Text = "还原牧猎";
            this.btnRecovery.UseVisualStyleBackColor = true;
            this.btnRecovery.Click += new System.EventHandler(this.btnRecovery_Click);
            // 
            // btnHunterSkill
            // 
            this.btnHunterSkill.Location = new System.Drawing.Point(20, 532);
            this.btnHunterSkill.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnHunterSkill.Name = "btnHunterSkill";
            this.btnHunterSkill.Size = new System.Drawing.Size(110, 28);
            this.btnHunterSkill.TabIndex = 23;
            this.btnHunterSkill.Text = "猎牧洗点";
            this.btnHunterSkill.UseVisualStyleBackColor = true;
            this.btnHunterSkill.Click += new System.EventHandler(this.btnHunterSkill_Click);
            // 
            // btnSanBoss
            // 
            this.btnSanBoss.Location = new System.Drawing.Point(140, 456);
            this.btnSanBoss.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSanBoss.Name = "btnSanBoss";
            this.btnSanBoss.Size = new System.Drawing.Size(111, 28);
            this.btnSanBoss.TabIndex = 22;
            this.btnSanBoss.Text = "摸boss";
            this.btnSanBoss.UseVisualStyleBackColor = true;
            this.btnSanBoss.Click += new System.EventHandler(this.btnSanBoss_Click);
            // 
            // btnConfirmDel
            // 
            this.btnConfirmDel.Location = new System.Drawing.Point(140, 494);
            this.btnConfirmDel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnConfirmDel.Name = "btnConfirmDel";
            this.btnConfirmDel.Size = new System.Drawing.Size(111, 28);
            this.btnConfirmDel.TabIndex = 21;
            this.btnConfirmDel.Text = "确认删除";
            this.btnConfirmDel.UseVisualStyleBackColor = true;
            this.btnConfirmDel.Click += new System.EventHandler(this.btnConfirmDel_Click);
            // 
            // btnPreDel
            // 
            this.btnPreDel.Location = new System.Drawing.Point(20, 494);
            this.btnPreDel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPreDel.Name = "btnPreDel";
            this.btnPreDel.Size = new System.Drawing.Size(112, 28);
            this.btnPreDel.TabIndex = 20;
            this.btnPreDel.Text = "预删除";
            this.btnPreDel.UseVisualStyleBackColor = true;
            this.btnPreDel.Click += new System.EventHandler(this.btnPreDel_Click);
            // 
            // btnMf
            // 
            this.btnMf.Location = new System.Drawing.Point(20, 456);
            this.btnMf.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnMf.Name = "btnMf";
            this.btnMf.Size = new System.Drawing.Size(110, 28);
            this.btnMf.TabIndex = 19;
            this.btnMf.Text = "更新Mf";
            this.btnMf.UseVisualStyleBackColor = true;
            this.btnMf.Click += new System.EventHandler(this.btnMf_Click);
            // 
            // btnGem
            // 
            this.btnGem.Location = new System.Drawing.Point(140, 421);
            this.btnGem.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnGem.Name = "btnGem";
            this.btnGem.Size = new System.Drawing.Size(111, 28);
            this.btnGem.TabIndex = 18;
            this.btnGem.Text = "合宝石";
            this.btnGem.UseVisualStyleBackColor = true;
            this.btnGem.Click += new System.EventHandler(this.btnGem_Click);
            // 
            // btnCookie
            // 
            this.btnCookie.Location = new System.Drawing.Point(20, 622);
            this.btnCookie.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnCookie.Name = "btnCookie";
            this.btnCookie.Size = new System.Drawing.Size(115, 36);
            this.btnCookie.TabIndex = 1;
            this.btnCookie.Text = "cookie";
            this.btnCookie.UseVisualStyleBackColor = true;
            this.btnCookie.Click += new System.EventHandler(this.btnCookie_Click);
            // 
            // btnNec
            // 
            this.btnNec.Location = new System.Drawing.Point(20, 418);
            this.btnNec.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnNec.Name = "btnNec";
            this.btnNec.Size = new System.Drawing.Size(110, 31);
            this.btnNec.TabIndex = 17;
            this.btnNec.Text = "献祭";
            this.btnNec.UseVisualStyleBackColor = true;
            this.btnNec.Click += new System.EventHandler(this.btnNec_Click);
            // 
            // btnRollArtifact
            // 
            this.btnRollArtifact.Location = new System.Drawing.Point(141, 376);
            this.btnRollArtifact.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRollArtifact.Name = "btnRollArtifact";
            this.btnRollArtifact.Size = new System.Drawing.Size(111, 35);
            this.btnRollArtifact.TabIndex = 16;
            this.btnRollArtifact.Text = "roll神器";
            this.btnRollArtifact.UseVisualStyleBackColor = true;
            this.btnRollArtifact.Click += new System.EventHandler(this.btnRollArtifact_Click);
            // 
            // btnAuction
            // 
            this.btnAuction.Location = new System.Drawing.Point(20, 376);
            this.btnAuction.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnAuction.Name = "btnAuction";
            this.btnAuction.Size = new System.Drawing.Size(110, 35);
            this.btnAuction.TabIndex = 15;
            this.btnAuction.Text = "一键拍卖";
            this.btnAuction.UseVisualStyleBackColor = true;
            this.btnAuction.Click += new System.EventHandler(this.btnAuction_Click);
            // 
            // btnDealTrade
            // 
            this.btnDealTrade.Location = new System.Drawing.Point(140, 258);
            this.btnDealTrade.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnDealTrade.Name = "btnDealTrade";
            this.btnDealTrade.Size = new System.Drawing.Size(111, 32);
            this.btnDealTrade.TabIndex = 1;
            this.btnDealTrade.Text = "处理乞讨";
            this.btnDealTrade.UseVisualStyleBackColor = true;
            this.btnDealTrade.Click += new System.EventHandler(this.BtnDealTrade_Click);
            // 
            // btnSendEquip
            // 
            this.btnSendEquip.Location = new System.Drawing.Point(18, 336);
            this.btnSendEquip.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSendEquip.Name = "btnSendEquip";
            this.btnSendEquip.Size = new System.Drawing.Size(112, 32);
            this.btnSendEquip.TabIndex = 2;
            this.btnSendEquip.Text = "发送装备";
            this.btnSendEquip.UseVisualStyleBackColor = true;
            this.btnSendEquip.Click += new System.EventHandler(this.btnSendEquip_Click);
            // 
            // btnSendRune
            // 
            this.btnSendRune.Location = new System.Drawing.Point(140, 296);
            this.btnSendRune.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSendRune.Name = "btnSendRune";
            this.btnSendRune.Size = new System.Drawing.Size(111, 34);
            this.btnSendRune.TabIndex = 1;
            this.btnSendRune.Text = "发送符文";
            this.btnSendRune.UseVisualStyleBackColor = true;
            this.btnSendRune.Click += new System.EventHandler(this.btnSendRune_Click);
            // 
            // btnProxy
            // 
            this.btnProxy.Location = new System.Drawing.Point(153, 622);
            this.btnProxy.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnProxy.Name = "btnProxy";
            this.btnProxy.Size = new System.Drawing.Size(112, 34);
            this.btnProxy.TabIndex = 1;
            this.btnProxy.Text = "代理登录";
            this.btnProxy.UseVisualStyleBackColor = true;
            this.btnProxy.Click += new System.EventHandler(this.btnProxy_Click);
            // 
            // btnDungeon
            // 
            this.btnDungeon.Location = new System.Drawing.Point(140, 336);
            this.btnDungeon.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnDungeon.Name = "btnDungeon";
            this.btnDungeon.Size = new System.Drawing.Size(112, 34);
            this.btnDungeon.TabIndex = 14;
            this.btnDungeon.Text = "每日秘境";
            this.btnDungeon.UseVisualStyleBackColor = true;
            this.btnDungeon.Click += new System.EventHandler(this.btnDungeon_Click);
            // 
            // Button_Inventory
            // 
            this.Button_Inventory.Location = new System.Drawing.Point(138, 78);
            this.Button_Inventory.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Button_Inventory.Name = "Button_Inventory";
            this.Button_Inventory.Size = new System.Drawing.Size(112, 36);
            this.Button_Inventory.TabIndex = 13;
            this.Button_Inventory.Text = "一键删库";
            this.Button_Inventory.UseVisualStyleBackColor = true;
            this.Button_Inventory.Click += new System.EventHandler(this.Button_Inventory_Click);
            // 
            // btnReform
            // 
            this.btnReform.Location = new System.Drawing.Point(20, 166);
            this.btnReform.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnReform.Name = "btnReform";
            this.btnReform.Size = new System.Drawing.Size(112, 34);
            this.btnReform.TabIndex = 12;
            this.btnReform.Text = "改造白装";
            this.btnReform.UseMnemonic = false;
            this.btnReform.UseVisualStyleBackColor = true;
            this.btnReform.Click += new System.EventHandler(this.BtnReform_Click);
            // 
            // BtnTest
            // 
            this.BtnTest.Location = new System.Drawing.Point(141, 563);
            this.BtnTest.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.BtnTest.Name = "BtnTest";
            this.BtnTest.Size = new System.Drawing.Size(112, 36);
            this.BtnTest.TabIndex = 11;
            this.BtnTest.Text = "测试";
            this.BtnTest.UseVisualStyleBackColor = true;
            this.BtnTest.Click += new System.EventHandler(this.BtnTest_Click);
            // 
            // btnHomePage
            // 
            this.btnHomePage.Location = new System.Drawing.Point(18, 32);
            this.btnHomePage.Margin = new System.Windows.Forms.Padding(18, 8, 0, 8);
            this.btnHomePage.Name = "btnHomePage";
            this.btnHomePage.Size = new System.Drawing.Size(112, 34);
            this.btnHomePage.TabIndex = 4;
            this.btnHomePage.Text = "载入账号";
            this.btnHomePage.UseVisualStyleBackColor = true;
            this.btnHomePage.Click += new System.EventHandler(this.btnHomePage_Click);
            // 
            // BtnInit
            // 
            this.BtnInit.Location = new System.Drawing.Point(18, 76);
            this.BtnInit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BtnInit.Name = "BtnInit";
            this.BtnInit.Size = new System.Drawing.Size(112, 34);
            this.BtnInit.TabIndex = 2;
            this.BtnInit.Text = "账号初始化";
            this.BtnInit.UseVisualStyleBackColor = true;
            this.BtnInit.Click += new System.EventHandler(this.BtnInit_Click);
            // 
            // BtnClean
            // 
            this.BtnClean.Location = new System.Drawing.Point(138, 32);
            this.BtnClean.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.BtnClean.Name = "BtnClean";
            this.BtnClean.Size = new System.Drawing.Size(112, 36);
            this.BtnClean.TabIndex = 0;
            this.BtnClean.Text = "收菜盘库";
            this.BtnClean.UseVisualStyleBackColor = true;
            this.BtnClean.Click += new System.EventHandler(this.BtnClean_Click);
            // 
            // BtnAutoEquip
            // 
            this.BtnAutoEquip.Location = new System.Drawing.Point(138, 120);
            this.BtnAutoEquip.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.BtnAutoEquip.Name = "BtnAutoEquip";
            this.BtnAutoEquip.Size = new System.Drawing.Size(112, 34);
            this.BtnAutoEquip.TabIndex = 1;
            this.BtnAutoEquip.Text = "一键修车";
            this.BtnAutoEquip.UseVisualStyleBackColor = true;
            this.BtnAutoEquip.Click += new System.EventHandler(this.BtnAutoEquip_Click);
            // 
            // BtnSkillPoint
            // 
            this.BtnSkillPoint.Location = new System.Drawing.Point(138, 166);
            this.BtnSkillPoint.Margin = new System.Windows.Forms.Padding(14, 6, 0, 6);
            this.BtnSkillPoint.Name = "BtnSkillPoint";
            this.BtnSkillPoint.Size = new System.Drawing.Size(112, 34);
            this.BtnSkillPoint.TabIndex = 8;
            this.BtnSkillPoint.Text = "加点";
            this.BtnSkillPoint.UseVisualStyleBackColor = true;
            this.BtnSkillPoint.Click += new System.EventHandler(this.BtnSkillPoint_Click);
            // 
            // btnMap
            // 
            this.btnMap.Location = new System.Drawing.Point(138, 212);
            this.btnMap.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnMap.Name = "btnMap";
            this.btnMap.Size = new System.Drawing.Size(112, 36);
            this.btnMap.TabIndex = 4;
            this.btnMap.Text = "切图";
            this.btnMap.UseVisualStyleBackColor = true;
            this.btnMap.Click += new System.EventHandler(this.btnMap_Click);
            // 
            // btnSyncFilter
            // 
            this.btnSyncFilter.Location = new System.Drawing.Point(18, 212);
            this.btnSyncFilter.Margin = new System.Windows.Forms.Padding(14, 6, 0, 6);
            this.btnSyncFilter.Name = "btnSyncFilter";
            this.btnSyncFilter.Size = new System.Drawing.Size(112, 34);
            this.btnSyncFilter.TabIndex = 9;
            this.btnSyncFilter.Text = "同步过滤";
            this.btnSyncFilter.UseVisualStyleBackColor = true;
            this.btnSyncFilter.Click += new System.EventHandler(this.btnSyncFilter_Click);
            // 
            // BtnAutoAh
            // 
            this.BtnAutoAh.Location = new System.Drawing.Point(18, 120);
            this.BtnAutoAh.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BtnAutoAh.Name = "BtnAutoAh";
            this.BtnAutoAh.Size = new System.Drawing.Size(112, 34);
            this.BtnAutoAh.TabIndex = 0;
            this.BtnAutoAh.Text = "开始扫拍";
            this.BtnAutoAh.UseMnemonic = false;
            this.BtnAutoAh.UseVisualStyleBackColor = true;
            this.BtnAutoAh.Click += new System.EventHandler(this.BtnAutoAh_Click);
            // 
            // BtnTODO2
            // 
            this.BtnTODO2.Location = new System.Drawing.Point(20, 256);
            this.BtnTODO2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.BtnTODO2.Name = "BtnTODO2";
            this.BtnTODO2.Size = new System.Drawing.Size(112, 34);
            this.BtnTODO2.TabIndex = 5;
            this.BtnTODO2.Text = "并行清盘修";
            this.BtnTODO2.UseVisualStyleBackColor = true;
            this.BtnTODO2.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // btnMonitor
            // 
            this.btnMonitor.Location = new System.Drawing.Point(18, 662);
            this.btnMonitor.Margin = new System.Windows.Forms.Padding(18, 8, 0, 8);
            this.btnMonitor.Name = "btnMonitor";
            this.btnMonitor.Size = new System.Drawing.Size(112, 38);
            this.btnMonitor.TabIndex = 6;
            this.btnMonitor.Text = "效率监控";
            this.btnMonitor.UseVisualStyleBackColor = true;
            this.btnMonitor.Click += new System.EventHandler(this.btnMonitor_Click);
            // 
            // BtnClear
            // 
            this.BtnClear.Location = new System.Drawing.Point(153, 662);
            this.BtnClear.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BtnClear.Name = "BtnClear";
            this.BtnClear.Size = new System.Drawing.Size(112, 38);
            this.BtnClear.TabIndex = 3;
            this.BtnClear.Text = "关闭当前页";
            this.BtnClear.UseVisualStyleBackColor = true;
            this.BtnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // BtnAutoRune
            // 
            this.BtnAutoRune.Location = new System.Drawing.Point(20, 294);
            this.BtnAutoRune.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.BtnAutoRune.Name = "BtnAutoRune";
            this.BtnAutoRune.Size = new System.Drawing.Size(112, 34);
            this.BtnAutoRune.TabIndex = 0;
            this.BtnAutoRune.Text = "一键合符文";
            this.BtnAutoRune.UseVisualStyleBackColor = true;
            this.BtnAutoRune.Click += new System.EventHandler(this.BtnAutoRune_Click);
            // 
            // BtnInventory
            // 
            this.BtnInventory.Location = new System.Drawing.Point(0, 0);
            this.BtnInventory.Name = "BtnInventory";
            this.BtnInventory.Size = new System.Drawing.Size(75, 23);
            this.BtnInventory.TabIndex = 0;
            // 
            // RuneGroup
            // 
            this.RuneGroup.Location = new System.Drawing.Point(0, 0);
            this.RuneGroup.Name = "RuneGroup";
            this.RuneGroup.Size = new System.Drawing.Size(200, 100);
            this.RuneGroup.TabIndex = 0;
            this.RuneGroup.TabStop = false;
            // 
            // AhGroup
            // 
            this.AhGroup.Location = new System.Drawing.Point(0, 30);
            this.AhGroup.Margin = new System.Windows.Forms.Padding(0);
            this.AhGroup.Name = "AhGroup";
            this.AhGroup.Padding = new System.Windows.Forms.Padding(0);
            this.AhGroup.Size = new System.Drawing.Size(200, 473);
            this.AhGroup.TabIndex = 6;
            this.AhGroup.TabStop = false;
            this.AhGroup.Text = "拍卖行菜单";
            // 
            // RoleGroup
            // 
            this.RoleGroup.Location = new System.Drawing.Point(0, 30);
            this.RoleGroup.Margin = new System.Windows.Forms.Padding(0);
            this.RoleGroup.Name = "RoleGroup";
            this.RoleGroup.Padding = new System.Windows.Forms.Padding(0);
            this.RoleGroup.Size = new System.Drawing.Size(200, 473);
            this.RoleGroup.TabIndex = 5;
            this.RoleGroup.TabStop = false;
            this.RoleGroup.Text = "角色菜单";
            // 
            // LoginGroup
            // 
            this.LoginGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.LoginGroup.Controls.Add(this.label1);
            this.LoginGroup.Controls.Add(this.CurLoginAccount);
            this.LoginGroup.Controls.Add(this.BtnLogin);
            this.LoginGroup.Location = new System.Drawing.Point(0, 27);
            this.LoginGroup.Name = "LoginGroup";
            this.LoginGroup.Size = new System.Drawing.Size(200, 473);
            this.LoginGroup.TabIndex = 4;
            this.LoginGroup.TabStop = false;
            this.LoginGroup.Text = "登录菜单";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.Location = new System.Drawing.Point(50, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "当前登录账号";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CurLoginAccount
            // 
            this.CurLoginAccount.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.CurLoginAccount.Location = new System.Drawing.Point(50, 42);
            this.CurLoginAccount.Name = "CurLoginAccount";
            this.CurLoginAccount.Size = new System.Drawing.Size(100, 23);
            this.CurLoginAccount.TabIndex = 1;
            this.CurLoginAccount.Text = "当前无账号登录";
            this.CurLoginAccount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BtnLogin
            // 
            this.BtnLogin.Location = new System.Drawing.Point(61, 98);
            this.BtnLogin.Name = "BtnLogin";
            this.BtnLogin.Size = new System.Drawing.Size(80, 25);
            this.BtnLogin.TabIndex = 3;
            this.BtnLogin.Text = "登录";
            this.BtnLogin.UseVisualStyleBackColor = true;
            // 
            // MenuWidget
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.Controls.Add(this.menuPanel);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MenuWidget";
            this.Size = new System.Drawing.Size(300, 754);
            this.Load += new System.EventHandler(this.MenuWidget_Load);
            this.menuPanel.ResumeLayout(false);
            this.HomeGroup.ResumeLayout(false);
            this.LoginGroup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel menuPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label CurLoginAccount;
        private System.Windows.Forms.ComboBox AccountCombo;
        private System.Windows.Forms.Button BtnLogin;
        private System.Windows.Forms.GroupBox LoginGroup;
        private System.Windows.Forms.GroupBox HomeGroup;
        private System.Windows.Forms.GroupBox RuneGroup;
        private System.Windows.Forms.Button BtnAutoEquip;
        private System.Windows.Forms.Button BtnClean;
        private System.Windows.Forms.Button BtnAutoRune;
        private System.Windows.Forms.Button btnHomePage;
        private System.Windows.Forms.Button BtnSkillPoint;
        private System.Windows.Forms.Button btnSyncFilter;
        private System.Windows.Forms.GroupBox RoleGroup;
        private System.Windows.Forms.GroupBox AhGroup;
        private System.Windows.Forms.Button BtnAutoAh;
        private System.Windows.Forms.Button BtnInit;
        private System.Windows.Forms.Button BtnClear;
        private System.Windows.Forms.Button btnMap;
        private System.Windows.Forms.Button BtnTODO2;
        private System.Windows.Forms.Button btnMonitor;

        private System.Windows.Forms.Button BtnTest;
        private System.Windows.Forms.Button btnReform;
        private System.Windows.Forms.Button BtnInventory;
        private System.Windows.Forms.Button Button_Inventory;
        private System.Windows.Forms.Button btnDungeon;
        private System.Windows.Forms.Button btnProxy;
        private System.Windows.Forms.Button btnSendRune;
        private System.Windows.Forms.Button btnDealTrade;
        private System.Windows.Forms.Button btnSendEquip;
        private System.Windows.Forms.Button btnAuction;
        private System.Windows.Forms.Button btnRollArtifact;
        private System.Windows.Forms.Button btnNec;
        private System.Windows.Forms.Button btnCookie;
        private System.Windows.Forms.Button btnGem;
        private System.Windows.Forms.Button btnMf;
        private System.Windows.Forms.Button btnConfirmDel;
        private System.Windows.Forms.Button btnPreDel;
        private System.Windows.Forms.Button btnSanBoss;
        private System.Windows.Forms.Button btnRecovery;
        private System.Windows.Forms.Button btnHunterSkill;
        private System.Windows.Forms.Button btnShengyi;
    }
}
