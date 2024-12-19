//发送POST消息
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
//将表单数据合并进数据对象
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
//根据职业，等级，装备栏位获取配置的装备名称
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
