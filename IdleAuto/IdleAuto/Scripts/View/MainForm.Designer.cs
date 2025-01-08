
partial class MainForm
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

    #region Windows 窗体设计器生成的代码

    /// <summary>
    /// 设计器支持所需的方法 - 不要修改
    /// 使用代码编辑器修改此方法的内容。
    /// </summary>
    private void InitializeComponent()
    {
            this.menuPanel = new System.Windows.Forms.Panel();
            this.AccountCombo = new System.Windows.Forms.ComboBox();
            this.RoleGroup = new System.Windows.Forms.GroupBox();
            this.JumpGroup = new System.Windows.Forms.GroupBox();
            this.LayoutRoot = new System.Windows.Forms.FlowLayoutPanel();
            this.LableRoleTitle = new System.Windows.Forms.Label();
            this.RoleCombo = new System.Windows.Forms.ComboBox();
            this.BtnHome = new System.Windows.Forms.Button();
            this.BtnRank = new System.Windows.Forms.Button();
            this.BtnMaterial = new System.Windows.Forms.Button();
            this.RuneGroup = new System.Windows.Forms.GroupBox();
            this.BtnAutoRune = new System.Windows.Forms.Button();
            this.LoginGroup = new System.Windows.Forms.GroupBox();
            this.BtnLogin = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.CurLoginAccount = new System.Windows.Forms.Label();
            this.HomeGroup = new System.Windows.Forms.GroupBox();
            this.BtnAutoEquip = new System.Windows.Forms.Button();
            this.BtnAutoOnline = new System.Windows.Forms.Button();
             this.BtnScanAh = new System.Windows.Forms.Button();
            this.browserPanel = new System.Windows.Forms.Panel();
            this.LoadingPanel = new System.Windows.Forms.Panel();
            this.LoadingContent = new System.Windows.Forms.Label();
            this.LoadingBg = new System.Windows.Forms.PictureBox();
            this.menuPanel.SuspendLayout();
            this.JumpGroup.SuspendLayout();
            this.LayoutRoot.SuspendLayout();
            this.RuneGroup.SuspendLayout();
            this.LoginGroup.SuspendLayout();
            this.HomeGroup.SuspendLayout();
            this.LoadingPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LoadingBg)).BeginInit();
            this.SuspendLayout();
            // 
            // menuPanel
            // 
            this.menuPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.menuPanel.AutoScroll = true;
            this.menuPanel.Controls.Add(this.AccountCombo);
            this.menuPanel.Controls.Add(this.RoleGroup);
            this.menuPanel.Controls.Add(this.JumpGroup);
            this.menuPanel.Location = new System.Drawing.Point(0, 0);
            this.menuPanel.Margin = new System.Windows.Forms.Padding(0);
            this.menuPanel.Name = "menuPanel";
            this.menuPanel.Size = new System.Drawing.Size(100, 502);
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
            this.AccountCombo.Size = new System.Drawing.Size(100, 24);
            this.AccountCombo.TabIndex = 2;
            this.AccountCombo.SelectedIndexChanged += new System.EventHandler(this.AccountCombo_SelectedIndexChanged);
            // 
            // RoleGroup
            // 
            this.RoleGroup.Location = new System.Drawing.Point(0, 40);
            this.RoleGroup.Margin = new System.Windows.Forms.Padding(0);
            this.RoleGroup.Name = "RoleGroup";
            this.RoleGroup.Padding = new System.Windows.Forms.Padding(0);
            this.RoleGroup.Size = new System.Drawing.Size(100, 226);
            this.RoleGroup.TabIndex = 5;
            this.RoleGroup.TabStop = false;
            this.RoleGroup.Text = "角色菜单";
            // 
            // JumpGroup
            // 
            this.JumpGroup.Controls.Add(this.LayoutRoot);
            this.JumpGroup.Location = new System.Drawing.Point(0, 270);
            this.JumpGroup.Margin = new System.Windows.Forms.Padding(0);
            this.JumpGroup.Name = "JumpGroup";
            this.JumpGroup.Padding = new System.Windows.Forms.Padding(0);
            this.JumpGroup.Size = new System.Drawing.Size(100, 226);
            this.JumpGroup.TabIndex = 3;
            this.JumpGroup.TabStop = false;
            this.JumpGroup.Text = "快捷跳转";
            // 
            // LayoutRoot
            // 
            this.LayoutRoot.Controls.Add(this.LableRoleTitle);
            this.LayoutRoot.Controls.Add(this.RoleCombo);
            this.LayoutRoot.Controls.Add(this.BtnHome);
            this.LayoutRoot.Controls.Add(this.BtnRank);
            this.LayoutRoot.Controls.Add(this.BtnMaterial);
            this.LayoutRoot.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.LayoutRoot.Location = new System.Drawing.Point(0, 20);
            this.LayoutRoot.Margin = new System.Windows.Forms.Padding(0);
            this.LayoutRoot.Name = "LayoutRoot";
            this.LayoutRoot.Size = new System.Drawing.Size(100, 210);
            this.LayoutRoot.TabIndex = 8;
            // 
            // LableRoleTitle
            // 
            this.LableRoleTitle.Location = new System.Drawing.Point(0, 0);
            this.LableRoleTitle.Margin = new System.Windows.Forms.Padding(0);
            this.LableRoleTitle.Name = "LableRoleTitle";
            this.LableRoleTitle.Size = new System.Drawing.Size(100, 20);
            this.LableRoleTitle.TabIndex = 7;
            this.LableRoleTitle.Text = "切换角色";
            this.LableRoleTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // RoleCombo
            // 
            this.RoleCombo.DisplayMember = "RoleName";
            this.RoleCombo.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RoleCombo.FormattingEnabled = true;
            this.RoleCombo.Location = new System.Drawing.Point(0, 20);
            this.RoleCombo.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.RoleCombo.Name = "RoleCombo";
            this.RoleCombo.Size = new System.Drawing.Size(100, 24);
            this.RoleCombo.TabIndex = 6;
            this.RoleCombo.SelectedIndexChanged += new System.EventHandler(this.RoleCombo_SelectedIndexChanged);
            // 
            // BtnHome
            // 
            this.BtnHome.Location = new System.Drawing.Point(12, 54);
            this.BtnHome.Margin = new System.Windows.Forms.Padding(12, 5, 0, 5);
            this.BtnHome.Name = "BtnHome";
            this.BtnHome.Size = new System.Drawing.Size(75, 23);
            this.BtnHome.TabIndex = 8;
            this.BtnHome.Text = "主页";
            this.BtnHome.UseVisualStyleBackColor = true;
            this.BtnHome.Click += new System.EventHandler(this.BtnHome_Click);
            // 
            // BtnRank
            // 
            this.BtnRank.Location = new System.Drawing.Point(12, 87);
            this.BtnRank.Margin = new System.Windows.Forms.Padding(12, 5, 0, 5);
            this.BtnRank.Name = "BtnRank";
            this.BtnRank.Size = new System.Drawing.Size(75, 23);
            this.BtnRank.TabIndex = 4;
            this.BtnRank.Text = "赛季排行";
            this.BtnRank.UseVisualStyleBackColor = true;
            this.BtnRank.Click += new System.EventHandler(this.BtnRank_Click);
            // 
            // BtnMaterial
            // 
            this.BtnMaterial.Location = new System.Drawing.Point(12, 120);
            this.BtnMaterial.Margin = new System.Windows.Forms.Padding(12, 5, 0, 5);
            this.BtnMaterial.Name = "BtnMaterial";
            this.BtnMaterial.Size = new System.Drawing.Size(75, 23);
            this.BtnMaterial.TabIndex = 9;
            this.BtnMaterial.Text = " 材料页面";
            this.BtnMaterial.UseVisualStyleBackColor = true;
            this.BtnMaterial.Click += new System.EventHandler(this.BtnMaterial_Click);
            // 
            // RuneGroup
            // 
            this.RuneGroup.Controls.Add(this.BtnAutoRune);
            this.RuneGroup.Location = new System.Drawing.Point(0, 40);
            this.RuneGroup.Margin = new System.Windows.Forms.Padding(0);
            this.RuneGroup.Name = "RuneGroup";
            this.RuneGroup.Padding = new System.Windows.Forms.Padding(0);
            this.RuneGroup.Size = new System.Drawing.Size(100, 226);
            this.RuneGroup.TabIndex = 4;
            this.RuneGroup.TabStop = false;
            this.RuneGroup.Text = "材料菜单";
            // 
            // BtnAutoRune
            // 
            this.BtnAutoRune.Location = new System.Drawing.Point(12, 20);
            this.BtnAutoRune.Name = "BtnAutoRune";
            this.BtnAutoRune.Size = new System.Drawing.Size(75, 23);
            this.BtnAutoRune.TabIndex = 0;
            this.BtnAutoRune.Text = "一键合符文";
            this.BtnAutoRune.UseVisualStyleBackColor = true;
            this.BtnAutoRune.Click += new System.EventHandler(this.BtnAutoRune_Click);
            // 
            // LoginGroup
            // 
            this.LoginGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.LoginGroup.Controls.Add(this.BtnLogin);
            this.LoginGroup.Controls.Add(this.label1);
            this.LoginGroup.Controls.Add(this.CurLoginAccount);
            this.LoginGroup.Location = new System.Drawing.Point(0, 40);
            this.LoginGroup.Name = "LoginGroup";
            this.LoginGroup.Size = new System.Drawing.Size(100, 460);
            this.LoginGroup.TabIndex = 4;
            this.LoginGroup.TabStop = false;
            this.LoginGroup.Text = "登录菜单";
            // 
            // BtnLogin
            // 
            this.BtnLogin.Location = new System.Drawing.Point(11, 107);
            this.BtnLogin.Name = "BtnLogin";
            this.BtnLogin.Size = new System.Drawing.Size(80, 25);
            this.BtnLogin.TabIndex = 3;
            this.BtnLogin.Text = "登录";
            this.BtnLogin.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.Location = new System.Drawing.Point(0, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "当前登录账号";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CurLoginAccount
            // 
            this.CurLoginAccount.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.CurLoginAccount.Location = new System.Drawing.Point(0, 42);
            this.CurLoginAccount.Name = "CurLoginAccount";
            this.CurLoginAccount.Size = new System.Drawing.Size(100, 23);
            this.CurLoginAccount.TabIndex = 1;
            this.CurLoginAccount.Text = "当前无账号登录";
            this.CurLoginAccount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // HomeGroup
            // 
            this.HomeGroup.Controls.Add(this.BtnAutoEquip);
            this.HomeGroup.Controls.Add(this.BtnAutoOnline);
            this.HomeGroup.Location = new System.Drawing.Point(0, 40);
            this.HomeGroup.Name = "HomeGroup";
            this.HomeGroup.Size = new System.Drawing.Size(100, 230);
            this.HomeGroup.TabIndex = 4;
            this.HomeGroup.TabStop = false;
            this.HomeGroup.Text = "主页菜单";
            // 
            // BtnAutoEquip
            // 
            this.BtnAutoEquip.Location = new System.Drawing.Point(12, 49);
            this.BtnAutoEquip.Name = "BtnAutoEquip";
            this.BtnAutoEquip.Size = new System.Drawing.Size(75, 23);
            this.BtnAutoEquip.TabIndex = 1;
            this.BtnAutoEquip.Text = "一键修车";
            this.BtnAutoEquip.UseVisualStyleBackColor = true;
            // 
            // BtnAutoOnline
            // 
            this.BtnAutoOnline.Location = new System.Drawing.Point(12, 20);
            this.BtnAutoOnline.Name = "BtnAutoOnline";
            this.BtnAutoOnline.Size = new System.Drawing.Size(75, 23);
            this.BtnAutoOnline.TabIndex = 0;
            this.BtnAutoOnline.Text = "一键点亮";
            this.BtnAutoOnline.UseVisualStyleBackColor = true;
            // 
            // browserPanel
            // 
            this.browserPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.browserPanel.Location = new System.Drawing.Point(100, 0);
            this.browserPanel.Margin = new System.Windows.Forms.Padding(4);
            this.browserPanel.Name = "browserPanel";
            this.browserPanel.Size = new System.Drawing.Size(887, 502);
            this.browserPanel.TabIndex = 1;
            // 
            // LoadingPanel
            // 
            this.LoadingPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadingPanel.Controls.Add(this.LoadingContent);
            this.LoadingPanel.Controls.Add(this.LoadingBg);
            this.LoadingPanel.Location = new System.Drawing.Point(0, 0);
            this.LoadingPanel.Name = "LoadingPanel";
            this.LoadingPanel.Size = new System.Drawing.Size(985, 502);
            this.LoadingPanel.TabIndex = 0;
            // 
            // LoadingContent
            // 
            this.LoadingContent.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.LoadingContent.BackColor = System.Drawing.SystemColors.ControlDark;
            this.LoadingContent.Location = new System.Drawing.Point(0, 0);
            this.LoadingContent.Name = "LoadingContent";
            this.LoadingContent.Size = new System.Drawing.Size(985, 502);
            this.LoadingContent.TabIndex = 3;
            this.LoadingContent.Text = "Loading";
            this.LoadingContent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LoadingBg
            // 
            this.LoadingBg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadingBg.BackColor = System.Drawing.SystemColors.ControlDark;
            this.LoadingBg.Location = new System.Drawing.Point(0, 0);
            this.LoadingBg.Name = "LoadingBg";
            this.LoadingBg.Size = new System.Drawing.Size(985, 502);
            this.LoadingBg.TabIndex = 2;
            this.LoadingBg.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(985, 503);
            this.Controls.Add(this.LoadingPanel);
            this.Controls.Add(this.menuPanel);
            this.Controls.Add(this.browserPanel);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuPanel.ResumeLayout(false);
            this.JumpGroup.ResumeLayout(false);
            this.LayoutRoot.ResumeLayout(false);
            this.RuneGroup.ResumeLayout(false);
            this.LoginGroup.ResumeLayout(false);
            this.HomeGroup.ResumeLayout(false);
            this.LoadingPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.LoadingBg)).EndInit();
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel menuPanel;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Panel browserPanel;
    private System.Windows.Forms.Label CurLoginAccount;
    private System.Windows.Forms.ComboBox AccountCombo;

    private System.Windows.Forms.Button BtnLogin;
    private System.Windows.Forms.GroupBox LoginGroup;
    private System.Windows.Forms.GroupBox HomeGroup;
    private System.Windows.Forms.GroupBox RuneGroup;
    private System.Windows.Forms.Button BtnAutoEquip;
    private System.Windows.Forms.Button BtnAutoOnline;
    private System.Windows.Forms.Button BtnAutoRune;
    private System.Windows.Forms.Panel LoadingPanel;
    private System.Windows.Forms.PictureBox LoadingBg;
    private System.Windows.Forms.Label LoadingContent;
    private System.Windows.Forms.GroupBox JumpGroup;
    private System.Windows.Forms.Button BtnRank;
    private System.Windows.Forms.ComboBox RoleCombo;
    private System.Windows.Forms.Label LableRoleTitle;
    private System.Windows.Forms.FlowLayoutPanel LayoutRoot;
    private System.Windows.Forms.Button BtnHome;
    private System.Windows.Forms.Button BtnMaterial;
    private System.Windows.Forms.GroupBox RoleGroup;
    private System.Windows.Forms.Button BtnScanAh;
}