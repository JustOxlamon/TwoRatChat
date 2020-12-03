using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TwoRatChat.Commands {
    class NullActuator: Actuator {
        public NullActuator(XElement x): base( x ) { }

        public override void Register() {
        }

        public override void Unregister() {
        }
    }
}
