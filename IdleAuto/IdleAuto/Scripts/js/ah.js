// ==UserScript==
// @name         ah
// @version      0.2
// @description  显示拍卖行符文价格|扫拍
// @author       奶牛
// @match         https://www.idleinfinity.cn/Auction/Query?*
// ==/UserScript==

let ah = {};
(function () {

    const EquipToBuy = "EquipToBuy";
    const ScanAhConfig = "ScanAhConfig";
    //初始化Bridge
    async function init() {
        try {
            await CefSharp.BindObjectAsync("Bridge");
        }
        catch (e) {
            console.log("Error:", e);
        }
    }
    var dataMap = {}//存储dataid对应装备的数据 

    init().then(async () => {
        await loadPriceSuffix();
        await Bridge.invokeEvent('OnScanAh');
    })


    //给每个拍卖物品加入后缀
    async function loadPriceSuffix() {
        if (location.href.indexOf("Auction/Query") == -1) return;
        var auctionEquipMap = {};
        //页面显示的每个物品span标签
        var container = $(".equip-container .equip-name ");
        $.each(container, (index, item) => {
            var dataid = $(item).attr("data-id");
            auctionEquipMap[dataid] = item;//建立data-id和 随后会在item父标签加入符文后缀
        })
        var equipContainer = $(".equip-content");
        $.each(equipContainer, (index, item) => {
            var dataid = $(item).attr("data-id");
            var e = {};//装备对象
            var priceContainer = $(item).find(".equip-price");
            var priceText = priceContainer.text();
            var canDirectBuy = priceText.indexOf("一口价") > -1;
            var goldCoin = priceText.match(/(\d+)(?=金币)/g);
            var goldCoinPrice;//金价
            if (goldCoin != null) {
                goldCoinPrice = goldCoin[0];
            }
            var rune = priceText.match(/(\d+)(?=#)/g);
            var runePriceArr = [];//符文价
            var runeCountArr = [];//符文数量
            if (rune != null) {
                for (let i = 0; i < rune.length; i++) {
                    var runePrice = rune[i];
                    var runeCountText = priceContainer.find(".physical");
                    var runeCount = runeCountText[i].innerText.match(/\d+/g)[0];
                    runePriceArr.push(runePrice);
                    runeCountArr.push(runeCount);
                }

            }
            //需要插入符文后缀的span
            var matchSpan = auctionEquipMap[dataid];
            var suffixStr = generatePriceSuffix(goldCoin, goldCoinPrice, runePriceArr, runeCountArr);
            $(matchSpan).parent().append($("<span>", {
                text: suffixStr,
                style: "color:#ff8281"
            }));
            //读取装备名称
            var eTitle = $(item).find(".equip-title").text().match(/.*(?=\()/g)[0];
            var lv = $(item).find(".equip-title").text().match(/\((\d+)\)/)[1];
            var content = $(item).text();
            e.eTitle = eTitle;
            e.goldCoinPrice = goldCoinPrice ? goldCoinPrice * 1 : 0;
            e.runePriceArr = runePriceArr;
            e.runeCountArr = runeCountArr;
            e.content = content;
            e.lv = lv;
            e.eid = dataid;
            dataMap[dataid] = e;
        });
        //需要购买的装备
        //var equipToBuyArr = await Bridge.sendData(EquipToBuy, dataMap);
        //buyAuto(equipToBuyArr);
        return dataMap;
    }

    //获取当前页装备
    async function getEqMap() {
        return dataMap;
    }


    async function isJumpToEnd(config) {
        var quality = config.quality;
        var part = config.part;
        var eqbase = config.eqbase
        var curQuality = $(".panel-heading button")[0].innerText.trim();
        var curPart = $(".panel-heading button")[1].innerText.trim();
        var curBase = $(".panel-heading button")[2].innerText.trim();
        //是否载入到正确的选项 即三个选项载入完毕
        debugger;
        if (curPart == part && curQuality == quality && (eqbase == null || eqbase == curBase)) {
            return "success";
        }
    }

    async function isLastPage() {
        return $(".panel-footer a:contains('下页')").length == 0;
    }

    async function nextPage() {
        $(".panel-footer a:contains('下页')")[0].click();
    }

    async function jumpTo(config) {
        var quality = config.quality;
        var part = config.part;
        var eqbase = config.eqbase
        var curQuality = $(".panel-heading button")[0].innerText.trim();
        var curPart = $(".panel-heading button")[1].innerText.trim();
        var curBase = $(".panel-heading button")[2].innerText.trim();
        var ulList = $(".dropdown-menu:contains('全部')");
     
        if (curQuality != quality) {
            $(ulList[0]).find("li a").each((index, item) => {
                if (item.innerText == quality) {
                    item.click();
                }
            });
        }
        if (curPart != part) {
            $(ulList[1]).find("li a").each((index, item) => {
                if (item.innerText == part) {
                    item.click();
                }
            });
        }
        if (curBase != eqbase) {
            $(ulList[2]).find("li a").each((index, item) => {
                if (item.innerText == eqbase) {
                    item.click();
                }
            });
        }
  
     
    }


    function buy(eid) {
        
        var data = MERGE_Form({
            eid: eid,
            cid: _char.cid
        });
        POST_Message("EquipBuy", data, "post", 1500).then((r) => {

        }).catch((e) => {
            console.log("购物失败" + e)
        })
    }
    function generatePriceSuffix(goldCoin, goldCoinPrice, runePriceArr, runeCountArr) {
        var str = "";
        str += (goldCoin != null ? goldCoinPrice + "金" : "")
        runePriceArr.forEach((item, index) => {
            var runePrice = item;
            var runeCount = runeCountArr[index];
            str += runePrice + "# *" + runeCount + " ";
        });
        return str;
    }


    ah.jumpTo = jumpTo;
    ah.isJumpToEnd = isJumpToEnd;
    ah.isLastPage = isLastPage;
    ah.nextPage = nextPage;
    ah.buy = buy;
    ah.getEqMap = getEqMap;
})();
