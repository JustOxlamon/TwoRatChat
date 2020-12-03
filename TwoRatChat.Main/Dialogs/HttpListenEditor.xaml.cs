using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;

namespace TwoRatChat.Main.Dialogs {
    /// <summary>
    /// Interaction logic for HelloWindow.xaml
    /// </summary>
    public partial class HttpListenEditor : Window {
        public HttpListenEditor() {
            InitializeComponent();

            string s = Properties.Settings.Default.HTTP_ListenUri.ToLower();
            Uri u = new Uri( s );

            if (s.Contains( "localhost" )) {
                ListenDefault.IsChecked = true;
                ListenInterface.IsChecked = false;
                Warn.Visibility = System.Windows.Visibility.Hidden;
            } else {
                ListenInterface.IsChecked = true;
                ListenDefault.IsChecked = false;
                intIP.Text = u.Host;
                Warn.Visibility = System.Windows.Visibility.Visible;
            }

            localPort.Text = u.Port.ToString();
            intPort.Text = u.Port.ToString();

            Fill();
            _update();
        }

        private void Fill() {
            intIP.Items.Clear();
            var host = Dns.GetHostEntry( Dns.GetHostName() );
            foreach (IPAddress ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    intIP.Items.Add( ip.ToString() );

            if (intIP.Items.Count > 0)
                intIP.SelectedIndex = 0;
        }

        private void Thumb_DragDelta_1( object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e ) {
            this.Left += e.HorizontalChange;
            this.Top += e.VerticalChange;
        }

        private void Button_Click_1( object sender, RoutedEventArgs e ) {
            this.DialogResult = false;
        }

        private int runAs( string cmd ) {
            Process netsh = new Process() {
                StartInfo = new ProcessStartInfo( "netsh", cmd ) {
                    Verb = "runas"
                }
            };
            netsh.Start();
            netsh.WaitForExit();
            return netsh.ExitCode;
        }

        private void Button_Click( object sender, RoutedEventArgs e ) {
            try {
                string nl = "";
            
                if (ListenDefault.IsChecked.Value) {
                    UriBuilder u = new UriBuilder( "http", "localhost", int.Parse( localPort.Text ) );
                    nl = u.Uri.ToString();

                    if (Properties.Settings.Default.HTTP_ListenUri != nl) {
                        if (!Properties.Settings.Default.HTTP_ListenUri.Contains( "localhost" ))
                            runAs( string.Format( "http delete urlacl url={0}", Properties.Settings.Default.HTTP_ListenUri ) );

                        Properties.Settings.Default.HTTP_ListenUri = nl;
                        Properties.Settings.Default.Save();
                        this.DialogResult = true;
                        return;
                    }
                } else {
                    UriBuilder u = new UriBuilder( "http", intIP.Text, int.Parse( intPort.Text ) );
                    nl = u.Uri.ToString();

                    var x = System.Security.Principal.WindowsIdentity.GetCurrent();

                    string cmd = string.Format( "http add urlacl url={0} user={1} listen=yes",
                        nl, x.Name );


                    if (Properties.Settings.Default.HTTP_ListenUri != nl) {
                        if (!Properties.Settings.Default.HTTP_ListenUri.Contains( "localhost" ))
                            runAs( string.Format( "http delete urlacl url={0}", Properties.Settings.Default.HTTP_ListenUri ) );

                        runAs( string.Format( cmd ) );

                        Properties.Settings.Default.HTTP_ListenUri = nl;
                        Properties.Settings.Default.Save();
                        this.DialogResult = true;
                        return;
                    }
                }

                this.DialogResult = false;
            } catch( Exception er ) {
                MessageBox.Show( "Access denied or Invalid parameters.\r\n" + er.Message );
            }
        }

        void AddRights() {
        }

        private void ListenInterface_Checked( object sender, RoutedEventArgs e ) {
            Warn.Visibility = ListenInterface.IsChecked.Value ?
                System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        private void UpdateText( object sender, SelectionChangedEventArgs e ) {
            _update();
        }

        private void _update() {
            try {
                int port = int.Parse( intPort.Text );
                if (port < 1000)
                    throw new Exception();

                UriBuilder u = new UriBuilder( "http", intIP.Text, port );
                var nl = u.Uri.ToString();
                var x = System.Security.Principal.WindowsIdentity.GetCurrent();
                string cmd = string.Format( "netsh http add urlacl url={0} user={1} listen=yes",
                    nl, x.Name );

                addAcl.Text = cmd;
                Delete.Text = string.Format( "netsh http delete urlacl url={0}", nl );
                apply.IsEnabled = true;
                
            } catch {
                apply.IsEnabled = false;
            }
        }

        private void UpdateText2( object sender, TextChangedEventArgs e ) {
            _update();
        }

    }
}
