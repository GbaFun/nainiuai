// ==UserScript==
// @name         IdleUtils
// @version      0.3.1
// @description  奶牛助手工具库
// @author       奶牛
// @match        https://www.idleinfinity.cn
// @grant        none
// @license MIT

// ==/UserScript==
const reformWhiteList = [["血红", "转换"], ["雄黄", "转换"], ["雷云风暴", "陨石"], ["支配", "陨石"], ["冰封球", "陨石"]]
const storedCompandDefault = {
    "夏-13#": 1000,
    "多尔-14#": 1000,
    "蓝姆-20#": 1000,
    "普尔-21#": 1000,
    "乌姆-22#": 1000,
    "马尔-23#": 1000,
    "伊司特-24#": 1000,
    "古尔-25#": 1000,
    "伐克斯-26#": 1000,
    "欧姆-27#": 1000,
    "罗-28#": 1000,
    "瑟-29#": 1000,
    "贝-30#": 1000,
    "乔-31#": 1000,
    "查姆-32#": 1000,
    "萨德-33#": 1000,
}
//post消息
function POST_Message(_url, _data, _dataType, _delay, _onSuccess, _onError) {
    setTimeout(function () {
        $.ajax({
            url: _url,
            type: "post",
            data: _data,
            dataType: _dataType,

            success: function (result) {
                _onSuccess(result);
                location.reload();
            },

            error: function (request, state, ex) {
                _onError(request, state, ex);
            }
        });
    }, _delay);
}
