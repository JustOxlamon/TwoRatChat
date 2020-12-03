// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using dotIRC;
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
using System.Windows.Threading;
using TwoRatChat.Model;
using TwoRatChat.Main.Helpers;

namespace TwoRatChat.Main.Sources {
    public class Twitch_ChatSource : TwoRatChat.Model.ChatSource {
        #region smiles
        //public const string UpdateSmilesUri = "https://api.twitch.tv/kraken/chat/emoticons";

        //internal class EmoticonImage {
        //    [JsonProperty( PropertyName = "emoticon_set" )]
        //    public int? Set { get; set; }
        //    [JsonProperty( PropertyName = "url" )]
        //    public string Uri { get; set; }
        //    [JsonProperty( PropertyName = "height" )]
        //    public int? Height { get; set; }
        //    [JsonProperty( PropertyName = "width" )]
        //    public int? Width { get; set; }
        //}

        //internal class Emoticon {
        //    [JsonProperty( PropertyName = "images" )]
        //    public EmoticonImage[] Images { get; set; }
        //    [JsonProperty( PropertyName = "regex" )]
        //    public string Regex { get; set; }

        //    public bool AllowToAdd() {
        //        for (int j = 0; j < Images.Length; ++j)
        //            if (!Images[j].Width.HasValue
        //                &&
        //                !Images[j].Height.HasValue)
        //                return false;

        //        for (int j = 0; j < Images.Length; ++j) {
        //            if (!Images[j].Set.HasValue)
        //                return true;
        //            //if (Images[j].Set.HasValue && Images[j].Set.Value == 1504)
        //            //    return true;
        //        }

        //        return false;
        //    }

        //    public string GetUri() {
        //        for (int j = 0; j < Images.Length; ++j)
        //            if (!Images[j].Set.HasValue)
        //                return Images[j].Uri;
        //        return null;
        //    }
        //}

        //internal class Emoticons {
        //    [JsonProperty( PropertyName = "emoticons" )]
        //    public Emoticon[] EmoticonsArray { get; set; }
        //}

        //internal class premiumEmoticonImage {
        //    [JsonProperty( PropertyName = "state" )]
        //    public string State { get; set; }
        //    [JsonProperty( PropertyName = "regex" )]
        //    public string regex { get; set; }
        //    [JsonProperty( PropertyName = "url" )]
        //    public string Uri { get; set; }
        //    [JsonProperty( PropertyName = "height" )]
        //    public int? Height { get; set; }
        //    [JsonProperty( PropertyName = "width" )]
        //    public int? Width { get; set; }
        //}

        //internal class premiumEmoticons {
        //    [JsonProperty( PropertyName = "emoticons" )]
        //    public premiumEmoticonImage[] EmoticonsArray { get; set; }
        //}

        internal class images {
            [JsonProperty( PropertyName = "image" )]
            public string image { get; set; }
        }

        internal class cbadges {
            [JsonProperty( PropertyName = "global_mod" )]
            public images globalModerator { get; set; }
            [JsonProperty( PropertyName = "admin" )]
            public images admin { get; set; }
            [JsonProperty( PropertyName = "broadcaster" )]
            public images broadcaster { get; set; }
            [JsonProperty( PropertyName = "mod" )]
            public images moderator { get; set; }
            [JsonProperty( PropertyName = "staff" )]
            public images staff { get; set; }
            [JsonProperty( PropertyName = "turbo" )]
            public images turbo { get; set; }
            [JsonProperty( PropertyName = "subscriber" )]
            public images subscriber { get; set; }
        }

        internal class user {
            [JsonProperty( PropertyName = "_id" )]
            public int Id;
            [JsonProperty( PropertyName = "name" )]
            public string Name;
            [JsonProperty( PropertyName = "display_name" )]
            public string DisplayName;
        }

        internal class follower {
            [JsonProperty( "created_at" )]
            public DateTime CreatedAt;
            [JsonProperty( "user" )]
            public user User;
        }

        internal class followers {
            [JsonProperty( "follows" )]
            public follower[] follow;
            [JsonProperty( "_total" )]
            public int Total;
        }

        HashSet<string> _preventRefollow = new HashSet<string>();
        cbadges badges;

