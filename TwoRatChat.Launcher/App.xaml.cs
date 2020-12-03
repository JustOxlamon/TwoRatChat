using Oxlamon.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using TwoRatChat.Update;

namespace TwoRatChat.Launcher {
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application {
        string _dropBoxRoot = Launcher.Properties.Settings.Default.DropboxUrlRoot;
        TwoRatChat.Update.Manager _updateManager;
        string _rootFolder;
        Downloading _downloadingWindow;
        SmartArgs _args;

        public App() {
            _args = new SmartArgs();
            _rootFolder = System.IO.Directory.GetCurrentDirectory();

            if ( _args.ContainsKey( "create" ) ) {
                try {
                    if ( CreateUpdate() )
                        MessageBox.Show( "Complete!" );
                } catch ( Exception er ) {
                    MessageBox.Show( "Error: " + er.ToString() );
                    return;
                }
            } else {

                clean();

                _downloadingWindow = new Downloading();
                _downloadingWindow.Closed += _downloadingWindow_Closed;


                _updateManager = new Update.Manager(
                    _dropBoxRoot,
                    _rootFolder,
                    _rootFolder + "\\update" );

                _updateManager.UpdateComplete += UpdateComplete;
                _updateManager.UpdateDetected += _updateManager_UpdateDetected;
                _updateManager.OnProgress += _updateManager_OnProgress;
                _updateManager.Run();
            }
        }

      

        void _updateManager_OnProgress( string title, double? progress ) {
            Dispatcher.Invoke( () => {
                if (_downloadingWindow == null) {
                    _downloadingWindow = new Downloading();
                    _downloadingWindow.Closed += _downloadingWindow_Closed;
                    _downloadingWindow.Show();
                }

                _downloadingWindow.OnProgress( title, progress );
            } );
        }

        void _downloadingWindow_Closed( object sender, EventArgs e ) {
            _updateManager.Cancel();
            _downloadingWindow = null;
        }

        bool _updateManager_UpdateDetected( string whatsNew ) {
            bool result =false;
            Dispatcher.Invoke(
                () => {
                    _updateManager.CloseSplash();
                    MainWindow _mainWindow = new Launcher.MainWindow();
                    _mainWindow.SetUpdateText( whatsNew );
                    var x = _mainWindow.ShowDialog();
                    if (x.HasValue) {
                        result = x.Value;
                        if (result)
                            _downloadingWindow.Show();
                    } else
                        result = false;
                }
            );
         //   Thread.Sleep( 1000 );
            return result;
        }

        public void UpdateComplete( bool rebootRequired ) {
            if (_downloadingWindow != null )
            Dispatcher.Invoke(
             () => {
                 _updateManager.CloseSplash();
                 _downloadingWindow.Close();
             } );

            // RUN TWORATCHAT
            Process.Start( Launcher.Properties.Settings.Default.RunApplication, "run" );
        }





        #region Create update
        static HashSet<string> blacklistext = new HashSet<string>( new string[]{
            ".pdb",
            ".config",
            ".manifest",
            ".vshost.exe",
            ".zip",
            ".application",
            ".log",
            ".bat",
            ".bak",
            ".lic"
        } );

        static bool isAllow( string file ) {
            if (file.StartsWith( "\\update" ))
                return false;
            if ( file.StartsWith( "\\local" ) )
                return false;

            foreach ( var x in blacklistext)
                if (file.EndsWith( x ))
                    return false;
            return true;
        }

        string _dropboxFolder;

        private void clean() {
            foreach (var file in Directory.GetFiles( _rootFolder, "*.bak", SearchOption.AllDirectories )) {
                try {
                    File.Delete( file );
                } catch {
                }
            }
        }

        private bool CreateUpdate() {
            _dropboxFolder = Launcher.Properties.Settings.Default.DropboxRoot;
            string _dropXml = _dropboxFolder + "\\..\\tworatchat.xml";
            string text = "";
            InputString ins = new InputString();

            XElement old = null;
            if (File.Exists( _dropXml )) {
                old = XElement.Load( _dropXml );
                ins.text.Text = old.Attribute( "news" ).Value;
            }

            byte[] data = File.ReadAllBytes( _rootFolder + "\\" + Launcher.Properties.Settings.Default.RunApplication );

            Assembly exe = Assembly.ReflectionOnlyLoad( data );

            string ver = string.Format( "TwoRatChat version: {0}\r\n===============================================\r\n\r\n\r\n", exe.GetName().Version );

            ins.text.Text = ver + ins.text.Text;

            var xu = ins.ShowDialog();
            if (xu.HasValue && xu.Value) {
                text = ins.Text;
            }

            ins = null;

            if (string.IsNullOrEmpty( text )) {
                MessageBox.Show( "Создание обновления отменено." );
                return false;
            }

            try {
                File.Delete( _dropXml );

                XElement x = new XElement( "list" );
                x.Add( new XAttribute( "news", text ) );
                foreach (var file in Directory.GetFiles( _rootFolder , "*", SearchOption.AllDirectories )) {
                    string relativeName = file.Substring( _rootFolder.Length );

                    if (isAllow( relativeName )) {
                        x.Add( saveFile( relativeName ).ToXElement() );
                    } else {
                        //Console.WriteLine( file.Substring( _rootFolder.Length ) + " - ignored." );
                    }
                }

                x.Save( _dropXml );

            } catch (Exception er) {
                Console.WriteLine( er );
                return false;
            }


            return true;
        }

        private FileItem saveFile( string file ) {
            byte[] data = File.ReadAllBytes( _rootFolder + file );
            string fileName = _dropboxFolder + file;

            Oxlamon.Common.IO.PathEx.CreateFolderPath( Path.GetDirectoryName( fileName ) );

            using (MemoryStream ms = new MemoryStream()) {
                long crc = data.Crc();
                long zipCrc = 0;
                using (Stream z = new GZipStream( ms, CompressionLevel.Optimal )) {
                    z.Write( data, 0, data.Length );
                    z.Flush();
                }
                zipCrc = ms.ToArray().Crc();
                File.WriteAllBytes( fileName + ".twoRatChat", ms.ToArray() );

                return new FileItem( file, crc, zipCrc );
            }
        }

        #endregion
    }
}
