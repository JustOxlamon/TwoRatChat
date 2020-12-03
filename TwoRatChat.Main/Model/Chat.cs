// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TwoRatChat.Main.Bot;
using TwoRatChat.Main.Commands;
using TwoRatChat.Main.Helpers;
using TwoRatChat.Main.Model;

namespace TwoRatChat.Model {
    internal class Chat {
    //    string _customUsersPath = 

        private Dispatcher _dispatcher;
        private ChatArgs _args;
        private Object _locker = new object();
        private Object _cmdLocker = new object();
        ObservableCollection<ChatSource> _sources;
        ObservableCollection<BotSender> _bots;
        ObservableCollection<ChatMessage> _systemMessages;
        ObservableCollection<ChatMessage> _firstMessages;
        List<Command> _commands = new List<Command>();
       // LevelManager _LevelManager;

        public Dispatcher Dispatcher { get { return _dispatcher; } }

        public ObservableCollection<BotSender> Bots { get { return _bots; } }
        public ObservableCollection<ChatSource> Sources { get { return _sources; } }
        public ObservableCollection<ChatMessage> SystemMessages { get { return _systemMessages; } }
        public ObservableCollection<ChatMessage> FirstMessages { get { return _firstMessages; } }
        //public Dictionary<string, PersonalData> PersonalData { get; private set; }
        List<ChatMessage> _messages;
        HashSet<Guid> _messageGIDs;
        HashSet<ChatActuator> _actuators = new HashSet<ChatActuator>();

        public CustomUsers CustomUsers { get; set; }

        public BlackList ImageBlackList { get; set; }
        public BlackList ImageWhiteList { get; set; }

        public BlackList BlackList { get; set; }
        public FortuneList FortuneList { get; set; }
        public Polling Polling { get; set; }

        //public class Options {
        //    public int imageMode;
        //    public int maxImageWidth;
        //    public int maxImageHeight;
        //}

        //public string GetOptions() {
        //    var options = new Options() {
        //        imageMode = TwoRatChat.Main.Properties.Settings.Default.showImageMode,
        //        maxImageWidth = TwoRatChat.Main.Properties.Settings.Default.maxImageWidth,
        //        maxImageHeight = TwoRatChat.Main.Properties.Settings.Default.maxImageHeight
        //    };
            
        //    return Newtonsoft.Json.JsonConvert.SerializeObject( options );
        //}

        public Chat( Dispatcher dispatcher, ChatArgs args ){
            this._dispatcher = dispatcher;
            this._args = args;
            this._sources = new ObservableCollection<ChatSource>();
            this._systemMessages = new ObservableCollection<ChatMessage>();
            this._bots = new ObservableCollection<BotSender>();
            this._messages = new List<ChatMessage>();
            this._messageGIDs = new HashSet<Guid>();
            this._firstMessages = new ObservableCollection<ChatMessage>();

            this.BlackList = new BlackList();
            this.FortuneList = new FortuneList();
            this.Polling = new Polling();

            this.ImageBlackList = new BlackList();
            this.ImageWhiteList = new BlackList();

        //    this.PersonalData = new Dictionary<string, Main.Model.PersonalData>();

           // this._LevelManager = new LevelManager();

            this.CustomUsers = CustomUsers.Load();
        }

        public void RemoveSystemMessage(ChatMessage chatMessage) {
            _systemMessages.Remove( chatMessage );
        }

        public void Load() {

            string[] code = Main.Properties.Settings.Default.ChatSave.Split( new string[] { ";" }, 
                StringSplitOptions.RemoveEmptyEntries );

            foreach (var x in code) {
                string[] pp = x.Split( ',' );
                int id = int.Parse( pp[0] );


                foreach( var s in TwoRatChat.Main.Sources.SourceManager.Sources)
                    if (s.Id == id) {
                        AddSource( s, pp[1], pp[2] );
                        break;
                    }
            }

            //code = Main.Properties.Settings.Default.voice_Personals.Split( new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries );
            //foreach( var x in code ) {
            //    string[] pp = x.Trim( ' ', '\r', '\n', '\t' ).Split( ',' );

            //    PersonalData pd = new Main.Model.PersonalData();
            //    pd.VoiceHello = pp[1];
            //    if( pp.Length > 2 )
            //        pd.CustomIcon = pp[2];

            //    PersonalData[pp[0]] = pd;
            //}

            //PersonalData["TwoRatChat"] = new Main.Model.PersonalData() { VoiceHello = "", CustomIcon = "" };

            BlackList.Load( Main.Properties.Settings.Default.Chat_Blacklist );
            ImageBlackList.Load( Main.Properties.Settings.Default.imageBlackList );
            ImageWhiteList.Load( Main.Properties.Settings.Default.imageWhiteList );
          //  this._LevelManager.Load();
        }



