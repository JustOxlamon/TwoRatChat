using dotIRC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using TwoRatChat.Model;

namespace TwoRatChat.Main.Sources {
    public class GohaTV_ChatSource : TwoRatChat.Model.ChatSource, IDisposable {
        IrcClient IrcClient;
        IrcUserRegistrationInfo regInfo;
        string StreamerChannel;
        Timer _timer;

        public void Dispose() {
            IrcClient?.Dispose();
            _timer?.Dispose();
        }


        #region Parsing
        Regex parsingRegex = new Regex( @"^(:(?<prefix>\S+) )?(?<command>\S+)( (?!:)(?<params>.+?))?( :(?<trail>.+))?$", RegexOptions.Compiled | RegexOptions.ExplicitCapture );
        bool ParseIrcMessageWithRegex( string message, out string prefix, out string command, out string[] parameters ) {
            string trailing = null;
            prefix = command = String.Empty;
            parameters = new string[] { };

            Match messageMatch = parsingRegex.Match( message );

            if (messageMatch.Success) {
                prefix = messageMatch.Groups["prefix"].Value;
                command = messageMatch.Groups["command"].Value;
                parameters = messageMatch.Groups["params"].Value.Split( ' ' );
                trailing = messageMatch.Groups["trail"].Value;

                if (!String.IsNullOrEmpty( trailing ))
                    parameters = parameters.Concat( new string[] { trailing } ).ToArray();
                return true;
            }
            return false;
        }
        #endregion

        void _timer_Elapsed( object sender, ElapsedEventArgs e ) {
            _timer.Stop();
            Reconnect();
        }


        private void StartReconnect() {
            this.Status = false;
            try {
                _timer.Stop();
                Unsub();
                _timer.Start();
            } catch (Exception er) {
                Console.WriteLine("ReplaceSmiles:{0}, {1}", GetType(), er);

            }
        }

        private void Unsub() {
            if (IrcClient != null) {
                IrcClient.Connected -= IrcClient_Connected;
                IrcClient.ProtocolError -= IrcClient_ProtocolError;
                IrcClient.Error -= IrcClient_Error;
                IrcClient.Disconnected -= IrcClient_Disconnected;
                IrcClient.RawMessageReceived -= IrcClient_RawMessageReceived;
                IrcClient.ConnectFailed -= IrcClient_ConnectFailed;
                IrcClient.MotdReceived -= IrcClient_MotdReceived;
            }
        }

        private void MessageError( string Error, string userName = "*SYSTEM*" ) {
            List<TwoRatChat.Model.ChatMessage> msgs = new List<TwoRatChat.Model.ChatMessage>();
            msgs.Add( new TwoRatChat.Model.ChatMessage() {
                Date = DateTime.Now,
                Name = userName,
                Text = Error,
                Source = this,
                //Form = 0,
                ToMe = true,
                Id = this.Label
            } );

            newMessagesArrived( msgs );
        }

        void IrcClient_Disconnected( object sender, EventArgs e ) {
            MessageError( "Disconnected..." );
            StartReconnect();
        }


        public override void ReloadChatCommand() {
            MessageError( "Chat reloading..." );
            StartReconnect();
        }

        private void Reconnect() {
            this.Status = false;
            if (IrcClient != null) {
                IrcClient.Disconnect();
                IrcClient = null;
            }

            IrcClient = new IrcClient();

            IrcClient.Connected += IrcClient_Connected;
            IrcClient.ProtocolError += IrcClient_ProtocolError;
            IrcClient.Error += IrcClient_Error;
            IrcClient.Disconnected += IrcClient_Disconnected;
            IrcClient.RawMessageReceived += IrcClient_RawMessageReceived;
            IrcClient.ConnectFailed += IrcClient_ConnectFailed;
            IrcClient.MotdReceived += IrcClient_MotdReceived;
            IrcClient.RawMessageSent += IrcClient_RawMessageSent;
           // IrcClient.
            //IrcClient.

            Random rnd = new Random();
            string s = "Guest" + rnd.Next( 1000, 99999 );
            regInfo = new IrcUserRegistrationInfo() {
                NickName = s,
                UserName = s,
                Password = null,
                RealName = s
            };

            string[] dat = Properties.Settings.Default.url_GohaTV.Split( ':' );
            try {
                int port = int.Parse( dat[1] );
                IrcClient.Connect( dat[0], port, false, regInfo );
            } catch (Exception e) {
                StartReconnect();
                MessageError( "Connection error: " + e.Message );
            }
        }

        void IrcClient_RawMessageSent( object sender, IrcRawMessageEventArgs e ) {
            Console.WriteLine( "SND: " + e.RawContent );
        }

        void IrcClient_MotdReceived( object sender, EventArgs e ) {
            //IrcClient.SendRawMessage( "TWITCHCLIENT 3" );
            //this.Header = string.Format( "twitch: {0}{1}", StreamerNick, dots );
            Console.WriteLine( "MOTD:" );
        }

