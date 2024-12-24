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
            this.BtnLogin1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CurLoginAct
            // 
            this.CurLoginAct.Location = new System.Drawing.Point(25, 25);
            this.CurLoginAct.Name = "CurLoginAct";
            this.CurLoginAct.Size = new System.Drawing.Size(80, 20);
            this.CurLoginAct.TabIndex = 0;
            this.CurLoginAct.Text = "当前登录账号";
            this.CurLoginAct.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(105, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "label2";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BtnLogin1
            // 
            this.BtnLogin1.Location = new System.Drawing.Point(27, 70);
            this.BtnLogin1.Name = "BtnLogin1";
            this.BtnLogin1.Size = new System.Drawing.Size(136, 33);
            this.BtnLogin1.TabIndex = 2;
            this.BtnLogin1.Text = "登录RasdSky";
            this.BtnLogin1.UseVisualStyleBackColor = true;
            this.BtnLogin1.Click += new System.EventHandler(this.BtnLogin_Click);
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.BtnLogin1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CurLoginAct);
            this.Name = "TestForm";
            this.Text = "TestForm";
            this.Load += new System.EventHandler(this.TestForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label CurLoginAct;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BtnLogin1;
    }
}