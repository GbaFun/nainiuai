using CefSharp;
using IdleAuto.Db;
using IdleAuto.Scripts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql;
using CefSharp.WinForms;

namespace IdleAuto.Scripts.Controller
{
    public class CharacterController
    {
        private ChromiumWebBrowser _browser;
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
        /// <summary>
        /// 起名冲突加值
        /// </summary>
        public int CharNameSeed { get; set; }

        public RoleModel CurRole { get; set; }

        public int CurRoleIndex { get; set; }

        private static string PrefixName = ConfigUtil.GetAppSetting("PrefixName");

        public CharacterController()
        {
            EventManager.Instance.SubscribeEvent(emEventType.OnInitChar, OnInitChar);
            EventManager.Instance.SubscribeEvent(emEventType.OnCharNameConflict, OnCharNameConflict);
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
            if (!String.IsNullOrWhiteSpace(PrefixName))
            {

                var name = $@"{PrefixName}{24 * index + CharCount + CharNameSeed}";
                return name;
            }

            return Names[12 * index + CharCount] + (CharNameSeed == 0 ? "" : CharNameSeed.ToString());
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
                var count = roles.Where(p => p.Job == emJob.骑士).Count();
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
        /// 起名冲突
        /// </summary>
        /// <param name="args"></param>
        private async void OnCharNameConflict(params object[] args)
        {
            await Task.Delay(500);
            CharNameSeed += 1;
        }
        /// <summary>
        /// 开始
        /// </summary>
        public async void StartInit()
        {
            MainForm.Instance.ShowLoadingPanel("开始初始化账号【建号、组队、工会】", emMaskType.AUTO_INIT);
            IsAutoInit = true;
            await StartAutoJob();
        }

        public void Stop()
        {
            IsAutoInit = false;
            //todo:更新按钮状态
            //MainForm.Instance.BtnInit.Invoke(new Action(() =>
            //{
            //    MainForm.Instance.BtnInit.Text = "开始初始化";
            //}));
        }
        /// <summary>
        /// 开始自动执行
        /// </summary>
        private async Task StartAutoJob()
        {
            
            if (IsAutoInit)
            {
                if (_browser.Address.IndexOf(PageLoadHandler.HomePage) > -1)
                {
                    var roles = await GetRoles();

                    CharCount = roles == null ? 0 : roles.Count;
                    if (CharCount >= 12)
                    {
                        //12个号建完去组队页面
                        await GoToMakeGroup();
                        return;
                    }
                    await Task.Delay(1000);
                    //不满12个号去建号
                    _browser.LoadUrl("https://www.idleinfinity.cn/Character/Create");
                }
                else if (_browser.Address.IndexOf(PageLoadHandler.CharCreate) > -1)
                {
                    //建号
                    await CreateRole();
                }
                else if (_browser.Address.IndexOf(PageLoadHandler.CharGroup) > -1)
                {
                    //工会
                    var isUnionDone = await MakeUnion();
                    if (isUnionDone)
                    {
                        //工会拉人结束 开始组队
                        var isGroupDone = await MakeGroup();
                        if (AccountController.Instance.CurRoleIndex == 9 && isGroupDone)
                        {
                            //最后一组完成
                            Stop();
                            return;
                        }
                    }
                }
            }
        }

        private async Task GoToMakeGroup()
        {
            if (_browser.Address.IndexOf("Character/Group") == -1)
            {
                _browser.Load($@"https://www.idleinfinity.cn/Character/Group?id={AccountController.Instance.User.Roles[0].RoleId}");
            }
            await Task.Delay(500);
        }

        /// <summary>
        /// 继续建队 跳三个角色作为队长
        /// </summary>
        /// <returns></returns>
        private async Task ContinueMakeGroup()
        {
            await Task.Delay(1000);
            int nextIndex = AccountController.Instance.User.Roles.FindIndex(p => p.RoleId == AccountController.Instance.CurRole.RoleId) + 3;
            if (nextIndex > 11) return;
            int roleId = AccountController.Instance.User.Roles[nextIndex].RoleId;
            _browser.Load($@"https://www.idleinfinity.cn/Character/Group?id={roleId}");

        }


        /// <summary>
        /// 拉队伍
        /// </summary>
        /// <returns>true拉队伍结束需要换一组</returns>
        private async Task<Boolean> MakeGroup()
        {
            var existMembers = await GetExistGroupMember();
            if (existMembers.Length == 3 && AccountController.Instance.User.Roles.FindIndex(p => p.RoleId == AccountController.Instance.CurRole.RoleId) < 9)
            {
                await ContinueMakeGroup();
            }

            var hasGroup = await HasGroup();
            if (!hasGroup)
            {
                await CreateGroup();
                return false;
            }
            else
            {
                await AddGroupMember();
                return true;//一个队伍三个人 加完一次就结束了
            }

        }

        /// <summary>
        /// 拉工会
        /// </summary>
        /// <returns>true拉工会结束 </returns>
        private async Task<Boolean> MakeUnion()
        {
            await Task.Delay(500);

            var hasUnion = await HasUnion();
            if (!hasUnion)
            {
                //创建工会
                await CreateUnion();
            }
            else
            {
                var existMember = await GetExistUnionMember();
                if (AccountController.Instance.User.Roles == null || existMember == null)
                {
                    Console.WriteLine("roles空");
                }
                var notInUnionMember = AccountController.Instance.User.Roles.Where(p => !existMember.Contains(p.RoleName)).FirstOrDefault();
                if (notInUnionMember == null) return true;
                await AddUnionMember(notInUnionMember);

            }
            return false;
        }

        /// <summary>
        /// 是否有工会
        /// </summary>
        /// <returns></returns>
        private async Task<Boolean> HasUnion()
        {
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.hasUnion();");
                return d.Result.ToObject<Boolean>();
            }
            else return false;
        }

