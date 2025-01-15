
;class Character {
    //当前角色id
    cid = 0;

    constructor() {
        this.init().then(() => {
            this.initCurrentChar();
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

    initAttribute() {
        if (location.href.indexOf("Character/Detail") == -1) return;
        var str = $("#char-str").text();
        var dex = $("#char-dex").text();
        var vit = $("#char-vit").text();
        var eng = $("#char-eng").text();
        var panels = (".panel.panel-inverse");
        var skillPanel = panels[1]
        var attPanel = panels[4]
        var damage
    }
}

var _char = new Character();
