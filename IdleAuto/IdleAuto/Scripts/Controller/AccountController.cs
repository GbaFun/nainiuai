using IdleAuto.Scripts.Utils;
using IdleAuto.Scripts.Wrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AccountController
{
    private static object lockObject = new object();
    private static AccountController instance;
    private EventSystem EventSystem;
    public static AccountController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AccountController();
            }
            return instance;
        }
    }


    public AccountController()
    {
        User = null;
        EventSystem = new EventSystem();
        EventSystem.SubscribeEvent(emEventType.OnLoginSuccess, OnLoginSuccess);
        EventSystem.SubscribeEvent(emEventType.OnCharLoaded, OnCharLoaded);
        EventSystem.SubscribeEvent(emEventType.OnIpBan, RestIp);
    }

    //当前登录账号
    public UserModel User;

    public RoleModel CurRole;

    public int CurRoleIndex;


    private void OnLoginSuccess(params object[] args)
    {
        lock (lockObject)
        {
            if (args.Length == 3)
            {
                bool isSuccess = (bool)args[0];
                string account = args[1] as string;
                List<RoleModel> roles = args[2].ToObject<List<RoleModel>>();
                if (!User.IsLogin)
                {
                    User.SetLogin(isSuccess, account, roles);
                    EventSystem.InvokeEvent(emEventType.OnAccountDirty, null);
                }
                else
                {
                    User.UnionRoles(roles);
                }
            }
        }
    }

    private void OnCharLoaded(params object[] args)
    {
        lock (lockObject)
        {
            var cid = int.Parse(args[0].ToString());
            this.CurRoleIndex = User.Roles.FindIndex(p => p.RoleId == cid);
            this.CurRole = User.Roles[CurRoleIndex];
        }

    }

    private void RestIp(params object[] args)
    {
        RemoveAndRestBro();
    }

    /// <summary>
    /// 销毁浏览器 设置代理重置
    /// </summary>
    /// <returns></returns>
    private async Task RemoveAndRestBro()
    {
        BroTabManager.Instance.ClearBrowsers();
        string proxyUrl = "https://dps.kdlapi.com/api/getdps/?secret_id=o1gw8hkwkxldn9648cop&signature=u7zc8v5gowxvsgqi1eu1da292jx7rz04&num=1&pt=1&format=json&sep=1&dedup=1";
        string s = HttpUtil.Get(proxyUrl);
        var o = JsonConvert.DeserializeObject<JObject>(s);
        var code = o.Value<int>("code");
        if (code != 0)
        {
            P.Log("快代理提取失败");
            return;
        }
        var data = o.Value<JToken>("data");
        var proxyList = data.Value<JArray>("proxy_list");
        var proxyServer = proxyList[0].ToString();
        string[] arr = proxyServer.Split(':');
        proxyServer = $"http://{proxyServer}";
        string ip = arr[0];
        int port = int.Parse(arr[1]);
        BroTabManager.Proxy = proxyServer;
        await BroTabManager.Instance.TriggerAddTabPage(User.AccountName, "https://www.idleinfinity.cn/Home/Login", proxy: proxyServer);
        return;

    }
}
