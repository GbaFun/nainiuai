
(function () {
    //加载选中的账号
    async function init() {
        try {
            await CefSharp.BindObjectAsync('Bridge');
            var user = await Bridge.getSelectedAccount();
            debugger;
            console.log(user);
            $("#username").val(user.Username);
            $("#password").val(user.Password);
            
        } catch (error) {
            console.error('Error:', error);
        }
    }


    init();



})();