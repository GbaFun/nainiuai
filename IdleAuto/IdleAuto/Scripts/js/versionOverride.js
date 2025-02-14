function generatePrint() {
    const regex = /encrypt\(\`([a-zA-Z0-9]+)\$\{CryptoJS\.SHA256\(rawString\)\.toString\(\)\}\`\);/;
    var code = document.head.innerHTML.match(regex)[1]

    const components = {};
    components.userAgent = `Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/${userKey} Safari/537.36`;
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

async function init() {
    try {
        await CefSharp.BindObjectAsync("Bridge");
    }
    catch (e) {
        console.log("Error:", e);
    }
}

var userKey;
async function getUserCfg() {
    var user = await Bridge.getSelectedAccount();
    userKey = user.key;
    console.log(userKey);
}

init().then((r) => {
    if (location.href.indexOf("Login") > -1) {
        getUserCfg();
    }
    Bridge.invokeEvent("OnJsInited", "versionOverride");
})