        public void Save() {
          //  this._LevelManager.Save();
            StringBuilder sb = new StringBuilder();

            foreach ( var s in _sources )
                sb.AppendFormat( "{0},{1},{2};", s.SystemId, s.Uri, s.Label );

            Main.Properties.Settings.Default.ChatSave = sb.ToString();
            Main.Properties.Settings.Default.Chat_Blacklist = BlackList.Save();

            Main.Properties.Settings.Default.imageBlackList = ImageBlackList.Save();
            Main.Properties.Settings.Default.imageWhiteList = ImageWhiteList.Save();

            this.CustomUsers.Save();
            //sb.Clear();

            //foreach ( var pd in PersonalData )
            //    sb.AppendFormat( "{0},{1},{2};", pd.Key, pd.Value.VoiceHello, pd.Value.CustomIcon );

            //Main.Properties.Settings.Default.voice_Personals = sb.ToString();
        }

        public void AddSource( TwoRatChat.Main.Sources.Source source, string param1, string param2 ) {
            if( source == null )
                return;

            ThreadPool.QueueUserWorkItem( new WaitCallback( b => {
                Thread.CurrentThread.Name = "create thread";
                ChatSource cs = Activator.CreateInstance( source.Type, this._dispatcher ) as ChatSource;
                cs.Status = false;
                cs.OnFatalError += Cs_OnFatalError;
                cs.OnNewMessages += cs_OnNewMessages;
                cs.OnRemoveUserMessages += Sender_OnRemoveUserMessages;
                cs.OnNewFollower += Sender_OnNewFollower;

                _dispatcher.Invoke( () => { _sources.Add( cs ); } );

                cs.SystemId = source.Id;
                cs.Create( param1, param2 );

            } ), null );
        }

        public void AddActuator(ChatActuator chatActuator) {
            _actuators.Add( chatActuator );

        }

        public void RemoveActuator(ChatActuator chatActuator) {
            _actuators.Remove( chatActuator );
        }

        private void Sender_OnNewFollower(ChatSource sender, string userName) {
            if ( Main.Properties.Settings.Default.newFollowerShowInChat ) {
                cs_OnNewMessages( new ChatMessage[] { new ChatMessage() {
                    Text = string.Format( Main.Properties.Resources.NewFollowerMessage, userName ),
                    Date = DateTime.Now,
                    Source = sender,
                    ToMe=true,
                    Name = "TwoRatChat"
                } } );
            }


            if( sender != null ) {
                if( !string.IsNullOrEmpty( Main.Properties.Settings.Default.voice_Followers ) ) {
                    TwoRatChat.Commands.CommandFactory.Talk(
                        string.Format( Main.Properties.Settings.Default.voice_Followers, userName, sender.Id ) );
                }

                lock( _cmdLocker ) {
                    _commands.Add( new Command( "shownotify",
                        string.Format( "{0}/{1}<br/>новый подписчик!",
                        sender.Id, userName ) ) );
                }
            }
        }

        public void AddMessage(ChatMessage message) {
            cs_OnNewMessages( new ChatMessage[] { message } );
        }

        public void AddCommand( string cmd, params string[] prms) {
            lock ( _cmdLocker ) {
                _commands.Add( new Command( cmd, prms ) );
            }
        }

