
;class Character {
    //当前角色id
    cid = 0;

    constructor() {
        this.initCurrentChar();
    }

    initCurrentChar() {
        let localtion = $("a:contains('消息')")[0].href;//消息中有当前id
        let url = new URL(localtion);
        let urlParams = new URLSearchParams(url.search);
        let id = urlParams.get("id");
        this.cid = id;


    }
}
//
var _char = new Character();