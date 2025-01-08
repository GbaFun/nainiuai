namespace IdleAuto.Scripts.View

{
    partial class RuneCfgItem
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
            this.RuneName = new System.Windows.Forms.Label();
            this.LableCnt = new System.Windows.Forms.Label();
            this.CountInput = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.CountInput)).BeginInit();
            this.SuspendLayout();
            // 
            // RuneName
            // 
            this.RuneName.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RuneName.Location = new System.Drawing.Point(0, 1);
            this.RuneName.Name = "RuneName";
            this.RuneName.Size = new System.Drawing.Size(40, 20);
            this.RuneName.TabIndex = 0;
            this.RuneName.Text = "29#";
            this.RuneName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LableCnt
            // 
            this.LableCnt.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LableCnt.Location = new System.Drawing.Point(40, 1);
            this.LableCnt.Name = "LableCnt";
            this.LableCnt.Size = new System.Drawing.Size(100, 20);
            this.LableCnt.TabIndex = 1;
            this.LableCnt.Text = "保留数量：";
            this.LableCnt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CountInput
            // 
            this.CountInput.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.CountInput.Location = new System.Drawing.Point(140, 1);
            this.CountInput.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.CountInput.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.CountInput.Name = "CountInput";
            this.CountInput.Size = new System.Drawing.Size(100, 21);
            this.CountInput.TabIndex = 2;
            this.CountInput.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.CountInput.ValueChanged += new System.EventHandler(this.CountInput_ValueChanged);
            // 
            // RuneCfgItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.RuneName);
            this.Controls.Add(this.LableCnt);
            this.Controls.Add(this.CountInput);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "RuneCfgItem";
            this.Size = new System.Drawing.Size(260, 22);
            ((System.ComponentModel.ISupportInitialize)(this.CountInput)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label RuneName;
        private System.Windows.Forms.Label LableCnt;
        private System.Windows.Forms.NumericUpDown CountInput;
    }
}
