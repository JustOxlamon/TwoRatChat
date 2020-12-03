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
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Windows.Threading;

namespace TwoRatChat.Main.Sources {
    public class Sc2Tv_ChatSource: TwoRatChat.Model.ChatSource, IDisposable {
        const string updateSmilesUri = "http://chat.sc2tv.ru/js/smiles.js";
        Regex extractSmile = new Regex("\\:s\\:\\w\\w.*?\\:");
        Regex caps = new Regex( "<span.*?title=\"(.*?)\">.*?span>" );

        const string badge_flame = "http://chat.sc2tv.ru/img/antidonate.png";
        const string badge_donater = "http://chat.sc2tv.ru/img/donate_01.png";

        Dictionary<string, DateTime> _activeViewers = new Dictionary<string, DateTime>();
        TimeZoneInfo _timeZone;

        public void Dispose() {
            _retriveTimer.Dispose();
        }


        internal class Message {
            [JsonProperty(PropertyName = "channelId")]
            public int ChannelId { get; set; }

            [JsonProperty(PropertyName = "date")]
            public DateTime Date { get; set; }

            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "uid")]
            public int Uid { get; set; }

            [JsonProperty(PropertyName = "roleIds")]
            public int[] Rids { get; set; }

            [JsonProperty(PropertyName = "message")]
            public string Text { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            public int NeedToDelete { get; set; }
        }

        internal class Messages {
            [JsonProperty(PropertyName = "messages")]
            internal Message[] Content { get; set; }
        }

        internal class EmoticonImage {
            [JsonProperty(PropertyName = "code")]
            public string Code { get; set; }
            [JsonProperty(PropertyName = "img")]
            public string Img { get; set; }
        }

        internal class Emoticons {
            [JsonProperty(PropertyName = "smiles")]
            public EmoticonImage[] Smiles { get; set; }
        }

        Timer _retriveTimer;
        List<Message> _cache;
        Dictionary<string, string> _smiles;
        int _streamerId;
        string _streamerNick;
        //string _streamURI;
        string _id;
        bool allowAddMessages;

        public override string Id { get { return "sc2tv"; } }

        public Sc2Tv_ChatSource( Dispatcher dispatcher )
            : base(dispatcher) {
        }

        public override void UpdateViewerCount() {
            int n = 0;
            int lc = Main.Properties.Settings.Default.sc2tv_lastCount;
            DateTime nn = DateTime.Now.ToUniversalTime();

            foreach (var x in _activeViewers.Keys.ToArray()) {
                var dt = nn - _activeViewers[x];// -ts;

                if (dt.TotalMinutes <= lc)
                    n++;
                else
                    if (dt.TotalMinutes > 20)
                        _activeViewers.Remove( x );
            }
            if (n == 0) {
                this.ViewersCount = null;
                this.Header = "";
            } else {
                this.Header = n.ToString();
                this.ViewersCount = n;
            }
        }

        void _retriveTimer_Tick( object sender, EventArgs e ) {
            _retriveTimer.Stop();

            if (_streamerId != 0)
                loadChat(_streamerId);
        }

        private void loadSmiles() {
            _smiles = new Dictionary<string, string>();
            try {
                WebClient wc = new WebClient();
                string js = wc.DownloadString(updateSmilesUri);
                _smiles.Clear();

                int start = js.IndexOf('[');
                int end = js.IndexOf(';');

                string s = "{\"smiles\":" + js.Substring(start, end - start) + "}";

                Emoticons o = JsonConvert.DeserializeObject<Emoticons>(s);

                Directory.CreateDirectory( App.DataLocalFolder + "/img" );
                Directory.CreateDirectory( App.DataLocalFolder + "/img/sc2tv" );

                foreach ( var smile in o.Smiles ) {
                    string[] d = smile.Img.Split( '?' );

                    string fileName = App.DataLocalFolder + "/img/sc2tv/" + d[0];

                    if ( !File.Exists( fileName ) ) {
                        // dowload and save

                        WebClient wsc = new WebClient();

                        byte[] data = wsc.DownloadData( "http://chat.sc2tv.ru/img/" + smile.Img );
                        File.WriteAllBytes( fileName, data );

                    }
                    _smiles[":s" + smile.Code] = "asset://rat/img/sc2tv/" + d[0];
                }
            } catch {
            }
        }

