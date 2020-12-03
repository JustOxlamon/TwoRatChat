using NHotkey;
using NHotkey.Wpf;
using System;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;

namespace TwoRatChat.Commands {
    class Hotkey : Actuator {
        readonly string _id = Guid.NewGuid().ToString();
        static Dispatcher _d = Dispatcher.FromThread( System.Threading.Thread.CurrentThread );
        Key PlayKey;
        ModifierKeys PlayKeyModifiers;

        public Hotkey(XElement x) : base(x) {
            this.PlayKey = (Key)Enum.Parse( typeof( Key ), x.Attribute( "key" ).Value );
            string[] mk = x.Attribute( "modifiers" ).Value.Split( new char[] { '+', ',', '|' }, StringSplitOptions.RemoveEmptyEntries );

            this.PlayKeyModifiers = ModifierKeys.None;

            foreach ( var s in mk )
                this.PlayKeyModifiers |= (ModifierKeys)Enum.Parse( typeof( ModifierKeys ), s );
        }

        public override void Register() {
            try {
                _d.Invoke( () =>
                HotkeyManager.Current.AddOrReplace(
                    this._id,
                    this.PlayKey,
                    this.PlayKeyModifiers,
                    onHotKey )
                    );
            } catch ( Exception er ) {
                Console.WriteLine( "Error while add hotkey {1} + {2}: {0}", er.Message, this.PlayKey, this.PlayKeyModifiers );
            }
        }

        protected void onHotKey(object sender, HotkeyEventArgs e) {
            fireActuating();
        }

        public override void Unregister() {
            _d.Invoke( () =>
            HotkeyManager.Current.Remove( this._id ) );
        }
    }
}
