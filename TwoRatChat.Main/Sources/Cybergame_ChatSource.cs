// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Windows.Threading;
using WebSocket4Net;

namespace TwoRatChat.Main.Sources {
    public class Cybergame_ChatSource: TwoRatChat.Model.ChatSource {
        //WebSocket ws;
        string _id = "";
        string _name = "";
        DateTime? _joinSended = null;

        public void BeginWork() {
            _joinSended = null;

           /* if (ws != null) {
                Unsub();
                if (ws.State == WebSocketState.Open)
                    ws.Close();
                ws = null;
            }

            if (_name != "" && ws == null) {
                Tooltip = "cybergame: " + Uri;
                ws = new WebSocket( Main.Properties.Settings.Default.url_Cybergame, "", WebSocketVersion.Rfc6455);
                ws.EnableAutoSendPing = true;
                ws.AutoSendPingInterval = 100;
                ws.Opened += ws_Opened;
                ws.Closed += ws_Closed;
                ws.Error += ws_Error;
                ws.DataReceived += ws_DataReceived;
                ws.MessageReceived += ws_MessageReceived;
                ws.Open();
            }*/
        }

        void Unsub() {
          /*  ws.Opened -= ws_Opened;
            ws.Closed -= ws_Closed;
            ws.Error -= ws_Error;
            ws.DataReceived -= ws_DataReceived;
            ws.MessageReceived -= ws_MessageReceived;*/
        }

        public override void ReloadChatCommand() {
            this.Status = false;
            this.BeginWork();
        }

        public Cybergame_ChatSource( Dispatcher dispatcher )
            : base(dispatcher) {
        }

        bool _inUpdateViewers = false;
        public override void UpdateViewerCount() {
            if ( _joinSended.HasValue && (DateTime.Now - _joinSended.Value).TotalSeconds > 10 ) {
                fireOnFatalError( FatalErrorCodeEnum.ChannelNotFound );
                return;
            }

            if (!_inUpdateViewers && !string.IsNullOrEmpty(_name)) {
                Regex rx = new Regex("viewers.*?\\:\\\"(.*?)\\\"");
                WebClient wc = new WebClient();
                wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(( b, a ) => {
                    if (a.Error == null) {
                        Match m = rx.Match(a.Result);
                        if (m.Success) {
                            int h;
                            if (int.TryParse(m.Groups[1].Value, out h)) {
                                this.ViewersCount = h;
                                if (h == 0) {
                                    this.Header = "";
                                }else
                                    this.Header = h.ToString();
                            }
                        }
                    }
                    _inUpdateViewers = false;
                });
                _inUpdateViewers = true;
                wc.DownloadStringAsync(new Uri(
                    string.Format("http://api.cybergame.tv/w/streams.php?channel={0}", _name), UriKind.RelativeOrAbsolute));
            }
        }

        public override string Id { get { return "cybergame"; } }

        class LocalMessage {
            [JsonProperty("when")]
            public long When { get; set; }
            [JsonProperty("from")]
            public string From { get; set; }
            [JsonProperty("text")]
            public string Text { get; set; }
        }

        void ws_MessageReceived( object sender, MessageReceivedEventArgs e ) {
            this.Status = true;
            _joinSended = null;

           // Console.WriteLine( e.Message );

            try {
                var _o = JsonConvert.DeserializeObject(e.Message) as Newtonsoft.Json.Linq.JObject;
                string st = (string)_o["command"];

                switch (st) {
                    case "chatMessage":
                        NewMessage( ((Newtonsoft.Json.Linq.JValue)_o["message"]).Value as string );
                        break;
                    default:
                        break;
                }

            } catch {
                BeginWork();
            }
        }

        static DateTime UnixTimeStampToDateTime( double unixTimeStamp ) {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        Regex _linkRegex = new Regex(@"(http|ftp|https)://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?", RegexOptions.IgnoreCase);

        private string ReplaceSmiles( string text ) {
            text = _linkRegex.Replace( text, ( m ) => {
                return "[url]" + m.Value + "[/url]";
            } );
            return text;
        }

        private void NewMessage( string msg ) {
            LocalMessage lm = JsonConvert.DeserializeObject<LocalMessage>(msg);

            TwoRatChat.Model.ChatMessage chatMessage = new TwoRatChat.Model.ChatMessage() {
                Date = DateTime.Now,
                Name = lm.From,
                Text = ReplaceSmiles(lm.Text.Replace("&quot;", "\"")),
                Source = this,
                //Form = 0,
                Id = _id,
                ToMe = this.ContainKeywords( lm.Text )
            };

            newMessagesArrived(new TwoRatChat.Model.ChatMessage[] { chatMessage });
        }

        void ws_Error( object sender, SuperSocket.ClientEngine.ErrorEventArgs e ) {
            BeginWork();
        }

        void ws_DataReceived( object sender, DataReceivedEventArgs e ) {
        }

        void ws_Closed( object sender, EventArgs e ) {
            BeginWork();
        }

        void ws_Opened( object sender, EventArgs e ) {
            _joinSended = DateTime.Now;
        /*    ws.Send(
                "{\"command\":\"login\",\"message\":\"{\\\"login\\\":\\\"\\\",\\\"password\\\":\\\"\\\",\\\"channel\\\":\\\"#" + _name + "\\\"}\"}");
      */  }

        public override void Create( string streamerUri, string id ) {
            this._id = this.Label = id;
            this._name = this.Uri = SetKeywords( streamerUri );
            this.Tooltip = "cybergame: " + streamerUri + " ЧАТ НЕ ЗАГРУЖАЕТСЯ!";
            this.Status = true;
            BeginWork();
        }


        public override void Destroy() {
          /*  if (ws != null) {
                Unsub();
                if( ws.State == WebSocketState.Open )
                    ws.Close();
                ws = null;
            }*/
        }
    }
}
