var previouslySelectedNode = null;

function constructDesktopToc() {

    processlist($("ul.root"));

}

function expandOrCollapseClickHandler(e) {

    // expand / collapse child
    var siblingList = $(this).nextAll("ul");
    expandOrCollapseNode(siblingList, true);

}

function clickHandler(e) {

    var element = $(this);    
    selectNode(element);

    if (element.attr("href") != "#") {
        var webframe = window.parent.parent;
        webframe.postMessage("navigate|" + element.attr('href'), "*");
    }

    // Don't collapse the nested list when clicking on a node (do expand it if it is collapsed)
    if (!$(this).prev("ins").hasClass("collapse")) {
        // expand /collapse child
        var siblingList = element.nextAll("ul");
        expandOrCollapseNode(siblingList, true);
    }

    if (window.UpdateNavigationButtons != undefined) {        
        UpdateNavigationButtons();
    }

    e.preventDefault();
    return false;

}

function selectNode(anchor) {

    // highlighting
    if (previouslySelectedNode != null) {
        previouslySelectedNode.removeClass("selected");
    }
    anchor.removeClass("hover");
    anchor.addClass("selected");
    previouslySelectedNode = anchor;

    setSelectedNode(anchor);
}

function expandOrCollapseNode(ul, animate) {

    if (ul.length) {
               
        if (!ul.hasClass("visible")) {
            processlist(ul);
        }

        if (animate) {
            ul.slideToggle(200, function () {
                ul.toggleClass('visible');
                ul.css('display', '');
            });
        }
        else {
            ul.toggleClass('visible');
        }

        // Swap icons for open and closed books
        var icon = ul.prev("a").children("ins");
        if (icon.hasClass("icon-1"))
            icon.removeClass("icon-1").addClass("icon-2");
        else if (icon.hasClass("icon-2"))
            icon.removeClass("icon-2").addClass("icon-1");
        else if (icon.hasClass("icon-3"))
            icon.removeClass("icon-3").addClass("icon-4");
        else if (icon.hasClass("icon-4"))
            icon.removeClass("icon-4").addClass("icon-3");
         
        ul.prevAll("ins").toggleClass("expand collapse");

    }

}

function processlist(ul) {

    if (ul.hasClass("root")) {
        ul.addClass("visible");
    }

    ul.children("li").each(function (index, element) {

        var hasNestedList = false;
        $(this).children("ul").each(function (index, element) {
            if ($(this).children().length > 0) {
                hasNestedList = true;
            }
            else {
                // Remove any empty <ul> elements
                $(this).remove();
            }
        });

        var anchor = $(this).children("a");
        
        // Add an <ins> node for the expand / collapse icon if one doesn't already exist
        if (this.firstChild.nodeName != "INS") {
            var spacer = $('<ins class="spacer"></ins>');

            if (hasNestedList) {
                spacer.addClass("expandorcollapse");
                spacer.addClass("expand");
                spacer.click(expandOrCollapseClickHandler);
            }

            spacer.prependTo($(this));
        }

        // Add an <ins> node for the node icon
        if (anchor.children("ins").length == 0) {
            var icon = $('<ins class="icon"></ins>');
            var iconIndex = $(this).attr('rel');
            var iconClassIndex = iconIndex
            var isNew = $(this).attr('isnew') == "True";

            if (hasNestedList) {
                if (iconIndex <= 0) {
                    if (isNew) {
                        iconClassIndex = 3;
                    }
                    else {
                        iconClassIndex = 1;
                    }
                }
            }
            else {
                if (iconIndex <= 0) {
                    if (isNew) {
                        iconClassIndex = 10;
                    }
                    else {
                        iconClassIndex = 9;
                    }
                }
            }

            icon.addClass("icon-" + iconClassIndex);
            icon.prependTo(anchor);
        }

        anchor.click(clickHandler);

        anchor.hover(
        function () {
            if (!$(this).hasClass("selected")) {
                $(this).addClass("hover");
            }
        },
        function () {
            $(this).removeClass("hover");
        });
        
    });

}

function syncToCNodeImplementation(anchor) {

    // Only sync if there wasn't a previously selected node or the new node is different from the currently selected node
    if (anchor.length && ((previouslySelectedNode != null && anchor.attr('data-node-index') != previouslySelectedNode.attr('data-node-index')) || previouslySelectedNode == null)) {
        selectNode(anchor);
        expandParents(anchor, false);
        
        // scroll selected anchor into view (don't use scrollInToView here as that causes problems with the iframe)
        $('html, body').animate({
            scrollTop: anchor.offset().top
        }, 200);
        
        if (window.UpdateNavigationButtons != undefined) {            
            UpdateNavigationButtons();
        }
    }

}

function expandParents(element, animate, parents) {

    var parentUl = element.parent().closest("ul");

    if (parentUl.length == 0) {

        for (var i = parents.length - 1; i > -1; i--)
        {
            expandOrCollapseNode(parents[i], animate);
        }

        return;
    }
    else {

        if (parents == undefined || parents == null) {
            parents = [];
        }

        // Only expand if it isn't the root, isn't already expanded or hasn't been procesed yet
        if (!parentUl.hasClass("root")) {
            var spacer = parentUl.prevAll("ins.spacer");

            if (spacer.length == 0 || spacer.hasClass("expand")) {
                parents.push(parentUl);
            }
        }

        expandParents(parentUl, animate, parents);
    }

}