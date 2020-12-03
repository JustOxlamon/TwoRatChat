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
using System.Xml;
using System.Xml.Linq;
using TwoRatChat.Model;

namespace TwoRatChat.Main.Sources {
    public class Midlane_Source: TwoRatChat.Model.ChatSource {
        jabber.client.JabberClient _client;
        jabber.connection.ConferenceManager _conference;
        jabber.client.PresenceManager _presence;
        int _chatId = 0;
        HashSet<string> _prevents = new HashSet<string>();

        public override string Id { get { return "midlane"; } }

        public Midlane_Source( Dispatcher dispatcher )
            : base(dispatcher) {
        }

        public override void UpdateViewerCount() {
          //  this.ViewersCount = null;
        }

        public override void Create( string streamerUri, string id ) {
            try {
                this.Status = false;
                this.Label = id;
                this.Uri = SetKeywords( streamerUri );

                if (int.TryParse( this.Uri, out this._chatId )) {

                } else {
                    WebClient wc = new WebClient();
                    string s = wc.DownloadString( "http://midlane.ru/channel/" + this.Uri );
                    Regex rr = new Regex( "'(.*?)_room@conference.midlane.ru" );
                    this._chatId = int.Parse( rr.Match( s ).Groups[1].Value );
                }

                _client = new jabber.client.JabberClient();
                _client.OnMessage += _client_OnMessage;
                _client.OnAuthError += _client_OnAuthError;
                _client.OnAuthenticate += _client_OnAuthenticate;
                _client.OnPresence += _client_OnPresence;
                _client.OnReadText += _client_OnReadText;
                _client.OnWriteText += _client_OnWriteText;

                _client.AutoLogin = true;
                _client.AutoPresence = true;
                _client.AutoReconnect = 1.0f;
                _client.AutoRoster = true;
                _client.AutoStartTLS = false;
                _client.AutoStartCompression = false;

                _client.KeepAlive = 30.0f;

                _conference = new jabber.connection.ConferenceManager();
                _conference.Stream = _client;

                _presence = new jabber.client.PresenceManager();
                _presence.Stream = _client;

                string[] x = Properties.Settings.Default.url_midlane.Split( ':' );

                _client.Server = x[0];
                _client.Port = 5222;// int.Parse( x[1] );
                _client.User = "mdl_client";
                _client.Password = "mdl_client";

                _client.Connect();

                Tooltip = "midlane: " + this.Uri;
            } catch ( Exception er ) {
                App.Log( '!', "Midlane create error: {0}", er );
                fireOnFatalError( FatalErrorCodeEnum.ParsingError );
                return;
            }
        }

        void _client_OnWriteText( object sender, string txt ) {
          //  Console.WriteLine( "WRT: {0}", txt );
        }

        void _client_OnReadText( object sender, string txt ) {
           // Console.WriteLine( "RDT: {0}", txt );
        }

        void _client_OnPresence( object sender, jabber.protocol.client.Presence pres ) {
            //_client.pre
            int n = 0;
            foreach (var p in _presence)
                n++;
            this.ViewersCount = n;
        }

        void _client_OnAuthenticate( object sender ) {
            _conference.GetRoom( new jabber.JID( string.Format( "{1}_room@conference.midlane.ru/tworatchat{0:x}",
                DateTime.Now.ToBinary(), _chatId ) ) )
                .Join();

            Console.WriteLine( "join: {0}", this._chatId );
        }

        void _client_OnAuthError( object sender, System.Xml.XmlElement rp ) {
            string s = rp.OuterXml;
        }

        void _client_OnMessage( object sender, jabber.protocol.client.Message msg ) {
            string key = msg.InnerXml;
            if (_prevents.Contains( key ))
                return;
            _prevents.Add( key );
            this.Status = true;
            /*<body xmlns="jabber:client">&lt;img src="/files/b147e27b87c5028726a4cdee13e682de.jpg"
             * id="smile_33" class="smile_cl"&gt;</body>123<delay xmlns="urn:xmpp:delay" 
             * from="41_user@midlane.ru/33232fea" stamp="2015-04-13T10:40:04.789Z" />
             * <x xmlns="jabber:x:delay" from="41_user@midlane.ru/33232fea" 
             * stamp="20150413T10:40:04" />*/


            // Console.WriteLine( msg.InnerXml );

            newMessagesArrived( new ChatMessage[] { 
                new ChatMessage() {
                                       Date = DateTime.Now,
                                       Name = msg.From.Resource,
                                       Text = SetupSmiles( msg.Body ),
                                       Source = this,
                                       Id = this.Label,
                                       ToMe = this.ContainKeywords( msg.Body ), //??
                                       //Form = 0
                                   }
            } );
        }

        Regex sm = new Regex( "<img.*?src=\"(.*?)\".*?>" );
        Regex nk = new Regex( ".*?nick\">(.*?)</span>(.*?)</span>" );

        private string SetupSmiles( string p ) {
            p = nk.Replace( p, ( b ) => {
                return string.Format( "[b]{0}[/b] {1}", b.Groups[1].Value, b.Groups[2].Value );
            } );

            return sm.Replace( p, ( a ) => {
                return "[sml]http://midlane.ru/" + a.Groups[1].Value + "[/sml]";
            } );
        }

        public override void Destroy() {
            _client.Close();
            _client.Dispose();
            _client = null;
        }
    }
}
