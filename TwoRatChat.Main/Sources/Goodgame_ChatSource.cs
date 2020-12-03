// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
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
using System.Threading;
using System.Globalization;

namespace TwoRatChat.Main.Sources {
    public class Goodgame_ChatSource : TwoRatChat.Model.ChatSource {
        WebSocket ws;
        string _ChatID = "";
        bool allowAddMessages = false;
        string _id = "";
        string channelName = "";
        string _name = "";
        //string _UserToken = "";
        int urlIndex = 0;
        HashSet<string> _TrackUsers = new HashSet<string>();
       // GGCounter counter;
        //http://goodgame.ru/images/generated/smiles.png
        //const string smilesUri = "http://goodgame.ru/images/generated/smiles.png";

        Dictionary<string, string> Smiles = new Dictionary<string, string>();

        //protected void ParseCSSSmiles2( string content ) {
        //    content = content.Replace( '\n', ' ' ).Replace( '\r', ' ' );

        //    string[] blocks = content.Split( '}' );

        //    foreach ( var block in blocks ) {
        //        string[] data = block.Split( '{' );

        //        string[] keys = data[0].Replace(" ", "").Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries );
        //        //  .big-smiles .smiles.thup, .smiles.thup-big 
        //        //  .animated-smiles .smiles.cat.animated


        //        string key = "";
        //        foreach( var k in keys ) {
        //           // Console.WriteLine( k );
        //            if( k.StartsWith( ".big-smiles.smile." ) ) 
        //                key = k.Substring( 18 );

        //            if ( k.StartsWith( ".animated-smiles.smile." ) && Properties.Settings.Default.GG_AllowAnimatedSmiles ) {
        //                if( k.EndsWith( ".animated" ) )
        //                    key = k.Substring( 23, k.Length - 23 - 9 );
        //            }
        //        }

        //        if ( !string.IsNullOrEmpty( key ) ) {
        //            string[] urls = data[1].Split( new string[] { "\'" }, StringSplitOptions.RemoveEmptyEntries );

        //            // local cahce
        //            updateLocalSmile( urls[1] );

        //            Smiles[":" + key + ":"] = "asset://rat/gg" + urls[1].Split('?')[0];
        //        } else {
        //            //Console.WriteLine( block );
        //        }
        //    }
        //}

        protected void ParseCSSSmiles3( string url) {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.CreateHttp( url );
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.87 Safari/537.36";

            string content = "";

            try {
                //WebClient wv = new WebClient();
                //var x = wv.DownloadData( url );

                using( HttpWebResponse response = (HttpWebResponse)request.GetResponse() ) {
                    byte[] buffer = new byte[2048];
                    using( MemoryStream ms = new MemoryStream() ) {
                        using( Stream s = response.GetResponseStream() ) {
                            int l;
                            while( (l = s.Read( buffer, 0, buffer.Length )) > 0 )
                                ms.Write( buffer, 0, l );
                        }
                        content = Encoding.UTF8.GetString( ms.ToArray() );
                    }
                }
            } catch( Exception er ) {
                return;
            }



            content = content.Replace( '\n', ' ' ).Replace( '\r', ' ' );

            string[] blocks = content.Split( '}' );

            foreach( var block in blocks ) {
                string[] data = block.Split( '{' );

                string[] keys = data[0].Replace( " ", "" ).Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries );
                //  .big-smiles .smiles.thup, .smiles.thup-big 
                //  .animated-smiles .smiles.cat.animated


                string key = "";
                foreach( var k in keys ) {
                    // Console.WriteLine( k );
                    if( k.StartsWith( ".big-smiles.smile." ) )
                        key = k.Substring( 18 );

                    if( k.StartsWith( ".animated-smiles.smile." ) && Properties.Settings.Default.GG_AllowAnimatedSmiles ) {
                        if( k.EndsWith( ".animated" ) )
                            key = k.Substring( 23, k.Length - 23 - 9 );
                    }
                }

                if( !string.IsNullOrEmpty( key ) ) {
                    string[] urls = data[1].Split( new string[] { "\'" }, StringSplitOptions.RemoveEmptyEntries );

                    // local cahce
                    // updateLocalSmile( urls[1] );

                    Smiles[":" + key + ":"] = "asset://rat/http://goodgame.ru" + urls[1].Split( '?' )[0];

                    //this.Header = "Downloading smiles: " + Smiles.Count + "/" + blocks.Length;
                } else {
                    //Console.WriteLine( block );
                }
            }
        }

