using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AppInitData
{

    public static void LoadData()
    {
        Categories = ReadCategories(filePath);
    }
    public static Dictionary<string, string> Categories { get; set; }


    private const string filePath = "Document/装备底子分类表.txt";
    public static Dictionary<string, string> ReadCategories(string filePath)
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
}

