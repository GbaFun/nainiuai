using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class Bridge
{
    public object SendData(params object[] a)
    {

        return JsInvoker.HandleMessage(a);

    }


    /// <summary>
    /// 读取账号配置 在js端调用
    /// </summary>
    /// <returns></returns>
    public object GetSelectedAccount()
    {
        return AccountController.User;
    }

}

