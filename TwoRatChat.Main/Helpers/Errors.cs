using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoRatChat.Main.Helpers {
    internal static class Errors {
        public static string GetLocalizedError( int code ) {
            string id = string.Format( "ERROR_{0:000}", code );

            string ret = Properties.Resources.ResourceManager.GetString( id );
            if ( !string.IsNullOrEmpty( ret ) )
                return ret;
            return string.Format( "Unknown error. Code: {0:000}", code );
        }
    }
}
