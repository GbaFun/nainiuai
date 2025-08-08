
namespace IdleAuto.Scripts.View
{
    partial class SecondMenuForm
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
            this.btnCollectDelAll = new System.Windows.Forms.Button();
            this.btnCollectNotDelAll = new System.Windows.Forms.Button();
            this.btnRollMinshen = new System.Windows.Forms.Button();
            this.btnBoss = new System.Windows.Forms.Button();
            this.txtJordan = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnCollectDelAll
            // 
            this.btnCollectDelAll.Location = new System.Drawing.Point(0, 29);
            this.btnCollectDelAll.Name = "btnCollectDelAll";
            this.btnCollectDelAll.Size = new System.Drawing.Size(78, 23);
            this.btnCollectDelAll.TabIndex = 1;
            this.btnCollectDelAll.Text = "全删收菜";
            this.btnCollectDelAll.UseVisualStyleBackColor = true;
            this.btnCollectDelAll.Click += new System.EventHandler(this.btnCollectDelAll_Click);
            // 
            // btnCollectNotDelAll
            // 
            this.btnCollectNotDelAll.Location = new System.Drawing.Point(0, 58);
            this.btnCollectNotDelAll.Name = "btnCollectNotDelAll";
            this.btnCollectNotDelAll.Size = new System.Drawing.Size(78, 23);
            this.btnCollectNotDelAll.TabIndex = 2;
            this.btnCollectNotDelAll.Text = "不全删收菜";
            this.btnCollectNotDelAll.UseVisualStyleBackColor = true;
            this.btnCollectNotDelAll.Click += new System.EventHandler(this.btnCollectNotDelAll_Click);
            // 
            // btnRollMinshen
            // 
            this.btnRollMinshen.Location = new System.Drawing.Point(3, 87);
            this.btnRollMinshen.Name = "btnRollMinshen";
            this.btnRollMinshen.Size = new System.Drawing.Size(75, 23);
            this.btnRollMinshen.TabIndex = 3;
            this.btnRollMinshen.Text = "洗冥神";
            this.btnRollMinshen.UseVisualStyleBackColor = true;
            this.btnRollMinshen.Click += new System.EventHandler(this.btnRollMinshen_Click);
            // 
            // btnBoss
            // 
            this.btnBoss.Location = new System.Drawing.Point(3, 116);
            this.btnBoss.Name = "btnBoss";
            this.btnBoss.Size = new System.Drawing.Size(75, 26);
            this.btnBoss.TabIndex = 4;
            this.btnBoss.Text = "开boss";
            this.btnBoss.UseVisualStyleBackColor = true;
            this.btnBoss.Click += new System.EventHandler(this.btnBoss_Click);
            // 
            // txtJordan
            // 
            this.txtJordan.Location = new System.Drawing.Point(84, 117);
            this.txtJordan.Name = "txtJordan";
            this.txtJordan.Size = new System.Drawing.Size(63, 25);
            this.txtJordan.TabIndex = 5;
            // 
            // SecondMenuForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.txtJordan);
            this.Controls.Add(this.btnBoss);
            this.Controls.Add(this.btnRollMinshen);
            this.Controls.Add(this.btnCollectNotDelAll);
            this.Controls.Add(this.btnCollectDelAll);
            this.Name = "SecondMenuForm";
            this.Text = "SecondMenuForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnCollectDelAll;
        private System.Windows.Forms.Button btnCollectNotDelAll;
        private System.Windows.Forms.Button btnRollMinshen;
        private System.Windows.Forms.Button btnBoss;
        private System.Windows.Forms.TextBox txtJordan;
    }
}