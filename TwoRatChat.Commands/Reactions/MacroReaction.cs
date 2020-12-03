using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TwoRatChat.Commands {
    class MacroReaction : Reaction {
        static Dictionary<string, Macro> _macroses = new Dictionary<string, Macro>();
        static bool run(string id) {
            Macro x;
            if ( _macroses.TryGetValue( id, out x ) ) {
                // if ( x.IsWindowActive( x.WindowTitle ) ) {
                Thread t = new Thread( () => {
                    x.Run();
                } );
                t.Start();
                return true;
                //}
            }
            return false;
        }

        string _cmdid;
        public MacroReaction(XElement x) : base() {
            _cmdid = Guid.NewGuid().ToString();

            var macro = new Macro();
           // macro.WindowTitle = x.Attribute( "window" ).Value;
            macro.Parse( x.Value );

            _macroses.Add( _cmdid, macro );
        }

        protected override void Execute(Actuator act) {
            run( _cmdid );
        }
    }
}