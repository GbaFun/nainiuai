namespace IdleAuto
{
    partial class TestForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.CurLoginAct = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.WebPanel = new System.Windows.Forms.Panel();
            this.LoginMenu = new System.Windows.Forms.Panel();
            this.LoginMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // CurLoginAct
            // 
            this.CurLoginAct.Location = new System.Drawing.Point(10, 0);
            this.CurLoginAct.Name = "CurLoginAct";
            this.CurLoginAct.Size = new System.Drawing.Size(80, 20);
            this.CurLoginAct.TabIndex = 0;
            this.CurLoginAct.Text = "当前登录账号";
            this.CurLoginAct.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(10, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "label2";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // WebPanel
            // 
            this.WebPanel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.WebPanel.Location = new System.Drawing.Point(105, 0);
            this.WebPanel.Name = "WebPanel";
            this.WebPanel.Size = new System.Drawing.Size(695, 447);
            this.WebPanel.TabIndex = 3;
            // 
            // LoginMenu
            // 
            this.LoginMenu.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LoginMenu.Controls.Add(this.CurLoginAct);
            this.LoginMenu.Controls.Add(this.label2);
            this.LoginMenu.Location = new System.Drawing.Point(2, 0);
            this.LoginMenu.Name = "LoginMenu";
            this.LoginMenu.Size = new System.Drawing.Size(100, 447);
            this.LoginMenu.TabIndex = 4;
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.LoginMenu);
            this.Controls.Add(this.WebPanel);
            this.Name = "TestForm";
            this.Text = "TestForm";
            this.Load += new System.EventHandler(this.TestForm_Load);
            this.LoginMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label CurLoginAct;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel WebPanel;
        private System.Windows.Forms.Panel LoginMenu;
    }
}