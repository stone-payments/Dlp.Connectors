/* General Utility Functions */

var ELEMENT_NODE = 1;

function getPhrase(name)
{
    var span = document.getElementById("phrase_" + name);
    if (span)
    {
        return span.innerHTML;
    }
}

function getClientSize()
{
    var height=0;
    var width=0;
    var doc=document;
    var win=window;

    if(doc.compatMode == 'CSS1Compat' && !win.opera && doc.documentElement && doc.documentElement.clientHeight)
    {
        height = doc.documentElement.clientHeight;
        width = doc.documentElement.clientWidth;
    }
    else if(doc.body && doc.body.clientHeight)
    {
        height = doc.body.clientHeight;
        width = doc.body.clientWidth;
    }
    else if(isDefined(win.innerHeight,win.innerWidth,doc.width))
    {
        height = win.innerHeight;
        width = win.innerWidth;
    }

    if(doc.width > win.innerWidth)
    {
        width = width - 16;
        height = height - 16;
    }

    return {width:width,height:height};
}

function isDefined()
{
  for(var x=0;x<arguments.length;x++)
  {
    if(typeof(arguments[x])=='undefined')
        return false;
  }
  return true;
}

function documentElement(id)
{
    return document.getElementById(id);
}

function sourceElement(e)
{
    if (window.event)
    {
        e = window.event;
    }

    return e.srcElement? e.srcElement : e.target;
}
    
/* Return Microsoft Internet Explorer (major) version number, or 0 for others. */
function msieversion()
{
    var ua = window.navigator.userAgent;
    var msie = ua.indexOf ( "MSIE " );

    if ( msie > 0 ) // is Microsoft Internet Explorer; return version number
    {
        return parseInt ( ua.substring ( msie+5, ua.indexOf ( ".", msie ) ) );
    }
    else
    {
        return 0;    // is other browser
    }
}   

/* Returns true if the passed element is currently in view */
function InView(element,margin) {
  if(!margin) margin=0;
  var Top=GetTop(element), ScrollTop=GetScrollTop();
  return !(Top<ScrollTop+margin||
    Top>ScrollTop+GetWindowHeight()-element.offsetHeight-margin);
}

/* Scrolls to ensure the passed element is currently in view */
function ScrollIntoView(element,bAlignTop,margin) {
  if(!margin) margin=0;
  var posY=GetTop(element);
  if(bAlignTop) posY-=margin;
  else posY+=element.offsetHeight+margin-GetWindowHeight();
  window.scrollTo(0, posY);
}

function GetWindowHeight() {
  return window.innerHeight||
    document.documentElement&&document.documentElement.clientHeight||
    document.body.clientHeight||0;
}

function GetScrollTop() {
  return window.pageYOffset||
    document.documentElement&&document.documentElement.scrollTop||
    document.body.scrollTop||0;
}

function GetTop(element) {
    var pos=0;
    do pos+=element.offsetTop
    while(element=element.offsetParent);
    return pos;
}

function findParentTagByName(e,tagName)
{
    if (!e)
        return;
    else if (e.tagName === tagName)
        return e;    
    else
        return findParentTagByName(e.parent,tagName);                      
}

/* End: General Utility Functions */


/* End: Frame helpers */

function findFrame(Name)
{
    var frameObject = parent.frames[Name];
    if((!frameObject) && parent.parent)
    {
        frameObject = parent.parent.frames[Name];
    }
    return frameObject;
}

function contentFrame()
{
    return findFrame("webcontent");
}

function navbarFrame()
{
    return findFrame("webnavbar");
}

function frameContainer()
{
	try
	{
    var frameLocation = parent.location.href;
    if (frameLocation.indexOf('webframe.') != -1)
        return parent;
    else if(parent.parent)
        return parent.parent;
}
	catch (e)
	{
		return parent;
	}
}

function contentDocument()
{
    return findFrame("webcontent").document;
}

/* End: Frame helpers */

/* Common Messaging Support */
function isPostMessageEnabled() {
    return (window['postMessage'] != null);
}

function addMessageListener(receiver) {
    if (isPostMessageEnabled())
    {
        if (window['addEventListener']) 
        {
            window.addEventListener("message", receiver, false);
        }
        else 
        {
            window.attachEvent("onmessage", receiver);
        }
    }
}

function Message(messageType,messageData)
{
	this.messageType = messageType;
	this.messageData = messageData;
}

function getMessage(data)
{
	var separator = data.indexOf("|");
    var messageType;
	var messageData;
	
	if (separator != -1)	
	{
		messageType = data.substring(0, separator);
		messageData = data.substring(separator + 1);
	}
	else
	{
		messageType = data;
		messageData = "";
	}
	
	return new Message(messageType,messageData);
}
/* Common Messaging Support */

/* Feature Flags */
var isDefaultTreeEnabled = true;
var isDefaultLayoutEnabled = true;

/* IFrame resize */
var isIframeResizeTimerDisabled = false;
function resizeIframes(ignoreOffScreen) {
    var maxHeight = 0;
    var minAllowedHeight = 0;
    try {
        minAllowedHeight = $(window.top).height();
    } catch (ex) {
        minAllowedHeight = $(window).height();
    }
    $('iframe').each(function () {
        if ($(this).is(":visible") && (ignoreOffScreen || $(this).offset().left >= 0)) {
            // Only resize if visible
            var currentHeight = 0;
            var doc = null
            try {
                doc = this.contentDocument ? this.contentDocument : (this.contentWindow.document || this.document);
            } catch (ex) {
                // Security may prevent access if frame hasn't loaded or is cross origin
            }
            var currentHeight = 0;
            if (doc) {
                currentHeight = $(doc).height();
            } else {
                currentHeight = minAllowedHeight;
            }
            var lastHeight = $(this).data('lastHeight');
            if (!lastHeight) lastHeight = 0;
            var heightDifference = currentHeight - lastHeight;
            if (heightDifference > 10 || (heightDifference < 0 && heightDifference < 10)) {
                var parent = $(this).parent();
                if (parent.get(0).tagName == "DIV" && currentHeight < parent.height()) {
                    // Resize to at least the containing DIV height
                    currentHeight = parent.height();
                }
                if (currentHeight < minAllowedHeight) {
                    // Make sure at least as high as the window
                    currentHeight = minAllowedHeight;
                }
                $(this).height((currentHeight) + "px");
                $(this).data('lastHeight', currentHeight);
            }
        }
        else if (!$(this).is(":visible")) {
            // Not visible, collapse to zero
            $(this).height(0);
            $(this).data('lastHeight', 0);
        }
        if (currentHeight > maxHeight) {
            // Record the maximum iframe height
            maxHeight = currentHeight;
        }
    });
    var busy = $("#busy");
    if (busy.get(0)) {
        busy.height(maxHeight);
    }
    return maxHeight;
}