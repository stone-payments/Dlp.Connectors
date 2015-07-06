$(function () {
    $('iframe#webcontent').attr('src', getDefaultTopic());
});

function getDefaultTopic() {
    var qs = window.location.search;

    if (qs.length > 0)
        return qs.substring(1) + window.location.hash;
    else if (window.location.hash.length > 0)
        return window.location.hash.substring(1);
    else
        return defaultTopic;
}

if (isDefaultLayoutEnabled) {
    loadDefaultLayout();
}
else {

    // Loading in respsonsive mode, conditionally load responsive or desktop layout
    switch (getDeviceType()) {

        case "TABLET":
        case "MOBILE":
            Modernizr.load([{
                load: ['stylesheets/responsive.webframe.css',
                        'script/responsive.webframe.js',
                        'script/jquery.animate-enhanced.js'],
                complete: function () {
                    onResponsiveWebFrameLoadComplete();
                }
            }]);
            break;

        case "DESKTOP":
            loadDefaultLayout();
            break;
    }

}

function loadDefaultLayout() {

    Modernizr.load([{
        load: ['stylesheets/jquery.layout.css',
                'script/jquery.layout.min.js',
                'script/webframe.js'],
        complete: function () {
            onDesktopWebFrameLoadComplete();
        }
    }]);

}