        private class MyWebClient : WebClient {
            protected override WebRequest GetWebRequest( Uri uri ) {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 1000;
                return w;
            }
        }

        void loadChat( int ChannelId ) {
            _retriveTimer.Stop();

            MyWebClient wc = new MyWebClient();
            wc.Headers.Add("user-agent", "TwoRatChat");
            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(( a, b ) => {
                if (b.Error == null) {
                    try {
                        Messages messages = JsonConvert.DeserializeObject<Messages>(b.Result);
                        if (messages != null)
                            updateMessages(messages);
                    } catch ( Exception err ) {
                        Console.WriteLine( err.ToString() );
                    }
                } else {
                    // Кстати, тут можно нарисовать ошибку сети.
                }

                if (_retriveTimer != null)
                    _retriveTimer.Start();
            });
            wc.DownloadStringAsync(new Uri(string.Format(Main.Properties.Settings.Default.url_Sc2tv, ChannelId)));
        }

        Message getById( int id ) {
            for (int j = 0; j < _cache.Count; ++j)
                if (_cache[j].Id == id)
                    return _cache[j];
            return null;
        }

        void updateMessages( Messages msgs ) {
            this.Status = true;
            for (int j = 0; j < _cache.Count; ++j)
                _cache[j].NeedToDelete--;

            var msg = (from b in msgs.Content
                       orderby b.Date
                       select b).ToArray();

            List<Message> NewMessage = new List<Message>();
            //////////
            for (int j = 0; j < msg.Length; ++j) {
                Message m = getById(msg[j].Id);

                if (m == null) {
                    _cache.Add(msg[j]);

                    //Match mmm = ExtractSmile.Match(msg[j].Text);
                    //if (mmm.Success) {
                    //}
                    string text = caps.Replace( msg[j].Text, new MatchEvaluator( match => {
                        return "Предупреждение за CAPS / Abuse!";// match.Groups[1].Value;
                    } ) );


                    msg[j].Text = HttpUtility.HtmlDecode( extractSmile.Replace( text, new MatchEvaluator( ( match ) => {
                        return " " + match.Value + " ";
                    })).Trim());

                    NewMessage.Add(msg[j]);
                    msg[j].NeedToDelete = 10;
                } else {
                    m.NeedToDelete = 10;
                }
            }

            int oldC = _activeViewers.Count;

            foreach (var nm in NewMessage) {
                _activeViewers[nm.Name] = TimeZoneInfo.ConvertTimeToUtc( nm.Date, _timeZone );
            }

            if (oldC != _activeViewers.Count)
                UpdateViewerCount();

            ///////////
            //http://chat.sc2tv.ru/img/donate_01.png
            int i = 0;
            while (i < _cache.Count)
                if (_cache[i].NeedToDelete < 0)
                    _cache.RemoveAt(i);
                else
                    ++i;

            if (NewMessage.Count > 0) {

                if (allowAddMessages) {
                    newMessagesArrived( from b in NewMessage
                                        orderby b.Date
                                        select new TwoRatChat.Model.ChatMessage() {
                                            Date = b.Date,
                                            Name = b.Name,
                                            Text = SetUpSmiles( b.Text ),
                                            Source = this,
                                            Id = _id,
                                            ToMe = this.ContainKeywords( b.Text )
                                            //Form = 0,

                                        } .AddBadges( rolesToBadges( b.Rids ).ToArray() ) );
                } else {

                    var arra =( from b in NewMessage
                          orderby b.Date
                          select new TwoRatChat.Model.ChatMessage() {
                                            Date = b.Date,
                                            Name = b.Name,
                                            Text = SetUpSmiles( b.Text ),
                                            Source = this,
                                            Id = _id,
                                            ToMe = this.ContainKeywords( b.Text )
                                            //Form = 0,
                        }.AddBadges( rolesToBadges( b.Rids ).ToArray() ) ).ToArray();
              


                    newMessagesArrived( arra );
                }

            }

            allowAddMessages = true;
        }

