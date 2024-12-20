
//#region  INIT 
function addScriptToHead(src) {
    const script = document.createElement('script');
    script.src = src;
    script.type = 'text/javascript';
    document.head.appendChild(script);
    console.log(document.head);
}

function getLibrary(_timeout = 5000) {
    return new Promise((resolve, reject) => {
        const _url = "https://raw.githubusercontent.com/GbaFun/IdleinfinityTools/refs/heads/main/IdleUtils.js"
        const json_url = "https://raw.githubusercontent.com/GbaFun/IdleinfinityTools/refs/heads/main/data.json";
        $.ajax({
            url: _url,
            timeout: _timeout,
            dataType: 'text',
            success: function (data) {
                // console.log(data);
                resolve(data);
            },
            error: function (xhr, status, error) {
                // 请求失败时的操作
                // console.log('从(' + _url + ')获取JS脚本失败!');
                reject(_url, xhr, status, error);
            }
        });
    });
}

//#endregion

// 示例调用
// addScriptToHead('path/to/your/script.js');
function doTest() {
    getLibrary().then(data => {
        // console.log(data);
        addScriptToHead(data);
        async function asyncLanuch() {
            doLogic();
        }
        asyncLanuch();
    }).catch((_url, xhr, status, error) => {
        alert('从(' + _url + ')获取数据失败!', function () { location.reload(); });
    });
}

function doLogic() {
    var data = MERGE_Form({
        eid: 1, rune: 33, count: 20,
    });
    console.log(data);
}
// var data = MERGE_Form({
//     eid: 1, rune: 33, count: 20,
// });
// console.log(data);

// GET_EquipName("死灵", 7, "主手", function (equipName) {
//     console.log(equipName)
// });
// GET_EquipName("死灵", 14, "戒指1", function (equipName) {
//     console.log(equipName)
// });
// GET_EquipName("武僧", 4, "头盔", function (equipName) {
//     console.log(equipName)
// });
// GET_EquipName("武僧", 11, "靴子", function (equipName) {
//     console.log(equipName)
// });

