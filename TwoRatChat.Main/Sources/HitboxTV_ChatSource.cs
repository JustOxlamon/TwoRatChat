using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using System.Windows.Threading;
using WebSocket4Net;
using SuperSocket.ClientEngine;
using TwoRatChat.Model;

namespace TwoRatChat.Main.Sources {
    public class HitboxTV_ChatSource : TwoRatChat.Model.ChatSource {
        //http://api.hitbox.tv/media/live/yuuhi
        //https://www.hitbox.tv/api/chat/servers
        //

        /*
        {"method":"loginMsg","params":{"channel":"yuuhi","name":"UnknownSoldier","role":"guest"}}
        {"method":"chatMsg","params":{"channel":"yuuhi","name":"BKKW91","nameColor":"26BE50","text":"@Sielson liczyłem na to, że ktoś się nabierze :P","time":1441907549,"role":"anon","isFollower":true,"isSubscriber":false,"isOwner":false,"isStaff":false,"isCommunity":false,"media":false,"buffer":true}}


5:::{"name":"message","args":["{\"method\":\"loginMsg\",\"params\":{\"channel\":\"yuuhi\",\"name\":\"UnknownSoldier\",\"role\":\"guest\"}}"]}
5:::{"name":"message","args":["{\"method\":\"chatMsg\",\"params\":{\"channel\":\"yuuhi\",\"name\":\"SPLESHER\",\"nameColor\":\"3894CB\",\"text\":\"@Yuuhi dabuble ?\",\"time\":1441905993,\"role\":\"anon\",\"isFollower\":true,\"isSubscriber\":false,\"isOwner\":false,\"isStaff\":false,\"isCommunity\":false,\"media\":false,\"buffer\":true}}"]}


5:::{"name":"message","args":["{\"method\":\"chatMsg\",\"params\":{\"channel\":\"yuuhi\",\"name\":\"DziabBot\",\"nameColor\":\"5BBE31\",\"text\":\"<div class=\\\"image\\\"><img src=\\\"http://edge.sf.ak.hitbox.tv/chatimage?u=http%3A%2F%2Fedge.sf.hitbox.tv%2Fstatic%2Fimg%2Fchannel%2Fts3bahuczat-png_5533b2c993805.png\\\" hbx-width=\\\"410\\\" hbx-height=\\\"60\\\"/></div> IP: jarock.ts3bahu.com | Kup wlasny bezpieczny serwer teamspeaka 3 na stronie <a href=\\\"https://www.ts3bahu.com\\\" target=\\\"_blank\\\">https://www.ts3bahu.com</a> z kodem rabatowym YUUHI zyskujesz 20% upustu!\",\"time\":1441905993,\"role\":\"user\",\"isFollower\":false,\"isSubscriber\":false,\"isOwner\":false,\"isStaff\":false,\"isCommunity\":false,\"media\":true,\"buffer\":true}}"]}
5:::{"name":"message","args":["{\"method\":\"chatMsg\",\"params\":{\"channel\":\"yuuhi\",\"name\":\"VaderV1\",\"nameColor\":\"D35635\",\"text\":\"czesc\",\"time\":1441905997,\"role\":\"anon\",\"isFollower\":true,\"isSubscriber\":true,\"isOwner\":false,\"isStaff\":false,\"isCommunity\":false,\"media\":false,\"image\":\"/static/img/chat/yuuhi/badge.png\",\"buffer\":true}}"]}
5:::{"name":"message","args":["{\"method\":\"chatMsg\",\"params\":{\"channel\":\"yuuhi\",\"name\":\"SPLESHER\",\"nameColor\":\"3894CB\",\"text\":\"@VaderV1 Yo !\",\"time\":1441906004,\"role\":\"anon\",\"isFollower\":true,\"isSubscriber\":false,\"isOwner\":false,\"isStaff\":false,\"isCommunity\":false,\"media\":false,\"buffer\":true}}"]}
5:::{"name":"message","args":["{\"method\":\"chatMsg\",\"params\":{\"channel\":\"yuuhi\",\"name\":\"dawid13og\",\"nameColor\":\"AE39AE\",\"text\":\"A co tam slychac w Nexusie? Rexxar wkroczyl juz na pole bitwy? :P\",\"time\":1441906015,\"role\":\"anon\",\"isFollower\":true,\"isSubscriber\":false,\"isOwner\":false,\"isStaff\":false,\"isCommunity\":false,\"media\":false,\"buffer\":true}}"]}
5:::{"name":"message","args":["{\"method\":\"chatMsg\",\"params\":{\"channel\":\"yuuhi\",\"name\":\"upadlytrol\",\"nameColor\":\"79BE29\",\"text\":\"@VaderV1 elo lord vader\",\"time\":1441906018,\"role\":\"anon\",\"isFollower\":true,\"isSubscriber\":false,\"isOwner\":false,\"isStaff\":false,\"isCommunity\":false,\"media\":false,\"buffer\":true,\"buffersent\":true}}"]}
5:::{"name":"message","args":["{\"method\":\"chatMsg\",\"params\":{\"channel\":\"yuuhi\",\"name\":\"VaderV1\",\"nameColor\":\"D35635\",\"text\":\"<img src=\\\"http://edge.sf.hitbox.tv//static/img/chat/yuuhi/emotes/emoji_55ca133ff38cb.png\\\" title=\\\"Ypiatka\\\" alt=\\\"\\\" class=\\\"smiley sub-emotes channel-sub-emote\\\">  <img src=\\\"http://edge.sf.hitbox.tv//static/img/chat/yuuhi/emotes/emoji_55ca133ff38cb.png\\\" title=\\\"Ypiatka\\\" alt=\\\"\\\" class=\\\"smiley sub-emotes channel-sub-emote\\\">  <img src=\\\"http://edge.sf.hitbox.tv//static/img/chat/yuuhi/emotes/emoji_55ca133ff38cb.png\\\" title=\\\"Ypiatka\\\" alt=\\\"\\\" class=\\\"smiley sub-emotes channel-sub-emote\\\">  <img src=\\\"http://edge.sf.hitbox.tv//static/img/chat/yuuhi/emotes/emoji_55ca133ff38cb.png\\\" title=\\\"Ypiatka\\\" alt=\\\"\\\" class=\\\"smiley sub-emotes channel-sub-emote\\\">  <img src=\\\"http://edge.sf.hitbox.tv//static/img/chat/yuuhi/emotes/emoji_55ca133ff38cb.png\\\" title=\\\"Ypiatka\\\" alt=\\\"\\\" class=\\\"smiley sub-emotes channel-sub-emote\\\">   \",\"time\":1441906044,\"role\":\"anon\",\"isFollower\":true,\"isSubscriber\":true,\"isOwner\":false,\"isStaff\":false,\"isCommunity\":false,\"media\":false,\"image\":\"/static/img/chat/yuuhi/badge.png\"}}"]}

    */

