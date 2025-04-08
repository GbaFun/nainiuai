let _trade = {};
(function () {
    async function init() {
        try {
            console.log('trade.js start init');
            await CefSharp.BindObjectAsync("Bridge");
        }
        catch (e) {
            console.log("Error:", e);
        }
    }

    init().then((r) => {
        console.log('trade.js inited');
        Bridge.invokeEvent('OnJsInited', 'trade');
    })
    function acceptTrade(tradeId) {
        console.log('start acceptTrade');
        var data = MERGE_Form({
            nid: tradeId
        });
        console.log(data);
        POST_Message("NoticeYes", data, "post", 1000)
            .then(r => {
                debugger;
                console.log("acceptTrade success");
            })
            .catch(r => {
                debugger;
                console.log(r);
            });
    }
    async function acceptAllTrade() {
        var tradeIds = [];
        var notices = $('.notice-content');
        notices.each(function () {
            debugger;
            var notice = $(this);
            var acceptBtn = notice.find('.notice-yes:first');
            var tradeId = acceptBtn.data('id');
            tradeIds.push(tradeId);
        });

        for (var i = 0; i < tradeIds.length; i++) {
            await sleep(1000);
            acceptTrade(tradeIds[i]);
        }

        return true;
    }

    async function getAnyTradeId() {
        var arr = $('.notice-content .notice-yes');
        if (arr.length == 0) return -1;
        return $(arr[0]).attr("data-id");
    }

    _trade.acceptAllTrade = acceptAllTrade;
    _trade.acceptTrade = acceptTrade;
    _trade.getAnyTradeId = getAnyTradeId;

})();


