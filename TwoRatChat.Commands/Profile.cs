using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TwoRatChat.Commands {
    class Profile {
        bool _active = true;

        public string ProfileName;
        public Regex WindowTitle;
        public string OnActivate;

        List<Reaction> _reactions = new List<Reaction>();
        public List<string> Ids { get; private set; }

        public Profile(XElement x) {
            Ids = new List<string>();

            ProfileName = x.Attribute( "id" ).Value;
            if ( x.Attribute( "window" ) != null )
                WindowTitle = new Regex( x.Attribute( "window" ).Value );
            else
                WindowTitle = null;

            if ( x.Attribute( "onactive" ) != null )
                OnActivate = x.Attribute( "onactive" ).Value;
            else
                OnActivate = null;

            foreach ( var a in x.Elements( "action" ) ) {
                Reaction r = CommandFactory.CreateGroup( a );
                if ( r != null ) {
                    var ac = CommandFactory.CreateActuators( a );
                    if ( ac.Length > 0 ) {
                        r.SetActuators( ac );
                        r.Register();

                        if ( !string.IsNullOrEmpty( r.Id ) )
                            Ids.Add( r.Id );

                        _reactions.Add( r );
                    }
                }
            }

            Console.WriteLine( "Profile: {0} loaded. Found: {1} actions.", this.ProfileName, _reactions.Count );
        }

        public void Fire(string id) {
            foreach ( var r in _reactions )
                if ( r.Id == id )
                    r.Fire( null );
        }

        public bool Activate() {
            if ( !_active ) {
                foreach ( var r in _reactions )
                    r.Register();

                if( !string.IsNullOrEmpty( OnActivate ) ) {
                    foreach ( var r in _reactions )
                        if( r.Id == OnActivate)
                            r.Fire( null );
                }
                _active = true;
                return true;
            }
            return false;
        }

        public bool Deactivate() {
            if ( _active ) {
                foreach ( var r in _reactions )
                    r.Unregister();
                _active = false;
                return true;
            }
            return false;
        }
    }
}
