using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum emEventType
{
    OnBrowserFrameLoadStart,
    OnBrowserFrameLoadEnd,
    /// <summary>
    /// js初始化完成
    /// </summary>
    OnJsInited,
    /// <summary>
    /// 登录成功
    /// </summary>
    OnLoginSuccess,
    /// <summary>
    /// 登录账户变化
    /// </summary>
    OnAccountDirty,
    /// <summary>
    /// 升级符文返回
    /// </summary>
    OnUpgradeRuneBack,
    /// <summary>
    /// 载入ah页面发送装备信息
    /// </summary>
    OnScanAh,
    /// <summary>
    /// 初始化账号 包括角色 组队 工会
    /// </summary>
    OnInitChar,

    /// <summary>
    /// 角色载入
    /// </summary>
    OnCharLoaded,

    /// <summary>
    /// 角色名冲突
    /// </summary>
    OnCharNameConflict
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
