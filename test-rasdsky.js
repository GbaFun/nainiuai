
function doTest() {
    init().then((r) => {
        showRuneNumView();
    })
}

//初始化Bridge
async function init() {
    try {
        await CefSharp.BindObjectAsync("Bridge");
    }
    catch (e) {
        console.log("Error:", e);
    }
}

function showRuneNumView() {
    $('.panel-heading:contains("符文") .rasdsky').remove();
    var storedRuneCounts = JSON.parse(localStorage.getItem('storedRuneCounts')) || {};
    console.log(storedRuneCounts);
    var storedTime = localStorage.getItem('storedTime');

    // 将按钮放入一个 div 中，并添加到 panel-heading 中
    var buttonContainer2 = $('<div class="pull-right rasdsky"></div>');

    // 创建展示存储时间的元素
    var timeDiv = $('<div class="pull-left rasdsky"></div>');
    var timeElement = $('<p>').text('存储时间: ' + storedTime);
    timeDiv.append(timeElement);
    // 创建存储符文数量的按钮
    var saveButton = $('<a class="btn btn-xs btn-default" id="saveButton">存储</a>');
    // 创建升级符文的按钮
    var compandButton = $('<a class="pull-right btn btn-xs btn-default" id="compandButton">升级</a>');

    buttonContainer2.append(timeDiv);
    buttonContainer2.append(saveButton);
    buttonContainer2.append(compandButton);

    $('.panel-heading:contains("符文")').append(buttonContainer2);

    console.log("添加按钮成功");
    saveButton.click(function () {
        var storedRuneCounts = {};

        $('.col-xs-12.col-sm-4.col-md-3.equip-container').each(function () {

            var runeName = $(this).find('p:first .equip-name .artifact:nth-child(2)').text().trim(); // 获取符文名称的第二个 span

            var runeCount = parseInt($(this).find('p:first .artifact').last().text().trim()); // 获取符文数量
            // 检查解析是否成功，如果是 NaN 或者数量小于等于 20 则跳过不存储
            var regexResult = runeName.match(/-(\d+)#/);
            if (regexResult && parseInt(regexResult[1]) >= 1) {
                // 检查解析是否成功，如果是 NaN 则设为 0
                if (isNaN(runeCount)) {
                    runeCount = 0;
                }
                storedRuneCounts[runeName] = runeCount; // 存储符文数量
            }
        });

        var currentTime = new Date().toLocaleString(); // 获取当前时间
        localStorage.setItem('storedRuneCounts', JSON.stringify(storedRuneCounts)); // 存储到 localStorage
        localStorage.setItem('storedTime', currentTime); // 存储时间到 localStorage
        alert("已存储数据", function () { });
    });

    compandButton.click(function () {
        compandMode = true;
        showChange();
    });

    $('.col-xs-12.col-sm-4.col-md-3.equip-container').each(function () {

        var $pElement = $(this).find('p:first'); // 获取当前容器下的第一个 <p> 元素

        var runeName = $(this).find('p:first .equip-name .artifact:nth-child(2)').text().trim(); // 获取符文名称的第二个 span

        var currentRuneCount = parseInt($(this).find('p:first .artifact').last().text().trim()); // 获取符文数量

        if (storedRuneCounts.hasOwnProperty(runeName)) {
            var storedCount = storedRuneCounts[runeName];
            var changeCount = currentRuneCount - storedCount; // 计算数量变动
            if (changeCount !== undefined) {
                var changeText = '  (' + storedCount + ' -> ' + currentRuneCount + ')'; // 根据变动数量生成对应文本

                $(this).find('.rasdsky').remove();
                // 将变动数量拼接到符文信息的最后，并为 <p> 标签添加对应的样式
                $(this).find('p:first .artifact:last').append($('<span class="rasdsky">').text(changeText).css('color', (changeCount > 0) ? 'red' : (changeCount < 0) ? 'green' : 'white')); // 为 <p> 标签添加颜色样式);)
            }
        }
    });
}

function getRuneNum(rune) {
    $('.col-xs-12.col-sm-4.col-md-3.equip-container').each(function () {
        var name = $(this).find('p:first .equip-name .artifact:nth-child(2)').text().trim(); // 获取符文名称的第二个 span
        var regexResult = runeName.match(/-(\d+)#/);
        if (regexResult == rune) {
            return parseInt($(this).find('p:first .artifact').last().text().trim()); // 获取符文数量
        }
    });
    return 0;
}

function upgradeRune(rune, count) {
    var data = MERGE_Form({
        rune: rune,
        count: count
    });
    POST_Message("RuneUpgrade", data, "html", 2000)
        .then(r => {
            compandMode = true;
            location.reload();
        })
        .catch(r => { console.log(r) });
}

//异步
async function POST_Message(url, data, dataType, timeout = 0) {
    console.log('Start');
    await sleep(timeout);
    console.log(timeout / 1000 + "秒后");
    return Post(url, data, dataType);

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