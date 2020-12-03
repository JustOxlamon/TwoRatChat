using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using TwoRatChat.Model;

namespace TwoRatChat.Main.Sources {
    public class StreamCube_ChatSource : TwoRatChat.Model.ChatSource {
        System.Timers.Timer _retriveTimer;
        int _channelId = 0;
        bool allowAddMessages = false;

        Dictionary<string, string> smiles = new Dictionary<string, string>();
        Dictionary<string, DateTime> users = new Dictionary<string, DateTime>();

        private class MyWebClient : WebClient {
            protected override WebRequest GetWebRequest( Uri uri ) {
                WebRequest w = base.GetWebRequest( uri );
                w.Timeout = 1000;
                return w;
            }
        }



        void loadChat( int ChannelId ) {
            _retriveTimer.Stop();

            MyWebClient wc = new MyWebClient();
            wc.Headers.Add( "user-agent", "TwoRatChat" );
            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler( ( a, b ) => {
                if ( b.Error == null ) {
                    try {
                        string[][] messages = JsonConvert.DeserializeObject<string[][]>( b.Result );
                        if ( messages != null )
                            updateMessages( messages );
                        this.Status = true;
                    } catch ( Exception err ) {
                        this.Status = false;
                        Console.WriteLine( err.ToString() );
                    }
                } else {
                    // Кстати, тут можно нарисовать ошибку сети.
                }

                if ( _retriveTimer != null )
                    _retriveTimer.Start();
            } );
            wc.DownloadStringAsync( new Uri( string.Format( Main.Properties.Settings.Default.url_StreamCube, "chat/api/" + ChannelId ) ) );
        }

        void loadSmiles() {
            MyWebClient wc = new MyWebClient();
            wc.Headers.Add( "user-agent", "TwoRatChat" );
            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler( ( a, b ) => {
                if ( b.Error == null ) {
                    try {
                        string[][] smiles = JsonConvert.DeserializeObject<string[][]>( b.Result );
                        if ( smiles != null ) {
                            this.smiles = new Dictionary<string, string>();
                            for( int j=0; j<smiles.Length;++j ) {
                                this.smiles[smiles[j][0]] = string.Format( Main.Properties.Settings.Default.url_StreamCube, smiles[j][1] );
                            }
                        }
                    } catch ( Exception err ) {
                        Console.WriteLine( err.ToString() );
                    }
                } else {
                    // Кстати, тут можно нарисовать ошибку сети.
                }

            } );
            wc.DownloadStringAsync( new Uri( string.Format( Main.Properties.Settings.Default.url_StreamCube, "chat/api/smile" ) ) );
        }


        HashSet<int> usedIds = new HashSet<int>();

        private void updateMessages( string[][] messages ) {
            var cm = new List<ChatMessage>();
            for( int j=0; j<messages.Length; ++j ) {
                int id = int.Parse( messages[j][1] );
                if ( usedIds.Contains( id ) )
                    continue;
                usedIds.Add( id );

                if ( allowAddMessages ) {
                    cm.Add( new ChatMessage() {
                        Source = this,
                        Id = Label,
                        ToMe = messages[j][0] != "0",
                        Name = messages[j][3],
                        Text = prepareSmiles( messages[j][5] )
                    } );
                    users[messages[j][3]] = DateTime.Now;
                }
            }

            if ( cm.Count > 0 )
                newMessagesArrived( cm );

            allowAddMessages = true;
            DateTime n = DateTime.Now;
            int nn = 0;
            foreach ( var x in users )
                if ( (n - x.Value).TotalMinutes < 5 )
                    nn++;
            this.Header = nn.ToString();
            this.ViewersCount = nn;
        }

        Regex url = new Regex( "\\[url=(.*?)\\](.*?)\\[\\/url\\]" );



        private string prepareSmiles( string text ) {
            foreach ( var smile in smiles )
                text = text.Replace( smile.Key, "[sml]" + smile.Value + "[/sml]" );

            text = url.Replace( text, new MatchEvaluator( a => {
                return "[url]" + a.Groups[1].Value + "[/url]";
            } ) );

            return text;
        }

        //http://streamcube.ru/ 
        public override string Id {
            get { return "streamcube"; }
        }

        public StreamCube_ChatSource( Dispatcher dispatcher )
            : base(dispatcher) {
            loadSmiles();
            _retriveTimer = new System.Timers.Timer( 3000 );
            _retriveTimer.Elapsed += _retriveTimer_Elapsed;
        }

        private void _retriveTimer_Elapsed( object sender, System.Timers.ElapsedEventArgs e ) {
            if ( _channelId != 0 )
                loadChat( _channelId );
        }

        public override void Create( string streamerUri, string id ) {
            
            this.Uri = streamerUri = SetKeywords( streamerUri );
            this.Label = id;
            allowAddMessages = Main.Properties.Settings.Default.Chat_LoadHistory;
            this.Tooltip = "streamCube: " + streamerUri;
            try {
                _channelId = int.Parse( streamerUri );
            } catch {
                try {
                    MyWebClient wc = new MyWebClient();
                    string[] data = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>( wc.DownloadString( string.Format( Main.Properties.Settings.Default.url_StreamCube, "chat/api/?url=" + streamerUri ) ) );
                    _channelId = int.Parse( data[0] );
                    this.Tooltip = "streamCube: " + data[1];
                } catch {

                }
            }
            _retriveTimer.Start();
        }

        public override void Destroy() {
        }

        public override void UpdateViewerCount() {
        }
    }
}
