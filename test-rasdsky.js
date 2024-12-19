
function doTest() {
    const job = "死灵";
    const level = 14;
    $.getJSON("https://raw.githubusercontent.com/GbaFun/IdleinfinityTools/refs/heads/main/data.json", function (data) {
        console.log(data);
        $.each(data[0], function (infoIndex, info) {
            var cfg = info[job];
            console.log(cfg);
            if (!cfg.isNaN) {
                $.each(cfg, function (infoIndex, info2) {
                    console.log(info2);
                    if (info2[Lv].min <= level && level < info2[Lv].max) {
                        console.log(info2.主手);
                    }
                })
            }
        })
    });
}