        //private void updateLocalSmile( string v ) {
        //    string fn = App.DataLocalFolder + "\\gg\\";
        //    Directory.CreateDirectory( fn );
        //    fn += "\\" + v.Split( '?' )[0].Replace( "/", "\\" );

        //    //if ( File.Exists( fn ) )
        //    //    return;

        //    string[] a = fn.Split( '\\' );
        //    string p = a[0];
        //    for( int j = 1; j < a.Length - 1; ++j ) {
        //        p += "\\" + a[j];
        //        Directory.CreateDirectory( p );
        //    }

        //    try {
        //        HttpWebRequest r = (HttpWebRequest)HttpWebRequest.CreateHttp( "https://goodgame.ru" + v );
        //        using( HttpWebResponse rr = (HttpWebResponse)r.GetResponse() ) {

        //            if( File.Exists( fn ) ) {
        //                FileInfo fi = new FileInfo( fn );
        //                if( fi.LastWriteTime == rr.LastModified )
        //                    return;
        //            }


        //            byte[] buffer = new byte[2048];
        //            int l = 0;
        //            using( MemoryStream ms = new MemoryStream() ) {
        //                using( Stream s = rr.GetResponseStream() ) {
        //                    while( (l = s.Read( buffer, 0, buffer.Length )) > 0 )
        //                        ms.Write( buffer, 0, l );
        //                }
        //                File.WriteAllBytes( fn, ms.ToArray() );
        //                FileInfo fi = new FileInfo( fn );
        //                fi.LastWriteTime = rr.LastModified;
        //            }
        //        }

                
        //    } catch {

        //    }
        //}

