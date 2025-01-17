

let _init = {};
(function () {
    const names = [
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
    ]

    async function init() {
        try {
            await CefSharp.BindObjectAsync("Bridge");
        }
        catch (e) {
            console.log("Error:", e);
        }
    }
    var dataMap = {}//存储dataid对应装备的数据 

    init().then(async () => {
        await Bridge.invokeEvent('OnInitChar');
    })

    //创建角色
    async function createRole() {
        var data = {
            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val(),
           name:"奶牛苦工996",
           type: 1,
           race:5
        };
        debugger;
        POST_Message("Create", data, "post", 1500).then((r) => {
            location.href = "home/index";
        }).catch((e) => {
            if (e.status == 200) {
                location.href = "../home/index";
            }
        })
    }
    async function getRoleInfo() {
        var roles = [];
        $(".col-sm-6.col-md-4").each(function () {
            var role = {};
            role.RoleId = $(this).data("id");
            role.RoleName = $(this).find("span.sort-item.name").text();
            var d = $(this).find("div.media-body").children("div").first();
            console.log(d);
            console.log(d.text());
            role.RoleInfo = d.text();
            roles.push(role);
        });
        return roles;
    }

    


    _init.createRole = createRole;
    _init.getRoleInfo = getRoleInfo;
})();
