using IdleApi.Scripts;
using IdleApi.Service.Interface;
using IdleApi.Wrap;
using System.Data;

namespace IdleApi.Service
{
    public class TradeService : BaseService, IService, ITrade
    {
        public TradeService(BroWindow window) : base(window) { }
        public async Task<bool> AcceptAll(int page = 0,NoticeParser not=null)
        {
            var win = GetWin();
            var url = IdleUrlHelper.NoticeUrl(page);
            string content = "";
            not = not == null ? new NoticeParser(content) : not;
            if (win.CurrentAddress != url)
            {
                content = await win.LoadUrlAsync(url);
                not.LoadHtml(content);
            }
             
            var anyTrade=not.GetFirstTrade();
            if (anyTrade == null) return true;
            //有消息 尝试接受
            var yesUrl = IdleUrlHelper.NoticeYesUrl();
            content = await win.SubmitFormAsync(yesUrl, new Dictionary<string, string> { { "nid", anyTrade.NoticeId } });
            //检测san值 检测是否还有消息 是否需要翻页
            not.LoadHtml(content);
            if (not.IsSanError()) return false;
            if (anyTrade.Type == emNoticeType.Equip)
            {

            }
            if (win.HasUnReadNotice())
            {
                anyTrade = not.GetFirstTrade();
                if (anyTrade == null)
                {
                    await AcceptAll(++page,not);
                }
                await AcceptAll(page,not);
            }
                return true;
        }
        public bool HasUnReadNotice()
        {
            return GetWin().HasUnReadNotice();
        }
    }
}
