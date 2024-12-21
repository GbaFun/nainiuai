using CefSharp;
using CefSharp.DevTools.Network;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IdleAuto
{
    public partial class Form2 : Form
    {
        WebBrowser webBrowser1;
        public Form2()
        {
            InitializeComponent();
            CefSettings settings = new CefSettings();
            Cef.Initialize(settings);
            webBrowser1 = new WebBrowser();
            this.Controls.Add(webBrowser1);

            OnFrameLoadEnd();
        }

        public void CallCSharpFromJS()
        {
            MessageBox.Show("Called from JavaScript");
        }

        private void Form2_Load(object sender, EventArgs e)
        {
        }

        private void OnFrameLoadEnd()
        {
            this.webBrowser1.DocumentText = @"<html><script type='text/javascript'>
                                            function callCSharpFunction() {
                                                window.external.CallCSharpFromJS();
                                            }
                                         </script><body>
                                            <button onclick='callCSharpFunction()'>Call C# Function</button>
                                         </body></html>";
            this.webBrowser1.ObjectForScripting = this;
        }
    }
}
