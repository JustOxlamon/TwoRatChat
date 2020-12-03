// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoRatChat.Model {
    public class BlackItem {
        public int Source { get; set; }
        public string Nick { get; set; }
        public string ReplaceText { get; set; }

        public BlackItem(int source, string nick, string replaceText = null) {
            this.Nick = nick;
            this.Source = source;
            this.ReplaceText = replaceText;
        }

        public static BlackItem Parse(string text) {
            string[] data = text.Split( '•' );
            return new BlackItem( int.Parse( data[0] ), data[1], data[2] );
        }

        public override string ToString() {
            return string.Format( "{0}•{1}•{2}", this.Source, this.Nick, this.ReplaceText );
        }
    }

    public class BlackList : ObservableCollection<BlackItem> {
        public BlackList() {
        }

        public void Add( int source, string nick, string replaceText = null ) {
            Add(new BlackItem( source, nick, replaceText));
        }

        //public bool IsInList( string name ) {
        //    return Find( name ) != null;
        //}

        public void Add(int source, BlackItem item) {
            if ( Find( source, item.Nick ) == null )
                base.Add( item );
        }

        protected BlackItem Find(int source, string nick) {
            foreach ( var v in this )
                if ( v.Source == source || source == -1 || v.Source == -1)
                    if ( string.Compare( v.Nick, nick, true ) == 0 )
                        return v;
            return null;
        }

        public bool IsInList( ChatSource source, string nick ) {
            if ( source == null )
                return false;
            BlackItem bi = Find( source.SystemId, nick );
            return bi != null;
        }

        public string GetReplaceText( ChatSource source, string nick, string originalText ) {
            if ( source == null )
                return originalText;

            BlackItem bi = Find( source.SystemId, nick );

            if (bi == null)
                return originalText;

            return bi.ReplaceText;
        }

        public void Load( string text ) {
            string[] s = text.Split( new string[] { "¶" }, StringSplitOptions.RemoveEmptyEntries );
            if ( s.Length > 0 )
                foreach ( var v in s )
                    base.Add( BlackItem.Parse( v ) );
        }

        public string Save() {
            StringBuilder sb = new StringBuilder();
            foreach (var v in this)
                if (sb.Length == 0)
                    sb.Append(v.ToString());
                else
                    sb.AppendFormat("¶{0}", v.ToString());
            return sb.ToString();
        }
    }
}
