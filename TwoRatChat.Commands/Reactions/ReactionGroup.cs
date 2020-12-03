using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TwoRatChat.Commands {
    class ReactionGroup: Reaction {
        List<Reaction> _reactions = new List<Reaction>();

        internal void Add(Reaction r) {
            if ( r != null )
                _reactions.Add( r );
        }

        internal Reaction GetMeOrFirst() {
            if ( _reactions.Count == 0 )
                return null;
            if ( _reactions.Count == 1 ) {
                _reactions[0].Id = this.Id;
                return _reactions[0];
            }
            return this;
        }

        public ReactionGroup(): base() {
        }

        protected override void Execute(Actuator act) {
            foreach ( var r in _reactions )
                r.Fire( act );
        }
    }
}
