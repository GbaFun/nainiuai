
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
        this.BroTabControl = new System.Windows.Forms.TabControl();
        this.SuspendLayout();
        // 
        // BroTabControl
        // 
        this.BroTabControl.Location = new System.Drawing.Point(100, 0);
        this.BroTabControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        this.BroTabControl.Name = "BroTabControl";
        this.BroTabControl.SelectedIndex = 0;
        this.BroTabControl.Size = new System.Drawing.Size(883, 502);
        this.BroTabControl.TabIndex = 0;
        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.BackColor = System.Drawing.SystemColors.ControlLight;
        this.ClientSize = new System.Drawing.Size(985, 503);
        this.Controls.Add(this.BroTabControl);
        this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
        this.Name = "MainForm";
        this.Text = "奶牛AI";
        this.Load += new System.EventHandler(this.MainForm_Load);
        this.ResumeLayout(false);

    }

    #endregion

    public System.Windows.Forms.TabControl BroTabControl;
}