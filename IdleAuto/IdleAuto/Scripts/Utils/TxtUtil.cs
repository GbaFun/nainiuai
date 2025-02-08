using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

public class TxtUtil
{
    private const string filePath = "Document/装备底子分类表.txt";
    private static Dictionary<string, string> ReadCategories(string filePath)
    {
        var categories = new Dictionary<string, string>();
        string currentCategory = null;

        foreach (var line in File.ReadLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (!line.Contains("\t"))
            {
                currentCategory = line.Trim();
            }
            else if (currentCategory != null)
            {
                var parts = line.Split('\t');
                if (parts.Length > 1)
                {
                    if (parts[0].Contains("#")) continue;
                    string itemName = parts[1].Trim();
                    if (!categories.ContainsKey(itemName))
                    {
                        categories[itemName] = currentCategory;
                    }
                }
            }
        }

        return categories;
    }

    public static Dictionary<string, string> Categories { get; } = ReadCategories(filePath);

    public static string GetCategory(string itemName)
    {
        return Categories.TryGetValue(itemName, out var category) ? category : itemName;
    }
}