        private void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
           // if ( e.PropertyName == "GG_AllowChannelSmiles" || e.PropertyName == "GG_AllowAnimatedSmiles" )
          //      UpdateSmiles();
        }

        

        public void UpdateSmiles() {
            Smiles = new Dictionary<string, string>();



            try {
                ParseCSSSmiles3( "http://goodgame.ru/css/compiled/common_smiles.css" );
            } catch ( Exception er ) {

            }

            if( Properties.Settings.Default.GG_AllowChannelSmiles )
                try {
                    ParseCSSSmiles3( "http://goodgame.ru/css/compiled/channels_smiles.css" );
                } catch( Exception er ) {

                }
        }


        public void BeginWork() {
            //Console.WriteLine( "GOODGAME: подключение." );
            if ( /*! string.IsNullOrEmpty(_ChatID ) &&*/ ws == null) {

                string[] urls = Main.Properties.Settings.Default.url_Goodgame.Split( new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries );

                ws = new WebSocket(urls[(urlIndex++)%urls.Length], "", WebSocketVersion.Rfc6455);
                ws.EnableAutoSendPing = false;
                ws.Closed += ws_Closed;
                ws.Error += ws_Error;
                ws.MessageReceived += ws_MessageReceived;
                ws.Open();
            }
        }



        public Goodgame_ChatSource( Dispatcher dispatcher )
            : base(dispatcher) {
        }

        public override void ReloadChatCommand() {
            Reconnect();
        }

        private void Reconnect() {
            this.Status = false;

            Unsub();
            if (ws != null) {
                if (ws.State == WebSocketState.Open)
                    ws.Close();
                ws = null;
            }

            Create(this.Uri, this.Label);
        }

        string _rating = "";
        string _userInChat = "";
        string _viewers = "";

        void updateHeader() {
            switch( Main.Properties.Settings.Default.gg_HeaderMode ) {
                case 0:
                    this.Header = string.Format( "{0}/{1}/{2}", _viewers, _userInChat, _rating );
                    this.Tooltip = "goodgame: " + this.channelName + "\r\n" + Properties.Resources.tip_goodGameTooltip;
                    break;

                case 1:
                    if( string.IsNullOrEmpty(_userInChat ))
                        this.Header = string.Format( "{0}", _viewers );
                    else
                        this.Header = string.Format( "{0} ({1})", _viewers, _userInChat );
                    this.Tooltip = "goodgame: " + this.channelName;
                    break;

                case 2:
                    this.Header = string.Format( "{0}", _viewers );
                    this.Tooltip = "goodgame: " + this.channelName;
                    break;

                case 3:
                    this.Header = string.Format( "{0}", _userInChat );
                    this.Tooltip = "goodgame: " + this.channelName;
                    break;

                case 4:
                    this.Header = "";
                    this.Tooltip = "goodgame: " + this.channelName;
                    break;
            }

        }

        // Random rnd = new Random();

        bool _inCountUpdate = true;
        public override void UpdateViewerCount() {
            if ( !string.IsNullOrEmpty(_ChatID) && !_inCountUpdate) {
                _inCountUpdate = true;

                //counter.Start();
                // ws.Send( ("{'type':'get_channel_counters','data':{'channel_id':" + _ChatID + "}}").Replace( "'", "\"" ) );

                WebClient wc = new WebClient();
                //{"type":"message",
                //"data":{"channel_id":1168,"user_id":69983,
                //"user_name":"MSoft","user_rights":0,"premium":0,"premiums":[3857],
                //"hideIcon":0,"color":"","icon":"","isStatus":0,"mobile":0,"payments":"2",
                //"paidsmiles":[],"message_id":50365,"timestamp":1440145019,"text":"TrekOr, дабадабадаба зет тру? :fibo:"}}

                //Console.WriteLine( "GG viewers count." );

                wc.DownloadStringCompleted += ( a, b ) => {

                    if (b.Error == null) {
                        try {
                            var _o = JsonConvert.DeserializeObject( b.Result ) as Newtonsoft.Json.Linq.JObject;

                            var sex = _o[_ChatID.ToString()] as Newtonsoft.Json.Linq.JObject;
                            if (((string)sex["status"]) == "Dead") {
                                this.ViewersCount = null;
                                this.Header = null;
                            } else {
                                int xxx = int.Parse( (string)sex["viewers"] );
                                int usersInChat = int.Parse( (string)sex["usersinchat"] );
                                this.ViewersCount = xxx;
                                if ( xxx > 0 )
                                    _viewers = xxx.ToString();
                                else
                                    _viewers = "";

                                if ( usersInChat > 0 )
                                    _userInChat = usersInChat.ToString();
                                else
                                    _userInChat = "";
                               /* if (usersInChat > 5 && (DateTime.Now - _lastMessage).TotalSeconds > 30) {
                                    Reconnect();
                                }*/
                                updateHeader();
                            }
                        } catch {
                            //Console.
                        }
                    }

                    _inCountUpdate = false;
                };

                if( ws != null )
                if (ws.State == WebSocketState.Open)
                        ws.Send( ("{'type':'get_channels_list','data':{'start':0, 'count': 50}}").Replace( "'", "\"" ) );

//                ws.Send( ("{'type':'get_users_list','data':{'channel_id':" + _ChatID + "}}").Replace( "'", "\"" ) );


                wc.DownloadStringAsync( new Uri( string.Format(
                     "http://goodgame.ru/api/getggchannelstatus?id={0}&fmt=json", _ChatID ), UriKind.RelativeOrAbsolute ) );

                //try {
                //    ws.Send( ("{'type':'get_channel_counters','data':{'channel_id':" + _ChatID + "}}").Replace( "'", "\"" ) );
                //} catch (Exception err) {
                //}
            }

            if( (DateTime.Now - _ping ).TotalMinutes>2 ) {
                Reconnect();
            }
        }

       
        public override string Id { get { return "goodgame"; } }
       
        long _lastMessageId = 0;

        void OnHistory( Newtonsoft.Json.Linq.JObject history ) {
            _lastMessage = DateTime.Now;
            try {
                var mess = (Newtonsoft.Json.Linq.JArray)history["messages"];

                foreach (Newtonsoft.Json.Linq.JObject message in mess)
                    NewMessage( message );
            } catch {
                App.Log( '!', "OnHistory error" );
            }
        }

        DateTime _ping = DateTime.Now;

        void ws_MessageReceived( object sender, MessageReceivedEventArgs e ) {
            Console.WriteLine( e.Message );



            if (bClosed)
                return;

            _ping = DateTime.Now;

            try {
               // Console.WriteLine(e.Message);

                var _o = JsonConvert.DeserializeObject(e.Message) as Newtonsoft.Json.Linq.JObject;
                string st = (string)_o["type"];
                WebSocket ws = sender as WebSocket;
                this.Status = true;

                switch (st) {
                    case "welcome":
                       // _inCountUpdate = false;
                        //ws.Send( ("{'type':'join','data':{'channel_id':" + _ChatID + "}}").Replace( "'", "\"" ) );
                        ws.Send(("{'type':'auth','data':{'site_id':1,'user_id':0}}").Replace("'", "\""));
                        break;

                    case "success_auth":
                       // ws.Send( ("{'type':'get_channels_list','data':{'start':0, 'count': 50}}").Replace( "'", "\"" ) );
                        ws.Send( ("{'type':'join','data':{'channel_id':" + _ChatID + "}}").Replace( "'", "\"" ) );

                        //                        
                        break;

                    case "users_list":
                       // OnRating( (Newtonsoft.Json.Linq.JObject)_o["data"] );
                        break;

                    case "success_join":
                        // Header = _name;
                        if ( allowAddMessages ) {
                            ws.Send( ("{'type':'get_channel_history','data':{'channel_id':'" + _ChatID + "'}}").Replace( "'", "\"" ) );
                            //_waitHistory = DateTime.Now;
                        }
                        allowAddMessages = true;
                        //counter.Start();
                        ws.Send( ("{'type':'get_channel_counters','data':{'channel_id':'" + _ChatID + "'}}").Replace( "'", "\"" ) );

                        ws.Send( ("{'type':'get_users_list','data':{'channel_id':'" + _ChatID + "'}}").Replace( "'", "\"" ) );


                        break;

                    case "channels_list":
                        OnRating( (Newtonsoft.Json.Linq.JObject)_o["data"] );
                        break;

                    case "remove_message":
                        RemoveMessage( (Newtonsoft.Json.Linq.JObject)_o["data"] );
                        break;

                    case "channel_counters":
                        break;

                    case "message":
                        NewMessage((Newtonsoft.Json.Linq.JObject)_o["data"]);
                        break;

                    case "channel_history":
                     //   _waitHistory = DateTime.Now;
                        OnHistory( (Newtonsoft.Json.Linq.JObject)_o["data"]);
                        break;

                    case "error":
                        this.Status = false;
                        break;

                    default:
                        break;
                }

            } catch( Exception error ) {
                // App.AddDebugHistory("Goodgame exception: #4 {0}", error);
                /*
                Вызвано исключение: "System.FormatException" в mscorlib.dll
 [!] Goodgame exception: #4 System.FormatException: Входная строка имела неверный формат.
    в System.Number.StringToNumber(String str, NumberStyles options, NumberBuffer& number, NumberFormatInfo info, Boolean parseDecimal)
    в System.Number.ParseInt32(String s, NumberStyles style, NumberFormatInfo info)
    в System.Int32.Parse(String s)
    в TwoRatChat.Main.Sources.Goodgame_ChatSource.OnRating(JObject jObject) в X:\Sources\Sc2tvChatPub\TwoRatChat.Main\Sources\Goodgame_ChatSource.cs:строка 382
    в TwoRatChat.Main.Sources.Goodgame_ChatSource.ws_MessageReceived(Object sender, MessageReceivedEventArgs e) в X:\Sources\Sc2tvChatPub\TwoRatChat.Main\Sources\Goodgame_ChatSource.cs:строка 342
 GOODGAME: подключение.

                 */
                App.Log( '!', "Goodgame exception: #4 {0}, message: {1}", error, e.Message );
                Reconnect();
            }
        }

        private void OnRating(Newtonsoft.Json.Linq.JObject jObject) {
            try {
                //HashSet<string> newUsers = new HashSet<string>();
                int num = 0;
                _rating = "50+";

                foreach ( var channel in jObject["channels"] as Newtonsoft.Json.Linq.JArray ) {
                    string _channelId = channel["channel_id"].ToString();
                    num++;
                    if ( _channelId == _ChatID ) {
                        _rating = num.ToString();
                        break;
                    }
                }

                updateHeader();
            } catch {

            }
        }

        private void RemoveMessage( Newtonsoft.Json.Linq.JObject jObject ) {
        }

        private void Unsub() {
            if (ws != null) {
                ws.Closed -= ws_Closed;
                ws.Error -= ws_Error;
                ws.MessageReceived -= ws_MessageReceived;
            }
        }

        void ws_Error( object sender, SuperSocket.ClientEngine.ErrorEventArgs e ) {
            App.Log( '!', "Goodgame exception: #2 {0}", e.Exception );
            Reconnect();
        }

        
        void ws_Closed( object sender, EventArgs e ) {
            if (!bClosed) {
                App.Log( '!', "Goodgame exception: #0" );
                Reconnect();
            }
        }

        Regex _linkRegex = new Regex( @"<a.*?href=(.*?)>.*?a>", RegexOptions.IgnoreCase );


        static DateTime UnixTimeStampToDateTime( double unixTimeStamp ) {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private string ReplaceSmiles( string text ) {
            try {
                text = _linkRegex.Replace( text, ( m ) => {
                    return "[url]" + m.Groups[1].Value.Trim(' ', '\'', '"') + "[/url]";
                } );

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

        internal void NewMessage( Newtonsoft.Json.Linq.JObject msg ) {
            _lastMessage = DateTime.Now;

            try {
                string text = msg.ToString();
                long id = (long)msg["message_id"];

                if (_lastMessageId < id) {
                    _lastMessageId = id;

                    string ChatText = (string)msg["text"];
                    string Sender = (string)msg["user_name"];
                    DateTime time = UnixTimeStampToDateTime( double.Parse( (string)msg["timestamp"] ) );
                    TwoRatChat.Model.ChatMessage chatMessage = new TwoRatChat.Model.ChatMessage() {
                        Date = time,
                        Name = Sender,
                        Text = ReplaceSmiles( ChatText
                                .Replace( "&quot;", "\"" )
                                .Replace( "&#039;", "'" ) ),
                        Source = this,
                        //Form = 0,
                        Id = _id,
                        ToMe = this.ContainKeywords( ChatText )
                    };

                    newMessagesArrived( new TwoRatChat.Model.ChatMessage[] { chatMessage } );
                }
            } catch ( Exception er ) {
                App.Log( '!', "Goodgame message parsing exception: {0} - {1}", er, msg.ToString() );
            }
        }

        bool _firstRun = true;

        public override void Create( string streamerUri, string id ) {
            if (_firstRun) {
                _firstRun = false;
                this.Label = this._id = id;
                this.Uri = this._name = SetKeywords( streamerUri );

                Properties.Settings.Default.PropertyChanged += Default_PropertyChanged;

                UpdateSmiles();
                this.allowAddMessages = Main.Properties.Settings.Default.Chat_LoadHistory;


                int chatNumberId;
                if( int.TryParse( streamerUri, out chatNumberId ) ) {
                    channelName = _ChatID = chatNumberId.ToString();
                    _inCountUpdate = false;

                    BeginWork();
                    return;
                }




                try {

                    WebClient lwc = new WebClient();
                    lwc.Headers.Add( "Accept", "application/json" );
                    dynamic d = Newtonsoft.Json.JsonConvert.DeserializeObject( lwc.DownloadString( "http://api2.goodgame.ru/streams/" + this.Uri ) );

                  //  _UserToken = (string)d.key;// m.Groups[1].Value;
                    _ChatID = ((int)d.id).ToString();// m.Groups[1].Value.Trim( '\'', ' ' );
                    channelName = (string)d.key;

                    _inCountUpdate = false;

                } catch ( Exception er ) {
                    Console.WriteLine( "Create:{0}, {1}", GetType(), er );
                    _ChatID = "";
                    fireOnFatalError( FatalErrorCodeEnum.ParsingError );
                    return;

                }                
            }
            BeginWork();
        }


        void counter_OnViewersCountUpdate( int obj ) {
            this.ViewersCount = obj;
        }

        bool bClosed = false;

        public override void Destroy() {
            bClosed = true;

            Unsub();
            if (ws != null)
                ws.Close();
        
            ws = null;
           
        }
    }
}
