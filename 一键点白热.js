loadAutoXuebai();
//加载一键血白
function loadAutoXuebai() {
    debugger;
    if (location.href.indexOf("Equipment/Query") == -1) {
        return;
    }
    var equipArr = [];//所有装备的id;
    var targetEquip = {};//回复药水所处在数组中的index
    $(".equip-box .equip-name .unique[data-id]").each(function (index, item) {
        var name = $(this)[0].innerText;
        if (name.indexOf("白热")>-1||name.indexOf("血红")>-1||name.indexOf("雄黄")>-1) {
            targetEquip[index] = item;
        }

    })
    if (countProperties(targetEquip) > 0) {
        var btn = $('<button>', {
            'class': 'btn btn-default btn-xs dropdown-toggle',
            'text': '一键血白',
            'id': "btnXuebai"
        });
        $(".panel-heading:eq(2) .pull-right").append(btn);
    }
    $(".equip-box .equip-use[data-id]").each(function (index, item) {
        let id = $(item).attr("data-id");
        equipArr.push(id);
    });

    $("#btnXuebai").on("click", function () {
        var count = 0;
        var sanMedcineIds = [];


    });

}



function countProperties(obj) {
    let count = 0;
    for (let prop in obj) {
        if (obj.hasOwnProperty(prop)) {
            count++;
        }
    }
    return count;
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
//post消息
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
            }
        });
    }, _delay);
}