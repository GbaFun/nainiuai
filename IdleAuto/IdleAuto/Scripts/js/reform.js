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
            type: type == "random" ? $("td:contains('随机数量凹槽')").parent().find('.label-danger.equip-reform').attr("data-type")*1 :50
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