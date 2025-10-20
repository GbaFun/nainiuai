using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace IdleApi.Scripts
{
    /// <summary>
    /// 消息页parser
    /// </summary>
    public class NoticeParser : BaseParser
    {
        public NoticeParser(string html) : base(html)
        {

        }
    


        public NoticeObj GetFirstTrade()

        {



            var arr = Doc.QuerySelectorAll(".notice-content .notice-yes");
            if (arr.Length == 0) return null;
            var first = arr[0];
            long equipId;
            var equipName = first?.ParentElement?.QuerySelector(".equip-name");
            bool isEquip = long.TryParse(equipName?.GetAttribute("data-id"), out equipId);
            var content = first?.ParentElement?.NextElementSibling?.Text();
            var type = equipName?.GetAttribute("data-type");
            var obj = new NoticeObj();
            if (isEquip)
            {
                obj.Type = emNoticeType.Equip;
                obj.EquipId = equipId;
                obj.Content = content;
                obj.Quality = type;
            }
            else
            {
                obj.Type = emNoticeType.Others;
            }
            var noticeId = first?.GetAttribute("data-id");
            obj.NoticeId = noticeId;

            return obj;
        }

        // 与 JS 返回结构一致的 C# 对象
        public class NoticeObj
        {
            public emNoticeType Type { get; set; }
            public long EquipId { get; set; }
            public string? Content { get; set; }
            public string Quality { get; set; }
            public string NoticeId { get; set; }
        }

    }
}
