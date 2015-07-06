/* Listen for nav bar commands (e.g. synctoc) */
if (isPostMessageEnabled()) {
    addMessageListener(navBarMessageHandler);
}

function navBarMessageHandler(event) {
    var message = getMessage(event.data);

    switch (message.messageType) {
        case "syncToC":
            var toc = document.getElementById("webtoc").contentWindow;
            toc.postMessage(message.messageType + "|" + message.messageData, "*");
            break;

        case "select":
            selectNavigationFrame(message.messageData);
            break;

        case "getPreviousTopic":
        case "getNextTopic":
            var toc = document.getElementById("webtoc").contentWindow;
            toc.postMessage(message.messageType + "|" + message.messageData, "*");
            break;

        case "shrinkIframes":
            shrinkFrame($('iframe#webtoc,iframe#webindex,iframe#websearch'));
            break;
    }
}

function shrinkFrame(frameSelector) {
    frameSelector.height(null);
    frameSelector.data('lastHeight', null);
    frameSelector.css('display', 'none');
}

function selectNavigationFrame(frame) {

    switch (frame) {
        case "nav-index":
            if (isAccordionView()) {
                $('#accordion').accordion('activate', 1);
            }
            else {
                $('iframe#webindex').css('display', 'block');
                $('iframe#webtoc,iframe#websearch').css('display', 'none');
            }
            break;
        case "nav-toc":
            if (isAccordionView()) {
                $('#accordion').accordion('activate', 0);
            }
            else {
                $('iframe#webtoc').css('display', 'block');
                $('iframe#websearch,iframe#webindex').css('display', 'none');
            }
            break;
        case "nav-search":
            if (isAccordionView()) {
                $('#accordion').accordion('activate', 2);
            }
            else {
                $('iframe#websearch').css('display', 'block');
                $('iframe#webtoc,iframe#webindex').css('display', 'none');
            }
            break;
    }

    if (!isAccordionView()) {
        resizeIframes(true);
    }

}

function isAccordionView() {

    if (!window.getDeviceType) {
        return true;
    } else {
        var deviceType = getDeviceType();
        return (deviceType != "MOBILE" && deviceType != "TABLET");
    }
}

$(function () {
    if (isAccordionView()) {

        Modernizr.load([{
            load: ['script/jquery-ui-1.8.19.custom.min.js'],
            complete: function () {
                $("#accordion").accordion({ fillSpace: true, animated: 'customslide' });

                $.extend($.ui.accordion.animations, {
                    customslide: function (options) {
                        var originalHeight = options.toHide.height();
                        var overflow = options.toShow.css("overflow");

                        options.toShow.css({ height: "hide", overflow: "hidden" }).show();

                        $({ dummyProperty: options.toHide.height() }).animate({ dummyProperty: "hide" },
                            {
                                duration: 300,
                                easing: 'swing',
                                step: function (now, settings) {
                                    var newHeight = Math.floor(now);
                                    options.toHide.height(newHeight);
                                    options.toShow.height(originalHeight - newHeight);
                                },
                                complete: function () {
                                    if (!options.autoHeight) {
                                        options.toShow.css("height", "");
                                    }
                                    options.toShow.css({
                                        overflow: overflow
                                    });
                                    options.toHide.hide();
                                    options.complete();
                                }
                            });
                    }
                });

                $(window).resize(function () {
                    $("#accordion").accordion("resize");
                });

                // Ensure that IE performs an initial resize
                setTimeout(function ()
                {
                    $("#accordion").accordion("resize");
                }, 1);

                // Notify the websearch iframe to set focus in the search box
                $('#accordion').on('accordionchange', function (event, ui) {
                    if (ui.newContent.attr('id') == "websearch") {
                        var search = document.getElementById("websearch").contentWindow;
                        search.postMessage("activated", "*");
                    }
                });
            }
        }]);
    }
    else {

        $('.header, .header.ui-state-active, iframe').css('display', 'none');
        if (Modernizr.touch) {
            $('iframe').attr('scrolling', 'no');
        }

    }
});