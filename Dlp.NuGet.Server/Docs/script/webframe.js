var baseTitle = document.title;

if (isPostMessageEnabled()) {
    addMessageListener(commonFrameMessageHandler);
}

/**
* @param {Object} event
**/
function commonFrameMessageHandler(event) {
    var message = getMessage(event.data);

    switch (message.messageType) {
        case "loaded":
            contentLoaded(message.messageData);
            break;
        case "updatePageTitle":
            updatePageTitle(message.messageData);
            break;
    }
}

/**
* @param {string} pageTitle
**/
function contentLoaded(url) {
    updateLocation(url);

    if (typeof contentLoadedImplementation == 'function') {
        contentLoadedImplementation();
    }
}

function updateLocation(url) {

    var pageName = url.substring(url.lastIndexOf('/') + 1);
    if ('#' + pageName != document.location.hash) {
        if (window.history.replaceState) {
            window.history.replaceState('', '', '#' + pageName);
        } else {
            window.location.replace('#' + pageName);
        }
    }

}

/**
* @param {string} pageTitle
**/
function updatePageTitle(pageTitle) {

    document.title = baseTitle + ' - ' + pageTitle;

}

function getCurrentPageName() {

    if (window.location.hash != "") {
        return window.location.hash.substring(1);
    }
    else {
        return getDefaultTopic();
    }

}
if (isPostMessageEnabled()) {
    addMessageListener(frameMessageHandler);
}

/**
* @param {!Object} event
**/
function frameMessageHandler(event) {
    var message = getMessage(event.data);

    switch (message.messageType) {
        case "navigate":

            if ($('#webcontent').attr('src') != message.messageData
                || (window.location.hash.length > 0 && window.location.hash.substring(1) != message.messageData)
                || (window.location.search.length > 0 && window.location.search.substring(1) != message.messageData)
                || (window.location.hash.length == 0 && window.location.search.length == 0)) {
                $('#webcontent').attr('src', message.messageData);
            }
            break;
    }
}

function constructLayout() {

    $('body').layout({
        resizeWhileDragging: true,
        west__size: 270,
        maskIframesOnResize: true
    });

}
window['constructLayout'] = constructLayout;

function contentLoadedImplementation() {

    setTimeout(function () {
        document.getElementById('webnavbar').contentWindow.postMessage('syncToC|' + getCurrentPageName(), '*');
    }, 500);

}
window['contentLoadedImplementation'] = contentLoadedImplementation;

function onDesktopWebFrameLoadComplete() {

    constructLayout();
    // Call contentLoadedImplementation now as the function won't be defined in the message handler until the specific
    // script has finished loading
    contentLoadedImplementation();
    setTimeout(function () {
        $('html').removeClass('loading');
        $('html').addClass('loaded');
    }, 1);

}
window['onDesktopWebFrameLoadComplete'] = onDesktopWebFrameLoadComplete;