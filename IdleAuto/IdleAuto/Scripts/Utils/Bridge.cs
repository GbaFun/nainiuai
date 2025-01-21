using CefSharp.WinForms;
using IdleAuto.Configs.CfgExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class Bridge
{
    /// <summary>
    /// 读取账号配置 在js端调用
    /// </summary>
    /// <returns></returns>
    public object GetSelectedAccount()
    {
        return AccountController.Instance.User;
    }

    public List<DemandEquip> GetAhDemandEquip()
    {
        return ScanAhCfg.Instance.Data;
    }

    public void InvokeEvent(string eventName, params object[] args)
    {
        EventManager.Instance.InvokeEvent((emEventType)Enum.Parse(typeof(emEventType), eventName), args);
    }
}