        private void Cs_OnFatalError( ChatSource sender, ChatSource.FatalErrorCodeEnum code ) {
            Action a = new Action( () => {
                _systemMessages.Add( new ChatMessage() {
                    Text = Main.Helpers.Errors.GetLocalizedError( (int)code ),
                    Date = DateTime.Now,
                    Source = sender
                } );
            } );

            if( _dispatcher.CheckAccess() ) {
                a();
            } else {
                _dispatcher.Invoke( a );
            }
            // Add message
            removeChatSource( sender );
        }

        internal void Stop() {
            foreach ( var s in _sources )
                s.Destroy();
        }

        private void removeChatSource(ChatSource sender) {
            lock ( _locker ) {
                List<ChatMessage> delMe = new List<ChatMessage>();
                foreach ( var m in _messages )
                    if ( m.Source == sender )
                        delMe.Add( m );
                foreach ( var m in delMe ) {
                    _messageGIDs.Remove( m.GID );
                    _messages.Remove( m );
                }
            }


            sender.Status = false;
            sender.OnNewMessages -= cs_OnNewMessages;
            sender.OnFatalError -= Cs_OnFatalError;
            sender.OnRemoveUserMessages -= Sender_OnRemoveUserMessages;
            sender.OnNewFollower -= Sender_OnNewFollower;

            if ( _dispatcher.CheckAccess() ) {
                _sources.Remove( sender );
                sender.Destroy();
            } else {
                _dispatcher.Invoke( () => {
                    _sources.Remove( sender );
                    sender.Destroy();
                } );
            }
        }

        internal void RemoveCommand(Guid guid) {
            lock ( _cmdLocker ) {
                foreach ( var c in _commands )
                    if ( c.GID == guid ) {
                        _commands.Remove( c );
                        break;
                    }
            }
        }

        private void Sender_OnRemoveUserMessages(ChatSource sender, string userName) {
            Action a = new Action( () => {
                foreach ( var m in _messages.ToArray() )
                    if ( string.Compare( m.Name, userName, true ) == 0 )
                        m.Text = Main.Properties.Resources.MES_Deleted;
            } );

            if ( _dispatcher.CheckAccess() )
                a();
            else
                _dispatcher.Invoke( a );

            //Console.WriteLine( "CLEAR!" );
        }

        bool checkOnFirst(ChatMessage msg) {
            foreach ( var m in _firstMessages )
                if ( m.Name == msg.Name && m.Source == msg.Source )
                    return false;
            return true;
        }

        void cs_OnNewMessages( IEnumerable<ChatMessage> messages ) {


            foreach( var m in messages ) {
                if( !_messageGIDs.Contains( m.GID ) ) {
                    var user = this._dispatcher.Invoke( () => this.CustomUsers.GetOrCreateUser( this._dispatcher, m.Source.Id, m.Name ) );

                    m.Text = user.GetReplaceText( m.Text );

                    if( !string.IsNullOrEmpty( m.Text ) ) {

                        //if ( TwoRatChat.Main.Properties.Settings.Default.allowLeveling ) {
                        //    if ( _LevelManager.OnMessage( m ) ) {
                        //        // add message on level
                        //        lock ( _locker ) {
                        //            _messages.Add( new ChatMessage() {
                        //                ToMe = true,
                        //                Date = DateTime.Now,
                        //                Source = null,
                        //                Name = "TwoRatChat",
                        //                Text = string.Format( TwoRatChat.Main.Properties.Settings.Default.promouteMessage, m.Name, _LevelManager.GetLevelName(m.Name) )
                        //            } );
                        //        }

                        //        if ( m.Source != null )
                        //            if ( !string.IsNullOrEmpty( Main.Properties.Settings.Default.voice_Promoute ) ) {
                        //                TwoRatChat.Commands.CommandFactory.Talk(
                        //                    Main.Properties.Settings.Default.voice_voiceId,
                        //                    string.Format( Main.Properties.Settings.Default.voice_Promoute, m.Name, _LevelManager.GetLevelName( m.Name ), m.Source.Id, m.Text ) );
                        //            }
                        //    }
                        //}

                        lock( _locker ) {
                            _messages.Add( m );
                            _messageGIDs.Add( m.GID );

                            foreach( var act in _actuators )
                                act.OnMessage( m );

                            if( checkOnFirst( m ) ) {
                                _firstMessages.Add( m );
                                user.OnMessage( m, true );
                                //if ( !string.IsNullOrEmpty( Main.Properties.Settings.Default.soundOnMessage ) ) {
                                //    string[] d = Main.Properties.Settings.Default.soundOnMessage.Split( '.' );
                                //    TwoRatChat.Commands.CommandFactory.Fire( d[0], d[1] );
                                //}
                                //if ( m.Source != null ) {
                                //    PersonalData pd;
                                //    if ( PersonalData.TryGetValue( m.Name, out pd ) ) {
                                //        if ( !string.IsNullOrEmpty( pd.VoiceHello ) ) {
                                //            TwoRatChat.Commands.CommandFactory.Talk(
                                //                Main.Properties.Settings.Default.voice_voiceId,
                                //                string.Format( pd.VoiceHello, m.Name, m.Source.Id, m.Text ) );
                                //        }
                                //    } else {
                                //        if ( !string.IsNullOrEmpty( Main.Properties.Settings.Default.voice_FirstMeet ) ) {
                                //            TwoRatChat.Commands.CommandFactory.Talk(
                                //                Main.Properties.Settings.Default.voice_voiceId,
                                //                string.Format( Main.Properties.Settings.Default.voice_FirstMeet, m.Name, m.Source.Id, m.Text ) );
                                //        }
                                //    }
                                //}
                            } else {
                                user.OnMessage( m, false );
                            }
                        }

                        onMessage( m );
                    } else {
                        lock( _locker ) {
                            _messageGIDs.Add( m.GID );
                        }
                    }
                }
            }

            lock( _locker )
                if( _messages.Count > 200 ) {
                    for( int j = 0; j < 50; ++j )
                        _messageGIDs.Remove( _messages[j].GID );
                    _messages.RemoveRange( 0, 50 );
                }

            OnMessageUpdated?.Invoke();
        }

