///--json配置文件地址 
const json_url = "https://raw.githubusercontent.com/GbaFun/IdleinfinityTools/refs/heads/main/data.json";


function Post(_url, _data, _dataType) {
    return new Promise((resolve, reject) => {
        $.ajax({
            url: _url,
            type: "post",
            data: _data,
            dataType: _dataType,

            success: function (result) {
                resolve(result);
            },

            error: function (request, state, ex) {
                reject(request);
            }
        });
    });
}

//利用promise实现优雅的暂停
function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

//异步
async function POST_Message(url, data, dataType, timeout = 0) {
    console.log('Start');
    await sleep(timeout);
    console.log(timeout / 1000 + "秒后")
    return Post(url, data, dataType)

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
function GET_EquipName(_job, _lv, _equipType, _onGet) {
    var job = _job;
    var level = _lv;
    if (EquipJson == null) {
        GET_JSON(json_url).then(data => {
            if (!$.isEmptyObject(data)) {
                EquipJson = data[0];
                EachConfig(_onGet);
            }
        }).catch(_url, xhr, status, error)
        {
            alert('从(' + _url + ')获取JSON数据失败!', function () { });
        }
    }
    else {
        EachConfig(asyncReturn);
    }

    function EachConfig(_onGet) {
        var cfg = EquipJson[job];
        if (cfg != undefined) {
            $.each(cfg, function (infoIndex, info2) {
                if (info2.Lv.min <= level && level <= info2.Lv.max) {
                    _onGet(info2);
                    return;
                }
            })
        }
    }
    async function asyncReturn(info) {
        _onGet(info[_equipType]);
    }
}

// 装备配置缓存
let EquipJson;
//
function GET_JSON(_url, _timeout = 500) {
    return new Promise((resolve, reject) => {
        $.ajax({
            url: _url,
            timeout: _timeout,
            dataType: 'json',
            success: function (data) {
                console.log(data);
                resolve(data);
            },
            error: function (xhr, status, error) {
                // 请求失败时的操作
                console.log('从(' + _url + ')获取JSON数据失败!');
                reject(_url, xhr, status, error);
            }
        });
    });
}
