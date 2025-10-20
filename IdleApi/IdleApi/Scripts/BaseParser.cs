using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace IdleApi.Scripts
{
    public class BaseParser
    {
        public BaseParser(string html)
        {
            Parser = new HtmlParser();
            LoadHtml(html);

        }

        public void LoadHtml(string html)
        {
            Doc = Parser.ParseDocument(html);
            InnerHtml = Doc.DocumentElement.InnerHtml;
        }
        public HtmlParser Parser { get; set; }

        public IHtmlDocument Doc { get; set; }

        public string InnerHtml { get; set; }

        /// <summary>
        /// san值警告
        /// </summary>
        /// <returns></returns>
        public bool IsSanError()
        {
            if (InnerHtml.Contains("SAN值不足"))
            {
                return true;
            }
            return false;
        }
    }
}
