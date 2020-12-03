using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TwoRatChat.Commands;

namespace TwoRatChat.Main.Commands {
    class ChatReaction : Reaction {
        string _text;

        public ChatReaction( XElement x ): base() {
            this._text = x.Attribute( "format" ).Value;
        }

        protected override void Execute(Actuator obj) {
            string x = this._text;

            foreach( var kv in obj ) 
                x = x.Replace( "{" + kv.Key + "}", kv.Value );

            MainWindow.AddMessage( "TwoRatChat", x );
        }
    }
}
