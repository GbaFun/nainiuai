using CefSharp;
using CefSharp.DevTools.Network;
using CefSharp.WinForms;
using CefSharp.WinForms.Internals;
using IdleAuto.Db;
using IdleAuto.Scripts.Controller;
using IdleAuto.Scripts.View;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm : Form
{
    public static MainForm Instance;


    public BroTabManager TabManager;
    private void MainForm_Load(object sender, EventArgs e)
    {
    }

    public MainForm()
    {
        InitializeComponent();
        Instance = this;
        // 初始化 TabManager，并传递 TabControl
        TabManager = new BroTabManager(BroTabControl);
        this.Controls.Add(new MenuWidget());
    }
    public void ShowLoadingPanel(string content = "", emMaskType mType = emMaskType.WEB_LOADING)
    {
        //P.Log($"ShowLoadingPanel--From:{mType}", emLogType.Warning);
        //if (maskForm == null)
        //{
        //    maskForm = new MaskForm(this);
        //}
        //if (!maskForm.Visible)
        //{
        //    maskForm.Show();
        //    maskType = mType;
        //}
        //if (!string.IsNullOrEmpty(content) && mType == maskType)
        //{
        //    maskForm.SetLoadContent(content);
        //}
        //this.LoadingPanel.Visible = true;
    }
    public void HideLoadingPanel(emMaskType mType = emMaskType.WEB_LOADING)
    {
        //P.Log($"HideLoadingPanel--From:{mType}", emLogType.Warning);
        //if (mType != maskType)
        //{
        //    return;
        //}
        //maskForm?.Hide();
        //if (this.LoadingPanel.Visible)
        //    this.LoadingPanel.Visible = false;
    }
    public void SetLoadContent(string content)
    {
        //if (maskForm == null)
        //{
        //    maskForm = new MaskForm(this);
        //}
        //if (!maskForm.Visible)
        //    maskForm.Show();
        //if (!string.IsNullOrEmpty(content))
        //{
        //    maskForm.SetLoadContent(content);
        //}
    }
}