
function doTest() {
    var EquipJson = null;

    GET_JSON_EquipName("死灵", 7, "主手", function (equipName) {
        console.log(equipName)
    });
    GET_JSON_EquipName("死灵", 14, "戒指1", function (equipName) {
        console.log(equipName)
    });
    GET_JSON_EquipName("武僧", 4, "头盔", function (equipName) {
        console.log(equipName)
    });
    GET_JSON_EquipName("武僧", 11, "靴子", function (equipName) {
        console.log(equipName)
    });

    function GET_JSON_EquipName(_job, _lv, _equipType, _onGet) {
        var job = _job;
        var level = _lv;
        if (EquipJson == null) {
            $.getJSON("https://raw.githubusercontent.com/GbaFun/IdleinfinityTools/refs/heads/main/data.json", function (data) {
                if (!$.isEmptyObject(data)) {
                    EquipJson = data[0];
                    var cfg = EquipJson[job];
                    if (cfg != undefined) {
                        $.each(cfg, function (infoIndex, info2) {
                            if (info2.Lv.min <= level && level < info2.Lv.max) {
                                _onGet(info2);
                            }
                        })
                    }
                }
            });
        }
        else {
            var cfg = EquipJson[job];
            if (cfg != undefined) {
                $.each(cfg, function (infoIndex, info2) {
                    if (info2.Lv.min <= level && level <= info2.Lv.max) {
                        _onGet(info2);
                    }
                })
            }
        }
    }
}
