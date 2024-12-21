using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IdleAuto
{
    public partial class ConsoleForm : Form
    {
        public ConsoleForm()
        {
            InitializeComponent();
            // 添加登录按钮
            Button btnLogin = new Button();
            btnLogin.Text = "登录";
            btnLogin.Location = new Point(10, 10); // 设置按钮位置
            btnLogin.Click += BtnLogin_Click; // 绑定点击事件
            this.Controls.Add(btnLogin); // 将按钮添加到表单
        }

        private void ConsoleForm_Load(object sender, EventArgs e)
        {

        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            // 实例化并显示Form1
            Form1 form1 = new Form1();
            form1.Show();
            //form1.Owner
            //this.Hide(); // 隐藏当前表单
        }
    }
}
