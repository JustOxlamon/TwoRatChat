using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TwoRatChat.Commands {
    class TrackActuator : Actuator {
        public TrackActuator(XElement x) : base( x ) {
        }

        public override void Register() {
            RadioReaction._player.OnTrack += trackChanged;
        }

        public override void Unregister() {
            RadioReaction._player.OnTrack -= trackChanged;
        }

        void trackChanged( string title) {
            this["param1"] = title;
            fireActuating();
        }
    }
}
