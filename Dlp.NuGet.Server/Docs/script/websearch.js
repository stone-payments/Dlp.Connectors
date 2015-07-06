if (isPostMessageEnabled()) {
    addMessageListener(searchMessageHandler);
}

/**
* @param {Object} event
**/
function searchMessageHandler(event) {
    var message = getMessage(event.data);

    switch (message.messageType) {
        case "activated":
            $('input#txtSearch').focus();
            break;
    }
}

$(function () {

    $('input#txtSearch').keyup(function (event) {
        if (event.keyCode == 13) {
            $('input#search').click();
        }
    });

    $('input#search').on('click', function () {
        btnSearch_onclick();
    });
});