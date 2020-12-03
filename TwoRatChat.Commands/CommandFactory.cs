using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
//using License;
using System.Timers;

namespace TwoRatChat.Commands {
    public static class CommandFactory {
        static double _GlobalVolume = 1.0;
        static Interfaces.IVoiceEngine _engine;
        static Dictionary<string, Type> _reactionTypes;
        static Dictionary<string, Type> _activatorsTypes;
        static object _profileLocker = new object();
        static List<Profile> _profiles = new List<Profile>();
        static Profile _profile = null;
        static Timer _timer = new Timer();

        public static Dictionary<string, string> Config = new Dictionary<string, string>();

        public static string GetValue( string key, string def ) {
            string x;
            if ( Config.TryGetValue( key, out x ) )
                return x;
            return def;
        }

        public static List<string> GetVoices {
            get {
                if( _engine == null )
                    return new List<string>();
                return _engine.Voices;
            }
        }

        public static IEnumerable<string> Ids {
            get {
                yield return string.Empty;
                foreach ( var p in _profiles )
                    foreach ( var id in p.Ids )
                        yield return p.ProfileName + "." + id;
            }
        }
        static object _ProfileLock = new object();

        internal static Profile Profile {
            get { return _profile; }
            set {
                lock( _ProfileLock ) {
                    if ( _profile != value || _profile == null ) {
                        if ( _profile != null ) {
                            Console.WriteLine( "Profile: {0} deactivated.", _profile.ProfileName );
                            _profile.Deactivate();
                        }
                        _profile = value;
                        if ( _profile != null ) {
                            Console.WriteLine( "Profile: {0} activated.", _profile.ProfileName );
                            _profile.Activate();
                        }

                        lock ( _profileLocker ) {
                            foreach ( var p in _profiles )
                                if ( p.WindowTitle == null ) {

                                    if ( p.Activate() ) {
                                        Console.WriteLine( "Additional profile: {0} activated.", p.ProfileName );
                                    }
                                }
                        }
                    }
                }
            }
        }

        public static string User { get; private set; }

        public static double GlobalVolume {
            get { return _GlobalVolume; }
            set {
                _GlobalVolume = value;
                if ( _GlobalVolume < 0.0 )
                    _GlobalVolume = 0.0;
                if ( _GlobalVolume > 1.0 )
                    _GlobalVolume = 1.0;

                RadioReaction.UpdateVolume();
            }
        }

        public static event ResolveFilePathDelegate OnResolveFilePath;

        //class Test {
        //    public Dictionary<string, string> ReadAdditonalLicenseInformation() {
        //        /* Check first if a valid license file is found */
        //        Dictionary<string, string> data = new Dictionary<string, string>();

        //        data["user"] = "Free";
        //        data["voice"] = "disallow";
        //        data["macro"] = "disallow";

        //        for ( int i = 0; i < License.Status.KeyValueList.Count; i++ ) {
        //            string key = License.Status.KeyValueList.GetKey( i ).ToString();
        //            string value = License.Status.KeyValueList.GetByIndex( i ).ToString();

        //            data[key] = value;
        //        }
        //        return data;
        //    }
        //}

        public static void BeginInitialize(string locale) {
            _reactionTypes = new Dictionary<string, Type>();
            _activatorsTypes = new Dictionary<string, Type>();

            User = "Free";

            try {
                UriBuilder uri = new UriBuilder( Assembly.GetExecutingAssembly().CodeBase );
                string path = Path.GetDirectoryName( Uri.UnescapeDataString( uri.Path ) );
                path = Path.Combine( path, "TwoRatChat.Voice.dll" );

                if( File.Exists( path ) ) {
                    Assembly a = Assembly.Load( File.ReadAllBytes( path ) );

                    foreach( Type e in a.GetExportedTypes() ) {
                        var ive = e.GetInterface( "TwoRatChat.Interfaces.IVoiceEngine" );
                        if( ive != null ) {
                            _engine = Activator.CreateInstance( e ) as TwoRatChat.Interfaces.IVoiceEngine;
                            _engine.BeginInitialize( locale );
                            _engine.OnRecognize += _engine_OnRecognize;
                            break;
                        }
                    }
                } else {
                    _engine = null;
                }
            } catch( Exception er ) {
                //_engine = null;
            }


            RegisterActivator( "null", typeof( NullActuator ) );
            RegisterActivator( "hotkey", typeof( Hotkey ) );
            RegisterActivator( "track", typeof( TrackActuator ) );
            //if ( _engine != null )
            //    RegisterActivator( "voice", typeof( VoiceActuator ) );


            //if ( lic["macro"] == "allow" ) {
                RegisterReaction( "macro", typeof( MacroReaction ) );
            //}


            if ( _engine != null )
                RegisterReaction( "speech", typeof( SpeechReaction ) );

            RegisterReaction( "playsound", typeof( PlaySound ) );
            RegisterReaction( "player", typeof( MusicPlayerReaction ) );
            RegisterReaction( "radio", typeof( RadioReaction ) );

            //mafaka.ReadAdditonalLicenseInformation();
        }

