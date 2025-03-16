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
            this.Button_Inventory = new System.Windows.Forms.Button();
            this.btnTestArtifact = new System.Windows.Forms.Button();
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
            this.BtnInventory = new System.Windows.Forms.Button();
            this.RuneGroup = new System.Windows.Forms.GroupBox();
            this.BtnAutoRune = new System.Windows.Forms.Button();
            this.AhGroup = new System.Windows.Forms.GroupBox();
            this.RoleGroup = new System.Windows.Forms.GroupBox();
            this.LoginGroup = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.CurLoginAccount = new System.Windows.Forms.Label();
            this.BtnLogin = new System.Windows.Forms.Button();
            this.menuPanel.SuspendLayout();
            this.HomeGroup.SuspendLayout();
            this.RuneGroup.SuspendLayout();
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
            this.menuPanel.Size = new System.Drawing.Size(267, 629);
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
            this.AccountCombo.Size = new System.Drawing.Size(265, 28);
            this.AccountCombo.TabIndex = 2;
            this.AccountCombo.SelectedIndexChanged += new System.EventHandler(this.AccountCombo_SelectedIndexChanged);
            // 
            // HomeGroup
            // 
            this.HomeGroup.Controls.Add(this.Button_Inventory);
            this.HomeGroup.Controls.Add(this.btnTestArtifact);
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
            this.HomeGroup.Location = new System.Drawing.Point(0, 38);
            this.HomeGroup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.HomeGroup.Name = "HomeGroup";
            this.HomeGroup.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.HomeGroup.Size = new System.Drawing.Size(267, 591);
            this.HomeGroup.TabIndex = 4;
            this.HomeGroup.TabStop = false;
            this.HomeGroup.Text = "主页菜单";
            // 
            // Button_Inventory
            // 
            this.Button_Inventory.Location = new System.Drawing.Point(123, 65);
            this.Button_Inventory.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Button_Inventory.Name = "Button_Inventory";
            this.Button_Inventory.Size = new System.Drawing.Size(100, 30);
            this.Button_Inventory.TabIndex = 13;
            this.Button_Inventory.Text = "一键删库";
            this.Button_Inventory.UseVisualStyleBackColor = true;
            this.Button_Inventory.Click += new System.EventHandler(this.Button_Inventory_Click);
            // 
            // btnTestArtifact
            // 
            this.btnTestArtifact.Location = new System.Drawing.Point(16, 139);
            this.btnTestArtifact.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnTestArtifact.Name = "btnTestArtifact";
            this.btnTestArtifact.Size = new System.Drawing.Size(100, 29);
            this.btnTestArtifact.TabIndex = 12;
            this.btnTestArtifact.Text = "神器测试";
            this.btnTestArtifact.UseMnemonic = false;
            this.btnTestArtifact.UseVisualStyleBackColor = true;
            this.btnTestArtifact.Click += new System.EventHandler(this.btnTestArtifact_Click);
            // 
            // BtnTest
            // 
            this.BtnTest.Location = new System.Drawing.Point(123, 365);
            this.BtnTest.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.BtnTest.Name = "BtnTest";
            this.BtnTest.Size = new System.Drawing.Size(100, 30);
            this.BtnTest.TabIndex = 11;
            this.BtnTest.Text = "测试";
            this.BtnTest.UseVisualStyleBackColor = true;
            this.BtnTest.Click += new System.EventHandler(this.BtnTest_Click);
            // 
            // btnHomePage
            // 
            this.btnHomePage.Location = new System.Drawing.Point(16, 28);
            this.btnHomePage.Margin = new System.Windows.Forms.Padding(16, 6, 0, 6);
            this.btnHomePage.Name = "btnHomePage";
            this.btnHomePage.Size = new System.Drawing.Size(100, 29);
            this.btnHomePage.TabIndex = 4;
            this.btnHomePage.Text = "载入账号";
            this.btnHomePage.UseVisualStyleBackColor = true;
            this.btnHomePage.Click += new System.EventHandler(this.btnHomePage_Click);
            // 
            // BtnInit
            // 
            this.BtnInit.Location = new System.Drawing.Point(16, 64);
            this.BtnInit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BtnInit.Name = "BtnInit";
            this.BtnInit.Size = new System.Drawing.Size(100, 29);
            this.BtnInit.TabIndex = 2;
            this.BtnInit.Text = "账号初始化";
            this.BtnInit.UseVisualStyleBackColor = true;
            this.BtnInit.Click += new System.EventHandler(this.BtnInit_Click);
            // 
            // BtnClean
            // 
            this.BtnClean.Location = new System.Drawing.Point(123, 28);
            this.BtnClean.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.BtnClean.Name = "BtnClean";
            this.BtnClean.Size = new System.Drawing.Size(100, 30);
            this.BtnClean.TabIndex = 0;
            this.BtnClean.Text = "收菜盘库";
            this.BtnClean.UseVisualStyleBackColor = true;
            this.BtnClean.Click += new System.EventHandler(this.BtnClean_Click);
            // 
            // BtnAutoEquip
            // 
            this.BtnAutoEquip.Location = new System.Drawing.Point(123, 100);
            this.BtnAutoEquip.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.BtnAutoEquip.Name = "BtnAutoEquip";
            this.BtnAutoEquip.Size = new System.Drawing.Size(100, 29);
            this.BtnAutoEquip.TabIndex = 1;
            this.BtnAutoEquip.Text = "一键修车";
            this.BtnAutoEquip.UseVisualStyleBackColor = true;
            this.BtnAutoEquip.Click += new System.EventHandler(this.BtnAutoEquip_Click);
            // 
            // BtnSkillPoint
            // 
            this.BtnSkillPoint.Location = new System.Drawing.Point(123, 139);
            this.BtnSkillPoint.Margin = new System.Windows.Forms.Padding(12, 5, 0, 5);
            this.BtnSkillPoint.Name = "BtnSkillPoint";
            this.BtnSkillPoint.Size = new System.Drawing.Size(100, 29);
            this.BtnSkillPoint.TabIndex = 8;
            this.BtnSkillPoint.Text = "加点";
            this.BtnSkillPoint.UseVisualStyleBackColor = true;
            this.BtnSkillPoint.Click += new System.EventHandler(this.BtnSkillPoint_Click);
            // 
            // btnMap
            // 
            this.btnMap.Location = new System.Drawing.Point(123, 178);
            this.btnMap.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnMap.Name = "btnMap";
            this.btnMap.Size = new System.Drawing.Size(100, 30);
            this.btnMap.TabIndex = 4;
            this.btnMap.Text = "切图测试";
            this.btnMap.UseVisualStyleBackColor = true;
            this.btnMap.Click += new System.EventHandler(this.btnMap_Click);
            // 
            // btnSyncFilter
            // 
            this.btnSyncFilter.Location = new System.Drawing.Point(16, 178);
            this.btnSyncFilter.Margin = new System.Windows.Forms.Padding(12, 5, 0, 5);
            this.btnSyncFilter.Name = "btnSyncFilter";
            this.btnSyncFilter.Size = new System.Drawing.Size(100, 29);
            this.btnSyncFilter.TabIndex = 9;
            this.btnSyncFilter.Text = "同步过滤";
            this.btnSyncFilter.UseVisualStyleBackColor = true;
            this.btnSyncFilter.Click += new System.EventHandler(this.btnSyncFilter_Click);
            // 
            // BtnAutoAh
            // 
            this.BtnAutoAh.Location = new System.Drawing.Point(16, 100);
            this.BtnAutoAh.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BtnAutoAh.Name = "BtnAutoAh";
            this.BtnAutoAh.Size = new System.Drawing.Size(100, 29);
            this.BtnAutoAh.TabIndex = 0;
            this.BtnAutoAh.Text = "开始扫拍";
            this.BtnAutoAh.UseMnemonic = false;
            this.BtnAutoAh.UseVisualStyleBackColor = true;
            this.BtnAutoAh.Click += new System.EventHandler(this.BtnAutoAh_Click);
            // 
            // BtnTODO2
            // 
            this.BtnTODO2.Location = new System.Drawing.Point(16, 365);
            this.BtnTODO2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.BtnTODO2.Name = "BtnTODO2";
            this.BtnTODO2.Size = new System.Drawing.Size(100, 29);
            this.BtnTODO2.TabIndex = 5;
            this.BtnTODO2.Text = "待用";
            this.BtnTODO2.UseVisualStyleBackColor = true;
            this.BtnTODO2.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // btnMonitor
            // 
            this.btnMonitor.Location = new System.Drawing.Point(16, 552);
            this.btnMonitor.Margin = new System.Windows.Forms.Padding(16, 6, 0, 6);
            this.btnMonitor.Name = "btnMonitor";
            this.btnMonitor.Size = new System.Drawing.Size(100, 32);
            this.btnMonitor.TabIndex = 6;
            this.btnMonitor.Text = "效率监控";
            this.btnMonitor.UseVisualStyleBackColor = true;
            this.btnMonitor.Click += new System.EventHandler(this.btnMonitor_Click);
            // 
            // BtnClear
            // 
            this.BtnClear.Location = new System.Drawing.Point(136, 555);
            this.BtnClear.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BtnClear.Name = "BtnClear";
            this.BtnClear.Size = new System.Drawing.Size(100, 29);
            this.BtnClear.TabIndex = 3;
            this.BtnClear.Text = "关闭当前页";
            this.BtnClear.UseVisualStyleBackColor = true;
            this.BtnClear.Click += new System.EventHandler(this.BtnClear_Click);
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
            this.RuneGroup.Controls.Add(this.BtnAutoRune);
            this.RuneGroup.Location = new System.Drawing.Point(0, 30);
            this.RuneGroup.Margin = new System.Windows.Forms.Padding(0);
            this.RuneGroup.Name = "RuneGroup";
            this.RuneGroup.Padding = new System.Windows.Forms.Padding(0);
            this.RuneGroup.Size = new System.Drawing.Size(200, 473);
            this.RuneGroup.TabIndex = 4;
            this.RuneGroup.TabStop = false;
            this.RuneGroup.Text = "材料菜单";
            // 
            // BtnAutoRune
            // 
            this.BtnAutoRune.Location = new System.Drawing.Point(49, 34);
            this.BtnAutoRune.Name = "BtnAutoRune";
            this.BtnAutoRune.Size = new System.Drawing.Size(75, 23);
            this.BtnAutoRune.TabIndex = 0;
            this.BtnAutoRune.Text = "一键合符文";
            this.BtnAutoRune.UseVisualStyleBackColor = true;
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.Controls.Add(this.menuPanel);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MenuWidget";
            this.Size = new System.Drawing.Size(267, 629);
            this.Load += new System.EventHandler(this.MenuWidget_Load);
            this.menuPanel.ResumeLayout(false);
            this.HomeGroup.ResumeLayout(false);
            this.RuneGroup.ResumeLayout(false);
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
        private System.Windows.Forms.Button btnTestArtifact;
        private System.Windows.Forms.Button BtnInventory;
        private System.Windows.Forms.Button Button_Inventory;
    }
}
