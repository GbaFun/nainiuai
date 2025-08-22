
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
            this.btnArtifact = new System.Windows.Forms.Button();
            this.comArtifact = new System.Windows.Forms.ComboBox();
            this.btnSwitchJustice = new System.Windows.Forms.Button();
            this.btnRollJewelry = new System.Windows.Forms.Button();
            this.txtJewelryId = new System.Windows.Forms.TextBox();
            this.comJewelryType = new System.Windows.Forms.ComboBox();
            this.btnSendEq = new System.Windows.Forms.Button();
            this.txtSendEqCon = new System.Windows.Forms.TextBox();
            this.txtSendEqNum = new System.Windows.Forms.TextBox();
            this.txtRoleToSend = new System.Windows.Forms.TextBox();
            this.btnReformJustice = new System.Windows.Forms.Button();
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
            this.txtJordan.Size = new System.Drawing.Size(121, 25);
            this.txtJordan.TabIndex = 5;
            // 
            // btnArtifact
            // 
            this.btnArtifact.Location = new System.Drawing.Point(3, 148);
            this.btnArtifact.Name = "btnArtifact";
            this.btnArtifact.Size = new System.Drawing.Size(75, 28);
            this.btnArtifact.TabIndex = 6;
            this.btnArtifact.Text = "神器制作";
            this.btnArtifact.UseVisualStyleBackColor = true;
            this.btnArtifact.Click += new System.EventHandler(this.btnArtifact_Click);
            // 
            // comArtifact
            // 
            this.comArtifact.FormattingEnabled = true;
            this.comArtifact.Location = new System.Drawing.Point(84, 153);
            this.comArtifact.Name = "comArtifact";
            this.comArtifact.Size = new System.Drawing.Size(121, 23);
            this.comArtifact.TabIndex = 7;
            // 
            // btnSwitchJustice
            // 
            this.btnSwitchJustice.Location = new System.Drawing.Point(3, 0);
            this.btnSwitchJustice.Name = "btnSwitchJustice";
            this.btnSwitchJustice.Size = new System.Drawing.Size(75, 23);
            this.btnSwitchJustice.TabIndex = 8;
            this.btnSwitchJustice.Text = "交换正义";
            this.btnSwitchJustice.UseVisualStyleBackColor = true;
            this.btnSwitchJustice.Click += new System.EventHandler(this.btnSwitchJustice_Click);
            // 
            // btnRollJewelry
            // 
            this.btnRollJewelry.Location = new System.Drawing.Point(3, 182);
            this.btnRollJewelry.Name = "btnRollJewelry";
            this.btnRollJewelry.Size = new System.Drawing.Size(75, 28);
            this.btnRollJewelry.TabIndex = 9;
            this.btnRollJewelry.Text = "洗珠宝";
            this.btnRollJewelry.UseVisualStyleBackColor = true;
            this.btnRollJewelry.Click += new System.EventHandler(this.btnRollJewelry_Click);
            // 
            // txtJewelryId
            // 
            this.txtJewelryId.Location = new System.Drawing.Point(84, 182);
            this.txtJewelryId.Name = "txtJewelryId";
            this.txtJewelryId.Size = new System.Drawing.Size(121, 25);
            this.txtJewelryId.TabIndex = 10;
            // 
            // comJewelryType
            // 
            this.comJewelryType.FormattingEnabled = true;
            this.comJewelryType.Location = new System.Drawing.Point(211, 182);
            this.comJewelryType.Name = "comJewelryType";
            this.comJewelryType.Size = new System.Drawing.Size(121, 23);
            this.comJewelryType.TabIndex = 11;
            // 
            // btnSendEq
            // 
            this.btnSendEq.Location = new System.Drawing.Point(3, 216);
            this.btnSendEq.Name = "btnSendEq";
            this.btnSendEq.Size = new System.Drawing.Size(75, 28);
            this.btnSendEq.TabIndex = 12;
            this.btnSendEq.Text = "交易装备";
            this.btnSendEq.UseVisualStyleBackColor = true;
            this.btnSendEq.Click += new System.EventHandler(this.btnSendEq_Click);
            // 
            // txtSendEqCon
            // 
            this.txtSendEqCon.Location = new System.Drawing.Point(84, 219);
            this.txtSendEqCon.Name = "txtSendEqCon";
            this.txtSendEqCon.Size = new System.Drawing.Size(248, 25);
            this.txtSendEqCon.TabIndex = 13;
            // 
            // txtSendEqNum
            // 
            this.txtSendEqNum.Location = new System.Drawing.Point(338, 220);
            this.txtSendEqNum.Name = "txtSendEqNum";
            this.txtSendEqNum.Size = new System.Drawing.Size(100, 25);
            this.txtSendEqNum.TabIndex = 14;
            // 
            // txtRoleToSend
            // 
            this.txtRoleToSend.Location = new System.Drawing.Point(444, 220);
            this.txtRoleToSend.Name = "txtRoleToSend";
            this.txtRoleToSend.Size = new System.Drawing.Size(100, 25);
            this.txtRoleToSend.TabIndex = 15;
            // 
            // btnReformJustice
            // 
            this.btnReformJustice.Location = new System.Drawing.Point(3, 250);
            this.btnReformJustice.Name = "btnReformJustice";
            this.btnReformJustice.Size = new System.Drawing.Size(75, 25);
            this.btnReformJustice.TabIndex = 16;
            this.btnReformJustice.Text = "正义底子";
            this.btnReformJustice.UseVisualStyleBackColor = true;
            this.btnReformJustice.Click += new System.EventHandler(this.btnReformJustice_Click);
            // 
            // SecondMenuForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnReformJustice);
            this.Controls.Add(this.txtRoleToSend);
            this.Controls.Add(this.txtSendEqNum);
            this.Controls.Add(this.txtSendEqCon);
            this.Controls.Add(this.btnSendEq);
            this.Controls.Add(this.comJewelryType);
            this.Controls.Add(this.txtJewelryId);
            this.Controls.Add(this.btnRollJewelry);
            this.Controls.Add(this.btnSwitchJustice);
            this.Controls.Add(this.comArtifact);
            this.Controls.Add(this.btnArtifact);
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
        private System.Windows.Forms.Button btnArtifact;
        private System.Windows.Forms.ComboBox comArtifact;
        private System.Windows.Forms.Button btnSwitchJustice;
        private System.Windows.Forms.Button btnRollJewelry;
        private System.Windows.Forms.TextBox txtJewelryId;
        private System.Windows.Forms.ComboBox comJewelryType;
        private System.Windows.Forms.Button btnSendEq;
        private System.Windows.Forms.TextBox txtSendEqCon;
        private System.Windows.Forms.TextBox txtSendEqNum;
        private System.Windows.Forms.TextBox txtRoleToSend;
        private System.Windows.Forms.Button btnReformJustice;
    }
}