        private void updateBadges( string userName ) {
            if (badges == null) {

                try {
                    WebClient wc = new WebClient();
                    string s = wc.DownloadString( "https://api.twitch.tv/kraken/chat/" + userName + "/badges" );
                    badges = JsonConvert.DeserializeObject<cbadges>( s );
                } catch {
                    badges = null;
                }
            }
        }

        #endregion

        readonly Timer _timer;
        readonly Timer _pingTimer;
      //  TwitchFollowersController _controller;
        string _id;
        readonly string _apiKey = "d45e19x896zb5hviy43ify8p6gmp4ei";
        string _streamerNick;
        IrcClient _ircClient;
        IrcRegistrationInfo _regInfo;
        readonly Regex _parsingRegex = new Regex(@"^(:(?<prefix>\S+) )?(?<command>\S+)( (?!:)(?<params>.+?))?( :(?<trail>.+))?$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        DateTime? _pingSended = null;
        readonly Regex _linkRegex = new Regex( @"(http|ftp|https)://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?", RegexOptions.IgnoreCase );
        const string _smilePlaceHolder = "☻♥%SMILE_{0}%♥☻";

        protected override void disposeManaged() {
            _timer?.Dispose();
            _ircClient?.Dispose();
        }

        public Twitch_ChatSource( Dispatcher dispatcher )
            : base( dispatcher ) {

            _timer = new Timer( 500 );
            _timer.Elapsed += _timer_Elapsed;

            _pingTimer = new Timer( 5000 );
            _pingTimer.Elapsed += _pingTimer_Elapsed;
        }

       

        public override void ReloadChatCommand() {
            //MessageError("Chat reloading...");
            StartReconnect();
        }

        bool ParseIrcMessageWithRegex( string message, out string prefix, out string command, out string[] parameters ) {
            //:justinfan84912118.tmi.twitch.tv 366 justinfan84912118 #pokerstaples :End of /NAMES list
            //:tmi.twitch.tv CAP *ACK :twitch.tv/commands
            //@color=#8A2BE2;display-name=Kamiiiiiiiii;emotes=;subscriber=0;turbo=0;user-type= :kamiiiiiiiii!kamiiiiiiiii@kamiiiiiiiii.tmi.twitch.tv PRIVMSG #pokerstaples :^ lol
            string trailing = null;
            prefix = command = String.Empty;
            parameters = new string[] { };

            Match messageMatch = _parsingRegex.Match(message);

            if (messageMatch.Success) {
                prefix = messageMatch.Groups["prefix"].Value;
                command = messageMatch.Groups["command"].Value;
                parameters = messageMatch.Groups["params"].Value.Split(' ');
                trailing = messageMatch.Groups["trail"].Value;

                if (!String.IsNullOrEmpty(trailing))
                    parameters = parameters.Concat(new string[] { trailing }).ToArray();
                return true;
            }
            return false;
        }

        string getUserName( string prefix ) {
            string[] nm = prefix.Split('!');
            if (nm.Length == 0 || nm[0].Length < 2)
                return "";
            return
                nm[0].Substring(0, 1).ToUpper() +
                nm[0].Substring(1);
        }


        public override string Id { get { return "twitchtv"; } }

        void _timer_Elapsed( object sender, ElapsedEventArgs e ) {
            _timer.Stop();
            Reconnect();
        }

        private void StartReconnect() {
            try {
                this.Status = false;
                _timer.Stop();
                Unsub();
                _timer.Start();
            } catch {

            }
        }

        private void Unsub(){
            if (_ircClient != null) {
                _ircClient.Connected -= IrcClient_Connected;
                _ircClient.ProtocolError -= IrcClient_ProtocolError;
                _ircClient.Error -= IrcClient_Error;
                _ircClient.Disconnected -= IrcClient_Disconnected;
                _ircClient.RawMessageReceived -= IrcClient_RawMessageReceived;
                _ircClient.ConnectFailed -= IrcClient_ConnectFailed;
                _ircClient.MotdReceived -= IrcClient_MotdReceived;
                _ircClient.PongReceived -= IrcClient_PongReceived;
                _ircClient.PingReceived -= IrcClient_PingReceived;
            }
        }

        private void MessageError( string Error, string userName = "*SYSTEM*" ) {
            //List<ChatMessage> msgs = new List<ChatMessage>(); 
            //msgs.Add(new ChatMessage() {
            //    Date = DateTime.Now,
            //    Name = userName,
            //    Text = Error,
            //    Source = this,
            //    //Form = 0,
            //    ToMe = true,
            //    Id = _id
            //});

            //newMessagesArrived(msgs);
        }


        private void Reconnect() {
            this.Tooltip = string.Format("twitch: {0}?", _streamerNick);
            this.Header = "Checking...";

            if (_ircClient != null) {
                _ircClient.Disconnect();
                _ircClient = null;
            }

            _ircClient = new IrcClient();
            //IrcClient. = "TWITCHCLIENT 3";
            _ircClient.PongReceived += IrcClient_PongReceived;
            _ircClient.PingReceived += IrcClient_PingReceived;
            _ircClient.Connected += IrcClient_Connected;
            _ircClient.ProtocolError += IrcClient_ProtocolError;
            _ircClient.Error += IrcClient_Error;
            _ircClient.Disconnected += IrcClient_Disconnected;
            _ircClient.RawMessageReceived += IrcClient_RawMessageReceived;
            _ircClient.ConnectFailed += IrcClient_ConnectFailed;
            _ircClient.MotdReceived += IrcClient_MotdReceived;

            //IrcClient.

            Random rnd = new Random();
            string s = "justinfan" + rnd.Next(100000000);
            _regInfo = new IrcUserRegistrationInfo() {
                NickName = s,
                UserName = s,
                Password = "blah",
                RealName = s
            };

            string[] dat = Main.Properties.Settings.Default.url_Twitch.Split(':');
            try {
                int port = int.Parse(dat[1]);
                _ircClient.Connect(dat[0], port, false, _regInfo);
            } catch ( Exception e ) {
                StartReconnect();
                MessageError("Connection error: " + e.Message);
            }
        }

        int _pongCounter = 0;
        private void _pingTimer_Elapsed( object sender, ElapsedEventArgs e ) {
            if( _pingSended.HasValue ) {
                if( (DateTime.Now - _pingSended.Value).TotalSeconds > 30 ) {
                    StartReconnect();
                    _pingTimer.Stop();
                }
            } else {
                _pingSended = DateTime.Now;
                _ircClient?.Ping();

                if( _pongCounter++ > 10 ) {
                    _pongCounter = 0;
                    _ircClient.SendRawMessage( "PONG tmi.twitch.tv\r\n" );
                }
            }
        }

        private void IrcClient_PingReceived( object sender, IrcPingOrPongReceivedEventArgs e ) {
         //   _pingSended = null;
        }

        private void IrcClient_PongReceived( object sender, IrcPingOrPongReceivedEventArgs e ) {
            _pingSended = null;
        }

        void IrcClient_MotdReceived( object sender, EventArgs e ) {
            _ircClient.SendRawMessage( "CAP REQ :twitch.tv/commands" );
            _ircClient.SendRawMessage( "CAP REQ :twitch.tv/tags" );


           // :tmi.twitch.tv USERSTATE #channel
            this.Tooltip = string.Format("twitch: {0}", _streamerNick);
        }

        void IrcClient_Error( object sender, IrcErrorEventArgs e ) {
            this.Tooltip = string.Format( "twitch: error" );
            this.Header = "Error";
            MessageError("Network Irc error: " + e.Error.Message);
            StartReconnect();
        }

        void IrcClient_ProtocolError( object sender, IrcProtocolErrorEventArgs e ) {
            this.Tooltip = string.Format( "twitch: error" );
            this.Header = "Proto Error";
            MessageError("IRC error: " + e.Message);
            StartReconnect();
        }

        void IrcClient_ConnectFailed( object sender, IrcErrorEventArgs e ) {
            this.Tooltip = string.Format( "twitch: invalid login" );
            this.Header = "Invalid name";
            MessageError("Invalid login: " + e.Error.Message);
            StartReconnect();
        }

        public static string ToDividedString<T>( IEnumerable<T> This, string Divider = ",", bool NeedStarterDivider = false ) {
            if (This == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            foreach (var item in This) {
                if (NeedStarterDivider) {
                    sb.AppendFormat("{0}{1}", Divider, item);
                } else {
                    sb.Append(item);
                }

                NeedStarterDivider = true;
            }
            return sb.ToString();
        }

        /*
:jtv!jtv@jtv.tmi.twitch.tv PRIVMSG #absnerdity :SPECIALUSER berndlauert8 subscriber
:jtv!jtv@jtv.tmi.twitch.tv PRIVMSG #absnerdity :SPECIALUSER berndlauert8 subscriber
:jtv!jtv@jtv.tmi.twitch.tv PRIVMSG #absnerdity :USERCOLOR berndlauert8 #0000FF
:jtv!jtv@jtv.tmi.twitch.tv PRIVMSG #absnerdity :USERCOLOR berndlauert8 #0000FF
:jtv!jtv@jtv.tmi.twitch.tv PRIVMSG #absnerdity :EMOTESET berndlauert8 [8742]
:jtv!jtv@jtv.tmi.twitch.tv PRIVMSG #absnerdity :EMOTESET berndlauert8 [8742]
 */
        Dictionary<string, HashSet<int>> _emotesets = new Dictionary<string, HashSet<int>>();
        Dictionary<string, List<System.Uri>> _bages = new Dictionary<string, List<Uri>>();

        private List<System.Uri> getBages( string nickname ) {
            if( _bages.TryGetValue( nickname, out List<Uri> u ) )
                return u;
            return new List<System.Uri>();
        }

        void addBage( string nickname, string id ) {
            if (badges != null) {

                List<System.Uri> u;
                if (!_bages.TryGetValue( nickname, out u ))
                    _bages[nickname] = u = new List<Uri>();

                Uri x;
                switch (id) {
                    case "subscriber":
                        x = new Uri(badges.subscriber.image);
                        if (!u.Contains( x ))
                            u.Add( x );
                        break;

                    case "turbo":
                        x = new Uri(badges.turbo.image);
                        if (!u.Contains( x ))
                            u.Add( x );
                        break;

                    case "moderator":
                        x = new Uri(badges.moderator.image);
                        if (!u.Contains( x ))
                            u.Add( x );
                        break;

                    case "broadcaster":
                        x = new Uri( badges.broadcaster.image );
                        if (!u.Contains( x ))
                            u.Add( x );
                        break;
                }
            }
        }

        void addUserEmoteset( string nick, string set ) {
            if( !_emotesets.TryGetValue( nick, out HashSet<int> x ) )
                _emotesets[nick] = x = new HashSet<int>();

            string[] xx = set.Substring( 1, set.Length - 2 ).Split( ',' );
            for (int j = 0; j < xx.Length; ++j)
                x.Add( int.Parse( xx[j] ) );

        }

        class Emo {
            public int start;
            public int end;
            public string id;
        }

        string doAttachEmotes(string text, string sets) {
            //34:9-17,28-36,47-55/22639:0-7,19-26,38-45
            //" + uu.ToString() + "[/sml]
            const string emoteUrl = "[sml]http://static-cdn.jtvnw.net/emoticons/v1/{0}/1.0[/sml]";

      //      Console.WriteLine( "1=>{0}", text );

            List<Emo> emos = new List<Emo>();

            foreach ( var emote in sets.Split( '/' ) ) {
                string[] d = emote.Split( ':' );

                foreach ( var r in d[1].Split( ',' ) ) {
                    string[] se = r.Split( '-' );

                    int s = int.Parse( se[0] );
                    int e = int.Parse( se[1] );

                    emos.Add( new Emo() { id = d[0], start = s, end = e } );

                  //  Console.WriteLine( "{0} => emote: {1}", text.Substring( s, e - s + 1 ), d[0] );
                }
            }

            foreach( var e in from b in emos
                              orderby b.start descending
                              select b ) {

                text =
                    text.Substring( 0, e.start ) +
                    string.Format( emoteUrl, e.id ) +
                    text.Substring( e.end+1 );
                    
            }


            //Console.WriteLine( "2=>{0}", text );
            return text;
        }

        //   string dots = ".";
        void IrcClient_RawMessageReceived(object sender, IrcRawMessageEventArgs e) {
            string prefix, command;
            Dictionary<string, string> ptags = new Dictionary<string, string>();
            string[] parameters;

            //Console.WriteLine( e.RawContent );


            try {
                if ( e.RawContent.StartsWith( "@" ) ) {
                    // TAGged command
                    int n = e.RawContent.IndexOf( ' ' );
                    if ( n > 0 ) {
                        string[] tags = e.RawContent.Substring( 1, n - 1 ).Split( ';' );
                        foreach ( var tag in tags ) {
                            string[] data = tag.Split( '=' );
                            ptags[data[0]] = data[1];
                        }

                        string txt = e.RawContent.Substring( n + 1 );
                        ParseIrcMessageWithRegex( txt, out prefix, out command, out parameters );

                        //@color=#00B4CC;display-name=ilittle17;emotes=501:9-10;subscriber=1;turbo=1;user-type=mod
                        //@color=#0000FF;display-name=WinBotCity;emotes=;subscriber=1;turbo=0;user-type=
                        //@color=#C2CC00;display-name=Event0011;emotes=;subscriber=1;turbo=1;user-type= 
                    } else {
                        return;
                    }
                } else {
                    // typical IRCv2 command
                    ParseIrcMessageWithRegex( e.RawContent, out prefix, out command, out parameters );
                }



                List<ChatMessage> msgs = new List<ChatMessage>();
                //List<ChatCommand> cmds = new List<ChatCommand>();
                string username = getUserName( prefix );
                switch ( command ) {
                    /* case "001":
                         this.Status = true;
                         this.Tooltip = string.Format( "twitch channel {0} not found!", StreamerNick );
                         Destroy();
                         break;*/

                    case "PART":
                    case "JOIN":
                        //Header = "http://twitch.tv, " + StreamerNick + dots;
                        this.Status = true;
                        this.Tooltip = string.Format( "twitch: {0}", _streamerNick );
                        this.Header = "Joined";
                        break;

                    case "MODE":
                        //if (parameters[1] == "+o") {
                        //    addBage( parameters[2].ToLower(), "moderator" );
                        //}
                        break;

                    case "CLEARCHAT":
                        fireOnRemoveUserMessages( parameters[1] );
                        break;

                    case "PRIVMSG":
                        //Header = "http://twitch.tv, " + StreamerNick;
                        if ( username != "Jtv" ) {

                            string text = parameters[1];

                            string emotes = ptags.SafeGet( "emotes", "" );
                            if ( !string.IsNullOrEmpty( emotes ) ) {

                                // FUCK emotes
                                text = doAttachEmotes( text, emotes );
                            } else {
                                text = Regex.Replace( text,
                                    @"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)",
                                    "[url]$1[/url]" );
                            }
                            // actualize badges!
                            //addBage
                            //@color=#00B4CC;display-name=ilittle17;emotes=501:9-10;subscriber=1;turbo=1;user-type=mod
                            if ( Properties.Settings.Default.twitch_ShowSubsIcon )
                                if ( ptags.SafeGet( "subscriber", "0" ) == "1" )
                                    addBage( username.ToLower(), "subscriber" );

                            if ( ptags.SafeGet( "turbo", "0" ) == "1" )
                                addBage( username.ToLower(), "turbo" );

                            if ( ptags.SafeGet( "user-type", "0" ) == "mod" )
                                addBage( username.ToLower(), "moderator" );

                            //parameters[1] = "Скажите позязя название сией композицииKappa";
                            if ( string.Compare( username, _streamerNick, true ) == 0 )
                                addBage( username.ToLower(), "broadcaster" );

                            string nick = ptags.SafeGet( "display-name", username );

                            if ( string.IsNullOrEmpty( nick ) )
                                nick = username;

                            var msg = new ChatMessage() {
                                Date = DateTime.Now,
                                Name = nick,
                                Text = Clean(text), //ReplaceSmiles( username.ToLower(), parameters[1] ),
                                Source = this,
                                //Form = 0,
                                ToMe = this.ContainKeywords( parameters[1] ),
                                Id = _id
                            };

                            if ( Properties.Settings.Default.twitch_AllowUseColors )
                                msg.Color = ptags.SafeGet( "color", "" );
                            else
                                msg.Color = "";

                            msg.AddBadges( getBages( username.ToLower() ).ToArray() );

                            msgs.Add( msg );
                        } else {
                         
                        }
                        break;

                    default:
                        break;
                }

                if ( msgs.Count > 0 )
                    newMessagesArrived( msgs );
                //if ( cmds.Count > 0 )
                //    newCommandsArrived( cmds );
            } catch ( Exception ee ) {
                App.Log( '!', "Twitch error: {0}", ee );
                this.Header = "XError";
                MessageError( "Oxlamon error: " + ee.Message );
                StartReconnect();
            }
        }

        private string Clean(string text) {
            string t = JsonConvert.ToString( text );
            return t.Substring( 1, t.Length - 2 );
        }

        void IrcClient_Disconnected( object sender, EventArgs e ) {
            this.Tooltip = string.Format("twitch: ?");
            this.Header = "Lost";

            MessageError( "Disconnected...");
            StartReconnect();
        }

        void IrcClient_Connected( object sender, EventArgs e ) {
            _pingTimer.Start();
            _ircClient.Channels.Join("#" + _streamerNick.ToLowerInvariant());
            this.Header = "Connected";
        }

        public override void Create( string streamerUri, string id ) {
            this.Uri = _streamerNick = SetKeywords( streamerUri );
            this.Label = _id = id;

         //   UpdateSmiles();
            updateBadges( _streamerNick );
            Reconnect();
        }

        public override void Destroy() {
           // this._controller.Stop();
            Unsub();
            _timer.Stop();
            if (_ircClient != null)
                _ircClient.Disconnect();
            _ircClient = null;
        }

        string _streamerID = null;
        bool _allowUpdateCount = true;
        public override void UpdateViewerCount() {
            if( _allowUpdateCount ) {
                _allowUpdateCount = false;
                if( !string.IsNullOrEmpty( _streamerNick ) ) {
                    if( string.IsNullOrEmpty( _streamerID ) ) {
                        this.Header = "Find ID...";
                        WebClient wc = new WebClient();
                        wc.Headers.Add( "Accept: application/vnd.twitchtv.v5+json" );
                        wc.Headers.Add( "Client-ID: " + _apiKey );
                        App.Log( ' ', "Twitch Find ID for: {0}", _streamerNick );
                        wc.DownloadStringCompleted += ( a, b ) => {
                            if( b.Error == null ) {
                                try {
                                    dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject( b.Result );
                                    if( response._total == 1 ) {
                                        _streamerID = response.users[0]._id;
                                        this.Header = "Loading..";
                                        App.Log( ' ', "Twitch Find ID: {0} is {1}", _streamerNick, _streamerID );
                                    } else {
                                        App.Log( ' ', "Twitch Find ID: {0} not found, stream offline?", _streamerNick );
                                        this.ViewersCount = null;
                                        this.Header = "Name?";
                                        //int h = (int)response.stream.viewers;
                                        //this.ViewersCount = h;
                                        //this.Header = string.Format( "{0}/{1}",
                                        //    h,
                                        //    response.stream.channel.followers );
                                    }
                                } catch( Exception ee ) {
                                    App.Log( '!', "Twitch Find ID error: {0}", ee );

                                    this.ViewersCount = null;
                                    this.Header = "Err";
                                }
                            } else {
                                App.Log( ' ', "Twitch Find ID {0} completed: {1}", _streamerNick, b.Error );

                            }
                            _allowUpdateCount = true;
                        };
                        wc.DownloadStringAsync( new System.Uri( "https://api.twitch.tv/kraken/users?login=" + _streamerNick ) );
                    } else {
                        WebClient wc = new WebClient();
                        wc.Headers.Add( "Accept: application/vnd.twitchtv.v5+json" );
                        wc.Headers.Add( "Client-ID: " + _apiKey );
                        wc.DownloadStringCompleted += ( a, b ) => {
                            if( b.Error == null ) {
                                try {
                                    dynamic response = Newtonsoft.Json.JsonConvert.DeserializeObject( b.Result );
                                    if( response.stream == null ) {
                                        this.ViewersCount = null;
                                        this.Header = "Offline";
                                    } else {
                                        int h = (int)response.stream.viewers;
                                        this.ViewersCount = h;
                                        this.Header = string.Format( "{0}/{1}",
                                            h,
                                            response.stream.channel.followers );
                                    }
                                } catch {
                                    this.ViewersCount = null;
                                    this.Header = "Err";
                                }

                            }
                            _allowUpdateCount = true;
                        };
                        wc.DownloadStringAsync( new System.Uri( "https://api.twitch.tv/kraken/streams/" + _streamerID ) );
                    }
                }
            }
        }
    }
}
