
class Character {
    //当前角色id
    cid = 0;

    constructor() {
        this.init().then(() => {
            this.initCurrentChar();
            if (this.cid > 0) Bridge.invokeEvent('OnCharLoaded', _char.cid);
            
            Bridge.invokeEvent('OnJsInited', 'char');
            Bridge.invokeEvent('OnSignal', 'charReload');
            this.addCloseButton();
        });
    }
    async init() {
        try {
            await CefSharp.BindObjectAsync("Bridge");
        }
        catch (e) {
            console.log("Error:", e);
        }
    }

    initCurrentChar() {
        console.log($("a:contains('消息')")[0]);
        let localtion = $("a:contains('消息')")[0].href;//消息中有当前id
        let url = new URL(localtion);
        let urlParams = new URLSearchParams(url.search);
        let id = urlParams.get("id");
        this.cid = id;


    }


    getAttribute() {
        if (location.href.indexOf("Character/Detail") == -1) return;
        var lvName = $($(".media-body")[0]).find("div")[0].innerText;
        var lv = lvName.match(/\d+/)[0] * 1;
        var job = lvName[lvName.length - 2] + lvName[lvName.length - 1];
        var str = $("#char-str").text();
        var dex = $("#char-dex").text();
        var vit = $("#char-vit").text();
        var eng = $("#char-eng").text();
        var panels = $(".panel.panel-inverse");
        var skillPanel = panels[1]
        var attPanel = panels[4]
        var att = $(attPanel).find("p span:contains('准确率')").next().text();
        var as = $(attPanel).find("p span:contains('攻击速度')").next().text();
        var fcr = $(attPanel).find("p span:contains('施法速度')").next().text();
        //生效的速度类型
        var speedType = $(attPanel).find("p span:contains('施法速度')").next().hasClass('skill') ? "fcr" : "as";
        var crushDamage = $(attPanel).find("p span:contains('压碎')").next().text().match(/\d+/)[0];
        var openWound = $(attPanel).find("p span:contains('撕开')").next().text().match(/\d+/)[0];
        var reduceDef = $(attPanel).find("p span:contains('减目标防御')").next().text().match(/\d+/)[0];
        var isIgnoreDef = $(attPanel).find("p span:contains('无视目标防御')").next().text();
        var obj = {
            roleId: this.cid,
            roleName: $($(".panel-heading")[0]).find(".name").text(),
            lv: lv,
            jobName: job,
            str: str,
            dex: dex,
            vit: vit,
            eng: eng,
            att: att,
            as: as,
            fcr: fcr,
            speedType: speedType,
            crushDamage: crushDamage,
            openWound: openWound,
            reduceDef: reduceDef,
            isIgnoreDef: isIgnoreDef
        }
        return obj;
    }

    getSimpleAttribute() {
        var point = $(".attr-points").first().text();
        if (point == "") point = 0;
        console.log(point);
        var str = $("#char-str");
        var strV = str.text();
        var strAdd = str.data("add");
        var dex = $("#char-dex");
        var dexV = dex.text();
        var dexAdd = dex.data("add");
        var vit = $("#char-vit");
        var vitV = vit.text();
        var vitAdd = vit.data("add");
        var eng = $("#char-eng");
        var engV = eng.text();
        var engAdd = eng.data("add");
        var obj = {
            point: point,
            str: strV,
            strAdd: strAdd,
            dex: dexV,
            dexAdd: dexAdd,
            vit: vitV,
            vitAdd: vitAdd,
            eng: engV,
            engAdd: engAdd
        }
        console.log(obj);
        return obj;
    }
    attributeReset(charid) {
        var data = MERGE_Form({
            cid: charid
        });
        POST_Message("AttributeReset", data, "post", 1500).then((r) => {
            //location.reload();
        }).catch((e) => {
            // location.reload();
        })
    }
    attributeSave(charid, data) {
        console.log("start attributeSave");
        var _data = MERGE_Form({
            id: charid,
            csa: data.strAdd,
            cda: data.dexAdd,
            cva: data.vitAdd,
            cea: data.engAdd
        });
        console.log(_data);
        POST_Message("AttributeSave", _data, "post", 1500).then((r) => {
            // location.reload();
        }).catch((e) => {
            // location.reload();
        })
    }

    //读取人物配置的技能
    getSkillConfig() {
        //两系技能合并一下
        var skillArr = $($(".panel")[0]).find(".skill-name").toArray().concat($($(".panel")[1]).find(".skill-name").toArray());

        //选中的技能
        var groupSkillArr = $($(".panel")[2]).find(".skill-name").toArray();
        var r = {};

        for (var i = 0; i < skillArr.length; i++) {
            var item = skillArr[i];
            var skillName = $(item).find("span")[1].innerText;
            var pointAdd = $(item).attr("data-add") * 1;
            var lvSum = $(item).attr("data-lvl") * 1;


            r[skillName] = {
                lv: pointAdd,
                name: skillName,
                lvSum: lvSum
            };
        }
        return r;

    }

    //获取携带技能
    getSkillGroup() {
        //选中的技能
        var groupSkillArr = $(".panel .panel-heading:contains('技能组')").parent().find(".skill-name").toArray();
        var arr = [];
        for (var i = 0; i < groupSkillArr.length; i++) {
            var item = groupSkillArr[i];
            var name = $(item).text().split(" ")[1];
            arr.push(name);
        }
        return arr;
    }

