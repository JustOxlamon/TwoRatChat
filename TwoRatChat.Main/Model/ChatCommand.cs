// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoRatChat.Model {
    public class ChatCommand {
        public ChatCommand( string nickName ) {
            this.NickName = nickName;
        }

        public readonly string NickName;
    }
}
