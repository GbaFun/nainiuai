using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Utils
{
    public class HtmlUtil
    {
        public static HtmlNodeCollection GetNodesByXpath(string html, string xpath)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes(xpath);
            return nodes;

        }
    }
}
