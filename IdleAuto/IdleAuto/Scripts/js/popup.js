$(document).ready(function () {
    $.initPopup();
});

$.extend({
    initPopup: function () {
        var generateRandomClass = function (prefix = "cls") {
            return prefix + Math.random().toString(36).substring(2, 8);
        }

        var classMappings = {
            "equip-content-container": generateRandomClass("ecc"),
            "equip-content": generateRandomClass("ec"),            
        };
        var temp = {};
        $(".equip-name").each(function (i, p) {
            var eid = $(this).data("id");
            classMappings["ei" + eid] = generateRandomClass("ei");
            temp[eid] = classMappings["ei" + eid];
        });
        window.classMappings = temp;
        
        $.each(classMappings, function (originalClass, newClass) {
            $(`.${originalClass}`).removeClass(originalClass).addClass(newClass);
        });

        const dynamicCSS = `  
            .${classMappings["equip-content"]} { display: none; }
        `;
        $("head").append(`<style>${dynamicCSS}</style>`);

        $(".panel-filter").on("input propertychange", function () {
            $(this).parent().prev().find(".selected").removeClass("selected")
            var value = $(this).val();
            window.localStorage.setItem($(this).attr("id"), value);
            if (value.length > 0) {
                var values = value.split(/,|，/);
                var equips = $(this).parent().prev().find(".equip-name");
                equips.each(function (i, e) {
                    var match = 0;
                    var eid = $(e).data("id");
                    $.each(values, function (j, p) {
                        var ec = $("." + classMappings["equip-content-container"] + " > ." + classMappings["ei" + eid]);
                        if ($(ec).text().indexOf(p) >= 0) {
                            match++;
                        }
                    });
                    if (match == values.length) {
                        $(e).parent().addClass("selected");
                    }
                });
            }
        });

        var getPlacement = function (offsetTop, contentHeight) {
            var totalHeight = $(document.body).height();
            if (totalHeight < window.innerHeight) {
                totalHeight = window.innerHeight;
            }
            return offsetTop + contentHeight <= totalHeight || contentHeight > offsetTop ? "bottom" : "top";
        };

        var triggerPopover = function (target, contentDiv, click) {
            var html = contentDiv.html();
            if (html != undefined && html != null) {
                var top = $(target).offset().top;
                var height = contentDiv.height();

                if (html.trim().length > 0) {
                    $(target).popover({
                        placement: getPlacement(top, height),
                        trigger: ($.isMobile() || !click) ? "hover" : "hover click",
                        html: true,
                        content: html,
                    });
                    $(target).popover("show");
                }
            }
        };

        $(".panel").on("mouseover", ".equip-name", null, function (e, obj) {
        //$(".equip-name").mouseover(function (e) {
            var nameDiv = $(this);
            var contentDiv = nameDiv.parent().next();
            if (!contentDiv.hasClass(classMappings["equip-content"])) {
                var eid = nameDiv.data("id");
                contentDiv = $("." + classMappings["equip-content-container"] + " > ." + classMappings["ei" + eid]);
                var s = contentDiv.data("s");
                s = s ? s : 0;
                var f = contentDiv.data("f");
                f = f ? f : 0;
                var m = contentDiv.data("m");
                m = m ? m : 0;
                var cid = $("#cid").val();
                if (contentDiv.children().length == 0 && e.pageX > 0 && e.pageY > 0 && e.hasOwnProperty('originalEvent') && (e.originalEvent.isTrusted || e.originalEvent.detail == 1)) {
                    //var eq_timer_id = setTimeout(function () {   //加一个timeout，防止鼠标正常移过时频繁触发                     
                    //    $.get("/Equipment/EquipRender?cid=" + cid + "&eid=" + eid + "&s=" + s + "&f=" + f + "&m=" + m, function (data) {
                    //        contentDiv.empty().append(data);
                    //        if (nameDiv.data("timer") == eq_timer_id) { //验证鼠标还在当前装备
                    //            triggerPopover(nameDiv, contentDiv, true);
                    //        }                            
                    //        nameDiv.data("timer", null);
                    //    }, "html");
                    //}, 200);
                    ////window.eq_timer_id = eq_timer_id;
                    //nameDiv.data("timer", eq_timer_id);
                }
                else {
                    triggerPopover(nameDiv, contentDiv, true);
                }
            }
            else {
                triggerPopover(nameDiv, contentDiv, true);
            }
        });

        //$(".equip-name").mouseout(function () {
        //    var eq_timer_id = $(this).data("timer");
        //    if (eq_timer_id) {
        //        window.clearTimeout(eq_timer_id);
        //        $(this).data("timer", null);
        //    }
        //});

        $(".skill-name").hover(function () {
            var contentDiv = $(this).next();
            if (!contentDiv.hasClass("skill-content")) {
                contentDiv = $(this).parent().next();
            }
            if (contentDiv.hasClass("skill-content")) {
                triggerPopover($(this), contentDiv, !$(this).hasClass("no-click"));
            }
        });

        $(".bmap-detail-box").on("click", ".monster-name", function () {
            var contentDiv = $(this).parent().next();
            triggerPopover($(this), contentDiv, true);
        });

        $(".group-name").hover(function () {
            var contentDiv = $(this).parent().next();
            triggerPopover($(this), contentDiv, false);
        });

        //IOS hover trigger
        if ($.isMobile()) {
            document.body.addEventListener('touchstart', function () { });
            //$("tr").hover(function () { });
            //$(".notice-content").hover(function () { });
            //$("sr-container").hover(function () { });
        }
    }
});