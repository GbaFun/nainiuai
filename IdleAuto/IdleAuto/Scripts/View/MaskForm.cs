using CefSharp.Structs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IdleAuto.Scripts.View
{
    public partial class MaskForm : Form
    {
        private Form BindForm;
        public MaskForm(Form form)
        {
            InitializeComponent();
            Opacity = 0.5;
            BackColor = Color.White;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            //位置和大小跟随主界面
            BindForm = form;
            RefreshFrom();
        }

        public void RefreshFrom()
        {
            if (BindForm != null)
            {
                Location = BindForm.Location;
                Size = BindForm.Size;
            }
        }
        public void SetLoadContent(string content)
        {
            RefreshFrom();
            this.Content.Text = content;
        }
    }
}
