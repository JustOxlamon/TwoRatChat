using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media;

namespace TwoRatChat.Commands {
    class MusicPlayerReaction : Reaction {
        Queue<string> _youtubeTracks = new Queue<string>();
        Timer _timers;


        public MusicPlayerReaction(): base() {
            _timers = new Timer();
            _timers.Interval = 1000.0;
            _timers.Elapsed += _timers_Elapsed;
            _timers.Start();

        }


        private void _timers_Elapsed(object sender, ElapsedEventArgs e) {
        }

        protected override void Execute(Actuator obj) {
            if( obj["cmd"] == "youtube" ) {
                _youtubeTracks.Enqueue( obj["param1"] );
            }
        }
    }
}
