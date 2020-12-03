using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TwoRatChat.Main.Commands {
    [Flags]
    [System.Reflection.Obfuscation(Exclude =true)]
    public enum Dest {
        Server = 0x01,
        Local = 0x02,
        Both = 0x03
    };

    class JSCommand : TwoRatChat.Commands.Reaction {
        string _cmd;
        Dest _dest;

        string[] _prms;

        public JSCommand(XElement x) : base() {
            this._cmd = x.Attribute( "cmd" ).Value;
            this._dest = (Dest)Enum.Parse( typeof( Dest ), x.Attribute( "dest" ).Value );

            var p = x.Attribute( "prms" );
            if ( p == null )
                this._prms = new string[0];
            else
                this._prms = p.Value.Split( '|' );
        }

        protected override void Execute( TwoRatChat.Commands.Actuator act ) {
            MainWindow.ExecuteJS( _dest, _cmd, _prms );
        }
    }
}
