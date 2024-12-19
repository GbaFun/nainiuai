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