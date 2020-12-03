// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;
using TwoRatChat.Model;

namespace TwoRatChat.Main.Model {
    [DataContract]
    public class CustomUserItem : INotifyPropertyChanged {
        static Regex _cleanBrackets = new Regex( "\\[(.*?)\\].*?\\[\\/.*?\\]" );


        [DataMember]
        long _messageCount = 0;
        [DataMember]
        long _expirience = 0;
        [DataMember]
        int _level = 0;
        [DataMember]
        DateTime _lastMessage;

        public event PropertyChangedEventHandler PropertyChanged;

        [DataMember]
        public string Source { get; set; }
        [DataMember]
        public string Nickname { get; set; }
        [DataMember]
        public bool Blacklisted { get; set; }
        [DataMember]
        public string WelcomePhrase { get; set; }
        [DataMember]
        public string BlacklistPhrase { get; set; }
        [DataMember]
        public bool ReadMessages { get; set; }
        [DataMember]
        public string VoiceId { get; set; }
        [DataMember]
        public string Alias { get; set; }


        [IgnoreDataMember]
        public DateTime LastMessage {
            get {
                return _lastMessage;
            }
            set {
                if( _lastMessage != value ) {
                    _lastMessage = value;
                    raisePropertyChanged( "LastMessage" );
                }
            }
        }

        [IgnoreDataMember]
        public int Level {
            get {
                return _level;
            }
            set {
                if( _level != value ) {
                    _level = value;
                    raisePropertyChanged( "Level" );
                }
            }
        }

        [IgnoreDataMember]
        public long Expirience {
            get {
                return _expirience;
            }
            set {
                if( _expirience != value ) {
                    _expirience = value;
                    raisePropertyChanged( "Expirience" );
                }
            }
        }

        [IgnoreDataMember]
        public long MessageCount {
            get {
                return _messageCount;
            }
            set {
                if( _messageCount != value ) {
                    _messageCount = value;
                    raisePropertyChanged( "MessageCount" );
                }
            }
        }

        private void raisePropertyChanged( string v ) {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( v ) );
        }

        public CustomUserItem() : base() {
            this.Source = "";
            this.Nickname = "oxlamon";
            this.Blacklisted = false;
            this.Level = 1;
            this.Expirience = 0;
            this.MessageCount = 0;
            this.ReadMessages = false;
        }

        internal string GetReplaceText( string text ) {
            if( Blacklisted ) {
                return BlacklistPhrase;
            } else {
                return text;
            }
        }

        internal void OnMessage( ChatMessage message, bool isFirst ) {
            MessageCount++;
            LastMessage = DateTime.Now;
            if( isFirst ) {
                string format = WelcomePhrase ?? "";
                if( string.IsNullOrEmpty( format ) )
                    format = Main.Properties.Settings.Default.voice_FirstMeet;

                string talk = string.Format( format ?? "", message.Name, message.Source, message.Text );
                if( !string.IsNullOrEmpty( talk ) ) {
                    TwoRatChat.Commands.CommandFactory.Talk(
                        Main.Properties.Settings.Default.voice_voiceId, talk );
                }
            }
            if( ReadMessages ) {
                if( message.ToMe && Expirience > 1000 ) {
                    string format = "";
                    if( message.Text.Contains( "?" ) ) {
                        format = Properties.Settings.Default.voice_ask;
                    } else
                    if( message.Text.Contains( "!" ) ) {
                        format = Properties.Settings.Default.voice_caps;
                    } else
                        format = Properties.Settings.Default.voice_talk;

                    int i = message.Text.IndexOf( ' ' );
                    string text = message.Text;
                    if( i > 0 ) {
                        text = text.Substring( i ).Trim();
                    }
                    string name = message.Name;
                    if( !string.IsNullOrEmpty( Alias ) )
                        name = Alias;

                    string talk = string.Format( format ?? "", name, _cleanBrackets.Replace( text, "" ) );
                    if( !string.IsNullOrEmpty( talk ) ) {
                        TwoRatChat.Commands.CommandFactory.Talk(
                            VoiceId ?? Main.Properties.Settings.Default.voice_voiceId, talk );
                    }
                }
            }
            this.Expirience += message.Text.Length / 10;
        }
    }


    public class CustomUsers: ObservableCollection<CustomUserItem> {
        [IgnoreDataMember]
        static DataContractSerializer dcs = new DataContractSerializer(
            typeof( CustomUsers ), 
            new Type[] {
                typeof( CustomUserItem )
            } );

        [IgnoreDataMember]
        Dictionary<string, CustomUserItem> _helper = new Dictionary<string, CustomUserItem>();

        public CustomUsers() {
        }

        public void Validate() {
            _helper.Clear();
            foreach( var item in this ) {
                string key = item.Source + ":" + item.Nickname;
                _helper[key] = item;
            }
        }

        public CustomUserItem GetOrCreateUser( Dispatcher dispartcher, string source, string nickName ) {
            string key = source + ":" + nickName;
            CustomUserItem item;
            if( _helper.TryGetValue(key, out item ) ) {
                return item;
            }

            item = new Model.CustomUserItem() {
                Source = source,
                Nickname = nickName,
                Level = 1,
                Expirience = 0,
                ReadMessages = false
            };
            _helper[key] = item;
            Add( item );
            return item;
        }

        public static CustomUsers Load() {
            string fileName = System.IO.Path.Combine( Main.App.DataLocalFolder, "customUsers.xml" );
            if( File.Exists( fileName ) ) {
                try {
                    using( MemoryStream ms = new MemoryStream( File.ReadAllBytes( fileName ) ) ) {
                        return load( ms );
                    }
                } catch( Exception er ) {
                    App.Log( '!', $"Error while loading customusers file. {er}" );
                }
            }
            return new Model.CustomUsers();
        }

        public void Save() {
            string fileName = System.IO.Path.Combine( Main.App.DataLocalFolder, "customUsers.xml" );

            using( MemoryStream ms = new MemoryStream() ) {
                save( ms );
                File.WriteAllBytes( fileName, ms.ToArray() );
            }
        }

        public void save( Stream s ) {
            XmlDictionaryWriter xdw = XmlDictionaryWriter.CreateTextWriter( s, Encoding.UTF8 );
            xdw.WriteStartDocument();
            dcs.WriteObject( xdw, this );
            xdw.Close();
        }

        public static CustomUsers load( Stream s ) {
            XmlDictionaryReader xdw = XmlDictionaryReader.CreateTextReader( s, new XmlDictionaryReaderQuotas() );
            var x =  dcs.ReadObject( xdw ) as CustomUsers;
            x.Validate();
            return x;
        }
    }
}
