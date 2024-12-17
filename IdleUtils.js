// ==UserScript==
// @name         IdleUtils
// @version      0.2
// @description  奶牛助手工具库
// @author       奶牛
// @match        https://www.idleinfinity.cn
// @grant        none
// @license MIT

// ==/UserScript==

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
