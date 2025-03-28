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
        getRuneCount();
        Bridge.invokeEvent('OnJsInited', 'inlay');

    })

    let nextRuneArr = [];//记录接下来要插的符文用于判断能不能做

    function makeArtifact(data) {
        if (isEnd()) {
            return
        }
        var name = data.name;//神器名字
        var target = map[name];
        if (target == undefined) {
            alert("底子没有这个神器");
        } debugger
        var nextRune = getCurRune(name);
        if (!checkRuneEnough(name, nextRune)) {

            setTimeout(() => {
                Bridge.invokeEvent("OnJsInited", 'inlay');
            }, 2000);
            return -1;

        }
        insertRune(nextRune);
        return 1;
    }
    //整体检查符文是否够用
    function isRuneEnough(data) {
        debugger
        var isEnough = true;
        var s = JSON.stringify(userRuneMap);
        var runeMapCopy = JSON.parse(s);
        var result = {};
        var targetRuneArr = map[data.name];
        for (var i = 0; i < targetRuneArr.length; i++) {
            var rune = targetRuneArr[i];
            runeMapCopy[rune * 1]--;
            if (runeMapCopy[rune * 1] < 0) {
                isEnough = false;
                break;
            }
        }
        return isEnough;
    }

    function getRuneMap(data) {
        var name = data.name;
        var targetRuneArr = map[name];
        var r = {};
        for (let i = 0; i < targetRuneArr.length; i++) {
            var num = targetRuneArr[i] * 1;
            r[num] = r[num] ? ++r[num] : 1;
        }
        return r;
    }

    function checkRuneEnough(name, nextRune) {
        var targetRuneArr = map[name];
        var index = targetRuneArr.indexOf(nextRune);
        var isEnough = true;
        for (var i = index; i < targetRuneArr.length; i++) {
            var rune = targetRuneArr[i];
            userRuneMap[rune * 1]--;
            if (userRuneMap[rune * 1] < 0) {
                isEnough = false
            }
        }
        return isEnough;
    }
    //获取现在要插的符文
    function getCurRune(name) {
        var runeElements = $(".panel-heading:contains('镶嵌物')").toArray();
        if (runeElements.length == 0) return map[name][0];
        //已插符文最后一个
        var LastRune = $(runeElements[runeElements.length - 1]).next().find('.artifact.equip-title').text().match(/\d+/)[0];
        var lastIndex = map[name].indexOf(LastRune)
        return map[name][lastIndex + 1];

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

    async function insertRune(rune) {
        var data = {
            eid2: rune
        };
        await POST_Message("RuneInlay", MERGE_Form(data)).then((r) => {

        }).catch((r, status, xhr) => {

        });
    }

    let map = {};//记录神器名和他的符文顺序


    function readData() {
        $('.table-condensed tr').each(function (index, item) {
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

    let userRuneMap = {};//存用户符文数量
    function getRuneCount() {
        $('.col-xs-12.col-sm-4.col-md-3.equip-container').each(function (index, item) {

            var $pElement = $(this).find('p:first'); // 获取当前容器下的第一个 <p> 元素

            var runeName = $(this).find('p:first .equip-name .artifact:nth-child(2)').text().trim(); // 获取符文名称的第二个 span

            var currentRuneCount = parseInt($(this).find('p:first .artifact').last().text().trim()); // 获取符文数量
            if (index <= 32) {
                userRuneMap[index + 1] = currentRuneCount;
            }
        });
    }

    function getSlotCount() {
        var match = $(".panel-body:contains('当前装备')").text().match(/凹槽\(0\/(?<v>\d+)/);
        return match[1];
    }
    _inlay.getRuneMap = getRuneMap;
    _inlay.isRuneEnough = isRuneEnough;
    _inlay.makeArtifact = makeArtifact;
    _inlay.isEnd = isEnd;
    _inlay.map = map;
    _inlay.userRuneMap = userRuneMap;
    _inlay.insertRune = insertRune;
    _inlay.getSlotCount = getSlotCount;
    

})();