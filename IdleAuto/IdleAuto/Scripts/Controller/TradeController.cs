using CefSharp;
using IdleAuto.Scripts.Wrap;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TradeController
{
    private EventSystem EventSystem;
    public TradeController()
    {
        EventSystem = new EventSystem();
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
    public async Task<bool> AcceptAll(BroWindow win, UserModel account)
    {
        //跳转消息页面
        var role = account.FirstRole;
        win.GetBro().ShowDevTools();
        P.Log($"跳转{role.RoleName}的消息页面", emLogType.AutoEquip);
        var response = await win.LoadUrlWaitJsInit(IdleUrlHelper.NoticeUrl(), "trade");
        if (response.Success)
        {
            //同意全部交易请求
            var result2 = await win.CallJsWaitReload($@"acceptAllTrade()", "trade");
            if (result2.Success)
            {
                return true;
            }
        }

        return false;
    }
}