        private List<System.Uri> rolesToBadges( int[] p ) {
            List<System.Uri> b = new List<Uri>();

            if (p.Contains( 24 ))
                b.Add( new Uri( badge_donater, UriKind.RelativeOrAbsolute ) );
            if (p.Contains( 35 ))
                b.Add( new Uri( badge_flame, UriKind.RelativeOrAbsolute ) );

            if (b.Count > 0)
                return b;

            return new List<System.Uri>();
        }

        private string SetUpSmiles( string text ) {
            string old = text;
            foreach (var smile in this._smiles)
                text = text.Replace(smile.Key, "[sml]" + smile.Value + "[/sml]");

            if( old != text ) {

            }

            text = url.Replace( text, new MatchEvaluator( a => {
                return "[url]" + a.Groups[1].Value + "[/url]";
            } ) );

            return text;
        }

        Regex url = new Regex("\\[url=(.*?)\\](.*?)\\[\\/url\\]");


        //http://sc2tv.ru/channel/%D0%BA%D0%BE%D1%80%D0%B5%D1%8F-20
        public override void Create( string streamerUri, string id ) {
            streamerUri = SetKeywords( streamerUri );
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById( "Russian Standard Time" );

            if (!streamerUri.StartsWith( "http://sc2tv.ru/channel" )) {
                streamerUri = "http://sc2tv.ru/channel/" + streamerUri;
            }

            var s = streamerUri.Split( new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries );
            _streamerNick = s[3];

            allowAddMessages = Main.Properties.Settings.Default.Chat_LoadHistory;

            this.Label = this._id = id;
            string _ChannelUri = this.Uri = streamerUri;
            this.Tooltip = "sc2tv: " + streamerUri;
            // ManualResetEvent mre = new ManualResetEvent(false);
            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            try {
                string Result = wc.DownloadString(new Uri(_ChannelUri, UriKind.RelativeOrAbsolute));

                //

                Regex rx = new Regex( "\"ajax_comments_nid\":\"(.*?)\"" );

                Match m = rx.Match(Result);
                if (m.Success) {
                    _streamerId = int.Parse(m.Groups[1].Value);
                    _streamerNick = "[b]" + _streamerNick + "[/b]";
                    AddKeyword( _streamerNick );
                    this.Tooltip = string.Format( "sc2tv: {0}", _streamerNick );
                } else {
                    _streamerId = 0;
                    this.Tooltip = string.Format("sc2tv: ?");
                    _streamerNick = "";
                    fireOnFatalError( FatalErrorCodeEnum.ChannelNotFound );
                    return;
                }
            } catch {
                _streamerId = 0;
                this.Tooltip = string.Format( "sc2tv: ?" );
                _streamerNick = "";
                fireOnFatalError( FatalErrorCodeEnum.ParsingError );
                return;
            }

            //LoadedMessages.Clear();
            //UpdateHeader();
            //UpdateSmiles();
            loadSmiles();
            _cache = new List<Message>();

            _retriveTimer = new Timer();
            _retriveTimer.Interval = 5000;// TimeSpan.FromSeconds(5.0);
            _retriveTimer.Elapsed += _retriveTimer_Tick;
            loadChat(_streamerId);
        }


        public override void Destroy() {
            _streamerId = 0;
            if( _retriveTimer != null )
                _retriveTimer.Stop();
        }
    }
}
