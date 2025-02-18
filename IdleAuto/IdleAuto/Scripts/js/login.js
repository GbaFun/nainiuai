
let userKey;
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
        checkIP();
    }

    //首页保存cookie或者替换cookie
    if (location.href.indexOf("Home/Index") > -1) {
        Bridge.invokeEvent("OnLoginSuccess", true, getAccountName(), getRoleInfo());
    }

    Bridge.invokeEvent("OnJsInited", "login");
})
function generatePrint() {
    const regex = /encrypt\(\`([a-zA-Z0-9]+)\$\{CryptoJS\.SHA256\(rawString\)\.toString\(\)\}\`\);/;
    var code = document.head.innerHTML.match(regex)[1]

    const components = {};
    components.userAgent = `Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/${userKey} Safari/537.36`;
    console.log(components.userAgent);
    components.screen = `${screen.width}x${screen.height}-${screen.colorDepth}`;
    components.timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
    components.language = navigator.language;
    components.platform = navigator.platform;
    components.deviceMemory = navigator.deviceMemory;
    components.hardwareConcurrency = navigator.hardwareConcurrency;

    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    ctx.textBaseline = 'top';
    ctx.fillText('Fingerprint', 2, 2);
    components.canvas = CryptoJS.MD5(ctx.getImageData(0, 0, 200, 50).data).toString();

    try {
        const canvas2 = document.createElement('canvas');
        const gl = canvas2.getContext('webgl');
        const debugInfo = gl.getExtension('WEBGL_debug_renderer_info');
        components.webglVendor = gl.getParameter(debugInfo.UNMASKED_VENDOR_WEBGL);
        components.webglRenderer = gl.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL);
    } catch (e) {
        components.webglVendor = 'unsupported';
        components.webglRenderer = 'unsupported';
    }
    try {
        components.plugins = Array.from(navigator.plugins).map(plugin => plugin.name);
    }
    catch (e) {
        components.plugins = "undefined";
    }

    const rawString = JSON.stringify(components);
    //console.log(rawString);
    return encrypt(`${code}${CryptoJS.SHA256(rawString).toString()}`);
}

function checkIP() {
    var errTxt = $(".validation-summary-errors").text();
    if (errTxt.indexOf("相同IP") == -1) return;
    Bridge.invokeEvent('OnIpBan', '');
}