using Awesomium.Core;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Linq;
using TwoRatChat.Main.Commands;
using TwoRatChat.Main.Helpers;
using TwoRatChat.Model;

namespace TwoRatChat.Main {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,  IResourceInterceptor {
        public MainWindow() {
            InitializeComponent();

            Awesomium.Core.WebCore.Initialize( new WebConfig() {
                UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 5_1 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9B176 Safari/7534.48.3"                
            } );
            Awesomium.Core.WebCore.Started += ( a, b ) => {
                Awesomium.Core.WebCore.ResourceInterceptor = this;
            };// .ResourceInterceptor = this;

            Awesomium.Core.WebCore.ShuttingDown += ( a, b ) => {
                Console.Write( "dfg" );
            };

            Awesomium.Core.WebPreferences prefs = new Awesomium.Core.WebPreferences() {
                WebSecurity = false,
                FileAccessFromFileURL = true
            };
            webControl.WebSession = Awesomium.Core.WebCore.CreateWebSession(
                App.TempFolder, prefs );

            //webControl.WebSession.
 
            /*  webControl.WebSession.AddDataSource( "img",
                  new Awesomium.Core.Data.ResourceDataSource( Awesomium.Core.Data.ResourceType.Embedded ) );
                  */
            webControl.ConsoleMessage += WebControl_ConsoleMessage;
            //webControl.co
            //webControl.WebSession.

            chat = _chat = new Chat( this.Dispatcher, new ChatArgs() );
            _chat.Sources.CollectionChanged += Sources_CollectionChanged;
            _chat.OnMessageUpdated += _chat_OnMessageUpdated;
            PART_Header.DataContext = _chat.Sources;
            PART_SystemMessages.DataContext = _chat.SystemMessages;
            webControl.WebSession.AddDataSource( "rat",
                new TwoRatChat.Main.Helpers.RatDataSource( _chat ) );
            //  webControl.WebSession.AddDataSource( "chat", _chat );

            _timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds( 1 ) };
            _timer.Tick += _timer_Tick;


            TwoRatChat.Commands.CommandFactory.BeginInitialize( "ru-RU" );

            TwoRatChat.Commands.CommandFactory.RegisterActivator( "chat", typeof( TwoRatChat.Main.Commands.ChatActuator ) );
            TwoRatChat.Commands.CommandFactory.RegisterReaction( "jscommand", typeof( TwoRatChat.Main.Commands.JSCommand ) );
            TwoRatChat.Commands.CommandFactory.RegisterReaction( "print", typeof( TwoRatChat.Main.Commands.ChatReaction ) );

            TwoRatChat.Commands.CommandFactory.OnResolveFilePath += (a) => {
                return App.MapFileName( a );
            };

            TwoRatChat.Commands.CommandFactory.EndInitialize();
            var v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            header.Content = "TwoRatChat v" + v.Major + "." + v.Minor;


            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon( Application.GetResourceStream( new Uri( "pack://application:,,,/TwoRatChat.Main;component/TwoRatChat.ico" ) ).Stream );
            ni.Visible = true;
            ni.DoubleClick +=
                delegate ( object sender, EventArgs args ) {

                    Options_Click( null, null );
                };
        }

        //protected override void OnStateChanged( EventArgs e ) {
        //    if ( WindowState == WindowState.Minimized )
        //        this.Hide();

        //    base.OnStateChanged( e );
        //}

        internal static Chat chat { get; private set; }

        internal static void AddMessage(string from, string text) {
            if ( _instance.Dispatcher.CheckAccess() ) {
                _instance._chat.AddMessage( new ChatMessage() {
                    Date = DateTime.Now,
                    Name = from,
                    Source = null,
                    Text = text,
                    ToMe = true
                } );
            } else {
                _instance.Dispatcher.Invoke(()=> {
                    _instance._chat.AddMessage( new ChatMessage() {
                        Date = DateTime.Now,
                        Name = from,
                        Source = null,
                        Text = text,
                        ToMe = true
                    } );
                } );
            }
        }

