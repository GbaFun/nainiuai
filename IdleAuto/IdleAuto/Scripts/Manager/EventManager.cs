using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum emEventType
{
    OnUpgradeRuneBack,
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
                instance.eventDic = new Dictionary<emEventType, Action<emEventType, object[]>>();
            }
            return instance;
        }

    }

    private Dictionary<emEventType, Action<emEventType, object[]>> eventDic;

    public void SubscribeEvent(emEventType eventType, Action<emEventType, object[]> action)
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
    public void UnsubscribeEvent(emEventType eventType, Action<emEventType, object[]> action)
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
            eventDic[eventType]?.Invoke(eventType, args);
        }
    }
}
