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
using System.Data;
using IdleAuto.Scripts.Controller;

namespace IdleAuto.Scripts.Controller
{
    public class CharacterController : BaseController
    {
        static readonly object LOCAKOBJECT = new object();

        static string[] Names = new string[]{
        "草泥马", "苦力怕", "烈焰人", "远古守卫", "恶魂", "幻翼", "史莱姆", "唤魔者", "凋零", "女巫", "掠夺者", "猪灵",
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

        /// <summary>
        /// 修车角色当前地图等级
        /// </summary>
        private int _curMapLv { get; set; }

        /// <summary>
        /// 目标地图等级
        /// </summary>
        private int _targetMapLv { get; set; }

        private bool _isNeedDungeon { get; set; }

        private int _broId = 0;

        protected delegate void DungeonEnd(bool result);
        private DungeonEnd _onDungeonEnd;

        private static string PrefixName = ConfigUtil.GetAppSetting("PrefixName");


        private void OnDungeonEnd(params object[] args)
        {
            string param = args[0] as string;
            if (param == "DungeonEnd")
            {
                _onDungeonEnd?.Invoke(true);
                _onDungeonEnd = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        private void OnDungeonRequired(params object[] args)
        {
            var data = args[0].ToObject<Dictionary<string, object>>();
            var isSuccess = data["isSuccess"];
            var isNeedDungeon = data["isNeedDungeon"];
            _isNeedDungeon = bool.Parse( isNeedDungeon.ToString() );
            onJsInitCallBack?.Invoke(true);
            onJsInitCallBack = null;
        }

        public CharacterController()
        {
            EventManager.Instance.SubscribeEvent(emEventType.OnJsInited, OnAhJsInited);
            EventManager.Instance.SubscribeEvent(emEventType.OnCharNameConflict, OnCharNameConflict);
            EventManager.Instance.SubscribeEvent(emEventType.OnDungeonRequired, OnDungeonRequired);
            EventManager.Instance.SubscribeEvent(emEventType.OnPostFailed, OnPostFailed);
            _isNeedDungeon = false;
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="count">当前有多少角色</param>
        /// <returns></returns>
        private async Task<string> CreateName()
        {
            await Task.Delay(50);
            var curName = AccountController.Instance.User.Username;
            var index = AccountCfg.Instance.Accounts.FindIndex(p => p.Username == curName);
            if (!String.IsNullOrWhiteSpace(PrefixName))
            {

                var name = $@"{PrefixName}{24 * index + CharCount + CharNameSeed}";
                P.Log(name);
                return name;
            }

            var name1 = Names[12 * index + CharCount] + (CharNameSeed == 0 ? "" : CharNameSeed.ToString());
            P.Log(name1);
            return name1;
        }
        /// <summary>
        /// 生成种族和职业
        /// </summary>
        /// <returns></returns>

        private Tuple<int, int> CreateRaceAndType(List<RoleModel> roles)
        {
            var lastJob = roles.Count == 0 ? null : roles.Last();
            if (lastJob == null || lastJob.Job == emJob.死骑)
            {
                return new Tuple<int, int>(1, 5);
            }
            if (lastJob.Job == emJob.骑士)
            {
                return new Tuple<int, int>(8, 4);
            }
            if (lastJob.Job == emJob.死灵)
            {
                return new Tuple<int, int>(12, 5);
            }
            //if (roles.Where(p => p.Job == emJob.武僧).Count() < 2)
            //{
            //    return new Tuple<int, int>(10, 5);
            //}
            return null;
        }



        /// <summary>
        /// 起名冲突
        /// </summary>
        /// <param name="args"></param>
        private void OnCharNameConflict(params object[] args)
        {
            lock (LOCAKOBJECT)
            {
                CharNameSeed += 1;
            }
        }

        private void OnPostFailed(params object[] args)
        {
            string errorMsg = args[0].ToString();
            P.Log(errorMsg, emLogType.Error);
        }

    

        private async Task StartDungeon( ChromiumWebBrowser bro,RoleModel role)
        {
            //开始秘境
            int dungeonLv = GetDungeonLv(_curMapLv);
            await SwitchTo(dungeonLv);
            if (bro.Address.IndexOf("InDungeon") == -1)
            {
                bro.LoadUrl($"https://www.idleinfinity.cn/Map/Dungeon?id={role.RoleId}");
                await JsInit();
            }
            //开始秘境
            var dungeonEndTask = new TaskCompletionSource<bool>();
            onJsInitCallBack = (result) => dungeonEndTask.SetResult(result);
            //开始跑自动秘境
            var d = await _browser.EvaluateScriptAsync($@"_map.startExplore();");
            await dungeonEndTask.Task;

            //再次尝试直接抵达
            await SwitchTo(_targetMapLv);
        }
        /// <summary>
        /// 开始
        /// </summary>
        public async void StartInit()
        {
            IsAutoInit = true;
            _browser = await GetBrowserAsync();
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
            {  //建号
                await CreateRole();
                await GoToMakeGroup();
                await MakeUnion();
                //工会拉人结束 开始组队
                await MakeGroup();
                Stop();
                return;
            }
        }


        private async Task GoToMakeGroup()
        {
            if (_browser.Address.IndexOf("Character/Group") == -1)
            {
                _browser.Load($@"https://www.idleinfinity.cn/Character/Group?id={AccountController.Instance.User.Roles[0].RoleId}");
                await JsInit();
            }

        }

        /// <summary>
        /// 继续建队 跳三个角色作为队长
        /// </summary>
        /// <returns></returns>
        private async Task ContinueMakeGroup()
        {
            int nextIndex = AccountController.Instance.User.Roles.FindIndex(p => p.RoleId == AccountController.Instance.CurRole.RoleId) + 3;
            if (nextIndex > 11) return;
            int roleId = AccountController.Instance.User.Roles[nextIndex].RoleId;
            _browser.Load($@"https://www.idleinfinity.cn/Character/Group?id={roleId}");
            await JsInit();

        }


        /// <summary>
        /// 拉队伍
        /// </summary>
        /// <returns>true拉队伍结束需要换一组</returns>
        private async Task MakeGroup()
        {
            var existMembers = await GetExistGroupMember();
            if (existMembers.Length == 3 && AccountController.Instance.User.Roles.FindIndex(p => p.RoleId == AccountController.Instance.CurRole.RoleId) < 9)
            {
                await ContinueMakeGroup();
                existMembers = await GetExistGroupMember();
            }
            if (existMembers.Length == 3 && AccountController.Instance.User.Roles.FindIndex(p => p.RoleId == AccountController.Instance.CurRole.RoleId) == 9)
            {
                return;
            }

            var hasGroup = await HasGroup();
            if (!hasGroup)
            {
                await CreateGroup();

            }
            else
            {
                await AddGroupMember();
                //一个队伍三个人 加完一次就结束了
            }
            await MakeGroup();

        }

        /// <summary>
        /// 拉工会
        /// </summary>
        /// <returns>true拉工会结束 </returns>
        private async Task MakeUnion()
        {
            var hasUnion = await HasUnion();
            if (!hasUnion)
            {
                //创建工会
                await CreateUnion();
            }

            var existMember = await GetExistUnionMember();
            if (AccountController.Instance.User.Roles == null || existMember == null)
            {
                Console.WriteLine("roles空");
            }
            var notInUnionMember = AccountController.Instance.User.Roles.Where(p => !existMember.Contains(p.RoleName)).FirstOrDefault();
            if (notInUnionMember != null)
            {
                await AddUnionMember(notInUnionMember);
                await MakeUnion();
            }


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
            var data = new Dictionary<string, object>();
            data["firstRoleId"] = AccountController.Instance.User.Roles[0].RoleId;
            data["cname"] = AccountController.Instance.User.Roles[1].RoleName;
            data["gname"] = AccountController.Instance.User.Roles[0].RoleName + "★";
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.createUnion({data.ToLowerCamelCase()});");
                await JsInit();
            }

        }

        /// <summary>
        /// 创建工会
        /// </summary>
        /// <returns></returns>
        private async Task CreateGroup()
        {

            var data = new Dictionary<string, object>();
            data["roleid"] = AccountController.Instance.CurRole.RoleId;
            int nextIndex = AccountController.Instance.User.Roles.FindIndex(p => p.RoleId == AccountController.Instance.CurRole.RoleId) + 1;
            data["cname"] = AccountController.Instance.User.Roles[nextIndex].RoleName;
            data["gname"] = AccountController.Instance.CurRole.RoleName + "★";
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.createGroup({data.ToLowerCamelCase()});");
                await JsInit();
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
                await JsInit();
            }

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
                await JsInit();
            }

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
        public async Task<Dictionary<string, SkillModel>> GetSkillInfo()
        {
            //因为是在自动换装那边进入这里 那边只等待了equip.js 加个延迟等待charjs载入
            await Task.Delay(1000);
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.getSkillInfo();");
                return d.Result?.ToObject<Dictionary<string, SkillModel>>();
            }
            else return null;

        }

        /// <summary>
        /// 获取人物技能
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetRoleId()
        {

            var d = await _browser.EvaluateScriptAsync($@"_char.cid");
            return d.Result.ToObject<int>();

        }


        /// <summary>
        /// 创建角色
        /// </summary>
        /// <returns></returns>
        public async Task CreateRole()
        {
            if (_browser.Address.IndexOf(PageLoadHandler.HomePage) > -1)
            {
                var roles = await GetRoles();
                CharCount = roles == null ? 0 : roles.Count;
                if (CharCount < 12)
                {
                    //不满12个号去建号
                    _browser.LoadUrl("https://www.idleinfinity.cn/Character/Create");
                    await JsInit();
                    var data = new Dictionary<string, object>();
                    data["name"] = await CreateName();
                    var info = CreateRaceAndType(roles);
                    data["race"] = info.Item2;
                    data["type"] = info.Item1;
                    if (_browser.CanExecuteJavascriptInMainFrame)
                    {
                        var d = await _browser.EvaluateScriptAsync($@"_init.createRole({data.ToLowerCamelCase()});");
                        await JsInit();
                    }
                    await CreateRole();
                }
            }



        }

        /// <summary>
        /// 读取home页所有角色
        /// </summary>
        /// <returns></returns>
        public async Task<List<RoleModel>> GetRoles()
        {

            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_init.getRoleInfo();");
                return d.Result?.ToObject<List<RoleModel>>();
            }
            return null;
        }

