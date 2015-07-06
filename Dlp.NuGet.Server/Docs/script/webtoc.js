var syncToCUrl = null;
var _ignoreSyncRequest = false;

$(function () {
    var nodeIndex = 0;
    $('ul.root a').each(function () {
        $(this).attr('data-node-index', nodeIndex);
        nodeIndex++;
    });
})

if (isDefaultTreeEnabled) {

    // Load the default deep tree ToC
    Modernizr.load([{
        load: ['stylesheets/webtoc.css',
                'script/desktop.webtoc.js'],
        complete: function () {
            constructDesktopToc();

            if (syncToCUrl != null) {
                syncToCNode(syncToCUrl);
                syncToCUrl = null;
            }
        }
    }]);

}
else {

    // Loading in responsive mode so disable JQM page initialization
    $(document).bind('mobileinit', function () {
        $.mobile.autoInitializePage = false;
    });

    yepnope.insertBeforeElement = document.getElementById('responsive-marker');

    // Conditionally load either responsive or desktop tree
    switch (getDeviceType()) {

        case "MOBILE":
        case "TABLET":
            var prefix = "";
            if (location.protocol == "file:") {
                prefix = "http:";
            }
            Modernizr.load([{
                load: [prefix + '//code.jquery.com/mobile/1.1.1/jquery.mobile-1.1.1.min.css',
                        prefix + '//code.jquery.com/mobile/1.1.1/jquery.mobile-1.1.1.min.js',
                        'script/responsive.webtoc.min.js'],
                complete: function () {
                    $('ul#root').css('display', 'none');
                    constructMobileToC();

                    if (syncToCUrl != null) {
                        syncToCNode(syncToCUrl);
                        syncToCUrl = null;
                    }
                }
            }]);
            break;

        case "DESKTOP":
            Modernizr.load([{
                load: ['stylesheets/webtoc.css',
                        'script/desktop.webtoc.js'],
                complete: function () {
                    constructDesktopToc();

                    if (syncToCUrl != null) {
                        syncToCNode(syncToCUrl);
                        syncToCUrl = null;
                    }
                }
            }]);
            break;
    }

}

if (isPostMessageEnabled()) {
    addMessageListener(tocMessageHandler);
}

/**
* @param {Object} event
**/
function tocMessageHandler(event) {
    var message = getMessage(event.data);

    switch (message.messageType) {
        case "syncToC":
            syncToCNode(message.messageData)
            break;
    }
}

/**
* @param {string} url
**/
function syncToCNode(data) {

    var anchor = null;

    if (typeof data == "string") {
        anchor = $('div#container > ul a[href="' + decodeURIComponent(data) + '"]').first();
    }
    else {
        anchor = data;
    }

    if (anchor != null 
		&& anchor.length 
		&& !_ignoreSyncRequest
		&& !isPageSelected(anchor)) {

        setSelectedNode(anchor);

        if (window.syncToCNodeImplementation != undefined) {
            syncToCNodeImplementation(anchor);
        }
        else {
            syncToCUrl = data;
        }
    }

    if (_ignoreSyncRequest) {
        _ignoreSyncRequest = false;
    }

}