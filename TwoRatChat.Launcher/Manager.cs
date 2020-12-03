using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TwoRatChat.Launcher;

namespace TwoRatChat.Update {
    static class LogMan {
        public static void Warn( this string This, params object[] data ) {

        }

        public static void Error( this string This, params object[] data ) {

        }

        public static void Log( this string This, params object[] data ) {

        }
    }

    public delegate bool UpdateDetectedDelegate( string whatsNew );

    public class Manager {
        Thread _thread;
        Splash _splash = new Splash();

        public void CloseSplash() {
            _splash.Close();
        }

        public Manager( string dropboxRoot, string fileSystemRoot, string updateFolder ) {
            this._dropboxRoot = dropboxRoot;
            this._fsRoot = fileSystemRoot;
            this._upRoot = updateFolder;
            this.OnProgress += ( a, b ) => { };
            this._thread = new Thread( WorkFunction );
        }

        public void Run() {
            _splash.Show();
            this._thread.Start();
        }

        string _dropboxRoot;
        string _fsRoot;
        string _upRoot;
        //public bool AllowUpdate { get; set; }
       // public bool RebootRequired { get; set; }
       // public event Action<string> UpdateDetected;
        //DateTime? _waitUntil;

        public event UpdateDetectedDelegate UpdateDetected;
        public event Action<bool> UpdateComplete;
        public event Action<string, double?> OnProgress;

        bool _checkUser = true;
        bool _inCancel = false;
        internal void Cancel() {
            _inCancel = true;
        }

        protected void Sleep( int seconds ) {
            for (int j = 0; j < seconds * 10; ++j) {
                if (_inCancel)
                    return;
                Thread.Sleep( 100 );
            }
        }

        protected void WorkFunction() {
            try {
                bool failed = true;
                for (int tries = 0; tries < 3; tries++) {
                    if (_inCancel)
                        return;
                    string news;
                    var list = checkFiles( out news );
                    if (list == null) {
                        if (_inCancel)
                            return;
                        OnProgress( "Error in check. Wait 5 seconds to go.", null );
                        Sleep( 5 );
                    } else {
                        if (list.Count == 0) {
                            if (_inCancel)
                                return;

                            actualizeUpdate();
                            return;
                        } else {
                            if (_checkUser) {
                                _checkUser = false;
                                if (!this.UpdateDetected( news )) {
                                    return;
                                }
                            }

                            double n = 0.0;

                            // downloading
                            foreach (var item in list) {
                                if (_inCancel)
                                    return;
                                OnProgress( string.Format( "Downloading: {0}", item.fileName ), n / list.Count * 100.0 );
                                n += 1.0;
                                if (downloadFile( item, string.Format( "Downloading: {0}", item.fileName ), n / list.Count * 100.0 ) ) {
                                    if (_inCancel)
                                        return;
                                    OnProgress( "Failed to download. Recheck in 5 seconds.", null );
                                    Sleep( 5 );
                                    break;
                                }
                            }
                        }
                    }
                }

                if (failed) {
                    OnProgress( "Update failed, try again later :'(", null );
                    Sleep( 10 );
                }

            } finally {
                UpdateComplete( false );
            }
        }

        private string BytesTo( double bytes ) {
            if ( bytes < 1000.0 )
                return bytes.ToString( "0b" );

            if ( bytes < 1000000.0 )
                return (bytes / 1024.0).ToString( "0.00 Kb" );


            if ( bytes < 1000000000.0 )
                return (bytes / (1024*1024)).ToString( "0.00 Mb" );

            return (bytes / (1024*1024*1024)).ToString( "0.00 Gb" );
        }

        private byte[] downloadFile( string url, string title, double progress ) {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create( url );
            using( MemoryStream ms = new MemoryStream() ) {
                byte[] buffer = new byte[1024 * 5];

                using( HttpWebResponse response = (HttpWebResponse)request.GetResponse() ) {

                    long length = request.ContentLength;

                    using( Stream sm = response.GetResponseStream() ) {
                        int l;
                        while( (l = sm.Read(buffer, 0, buffer.Length)) > 0 ) {
                            ms.Write( buffer, 0, l );

                            OnProgress( string.Format( "{0}, {1}", title, BytesTo(ms.Length) ),
                                 progress );
                        }
                    }
                }

                return ms.ToArray();
            }
        }


