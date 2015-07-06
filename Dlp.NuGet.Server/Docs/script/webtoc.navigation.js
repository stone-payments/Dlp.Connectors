var _selectedNodeIndex = -1;
var _tocNodeCount = -1
var _selectedNodeHref = null;

function moveToNextNode(ignoreHeadingNodes) {
    return _moveNode(false, ignoreHeadingNodes);
}

function moveToPreviousNode(ignoreHeadingNodes) {
    return _moveNode(true, ignoreHeadingNodes);
}

function getNodeByHref(href) {
    return _getAnchors().filter(function () {
        return $(this).attr("href") == href;
    }).first();
}

function getSelectedNode() {
    return _getAnchors().filter(function () {
        return $(this).attr("data-node-index") == _selectedNodeIndex;
    }).first();
}

function setSelectedNode(anchor) {
    _selectedNodeIndex = parseInt(anchor.attr("data-node-index"));
	_selectedNodeHref = anchor.attr("href");
}

function isLastNodeSelected() {

    if (_tocNodeCount == -1) {
        _tocNodeCount = _getAnchors().length;
    }

    return _selectedNodeIndex == _tocNodeCount - 1;
}

function isFirstNodeSelected() {
    return _selectedNodeIndex == 0;
}

function _getAnchors() {
    return $('div#container > ul#root a[data-node-index]');
}

function _moveNode(isPrevious, ignoreHeadingNodes) {

    var anchors = _getAnchors();

    if (isPrevious) {
        if (_selectedNodeIndex > 0) {
            if (ignoreHeadingNodes) {
                do {
                    _selectedNodeIndex--;
                } while (anchors.eq(_selectedNodeIndex).attr("href").substring(0, 1) == "#" && _selectedNodeIndex > 0);
            }
            else {
                _selectedNodeIndex--;
            }
        }
    }
    else {
        if (anchors.length - 1 > _selectedNodeIndex) {
            if (ignoreHeadingNodes) {
                do {
                    _selectedNodeIndex++;
                } while (anchors.eq(_selectedNodeIndex).attr("href").substring(0, 1) == "#" && anchors.length - 1 > _selectedNodeIndex);
            }
            else {
                _selectedNodeIndex++;
            }
        }
    }

    return anchors.eq(_selectedNodeIndex);
}

function isPageSelected(anchor) {
	return (anchor.attr("href") == _selectedNodeHref);
}