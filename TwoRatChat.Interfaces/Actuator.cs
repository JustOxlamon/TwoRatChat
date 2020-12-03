using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TwoRatChat.Commands {
    public abstract class Actuator {
        private Dictionary<string, string> _kvs = new Dictionary<string, string>();

        public event Action<Actuator> OnActuating;

        public Actuator( XElement x ) {
            foreach( var a in x.Attributes() )
                if( a.Name.LocalName.StartsWith( "cmd_" ) ) 
                    this[a.Name.LocalName.Substring( 4 )] = a.Value;
        }

        public abstract void Register();
        public abstract void Unregister();

        protected void fireActuating() {
            if ( OnActuating != null )
                OnActuating( this );
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
            foreach ( var kv in _kvs )
                yield return kv;
        }

        public string this[string key] {
            get {
                string r;
                if ( _kvs.TryGetValue( key, out r ) )
                    return r;
                return string.Empty;
            }
            set {
                _kvs[key] = value;
            }
        }
    }
}
