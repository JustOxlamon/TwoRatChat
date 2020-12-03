var remainMessages = 30;
var chats = {
    delMe: [],
    oldMes: {}
};

function createMessage(mes) {
    var s = '<span class="namecontainer">';
    if (mes.source !== undefined && mes.source !== null ) {
        s += "<img src='"+_rootAsset+"img/" + mes.source + ".png' class='message_source' />";
    };
    if( mes.badges !== undefined && mes.badges.length > 0 ){
        for( var b=0; b<mes.badges.length; ++b ){
            s += "<img class='message_badge' src='" + onUrl(mes.badges[b]) + "' />";
        }
    }

    var mclass = "message";

    if( mes.color === undefined || mes.color == "" ){

    }else{
        mes.name = "<font color='" + mes.color + "'>" + mes.name + "</font>";
    }

    if (mes.source === undefined || mes.source=="system") {
        mclass = "messagesys";
        s += "<span class='text'>" + convertText(mes.text);
    } else {
        if (mes.tome == 'True'){
            mclass = "messagetm";
            s += "<span class='name'>" + mes.name +
		         "</span></span><span class='text'>" + convertText(mes.text);
        }else
            s += "<span class='name'>" + mes.name +
				 "</span></span><span class='text'>" + convertText(mes.text);
    }

    s = "<div class='" + mclass + " "+_animShow+" animated'>" + s + "</span></div>"

    // Создается объект на основе разметки сообщения
    return $(s);
}

function onNewMessage(mes) {
    var sex = createMessage( mes );
    sex.appendTo("#root");
    return sex;
}

function onClearUser(nick){
    console.log("clear user:" + nick);
}

function onData(data) {
    try{
        var start = data.messages.length - remainMessages;
        if (start < 0) start = 0;

        for (var i = start; i < data.messages.length; ++i) {
            var mes = data.messages[i];
            var old = chats.oldMes[mes.gid];
            if (old == undefined) {
                try {
                    var m = onNewMessage(mes);
                    chats.delMe.push(m);
                    chats.oldMes[mes.gid] = {
                        delay: 5,
                        message: m,
                        data: mes
                    };
                } catch (err) {
                   console.error(err);
                   //alert("failed: " + err);
                }
            }else{
                if( old.data.text != mes.text ){
                    console.log("text deleted: " + old.data.text );
                    old.data = mes;
                    old.message.html("<div></div>");
                }
            }
        }

    }catch(pika){
        console.error(pika);
    }
    setTimeout(getData, 1000);
}

function validateAndScroll(){
    try{
        if (chats.delMe.length > 1) {
            var nDelMe = [];

            for (var j = 0; j < chats.delMe.length; ++j)
                if (chats.delMe[j].hasClass(_animHide)) {
                    chats.delMe[j].remove();
                } else {
                    nDelMe.push(chats.delMe[j])
                }

            if( chats.delMe.length > _maxMessages){
                for (var j = 0; j < chats.delMe.length - _maxMessages; ++j) {
                    var mes = chats.delMe[j];
                    mes.addClass(_animHide)
                }
            }
            chats.delMe = nDelMe;
        }

        $("#root").animate({ scrollTop: $("#root")[0].scrollHeight + 1000 }, 90);
        setTimeout(validateAndScroll, 100);
    }catch( err ){
        console.error(err);
    }
}

function getData() {
    $.getJSON(_rootChat)
        .done(onData)
        .fail(function (jqxhr, textStatus, error) {
            console.log("failed: " + error);
            setTimeout(getData, 5000);
        });
}

function onUpdate(){

}

function onStart(){
    console.log("stared");
    getData();
    validateAndScroll();
}