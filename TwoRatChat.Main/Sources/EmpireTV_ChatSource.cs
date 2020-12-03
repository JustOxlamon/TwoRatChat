// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Windows.Threading;
using TwoRatChat.Model;

namespace TwoRatChat.Main.Sources {
    public class EmpireTV_ChatSource : TwoRatChat.Model.ChatSource {
        internal class Message {
            //[JsonProperty(PropertyName = "c")]
            //public string ChannelId { get; set; }

            [JsonProperty(PropertyName = "c")]
            public long UnixDate { get; set; }


            [JsonProperty(PropertyName = "i")]
            public string Id { get; set; }

            //[JsonProperty(PropertyName = "u")]
            //public int Uid { get; set; }

            [JsonProperty(PropertyName = "m")]
            public string Text { get; set; }

            [JsonProperty(PropertyName = "n")]
            public string Name { get; set; }

            public int NeedToDelete { get; set; }
        }

        Timer _next;

        protected override void disposeManaged() {
            _next.Dispose();
        }

        string _ChannelUri = "";
        int _StreamerID = 0;
        string StreamerNick = "";
        string _id = "";
        List<Message> LoadedMessages = new List<Message>();
       
        Regex ExtractSmile = new Regex("\\:s\\:\\w\\w.*?\\:");

        static long unixTimestamp() {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = DateTime.Now - origin;
            return (long)Math.Floor(diff.TotalSeconds);
        }

        static DateTime UnixTimeStampToDateTime( long unixTimeStamp ) {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public override string Id { get { return "empire"; } }

        public EmpireTV_ChatSource( Dispatcher dispatcher )
            : base(dispatcher) {
        }

        public override void UpdateViewerCount() {
            this.ViewersCount = null;
        }

        void LoadChat( int ChannelId ) {
            WebClient wc = new WebClient();
            wc.Headers.Add("user-agent", "RatChat");
            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(( a, b ) => {
                if (b.Error == null) {
                    var messages = JsonConvert.DeserializeObject<Message[]>(b.Result);
                    if (messages != null)
                        UpdateMessages(messages);
                } else {
                    // Кстати, тут можно нарисовать ошибку сети.
                    this.Status = false;
                }
                if (_next != null)
                    _next.Start();
            });
            wc.DownloadStringAsync(new Uri(
                string.Format(Properties.Settings.Default.url_Empire,
                ChannelId, unixTimestamp())));
        }

        Message GetById( string id ) {
            for (int j = 0; j < LoadedMessages.Count; ++j)
                if (LoadedMessages[j].Id == id)
                    return LoadedMessages[j];
            return null;
        }

        private void UpdateMessages( Message[] msgs ) {
            this.Status = true;

            for (int j = 0; j < LoadedMessages.Count; ++j)
                LoadedMessages[j].NeedToDelete--;

            List<Message> NewMessage = new List<Message>();
            //////////
            for (int j = 0; j < msgs.Length; ++j) {
                Message m = GetById(msgs[j].Id);
                if (m == null) {
                    LoadedMessages.Add(msgs[j]);

                    msgs[j].Text = HttpUtility.HtmlDecode(msgs[j].Text);

                    NewMessage.Add(msgs[j]);
                    msgs[j].NeedToDelete = 60 * 5;
                } else {
                    m.NeedToDelete = 60 * 5;
                }
            }
            ///////////

            int i = 0;
            while (i < LoadedMessages.Count)
                if (LoadedMessages[i].NeedToDelete < 0)
                    LoadedMessages.RemoveAt(i);
                else
                    ++i;

            ////////////
            if (NewMessage.Count > 0) {
                //CasterAchivment.Temperature -= NewMessage.Count * 0.01;

                //foreach (var m in NewMessage)
                //    OnNewMessage(m);
                newMessagesArrived(from b in NewMessage
                                   //orderby b.Date
                                   select new ChatMessage() {
                                       Date = UnixTimeStampToDateTime(b.UnixDate),
                                       Name = b.Name,
                                       Text = PrepareText( b.Text ),
                                       Source = this,
                                       Id = _id,
                                       //Form = 0, 
                                       ToMe = this.ContainKeywords( b.Text )
                                   });
            }
        }

        private string PrepareText( string text ) {
            foreach (var v in _smiles)
                text = text.Replace(v.Key, "[sml]" + v.Value + "[/sml]");
            return text;
        }


        public override void Create( string streamerUri, string id ) {
            this.Label = _id = id;
            this.Uri = Header = _ChannelUri = SetKeywords( streamerUri );

            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            try {
                string Result = wc.DownloadString(new Uri(_ChannelUri, UriKind.RelativeOrAbsolute));
                LoadSmiles(Result);
                Regex rx = new Regex("title\\>(.*?)\\|");
                Match m = rx.Match(Result);
                if (m.Success) {
                    _StreamerID = int.Parse(_ChannelUri.Split('/')[4]);
                    StreamerNick = m.Groups[1].Value.Trim();
                } else {
                    _StreamerID = 0;
                    StreamerNick = "";
                   // return false;
                }
            } catch {
                _StreamerID = 0;
                StreamerNick = "";
              //  return false;
            }

            LoadedMessages.Clear();
           

            _next = new Timer();
            _next.Interval = 1000;
            _next.Elapsed += next_Elapsed;
            _next.Start();

            Header = "empiretv: " + StreamerNick;
         //   return true;
        }

        private void LoadSmiles( string page ) {
            _smiles.Clear();
            Regex r = new Regex("\"smiles\"\\:\\{(.*?)\\}");
            string[] smiles = r.Match(page).Groups[1].Value.Split(',');

            r = new Regex("\"(.*?)\".*?src=\\\\u0022(.*?)\\\\u0022");

            for (int j = 0; j < smiles.Length; ++j) {
                Match m = r.Match(smiles[j]);
                _smiles.Add(
                    Regex.Unescape(Regex.Unescape( m.Groups[1].Value )),
                    Regex.Unescape( m.Groups[2].Value ) );
            }
        }

        Dictionary<string, string> _smiles = new Dictionary<string, string>();

        void next_Elapsed( object sender, ElapsedEventArgs e ) {
            if (_StreamerID != 0) 
                if (_next != null) {
                    _next.Stop();
                    LoadChat(_StreamerID);
                }
        }

        public override void Destroy() {
            _StreamerID = 0;
            StreamerNick = "";
            _next.Stop();
            _next = null;
        }
    }
}
