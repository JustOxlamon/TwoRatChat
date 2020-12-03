using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoRatChat.Main.Helpers {
    public static class DictionaryHelper {
        public static T SafeGet<K, T>( this Dictionary<K, T> This, K key, T Default) {
            T val;
            if ( This.TryGetValue( key, out val ) )
                return val;
            return Default;
        }
    }
}
