using CefSharp;
using IdleAuto.Scripts.Wrap;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TradeController : BaseController
{

    public TradeController(BroWindow win) : base(win)
    {
        _eventMa = new EventSystem();
    }

    public async Task<bool> StartTrade(BroWindow win, UserModel account, long equipID, string roleName)
    {
        //跳转装备页面
        var role = account.FirstRole;
        win.GetBro().ShowDevTools();
        P.Log($"跳转{role.RoleName}的装备详情页面", emLogType.AutoEquip);
        var response = await win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(role.RoleId), "equip");
        if (response.Success)
        {
            //向对应角色发送交易请求
            var result2 = await win.CallJsWaitReload($@"equipTrade({role.RoleId},{equipID},""{roleName}"")", "equip");
            if (result2.Success)
            {
                return true;
            }
        }
        return false;
    }

    public async Task<bool> AcceptTrade(BroWindow win, UserModel account, long equipId, string fromName, string toName)
    {
        //TODO 交易请求的接受

        ////跳转消息页面
        //var role = account.FirstRole;
        //win.GetBro().ShowDevTools();
        //P.Log($"跳转{role.RoleName}的消息页面", emLogType.AutoEquip);
        //var response = await win.LoadUrlWaitJsInit(IdleUrlHelper.NoticeUrl(), "trade");
        //if (response.Success)
        //{

        //}
        ////接受交易请求
        return true;
    }
    public async Task<bool> AcceptAll(UserModel account)
    {
        //跳转消息页面
        var role = account.FirstRole;
        // _win.GetBro().ShowDevTools();
        P.Log($"跳转{role.RoleName}的消息页面", emLogType.AutoEquip);
        if (_browser.Address.IndexOf(IdleUrlHelper.NoticeUrl()) == -1)
        {
            var response = await _win.LoadUrlWaitJsInit(IdleUrlHelper.NoticeUrl(), "trade");
            if (!response.Success) throw new Exception("消息页加载失败");
        }

        //同意全部交易请求
        var result2 = await _win.CallJs($@"_trade.getAnyTradeId()");
        var anyId = result2.Result.ToObject<int>();
        if (anyId > 0)
        {
            var r3 = await _win.CallJsWaitReload($"_trade.acceptTrade({anyId})", "trade");
            if (!r3.Success)
            {
                throw new Exception("接收失败");
            }
            await Task.Delay(1500);
            await AcceptAll(account);
        }


        return false;
    }

    /// <summary>
    /// 发送符文
    /// </summary>
    /// <param name="dic">符文数量字典</param>
    /// <param name="toRolename">发给谁</param>
    /// <param name="isTotal">无视数量发送全部该种类符文</param>
    /// <returns></returns>
    public async Task TradeRune(Dictionary<int, int> dic, string toRolename, bool isTotal = false)
    {

        var role = _win.User.FirstRole;
        // _win.GetBro().ShowDevTools();
        var response = await _win.LoadUrlWaitJsInit(IdleUrlHelper.MaterialUrl(role.RoleId), "rune");


        if (!isTotal)
        {
            var isRuneEnough = await CheckRuneEnough(dic);
            if (!isRuneEnough) throw new Exception("符文不够");
        }
        var sendDic = new Dictionary<int, int>();
        foreach (var item in dic)
        {
            var rune = item.Key;
            var r = await _win.CallJs($"getRuneNum({item.Key})");

            var count = r.Result.ToObject<int>();
            if (count < 1) continue;
            int sendCount = isTotal ? count : item.Value;
            if (sendCount == 0) continue;
            sendDic.Add(item.Key, sendCount);
        }

        foreach (var item in sendDic)
        {
            var data = new Dictionary<string, object>();
            data.Add("rune", item.Key);
            data.Add("count", item.Value);
            data.Add("roleName", toRolename);
            var r = await _win.CallJsWaitReload($"tradeRune({data.ToLowerCamelCase()})", "rune");
            if (!r.Success)
            {
                throw new Exception("发送失败");
            }
        }

    }

    private async Task<bool> CheckRuneEnough(Dictionary<int, int> dic)
    {
        foreach (var item in dic)
        {
            var r = await _win.CallJs(@"getRuneNum()");
            if (r.Success)
            {
                var count = r.Result.ToObject<int>();
                if (item.Value > count) return false;
            }
        }
        return true;
    }
}