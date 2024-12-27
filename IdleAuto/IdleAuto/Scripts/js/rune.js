//载入符文插件
function loadStorePlugin() {
    if (location.href.indexOf("Equipment/Material") == -1) {
        return;
    }

    var compandMode = false;
    var autoCompandMode = false;
    var storedRuneCounts = {}; // 存储每个符文项的数量
    var storedCompandCounts = {}; // 合成每个符文项的保留数量

    function showChange() {
        var lastCompandRuneId = parseInt(localStorage.getItem('lastCompandRuneId'));
        //刷新页面前正在运行一键升级符文逻辑，且符文检查没有运行完
        if (!lastCompandRuneId.isNaN && lastCompandRuneId >= 0 && lastCompandRuneId < 33) {
            autoCompandMode = true;
        }

        console.log("上次自动升级符文id：" + lastCompandRuneId + "----是否进入自动升级模式：" + autoCompandMode);
        //一键升级模式
        if (autoCompandMode) {
            var curCompandRuneId = lastCompandRuneId + 1;
            $('.col-xs-12.col-sm-4.col-md-3.equip-container').each(function () {
                var runeName = $(this).find('p:first .equip-name .artifact:nth-child(2)').text().trim(); // 获取符文名称的第二个 span
                var runeCount = parseInt($(this).find('p:first .artifact').last().text().trim()); // 获取符文数量

                var storedCompandCounts = JSON.parse(localStorage.getItem('storedCompandCounts'));

                var storedCount = storedCompandCounts[runeName];
                if (storedCount.isNaN || storedCount == undefined)
                    storedCount = 0;
                var regexResult = runeName.match(/-(\d+)#/);
                var count = 0;
                if (runeCount > storedCount) {
                    count = runeCount - storedCount;
                    count = count - count % 2;
                }

                if (regexResult[1] == curCompandRuneId) {
                    localStorage.setItem('lastCompandRuneId', regexResult[1]);
                    if (count > 1) {
                        //升级符文消息
                        compandStore(regexResult[1], count);
                    }
                    else {
                        showChange();
                    }
                    // return;
                }
            });
            compandMode = true;
        }

        //设置升级保留数量模式
        if (compandMode) {
            // localStorage.setItem('storedCompandCounts', JSON.stringify(""));
            //默认保留数量
            var storedCompandCounts = JSON.parse(localStorage.getItem('storedCompandCounts')) || storedCompandDefault;
            $('.panel-heading:contains("符文") .rasdsky').remove();

            var confirmButton = $('<a class="btn btn-xs btn-default" id="confirmButton">确认</a>');
            var cancleButton = $('<a class="btn btn-xs btn-default" id="cancleButton">退出</a>');

            // 将按钮放入一个 div 中，并添加到 panel-heading 中
            var buttonContainer = $('<div class="pull-right rasdsky"></div>');
            buttonContainer.append(confirmButton);
            buttonContainer.append(cancleButton);

            $('.panel-heading:contains("符文")').append(buttonContainer);
            confirmButton.click(function () {
                $('.col-xs-12.col-sm-4.col-md-3.equip-container').each(function () {
                    var runeName = $(this).find('p:first .equip-name .artifact:nth-child(2)').text().trim(); // 获取符文名称的第二个 span
                    var runeCount = parseInt($(this).find('p:first .artifact').last().text().trim()); // 获取符文数量

                    var _inputElement = $(this).find('.rasdsky-input:first');
                    var cnt = parseInt(_inputElement.val());
                    if (storedCompandCounts.hasOwnProperty(runeName)) {
                        if (!cnt.isNaN && cnt != undefined) {
                            storedCompandCounts[runeName] = cnt;
                        }
                    }
                    else {
                        storedCompandCounts[runeName] = 0;
                    }
                    // var storedCount = storedCompandCounts[runeName];
                    // var regexResult = runeName.match(/-(\d+)#/);
                    // if (runeCount > storedCount) {
                    //     var count = runeCount - storedCount;
                    //     count = count - count % 2;
                    //     compandStore(regexResult[1], count);
                    // }
                });
                localStorage.setItem('storedCompandCounts', JSON.stringify(storedCompandCounts)); // 存储到 localStorage 

                //进入一键升级模式
                autoCompandMode = true;
                localStorage.setItem('lastCompandRuneId', 0);
                showChange();
            });
            cancleButton.click(function () {
                compandMode = false;
                autoCompandMode = false;
                showChange();
            });

            $('.col-xs-12.col-sm-4.col-md-3.equip-container').each(function () {
                var runeName = $(this).find('p:first .equip-name .artifact:nth-child(2)').text().trim(); // 获取符文名称的第二个 span

                var retainCount = 0;
                if (storedCompandCounts.hasOwnProperty(runeName)) {
                    retainCount = storedCompandCounts[runeName];
                    if (retainCount == undefined || retainCount.isNaN)
                        retainCount = 0;
                }
                $(this).find('.rasdsky').remove();

                var t = ($('<span>').text('  保留：').css('color', 'grey'));

                var inputElement = $('<input class="rasdsky-input">').css({
                    "color": "grey",
                    "width": 120,
                    "height": 21,
                });
                inputElement.val(retainCount);
                var p = $('<p class="rasdsky">');
                p.append(t);
                p.append(inputElement);

                $(this).append(p)

            });
        }
        //查看变动数量模式
        else {

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
    }
    showChange();

    // function compandStore(rune, count) {
    //     var t = 1500;
    //     POST_Message("RuneUpgrade", MERGE_Form({
    //         rune: rune,
    //         count: count,
    //     }), "html", t, function (result) {
    //         compandMode = true;
    //         // location.reload();
    //     }, function (request, state, ex) {
    //         // console.log(result)
    //     })
    // }
    function compandStore(rune, count) {
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
}