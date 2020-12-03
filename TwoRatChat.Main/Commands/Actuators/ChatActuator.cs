using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using TwoRatChat.Model;

namespace TwoRatChat.Main.Commands {
    public class ChatActuator : TwoRatChat.Commands.BaseTextCommandActuator {
        int _level;

        public ChatActuator(XElement x) : base( x ) {
            _level = int.Parse( x.Attribute( "minlevel" )?.Value??"0" );
        }

        public override void Register() {
            MainWindow.RegisterChatActuator( this );
        }

        public override void Unregister() {
            MainWindow.UnregisterChatActuator( this );
        }

        public void OnMessage(ChatMessage msg) {
            if ( msg.Level >= _level )
                this.OnText( msg.Text );
        }
    }
}
