// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TwoRatChat.Main.Sources {
    public class YoutubeHack_ChatSource : TwoRatChat.Model.ChatSource {
        //Thread _loader;
        WebView _browser;
        System.Timers.Timer _timer;


        protected override void disposeManaged() {
            _browser.Dispose();
            _timer.Dispose();
        }

        public YoutubeHack_ChatSource( Dispatcher dispatcher ) : base( dispatcher ) {
            _timer = new System.Timers.Timer();
            _timer.Interval = 10000;
            _timer.Elapsed += _timer_Elapsed;
            
        }

        private void _timer_Elapsed( object sender, System.Timers.ElapsedEventArgs e ) {
            string sex;
            _dispatcher.Invoke( () => { sex = _browser.HTML; } );
        }

        private void _timer_Tick( object sender, EventArgs e ) {
         
        }

        public override string Id {
            get { return "youtube"; }
        }

        public override void Create( string streamerUri, string id ) {
            _dispatcher.Invoke( () => { 
                _browser = WebCore.CreateWebView( 256, 256, WebViewType.Offscreen );
                
                _browser.DocumentReady += browser_DocumentReady;
                _browser.Source = new Uri( "https://www.youtube.com/live_chat?is_popout=1&v=EeEbPh6zpHc" );
            } );

       //     _timer.Start();
        }

        void worker() {
            //  WebCore.Initialize( new WebConfig(), true );
          //  WebCore.Run();
        }

        private void browser_DocumentReady( object sender, DocumentReadyEventArgs e ) {
            if ( !_timer.Enabled )
                _timer.Start();
        }

        public override void Destroy() {
       //     WebCore.Shutdown();
            //_loader.Abort();
        }

        public override void UpdateViewerCount() {
        }
    }
}
