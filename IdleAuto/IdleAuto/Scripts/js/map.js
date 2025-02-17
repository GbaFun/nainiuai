// ==UserScript==
// @name         New Userscript
// @namespace    http://tampermonkey.net/
// @version      2025-01-26
// @description  try to take over the world!
// @author       You
// @match        https://www.idleinfinity.cn/Map/Dungeon?*
// @match        https://www.idleinfinity.cn/Battle/InDungeon?*

// @icon         https://www.google.com/s2/favicons?sz=64&domain=idleinfinity.cn
// @grant        none
// ==/UserScript==
let _map = {};
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
        Bridge.invokeEvent('OnJsInited', 'map');

    })

    let map;
    var step = [];
    var visitedStep = {};
    backToMap();
    if (localStorage.getItem("startMap")) {

        setTimeout(() => {
            explore();
        }, 1000)
    }



    async function startExplore() {
       
 
        if (localStorage.getItem("startMap")) {

        }
        else {
            localStorage.setItem("startMap", 1);
            explore();
        }
    }
    async function explore() {
        if (location.href.indexOf("Map/Dungeon") == -1) return;
        if ($("span.boss-left").text() != "1")  {
            //打完了
            localStorage.removeItem("startMap");
            localStorage.removeItem("mapStep");
            exitDungeon()
            await sleep(2000);
            Bridge.invokeEvent('OnSignal', 'DungeonEnd');
            return;

        }
        //地图状态会随着点击变化 重置比较暴力但是方便
        map = Array.from({ length: 20 }, () => Array(20).fill(false));
        step = [];
        let curPos = getCurPos();
        map[curPos[0]][curPos[1]] = true;
        await dfs(curPos[0], curPos[1]);
        await startMove();
        localStorage.setItem("mapStep", JSON.stringify(visitedStep));
        if ($("span.boss-left").text() == "1") {
            //地图状态变化了需要重新递归地图状态
            explore();
        }
    }

    async function exitDungeon() {
        var data = {
            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val(),
            cid: _char.cid
        };
        debugger;
        POST_Message("DungeonExit", data, "post", 1500).then((r) => {

        }).catch((e) => {

        })
    }

    async function backToMap() {
        if (location.href.indexOf("InDungeon") == -1) { return }

        const win = $('.turn').first().text().indexOf('战斗胜利') > 0;
        await sleep(5000)
        var href = $("a:contains('返回')").attr("href");
        if (win) {
            location.href = href;
        }
        else {
            //记录失败次数 
        }
    }



    async function startMove() {

        debugger;
        step = await filterStep(step);
        for (let i = 0; i < step.length; i++) {

            await sleep(1000);
            var block = $(`#${step[i]}`);

            if (block.hasClass("monster") || block.hasClass("boss")) {
                localStorage.setItem("mapStep", JSON.stringify(visitedStep));
            }
            clickBlock(block);
        }
    }

    //上下左右没有mask且没有怪物的格子不用点击
    async function filterStep(steps) {
        var newStep = [];
        for (let i = 0; i < step.length; i++) {
            var index = steps[i];
            var curPos = calPos(index);
            var up = [curPos[0] - 1, curPos[1]]
            var down = [curPos[0] + 1, curPos[1]]
            var left = [curPos[0]  , curPos[1]-1]
            var right = [curPos[0], curPos[1] + 1]
            var curBlock = $(`#${index}`);
            var hasMonster = curBlock.hasClass("monster") || curBlock.hasClass("boss");
            if (!hasMonster && isBlockIgnore(up, down, left, right)) {
                visitedStep[index] = true;
            }
            else {
                newStep.push(index);
            }
        }
        return newStep;
    }

    function isBlockIgnore(up, down, left, right) {
        var u = calBlockIndex(up);
        var d = calBlockIndex(down);
        var l = calBlockIndex(left);
        var r = calBlockIndex(right);
        var arr = [];
        if (up[0] >= 0 && up[0] <= 19) arr.push($(`#${u}`));
        if (down[0] >= 0 && down[0] <= 19) arr.push($(`#${d}`));
        if (left[0] >= 0 && left[0] <= 19) arr.push($(`#${l}`));
        if (right[0] >= 0 && right[0] <= 19) arr.push($(`#${r}`));
        var r = arr.filter(f => { return f.hasClass("mask") });
        debugger;
        if (r.length == 0) return true
        else return false
        
    }
    //利用promise实现优雅的暂停
    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }


    async function dfs(i, j) {
        var s = localStorage.getItem("mapStep");
        if (s) {
            visitedStep = JSON.parse(s);
        }
        //处理点击
        var index = i + j * 20;
        var block = $(`#${index}`);
        if (!map[i][j]) {
            if (!visitedStep[index]) step.push(index);
            map[i][j] = true;
        }
        if (i != 0) {
            //左
            let left = i - 1;
            let nextIndex = left + j * 20;
            if (canExplore(nextIndex, block, 'left') && !map[left][j]) {
                dfs(left, j);
            }

        }
        if (i < 19) {
            let right = i + 1;
            let nextIndex = right + j * 20;
            if (canExplore(nextIndex, block, 'right') && !map[right][j]) {
                dfs(right, j);
            }
        }
        if (j != 0) {
            let up = j - 1;
            let nextIndex = i + up * 20;
            if (canExplore(nextIndex, block, 'up') && !map[i][up]) {
                dfs(i, up);
            }
        }
        if (j < 19) {
            let down = j + 1;
            let nextIndex = i + down * 20;
            if (canExplore(nextIndex, block, 'down') && !map[i][down]) {
                dfs(i, down);
            }
        }
    }

    //获取初始位置
    function getCurPos() {
        var id = $("a.current").attr("id");
        return calPos(id);
    }

    function calPos(id) {
        var j = Math.floor((id * 1) / 20);
        var i = (id * 1) % 20;
        return [i, j];
    }

    function calBlockIndex(arr) {
        return arr[0] + arr[1] * 20;
    }



    // 判断是否可以点击
    function canExplore(i, curBlock, direction) {
        const block = $(`#${i}`);
        if (block.hasClass('mask')) return false;
        if (block.hasClass('monster')) return true;
        if (direction == 'up') {
            return !curBlock.hasClass("top");
        }
        if (direction == "down") {
            return !block.hasClass('top')
        }
        if (direction == 'left') {
            return !curBlock.hasClass("left");
        }
        if (direction == 'right') {
            return !block.hasClass("left");
        }

    }

    function clickBlock(block) {
        const idMatch = location.href.match(/id=(\d+)/i);
        const id = idMatch[1];
        const width = block.width();
        const height = block.height();
        const rect = document.getElementById(block.attr('id')).getBoundingClientRect();
        const x = Math.round(rect.left + width / 3 + (width / 4 * Math.random(id))) + $(window).scrollLeft();
        const y = Math.round(rect.top + height / 3 + (height / 4 * Math.random(id))) + $(window).scrollTop();
        ajaxMove(block, { pageX: x, pageY: y, originalEvent: { isTrusted: true } });
    }

    function ajaxMove(block, a) {
        const f = block;
        var c = f.parent();
        const g = f.attr("id");
        const k = $("#cid").val();
        const td = localStorage.getItem("t");
        if (f.hasClass("monster")) {
            location.href = "/Battle/InDungeon?id=" + k + "&bid=" + g;
        } else {

            var e = [];
            if (0 < a.pageX && 0 < a.pageY && a.hasOwnProperty("originalEvent") && (a.originalEvent.isTrusted || 1 == a.originalEvent.detail)) {
                e = $(c).offset();
                const h = $(c).width();
                c = $(c).height();
                const l = Math.floor(Math.random() * h);
                e = [a.pageX, l, a.pageY, e.left, h - l, e.top, h, Math.floor(Math.random() * c), c]
            }
            a = {
                id: k,
                bid: g,
                m: e,
                t: td,
                __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
            };
            $.ajax({
                url: "MoveTo",
                type: "post",
                data: a,
                dataType: "json",
                success: function (a) {
                    visitedStep[g] = true;

                    $.each(a, function (a, b) {
                        void 0 == b.id && (b.id = 0);
                        a = "";
                        0 == b.d[0] && (a += " top");
                        0 == b.d[1] && (a += " left");
                        if (1 == b.m)
                            $("#" + b.id).addClass(a);
                        else {
                            a += " public";
                            var c = "";
                            0 < b.mlvl && (c += "Lv" + b.mlvl + " " + b.mname, a = a + " monster " + b.mtype);
                            $("#" + b.id).removeClass("mask").addClass(a);
                            "" != c && $("#" + b.id).attr("title", c)
                        }
                    });
                    0 < a.length && ($("#explore").text(parseInt($("#explore").text()) + a.length),
                        $("#not-explore").text(parseInt($("#not-explore").text()) - a.length));
                    $(".current").removeClass("current");
                    f.addClass("current");


                },
                error: function (XMLHttpRequest) {
                    const responseText = XMLHttpRequest.responseText;
                    if (responseText.indexOf('封号') >= 0) {
                        addBlockNum();
                    }

                }
            });
        }
    }


    _map.explore = explore;
    _map.startExplore = startExplore;

})();