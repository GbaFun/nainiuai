// ==UserScript==
// @name         过滤工具
// @version      1.0
// @description
// @author       RasdSky
// @match        https://www.idleinfinity.cn/Config/Query?*
// @grant        none
// @license MIT
// ==/UserScript==

/*
    power   //品质
    {
        ""      //全部
        "0"     //普通
        "1"     //魔法
        "2"     //稀有
        "3"     //套装
        "4"     //传奇
    }
    type    //类型
    {
        ""              //全部
        "1"             //帽子
        "2"             //衣服
        4295458816      //元素武器
    }
*/
/*
{"power":"3","type":"4295458816","values":[{"name":"EquipLevel","type":1,"value":"1"}]} //套装-
*/

(function () {
    var btnDiv = $('div.panel-footer');
    var importButton = $('<a class="btn btn-xs btn-success config-default-apply" href="#" role="button">一键导入</a>');
    importButton.click(OnClickImportBtn);
    btnDiv.append(importButton);

    //#region func
    function OnClickImportBtn() {
        var tab = $('table:first');
        console.log(tab);
        // console.log(trs.childern().childern());
        var trs = $('table:first tr');
        console.log(trs);
        var trNum = trs.length;
        console.log(trNum);
        if (trNum > 1) {
            alert("需要先清空当前过滤，是否现在清空！", function () {
                console.log("确认清空");
                CleraAll();
            });
        }
    }

    //从配置导入过滤
    function ImportFromConfig() {
        if (CheckEmpty()) {

        }
    }

    //尝试从配置导入过滤
    function CheckEmpty() {

    }

    function GetFilterData(name, type, value) {
        var data = { "power": "", "type": "", "values": [{ "name": name, "type": type, "valueStr": value }] }
        return data;
    }

    //添加过滤
    function AddFiltration(fdata) {
        $.ajax({
            url: "ConfigApply",
            type: "post",
            data: {
                cid: $("#cid").val(),
                __RequestVerificationToken: $("[name='__RequestVerificationToken']").val(),
                filter: fdata
            },
            dataType: "html",

            success: function (result) {
                // callback();
                location.reload();
            },

            error: function (request, state, ex) {
                console.log(ex)
            }
        });
    }
    //清空过滤
    function CleraAll() {
        $.ajax({
            url: "ConfigClear",
            type: "post",
            data: {
                cid: $("#cid").val(),
                __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
            },
            dataType: "html",

            success: function (result) {
                // callback();
                location.reload();
            },

            error: function (request, state, ex) {
                console.log(ex)
            }
        });
    }
    //#endregion
})();