using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TwoRatChat.Commands {
    class VoiceActuator : Actuator {
        CultureInfo _locale;
        string _start;
        string[] _phrases;

        public VoiceActuator(XElement x) : base( x ) {
            _locale = new CultureInfo( x.Attribute( "culture" ).Value );
            _start = x.Attribute( "start" ).Value;
            _phrases = x.Attribute( "phrases" ).Value.Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries );
        }

        public override void Register() {
            CommandFactory.registerVoiceActuator( this, _locale, _start, _phrases );
        }

        public override void Unregister() {
            CommandFactory.unregisterVoiceActuator( this );
        }

        public void OnVoiceCommand() {
            this.fireActuating();
        }
    }
}
