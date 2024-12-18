const autoXuebaiType="autoXuebaiType";
loadAutoXuebai();//在储藏箱执行
reformXuebai();//在改造页执行
//加载一键血白
function loadAutoXuebai() {
    if (location.href.indexOf("Equipment/Query") == -1) {
        return;
    }
    var targetEquip = [];//回复药水所处在数组中的index
    $(".equip-box .equip-name").each(function (index, item) {
        var name = $(this)[0].innerText;
        if (name=="【白热的珠宝】"||name=="血红之珠宝】"||name=="【雄黄之珠宝】") {
            targetEquip.push( item);
        }

    })
    if (targetEquip.length>0) {
        var btn = $('<button>', {
            'class': 'btn btn-default btn-xs dropdown-toggle',
            'text': '一键血白',
            'id': "btnXuebai"
        });
        $(".panel-heading:eq(2) .pull-right").append(btn);
    }
    else{
       localStorage.removeItem(autoXuebaiType);
    }
    $("#btnXuebai").on("click", function () {
        saveMergeStatus("20",autoXuebaiType);
    });
    if(targetEquip.length>0){
        var btn=$(targetEquip[0]).parent().find(".equip-reform");
        btn[0].click();
    }
}

function reformXuebai(){
    if (location.href.indexOf("Equipment/Reform") == -1) {
        return;
    }
    var type=localStorage.getItem(autoXuebaiType);
    if(type){
        _idle.reform(type,function(){
            _idle.reformBackToBag();
        });

    }
    
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