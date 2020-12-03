using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TwoRatChat.Commands.Radio;

namespace TwoRatChat.Commands {
    public enum RadioCommand {
        PlayRadio,
        StopRadio,

        PlayFile,
        EnqueueYoutube,

        CancelTrack,

        Mute,

        VolumeUp,
        VolumeDown

    }

    class RadioReaction: Reaction {
        internal static StreamMp3Player _player = new StreamMp3Player();

        RadioCommand _cmd;
        string _fileName;

        public RadioReaction( XElement x): base() {
            _cmd = (RadioCommand)Enum.Parse( typeof(RadioCommand), x.Attribute( "cmd" ).Value );
            if ( x.Attribute( "param1" ) != null )
                _fileName = x.Attribute( "param1" ).Value;
        }

        protected override void Execute(Actuator obj) {
            switch( _cmd ) {
                case RadioCommand.StopRadio:
                    _player.StopRadio();
                    break;

                case RadioCommand.PlayRadio:
                    _player.PlayRadio( _fileName );
                    break;

                case RadioCommand.PlayFile:
                    _player.PlayFile( _fileName );
                    break;

                case RadioCommand.EnqueueYoutube:
                    _player.EnqueueYoutubeFile( obj["param1"] );
                    break;

                case RadioCommand.CancelTrack:
                    _player.CancelCurrentTrack();
                    break;

                case RadioCommand.Mute:
                    CommandFactory.GlobalVolume = 0.0;
                    break;

                case RadioCommand.VolumeDown:
                    CommandFactory.GlobalVolume -= 0.1;
                    break;
                case RadioCommand.VolumeUp:
                    CommandFactory.GlobalVolume += 0.1;
                    break;
            }
        }

        internal static void UpdateVolume() {
            _player.UpdateVolume();
        }
    }
}