        protected async Task<ChromiumWebBrowser> GetBrowserAsync()
        {

            var accName = AccountController.Instance.User.AccountName;
            var seed = await MainForm.Instance.TabManager.TriggerAddTabPage(accName, $"https://www.idleinfinity.cn/Home/Index");
            _broSeed = seed;
            return MainForm.Instance.TabManager.GetBro(seed);
        }

        #region 属性点

        /// <summary>
        /// 获取人物属性
        /// </summary>
        /// <returns></returns>
        public async Task<CharBaseAttributeModel> GetCharBaseAtt()
        {
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.getSimpleAttribute();");
                return d.Result?.ToObject<CharBaseAttributeModel>();
            }
            else return null;

        }

        public async Task<CharBaseAttributeModel> GetAttributeSimpleInfo(ChromiumWebBrowser bro, RoleModel role)
        {
            _browser = bro;
            int roleid = role.RoleId;
            //不在详细页先去详细页读取属性
            if (bro.Address.IndexOf(PageLoadHandler.CharDetail) == -1)
            {
                _browser.LoadUrl($"https://www.idleinfinity.cn/Character/Detail?id={roleid}");
                await JsInit();
            }

            var info = await GetCharBaseAtt();
            return info;
        }

        /// <summary>
        /// 保存人物属性加点
        /// </summary>
        /// <returns></returns>
        public async Task SaveCharAtt(CharBaseAttributeModel data)
        {
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                await _browser.EvaluateScriptAsync($@"_char.attributeSave({data.ToLowerCamelCase()});");
                return;
            }
            else return;
        }
        /// <summary>
        /// 重置人物属性加点
        /// </summary>
        /// <returns></returns>
        public async Task ResetCharAtt()
        {
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                //P.Log($"save att:{data.StrAdd}");
                await _browser.EvaluateScriptAsync($@"_char.attributeReset();");
                return;
            }
            else return;
        }

        #endregion  

        #region 技能
        public async Task AddSkillPoints(ChromiumWebBrowser bro, RoleModel role)
        {
            _browser = bro;
            int roleid = role.RoleId;
            //不在详细页先去详细页读取属性
            if (bro.Address.IndexOf(PageLoadHandler.CharDetail) == -1)
            {
                _browser.LoadUrl($"https://www.idleinfinity.cn/Character/Detail?id={roleid}");
                await JsInit();
            }
            //判断下当前技能合不合适 合适就跳过
            var curSkill = await GetSkillInfo();
            var skillConfig = SkillPointCfg.Instance.GetSkillPoint(role.Job, role.Level);
            var targetSkillPoint = GetTargetSkillPoint(role.Level, skillConfig);
            var isAddPoint = CheckRoleSkill(curSkill, targetSkillPoint);
            if (isAddPoint)
            {
                _browser.LoadUrl($"https://www.idleinfinity.cn/Skill/Config?id={roleid}&e=1");
                await JsInit();
                P.Log("开始重置技能加点！", emLogType.AutoEquip);
                await SkillRest();
                await SkillSave(targetSkillPoint, skillConfig.JobName);
                var groupid = GetSkillGroup(skillConfig);
                await SkillGroupSave(groupid);
            }

            if (bro.Address.IndexOf(PageLoadHandler.CharDetail) == -1)
            {
                _browser.LoadUrl($"https://www.idleinfinity.cn/Character/Detail?id={roleid}");
                await JsInit();
            }
            await SkillKeySave(skillConfig.KeySkillId);
        }
        private async Task SkillRest()
        {
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.skillReset();");
                await JsInit();
            }
        }

        /// <summary>
        /// 保存技能并返回需要确认的技能组
        /// </summary>
        /// <param name="targetSkill"></param>
        /// <param name="jobname"></param>
        /// <returns></returns>
        private async Task SkillSave(Dictionary<string, int> targetSkill, string jobname)
        {
            P.Log("开始技能加点！", emLogType.AutoEquip);

            List<string> strList = new List<string>();

            targetSkill.ToList().ForEach(p =>
            {
                var s = IdleSkillCfg.Instance.GetIdleSkill(jobname, p.Key);
                strList.Add($"{s.Id}|{p.Value}");

            });
            var skillStr = string.Join(",", strList);
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("sid", skillStr);
            P.Log($"技能加点详情:{skillStr}", emLogType.AutoEquip);
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.skillSave({data.ToLowerCamelCase()});");
                if (!d.Success)
                {
                    P.Log($"技能加点失败：{d.Message}", emLogType.AutoEquip);
                }
                else
                {
                    P.Log($"技能加点成功！", emLogType.AutoEquip);
                }
                await JsInit();
            }

        }

        private string GetSkillGroup(SkillPoint config)
        {
            List<string> list = new List<string>();
            config.GroupSkill.ForEach(p =>
            {
                var s = IdleSkillCfg.Instance.GetIdleSkill(config.JobName, p);
                list.Add(s.Id.ToString());
            });
            return string.Join(",", list);
        }

        /// <summary>
        /// 选技能组
        /// </summary>
        /// <param name="targetSkill"></param>
        /// <param name="jobname"></param>
        /// <returns></returns>
        private async Task SkillGroupSave(string sid)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("sid", sid);
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.skillGroupSave({data.ToLowerCamelCase()});");
                if (!d.Success)
                {
                    P.Log($"保存携带技能失败：{d.Message}", emLogType.AutoEquip);
                }
                else
                {
                    P.Log($"保存携带技能成功！", emLogType.AutoEquip);
                }
                await JsInit();
            }
        }
        /// <summary>
        /// 获取配置匹配当前等级合适的技能字典
        /// </summary>
        /// <param name="curLv"></param>
        /// <param name="config"></param>
        /// <param name="curSkillList"></param>
        /// <returns>返回一个技能分配字典</returns>
        private Dictionary<string, int> GetTargetSkillPoint(int curLv, SkillPoint config)
        {
            int pointCount = curLv;//技能点数等于人物等级
            P.Log($"开始计算技能加点！技能点总数：{pointCount}", emLogType.AutoEquip);
            var targetSkillDic = new Dictionary<String, int>();
            foreach (var item in config.RemainSkill)
            {
                var name = item.Key;
                var lv = item.Value;
                targetSkillDic.Add(name, lv);
                pointCount -= lv;
            }
            foreach (var p in config.PrioritySkill)
            {
                if (pointCount == 0) break;

                var name = p;
                var idleSkill = IdleSkillCfg.Instance.GetIdleSkill(config.JobName, p);
                var maxSkillPoint = idleSkill.CurLvMaxPoint(curLv);//当前等级能加到max是多少
                if (!targetSkillDic.ContainsKey(name)) targetSkillDic.Add(name, 0);
                int addPoint = maxSkillPoint - targetSkillDic[name];
                if (pointCount <= addPoint)
                {
                    //剩余点数不够加到最大了就按照剩余加
                    targetSkillDic[name] += pointCount;
                    pointCount = 0;//加完了
                }
                else
                {
                    targetSkillDic[name] = maxSkillPoint;
                    pointCount -= addPoint;
                }

            }
            return targetSkillDic;

        }

        /// <summary>
        /// 判断一下技能是否合适
        /// </summary>
        /// <param name="skills"></param>
        /// <param name="targetSkillDic"></param>
        /// <returns></returns>
        private bool CheckRoleSkill(Dictionary<string, SkillModel> skills, Dictionary<string, int> targetSkillDic)
        {
            bool result = false;
            foreach (var item in targetSkillDic)
            {
                if (!skills.ContainsKey(item.Key))
                {
                    //当前技能没点目标技能 
                    result = true;
                    break;
                }
                else
                {
                    //比较下数值
                    if (skills[item.Key].Lv != item.Value)
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 保存Key技能
        /// </summary>
        /// <param name="targetSkill"></param>
        /// <param name="jobname"></param>
        /// <returns></returns>
        private async Task SkillKeySave(int skillId)
        {
            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.skillKeySave({skillId});");
                await JsInit();
            }
        }
        #endregion

        #region 切图

        public async Task StartSwitchMap()
        {

            for (int i = 0; i <1; i++)
            {
                RoleModel role = AccountController.Instance.User.Roles[i];
                int seed = await BroTabManager.Instance.TriggerAddTabPage(AccountController.Instance.User.AccountName, $"https://www.idleinfinity.cn/Map/Detail?id={role.RoleId}","char");
                var bro = BroTabManager.Instance.GetBro(seed);
                await SwitchMap(bro, role);
            }
        }
        public async Task SwitchMap(ChromiumWebBrowser bro, RoleModel role)
        {
            _browser = bro;
            int roleid = role.RoleId;
            if (bro.Address.IndexOf(PageLoadHandler.MapPage) == -1)
            {
                _browser.LoadUrl( $"https://www.idleinfinity.cn/Map/Detail?id={roleid}");
                await JsInit();
            }
            var curMapLv = await GetCurMapLv();
            _curMapLv = curMapLv;
            //检查是否层数合适
            var setting = MapSettingCfg.Instance.GetSetting(role.Level);
            if (setting == null || !setting.CanSwitch(role.Level, curMapLv)) return;

            int targetLv = setting.MapLv; //setting.MapLv;
            _targetMapLv = targetLv;

            TaskCompletionSource<bool> dungenonCallback = new TaskCompletionSource<bool>();
            onJsInitCallBack = (result) => dungenonCallback.SetResult(result);
            //尝试抵达
            await SwitchTo(targetLv, curMapLv);
            await dungenonCallback.Task;
            if (_isNeedDungeon)
            {
                _isNeedDungeon = false;//进来了就重置
                await StartDungeon(bro,role);
            }

        }

        private async Task SwitchTo(int targetLv, int curMapLv = 0)
        {

            if (_browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await _browser.EvaluateScriptAsync($@"_char.mapSwitch({targetLv});");
            }

        }


        private async Task<int> GetCurMapLv()
        {
            var d = await _browser.EvaluateScriptAsync($@"_char.getCurMapLv();");
            return d.Result.ToObject<int>();
        }

        private int GetDungeonLv(int curLv)
        {
            double result = (double)curLv / 10.0;
            return int.Parse(Math.Ceiling(result).ToString()) * 10;
        }
        #endregion

    }
}


/* 加点调用测试
//var info = await CharacterController.Instance.GetAttributeSimpleInfo(BroTabManager.Instance.GetBro(BroTabManager.Instance.GetFocusID()), AccountController.Instance.User.Roles[1]);
//P.Log($"point:{info.Point}--csa:{info.StrAdd}");
//if (info != null && info.Point > 0)
//{
//    info.StrAdd += info.Point;
//    await CharacterController.Instance.SaveCharAtt(info);
//}
*/
