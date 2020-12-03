using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TwoRatChat.Commands {
    public class BaseTextCommandActuator: Actuator {
        protected Regex _regex;

        public BaseTextCommandActuator(XElement x) : base(x) {
            _regex = new Regex( x.Attribute( "regex" ).Value );
        }

        public void OnText( string text ) {
            Match m = _regex.Match( text );
            if( m.Success ) {

                for( int j=1; j<m.Groups.Count; ++j ) 
                    this["param" + j] = m.Groups[j].Value;

                fireActuating();
            }
        }

        public override void Register() {
        }

        public override void Unregister() {
        }
    }
}
