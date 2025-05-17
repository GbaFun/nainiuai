

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
        userKey = user.Key;
        console.log(userKey);
        console.log(generatePrint());
    } catch (error) {
        console.error('Error:', error);
    }
}

function getAccountName() {
    $('a[href="/Home/Index"]').each(function () {
        if ($(this).text() !== "idle infinity") {
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
        (function () { function r(t) { return t[Math.floor(Math.random() * t.length)] } const uaTypes = [`Mozilla/5.0 (Linux; Android ${r(["8", "9", "10", "11", "12"])}; ${r(["Pixel 5", "Mi 10T", "SM-G975F"])}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/${r([80, 90, 100])}.0.0.0 Mobile Safari/537.36`, `Mozilla/5.0 (iPhone; CPU iPhone OS ${r(["14", "15", "16"])}_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/${r(["14", "15"])}.0 Mobile/15E148 Safari/604.1`]; function f() { try { Object.defineProperty(navigator, "userAgent", { value: r(uaTypes) }); const res = r(["360x640", "414x896", "375x812"]).split("x"); Object.defineProperties(screen, { width: { value: +res[0] }, height: { value: +res[1] } }); Object.defineProperty(navigator, "language", { value: r(["zh-CN", "en-US", "ja-JP"]) }); Object.defineProperty(navigator, "hardwareConcurrency", { value: r([2, 4, 8]) }); return "✅ 指纹修改成功\nUA: " + navigator.userAgent } catch (e) { return "❌ 修改失败: " + e.message } } const d = document.createElement("div"); d.style = "position:fixed;bottom:0;left:0;right:0;background:#fff;padding:10px;z-index:99999;font-family:Arial;box-shadow:0 0 10px #0003;"; d.textContent = f(); document.body.appendChild(d); setTimeout(() => d.remove(), 3000); })();
        // checkIP();
    }


    setTimeout(() => {
        if (location.href.indexOf("Home/Index") > -1) {
            Bridge.invokeEvent("OnLoginSuccess", true, getAccountName(), getRoleInfo());
        }

        Bridge.invokeEvent("OnJsInited", "login");
    }, 1000);//首页保存cookie或者替换cookie
  
});


function checkIP() {
    var errTxt = $(".validation-summary-errors").text();
    if (errTxt.indexOf("相同IP") === -1) return;
    Bridge.invokeEvent('OnIpBan', '');
}