namespace IdleAuto.Scripts.View
{
    partial class MaskForm
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
            this.Content = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Content
            // 
            this.Content.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.Content.BackColor = System.Drawing.Color.Black;
            this.Content.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Content.ForeColor = System.Drawing.SystemColors.Control;
            this.Content.Location = new System.Drawing.Point(-7, 464);
            this.Content.Name = "Content";
            this.Content.Size = new System.Drawing.Size(1001, 40);
            this.Content.TabIndex = 0;
            this.Content.Text = "Loading";
            this.Content.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MaskForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(985, 503);
            this.Controls.Add(this.Content);
            this.Name = "MaskForm";
            this.Text = "MaskForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label Content;
    }
}