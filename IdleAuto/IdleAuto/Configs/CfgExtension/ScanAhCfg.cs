using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Configs.CfgExtension
{
    public class ScanAhCfg
    {
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "ScanAhCfg.json");
        public static ScanAhCfg Instance { get; } = new ScanAhCfg();

        /// <summary>
        /// 配置列表
        /// </summary>
        public List<DemandEquip> Data { get; set; }
        /// <summary>
        /// 方便扫描同级装备的树结构
        /// </summary>
        public ScanAhTreeNode ConfigTree { get; set; }

        /// <summary>
        /// 将合并的节点存入数组方便遍历
        /// </summary>
        public List<ScanAhTreeNode> NodeList { get; set; }

        public ScanAhCfg()
        {
            NodeList = new List<ScanAhTreeNode>();
            LoadConfig();
        }

        public void LoadConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                Data = json.ToUpperCamelCase<List<DemandEquip>>();
                ConfigTree = BuildTree(Data);
                Dfs(ConfigTree);
            }
            else
            {
                Data = new List<DemandEquip>();
            }
        }



        /// <summary>
        /// 建立一棵树 将数据合并在最下层子节点 层结构为 quality->part->eqbase 例如全能只需要维护 quality:传奇,part:戒指 不需要维护底子数据将会存在 传奇 戒指节点下
        ///用于去掉同类装备的重复扫描
        /// </summary>
        /// <param name="configs"></param>
        /// <returns></returns>
        public ScanAhTreeNode BuildTree(List<DemandEquip> configs)
        {
            ScanAhTreeNode root = new ScanAhTreeNode("Root");

            foreach (var config in configs)
            {
                ScanAhTreeNode qualityNode = root.FindChild(config.Quality);
                if (qualityNode == null)
                {
                    qualityNode = new ScanAhTreeNode(config.Quality);
                    root.AddChild(qualityNode);
                }

                ScanAhTreeNode partNode = qualityNode.FindChild(config.Part);
                if (partNode == null)
                {
                    partNode = new ScanAhTreeNode(config.Part);
                    qualityNode.AddChild(partNode);
                }
                partNode.Quality = config.Quality;
                partNode.Part = config.Part;
                //没有维护底子 节点到此结束
                if (string.IsNullOrWhiteSpace(config.Eqbase))
                {
                    partNode.Configs.Add(config);
                    continue;
                }

                ScanAhTreeNode eqBaseNode = partNode.FindChild(config.Eqbase);
                if (eqBaseNode == null)
                {
                    eqBaseNode = new ScanAhTreeNode(config.Eqbase);
                    partNode.AddChild(eqBaseNode);
                }
                eqBaseNode.Quality = config.Quality;
                eqBaseNode.Part = config.Part;
                eqBaseNode.Eqbase = config.Eqbase;
                eqBaseNode.Configs.Add(config);
            }

            return root;
        }

        public void Dfs(ScanAhTreeNode node)
        {
            if (node.Children.Count == 0)
            {
                NodeList.Add(node);
            }
            else
            {
                foreach (var item in node.Children)
                {
                    Dfs(item);
                }
            }

        }

        public void PrintTree(ScanAhTreeNode node, int level)
        {
            Console.WriteLine(new string(' ', level * 2) + node.Value);
            foreach (var child in node.Children)
            {
                PrintTree(child, level + 1);
            }

            foreach (var equipment in node.Configs)
            {
                Console.WriteLine(new string(' ', (level + 1) * 2) + equipment.Name);
            }
        }
    }
    /// <summary>
    /// 扫拍需求装备 通过配置序列化
    /// </summary>
    public class DemandEquip
    {
        /// <summary>
        /// 品质
        /// </summary>
        public string Quality { get; set; }

        /// <summary>
        /// 部位
        /// </summary>
        public string Part { get; set; }
        /// <summary>
        /// 底子
        /// </summary>
        public string Eqbase { get; set; }

        /// <summary>
        /// 能接受的逻辑价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 装备名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 最低装等
        /// </summary>
        public int MinLv { get; set; }

        /// <summary>
        /// 包含词缀
        /// </summary>
        public List<string> Content { get; set; }

        public List<RegexMatch> RegexList { get; set; }
    }
    public class RegexMatch
    {
        /// <summary>
        /// 匹配类型 compareNum:数值比较
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 数值
        /// </summary>
        public string Val { get; set; }

        /// <summary>
        /// 操作 >= <= ==
        /// </summary>
        public string Op { get; set; }

        /// <summary>
        /// 请维护数值前后两个关键字 逗号拼接 触发,凤凰击
        /// </summary>
        public string Keywords { get; set; }
    }

    public class ScanAhTreeNode
    {
        public string Value { get; set; }

        public string Quality { get; set; }

        public string Part { get; set; }

        public string Eqbase { get; set; }
        public List<ScanAhTreeNode> Children { get; set; } = new List<ScanAhTreeNode>();
        public List<DemandEquip> Configs { get; set; } = new List<DemandEquip>();

        public ScanAhTreeNode(string value)
        {
            Value = value;
        }

        public void AddChild(ScanAhTreeNode child)
        {
            Children.Add(child);
        }

        public ScanAhTreeNode FindChild(string value)
        {
            return Children.Find(child => EqualityComparer<string>.Default.Equals(child.Value, value));
        }
    }
}
