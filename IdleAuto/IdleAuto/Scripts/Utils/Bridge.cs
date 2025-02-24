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

    private EventManager _eventMa { get; set; }

    public Bridge(int seed = -1, EventManager eventMa = null)
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
        else EventManager.Instance.InvokeEvent((emEventType)Enum.Parse(typeof(emEventType), eventName), args);
    }


}