        /// <summary>
        /// 是否有工会
        /// </summary>
        /// <returns></returns>
        private async Task<Boolean> HasGroup()
        {
            await Task.Delay(1000);
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.hasGroup();");
                return d.Result.ToObject<Boolean>();
            }
            else return false;
        }

        /// <summary>
        /// 创建工会
        /// </summary>
        /// <returns></returns>
        private async Task CreateUnion()
        {
            await Task.Delay(1000);
            var data = new Dictionary<string, object>();
            data["firstRoleId"] = AccountController.Instance.User.Roles[0].RoleId;
            data["cname"] = AccountController.Instance.User.Roles[1].RoleName;
            data["gname"] = AccountController.Instance.User.Roles[0].RoleName + "★";
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.createUnion({data.ToLowerCamelCase()});");

            }

        }

        /// <summary>
        /// 创建工会
        /// </summary>
        /// <returns></returns>
        private async Task CreateGroup()
        {
            await Task.Delay(1000);
            var data = new Dictionary<string, object>();
            data["roleid"] = AccountController.Instance.CurRole.RoleId;
            int nextIndex = AccountController.Instance.User.Roles.FindIndex(p => p.RoleId == AccountController.Instance.CurRole.RoleId) + 1;
            data["cname"] = AccountController.Instance.User.Roles[nextIndex].RoleName;
            data["gname"] = AccountController.Instance.CurRole.RoleName + "★";
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.createGroup({data.ToLowerCamelCase()});");

            }

        }

        /// <summary>
        /// 工会加人
        /// </summary>
        /// <returns></returns>
        private async Task AddUnionMember(RoleModel r)
        {

            var data = new Dictionary<string, object>();
            data["firstRoleId"] = AccountController.Instance.User.Roles[0].RoleId;
            data["cname"] = r.RoleName;
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.addUnionMember({data.ToLowerCamelCase()});");
            }
            await Task.Delay(1000);
        }
        /// <summary>
        /// 组队加人
        /// </summary>
        /// <returns></returns>
        private async Task AddGroupMember()
        {
            var existMembers = await GetExistGroupMember();
            if (existMembers.Length == 3) return;
            var data = new Dictionary<string, object>();
            data["roleid"] = AccountController.Instance.CurRole.RoleId;
            int nextIndex = AccountController.Instance.User.Roles.FindIndex(p => p.RoleId == AccountController.Instance.CurRole.RoleId) + 2;
            data["cname"] = AccountController.Instance.User.Roles[nextIndex].RoleName;
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.addGroupMember({data.ToLowerCamelCase()});");
            }
            await Task.Delay(1000);

        }

        /// <summary>
        /// 查找在工会中的人员
        /// </summary>
        /// <returns></returns>
        private async Task<string[]> GetExistUnionMember()
        {

            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.getExistUnionMember();");
                return d.Result.ToObject<string[]>();

            }
            else return new string[] { };

        }

        /// <summary>
        /// 查找在队伍中的人员
        /// </summary>
        /// <returns></returns>
        private async Task<string[]> GetExistGroupMember()
        {

            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.getExistGroupMember();");
                return d.Result.ToObject<string[]>();
            }
            else return new string[] { };

        }


        /// <summary>
        /// 获取人物属性
        /// </summary>
        /// <returns></returns>
        public async Task<CharAttributeModel> GetCharAtt()
        {
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.getAttribute();");
                return d.Result?.ToObject<CharAttributeModel>();
            }
            else return null;

        }

        /// <summary>
        /// 获取人物技能
        /// </summary>
        /// <returns></returns>
        public async Task<List<SkillModel>> GetSkillInfo()
        {
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.getSkillInfo();");
                return d.Result?.ToObject<List<SkillModel>>();
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
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.createRole({data.ToLowerCamelCase()});");
            }

        }

        /// <summary>
        /// 读取home页所有角色
        /// </summary>
        /// <returns></returns>
        public async Task<List<RoleModel>> GetRoles()
        {
            await Task.Delay(1500);
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.getRoleInfo();");
                return d.Result?.ToObject<List<RoleModel>>();
            }
            return null;
        }


    }
}
