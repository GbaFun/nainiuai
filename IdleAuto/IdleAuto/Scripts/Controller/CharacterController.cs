using CefSharp;
using IdleAuto.Db;
using IdleAuto.Scripts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql;

namespace IdleAuto.Scripts.Controller
{
    public class CharacterController
    {
        static string[] Names = new string[]{
        "末影龙", "苦力怕", "烈焰人", "远古守卫", "恶魂", "幻翼", "史莱姆", "唤魔者", "凋零", "女巫", "掠夺者", "猪灵",
        "钻石", "煤矿石", "铁矿石", "铜矿石", "青金石", "金矿石", "黑曜石", "绿宝石", "石英石", "萤石", "下届合金", "紫水晶",
        "基岩", "圆石", "安山岩", "闪长岩", "花岗岩", "深板岩", "砂岩", "凝灰岩", "下届岩", "菌岩", "玄武岩", "灵魂沙",
        "时运", "无限火矢", "节肢杀手", "精准采集", "水下呼吸", "亡灵杀手", "水下速掘", "冰霜行者", "海之眷顾", "饵钓", "经验修补", "横扫之刃",
        "要塞", "废弃矿道", "沙漠神殿", "远古城市", "丛林神庙", "林地府邸", "雪屋", "海底神殿", "堡垒遗迹", "林中小屋", "掠夺前哨战", "末地城",
        "竹林", "暖水海洋", "蘑菇岛", "樱花树林", "风袭丘陵", "诡异森林", "冰刺之地", "风蚀恶地", "溶洞", "繁茂洞穴", "深暗之域", "绯红森林",
        "傻子", "农民", "渔夫", "牧羊人", "制箭师", "武器匠", "图书管理员", "皮匠", "屠夫", "石匠", "制图师", "盔甲匠",
        "工作台", "堆肥桶", "木桶", "织布机", "制箭台", "砂轮", "讲台", "炼药锅", "烟熏炉", "切石机", "制图台", "高炉",
        "酿造台", "治疗药水", "再生药水", "水肺药水", "虚弱药水", "喷溅药水", "力量药水", "迅捷药水", "夜视药水", "隐身药水", "跳跃药水", "缓降药水",
        "打火石", "木屋", "TNT", "铁轨机", "刷线机", "地狱门", "刷怪笼", "附魔台", "末影珍珠", "熔岩桶", "烟花火箭", "火焰弹"
    };
        private static CharacterController instance;
        public static CharacterController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CharacterController();
                }
                return instance;
            }
        }
        /// <summary>
        /// 是否在自动初始化状态
        /// </summary>
        public bool IsAutoInit { get; set; }

        /// <summary>
        /// 角色总数
        /// </summary>
        public int CharCount { get; set; }

        public CharacterController()
        {
            EventManager.Instance.SubscribeEvent(emEventType.OnInitChar, OnInitChar);
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="count">当前有多少角色</param>
        /// <returns></returns>
        private async Task<string> CreateName()
        {
            await Task.Delay(500);
            var curName = AccountController.Instance.User.Username;
            var index = AccountCfg.Instance.Accounts.FindIndex(p => p.Username == curName);
            return Names[12 * index + CharCount];
        }
        /// <summary>
        /// 生成种族和职业
        /// </summary>
        /// <returns></returns>

        private Tuple<int, int> CreateRaceAndType()
        {
            var roles = AccountController.Instance.User.Roles;
            if (roles.Where(p => p.Job == emJob.骑士).Count() < 4)
            {
                return new Tuple<int, int>(1, 5);
            }
            if (roles.Where(p => p.Job == emJob.死灵).Count() < 4)
            {
                return new Tuple<int, int>(8, 4);
            }
            if (roles.Where(p => p.Job == emJob.死骑).Count() < 2)
            {
                return new Tuple<int, int>(12, 5);
            }
            if (roles.Where(p => p.Job == emJob.武僧).Count() < 2)
            {
                return new Tuple<int, int>(10, 5);
            }
            return null;
        }

        /// <summary>
        /// js主动调用继续执行初始化
        /// </summary>
        /// <param name="args"></param>
        private async void OnInitChar(params object[] args)
        {

            if (IsAutoInit)
            {
                await StartAutoJob();
            }
        }
        /// <summary>
        /// 开始
        /// </summary>
        public async void StartInit()
        {
            IsAutoInit = true;
            await StartAutoJob();
        }

        public void Stop()
        {
            IsAutoInit = false;
        }
        /// <summary>
        /// 开始自动执行
        /// </summary>
        private async Task StartAutoJob()
        {
            if (IsAutoInit)
            {
                if (MainForm.Instance.browser.Address.IndexOf(PageLoadHandler.HomePage) > -1)
                {
                    var roles = await GetRoles();
                    CharCount = roles.Count;
                    if (roles.Count >= 12)
                    {
                        IsAutoInit = false;
                        return;
                    }
                    await Task.Delay(1500);
                    MainForm.Instance.browser.LoadUrl("https://www.idleinfinity.cn/Character/Create");
                }
                else if (MainForm.Instance.browser.Address.IndexOf(PageLoadHandler.CharCreate) > -1)
                {
                    await CreateRole();
                }
            }
        }


        /// <summary>
        /// 获取人物属性
        /// </summary>
        /// <returns></returns>
        public async Task<CharAttributeModel> GetCharAtt()
        {
            if (MainForm.Instance.browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await MainForm.Instance.browser.EvaluateScriptAsync($@"_char.getAttribute();");
                return d.Result?.ToObject<CharAttributeModel>();
            }
            else return null;

        }


        /// <summary>
        /// 创建角色
        /// </summary>
        /// <returns></returns>
        public async Task CreateRole()
        {
            await Task.Delay(1500);
            var data = new Dictionary<string, object>();
            data["name"] = await CreateName();
            var info = CreateRaceAndType();
            data["race"] = info.Item2;
            data["type"] = info.Item1;
            if (MainForm.Instance.browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await MainForm.Instance.browser.EvaluateScriptAsync($@"_init.createRole({data.ToLowerCamelCase()});");
            }

        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <returns></returns>
        public async Task<List<RoleModel>> GetRoles()
        {
            await Task.Delay(1500);
            if (MainForm.Instance.browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await MainForm.Instance.browser.EvaluateScriptAsync($@"_init.getRoleInfo();");
                return d.Result?.ToObject<List<RoleModel>>();
            }
            return null;
        }


    }
}
