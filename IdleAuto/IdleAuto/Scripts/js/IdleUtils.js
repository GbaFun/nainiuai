///--json配置文件地址 
//const json_url = "https://raw.githubusercontent.com/GbaFun/IdleinfinityTools/refs/heads/main/data.json";

//$(".bg-canvas").remove()
function Post(_url, _data, _dataType) {
    return new Promise((resolve, reject) => {
        $.ajax({
            url: _url,
            type: "post",
            data: _data,
            dataType: _dataType,

            success: function (result, status, xhr) {
                resolve({ result: result, status: status, xhr: xhr });
            },

            error: function (request, state, ex) {
                reject(request);
            }
        });
    });
}

async function FetchPost(_url, _data, needReload = true) {
    const formData = new FormData();
    for (const key in _data) {
        formData.append(key, _data[key]);
    }

    return new Promise((resolve, reject) => {
        fetch(_url, {
            method: "POST",
            redirect: 'manual',
            body: formData,
        })
            .then(async response => { // 将回调声明为async函数
                debugger
                if (response.type === "opaqueredirect") {
                    needReload && location.reload();
                    resolve(response)
                    return;
                }
                if (response.status === 500) {
                    try {
                        debugger
                        const str = await getResponseText(response.body);
                        Bridge.invokeEvent("OnPostFailed", str, JSON.stringify(_data));
                        console.log("500error");
                        console.log(str);
                        reject({ responseText: str });
                    } catch (error) {
                        reject(error);
                    }
                } else {
                    const str = await getResponseText(response.body);
                    response.html = str;
                    resolve(response);
                }
            })
            .catch(err => {
                //console.error(err);
                reject(err);
            });
    });
}

async function getResponseText(stream) {
    const reader = stream.getReader();
    let accumulatedString = '';

    try {
        while (true) {
            const { done, value } = await reader.read();
            if (done) break;
            accumulatedString += new TextDecoder().decode(value);
        }
        return accumulatedString;
    } finally {
        reader.releaseLock(); // 确保释放Reader锁
    }
}


async function getResponseText(stream) {
    let accumulatedString = '';

    // 假设 stream 是你的 ReadableStream 对象
    const reader = stream.getReader();

    function readStream() {
        return reader.read().then(({ done, value }) => {
            if (done) {
                return accumulatedString; // 返回累积的字符串
            }
            accumulatedString += new TextDecoder().decode(value); // 将新的数据块解码并添加到累积字符串中
            return readStream(); // 递归读取直到流结束
        });
    }

    await readStream();
    
    return accumulatedString;
    
}

//利用promise实现优雅的暂停
function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

//异步
async function POST_Message(url, data, needReload = true, timeout = 1000) {
    console.log('Start POST Message');
    await sleep(timeout);
    console.log(timeout / 1000 + "秒后")
    return FetchPost(url, data,needReload)

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

function deepMerge(target, ...sources) {
    for (let source of sources) {
        for (let key in source) {
            if (source.hasOwnProperty(key)) {
                if (typeof target[key] === 'object' && typeof source[key] === 'object') {
                    deepMerge(target[key], source[key]);
                } else {
                    target[key] = source[key];
                }
            }
        }
    }
    return target;
}
//保存对象到本地缓存，有则合并,无则直接新增
function saveMergeStatus(obj, key) {
    var localObj = localStorage.getItem(key);
    localObj = JSON.parse(localObj);
    if (!!!localObj) {
        var str = JSON.stringify(obj);
        localStorage.setItem(key, str);
    }
    else {

        var t = deepMerge(localObj, obj);
        var saveStr = JSON.stringify(t);
        localStorage.setItem(key, saveStr);
    }
}
