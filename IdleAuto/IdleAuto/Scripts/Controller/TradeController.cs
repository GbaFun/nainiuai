using AttributeMatch;
using CefSharp;
using IdleAuto.Db;
using IdleAuto.Scripts.Model;
using IdleAuto.Scripts.Utils;
using IdleAuto.Scripts.Wrap;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TradeController : BaseController
{
    private bool _isSkipTrade { get; set; } = false;

    public TradeController(BroWindow win) : base(win)
    {
        _eventMa = win.GetEventMa();
        _eventMa.SubscribeEvent(emEventType.OnSanError, OnSanNotEnough);
    }

    public void OnSanNotEnough(params object[] args)
    {
        _isSkipTrade = true;
    }

    public async Task<bool> StartTrade(EquipModel equip, string roleName)
    {
        await Task.Delay(1000);
        //跳转装备页面
        var role = _win.User.FirstRole;
        //_win.GetBro().ShowDevTools();
        P.Log($"跳转{role.RoleName}的装备详情页面", emLogType.AutoEquip);
        var response = await _win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(role.RoleId), "equip");
        await Task.Delay(1500);
        if (response.Success)
        {
            //向对应角色发送交易请求
            var result2 = await _win.CallJsWaitReload($@"equipTrade({role.RoleId},{equip.EquipID},""{roleName}"")", "equip");
            if (result2.Success)
            {
                equip.EquipStatus = emEquipStatus.Trading;
                DbUtil.InsertOrUpdate<EquipModel>(equip);
                return true;
            }
        }
        return false;
    }


    public async Task<bool> PutToAuction(EquipModel equip, int rune, int count)
    {
        //跳转装备页面
        var role = _win.User.FirstRole;
        //_win.GetBro().ShowDevTools();
        P.Log($"跳转{role.RoleName}的装备详情页面", emLogType.AutoEquip);
        var response = await _win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(role.RoleId), "equip");
        await Task.Delay(1500);
        var data = new Dictionary<string, object>();
        data.Add("cid", role.RoleId);
        data.Add("eid", equip.EquipID);
        var obj = new
        {
            isFix = true,
            gold = 0,
            runes = new[]
            {
                new
                {
                    index = rune,
                    count = count
                 }
            }
        };


        data.Add("priceJson", JsonConvert.SerializeObject(obj));
        //向对应角色发送交易请求
        var result2 = await _win.CallJsWaitReload($@"putToAuction({data.ToLowerCamelCase()})", "equip");
        if (result2.Success)
        {

            return true;
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
        if (_isSkipTrade)
        {
            _isSkipTrade = false;
            return false;
        }
        await Task.Delay(1000);
        //跳转消息页面
        var role = account.FirstRole;
        // _win.GetBro().ShowDevTools();
        P.Log($"跳转{role.RoleName}的消息页面", emLogType.AutoEquip);
        if (_browser.Address.IndexOf(IdleUrlHelper.NoticeUrl()) == -1)
        {
            var response = await _win.LoadUrlWaitJsInit(IdleUrlHelper.NoticeUrl(), "trade,equip");
            if (!response.Success) throw new Exception("消息页加载失败");
        }

        //同意全部交易请求
        var result2 = await _win.CallJs($@"_trade.getAnyTrade()");
        var anyObj = result2.Result.ToObject<Dictionary<string, object>>();
        var type = anyObj["type"].ToString();
        var noticeId = anyObj["noticeId"] == null ? 0 : int.Parse(anyObj["noticeId"].ToString());
        if (noticeId > 0)
        {

            await Task.Delay(1000);
            var r3 = await _win.CallJsWaitReload($"_trade.acceptTrade({noticeId})", "trade,equip");
            if (!r3.Success)
            {
                throw new Exception("接收失败");
            }
            if (type == "装备"&& !_isSkipTrade)
            {
                var content = anyObj["content"].ToString();
                var eqid = anyObj["equipId"].ToString();
                var quality = anyObj["quality"].ToString();
                await UpdateTradeStatus(anyObj, emTradeStatus.Received, emEquipStatus.Repo);
            }

            await Task.Delay(1000);
            await AcceptAll(account);
        }


        return false;
    }

    /// <summary>
    /// 需要更新仓库和交易记录中的状态
    /// </summary>
    /// <returns></returns>
    private async Task UpdateTradeStatus(Dictionary<string, object> anyObj, emTradeStatus tradeStatus, emEquipStatus eqStatus)
    {
        var content = anyObj["content"].ToString();
        var eqid = long.Parse(anyObj["equipId"].ToString());
        var quality = anyObj["quality"].ToString();
        // var eq = FreeDb.Sqlite.Select<EquipModel>(new long[] { eqid }).First();
        var json = JsonConvert.SerializeObject(new { eid = eqid, sortid = 999, quality = quality, content = content });
        var r = await _win.CallJs($"readEquipInfo({json})");
        var eq = r.Result.ToObject<EquipModel>();
        eq.EquipStatus = eqStatus;
       // eq.Category = CategoryUtil.GetCategory(eq.EquipBaseName);
        eq.SetAccountInfo(_win.User);
        var tradeInfo = FreeDb.Sqlite.Select<TradeModel>(new long[] { eq.EquipID }).First();

        using (var uow = FreeDb.Sqlite.CreateUnitOfWork())
        {
            var repo = uow.GetRepository<EquipModel>(); //仓储 CRUD
            repo.InsertOrUpdate(eq);
            if (tradeInfo != null)
            {
                tradeInfo.TradeStatus = tradeStatus;
                var r2 = uow.GetRepository<TradeModel>();
                r2.InsertOrUpdate(tradeInfo);
            }

            uow.Commit();
        }
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

    /// <summary>
    /// 检查符文是否为0 在汇集符文阶段跳过这种账号节省san
    /// </summary>
    /// <param name="dic">符文数量字典</param>

    /// <returns></returns>
    public async Task<bool> CheckRuneIsZero(Dictionary<int, int> dic)
    {

        var role = _win.User.FirstRole;
        // _win.GetBro().ShowDevTools();
        if (_win.GetBro().Address.IndexOf(IdleUrlHelper.MaterialUrl(role.RoleId)) == -1) await _win.LoadUrlWaitJsInit(IdleUrlHelper.MaterialUrl(role.RoleId), "rune");

        var a = await _win.CallJs("getRuneMap()");
        var runeMap = a.Result.ToObject<Dictionary<int, int>>();

        bool result = true;

        foreach (var item in dic)
        {
            if (runeMap[item.Key] > 0)
            {
                result = false; break;
            }
        }
        return result;
    }

    public async Task SaveRuneMap()
    {
        var role = _win.User.FirstRole;
        // _win.GetBro().ShowDevTools();
        if (_win.GetBro().Address.IndexOf(IdleUrlHelper.MaterialUrl(role.RoleId)) == -1) await _win.LoadUrlWaitJsInit(IdleUrlHelper.MaterialUrl(role.RoleId), "rune");
        var a = await _win.CallJs("getRuneMap()");
        var runeMap = a.Result.ToObject<Dictionary<int, int>>();
        foreach (var item in runeMap)
        {
            var model = new RuneMapModel() { Key = _win.User.AccountName + item.Key, AccountName = _win.User.AccountName, Count = item.Value, RuneName = item.Key };
            DbUtil.InsertOrUpdate<RuneMapModel>(model);
        }
    }



    private async Task<bool> CheckRuneEnough(Dictionary<int, int> dic)
    {
        foreach (var item in dic)
        {
            var r = await _win.CallJs($"getRuneNum({item.Key})");
            if (r.Success)
            {
                var count = r.Result.ToObject<int>();
                if (item.Value > count) return false;
            }
        }
        return true;
    }
}