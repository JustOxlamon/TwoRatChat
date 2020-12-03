using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TwoRatChat.Main.Helpers {
    class Renderer {
        static long _crc = 0;

        public static void RenderAndSave( FrameworkElement element, string fileName, Action onComplete ) {
            try {
                RenderTargetBitmap bmpSource = null;
                element.Dispatcher.Invoke( () => {
                    bmpSource = new RenderTargetBitmap(
                        (int)element.ActualWidth, (int)element.ActualHeight, 96, 96,
                        PixelFormats.Pbgra32 );
                    bmpSource.Render( element );
                    bmpSource.Freeze();
                } );

                ThreadPool.QueueUserWorkItem(
                    new WaitCallback( ( a ) => {
                        var p = a as Tuple<string, RenderTargetBitmap>;

                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add( BitmapFrame.Create( p.Item2 ) );

                        byte[] image = null;

                        using (MemoryStream ms = new MemoryStream()) {
                            encoder.Save( ms );
                            //ms.Close();
                            image = ms.ToArray();
                        }

                        long newCrc = Crc32.Crc( image );

                        if (newCrc != _crc) {

                            for (int j = 0; j < 5; ++j) {
                                try {
                                    File.WriteAllBytes( p.Item1, image );
                                    _crc = newCrc;
                                    break;
                                } catch {
                                }
                            }
                        }

                        if (onComplete != null)
                            onComplete();

                    } ), new Tuple<string, RenderTargetBitmap>( fileName, bmpSource ) );
            } catch ( Exception er ) {
                App.Log( '?', "Renderer error: {0}", er.Message );
            }
        }
    }
}
