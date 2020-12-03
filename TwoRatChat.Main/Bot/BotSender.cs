using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoRatChat.Model;

namespace TwoRatChat.Main.Bot {
    public abstract class BotSender {
        public string Login { get; protected set; }
        public string Pass { get; protected set; }
        public string Source { get; protected set; }

        public abstract string Name { get; }

        public BotSender() {

        }

        public virtual void SetCredentials(string login, string pass, string source) {
            this.Login = login;
            this.Pass = pass;
            this.Source = source;
        }

        public abstract void OnMessage( ChatMessage msg );

        public virtual void OnTimer() { }

        public abstract void Send(string text, string nick = null);
    }
}
