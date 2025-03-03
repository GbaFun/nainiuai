let _inlay = {};
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
        readData();
        Bridge.invokeEvent('OnJsInited', 'inlay');

    })


    function makeArtifact(data) {
        if (isEnd()) {
            return
        }
        var name = data.name;//神器名字
        var target = map[name];
        if (target == undefined) {
            alert("底子没有这个神器");
        }
        debugger;
        var rune = getCurRune(name);
        insertRune(rune);
  
    }
    //获取现在要插的符文
    function getCurRune(name) {
        var runeElements = $(".panel-heading:contains('镶嵌物')").toArray();
        if (runeElements.length == 0) return map[name][0];
        //已插符文最后一个
        var LastRune = $(runeElements[runeElements.length - 1]).next().find('.artifact.equip-title').text().match(/\d+/)[0];
        debugger
        var lastIndex = map[name].indexOf(LastRune)
        return map[name][lastIndex+1];

    }


    //检测有没有符文 做不了返回-1 做完返回装备id

    function isEnd() {
        var str = $("p:contains('凹槽')").text().match(/\d\/\d/)[0];
        var nums = str.split('/');
        if (nums[0] == nums[1]) {
            return true;
        }
        else {
            return false;
        }
    }

    function insertRune(rune) {
        var data = {
            eid2: rune
        };
        POST_Message("RuneInlay", MERGE_Form(data)).then().catch((r) => {
            location.reload();
        });
    }

    let map = {};//记录神器名和他的符文顺序


    function readData() {
        $('.table-condensed tr').each(function (index,item) {
            var currentRow = $(item);
            var secondTextCenter = currentRow.find('.text-center:eq(1)');
            var artifactElements = secondTextCenter.find('.artifact');
            var artifactName = currentRow.find(".artifact.equip-name").text();
            //console.log(artifactElements);
            let arr = []
            let num = 0
            artifactElements.each(function () {
                if (this.tagName.toLowerCase() === 'p') {
                    var innerText = $(this).text();
                    var match = innerText.match(/(?:\d+)(?=#)/);
                    if (match) {
                        var numbersBeforeHash = match[0];
                        arr.push(numbersBeforeHash)
                    }
                }
            });
            map[artifactName] = arr;

        });
    }
    _inlay.makeArtifact = makeArtifact;
    _inlay.isEnd = isEnd;
    _inlay.map = map;


})();