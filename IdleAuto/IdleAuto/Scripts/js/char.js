
; class Character {
    //当前角色id
    cid = 0;

    constructor() {
        this.init().then(() => {
            this.initCurrentChar();
            if (this.cid) Bridge.invokeEvent('OnCharLoaded', this.cid);
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
        let localtion = $("a:contains('消息')")[0].href;//消息中有当前id
        let url = new URL(localtion);
        let urlParams = new URLSearchParams(url.search);
        let id = urlParams.get("id");
        this.cid = id;


    }
   

    getAttribute() {
        if (location.href.indexOf("Character/Detail") == -1) return;
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
}

var _char = new Character();