        string socketAddr;

        public HitboxTV_ChatSource(Dispatcher dispatcher)
            : base(dispatcher) {
        }

        public override string Id {
            get { return "hitboxtv"; }
        }

        class server {
            [JsonProperty( "server_ip" )]
            public string ServerIP { get; set; }
        }

        class prms {
            [JsonProperty( "name", Required = Required.Default )]
            public string name { get; set; }
            [JsonProperty( "role", Required = Required.Default )]
            public string role { get; set; }
            [JsonProperty( "text", Required = Required.Default )]
            public string text { get; set; }
            [JsonProperty( "nameColor", Required = Required.Default )]
            public string color { get; set; }

            [JsonProperty( "image", Required = Required.Default )]
            public string badge { get; set; }


            [JsonProperty( "isFollower", Required = Required.Default )]
            public bool? isFollower { get; set; }
            [JsonProperty( "isSubscriber", Required = Required.Default )]
            public bool? isSubscriber { get; set; }
            [JsonProperty( "isOwner", Required = Required.Default )]
            public bool? isOwner { get; set; }
            [JsonProperty( "isStaff", Required = Required.Default )]
            public bool? isStaff { get; set; }
            [JsonProperty( "isCommunity", Required = Required.Default )]
            public bool? isCommunity { get; set; }
        }

        class args {
            [JsonProperty( "method" )]
            public string method { get; set; }
            [JsonProperty( "params" )]
            public prms prms { get; set; }
        }

        class message {
            [JsonProperty( "name" )]
            public string name { get; set; }
            [JsonProperty( "args" )]
            public string[] argsarray { get; set; }
        }

        public override void Create(string streamerUri, string id) {
            this.Label = id;
            this.Uri = streamerUri;

            try {
                WebClient wc = new WebClient();
                server[] servers = JsonConvert.DeserializeObject<server[]>( wc.DownloadString( "https://www.hitbox.tv/api/chat/servers" ) );

                foreach( server s in servers ) {

                    string x = wc.DownloadString( string.Format( "http://{0}/socket.io/1/?t={1}",
                        s.ServerIP, DateTime.Now.ToBinary() ) );
                    //Af09N_ZarHPEPT7CjmUx:60:60:websocket

                    socketAddr = string.Format( "ws://{0}:80/socket.io/1/websocket/{1}",
                        s.ServerIP, x.Split( ':' )[0] );

                    socketConnect();
                        break;
                }

            } catch ( Exception er ) {
                App.Log( '!', "Failed to create hitboxtv: {0}", er );
                fireOnFatalError( FatalErrorCodeEnum.ParsingError );
            }
        }

