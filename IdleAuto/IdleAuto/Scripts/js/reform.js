let _reform = {};
(function () {

    async function init() {
        try {
            await CefSharp.BindObjectAsync("Bridge");
        }
        catch (e) {
            console.log("Error:", e);
        }
    }

    init().then(async () => {
        Bridge.invokeEvent('OnJsInited', 'reform');

    })


    async function reform(d) {
        var type = d.type;
        var data = {
            type: type
        };
        debugger
        await POST_Message("EquipReform", MERGE_Form(data)).then((r) => {
            debugger
            Bridge.invokeEvent('OnJsInited', 'reform');
        }).catch((r, status, xhr) => {
            debugger
            Bridge.invokeEvent('OnJsInited', 'reform');
        });
    }

    _reform.reform = reform;
})();