        void IrcClient_Connected( object sender, EventArgs e ) {
            Console.WriteLine( "CONECTED:" );
            
            Random rnd = new Random();
            string s = "Guest" + rnd.Next( 1000, 99999 );
            string userID = rnd.NextDouble().ToString().Substring( 0, 8 );
            IrcClient.SendRawMessage("NICK " + s);
            IrcClient.SendRawMessage("USER "+userID+" gohaTV IRC Client" );

          //  IrcClient.Channels.Join( "#" + StreamerChannel.ToLowerInvariant() );
        }

        void IrcClient_Error( object sender, IrcErrorEventArgs e ) {
            this.Header = string.Format( "twitch: error" );
            MessageError( "Network Irc error: " + e.Error.Message );
            StartReconnect();
        }

        void IrcClient_ProtocolError( object sender, IrcProtocolErrorEventArgs e ) {
           // this.Header = string.Format( "twitch: error" );
           // MessageError( "IRC error: " + e.Message );
           // StartReconnect();
        }

        void IrcClient_ConnectFailed( object sender, IrcErrorEventArgs e ) {
            this.Header = string.Format( "twitch: invalid login" );
            MessageError( "Invalid login: " + e.Error.Message );
            StartReconnect();
        }


        public GohaTV_ChatSource( Dispatcher dispatcher )
            : base( dispatcher ) {

            _timer = new Timer( 1000 );
            _timer.Elapsed += _timer_Elapsed;
        }

        public override string Id {
            get { return "gohatv"; }
        }

        string getUserName( string prefix ) {
            string[] nm = prefix.Split( '!' );
            if (nm.Length == 0 || nm[0].Length < 2)
                return "";
            return
                nm[0].Substring( 0, 1 ).ToUpper() +
                nm[0].Substring( 1 );
        }

        void IrcClient_RawMessageReceived( object sender, IrcRawMessageEventArgs e ) {
            Console.WriteLine( "RAW IN: " + e.RawContent );

            string prefix, command;
            string[] parameters;

            try {
                ParseIrcMessageWithRegex( e.RawContent, out prefix, out command, out parameters );
                List<ChatMessage> msgs = new List<ChatMessage>();

                switch (command) {
                    case "422":
                        this.Status = true;
                        IrcClient.SendRawMessage( "JOIN #" + StreamerChannel );
                        //IrcClient.SendRawMessage( "PRIVMSG NickServ :IDENTIFY 123" );
                        break;

                    case "332":
                        break;

                    case "353":
                        break;

                    case "PRIVMSG":
                        var msg = new ChatMessage() {
                            Date = DateTime.Now,
                            Name = getUserName( prefix ),
                            Text = replaceSmiles( parameters[1] ),
                            Source = this,
                            //Form = 0,
                            ToMe = this.ContainKeywords( parameters[1] ),
                            Id = this.Label
                        };

                        msgs.Add( msg );

                        newMessagesArrived( msgs );
                        break;
                }
            } catch (Exception er) {
                Console.WriteLine( "OXLAMON ERROR: " + er.ToString() );
            }
        }

        const string sml_path = "http://www.goha.tv/app/tv/smileys/";
        Dictionary<string, string> smls;

        void create() {
            smls = new Dictionary<string, string>();

            smls[":)))))))"] = "pandaredlol";
            smls[":))"] = "redlol";
            smls[":("] = "cry";
            smls[":-("] = "cry";
            smls["=("] = "cry";
            smls["=P"] = "facepalm";
            smls[":-P"] = "facepalm";
            smls["=)"] = "grin";
            smls[":-)"] = "grin";
            smls[":)"] = "grin";
            smls["<3"] = "grin";
            smls[":-)*"] = "beer2";
            smls[":-*"] = "beer2";
            smls["XD"] = "hapydancsmil";
            smls[":D"] = "hapydancsmil";
            smls["=/"] = "cry";
            smls["=8)"] = "yahoo";
            smls["=("] = "cry";
            smls[":-("] = "cry";
            smls["=D"] = "yahoo";
            smls[";-)"] = "blush";
            smls[";)"] = "blush";
            smls[":-|"] = "cry";
            smls[":)))"] = "yahoo";
            smls[":|)"] = "grin";
            smls["B-)"] = "victory";
            smls["//_-)"] = "facepalm";
            smls["(&)"] = "beer2";
            smls["=)"] = "nice";
            smls["\\*/"] = "dunno";
            smls["$)"] = "super";
            smls[":P"] = "blum";
            smls["\\^/"] = "lalala";
            smls["+\\"] = "plus1";
            smls["g]"] = "bow";
            smls["&)))"] = "hapydancsmil";
            smls[":>"] = "blush";
        }

        string replaceSmiles( string text ) {
            if (smls == null)
                create();

            foreach (var kv in smls) {
                text = text.Replace( kv.Key, "[sml]" + sml_path  + kv.Value + ".gif[/sml]" );
            }


            return text;
        }

        //http://www.goha.tv/app/tv/smileys/pandaredlol.gif

        public override void Create( string streamerUri, string id ) {
            this.Uri = StreamerChannel = SetKeywords( streamerUri );
            this.Label = id;
            Reconnect();
        }

        public override void Destroy() {
            //this._controller.Stop();
            Unsub();
            _timer.Stop();
            if (IrcClient != null)
                IrcClient.Disconnect();
            IrcClient = null;
        }

        public override void UpdateViewerCount() {
        }
    }
}
