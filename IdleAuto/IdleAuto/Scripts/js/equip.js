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

async function getCurEquips() {
    console.log('start getCurEquips');
    var eMap = {};
    $('.sr-only.label.label-danger.equip-off').each(function () {
        var sortid = $(this).data('type');
        var __equip = $(this).prev();
        var id = __equip.data('id');
        var quality = __equip.data('type');
        var equipContent = $(this).parent().next();
        var content = equipContent.text();
        var e = getEquipInfo(id, sortid, quality, content);
        eMap[e.esort] = e;
        console.log(e);

    });
    var isBug = false
    for (var key in eMap) {
        if (eMap[key].eid == null) {
            isBug = true;
        }
    }
    if (isBug) {
        debugger;
        console.log(eMap);
        //dom没渲染？
        await sleep(1000);
        eMap = getCurEquips();

    }
    return eMap;
}

function getPackageEquips() {
    console.log('start getPackageEquips');
    var eMap = {};
    var box = $('.panel-body.equip-bag')[0];
    $(box).children().each(function () {
        var equipItem = $(this).find('span:first');
        var id = equipItem.data('id');
        //var quality = equipItem.data('type');
        var equipContent = $(`.equip-content-container`).find(`[data-id="${id}"]`);
        var classname = equipContent.find('p.equip-title:first').attr('class');
        console.log(classname);
        var quality = classname.split(' ')[0];
        var content = equipContent.text();
        var e = getEquipInfo(id, 999, quality, content);
        eMap[e.eid] = e;
    });
    return eMap;
}

function getEquipInfo(eid, sortid, quality, content) {
    console.log("getEquipInfo");
    var e = {};
    e.eid = eid;
    e.esort = sortid;
    e.quality = quality;
    content = content.replace(/^\s*\n/gm, "")
    content = content.replace(/^\s*/gm, "")
    content = content.replace(/^\t*/gm, "")
    e.content = content;
    var sc = content.split('\n');
    var name = sc[0].match(/(.*)★{0,1}\(\d*\)/);
    var baseName = name[1];

    if (sc[1].includes("可以作为镶嵌物")) {
        baseName = "珠宝";
    }
    else if (sc[2].includes("可以作为镶嵌物")) {
        baseName = "珠宝";
    }
    else if (name[1].includes("秘境")) {
        baseName = "秘境";
    }
    else if (name[1].includes("药水") || name[1].includes("宝箱") || name[1].includes("改名卡")) {
        baseName = "道具";
    }
    else if (quality == "set" || quality == "unique" || quality == "artifact") {
        if (sc[1] != "已绑定") baseName = sc[1];
        else baseName = sc[2];
    }
    else {
        var sbname = name[1].replace("太古", "").replace("无形", "");
        if (sbname.includes("的")) {
            var sc2 = sbname.split("的");
            baseName = sc2[sc2.length - 1];
        }
        else {
            var sc2 = sbname.split("之");
            if (sc2.length > 1) {
                if (sc2[sc2.length - 1].length <= 1) {
                    var c = sc2[sc2.length - 1];
                    if (c == "斧" || c == "矛" || c == "叉" || c == "爪")
                        baseName = sc2[sc2.length - 1];
                    else {
                        baseName = sc2[sc2.length - 2] + "之" + sc2[sc2.length - 1];
                    }
                }
                else {
                    var c1 = sc2[sc2.length - 2];
                    var c2 = sc2[sc2.length - 1];
                    if (c2 == "法珠" && c1 == "鹰") {
                        baseName = "鹰之法珠";
                    }
                    else
                        baseName = sc2[sc2.length - 1];
                }
            }
        }
    }

    if (baseName.includes("太古")) baseName = baseName.replace("太古", "");
    if (baseName.includes("无形")) baseName = baseName.replace("无形", "");

    e.equipBaseName = baseName;
    e.equipName = name[1];
    e.isPerfect = sc[0].includes('★');
    e.isLocal = sc[1].includes("已绑定");
    return e;
}

function getRepositoryEquips() {
    console.log('start getRepositoryEquips');
    var eMap = {};
    var box = $('.panel-body.equip-box')[0];
    $(box).children().each(function () {
        var equipItem = $(this).find('span:first');
        var id = equipItem.data('id');
        var equipContent = $(`.equip-content-container`).find(`[data-id="${id}"]`);
        var classname = equipContent.find('p.equip-title:first').attr('class');
        var quality = classname.split(' ')[0];
        var content = equipContent.text();
        var e = getEquipInfo(id, 999, quality, content);
        eMap[e.eid] = e;
    });
    return eMap;
}
function getPackageEquips() {
    console.log('start getPackageEquips');
    var eMap = {};
    var box = $('.panel-body.equip-bag')[0];
    $(box).children().each(function () {
        var equipItem = $(this).find('span:first');
        var id = equipItem.data('id');
        var equipContent = $(`.equip-content-container`).find(`[data-id="${id}"]`);
        var classname = equipContent.find('p.equip-title:first').attr('class');
        var quality = classname.split(' ')[0];
        var content = equipContent.text();
        var e = getEquipInfo(id, 999, quality, content);
        eMap[e.eid] = e;
    });
    return eMap;
}

