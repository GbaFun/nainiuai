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

    //随机打孔不同部位数值不一样
    var reformType = {
        random: $("td:contains('随机数量凹槽')").parent().find('.label-danger.equip-reform').attr("data-type") * 1,
        direct: 50,
        mage: $("td:contains('合成施法者系列物品')").parent().find('.label-danger.equip-reform').attr("data-type") * 1,
    }

    async function isMeterialEnough() {
        var canDirect = $("td:contains('最大数量凹槽')").parent().find('.label ').text() == "执行";
        var canRandom = $("td:contains('随机数量凹槽')").parent().find('.label ').text() == "执行";
        var canMage = $("td:contains('合成施法者系列物品')").parent().find('.label ').text() == "执行";

        return { canDirect: canDirect, canRandom: canRandom, canMage: canMage }
    }

    async function reform(d) {

        var type = d.type;
        var data = {
            type: reformType[type]
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

    async function removeRune() {
        var data = {
            type: $("td:contains('移除并销毁当前镶嵌物')").parent().find('.label-danger.equip-reform').attr("data-type") * 1
        };
        await POST_Message("EquipReform", MERGE_Form(data)).then((r) => {

        }).catch((r, status, xhr) => {

        });
    }

    function getEquipContent() {
        return $(".panel-body").text()
    }

    _reform.reform = reform;
    _reform.isMeterialEnough = isMeterialEnough;
    _reform.removeRune = removeRune;
    _reform.getEquipContent = getEquipContent;
})();