        Regex r = new Regex( "\\[url\\](.*?)\\[/url\\]" );

        private void onMessage(ChatMessage m) {


            if ( Main.Properties.Settings.Default.showImageMode > 0 ) {
                bool allow = false;
                switch ( Main.Properties.Settings.Default.showImageMode ) {
                    case 5:
                        allow = true;
                        break;
                    case 4:
                        if ( !ImageBlackList.IsInList( m.Source, m.Name ) )
                            allow = true;
                        break;
                    case 3:
                        if ( ImageWhiteList.IsInList( m.Source, m.Name ) )
                            allow = true;
                        break;
                }

                if ( allow )
                    m.Text = r.Replace( m.Text, ( match ) => {
                        return string.Format( "[img{1}]{0}[/img]", match.Groups[1].Value,
                            string.Format( "max-width:{0};max-height:{1};",
                            Main.Properties.Settings.Default.maxImageWidth,
                            Main.Properties.Settings.Default.maxImageHeight ) );
                    } );
            }

            if ( Polling != null && Polling.Started == PollStatusEnum.Started )
                if ( _dispatcher.CheckAccess() )
                    Polling.OnMessage( m );
                else
                    _dispatcher.Invoke( () => {
                        Polling.OnMessage( m );
                    } );


            if ( FortuneList != null && FortuneList.Status == FortuneStatusEnum.Started )
                if ( _dispatcher.CheckAccess() )
                    FortuneList.OnMessage( m );
                else
                    _dispatcher.Invoke( () => {
                        FortuneList.OnMessage( m );
                    } );


            foreach ( var bot in _bots )
                bot.OnMessage( m );
        }

        public string GetMessages( bool allowRatSource, int maxCount = 30 ) {
            StringBuilder sb = new StringBuilder("{ \"messages\": [");
            bool bAddComma = false;
            lock (_locker) {
                int x = 0;
                int l = _messages.Count - maxCount;
                if (l < 0) l = 0;

                foreach (var m in _messages) {
                    if ((x++) >= l) {
                        if (bAddComma)
                            sb.Append( "," );

                        sb.Append( m.ToJson( allowRatSource ) );

                        bAddComma = true;
                    }
                }
            }
            sb.Append( "], \"cmds\":[" );
            lock( _cmdLocker ) {
                bAddComma = false;
                foreach ( var cmd in _commands ) {
                    if ( bAddComma )
                        sb.Append( "," );

                    sb.Append( cmd.ToJson( allowRatSource ) );
                    bAddComma = true;
                }
            }
            sb.Append( "]}" );
            return sb.ToString();
        }

