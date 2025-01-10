async function init() {
    try {
        console.log('equip.js start init');
        await CefSharp.BindObjectAsync("Bridge");
    }
    catch (e) {
        console.log("Error:", e);
    }
}

init().then((r) => {
    console.log('equip.js inited');
    Bridge.invokeEvent('OnJsInited', 'equip');
})

function getCurEquips() {
    console.log('start getCurEquips');
    var eMap = {};
    $('.sr-only.label.label-danger.equip-off').each(function () {
        var e = {};//装备对象
        console.log($(this));
        //console.log($(this).prev());
        e.etype = $(this).data('type');
        console.log(e.etype);
        var equipItem = $(this).prev();
        console.log(equipItem);
        console.log(equipItem.text());
        e.equipName = equipItem.text().replace('【', '').replace('】', '');
        e.etypeName = equipItem.prev().text().replace('：', '');
        console.log(e);
        eMap[e.etype] = e;
    });

    return eMap;
}

function getPackageEquips() {
    console.log('start getPackageEquips');
    var eMap = {};
    $('.sr-only.label.label-danger.equip-off').each(function () {
        var e = {};//装备对象
        console.log($(this));
        //console.log($(this).prev());
        e.etype = $(this).data('type');
        console.log(e.etype);
        var equipItem = $(this).prev();
        console.log(equipItem);
        console.log(equipItem.text());
        e.equipName = equipItem.text().replace('【', '').replace('】', '');
        e.etypeName = equipItem.prev().text().replace('：', '');
        console.log(e);
        eMap[e.etype] = e;
    });
    return eMap;
}