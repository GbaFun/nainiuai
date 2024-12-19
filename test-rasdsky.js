
function doTest() {
    const cfg = GM_getResourceText("data");
    console.log(cfg);
    $.getJSON("https://raw.githubusercontent.com/GbaFun/IdleinfinityTools/refs/heads/main/data.json", function (data) {
        console.log(data);
    });
}