    getEquipAttachSkill() {
        var groupSkillArr = $(".panel .panel-heading:contains('装备附加技能')").parent().find(".skill-name").toArray();
        var arr = [];
        for (var i = 0; i < groupSkillArr.length; i++) {
            var item = groupSkillArr[i];
            var name = $(item).text().split(" ")[1];
            arr.push(name);
        }
        return arr;

    }

    //读取人物详细页的技能
    getSkillInfo() {
        if (location.href.indexOf("Character/Detail") == -1) return;
        var r = {};

        var arr = $(".skill-container .sr-container").toArray();
        //前五个为主动技能 后三个为组队信息
        for (let i = 0; i < arr.length; i++) {
            var item = arr[i];
            var lvStr = $(item).find(".label.label-success").text();
            var lv = lvStr.match(/\d+/)[0];
            var skillName = $(item).find(".skill-name").text();
            var next = $(item).find(".skill-name").next();
            var isK = false;
            if (next) {
                if (!next.hasClass("skill-pve-key")) {
                    isK = true;
                }
            }
            r[skillName] = {
                type: "主动",
                lv: lv,
                name: skillName,
                isK: isK
            };

        }

        var skillContainer = $($(".skill-container")[1]);
        var passiveSkillLvArr = skillContainer.find("p span.label.label-info").toArray();
        var passiveSkillNameArr = skillContainer.find("p span.skill-name").toArray();
        passiveSkillLvArr.forEach((item, index) => {
            var lvStr = item.innerText;
            var lv = lvStr.match(/\d+/)[0];
            var skillName = passiveSkillNameArr[index].innerText;
            r[skillName] = {
                type: skillName.indexOf("光环") > -1 ? "光环" : "被动",
                lv: lv,
                name: skillName
            };
        })
        return r;
    }
    //保存技能 sid为技能id|点数逗号拼接
    skillSave(data) {

        var data = MERGE_Form({
            sid: data.sid,
            cid: _char.cid
        });
        console.log(data);
        POST_Message("SkillSave", data, "post", 1500).then((r) => {
            console.log("技能加点成功")
        }).catch((e) => {
            debugger
            // location.reload();
            console.log("技能加点失败")
        })
    }

    skillReset() {

        var data = MERGE_Form({
            cid: _char.cid
        });
        POST_Message("SkillReset", data, "post", 1500).then((r) => {

        }).catch((e) => {
            // location.reload();
        })
    }

    skillGroupSave(data) {

        var data = MERGE_Form({
            cid: _char.cid,
            sid: data.sid
        });
        POST_Message("SkillGroupSave", data, "post", 1500).then((r) => {
            debugger
            //location.reload();
        }).catch((e) => {
            // location.reload();
            debugger
        })
    }
    skillKeySave(sid) {

        var data = MERGE_Form({
            cid: _char.cid,
            sid: sid
        });
        POST_Message("SetKeySkill", data, "post", 1500).then((r) => {
            // location.reload();
        }).catch((e) => {
            //location.reload();
        })
    }
    //是否设置了Key
    hasKey() {
        return $("span.label.label-danger:contains('K')").not('.sr-only, .skill-pve-key').length == 1;
    }

    mapSwitch(ml) {
        var data = MERGE_Form({
            cid: _char.cid,
            ml: ml
        })
        POST_Message("MapSwitch", data, "post", 1500).then((r) => {
            debugger
        }).catch((e) => {
            debugger
            var isNeedDungeon = e.responseText.indexOf('请先击杀上一层秘境BOSS') > -1
            var data = { isSuccess: true, isNeedDungeon: false };
            if (isNeedDungeon) {
                data.isSuccess = false;
                data.isNeedDungeon = true;
                Bridge.invokeEvent('OnDungeonRequired', data);//触发秘境异常不会刷页面 如果页面刷新了说明切图成功
            }

            location.reload();
        })
    }

    copyConfig(data) {
        var data = MERGE_Form({
            cid: _char.cid,
            cname: data.name
        })
        POST_Message("ConfigCopy", data, "post", 1500).then((r) => {

        }).catch((e) => {
            //location.reload();
        })
    }



    getCurMapLv() {
        return $(".panel-heading")[0].innerText.match(/\d+/)[0]
    }

    hasNotice() {
        var num = $(".navbar-fixed-top a:contains('消息') .badge").text() * 1;
        return num > 0;
    }

    getSan() {
        return $("span:contains('SAN：')").next().text() * 1;
    }

    //添加关闭本页的按钮
    addCloseButton() {
        var container = $("#navbar");
        var btn = $('<button>', {
            'class': 'btn btn-danger btn-xs',
            'text': '关闭当前页',
            'style': "position:absolute;left:25%;top:15px;",

            click: () => {
                Bridge.closeWin()
            }
        });
        container.append(btn);
    }
    isBossDone() {
        const regex = /([0-9]|[1-9][0-9])%-\(([0-5])\/5\)/;
        var str = $(".panel-heading").text().trim();
        return regex.test(str);
    }
}

var _char = new Character();
