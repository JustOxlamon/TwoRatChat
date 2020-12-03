// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TwoRatChat.Model {
    public class NotifyPropertyChanged : INotifyPropertyChanged {
        protected readonly Dispatcher _dispatcher;
        public event PropertyChangedEventHandler PropertyChanged;
        HashSet<string> Changes;

        public bool IsNotifiesPaused { get { return Changes != null; } }

        public NotifyPropertyChanged( Dispatcher dispatcher ) {
            this._dispatcher = dispatcher;
        }

        public void PauseNotifies() {
            Changes = new HashSet<string>();
        }

        public void UnpauseNotifies( bool RaiseEvents = true ) {
            if( Changes != null && RaiseEvents )
                if( PropertyChanged != null )
                    foreach( var s in Changes ) {
                        if( this._dispatcher.CheckAccess() ) {
                            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( s ) );
                        } else {
                            this._dispatcher.Invoke( new Action<string>( a => {
                                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( a ) );
                            } ), s );
                        }
                    }
            Changes = null;
        }

        protected void raisePropertyChanged( string PropertyName ) {
            if( Changes != null )
                Changes.Add( PropertyName );
            else {
                if( this._dispatcher.CheckAccess() ) {
                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( PropertyName ) );
                } else {
                    this._dispatcher.Invoke( new Action<string>( a => {
                        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( a ) );
                    } ), PropertyName );
                }
            }
        }
    }
}
