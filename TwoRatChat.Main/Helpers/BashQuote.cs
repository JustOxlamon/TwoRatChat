using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace TwoRatChat.Main.Helpers {
    public class BashQuote {
        Random rnd = new Random();

        public void GetRandomQuote( Action<string> onQuote ) {
            var q = onQuote;
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += ( a, b )=>{
                if( b.Error == null ) {
                    Regex r = new Regex( "<div.class=\"text\">(.*?)</div>" );
                    string t = b.Result.Replace( "\r", "" ).Replace( "\n", "" ).Replace( "<br>", " " );
                    var m = r.Match( t );
                    q( HttpUtility.HtmlDecode( m.Groups[1].Value ) );
                }else {
                    q( b.Error.Message );
                }
            };
            int x = rnd.Next( 40000, 43000 ) * 10;
            wc.DownloadStringAsync( new Uri( string.Format( "http://bash.im/quote/{0}", x ) ) );
        }
    }
}