        private void WebControl_ConsoleMessage(object sender, Awesomium.Core.ConsoleMessageEventArgs e) {
            App.Log( 'W', "WEBCORE[{2}]: {1}. {0}", e.Message, e.LineNumber, e.EventName );
        }

        private void Sources_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if ( _chat.Sources.Count == 0 )
                welcomeTextBlock.Visibility = Visibility.Visible;
            else
                welcomeTextBlock.Visibility = Visibility.Hidden;

            if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ) {
                webControl.Reload( true );
            }
        }

        static MainWindow _instance;
        //static string _oldSkin = "";
        Chat _chat;
        DispatcherTimer _timer;
        bool _rerenderAgain = false;
        int _delay = 0;
        TransparentMouse _TransparentMouse;
        Http.HttpServer _server;
        Dialogs.SkinDesigner _skinDesigner;


        public static void RunInUiThread( Action a ) {
            if( _instance.Dispatcher.CheckAccess() ) {
                a();
            } else {
                _instance.Dispatcher.BeginInvoke( a );
            }
        }


        internal static void RegisterChatActuator(ChatActuator chatActuator) {
            _instance._chat.AddActuator( chatActuator );
        }

        internal static void UnregisterChatActuator(ChatActuator chatActuator) {
            _instance._chat.RemoveActuator( chatActuator );
        }

        private void RemoveSystemMessage_Click(object sender, RoutedEventArgs e) {
            _chat.RemoveSystemMessage( ((Button)sender).Tag as ChatMessage );
        }

        internal static void ExecuteJS(Dest dest, string cmd, params string[] prms) {
            if ( _instance != null ) {
                if ( _instance.webControl != null && (dest & Dest.Local) > 0 )
                    _instance.webControl.ExecuteJavascript( cmd );

                if ( (dest & Dest.Server) > 0 ) {
                    //_instance._server.AddJSCommand( cmd );
                    _instance._chat.AddCommand( cmd, prms );
                }
            }
        }

        #region save/load
        private void Window_Loaded( object sender, RoutedEventArgs e ) {
            _instance = this;

            _chat.Load();
            _timer.Start();

            if ( Properties.Settings.Default.HTTP_Enable ) {
                _server = new Http.HttpServer( _chat, Properties.Settings.Default.HTTP_ListenUri );
                _server.Start();
            }
            try {
                TwoRatChat.Commands.CommandFactory.Parse( XElement.Load( App.MapFileName( "/commands.xml" ) ) );
            } catch ( Exception er ) {
                App.Log( '!', "Ошибка загрузки commands.xml: {0}", er );
            }
            App.Log( ' ', "CommandFactory loaded" );

            ReloadSkin();

        }

        public static void ReloadSkin() {


            string newSkin = App.MapFileName( Properties.Settings.Default.Chat_RootSkin );
            if ( !string.IsNullOrEmpty( newSkin ) )
                _instance.webControl.LoadHTML(
                    File.ReadAllText( newSkin ).Replace( "/tworat/", "asset://rat/" ) );
        }

        private void Window_SourceInitialized(object sender, EventArgs e) {
            _TransparentMouse = new TransparentMouse( this );
            _TransparentMouse.AllowTransparency = Properties.Settings.Default.Window_MouseTransparent;
        }


        private void Window_Closed( object sender, EventArgs e ) {
            if ( _skinDesigner != null )
                _skinDesigner.Close();
            if ( _server != null )
                _server.Stop();
            _timer.Stop();
            _chat.Save();
            _chat.Stop();
            Properties.Settings.Default.Save();

            TwoRatChat.Commands.CommandFactory.Destroy();
        }
        #endregion

        #region CHAT
        void _timer_Tick( object sender, EventArgs e ) {
            if (_delay > 0)
                _delay--;
            else {
                _rerenderAgain = true;
                _chat.OnTimer();
                _delay = 10;
            }

            if( (_delay % 2) == 0 )
                _rerenderAgain = true;

            if (_rerenderAgain) {
                _rerenderAgain = false;
                if (!string.IsNullOrEmpty( Properties.Settings.Default.Chat_ImagePath )) {
                    TwoRatChat.Main.Helpers.Renderer.RenderAndSave( this, Properties.Settings.Default.Chat_ImagePath, () => { } );
                }
            }

            var error = webControl.GetLastError();
            if( error != Awesomium.Core.Error.None ) {

            }
        }

        internal static void OnCompleteCommand(string id) {
            _instance._chat.RemoveCommand( new Guid( id ) );
        }

        void _chat_OnMessageUpdated() {
            _rerenderAgain = true;
        }
        #endregion

        #region Context menu
        private void MenuItem_Click( object sender, RoutedEventArgs e ) {
            AddSourceDialog asd = new AddSourceDialog();
            var x = asd.ShowDialog();
            if (x.HasValue && x.Value) {
                _chat.AddSource(
                    asd.chatSourceCB.SelectedItem as TwoRatChat.Main.Sources.Source,
                    asd.chatSourceUri.Text,
                    asd.chatMessageID.Text );
            }
        }

        Dialogs.CustomUser _currentCustomUserDialog;
        private void CustomUser_Click( object sender, RoutedEventArgs e ) {
            if( _currentCustomUserDialog == null ) {
                _currentCustomUserDialog = new Dialogs.CustomUser();
                _currentCustomUserDialog.Show();
                _currentCustomUserDialog.Closed += ( a, b ) => {
                    _currentCustomUserDialog = null;
                };
            } else {
                _currentCustomUserDialog.Activate();
            }
        }

        private void SkinDesigner_Click(object sender, RoutedEventArgs e) {
            if ( _skinDesigner == null ) {
                _skinDesigner = new Dialogs.SkinDesigner();
                _skinDesigner.Closed += (a, b) => {
                    _skinDesigner = null;
                    _chat.FakeMode = false;
                };
                _chat.FakeMode = true;
                _skinDesigner.Show();
            }
        }

        private void Options_Click2(object sender, EventArgs e) {
            Options_Click( sender, null );
        }

        private void MenuItem_Click2(object sender, EventArgs e) {
            MenuItem_Click( sender, null );
        }

        private void MenuItemClose_Click( object sender, RoutedEventArgs e ) {
            this.Close();
        }

        private void ResetPosition_Click(object sender, EventArgs e) {
            this.Left = 100;
            this.Top = 100;
            this.Width = 100;
            this.Height = 100;
        }

        private void ReloadChat_Click( object sender, RoutedEventArgs e ) {
            ChatSource cs = (sender as MenuItem).Tag as ChatSource;
            _chat.ReloadChat( cs );
        }

        private void CloseChat_Click( object sender, RoutedEventArgs e ) {
            ChatSource cs = (sender as MenuItem).Tag as ChatSource;
            _chat.CloseChat( cs );
        }

        private void FortuneRingEditor_Click(object sender, RoutedEventArgs e) {
            FortuneWindow fw = new FortuneWindow();
            fw.Show( _chat.FortuneList );
        }

        private void BlacklistEditor_Click(object sender, RoutedEventArgs e) {
            BlackListForm blf = new BlackListForm();
            blf.ShowDialog( _chat.BlackList );
        }

        private void PollEditor_Click(object sender, RoutedEventArgs e) {
            PollingEditorForm ped = new PollingEditorForm();
            ped.Show( _chat.Polling );
        }

        private void Options_Click(object sender, RoutedEventArgs e) {
            Dialogs.OptionsDialog od = new Dialogs.OptionsDialog();

            od.ShowDialog();

            _TransparentMouse.AllowTransparency = Properties.Settings.Default.Window_MouseTransparent;
        }
        #endregion

        #region helpers
        private void Thumb_DragDelta_1( object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e ) {
            this.Left += e.HorizontalChange;
            this.Top += e.VerticalChange;
        }

        public ResourceResponse OnRequest(ResourceRequest request) {
           // Console.WriteLine( request.Url.OriginalString );
            return null;
        }

        public bool OnFilterNavigation(NavigationRequest request) {
            return false;
        }

        #endregion

        private void TalkForMe_Click( object sender, RoutedEventArgs e ) {
            Talk t = new Talk();
            t.Show();
        }
    }
}
