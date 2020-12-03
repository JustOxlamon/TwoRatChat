// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace TwoRatChat.Main.Sources {
    public class Gipsyteam_Source : TwoRatChat.Model.ChatSource {
        //http://www.gipsyteam.ru/chat/streams/messages/88.js?nocache=1411029944422
        const string _url = "http://www.gipsyteam.ru/chat/streams/messages/{0}.js?nocache={1}";
        const string _count = "http://www.gipsyteam.ru/chat/streams/messages/u{0}.js?nocache={1}";
        string _chatId;
        string _label;
        Timer _timer;

        protected override void disposeManaged() {
            _timer.Dispose();
        }

        static Regex _r = new Regex(">(.*?)\\\\u043");

        public Gipsyteam_Source( Dispatcher disp )
            : base(disp) {

            _timer = new Timer(1000);
            _timer.Elapsed += _timer_Elapsed;
        }

                  

        class LocalMessage : TwoRatChat.Model.ChatMessage {
            static Regex _cleanRegex = new Regex("<strong>(.*?)</strong>.*?<span>(.*?)<");

            public int _deleteMe;
            public readonly int _id;

            public LocalMessage( int id, string content )
                : base() {
                _id = id;
                _deleteMe = 100;

                content = content.Replace("\t", "").Replace("\n", "").Replace("\r", "");
                Match m = _cleanRegex.Match(content);
                if (m.Success) {
                    Date = DateTime.Now;
                    Name = m.Groups[1].Value;
                    Text = m.Groups[2].Value;
                } else
                    _id = 0;
            }
        }
    

        List<LocalMessage> _cache = new List<LocalMessage>();
       

        LocalMessage getById( int id ) {
            foreach (var msg in _cache)
                if (msg._id == id)
                    return msg;
            return null;
        }

         void _timer_Elapsed( object sender, ElapsedEventArgs e ){
            if (!string.IsNullOrEmpty(_chatId)) {
                _timer.Stop();
                LoadChat();
            }
        }

        private void LoadChat() {
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += ( a, b ) => {
                if (b.Error == null) {
                    try {
                        this.Status = true;
                        List<LocalMessage> add = new List<LocalMessage>();
                        foreach (KeyValuePair<string, Newtonsoft.Json.Linq.JToken> item in Newtonsoft.Json.JsonConvert.DeserializeObject(b.Result) as Newtonsoft.Json.Linq.JObject) {
                            int _toAdd = int.Parse(item.Key);
                            LocalMessage lm = getById(_toAdd);
                            if (lm == null) {
                                lm = new LocalMessage(_toAdd, (string)item.Value);
                                if (lm._id > 0) {
                                    lm.Source = this;
                                    lm.Id = this._label;
                                    lm.ToMe = this.ContainKeywords( lm.Text );
                                    add.Add(lm);
                                }
                                _cache.Add(lm);
                            } else {
                            }
                        }

                        if (add.Count > 0)
                            newMessagesArrived(add);
                    } catch ( Exception er ) {
                        App.Log( '?', "Gipsyteam error: {0}", er );
                    }
                } else {
                    //if( b.Error )
                    this.Status = false;
                }
                _timer.Start();
            };
            wc.DownloadStringAsync(new Uri(string.Format(_url, _chatId, DateTime.Now.ToBinary()), UriKind.Absolute));
        }

        public override string Id { get { return "gipsyteam"; } }

        public override void Create( string streamerUri, string id ) {
            this._chatId = this.Uri = SetKeywords( streamerUri );
            this._label = this.Label = id;

            _timer.Stop();
            LoadChat();

            this.Tooltip = string.Format("gipsyteam: {0}", this._chatId);

            WebClient wc = new WebClient();
            try {
                wc.DownloadString( new Uri( string.Format( _url, _chatId, DateTime.Now.ToBinary() ), UriKind.Absolute ) );
            } catch ( WebException we ) {
                if ( ((HttpWebResponse)we.Response).StatusCode == HttpStatusCode.NotFound ) {
                    fireOnFatalError( FatalErrorCodeEnum.ChannelNotFound );
                }
            } catch ( Exception err ) {
                App.Log( '?', "Error while create gipsyteam object: {0}", err );
                fireOnFatalError( FatalErrorCodeEnum.ParsingError );
            }

        }

        public override void Destroy() {
            this._timer.Stop();
            this._chatId = this.Uri = "";
            this._label = this.Label = "";
        }

        bool _inCountRequest = false;


        
        public override void UpdateViewerCount() {
            if (_inCountRequest) return;

            _inCountRequest = true;
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += ( a, b ) => {
                if (b.Error == null) {
                    try {
                        Match m = _r.Match(b.Result.Replace("\t", "").Replace("\n", "").Replace("\r", ""));
                        if ( m.Success ) {
                            int n = int.Parse( m.Groups[1].Value );
                            if ( n == 0 ) {
                                this.ViewersCount = null;
                                this.Header = "";
                            } else {
                                this.ViewersCount = n;
                                this.Header = n.ToString();
                            }
                        }
                    } catch {
                    }
                }
                _inCountRequest = false;
            };
            wc.DownloadStringAsync(new Uri(string.Format(_count, _chatId, DateTime.Now.ToBinary()), UriKind.Absolute));
      
        }
    }
}
