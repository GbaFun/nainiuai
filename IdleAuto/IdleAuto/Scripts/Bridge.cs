using CefSharp.WinForms;
using IdleAuto.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class Bridge
{
    public void ShowMessage(params object[] a)
    {
        foreach (var item in a)
        {
            Console.WriteLine(a.ToString());
        }
    }
    public string GetMessage()
    {
        Console.WriteLine("JavaScript Called Cs");
        return "Message from C#";
    }

    /// <summary>
    /// 读取账号配置 在js端调用
    /// </summary>
    /// <returns></returns>
    public object GetSelectedAccount()
    {
        return CurrentUser.User;
    }
}