        WebSocket ws;

        private void socketConnect() {
            ws = new WebSocket( socketAddr, "", WebSocketVersion.Rfc6455 );
            ws.EnableAutoSendPing = false;
            Sub();
            ws.Open();
        }

        private void Sub() {
            ws.Closed += ws_Closed;
            ws.Error += ws_Error;
            ws.DataReceived += ws_DataReceived;
            ws.MessageReceived += ws_MessageReceived;
        }

        private void Unsub() {
            ws.Closed -= ws_Closed;
            ws.Error -= ws_Error;
            ws.DataReceived -= ws_DataReceived;
            ws.MessageReceived -= ws_MessageReceived;
        }

        private void ws_MessageReceived(object sender, MessageReceivedEventArgs e) {
            if ( "1::" == e.Message )
                join();

            if ( "2::" == e.Message )
                pong();


            //5:::{"name":"message","args":["{\"method\":\"chatMsg\",\
            //"params\":{\"channel\":\"yuuhi\",\"name\":\"Slagator\",\"nameColor\":\"8051C7\",
            //\"text\":\"Uwaga Mikihashi zrzucil atomowke. xD\",\"time\":1441908923,
            //\"role\":\"anon\",\"isFollower\":true,\"isSubscriber\":true,\"isOwner\":false,
            //\"isStaff\":false,\"isCommunity\":false,\"media\":false,
            //\"image\":\"/static/img/chat/yuuhi/badge.png\"}}"]}

            try {
                if ( e.Message.StartsWith( "5:::" ) ) {
                    message m = JsonConvert.DeserializeObject<message>( e.Message.Substring( 4 ) );
                    if ( m.argsarray.Length > 0 ) {
                        args a = JsonConvert.DeserializeObject<args>( m.argsarray[0] );
                        this.Status = true;

                        if ( a.method == "chatMsg" ) {
                            addMessage( a.prms );
                        }
                    }
                }
            }catch( Exception er ) {
                App.Log( '!', "hitbox: Json parse error: {0}", er );
            }

            Console.WriteLine( e.Message );
        }

        private void pong() {
            ws.Send( "2::" );
        }

        private void addMessage(prms prms) {
            ChatMessage cm = new ChatMessage() {
                Name = prms.name,
                Date = DateTime.Now,
                //Form = 1,
                Id = this.Label,
                Source = this,
                Text = prms.text,
                ToMe = false
            };

            //5:::{"name":"message","args":["{\"method\":\"chatMsg\",\"params\":{\"channel\":\"yuuhi\",\"name\":\"bebo7\",\"nameColor\":\"39BF42\",\"text\":\"@Kravchenko dawno nie gral;P\",\"time\":1441910697,\"role\":\"anon\",\"isFollower\":true,\"isSubscriber\":true,\"isOwner\":false,\"isStaff\":false,\"isCommunity\":false,\"media\":false,\"image\":\"/static/img/chat/yuuhi/badge.png\"}}"]}

            if ( Properties.Settings.Default.hitbox_AllowUserColors && !string.IsNullOrEmpty( prms.color ) ) {
                cm.Color = "#" + prms.color;
            }

            if ( !string.IsNullOrEmpty( prms.badge ) ) {
                cm.AddBadge( "http://edge.sf.hitbox.tv" + prms.badge );
            }

            this.newMessagesArrived( new ChatMessage[] {
                cm
            } );
        }

        private void join() {
            ws.Send( "5:::{\"name\":\"message\",\"args\":[{\"method\":\"joinChannel\",\"params\":{\"channel\":\"" + this.Uri + "\", \"name\": \"UnknownSoldier\"}}]}" );
        }

        private void ws_DataReceived(object sender, DataReceivedEventArgs e) {
        }

        private void ws_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e) {
        }

        private void ws_Closed(object sender, EventArgs e) {
        }

        public override void Destroy() {
            if ( ws != null ) {
                Unsub();
                ws.Close();
                ws = null;
            }
        }

        public override void UpdateViewerCount() {
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += (a, b) => {
                if( b.Error == null ) {
                    Console.WriteLine( b.Result );
                }
            };

            wc.DownloadStringAsync( new System.Uri( "http://api.hitbox.tv/media/live/" + Uri, UriKind.Absolute ) );
        }
    }



}
