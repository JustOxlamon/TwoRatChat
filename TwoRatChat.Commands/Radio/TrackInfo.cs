using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TwoRatChat.Radio {
    public class TrackInfo {
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Uri { get; set; }
        public string IconUri { get; set; }

        public TrackInfo( string Track ) {
            if (Track.Contains(" - ")) {
                string[] s = Track.Split( new string[]{" - "}, StringSplitOptions.RemoveEmptyEntries );
                Name = s[1].Trim();
                Artist = s[0].Trim();
            } else {
                Name = Track;
                Artist = "Unknown artist";
            }
        }

        public TrackInfo(XElement x) {
            if ( x.Attribute( "status" ).Value == "ok" ) {
                XElement track = x.Element( "results" ).Element( "trackmatches" ).Element( "track" );

                Name = track.Element( "name" ).Value;
                Artist = track.Element( "artist" ).Value;
                Uri = track.Element( "url" ).Value;

                foreach ( var img in x.Elements( "image" ) ) {
                    if ( img.Attribute( "size" ).Value == "medium" ) {
                        IconUri = img.Value;
                        break;
                    }
                }
            }
        }
    }
}
