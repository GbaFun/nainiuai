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
        }

        private void TestForm_Load(object sender, EventArgs e)
        {
            Console.WriteLine("On TestForm_Load");
            AccountCfg.Instance.LoadConfig();
            for (int i = 0; i < AccountCfg.Instance.Accounts.Count; i++)
            {
                Console.WriteLine($"Load Account {AccountCfg.Instance.Accounts[i].Username} Password {AccountCfg.Instance.Accounts[i].Password}");
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            // 实例化并显示Form1
            LoginForm form1 = new LoginForm();
            form1.LoginName = "rasdsky";
            form1.LoginPassword = "123456";
            form1.Show();
            this.Hide(); // 隐藏当前表单
        }


    }
}
