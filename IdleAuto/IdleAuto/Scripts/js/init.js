

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
            location.href = "home/index";
        }).catch((e) => {
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

    


    _init.createRole = createRole;
    _init.getRoleInfo = getRoleInfo;
})();
