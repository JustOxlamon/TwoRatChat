// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace TwoRatChat.Model {
    public abstract class ChatSource : NotifyPropertyChanged, IDisposable {
        public enum FatalErrorCodeEnum {
            Success = 0,

            ChannelNotFound = 1,
            ParsingError = 2
        }

        string _header = string.Empty;
        string _tooltip = string.Empty;
        int? _ViewersCount = null;
        bool _status = false;

        public string Header {
            get { return _header; }
            set {
                if( _header != value ){
                    _header = value;
                    raisePropertyChanged("Header");
                }
            }
        }

        public string Tooltip {
            get { return _tooltip; }
            set {
                if (_tooltip != value) {
                    _tooltip = value;
                    raisePropertyChanged( "Tooltip" );
                }
            }
        }

        public abstract string Id { get; }
        public int SystemId { get; set; }
        public bool Status {
            get { return _status; }
            set {
                if (_status != value) {
                    _status = value;
                    raisePropertyChanged( "Status" );
                }
            }
        }
  
        public string Uri { get; protected set; }
        public string Label { get; protected set; }

        HashSet<string> _keyWords = new HashSet<string>();

        public bool ContainKeywords( string text ) {
            string t = text.ToLower();
            foreach (var x in _keyWords)
                if (t.Contains( x ))
                    return true;
            return false;
        }

        public void AddKeyword( string text ) {
            _keyWords.Add( text.Trim().ToLower() );
        }

        public string SetKeywords( string text, bool toLower = true ) {
            if ( toLower ) {
                var keywords = from b in text.Split( new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries )
                               select b.Trim().ToLower();
                _keyWords = new HashSet<string>( keywords );
                return keywords.FirstOrDefault() ?? string.Empty;
            } else {
                var keywords = from b in text.Split( new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries )
                               select b.Trim();
                _keyWords = new HashSet<string>( keywords );
                return keywords.FirstOrDefault() ?? string.Empty;
            }
        }

        public ChatSource( Dispatcher dispatcher )
            : base(dispatcher) {
        }

        public int? ViewersCount {
            get { return _ViewersCount; }
            protected set {
                if (_ViewersCount != value) {
                    _ViewersCount = value;
                    raisePropertyChanged("ViewersCount");
                }
            }
        }

        public delegate void OnNewMessagesDelegate( IEnumerable<TwoRatChat.Model.ChatMessage> messages );
        public delegate void OnNewCommandsDelegate( IEnumerable<TwoRatChat.Model.ChatCommand> cmds );
        public delegate void OnFatalErrorDelegate( ChatSource sender, FatalErrorCodeEnum code );
        public delegate void OnRemoveUserMessagesDelegate(ChatSource sender, string userName);
        public delegate void OnNewFollowerDelegate(ChatSource sender, string userName);

        public event OnNewMessagesDelegate OnNewMessages;
        public event OnNewCommandsDelegate OnNewCommands;
        public event OnFatalErrorDelegate OnFatalError;
        public event OnRemoveUserMessagesDelegate OnRemoveUserMessages;
        public event OnNewFollowerDelegate OnNewFollower;

        protected void fireOnNewFollower(string userName) {
            OnNewFollower?.Invoke( this, userName );
        }

        protected void fireOnRemoveUserMessages(string userName) {
            OnRemoveUserMessages?.Invoke( this, userName );
        }

        protected void fireOnFatalError(FatalErrorCodeEnum code) {
            OnFatalError?.Invoke( this, code );
        }

        protected void newMessagesArrived( IEnumerable<TwoRatChat.Model.ChatMessage> messages ) {
            OnNewMessages?.Invoke( messages );
        }

        protected void newCommandsArrived( IEnumerable<TwoRatChat.Model.ChatCommand> cmds ) {
            OnNewCommands?.Invoke( cmds );
        }

        public virtual void ReloadChatCommand() {
        }

        public abstract void Create( string streamerUri, string id );
        public abstract void Destroy();
        public abstract void UpdateViewerCount();

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected void Dispose( bool disposing ) {
            if( !disposedValue ) {
                if( disposing ) {
                    // TODO: dispose managed state (managed objects).
                    disposeManaged();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        protected virtual void disposeManaged() {
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ChatSource() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose( true );
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
