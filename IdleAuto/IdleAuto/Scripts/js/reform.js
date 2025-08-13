let _reform = {};
(function () {

    async function init() {
        try {
            await CefSharp.BindObjectAsync("Bridge");
            addRuneModule();
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
        slotRandom: $("td:contains('随机数量凹槽')").parent().find('.label-danger.equip-reform').attr("data-type") * 1,
        direct: 50,
        mage: $("td:contains('合成施法者系列物品')").parent().find('.label-danger.equip-reform').attr("data-type") * 1,
        upgradeRare: $("td:contains('升级为稀有物品')").parent().find('.label-danger.equip-reform').attr("data-type") * 1,
        upgradeMagical: $("td:contains('升级为魔法物品')").parent().find('.label-danger.equip-reform').attr("data-type") * 1,
        rare19: $("td:contains('重置所有词缀数值')").parent().find('.label-danger.equip-reform').attr("data-type") * 1,
        set21: $($("td:contains('重置所有词缀')")[1]).parent().find('.label-danger.equip-reform').attr("data-type") * 1,
        set25: $("td:contains('重置所有词缀数值')").parent().find('.label-danger.equip-reform').attr("data-type") * 1,
        unique22: $($("td:contains('重置所有词缀')")[1]).parent().find('.label-danger.equip-reform').attr("data-type") * 1,

    }

    async function isMeterialEnough() {
        var canDirect = $("td:contains('最大数量凹槽')").parent().find('.label ').text() == "执行";
        var canRandom = $("td:contains('随机数量凹槽')").parent().find('.label ').text() == "执行";
        var canSlotRandom = $("td:contains('随机数量凹槽')").parent().find('.label ').text() === "执行";
        var canMage = $("td:contains('合成施法者系列物品')").parent().find('.label ').text() == "执行";
        var canUpgradeRare = $("td:contains('升级为稀有物品')").parent().find('.label ').text() == "执行";
        var canUpgradeMagical = $("td:contains('升级为魔法物品')").parent().find('.label ').text() == "执行";
        var canRare19 = $("td:contains('重置所有词缀数值')").parent().find('.label ').text() == "执行";
        var canSet21 = $($("td:contains('重置所有词缀')")[1]).parent().find('.label ').text() == "执行";
        var canSet25 = $("td:contains('重置所有词缀数值')").parent().find('.label ').text() == "执行";
        var canUnique22 = $($("td:contains('重置所有词缀')")[1]).parent().find('.label ').text() == "执行";
        return {
            canDirect: canDirect, canRandom: canRandom, canMage: canMage, canUpgradeRare: canUpgradeRare, canUpgradeMagical: canUpgradeMagical, canRare19: canRare19, canSlotRandom: canSlotRandom
            , canSet21: canSet21, canSet25: canSet25, canUnique22: canUnique22
        }
    }

    async function reform(d) {

        var type = d.type;
        var data = {
            type: reformType[type]
        };
        debugger
        await POST_Message("EquipReform", MERGE_Form(data)).then((r) => {
            debugger
        }).catch((r, status, xhr) => {
            debugger
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

    //加入可以直接交易符文的模块
    function addRuneModule() {
        var div = $('<div>', {
            'class': 'col-md-9',
            "style": "color:black"
        });
        var input = $('<input>', {
            "type": "text",
            "style": "color:black",
            "id":"txtRune"

        });
        var input1 = $('<input>', {
            "type": "button",
            "value": "继续改造",
            "click": () => {
                Bridge.invokeEvent("OnSignal", "Continue");
            }

        });
        var input2 = $('<input>', {
            "type": "button",
            "value": "交易符文",
            "click": () => {
                var text = $("#txtRune").val();
                debugger
                Bridge.invokeEvent("TradeRune", text);
            }

        });
        var input3 = $('<input>', {
            "type": "button",
            "value": "跳过",
            "click": () => {
                Bridge.invokeEvent("OnSignal", "Skip");
            }

        });
        div.append(input);
        div.append(input2);
        div.append(input3);
        div.append(input1);

        $(".container .row").append(div);
    }

    _reform.reform = reform;
    _reform.isMeterialEnough = isMeterialEnough;
    _reform.removeRune = removeRune;
    _reform.getEquipContent = getEquipContent;
})();