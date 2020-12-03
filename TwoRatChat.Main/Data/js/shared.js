var ggbg = /\[sml\]file.*?www\\(.*?).png\[\/sml\]/g;
var smlr = /\[sml\](.*?)\[\/sml\]/g;
var urlr = /\[url\](.*?)\[\/url\]/g;
var imgr = /\[img(.*?)\](.*?)\[\/img\]/g;
var br = /\[b\](.*?)\[\/b\]/g;

function extractDomain(url) {
    try{
        var domain;
        if (url.indexOf("://") > -1) {
            domain = url.split('/')[2];
        }else{
            domain = url.split('/')[0];
        }
        domain = domain.split(':')[0];
        return domain;
    }catch( err ){
        return url;
    }
};


function convertText(text) {
    var oldText;
    var j = 10;
    text = text
		.replace(ggbg, "<img src='$1.png' class='message_smile' />") // BUGFIX :((
		.replace(smlr, "<img src='$1' class='message_smile' />");
    text = text
		.replace(br, "<b>$1</b>")
		.replace(urlr, function (lnk) {
		    return "<a href='" + lnk + "'>" + extractDomain(lnk) + "</a>";
		});
    text = text.replace(imgr, "<img src='$2' style='$1' class='imgfromurl' />");
    return text;
}

function onUrl(url){
    if( url.indexOf("http://") == 0 )
        return url;
    if( url.indexOf("https://") == 0 )
        return url;
    return _rootAsset + url;
}

var getUrlParameter = function getUrlParameter(sParam) {
    var sPageURL = decodeURIComponent(window.location.search.substring(1)),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;

    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');

        if (sParameterName[0] === sParam) {
            return sParameterName[1] === undefined ? true : sParameterName[1];
        }
    }
    return "";
};