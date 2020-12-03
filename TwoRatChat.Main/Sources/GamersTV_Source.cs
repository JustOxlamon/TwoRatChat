// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Windows.Threading;

namespace TwoRatChat.Main.Sources {
    public class GamersTV_Source : TwoRatChat.Model.ChatSource {
        public GamersTV_Source(Dispatcher dispatcher)
            : base(dispatcher) {
        }

        bool _showMessages;

        public override string Id { get { return "gamerstv"; } }

        public override void Create(string streamerUri, string id) {
            this.Label = this._id = id;
            streamerUri = SetKeywords( streamerUri );

            _showMessages = Properties.Settings.Default.Chat_LoadHistory;

            try {
                int _did;
                if ( int.TryParse( streamerUri, out _did ) ) {
                    this.Uri = streamerUri;
                } else {
                    Regex _ix = new Regex( @"i(\d*?)\." );
                    streamerUri = this.Uri = _ix.Match( streamerUri ).Groups[1].Value;
                }

                _streamerId = int.Parse( streamerUri );
            } catch {
                this.Tooltip = string.Format( "gamerstv: ?" );
                fireOnFatalError( FatalErrorCodeEnum.ParsingError );
                return;
            }

            this.Tooltip = string.Format( "gamerstv: {0}?", streamerUri );
            this.Header = "";

            _cache = new List<Message>();

            _retriveTimer = new Timer();
            _retriveTimer.Interval = 5000;// TimeSpan.FromSeconds(5.0);
            _retriveTimer.Elapsed += _retriveTimer_Tick;

            //Header = streamerUri;

            loadSmiles();

            loadChat( _streamerId );
        }

        public override void Destroy() {
            _retriveTimer.Stop();
        }

        public override void UpdateViewerCount() {
        }




        const string _url = "http://gamerstv.ru/modules/ajax/chat.php?action=update&room_id={0}&room_type=streams&user_id=0";
        const string _smilesUri = "http://gamerstv.ru/smiles/smiles.js";

        List<Message> _cache;
        string _id;
        int _streamerId;
        DateTime? _date;
        Dictionary<string, string> _smiles;

        internal class TimeConverter : JsonConverter {
            public override bool CanConvert(Type objectType) {
                return objectType == typeof( DateTime );
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
                string sex = reader.Value.ToString();

                return DateTime.ParseExact( sex, "dd.MM HH:mm", CultureInfo.InvariantCulture );
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
                throw new NotImplementedException();
            }
        }

        internal class Message {
            [JsonProperty( PropertyName = "name" )]
            public string Name { get; set; }
            [JsonProperty( PropertyName = "text" )]
            public string Text { get; set; }
            [JsonProperty( PropertyName = "chat_id" )]
            public int Id { get; set; }
            [JsonProperty( PropertyName = "time" )]
            [JsonConverter( typeof( TimeConverter ) )]
            public DateTime Time { get; set; }

            [JsonIgnore]
            public int NeedToDelete { get; set; }
        }

        internal class Messages {
            [JsonProperty( PropertyName = "date" )]
            public DateTime Date { get; set; }

            [JsonProperty( PropertyName = "text" )]
            internal Message[] Content { get; set; }
        }

        internal class EmoticonImage {
            [JsonProperty( PropertyName = "code" )]
            public string Code { get; set; }
            [JsonProperty( PropertyName = "src" )]
            public string Img { get; set; }
        }

        internal class Emoticons {
            [JsonProperty( PropertyName = "img" )]
            public EmoticonImage[] Smiles { get; set; }
        }

        private void loadSmiles() {
            this.Status = true;
            _smiles = new Dictionary<string, string>();
            try {
                WebClient wc = new WebClient();
                string js = wc.DownloadString( _smilesUri );
                _smiles.Clear();

                Emoticons o = JsonConvert.DeserializeObject<Emoticons>( js );

                foreach ( var smile in o.Smiles )
                    _smiles[smile.Code] = "http://gamerstv.ru/smiles/" + smile.Img;
            } catch {
            }
        }
        Message getById(int id) {
            for ( int j = 0; j < _cache.Count; ++j )
                if ( _cache[j].Id == id )
                    return _cache[j];
            return null;
        }

        private string SetUpSmiles(string text) {
            foreach ( var smile in this._smiles )
                text = text.Replace( smile.Key, "[sml]" + smile.Value + "[/sml]" );
            return text;
        }

        void updateMessages(Messages msgs) {
            for ( int j = 0; j < _cache.Count; ++j )
                _cache[j].NeedToDelete--;

            var msg = (from b in msgs.Content
                       orderby b.Time
                       select b).ToArray();

            List<Message> NewMessage = new List<Message>();
            //////////
            for ( int j = 0; j < msg.Length; ++j ) {
                Message m = getById( msg[j].Id );

                if ( m == null ) {
                    _cache.Add( msg[j] );

                    msg[j].Text = HttpUtility.HtmlDecode( msg[j].Text );

                    NewMessage.Add( msg[j] );
                    msg[j].NeedToDelete = 60 * 5;
                } else {
                    m.NeedToDelete = 60 * 5;
                }
            }
            ///////////

            int i = 0;
            while ( i < _cache.Count )
                if ( _cache[i].NeedToDelete < 0 )
                    _cache.RemoveAt( i );
                else
                    ++i;

            if ( _showMessages )
                if ( NewMessage.Count > 0 ) {
                    newMessagesArrived( from b in NewMessage
                                        orderby b.Time
                                        select new TwoRatChat.Model.ChatMessage() {
                                            Date = b.Time,
                                            Name = b.Name,
                                            Text = SetUpSmiles( b.Text.Replace( "<u>", "[b]" ).Replace( "</u>", "[/b]" ) ),
                                            Source = this,
                                            Id = _id,
                                            ToMe = this.ContainKeywords( b.Text )
                                        } );
                }
            _showMessages = true;
        }

        void loadChat(int ChannelId) {
            WebClient wc = new WebClient();
            wc.Headers.Add( "user-agent", "TwoRatChat" );
            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler( (a, b) => {
                if ( b.Error == null ) {

                    if ( b.Result != "null" ) {
                        try {
                            Messages messages = JsonConvert.DeserializeObject<Messages>( b.Result );
                            if ( messages != null )
                                updateMessages( messages );
                            _date = messages.Date;
                        } catch ( Exception e ) {
                            Console.WriteLine( e.Message );
                        }
                    }
                } else {
                    // Кстати, тут можно нарисовать ошибку сети.
                }
                if ( _retriveTimer != null )
                    _retriveTimer.Start();
            } );

            string _s = string.Format( Properties.Settings.Default.url_GamersTV, ChannelId );

            //  if( _date.HasValue )
            //      _s += string.Format("&time={0:yyyy-MM-dd+HH:mm:ss}", _date);

            wc.DownloadStringAsync( new Uri( _s ) );
        }

        Timer _retriveTimer;

        void _retriveTimer_Tick(object sender, EventArgs e) {
            _retriveTimer.Stop();

            if ( _streamerId != 0 )
                loadChat( _streamerId );
        }

        protected override void disposeManaged() {
            _retriveTimer.Dispose();
        }
    }
}