        private static void _engine_OnRecognize(object obj) {
            ((VoiceActuator)obj).OnVoiceCommand();
        }

        internal static string ResolveFilePath( string fileName ) {
            if ( OnResolveFilePath != null )
                return OnResolveFilePath( fileName );
            return null;
        }

        internal static void registerVoiceActuator(VoiceActuator voiceActuator, CultureInfo locale, string start, string[] phrases) {
            if ( _engine != null )
                _engine.Register( voiceActuator, locale, start, phrases );
        }

        internal static void unregisterVoiceActuator(VoiceActuator voiceActuator) {
            if ( _engine != null )
                _engine.Unregister( voiceActuator );
        }

        public static void EndInitialize() {
            if ( _engine != null ) 
                _engine.EndInitialize();

            _timer.Interval = 500.0;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        static string _oldTitle = "";

        private static void _timer_Elapsed(object sender, ElapsedEventArgs e) {
            string title = WinAPI.GetForegroundWindowText();
            Profile _p = null;
            lock ( _profileLocker )
                foreach ( var p in _profiles )
                    if ( p.WindowTitle != null )
                        if ( p.WindowTitle.IsMatch( title ) )
                            _p = p;
            Profile = _p;

            if ( _oldTitle != title ) {
                _oldTitle = title;
                Console.WriteLine( _oldTitle );
            }
        }

        public static void Talk(string voice, string text) {
            try {
                if ( _engine != null )
                    _engine.Talk( voice, text );
            }catch {

            }
        }

        public static void Talk( string text ) {
            try {
                Talk( "", text );
            } catch {

            }
        }

        public static void RegisterReaction( string id, Type type) {
            _reactionTypes.Add( id, type );
        }

        public static void RegisterActivator(string id, Type type) {
            _activatorsTypes.Add( id, type );
        }

        static Reaction Create( XElement x ) {
            Type r;
            if( _reactionTypes.TryGetValue( x.Name.LocalName, out r ) )
                return Activator.CreateInstance( r, x ) as Reaction;
            return null;
        }

        internal static Reaction CreateGroup(XElement x) {
            ReactionGroup rg = new ReactionGroup();
            if ( x.Attribute( "id" ) != null )
                rg.Id = x.Attribute( "id" ).Value;
            foreach ( var c in x.Elements() ) {
                rg.Add( Create( c ) );
            }
            return rg.GetMeOrFirst();
        }

        public static void Destroy() {
            if ( RadioReaction._player != null )
                RadioReaction._player.Destroy();
        }

        internal static Actuator[] CreateActuators(XElement a) {
            List<Actuator> actuators = new List<Actuator>();
            Type t;
            foreach ( var c in a.Elements() ) {
                if ( _activatorsTypes.TryGetValue( c.Name.LocalName, out t ) )
                    actuators.Add( Activator.CreateInstance( t, c ) as Actuator );
            }

            return actuators.ToArray();
        }

        public static void Fire( string profile, string id) {
            lock ( _profileLocker )
                foreach ( var p in _profiles ) {
                    if ( p.ProfileName == profile ) {
                        p.Fire( id );
                        break;
                    }
                }
        }

        public static void Parse( XElement x) {
            lock( _profileLocker ) {
                Config.Clear();
                foreach ( var item in x.Element( "config" ).Elements() )
                    Config[item.Attribute( "key" ).Value] = item.Attribute( "value" ).Value;

                GlobalVolume = int.Parse( GetValue( "volume", "0.0" ) ) / 100.0;

                _profile = null;
                foreach ( var p in _profiles )
                    p.Deactivate();

                _profiles.Clear();

                foreach ( var p in x.Elements( "profile" ) ) {
                    var xp = new Profile( p );
                    _profiles.Add( xp );
                    xp.Deactivate();
                }
            }
        }
    }
}
