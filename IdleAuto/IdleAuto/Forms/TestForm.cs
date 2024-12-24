using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IdleAuto
{
    public partial class TestForm : Form
    {
        public static TestForm Instance { get; private set; }
        public TestForm()
        {
            Instance = this;
            InitializeComponent();
            InitializeLoginBtns();
        }

        private void InitializeLoginBtns()
        {
            AccountCfg.Instance.LoadConfig();
            for (int i = 0; i < AccountCfg.Instance.Accounts.Count; i++)
            {
                Button btnLogin = new Button();
                this.LoginMenu.Controls.Add(btnLogin);
                btnLogin.Location = new System.Drawing.Point(10, 40 + (i * 40));
                btnLogin.Name = $"BtnLogin{i}";
                btnLogin.Size = new System.Drawing.Size(80, 35);
                btnLogin.TabIndex = i + 2;
                btnLogin.Text = $"登录{i}-{AccountCfg.Instance.Accounts[i].Username}";
                btnLogin.UseVisualStyleBackColor = true;
                btnLogin.Tag = AccountCfg.Instance.Accounts[i];
                btnLogin.Click += new System.EventHandler(this.BtnLogin_Click);
            }
        }

        private void TestForm_Load(object sender, EventArgs e)
        {
            Console.WriteLine("On TestForm_Load");

        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"{(sender as Button).Tag.ToString()}");

            //// 实例化并显示Form1
            //LoginForm form1 = new LoginForm();
            //form1.LoginName = "rasdsky";
            //form1.LoginPassword = "123456";
            //form1.Show();
            //this.Hide(); // 隐藏当前表单
        }
    }
}
