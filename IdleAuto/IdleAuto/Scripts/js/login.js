
(function () {

    async function init() {
        try {
            await CefSharp.BindObjectAsync('Bridge');
            var t = await Bridge.getAccount();
            console.log(t);
        } catch (error) {
            console.error('Error:', error);
        }
    }

    init();



})();