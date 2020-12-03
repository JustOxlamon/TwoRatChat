using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace TwoRatChat.Main {
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application {
        public static string DataFolder { get; private set; }
        public static string DataLocalFolder { get; private set; }

        public static string UserFolder { get; private set; }
        public static string TempFolder { get; private set; }

        public static void Log( char level, string Format, params object[] Params ) {
            DateTime dt = DateTime.Now;
            string text = string.Format( Format, Params );
            try {
                File.AppendAllText( TempFolder + "\\trace.log",
                    string.Format( "[{0}] {1:dd.HH:mm:ss} {2}\r\n", level, dt, text ) );
            } catch {

            }
#if DEBUG
            Console.WriteLine( string.Format( "[{0}] {1}", level, text ) );
#endif
        }

        public static Version GetRunningVersion() {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        public static string MapFileName( string relativeFileName ) {
            string file = DataLocalFolder + relativeFileName;
            if (File.Exists( file ))
                return file;
            file = DataFolder + relativeFileName;
            if (File.Exists( file ))
                return file;
            return null;
        }

        #region .ctor
        public App()
            : base() {

            Oxlamon.Common.SmartArgs args = new Oxlamon.Common.SmartArgs();
#if !ANTIKRIZIS
#if !DEBUG
            if (!args.ContainsKey( "run" )) {
                System.Diagnostics.Process.Start( "TwoRatChat.Launcher.exe" );
                if( App.Current != null )
                    App.Current.Shutdown();
                return;
            }
#endif
#endif
            if ( args.ContainsKey( "reset" ) ) {
                MessageBox.Show( "Будут сброшены настройки, после чего программа будет закрыта. Перезапустите ее самостоятельно." );
                TwoRatChat.Main.Properties.Settings.Default.Reset();
                TwoRatChat.Main.Properties.Settings.Default.Save();
                if ( App.Current != null )
                    App.Current.Shutdown();
                return;
            }

            //Oxlamon.Common.SmartArgs args = new Oxlamon.Common.SmartArgs();
            //if ( !args.ContainsKey( "czt" ) ) {
            //    if ( App.Current != null )
            //        App.Current.Shutdown();
            //    return;
            //}
            ServicePointManager.ServerCertificateValidationCallback +=
                   ( sender, cert, chain, sslPolicyErrors ) => {
                       return true;
                   };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
               | SecurityProtocolType.Tls11
               | SecurityProtocolType.Tls12
               | SecurityProtocolType.Ssl3;

            UserFolder = Directory.GetCurrentDirectory();
            Directory.CreateDirectory( Path.Combine( UserFolder, "local" ) );
            TempFolder = UserFolder + "\\local\\temp";
            Directory.CreateDirectory( TempFolder );
            DataLocalFolder = UserFolder + "\\local\\data";
            Directory.CreateDirectory( DataLocalFolder );
            
            DataFolder = UserFolder + "\\data";

            Log( ' ', "TwoRatChat started. Version: {0}", GetRunningVersion() );

            //CultureInfo.DefaultThreadCurrentCulture = new CultureInfo( "ru-RU" );

            // Код для апгрейда настроек
            if (!TwoRatChat.Main.Properties.Settings.Default.Upgraded) {
                TwoRatChat.Main.Properties.Settings.Default.Upgrade();
                TwoRatChat.Main.Properties.Settings.Default.Upgraded = true;
                TwoRatChat.Main.Properties.Settings.Default.Save();
            }

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e ) {
            Log( '!', "CurrentDomain_UnhandledException: {0}", e.ExceptionObject );
            MessageBox.Show( TwoRatChat.Main.Properties.Resources.MES_FatalError );

            Application.Current.Shutdown( -1 );
        }

        void App_DispatcherUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e ) {
            Log( '!', "App_DispatcherUnhandledException: {0}", e.Exception );
            MessageBox.Show( TwoRatChat.Main.Properties.Resources.MES_FatalError );

            Application.Current.Shutdown( -2 );
        }
        #endregion
    }
}
