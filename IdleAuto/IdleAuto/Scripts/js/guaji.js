let _guaji = {};
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
        Bridge.invokeEvent('OnJsInited', 'guaji');

    })

    function getData() {
        var rows = $(".char_tr").toArray();
        var arr=[]
        rows.forEach(item => {
            var roleId = $(item).attr("data-id");
            var roleName = $(item).find("td[data-label='角色'] span")[1].innerText;
            var mapLv = $(item).find("td[data-label='地图']").text();
            var battleNum = $(item).find("td[data-label='战斗']").text();
            var successNum = $(item).find("td[data-label='胜利']").text();
            var winningRate = $(item).find("td[data-label='胜率']").text();
            var round = $(item).find("td[data-label='回合']").text();
            var effVal = $(item).find("td[data-label='效率']").text();
            arr.push({
                roleId: roleId,
                roleName: roleName,
                mapLv: mapLv,
                battleNum:battleNum,
                successNum: successNum,
                winningRate: winningRate,
                round: round,
                effVal: effVal
            });
        });
        return arr;
    }

   


    _guaji.getData = getData;
    _map.startExplore = startExplore;

})();