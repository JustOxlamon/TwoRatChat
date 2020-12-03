using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;

namespace TwoRatChat.Commands {
    class PlaySound : Reaction {
        #region static realization
        static Random _rnd = new Random();
        static Dictionary<Uri, MediaPlayer> _players = new Dictionary<Uri, MediaPlayer>();

        static void Play(Uri soundFile, double Volume) {
            if ( Volume <= 0.0 )
                return;
            if ( Volume > 1.0 )
                Volume = 1.0;

            MediaPlayer mp;
            if ( !_players.TryGetValue( soundFile, out mp ) ) {
                mp = new MediaPlayer();
                mp.Open( soundFile );
                mp.MediaFailed += mp_MediaFailed;

                _players.Add( soundFile, mp );
            }
            mp.Volume = Volume;
            mp.Position = new TimeSpan();
            mp.Stop();
            mp.Play();
        }

        static void mp_MediaFailed(object sender, ExceptionEventArgs e) {
        }
        #endregion

        class Sound {
            public readonly Uri uri;
            public readonly double Volume;

            public Sound( XElement x ) {
                this.uri = new Uri( CommandFactory.ResolveFilePath( x.Attribute( "fileName" ).Value ), UriKind.Absolute );
                this.Volume = double.Parse( x.Attribute( "volume" ).Value, CultureInfo.InvariantCulture );
            }
        }

        List<Sound> SoundFiles;
        double Volume;

        public PlaySound(XElement x) : this() {
            this.Volume = double.Parse( x.Attribute( "volume" ).Value, CultureInfo.InvariantCulture );

            foreach ( var s in x.Elements( "soundfile" ) )
                SoundFiles.Add( new Sound( s ) );
        }

        public PlaySound(): base() {
            this.SoundFiles = new List<Sound>();
            this.Volume = 1.0;
        }

        protected override void Execute(Actuator act) {
            Sound snd = this.SoundFiles[_rnd.Next( SoundFiles.Count )];
            Play( snd.uri, this.Volume * snd.Volume * CommandFactory.GlobalVolume );
        }
    }
}
