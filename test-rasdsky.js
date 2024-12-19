
function doTest() {
    var EquipJson = null;

    var data = MERGE_Form({
        eid: 1, rune: 33, count: 20,
    });
    console.log(data);

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

    // POST_Message("EquipOn", MERGE_Form({ eid: 1 }), "html", 0, function (result) { }, function (request, state, ex) { });

    function GET_JSON_EquipName(_job, _lv, _equipType, _onGet) {
        var job = _job;
        var level = _lv;
        if (EquipJson == null) {
            // https://raw.githubusercontent.com/GbaFun/IdleinfinityTools/refs/heads/main/data.json
            $.getJSON("https://raw.githubusercontent.com/GbaFun/IdleinfinityTools/refs/heads/main/data.json", function (data) {
                if (!$.isEmptyObject(data)) {
                    EquipJson = data[0];
                    var cfg = EquipJson[job];
                    if (cfg != undefined) {
                        $.each(cfg, function (infoIndex, info2) {
                            if (info2.Lv.min <= level && level < info2.Lv.max) {
                                _onGet(info2[_equipType]);
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
                        _onGet(info2[_equipType]);
                    }
                })
            }
        }
    }

    //临时库接口
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
                    console.log(result);
                }
            });
        }, _delay);
    }
    function MERGE_Form(_data) {
        var data = {};
        var form = $("#form")[0];
        $.each(form, function (infoIndex, info) {
            // console.log("Name = " + info.name + " -- Id = " + info.id + " -- Value = " + info.value);
            data[info.name] = info.value;
        });
        $.each(_data, function (infoIndex, info) {
            // console.log("Name = " + infoIndex + " -- Id = " + infoIndex + " -- Value = " + info);
            data[infoIndex] = info;
        });
        return data;
    }
}
