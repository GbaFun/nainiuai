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
    public BroTabManager TabManager;
    private TabManager _tabManager;
    private void MainForm_Load(object sender, EventArgs e)
    {
    }

    public MainForm()
    {
        InitializeComponent();
        // 初始化 TabManager，并传递 TabControl
        TabManager = new BroTabManager(BroTabControl);
        _tabManager = new TabManager(BroTabControl);
        this.Controls.Add(new MenuWidget());
    }

   

}