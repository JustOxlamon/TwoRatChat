// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web;
using TwoRatChat.Main;

namespace TwoRatChat.Model {
    public class ChatMessage {
        public readonly Guid GID = Guid.NewGuid();

        [JsonIgnore]
        public bool IsFake { get; set; }

        [JsonProperty( "color" )]
        public string Color { get; set; }

        /// <summary>
        /// Дата сообщения
        /// </summary>
        [JsonProperty( "date" )]
        public DateTime Date { get; set; }

        /// <summary>
        /// Автор сообщения
        /// </summary>
        [JsonProperty( "name" )]
        public string Name { get; set; }

        /// <summary>
        /// Текст сообщения
        /// </summary>
        [JsonProperty( "text" )]
        public string Text { get; set; }

        /// <summary>
        /// Источник сообщения
        /// </summary>
        [JsonIgnore]
        public ChatSource Source { get; set; }

        [JsonProperty( "level" )]
        public int Level { get; set; }

        [JsonProperty( "exp" )]
        public long Exp { get; set; }

        /// <summary>
        /// Форма сообщения
        /// 0 - одно сообщение
        /// 1 - начало сообщения
        /// 2 - середина сообщения
        /// 3 - конец сообщения
        /// </summary>
        //[JsonIgnore]
        //public int Form { get; set; }

        /// <summary>
        /// Дополнительный идентификатор
        /// </summary>
        [JsonIgnore]
        public string Id { get; set; }

        /// <summary>
        /// Содержит ли обращение к стримеру
        /// </summary>
        [JsonIgnore]
        public bool ToMe { get; set; }

        /// <summary>
        /// Бейджи
        /// </summary>
        [JsonIgnore]
        protected List<Uri> Badges { get; set; }

        public string UserSource => $"{Name}:{Source?.Id}";

        string badgesAsJson() {
            if ( Badges == null || Badges.Count == 0 )
                return "[]";

            string s = "[";
            bool comma = false;
            foreach ( var u in Badges ) {
                if ( comma )
                    s += ",";
                s += string.Format( "\"{0}\"", u );
                comma = true;
            }
            s += "]";
            return s;
        }

        public void AddBadge( string url ) {
            AddBadge( new Uri( url, UriKind.RelativeOrAbsolute ) );
        }

        public ChatMessage() {

        }

        public ChatMessage( params Uri[] badges ) {
            AddBadges( badges );
        }

        public void AddBadge( Uri url ) {
            if ( url == null )
                return;

            if ( Badges == null )
                Badges = new List<Uri>();

#if DEBUG
            if( Badges.Contains( url ) ) {
                Console.WriteLine( "WARNING" );
                return;
            }
#endif
            Badges.Add( url );
        }

        public ChatMessage AddBadges( params Uri[] urls ) {
            if ( urls != null && urls.Length > 0 )
                foreach ( var url in urls )
                    AddBadge( url );
            return this;
        }

        class jsonHelper {
            public string label;
            public string name;
            public string text;
            public bool tome;
            public DateTime date;
            public string source;
            public string gid;
            public string[] badges;
            public string color;

            public jsonHelper( ChatMessage cm ) {
                this.label = cm.Id;
                this.name = cm.Name;
                this.text = cm.Text;
                this.tome = cm.ToMe;
                this.date = cm.Date.ToUniversalTime();
                this.gid = cm.GID.ToString();

                if ( cm.Source == null ) {
                    this.source = "system";
                } else {
                    if ( badges != null && badges.Length > 0 )
                        this.badges = (from b in badges
                                       select b.ToString()).ToArray();
                    this.source = cm.Source.Id;
                    this.color = cm.Color;
                }

                if ( this.badges == null )
                    this.badges = new string[] { };
            }
        }

        public static string cleanForJSON( string s ) {
            if ( s == null || s.Length == 0 ) {
                return "";
            }

            char c = '\0';
            int i;
            int len = s.Length;
            StringBuilder sb = new StringBuilder( len + 4 );
            String t;

            for ( i = 0; i < len; i += 1 ) {
                c = s[i];
                switch ( c ) {
                    case '\\':
                    case '"':
                        sb.Append( '\\' );
                        sb.Append( c );
                        break;
                    case '/':
                        sb.Append( '\\' );
                        sb.Append( c );
                        break;
                    case '\b':
                        sb.Append( "\\b" );
                        break;
                    case '\t':
                        sb.Append( "\\t" );
                        break;
                    case '\n':
                        sb.Append( "\\n" );
                        break;
                    case '\f':
                        sb.Append( "\\f" );
                        break;
                    case '\r':
                        sb.Append( "\\r" );
                        break;
                    default:
                        if ( c < ' ' ) {
                            t = "000" + String.Format( "{0:X}", c );
                            sb.Append( "\\u" + t.Substring( t.Length - 4 ) );
                        } else {
                            sb.Append( c );
                        }
                        break;
                }
            }
            return sb.ToString();
        }

        public string ToJson( bool allowRatSource ) {
            string s = badgesAsJson();

            //Newtonsoft.Json

            string text = HttpUtility.HtmlEncode( Text.Replace( "\"", "'" ).Replace( "\\", "\\\\" ) );
            if ( !allowRatSource )
                text = text.Replace( "asset://rat/", "" );

            string result = "";

            if ( Source == null ) {
                result = string.Format(
                    "{{ \"label\": \"{0}\", \"name\": \"{1}\", \"text\": \"{2}\", \"tome\": \"{3}\", \"date\": \"{4}\", \"source\": \"{5}\", \"gid\": \"{6}\", \"badges\": [] }}",
                    Id,
                    HttpUtility.HtmlEncode( Name.Replace( "\"", "'" ).Replace( "\\", "\\\\" ) ),
                    text,
                    ToMe,
                    Date.ToUniversalTime(),
                    "system",
                    GID );
            } else {
                result = string.Format(
                    "{{ \"label\": \"{0}\", \"name\": \"{1}\", \"text\": \"{2}\", \"tome\": \"{3}\", \"date\": \"{4}\", \"source\": \"{5}\", \"gid\": \"{6}\", \"badges\": {7}, \"color\": \"{8}\" }}",
                    Id,
                    HttpUtility.HtmlEncode( Name.Replace( "\"", "'" ).Replace( "\\", "\\\\" ) ),
                    text,
                    ToMe,
                    Date.ToUniversalTime(),
                    Source.Id,
                    GID,
                    badgesAsJson(),
                    Color );
            }

            try {
                var x = Newtonsoft.Json.JsonConvert.DeserializeObject( result );
            } catch (Exception err) {
                App.Log( '~', "Сраная ошибка зависания твича, я поймало тебя: {0} = (( {1} ))", err, result );
            }

            return result;
        }
    }
}
