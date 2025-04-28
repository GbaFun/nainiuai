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

    function getEquipContent() {
        return $(".panel-body .panel.panel-inverse").text()
    }
    _inlay.getRuneMap = getRuneMap;
    _inlay.isRuneEnough = isRuneEnough;
    _inlay.makeArtifact = makeArtifact;
    _inlay.isEnd = isEnd;
    _inlay.map = map;
    _inlay.userRuneMap = userRuneMap;
    _inlay.insertRune = insertRune;
    _inlay.getSlotCount = getSlotCount;
    _inlay.getEquipContent = getEquipContent;
    

})();

// ==UserScript==
// @name         Idle Infinity - 一键做神器
// @version      0.4
// @description  一键神器，记得保证自己有足够的符文！！！
// @author       浮世
// @match        https://www.idleinfinity.cn/Equipment/Inlay?*
// @grant        none
// ==/UserScript==

(function () {
    addColumnWithButton();
    // 从 URL 参数中获取 id
    function getUrlParameter(name) {
        name = name.replace(/[\[]/, '\\[').replace(/[\]]/, '\\]');
        var regex = new RegExp('[\\?&]' + name + '=([^&#]*)');
        var results = regex.exec(location.search);
        return results === null ? '' : decodeURIComponent(results[1].replace(/\+/g, ' '));
    }

    // 获取当前页面的 id
    var id = getUrlParameter('id');
    var eid = getUrlParameter('eid');

    // 点击按钮后的事件处理程序
    $(document).on('click', '.btn-default', function () {
        // 获取当前行的所有 <span class="artifact"> 元素的文本内容中的数字部分
        var artifacts = $(this).closest('tr').find('.equip-name .artifact').map(function () {
            // 使用正则表达式匹配文本中的数字部分
            var match = $(this).text().match(/\d+/);
            // 如果匹配到数字则返回，否则返回 undefined
            return match ? match[0] : undefined;
        }).get().filter(function (item) {
            // 过滤掉 undefined 和空字符串
            return item !== undefined && item.trim() !== '';
        });

        // 获取 __RequestVerificationToken 的值
        var requestVerificationToken = $('input[name="__RequestVerificationToken"]').val();

        // 循环发送同步请求
        var requestsCompleted = 0;
        artifacts.forEach(function (eid2, index) {
            // 添加延迟
            setTimeout(function () {
                $.ajax({
                    type: 'POST',
                    url: 'https://www.idleinfinity.cn/Equipment/RuneInlay',
                    data: {
                        cid: id,
                        eid: eid,
                        eid2: eid2,
                        __RequestVerificationToken: requestVerificationToken // 添加 __RequestVerificationToken 参数
                    },
                    async: false, // 设置为同步请求
                    success: function (response) {
                        // 从响应中提取新的 RequestVerificationToken
                        var newToken = $(response).find('input[name="__RequestVerificationToken"]').val();

                        // 如果找到新token，则更新全局变量
                        if (newToken) {
                            requestVerificationToken = newToken;
                        }

                        // 请求成功后的处理
                        requestsCompleted++;
                        // 如果所有请求都完成了，则刷新当前页面
                        if (requestsCompleted === artifacts.length) {
                            // 刷新当前页面
                            window.location.reload();
                        }
                    },
                    error: function (xhr, status, error) {

                        console.error('Error:', error);
                        window.location.reload();

                    }
                });
            }, (index + 1) * 2000); // 延迟时间为请求的序号乘以2秒
        });
    });


})();
// 添加一个新列到表格中，包含一个按钮
function addColumnWithButton() {
    // 选择具有 'table-condensed' 类的表格
    $('.table-condensed tbody tr').each(function () {
        // 在当前行的末尾添加一个新的单元格
        var buttonCell = $('<td></td>');
        var button = $('<a class="btn btn-xs btn-default" role="button">一键神器</a>');

        // 添加按钮到单元格中
        buttonCell.append(button);

        // 将单元格添加到当前行的末尾
        $(this).append(buttonCell);
    });
}