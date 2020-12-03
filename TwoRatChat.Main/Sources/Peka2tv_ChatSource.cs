using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using WebSocket4Net;
using Newtonsoft.Json;
using TwoRatChat.Controls;
using System.IO;
using Quobject.Collections.Immutable;
using SuperSocket.ClientEngine;

namespace TwoRatChat.Main.Sources {
    public class Peka2tv_ChatSource : TwoRatChat.Model.ChatSource {
        string channelName = "";
        Dictionary<string, string> Smiles = new Dictionary<string, string>();
        WebSocket ws;
        int urlIndex = 0;
        int _channelId = -1;

        int _sendMessageId = 0;

        Regex _packet = new Regex( "(\\d\\d)(\\d*)(.*)" );



        public void BeginWork() {
            //Console.WriteLine( "GOODGAME: подключение." );
            if ( _channelId != -1 && ws == null ) {

                string[] urls = Main.Properties.Settings.Default.peka2tv_chatUrl.Split( new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries );

                ws = new WebSocket( urls[(urlIndex++) % urls.Length], "", WebSocketVersion.Rfc6455 );
                ws.EnableAutoSendPing = false;
                ws.Closed += ws_Closed;
                ws.Error += ws_Error;
                ws.MessageReceived += ws_MessageReceived;
                ws.Opened += Ws_Opened;
                ws.Open();

                _listeners.Clear();
                _permanentlisteners.Clear();
                _permanentlisteners["/chat/message"] = ( msg ) => {
                    string text = (string)msg.text;
                    if ( msg.to != null ) {
                        text = "@" + msg.to.name + ", " + text.Trim();
                    }
                    NewMessage(
                        (int)msg.id,
                        (string)msg.from.name,
                        text,
                        UnixTimeStampToDateTime( (Int64)msg.time ) );
                };
                _permanentlisteners["/chat/message/remove"] = ( a ) => {
                };
            }
        }

        Dictionary<int, Action<dynamic>> _listeners = new Dictionary<int, Action<dynamic>>();
        Dictionary<string, Action<dynamic>> _permanentlisteners = new Dictionary<string, Action<dynamic>>();

        private void send( string cmd, dynamic data, Action<dynamic> a ) {
            int sendId = _sendMessageId++;
            _listeners[sendId] = a;
            ws.Send( "42" + (sendId) + $"[\"{cmd}\",{Newtonsoft.Json.JsonConvert.SerializeObject(data)}]" );
        }

        private void Ws_Opened( object sender, EventArgs e ) {

            send( "/chat/join", new { channel = "system" }, ( a ) => {
                if( (string)a[0].status != "ok" ) {
                    //...
                    App.Log( '?', "Invalid system join." );
                } else {
                }
            } );
            send( "/chat/join", new { channel = "stream/" + _channelId }, ( a ) => {
                if ( (string)a[0].status != "ok" ) {
                    //...
                    App.Log( '?', "Invalid stream join." );
                } else {
                    Status = true;
                }
            } );
        }

        void unsub() {
            if ( ws != null ) {
                ws.Closed -= ws_Closed;
                ws.Error -= ws_Error;
                ws.MessageReceived -= ws_MessageReceived;
                ws.Opened -= Ws_Opened;
            }
        }

        private void ws_Error( object sender, SuperSocket.ClientEngine.ErrorEventArgs e ) {
            unsub();

            try {
                ws.Close();
            }catch {

            }

            ws = null;
            BeginWork();
        }

        private void ws_Closed( object sender, EventArgs e ) {
            unsub();
            // throw new NotImplementedException();
            ws = null;
            BeginWork();
        }


        void ws_MessageReceived( object sender, MessageReceivedEventArgs e ) {
            if ( bClosed )
                return;
            try {
                if ( e.Message == "3" ) {
                    _ping = DateTime.Now;
                    //ws.Send( "3" );
                    return;
                }



                Match m = _packet.Match( e.Message );
                if ( m.Success ) {
                    Console.WriteLine( e.Message );

                    //if ( m.Groups[1].Value == "0" ) {
                    //    return;
                    //}
                    int id = int.Parse( m.Groups[1].Value );


                    switch ( id ) {
                        case 43: {
                                int recvId = int.Parse( m.Groups[2].Value );
                                Action<dynamic> listener;
                                if ( _listeners.TryGetValue( recvId, out listener ) ) {
                                    dynamic message = Newtonsoft.Json.JsonConvert.DeserializeObject( m.Groups[3].Value );
                                    listener( message );
                                    _listeners.Remove( recvId );
                                } else {
                                    App.Log( '?', "Unknown server answer: " + e.Message );
                                }
                            }                    
                            break;

                        case 42: {
                               //int recvId = int.Parse( m.Groups[2].Value );
                                Action<dynamic> listener;
                                dynamic message = Newtonsoft.Json.JsonConvert.DeserializeObject( m.Groups[3].Value );

                                if ( _permanentlisteners.TryGetValue( (string)message[0], out listener ) ) {
                                    listener( message[1] );
                                } else {
                                    App.Log( '?', "Unhandled server request: " + e.Message );
                                }
                            }
                            break;
                    }



                    //
                    //if ( message == null )
                    //    return;

                    //if ( message.Count != 2 )
                    //    return;

                    ////430[{"status":"ok","result":""}]

                    //switch ( (string)message[0] ) {
                    //    case "/chat/message":

                    //        break;


                    //}
                }
            }catch( Exception er ) {

            }
        }


