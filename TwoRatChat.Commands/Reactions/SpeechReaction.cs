using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TwoRatChat.Commands {
    class SpeechReaction : Reaction {
        string _voice;
        string _text;

        public SpeechReaction(XElement x): base() {
            _voice = x.Attribute( "voice" ).Value;
            _text = x.Attribute( "text" ).Value;
        }

        protected override void Execute(Actuator act) {
            CommandFactory.Talk( _voice, _text );
        }
    }
}
