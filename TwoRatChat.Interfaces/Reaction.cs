using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoRatChat.Commands {
    public abstract class Reaction {
        public string Id { get; set; }

        Actuator[] _actuator = null;

        public void SetActuators( Actuator[] actuators) {
            this._actuator = actuators;
            foreach( var a in _actuator)
                a.OnActuating += this._actuator_OnActuating;
        }

        private void _actuator_OnActuating(Actuator obj) {
            Execute( obj );
        }

        protected abstract void Execute(Actuator obj);

        public void Register() {
            foreach ( var a in _actuator )
                a.Register();
        }

        public void Unregister() {
            foreach ( var a in _actuator )
                a.Unregister();
        }

        public void Fire(Actuator obj) {
            this.Execute( obj );
        }
    }
}
