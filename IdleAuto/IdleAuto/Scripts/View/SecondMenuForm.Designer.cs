
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
            // SecondMenuForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnCollectNotDelAll);
            this.Controls.Add(this.btnCollectDelAll);
            this.Name = "SecondMenuForm";
            this.Text = "SecondMenuForm";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnCollectDelAll;
        private System.Windows.Forms.Button btnCollectNotDelAll;
    }
}