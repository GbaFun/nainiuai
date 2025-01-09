using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum emEventType
{
    OnLoginSuccess,
    OnAccountDirty,
    OnUpgradeRuneBack,
    /// <summary>
    /// 载入ah页面发送装备信息
    /// </summary>
    OnScanAh,
}

public class EventManager
{
    private static EventManager instance;
    public static EventManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new EventManager();
                instance.eventDic = new Dictionary<emEventType, Action<object[]>>();
            }
            return instance;
        }

    }

    private Dictionary<emEventType, Action<object[]>> eventDic;

    public void SubscribeEvent(emEventType eventType, Action<object[]> action)
    {
        if (eventDic.ContainsKey(eventType))
        {
            eventDic[eventType] += action;
        }
        else
        {
            eventDic.Add(eventType, action);
        }
    }
    public void UnsubscribeEvent(emEventType eventType, Action<object[]> action)
    {
        if (eventDic.ContainsKey(eventType))
        {
            eventDic[eventType] -= action;
        }
    }

    public void InvokeEvent(emEventType eventType, params object[] args)
    {
        if (eventDic.ContainsKey(eventType))
        {
            eventDic[eventType]?.Invoke(args);
        }
    }
}
