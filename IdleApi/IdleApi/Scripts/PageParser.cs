using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace IdleApi.Scripts
{
    /// <summary>
    /// 处理一些公共页面信息的parser 比如消息大多页面都能看到消息提示
    /// </summary>
    public class PageParser : BaseParser
    {
        public PageParser(string html) : base(html)
        {


        }


        /// <summary>
        /// 有没有未读消息
        /// </summary>
        /// <returns></returns>
        public bool HasUnReadNotice()
        {
            var badge = Doc.QuerySelector(".navbar-fixed-top a:contains('消息') .badge");
            if (badge != null && int.TryParse(badge.TextContent.Trim(), out var num))
            {
                return num > 0;
            }
            return false;
        }

    }
}
