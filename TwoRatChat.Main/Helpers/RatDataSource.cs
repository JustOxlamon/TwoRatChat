using Awesomium.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TwoRatChat.Model;

namespace TwoRatChat.Main.Helpers {
    public class RatDataSource :DataSource {
        Chat _chat;
        internal RatDataSource( Chat chat ): base() {
            _chat = chat;
        }

        protected override void OnRequest( DataSourceRequest request ) {
           // App.Log( 'H', "RAT PROTO REQUEST: {0}", request.Url.LocalPath );
            byte[] data = null;
            //Console.WriteLine( "RAT PROTO REQUEST: {0}", request.Url.LocalPath );
            if ( request.Url.LocalPath =="/chat/" ) {

                IntPtr unmanagedPointer = IntPtr.Zero;
                try {
                    data = _chat.GetEncodedMessages();
                    unmanagedPointer = Marshal.AllocHGlobal( data.Length );
                    Marshal.Copy( data, 0, unmanagedPointer, data.Length );

                    var response = new Awesomium.Core.Data.DataSourceResponse();
                    response.Buffer = unmanagedPointer;
                    response.Size = (uint)data.Length;

                    this.SendResponse( request, response );

                } catch {
                    this.SendRequestFailed( request );
                } finally {
                    Marshal.FreeHGlobal( unmanagedPointer );
                }

                return;
            }

            string fileName = App.MapFileName( request.Url.LocalPath );


            if ( string.IsNullOrEmpty( fileName ) ) {
                if ( request.Url.LocalPath.StartsWith( "/http" ) ) {
                    try {
                        data = new System.Net.WebClient().DownloadData( request.Url.LocalPath.Substring( 1 ) );
                    } catch {

                    }
                }
            } else {
                try {
                    data = File.ReadAllBytes( fileName );
                } catch {
                }
            }

            if( data != null ) { 
                IntPtr unmanagedPointer = IntPtr.Zero;
                try {
                    unmanagedPointer = Marshal.AllocHGlobal( data.Length );
                    Marshal.Copy( data, 0, unmanagedPointer, data.Length );

                    var response = new DataSourceResponse();
                    response.Buffer = unmanagedPointer;
                    response.Size = (uint)data.Length;

                    this.SendResponse( request, response );

                } catch {
                    this.SendRequestFailed( request );
                } finally {
                    Marshal.FreeHGlobal( unmanagedPointer );
                }
            } else {
                this.SendRequestFailed( request );
            }
        }
    }
}