        private bool downloadFile( FileItem item, string title, double progress ) {

            "Downloading file {0}".Log( item.fileName );
            try {
                //WebClient wc = new WebClient();
                //byte[] data = wc.DownloadData( _dropboxRoot + "Update/" + item.fileName + ".twoRatChat" );

                byte[] data = downloadFile( _dropboxRoot + "Update/" + item.fileName + ".twoRatChat", title, progress );

                if ( data.Crc() == item.zipCrc ) {
                    "File downloaded and crc is same. Saving.".Log();
                    string fileName = _upRoot + item.fileName;
                    if( File.Exists( fileName ) )
                        File.Delete( fileName );

                    Oxlamon.Common.IO.PathEx.CreateFolderPath( Path.GetDirectoryName( fileName ) );

                    File.WriteAllBytes( fileName, data );
                    return false;
                } else {
                    "File downloaded, but crc is wrong.".Error();
                    // crc check failed, restart
                }
            } catch( Exception er ) {
                "Download or save file error. {0}".Error( er.Message );
            }

            return true;
        }

       

        private void makeBackup() {

            string zip = string.Format("{0}//backup-{1:yyyyMMddhh}.zip",
                _fsRoot, DateTime.Now);

            "Creating backup: {0}".Log(Path.GetFileName(zip));

            if (File.Exists(zip)) {
                "Backup already exists.".Log();
                return;
            }

            string[] files = (from f in Directory.GetFiles( _fsRoot, "backup*.zip" )
                              orderby new FileInfo( f ).CreationTime descending
                              select f).ToArray();

            if ( files.Length > 3 ) {
                for ( int j = 3; j < files.Length; ++j )
                    try {
                        File.Delete( files[j] );
                    } catch {
                    }
            }


            try {
                using (Stream zipfile = File.OpenWrite(zip)) {
                    using (ZipArchive za = new ZipArchive(zipfile, ZipArchiveMode.Create)) {
                        foreach (var file in Directory.GetFiles(_fsRoot, "*", SearchOption.AllDirectories)) {
                            if (!file.StartsWith(_upRoot) && Path.GetExtension(file) != ".zip") {
                                za.CreateEntryFromFile(file, file.Substring(_fsRoot.Length));
                            }
                        }
                    }
                }
            } catch ( Exception er ) {
                "Backup failed: {0}".Warn(er.Message);
                return;
            }

            "TwoRatChat folder backuped.".Log();
        }

        private bool actualizeUpdate() {
            makeBackup();

            foreach (var file in Directory.GetFiles( _upRoot, "*.*", SearchOption.AllDirectories )) {
                string relativeName = file.Substring( _upRoot.Length );
                string name = _fsRoot + relativeName;

                if (File.Exists( name )) {
                    try {
                        File.Delete( name );
                    } catch {
                        File.Move( name, name + DateTime.Now.ToString( "-yyyyMMddhhmmss" ) + ".bak" );
                    }
                }

                try {
                    Oxlamon.Common.IO.PathEx.CreateFolderPath( Path.GetDirectoryName( name ) );
                    UnpackFile( file, name );
                } catch {
                    // pizdec
                }
            }

            return true; // return true if restart required
        }

        private void UnpackFile( string sourceZipFile, string destFile ) {
            using (Stream fs = File.OpenRead( sourceZipFile )) {
                using (MemoryStream ms = new MemoryStream()) {
                    byte[] data = new byte[1024 * 10];
                    int l = 0;
                    using (Stream z = new GZipStream( fs, CompressionMode.Decompress, false )) {
                        while ((l = z.Read( data, 0, data.Length )) > 0)
                            ms.Write( data, 0, l );
                    }

                    File.WriteAllBytes( destFile, ms.ToArray() );
                }
            }

            File.Delete( sourceZipFile );
        }

        private List<FileItem> checkFiles(out string updateText) {
            List<FileItem> fi = new List<FileItem>();
            updateText = "Nothing special.";

            try {
                XElement list = XElement.Load( this._dropboxRoot + "//tworatchat.xml" );
                if (list.Attribute( "news" ) != null) {
                    updateText = list.Attribute( "news" ).Value;
                }
                foreach( var file in list.Elements( "file" ) ) {
                    FileItem serverItem = new FileItem( file );
                    if( checkFile( serverItem ) )
                        fi.Add( serverItem );
                }
            } catch {
                return null;
            }

            return fi;
        }

        /// <summary>
        /// Check file
        /// </summary>
        /// <param name="serverItem">server file</param>
        /// <returns>true - if need to be update</returns>
        private bool checkFile(FileItem serverItem) {

            long crc = 0;
            string uFile = _upRoot + serverItem.fileName;
            // check in update (zipped)

            if (File.Exists(uFile)) {
                using (Stream s = File.OpenRead(uFile))
                    crc = s.Crc();
                // Если крк файла в обновлении не совпадает с серверной сжатой крк, то обновить
                return crc != serverItem.zipCrc;
            } else {
                string oFile = _fsRoot + serverItem.fileName;
                if( File.Exists(oFile))
                    using (Stream s = File.OpenRead(oFile))
                        crc = s.Crc();

                return crc != serverItem.crc;
            }

            //return true; // файла нету нигде
        }

       
    }
}