        public delegate void OnMessageDelegate( ChatMessage message );
        public event Action OnMessageUpdated;

        public bool FakeMode { get; set; }
        Random _rnd = new Random();

        internal void OnTimer() {
            foreach ( var s in _sources.ToArray() )
                s.UpdateViewerCount();

            foreach ( var bot in _bots )
                bot.OnTimer();

            if ( Sources.Count > 0 ) {
                if ( FakeMode && _rnd.Next( 100 ) > 40 ) {

                    BashQuote bq = new BashQuote();
                    bq.GetRandomQuote( ( a ) => {

                        List<ChatMessage> msgs = new List<ChatMessage>();

                        var s = Sources[_rnd.Next( Sources.Count )];
                        if ( _rnd.Next( 10 ) == 0 )
                            s = null;

                        msgs.Add( new ChatMessage() {
                            Date = DateTime.Now,
                            Level = 0,
                            Name = _rnd.Next( 5 ) == 2 ? "oxlamon" : "twoRatChat",
                            Text = a,
                            Source = s,
                            ToMe = _rnd.Next( 3 ) == 1,
                            IsFake = true
                        } );

                        cs_OnNewMessages( msgs );

                    } );

                
                }
            }

            Sort();
        }

        public void Send(string text, string nick = null, string source = null) {
            foreach ( var bot in _bots )
                if( string.IsNullOrEmpty( source ) || bot.Name == source )
                    bot.Send( text, nick );
        }

        void Sort() {
            int i, j;
            ChatSource temp;

            for ( i = 1; i < Sources.Count; i++ ) {
                temp = Sources[i];     //If you can't read it, it should be index = this[x], where x is i :-)
                j = i;
                while ( (j > 0) && (onSort( j, temp )) ) {
                    Sources[j] = Sources[j - 1];
                    j = j - 1;
                }
                Sources[j] = temp;
            }
        }

        private bool onSort(int j, ChatSource temp) {
            if( Sources[j - 1].ViewersCount.HasValue ) {
                return Sources[j - 1].ViewersCount < temp.ViewersCount;
            } else {
                int x = string.Compare( Sources[j - 1].Id, temp.Id );

                return x < 0;
            }            
        }

        internal void ReloadChat( ChatSource cs ) {
            cs.Status = false;
            cs.ReloadChatCommand();
        }

        internal void CloseChat( ChatSource cs ) {

            removeChatSource( cs );
        }
       
        public byte[] GetEncodedMessages() {
            string oldText = "";
            string text = GetMessages( true );

            try {
                var x = Newtonsoft.Json.JsonConvert.DeserializeObject( text );
                oldText = text;
                text = Newtonsoft.Json.JsonConvert.SerializeObject( x );
            } catch( Exception err ) {
                Main.App.Log( '?', "Ошибка сохранения в JSON: {0}\n{1}", err, oldText );
            }

            byte[] data = Encoding.UTF8.GetBytes( text );
            return data;
        }

        
    }
}


/*
 { 
    "messages": [{ "label": "", "name": "Oxlamon", "text": "123", "tome": "False", "date": "01.01.0001 0:00:00", "source": "streamcube", "gid": "ef4c0646-ebc6-4243-9986-8435498abcac", "badges": [], "color": "" }], "cmds":[{"gid":"5f5e441c-872c-43b8-99d9-683b3bb254af","type":"shownotify","prms":["Всем привет, это крысочат!"]}]}
     
     { "messages": [{ "label": "", "name": "Oxlamon", "text": "123
", "tome": "False", "date": "01.01.0001 0:00:00", "source": "streamcube", "gid": "ef4c0646-ebc6-4243-9986-8435498abcac", "badges": [], "color": "" }], "cmds":[{"gid":"5f5e441c-872c-43b8-99d9-683b3bb254af","type":"shownotify","prms":["Всем привет, это крысочат!"]}]}
     */
