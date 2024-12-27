
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
        this.RuneGroup = new System.Windows.Forms.GroupBox();
        this.HomeGroup = new System.Windows.Forms.GroupBox();
        this.BtnAutoEquip = new System.Windows.Forms.Button();
        this.BtnAutoOnline = new System.Windows.Forms.Button();
        this.LoginGroup = new System.Windows.Forms.GroupBox();
        this.BtnLogin = new System.Windows.Forms.Button();
        this.label1 = new System.Windows.Forms.Label();
        this.CurLoginAccount = new System.Windows.Forms.Label();
        this.browserPanel = new System.Windows.Forms.Panel();
        this.BtnAutoRune = new System.Windows.Forms.Button();
        this.menuPanel.SuspendLayout();
        this.RuneGroup.SuspendLayout();
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
        this.menuPanel.Controls.Add(this.RuneGroup);
        this.menuPanel.Location = new System.Drawing.Point(0, 0);
        this.menuPanel.Margin = new System.Windows.Forms.Padding(4);
        this.menuPanel.Name = "menuPanel";
        this.menuPanel.Size = new System.Drawing.Size(100, 600);
        this.menuPanel.TabIndex = 0;
        // 
        // AccountCombo
        // 
        this.AccountCombo.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.AccountCombo.FormattingEnabled = true;
        this.AccountCombo.Location = new System.Drawing.Point(0, 0);
        this.AccountCombo.Margin = new System.Windows.Forms.Padding(4);
        this.AccountCombo.Name = "AccountCombo";
        this.AccountCombo.Size = new System.Drawing.Size(100, 29);
        this.AccountCombo.TabIndex = 2;
        this.AccountCombo.SelectedIndexChanged += new System.EventHandler(this.AccountCombo_SelectedIndexChanged);
        // 
        // RuneGroup
        // 
        this.RuneGroup.Controls.Add(this.BtnAutoRune);
        this.RuneGroup.Location = new System.Drawing.Point(0, 40);
        this.RuneGroup.Name = "RuneGroup";
        this.RuneGroup.Size = new System.Drawing.Size(100, 560);
        this.RuneGroup.TabIndex = 4;
        this.RuneGroup.TabStop = false;
        this.RuneGroup.Text = "RuneMenu";
        // 
        // HomeGroup
        // 
        this.HomeGroup.Controls.Add(this.BtnAutoEquip);
        this.HomeGroup.Controls.Add(this.BtnAutoOnline);
        this.HomeGroup.Location = new System.Drawing.Point(0, 40);
        this.HomeGroup.Name = "HomeGroup";
        this.HomeGroup.Size = new System.Drawing.Size(100, 560);
        this.HomeGroup.TabIndex = 4;
        this.HomeGroup.TabStop = false;
        this.HomeGroup.Text = "HomeMenu";
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
        // LoginGroup
        // 
        this.LoginGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)));
        this.LoginGroup.Controls.Add(this.BtnLogin);
        this.LoginGroup.Controls.Add(this.label1);
        this.LoginGroup.Controls.Add(this.CurLoginAccount);
        this.LoginGroup.Location = new System.Drawing.Point(0, 40);
        this.LoginGroup.Name = "LoginGroup";
        this.LoginGroup.Size = new System.Drawing.Size(100, 560);
        this.LoginGroup.TabIndex = 4;
        this.LoginGroup.TabStop = false;
        this.LoginGroup.Text = "LoginMenu";
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
        // browserPanel
        // 
        this.browserPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.browserPanel.Location = new System.Drawing.Point(100, 0);
        this.browserPanel.Margin = new System.Windows.Forms.Padding(4);
        this.browserPanel.Name = "browserPanel";
        this.browserPanel.Size = new System.Drawing.Size(846, 600);
        this.browserPanel.TabIndex = 1;
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
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ClientSize = new System.Drawing.Size(944, 601);
        this.Controls.Add(this.browserPanel);
        this.Controls.Add(this.menuPanel);
        this.Margin = new System.Windows.Forms.Padding(2);
        this.Name = "MainForm";
        this.Text = "Form1";
        this.Load += new System.EventHandler(this.MainForm_Load);
        this.menuPanel.ResumeLayout(false);
        this.RuneGroup.ResumeLayout(false);
        this.HomeGroup.ResumeLayout(false);
        this.LoginGroup.ResumeLayout(false);
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
}