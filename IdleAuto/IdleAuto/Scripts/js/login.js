
(function () {
    //初始化Bridge
    async function init() {
        try {
            await CefSharp.BindObjectAsync("Bridge");
        }
        catch (e) {
            console.log("Error:", e);
        }
    }
    //在登录页填充
    async function setUsernamePwd() {
        try {
            var user = await Bridge.getSelectedAccount();
            $("#username").val(user.Username);
            $("#password").val(user.Password);
            $("#remember").prop("checked", true);
        } catch (error) {
            console.error('Error:', error);
        }
    }

    function getAccountName() {
        $('a[href="/Home/Index"]').each(function () {
            if ($(this).text() != "idle infinity") {
                return $(this).text();
            }
        });
        return '';
    }
    function getRoleInfo() {
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

    init().then((r) => {
        //登录页逻辑
        if (location.href.indexOf("Login") > -1) {
            setUsernamePwd();
        }

        //首页保存cookie或者替换cookie
        if (location.href.indexOf("Home/Index") > -1) {
            Bridge.invokeEvent("OnLoginSuccess", true, getAccountName(), getRoleInfo());
        }

        Bridge.invokeEvent("OnJsInited", "login");
    })
})();