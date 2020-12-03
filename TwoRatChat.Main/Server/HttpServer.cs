using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace TwoRatChat.Main.Http {
    internal class HttpServer: ThreadWorkerTemplate {
        HttpListener _listener;
        TwoRatChat.Model.Chat _source;
        Queue<string> _jscommands = new Queue<string>();
        bool bRightStopping = false;

        public HttpServer(TwoRatChat.Model.Chat source, string listenUri = "http://localhost:2222" )
            : base(10) {
            this._listener = new HttpListener();
            this._listener.Prefixes.Add(listenUri);

            this._source = source;
        }

        public new void Start() {
            _listener.Start();
            base.Start();
        }

        public new void Stop() {
            bRightStopping = true;
            _listener.Stop();
            base.Stop();
        }

        protected override void WorkFunction() {
            HttpListenerContext context = _listener.GetContext();

            string contentType = System.Net.Mime.MediaTypeNames.Text.Html;
            byte[] answer = OnHttpRequest(context.Request.RawUrl, ref contentType);
            context.Response.ContentType = contentType;
            context.Response.ContentEncoding = Encoding.UTF8;

            if (answer != null) {
                using (Stream s = context.Response.OutputStream) {
                    s.Write(answer, 0, answer.Length);
                    //s.Close();
                }
            } else {
                context.Response.StatusCode = 404;
                context.Response.OutputStream.Close();
            }
        }

        private string contentTypeFromFileExt( string fileExt ) {
            switch (fileExt) {
                case ".html":
                case ".htm":
                    return System.Net.Mime.MediaTypeNames.Text.Html;

                case ".js":
                    return "text/javascript";

                case ".jpg":
                case ".jpeg":
                    return System.Net.Mime.MediaTypeNames.Image.Jpeg;

                case ".gif":
                    return System.Net.Mime.MediaTypeNames.Image.Gif;

                case ".png":
                    return "image/png";

                default:
                    return System.Net.Mime.MediaTypeNames.Text.Plain;
            }
        }

        private byte[] assetsData( string uri ) {
            var resStream = Application.GetResourceStream( new Uri( "Assets" + uri, UriKind.Relative ) );
            if (resStream == null)
                return null;
            byte[] b = new byte[resStream.Stream.Length];

            resStream.Stream.Read( b, 0, b.Length );

            return b;
        }

        internal void AddJSCommand(string cmd) {
            _jscommands.Enqueue( cmd );
        }

        private byte[] OnHttpRequest( string rawUri, ref string contentType ) {
            try {
                string[] da = rawUri.Split( '?' );
                rawUri = da[0];

                if (rawUri == "/")
                    rawUri = "/default.html";

                rawUri = rawUri.Replace( "/tworat/", "/" );

                switch (rawUri) {
                    case "/chat/":
                        contentType = "text/javascript";
                        string oldText = "";
                        string text = _source.GetMessages( false );

                        try {
                            var x = Newtonsoft.Json.JsonConvert.DeserializeObject( text );
                            oldText = text;
                            text = Newtonsoft.Json.JsonConvert.SerializeObject( x );
                        } catch ( Exception err ) {
                            Main.App.Log( '?', "Ошибка сохранения в JSON: {0}\n{1}", err, oldText );
                        }
                        return Encoding.UTF8.GetBytes( text );

                    //case "/cmd":
                    //    contentType = "text/javascript";
                    //    return pushCommands();

                    case "/chat/complete":
                        return completeCommand(da[1]);

                    default:
                        string _file = App.MapFileName( rawUri );
                        if (!string.IsNullOrEmpty( _file )) {
                            byte[] data = File.ReadAllBytes( _file );
                            contentType = contentTypeFromFileExt( Path.GetExtension( _file ).ToLowerInvariant() );
                            return data;
                        }
                        break;
                }
            } catch {
            }

            return null;
        }

        private byte[] pushCommands() {
            StringBuilder sb = new StringBuilder("{ \"cmds\": [");
            bool addComma = false;
            while( _jscommands.Count > 0 ) {
                if ( addComma )
                    sb.Append( "," );
                string c = _jscommands.Dequeue();
                sb.AppendFormat( "{{ \"code\": \"{0}\" }}",
                    c.Replace( "\"", "'" ).Replace( "\\", "\\\\" ) );
                addComma = true;
            }
            sb.Append( "]}" );
            return Encoding.UTF8.GetBytes( sb.ToString() );
        }

        private byte[] completeCommand(string id) {
            StringBuilder sb = new StringBuilder( "{ \"result\": 0 }" );
            MainWindow.OnCompleteCommand( id );
            return Encoding.UTF8.GetBytes( sb.ToString() );
        }

        protected override bool OnWorkFunctionException( Exception e ) {
            return bRightStopping;
        }

        protected override void DisposeManagedResources() {
        }
    }
}
