using HtmlAgilityPack;
using System.Net.Http;

namespace IdleApi.Scripts
{
    public class RoleParser
    {
        private HtmlDocument _doc;
        public RoleParser(string htmlContent)
        {
            _doc = new HtmlDocument();
            _doc.LoadHtml(htmlContent);
        }

        public async Task<List<RoleModel>> GetRoleInfoAsync()
        {
            var characters = new List<RoleModel>();

            try
            {
                // 使用与JS相同的选择器结构
                var roleNodes = _doc.DocumentNode.SelectNodes("//div[contains(@class, 'col-sm-6') and contains(@class, 'col-md-4')]");

                if (roleNodes != null)
                {
                    foreach (var node in roleNodes)
                    {
                        var character = ParseRoleNode(node);
                        if (character != null)
                        {
                            characters.Add(character);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scraping role info: {ex.Message}");
            }

            return characters;
        }

        private RoleModel ParseRoleNode(HtmlNode node)
        {
            try
            {
                var character = new RoleModel();

                // 获取RoleId（从data-id属性）
                var roleIdAttr = node.GetAttributeValue("data-id", "");
                if (!string.IsNullOrEmpty(roleIdAttr) && int.TryParse(roleIdAttr, out int roleId))
                {
                    character.RoleId = roleId;
                }

                // 获取角色名称
                var nameSpan = node.SelectSingleNode(".//span[contains(@class, 'sort-item') and contains(@class, 'name')]");
                if (nameSpan != null)
                {
                    character.RoleName = nameSpan.InnerText.Trim();
                }

                // 获取角色信息（第一个media-body div的第一个子div）
                var mediaBody = node.SelectSingleNode(".//div[contains(@class, 'media-body')]");
                if (mediaBody != null)
                {
                    var firstChildDiv = mediaBody.SelectSingleNode("./div[1]");
                    if (firstChildDiv != null)
                    {
                        character.RoleInfo = firstChildDiv.InnerText.Trim();
                    }
                }

                // 解析是否挂机
                var idleLabel = node.SelectSingleNode(".//span[contains(@class, 'label') and contains(@class, 'label-info')]");
                character.IsIdle = idleLabel != null && idleLabel.InnerText.Trim() == "挂机";

                // 解析经验值百分比
                var expProgress = node.SelectSingleNode(".//div[contains(@class, 'progress-bar-exp')]");
                if (expProgress != null)
                {
                    var expText = expProgress.SelectSingleNode(".//span")?.InnerText;
                    character.ExpPercent = ExtractPercentage(expText);
                }

                return character;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing role node: {ex.Message}");
                return null;
            }
        }

        private decimal ExtractPercentage(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;

            text = text.Replace("%", "");
            if (decimal.TryParse(text, out decimal result))
            {
                return result;
            }
            return 0;
        }
    }
}