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

    var title = s + "<span class='name'>" + mes.name + "</span></span>";
                
    s = "";

    if (mes.source === undefined || mes.source=="system") {
        mclass = "messagesys";
        s += "<span class='text'>" + convertText(mes.text);
    } else {
        if (mes.tome == 'True'){
            mclass = "messagetm";
            s += "<span class='text'>" + convertText(mes.text);
        }else
            s += "<span class='text'>" + convertText(mes.text);
    }

    s = s + "</span>";

    // Создается объект на основе разметки сообщения
    return {
        title: title,
        text: s
    };
;
}

function onNewMessage(mes) {
    return createMessage( mes );
}

function onClearUser(nick){
    console.log("clear user:" + nick);
}

var doNotShowFirstTime = true;

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
                    
                    chats.oldMes[mes.gid] = {
                        delay: 5,
                        message: m,
                        data: mes
                    };

                    if( !doNotShowFirstTime ) {
                        $.amaran({
                                content:{
                                    themeName:'myTheme',
                                    title: m.title,
                                    message: m.text
                                },
                                delay: 15000,
                                //inEffect: "fadeInLeft",
                                //outEffect: "fadeOut",
                                'cssanimationIn'  :'bounceInRight',
                                'cssanimationOut' :'bounceOutRight',
                                position:'top right',
                                themeTemplate:function(data){
                                  return '<div class="mydiv"><span class="title">'+data.title+
                                  '</span><div class="inner-content"><span class="message">'
                                  +data.message+'</span></div></div>';
                                }
                            });
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
        doNotShowFirstTime=false;

    }catch(pika){
        console.error(pika);
    }
    setTimeout(getData, 1000);
}

function validateAndScroll(){

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