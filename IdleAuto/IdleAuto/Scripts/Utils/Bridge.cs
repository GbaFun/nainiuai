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
    private int _seed { get; set; }

    private EventSystem _eventMa { get; set; }

    public Bridge(int seed = -1, EventSystem eventMa = null)
    {
        _seed = seed;
        _eventMa = eventMa;
    }
    public int GetSeed()
    {
        return _seed;
    }

    public void InvokeEvent(string eventName, params object[] args)
    {
        if (_eventMa != null)
        {
            _eventMa.InvokeEvent((emEventType)Enum.Parse(typeof(emEventType), eventName), args);
        }
        else
            throw (new Exception("请先初始化事件控制器！"));
        //else _eventMa.InvokeEvent((emEventType)Enum.Parse(typeof(emEventType), eventName), args);
    }

    /// <summary>
    /// 读取账号配置 在js端调用
    /// </summary>
    /// <returns></returns>
    public object GetSelectedAccount()
    {
        return AccountController.Instance.User;
    }


}