        public override void UpdateViewerCount() {
            if ( ws != null )
                if ( ws.State == WebSocketState.Open ) {
                    ws.Send( "2" );

                    //ws.Send( (_sendMessageId++) +
                    //    $"[\"/chat/channel/list\",{{\"channel\":\"{_channelId}\"}}]" );

                    send( "/chat/channel/list", new { channel = "stream/" + _channelId }, ( a ) => {
                        if ( (string)a[0].status == "ok" ) {
                            this.ViewersCount = (int)a[0].result.amount;
                        } else {
                            this.ViewersCount = null;
                        }
                        updateHeader();
                    } );
                }
        }











        public Peka2tv_ChatSource( Dispatcher dispatcher )
            : base(dispatcher) {
        }

        public override void ReloadChatCommand() {
            Reconnect();
        }

        private void Reconnect() {
            this.Status = false;

        }

        void updateHeader() {
            this.Header = string.Format( "{0}", ViewersCount );
            this.Tooltip = "peka2tv: " + this.channelName;
            //switch ( Main.Properties.Settings.Default.gg_HeaderMode ) {
            //    case 0:
            //        this.Header = string.Format( "{0}/{1}/{2}", _viewers, _userInChat, _rating );
            //        this.Tooltip = "peka2tv: " + this.channelName + "\r\n" + Properties.Resources.tip_goodGameTooltip;
            //        break;

            //    case 1:
            //        if( string.IsNullOrEmpty(_userInChat ))
            //            this.Header = string.Format( "{0}", _viewers );
            //        else
            //            this.Header = string.Format( "{0} ({1})", _viewers, _userInChat );
            //        this.Tooltip = "peka2tv: " + this.channelName;
            //        break;

            //    case 2:
            //        this.Header = string.Format( "{0}", _viewers );
            //        this.Tooltip = "peka2tv: " + this.channelName;
            //        break;

            //    case 3:
            //        this.Header = string.Format( "{0}", _userInChat );
            //        this.Tooltip = "peka2tv: " + this.channelName;
            //        break;

            //    case 4:
            //        break;
            //}

        }

        // Random rnd = new Random();


       
        public override string Id { get { return "peka2tv"; } }

        DateTime _ping = DateTime.Now;


        private void RemoveMessage( Newtonsoft.Json.Linq.JObject jObject ) {
        }

        private void Unsub() {

        }



     ///   Regex _linkRegex = new Regex( @"<a.*?href=(.*?)>.*?a>", RegexOptions.IgnoreCase );