function packageNext() {
    var i = $('.panel-body.equip-bag:first').next().find('a:contains("下页")');
    if (i.length == 0) {
        setTimeout(() => {
            Bridge.invokeEvent('OnJsInited', 'equip');
        }, 1500)
        return false;
    }
    else {
        i[0].click();
        return true;
    }
}

function repositoryEquipsCount() {
    console.log('start repositoryEquipsCount');
    var count = 0;
    var i = $('.panel-heading:contains("储藏箱")');
    if (i.length > 0) {
        var txt = i[0].outerText;
        var match2 = txt.match(/\d+/gm);
        count = parseInt(match2[0]);
    }

    console.log(`储藏箱装备总量：${count}`);
    return count;
}
function packageEquipsCount() {
    console.log('start packageEquipsCount');
    var count = 0;
    var i = $('.panel-heading:contains("背包")');
    if (i.length > 0) {
        var txt = i[0].outerText;
        var match2 = txt.match(/\d+/gm);
        count = parseInt(match2[0]);
    }

    console.log(`背包装备总量：${count}`);
    return count;
}

function packageHasEquips() {
    console.log('start packageHasEquips');
    var bag = $('.panel-body.equip-bag');
    var count = 0;
    if (bag == undefined || bag.length == 0) return 0;
    var box = $('.panel-body.equip-bag')[0];
    if (box == undefined) return 0;
    count = $(box).children().length;
    console.log(count);
    return count;
}
function repositoryNext() {
    var i = $('.panel-body.equip-box:first').next().find('a:contains("下页")');
    if (i.length == 0) {
        //location.reload();
        setTimeout(() => {
            Bridge.invokeEvent('OnJsInited', 'equip');
        }, 1500)
        return false;
    }
    else {
        i[0].click();
        return true;
    }
}


function repositoryPre() {
    var i = $('.panel-body.equip-box:first').next().find('a:contains("上页")');
    if (i.length == 0) {
        setTimeout(() => {
            Bridge.invokeEvent('OnJsInited', 'equip');
        }, 1500)
        return false;
    }
    else {
        i[0].click();
        return true;
    }
}

function equipOn(cid, eid) {
    console.log('start equipOn');
    var data = MERGE_Form({
        cid: cid,
        eid: eid,
    });
    POST_Message("EquipOn", data, "post", 1000)
        .then(r => {
            console.log("EquipOn success");
            //reloadPage();
        })
        .catch(r => {
            console.log(r);
            //reloadPage();
        });
}
function equipOff(cid, etype) {
    console.log('start equipOff');
    var data = MERGE_Form({
        cid: cid,
        cet: etype,
    });
    POST_Message("EquipOff", data, "post", 1000)
        .then(r => {
            console.log("EquipOff success");
            reloadPage();
        })
        .catch(r => {
            console.log(r);
            reloadPage();
        });
}
function equipStorage(cid) {
    console.log('start equipStorage');
    var data = MERGE_Form({
        cid: cid,
    });
    console.log(data);
    POST_Message("EquipStoreAll", data, "post", 1000)
        .then(r => {
            console.log("equipStorage success");
            //reloadPage();
        })
        .catch(r => {
            console.log(r);
            //reloadPage();
        });
}
function equipClear(cid, eids) {
    console.log('start equipClear');
    var data = MERGE_Form({
        cid: cid,
        eidsbox: eids.replace('-', ','),
    });
    console.log(data);
    POST_Message("EquipSellBoxAll", data, "post", 1000)
        .then(r => {
            console.log("equipClear success");
            //reloadPage();
        })
        .catch(r => {
            console.log(r);
            //reloadPage();
        });
}

//仓库翻到对应category
function jumpToCategory(c) {
    $($($(".panel-heading")[2]).find("ul")[1]).find("li:contains('" + c + "') a")[0].click();
}
async function reloadPage() {
    //await sleep(1000);
    // location.reload();
}

function equipTrade(cid, eid, name) {
    console.log('start equipTrade');
    var data = MERGE_Form({
        cid: cid,
        eid: eid,
        cname: name
    });
    console.log(data);
    POST_Message("EquipTrade", data, "post", 1000)
        .then(r => {
            console.log("EquipTrade success");
        })
        .catch(r => {
            console.log(r);
        });
}