(function () {
    alert(1)
    var dataMap = {}//存储dataid对应装备的数据 

    loadPriceSuffix();
    if (location.href.indexOf("Auction/Query") == -1) return;
    //给每个拍卖物品加入后缀
    function loadPriceSuffix() {
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
            e.eTitle = eTitle;
            e.goldCoinPrice = goldCoinPrice ? goldCoinPrice * 1 : 0;
            e.runePriceArr = runePriceArr;
            e.runeCountArr = runeCountArr;
            dataMap[dataid] = e;
        });
        console.log(dataMap);
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
})();