        static DateTime UnixTimeStampToDateTime( double unixTimeStamp ) {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private string ReplaceSmiles( string text ) {
            try {

                text = Regex.Replace( text,
                             @"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)",
                             "[url]$1[/url]" );

                //text = _linkRegex.Replace( text, ( m ) => {
                //    return "[url]" + m.Groups[1].Value.Trim(' ', '\'', '"') + "[/url]";
                //} );

                text = text.Replace( "  ", " " );

                foreach (var kv in Smiles) {
                    if (text.Contains( kv.Key )) {
                        text = text.Replace( kv.Key, " [sml]" + kv.Value + "[/sml] " );
                    }
                }

                return text;
            } catch ( Exception er ) {
                Console.WriteLine("ReplaceSmiles:{0}, {1}", GetType(), er);
                return string.Empty;
            }
        }

        /*
         * {
  "channel_id": 9975,
  "user_id": 158781,
  "user_name": "stalin76",
  "user_rights": 0,
  "premium": true,
  "hideIcon": 0,
  "color": "",
  "icon": "",
  "isStatus": 0,
  "mobile": 0,
  "payments": "300",
  "paidsmiles": [
    1
  ],
  "message_id": 15470,
  "timestamp": 1426925597,
  "text": "_SKY_, какой типа чай посоветуешь?)"
}

         */

        DateTime _lastMessage = DateTime.Now;

        internal void NewMessage( int id, string from, string text, DateTime time ) {

            TwoRatChat.Model.ChatMessage chatMessage = new TwoRatChat.Model.ChatMessage() {
                Date = time,
                Name = from,
                Text = ReplaceSmiles( text
                                    .Replace( "&quot;", "\"" )
                                    .Replace( "&#039;", "'" ) ),
                Source = this,
                //Form = 0,
                Id = this.Label,
                ToMe = this.ContainKeywords( text )
            };

            newMessagesArrived( new TwoRatChat.Model.ChatMessage[] { chatMessage } );


            //_lastMessage = DateTime.Now;

            //try {
            //    string text = msg.ToString();
            //    int id = (int)msg["message_id"];

            //    if (_lastMessageId < id) {
            //        _lastMessageId = id;

            //        string ChatText = (string)msg["text"];
            //        string Sender = (string)msg["user_name"];
            //        DateTime time = UnixTimeStampToDateTime( double.Parse( (string)msg["timestamp"] ) );
            //        
            //    }
            //} catch ( Exception er ) {
            //    App.Log( '!', "Goodgame message parsing exception: {0} - {1}", er, msg.ToString() );
            //}
        }

        bool _firstRun = true;

        public class ChannelInfo {
            public int id;
            public string name;
            public string slug;
        }

        public class SmileInfo {
            public string code;
            public string url;
        }

        public override void Create( string streamerUri, string id ) {
            this.Label = id;
            this.Uri = this.channelName = SetKeywords( streamerUri );


            WebClient wc = new WebClient();
            wc.Headers.Add( "Accept: application/json; version=1.0" );
            try {
                ChannelInfo result = Newtonsoft.Json.JsonConvert.DeserializeObject<ChannelInfo>( wc.UploadString( "http://funstream.tv/api/user",
                    $"{{'name':'{this.channelName}'}}" ) );
                //{"id":100055,"name":"phoagne","slug":"phoagne"}
                _channelId = result.id;

            } catch ( Exception er ) {
                _channelId = -1;
                fireOnFatalError( FatalErrorCodeEnum.ChannelNotFound );
            }

            wc = new WebClient();
            wc.Headers.Add( "Accept: application/json; version=1.0" );

            try {
                SmileInfo[] result = Newtonsoft.Json.JsonConvert.DeserializeObject<SmileInfo[]>( wc.UploadString( "http://funstream.tv/api/smile", "{}" ) );
                //{"id":100055,"name":"phoagne","slug":"phoagne"}

                foreach ( var smile in result )
                    Smiles[":" + smile.code + ":"] = smile.url;

            } catch ( Exception er ) {
                _channelId = -1;
                fireOnFatalError( FatalErrorCodeEnum.ChannelNotFound );
            }
            //if (_firstRun) {
            //    _firstRun = false;

            //    Properties.Settings.Default.PropertyChanged += Default_PropertyChanged;

            //    UpdateSmiles();
            //    this.allowAddMessages = Main.Properties.Settings.Default.Chat_LoadHistory;

            //    string ChannelUri = "http://goodgame.ru/chat/" + this.Uri;
            //    Header = "";
            //    this.Tooltip = "goodgame: " + streamerUri + "\r\n" + Properties.Resources.tip_goodGameTooltip;
            //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;

            //    CookieContainer cookieJar = new CookieContainer();

            //    CookieAwareWebClient wc = new CookieAwareWebClient( cookieJar );
            //    //wc.Headers.Add( "user-agent", "TwoRatChat" );
            //    wc.Encoding = Encoding.UTF8;
            //    try {
            //        ChannelUri = "https://goodgame.ru/chat/" + this.Uri + "?attempt=1";
            //        string Result = wc.DownloadString( new Uri( ChannelUri, UriKind.RelativeOrAbsolute ) ).Replace( '\n', ' ' ).Replace( '\r', ' ' );

            //        Regex rx = new Regex( "Chat.openRoom\\((.*?)\\)" );// "userToken.*?\\'(.*?)\\'.*?channelId\\:(.*?)\\,", RegexOptions.Multiline );
            //        Match m = rx.Match( Result );
            //        if ( m.Success ) {
            //            _UserToken = m.Groups[1].Value;
            //            _ChatID = m.Groups[1].Value.Trim( '\'', ' ' );
            //            channelName = this.Uri;

            //        } else {
            //            _ChatID = "";
            //            _UserToken = "";
            //            fireOnFatalError( FatalErrorCodeEnum.ChannelNotFound );
            //            return;
            //        }

            //        Console.WriteLine( "CHATID: " + _ChatID );
            //    } catch ( Exception er ) {
            //        Console.WriteLine( "Create:{0}, {1}", GetType(), er );
            //        _ChatID = "";
            //        fireOnFatalError( FatalErrorCodeEnum.ParsingError );
            //        return;
            //    }
            //}
            BeginWork();
        }


        void counter_OnViewersCountUpdate( int obj ) {
         

        

            //WebClient wc = new WebClient();
            //wc.Headers.Add( "Accept: application/json; version=1.0" );

            //wc.UploadStringCompleted += ( a, b ) => {
            //    if( b.Error != null ) {

            //    }
            //};
            //wc.UploadStringAsync( new System.Uri( "http://funstream.tv/chat/channel/list" ),
            //$"{{'channel': {_channelId}}}" );

            //try {
            //    ChannelInfo result = Newtonsoft.Json.JsonConvert.DeserializeObject<ChannelInfo>( wc.UploadString( "http://funstream.tv/api/user",
            //        $"{{'name':'{this.channelName}'}}" ) );
            //    //{"id":100055,"name":"phoagne","slug":"phoagne"}
            //    _channelId = result.id;

            //} catch ( Exception er ) {
            //    _channelId = -1;
            //    fireOnFatalError( FatalErrorCodeEnum.ChannelNotFound );
            //}
        }

        bool bClosed = false;

        public override void Destroy() {
            bClosed = true;

            Unsub();

           
        }
    }
}
