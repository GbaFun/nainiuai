using HtmlAgilityPack;
using IdleApi.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;



public class EquipParser
{
    private HtmlDocument _doc;

    public EquipParser(string htmlContent)
    {
        _doc = new HtmlDocument();
        _doc.LoadHtml(htmlContent);
    }

    public Dictionary<int, EquipModel> GetCurEquips()
    {
        var eMap = new Dictionary<int, EquipModel>();
        var nodes = _doc.DocumentNode.SelectNodes("//span[@class='sr-only label label-danger equip-off']");

        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                var sortId = int.Parse(node.GetAttributeValue("data-type", ""));
                var prevSibling = node.PreviousSibling;
                while (prevSibling != null && prevSibling.NodeType != HtmlNodeType.Element)
                {
                    prevSibling = prevSibling.PreviousSibling;
                }

                if (prevSibling != null)
                {
                    var id = long.Parse(prevSibling.GetAttributeValue("data-id", ""));
                    
                    var contentNode = node.ParentNode.NextSibling;
                    var quality = GetQuality(prevSibling,contentNode);
                    while (contentNode != null && contentNode.NodeType != HtmlNodeType.Element)
                    {
                        contentNode = contentNode.NextSibling;
                    }

                    if (contentNode != null)
                    {
                        var content = contentNode.InnerText;
                        var e = GetEquipModel(id, sortId, quality, content, emEquipStatus.Equipped);
                        eMap.Add((int)e.emEquipSort, e);
                    }
                }
            }
        }

        return eMap;
    }

    public Dictionary<long, EquipModel> GetPackageEquips()
    {
        var eMap = new Dictionary<long, EquipModel>();
        var bagNode = _doc.DocumentNode.SelectSingleNode("//div[@class='panel-body equip-bag']");
        if (bagNode != null)
        {
            var equipNodes = bagNode.SelectNodes(".//div[contains(@class, 'equip-container')]");
            if (equipNodes != null)
            {
                foreach (var node in equipNodes)
                {
                    var equipItem = node.SelectSingleNode(".//span[contains(@class, 'equip-name')]");
                    if (equipItem != null)
                    {
                        var id = long.Parse(equipItem.GetAttributeValue("data-id", ""));
                        
                        var contentNode = _doc.DocumentNode.SelectSingleNode($"//div[@class='equip-content ei{id}']");
                        var quality = GetQuality(equipItem,contentNode);
                        if (contentNode != null)
                        {
                            var content = contentNode.InnerText;
                            var e = GetEquipModel(id, 999, quality, content, emEquipStatus.Package);
                            eMap.Add(e.EquipID, e);
                        }
                    }
                }
            }
        }

        return eMap;
    }

    private string GetQuality(HtmlNode node,HtmlNode content)
    {
        if (content.InnerText.Contains("最大凹槽"))
        {
            return "base";
        }
        // 获取【】中的中间span节点
        // 假设结构为：<span class="equip-name"><span class="">【</span><span class="X">无形神圣小盾</span><span class="require">】</span></span>
        // 其中X可以是任意类名
        var middleSpan = node.SelectSingleNode(".//span[position()=2]");
        // 获取中间span的class属性
        var qualityClass = middleSpan.GetAttributeValue("class", "未知");
        return qualityClass;
    }

    public Dictionary<long, EquipModel> GetRepositoryEquips()
    {
        var eMap = new Dictionary<long, EquipModel>();
        var boxNode = _doc.DocumentNode.SelectSingleNode("//div[@class='panel-body equip-box']");
        if (boxNode != null)
        {
            var equipNodes = boxNode.SelectNodes(".//div[contains(@class, 'equip-container')]");
            if (equipNodes != null)
            {
                foreach (var node in equipNodes)
                {
                    var equipItem = node.SelectSingleNode(".//span[contains(@class, 'equip-name')]");
                    if (equipItem != null)
                    {
                        var id = long.Parse(equipItem.GetAttributeValue("data-id", ""));
                        var contentNode = _doc.DocumentNode.SelectSingleNode($"//div[@class='equip-content ei{id}']");
                        var quality = GetQuality(equipItem,contentNode);
                        if (contentNode != null)
                        {
                            var content = contentNode.InnerText;
                            var e = GetEquipModel(id, 999, quality, content, emEquipStatus.Repo);
                            eMap.Add(e.EquipID, e);
                        }
                    }
                }
            }
        }

        return eMap;
    }

    private EquipModel GetEquipModel(long eid, int sortid, string quality, string content, emEquipStatus equipStatus)
    {
        // 移除所有空白行（包括仅包含空白字符的行）
        content = Regex.Replace(content, @"^\s*$\n?", "", RegexOptions.Multiline);

        // 移除每行的前导空白字符（包括空格和制表符）
        content = Regex.Replace(content, @"^[ \t]*", "", RegexOptions.Multiline);

        var e = new EquipModel
        {
            EquipID = eid,
            emEquipSort = (emEquipSort)sortid,
            Quality = quality,
            EquipStatus = equipStatus,
            Content = content
        };

        var sc = content.Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        if (sc.Length > 0)
        {
            var nameMatch = System.Text.RegularExpressions.Regex.Match(sc[0], @"(.*)★{0,1}\(\d*\)");
            if (nameMatch.Success)
            {
                var baseName = nameMatch.Groups[1].Value;
                var lvMatch = System.Text.RegularExpressions.Regex.Match(sc[0], @"\d+");
                if (lvMatch.Success)
                {
                    e.Lv = int.Parse(lvMatch.Value);
                }

                // 处理珠宝、秘境和道具
                if (sc.Length > 1 && sc[1].Contains("可以作为镶嵌物"))
                {
                    baseName = "珠宝";
                }
                else if (sc.Length > 2 && sc[2].Contains("可以作为镶嵌物"))
                {
                    baseName = "珠宝";
                }
                else if (baseName.Contains("秘境"))
                {
                    baseName = "秘境";
                }
                else if (baseName.Contains("药水") || baseName.Contains("宝箱") || baseName.Contains("改名卡"))
                {
                    baseName = "道具";
                }
                // 处理套装、传奇和神器
                else if (quality == "set" || quality == "unique" || quality == "artifact")
                {
                    // 跳过"已绑定"行
                    int startIndex = 1;
                    if (sc.Length > 1 && sc[1].Contains("已绑定"))
                    {
                        startIndex = 2;
                    }

                    // 查找装备类型行
                    for (int i = startIndex; i < sc.Length; i++)
                    {
                        if (!sc[i].Contains(":") && !string.IsNullOrWhiteSpace(sc[i]))
                        {
                            baseName = sc[i].Trim();
                            break;
                        }
                    }
                }
                else if (baseName.Contains("猎人的伪装"))
                {
                    baseName = "猎人的伪装";
                }
                else
                {
                    // 处理普通装备名称
                    var sbname = baseName.Replace("太古", "").Replace("无形", "");
                    if (sbname.Contains("的"))
                    {
                        var sc2 = sbname.Split('的');
                        baseName = sc2[sc2.Length - 1];
                    }
                    else if (sbname.Contains("之"))
                    {
                        var sc2 = sbname.Split('之');
                        if (sc2.Length > 1)
                        {
                            if (sc2[sc2.Length - 1].Length <= 1)
                            {
                                var c = sc2[sc2.Length - 1];
                                if (c == "斧" || c == "矛" || c == "叉" || c == "爪")
                                {
                                    baseName = sc2[sc2.Length - 1];
                                }
                                else
                                {
                                    baseName = sc2[sc2.Length - 2] + "之" + sc2[sc2.Length - 1];
                                }
                            }
                            else
                            {
                                var c1 = sc2[sc2.Length - 2];
                                var c2 = sc2[sc2.Length - 1];
                                if (c2 == "法珠" && c1 == "鹰")
                                {
                                    baseName = "鹰之法珠";
                                }
                                else
                                {
                                    baseName = sc2[sc2.Length - 1];
                                }
                            }
                        }
                    }
                }

                baseName = baseName.Replace("太古", "").Replace("无形", "");
                e.EquipBaseName = baseName;
                e.EquipName = nameMatch.Groups[1].Value;
                e.IsPerfect = sc[0].Contains('★');
                e.IsLocal = sc.Length > 1 && sc[1].Contains("已绑定");
            }
        }

        return e;
    }

    public int PackageEquipsCount()
    {
        var heading = _doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'panel-heading') and contains(text(), '背包')]");
        if (heading != null)
        {
            var text = heading.InnerText;
            var match = System.Text.RegularExpressions.Regex.Match(text, @"\d+");
            if (match.Success)
            {
                return int.Parse(match.Value);
            }
        }
        return 0;
    }

    public int RepositoryEquipsCount()
    {
        var heading = _doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'panel-heading') and contains(text(), '储藏箱')]");
        if (heading != null)
        {
            var text = heading.InnerText;
            var matches = System.Text.RegularExpressions.Regex.Matches(text, @"\d+");
            if (matches.Count > 0)
            {
                return int.Parse(matches[0].Value);
            }
        }
        return 0;
    }
    /// <summary>
    /// 如果不存在下页就返回null
    /// </summary>
    /// <param name="doc"></param>
    /// <returns></returns>
    public string GetRepoNextPageUrl()
    {
        // 使用XPath直接匹配jQuery选择器的逻辑
        var nextPageLink = _doc.DocumentNode.SelectSingleNode(
            "//div[contains(@class, 'panel-body') and contains(@class, 'equip-box')]" +
            "/following-sibling::*[1]" +
            "//a[contains(text(), '下页')]");

        return nextPageLink?.GetAttributeValue("href", null);
    }

    public string GetBagNextPageUrl()
    {
        // 使用XPath直接匹配jQuery选择器的逻辑
        var nextPageLink = _doc.DocumentNode.SelectSingleNode(
            "//div[contains(@class, 'panel-body') and contains(@class, 'equip-bag')]" +
            "/following-sibling::*[1]" +
            "//a[contains(text(), '下页')]");

        return nextPageLink?.GetAttributeValue("href", null);
    }
}