//using Newtonsoft.Json;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Net;
//using TwoRatChat.Model;
//using WebSocket4Net;

//namespace TwoRatChat.Main.Bot {
//    public class GGBot : BotSender {
//        WebSocket ws;
//        Token _token;
//        WebClient tokenClient;
//        int urlindex = 0;

//        public override string Name {
//            get {
//                return "goodgame";
//            }
//        }

//        //{"success":true,"scope":false,"access_token":"86842f330176bc681e27ba802230d8d9","refresh_token":"7cb1035291688f7e051564e11a9ca7e2"}

//        internal class Token {
//            public bool success;
//            public bool scope;
//            public string access_token;
//            public string refresh_token;
//        }

//        public void BeginWork() {
//            if ( _token == null ) {
//                if ( tokenClient == null ) {
//                    tokenClient = new WebClient();
//                    tokenClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
//                    tokenClient.UploadStringCompleted += (a, b) => {
//                        _token = JsonConvert.DeserializeObject<Token>( b.Result );
//                        BeginWork();
//                    };
//                    tokenClient.UploadStringAsync( new Uri( "http://goodgame.ru/api/token" ), "username=" + Login + "&password=" + Pass );
//                }
//                return;
//            } else {

//                if ( ws == null ) {
//                    Console.WriteLine( "GOODGAME-Bot: подключение." );
//                   // ws = new WebSocket( Main.Properties.Settings.Default.url_Goodgame, "", WebSocketVersion.Rfc6455 );
//                    ws.EnableAutoSendPing = false;
//                    ws.Closed += ws_Closed;
//                    ws.Error += ws_Error;
//                    ws.MessageReceived += ws_MessageReceived;
//                    ws.Open();
//                }
//            }
//        }

//        void ws_MessageReceived(object sender, MessageReceivedEventArgs e) {
//            try {
//                Console.WriteLine( "BOT: {0}", e.Message );

//                var _o = JsonConvert.DeserializeObject( e.Message ) as Newtonsoft.Json.Linq.JObject;
//                string st = (string)_o["type"];
//                WebSocket ws = sender as WebSocket;

//                switch ( st ) {
//                    case "welcome":
//                        ws.Send( ("{'type':'auth','data':{'site_id':1,'user_id':'"+
//                            149347 + "', 'api_token': '"+
//                            _token.access_token+
//                            "' }}" ).Replace( "'", "\"" ) );
//                        break;

//                    case "success_auth":
//                        //if( _o["data"]["user_id"] == "0")
//                        ws.Send( ("{'type':'join','data':{'channel_id':" + Source + "}}").Replace( "'", "\"" ) );
//                        break;

//                    case "users_list":
//                        break;

//                    case "success_join":
//                        sendQeued();
//                        break;

//                    case "channel_counters":
//                        break;

//                    case "message":
//                        //NewMessage( (Newtonsoft.Json.Linq.JObject)_o["data"] );
//                        break;

//                    case "error":
//                        ws.Close();
//                        ws = null;
//                        break;

//                    default:
//                        break;
//                }

//            } catch ( Exception error ) {
//                App.Log( '!', "Goodgame exception: #4 {0}", error );
//                ws.Close();
//                ws = null;
//            }
//        }

//        private void sendQeued() {
//            while( _sendQueue.Count > 0 ) {
//                ChatMessage cm = _sendQueue.Dequeue();
//                try {
//                    sendMessage( cm.Text, cm.Name );
//                } catch {
//                    _sendQueue.Enqueue( cm );
//                    BeginWork();
//                    return;
//                }
//            }
//        }

//        void ws_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e) {
//            ws.Close();
//        }

//        void ws_Closed(object sender, EventArgs e) {
//            ws = null;
//        }

//        public override void OnMessage(ChatMessage msg) {
//            if( msg.Text.Contains( "#" ) ) {
//                Send( "Hello my friend!" );
//            }
//        }

//        public override void Send(string text, string nick = null) {
//            if ( ws == null ) {
//                _sendQueue.Enqueue( new ChatMessage() { Name = nick, Text = text } );
//            } else {
//                try {
//                    sendMessage( text, nick );
//                } catch {
//                    _sendQueue.Enqueue( new ChatMessage() { Name = nick, Text = text } );
//                    BeginWork();
//                }
//            }
//        }

//        public override void SetCredentials(string login, string pass, string source) {
//            base.SetCredentials( login, pass, source );

//            BeginWork();
//        }

//        protected void sendMessage( string text, string nick ) {
//            if ( !string.IsNullOrEmpty( nick ) )
//                text = nick + ", " + text;

//            ws.Send( ("{\"type\":\"send_message\",\"data\":{\"channel_id\":" + Source + ", \"text\": \"" +
//                text.Replace( "\"", "'" )
//                + "\"}}") );
//        }

//        Queue<ChatMessage> _sendQueue = new Queue<ChatMessage>();
//    }
//}
