

let _init = {};
(function () {
 

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
    async function createRole(data) {
        var data = {
            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val(),
           name:data.name,
           type: data.type,
           race:data.race
        };
        debugger;
        POST_Message("Create", data, "post", 1500).then((r) => {
            location.href = "Home/Index";
        }).catch((e) => {
            if (e.responseText.indexOf("角色名称已经存在")>-1) {
                Bridge.invokeEvent('OnCharNameConflict');
            }
            if (e.status == 200) {
                location.href = "../Home/Index";
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
    async function hasUnion(data) {
        var hasUnion = $("span:contains('工会')").next().text().indexOf("创建并邀请") > -1;
        return hasUnion;
    }

    async function createUnion(data) {
        var data = {
            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val(),
            id: data.firstRoleId,
            type: 2,
            cname: data.cname,
            gname: data.gname
        };
        debugger;
        POST_Message("GroupCreate", data, "post", 1500).then((r) => {
            location.reload();
        }).catch((e) => {
            location.reload();
        })
    }


    _init.createRole = createRole;
    _init.getRoleInfo = getRoleInfo;
    _init.hasUnion = hasUnion;
    _init.createUnion = createUnion;
})();
