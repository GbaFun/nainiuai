
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
            debugger;
            console.log(user);
            $("#username").val(user.Username);
            $("#password").val(user.Password);
            $("#remember").prop("checked", true);
        } catch (error) {
            console.error('Error:', error);
        }
    }


  
    init().then((r) => {
        //登录页逻辑
        if (location.href.indexOf("Login") > -1) {
            setUsernamePwd();
        }

        //首页保存cookie或者替换cookie
        if (location.href.indexOf("Home/Index") > -1) {
            
        }
    })




})();