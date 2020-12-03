using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TwoRatChat.Model;

namespace TwoRatChat.Main.Sources {
    public class Source {
        public int Id { get; set; }
        public string Title { get; set; }
        public Uri Icon { get; set; }
        public Type Type;
        public string SourceId { get; set; }
        public string Param1 { get; set; }
        public string Param2 { get; set; }

        public Source() {
            this.Id = -1;
            this.Title = "All sources";
        }

        public override string ToString() {
            return Title;
        }
    }

    public static class SourceManager {
        public static Source All { get; private set; }

        public static List<Source> Sources { get; private set; }

        static SourceManager() {
            Sources = new List<Source>();
            All = new Source();

            Sources.Add( new Source() {
                Id = Sources.Count + 1,
                Title = "http://goodgame.ru/",
                Type = typeof( Goodgame_ChatSource ),
                Icon = new Uri( "/Assets/goodgame.png", UriKind.Relative ),
                Param1 = Main.Properties.Resources.MES_Streamer,
                Param2 = Main.Properties.Resources.MES_Label,
                SourceId = "goodgame"
            } );

            Sources.Add( new Source() {
                Id = Sources.Count + 1,
                Title = "http://twitch.tv/",
                Type = typeof( Twitch_ChatSource ),
                Icon = new Uri( "/Assets/twitchtv.png", UriKind.Relative ),
                Param1 = Main.Properties.Resources.MES_Streamer,
                Param2 = Main.Properties.Resources.MES_Label,
                SourceId = "twitchtv"
            } );

            Sources.Add( new Source() {
                Id = Sources.Count + 1,
                Title = "http://cybergame.tv/",
                Type = typeof( Cybergame_ChatSource ),
                Icon = new Uri( "/Assets/cybergame.png", UriKind.Relative ),
                Param1 = Main.Properties.Resources.MES_Streamer,
                Param2 = Main.Properties.Resources.MES_Label,
                SourceId = "cybergame"
            } );

            Sources.Add( new Source() {
                Id = Sources.Count + 1,
                Title = "http://sc2tv.ru/",
                Type = typeof( Sc2Tv_ChatSource ),
                Icon = new Uri( "/Assets/sc2tv.png", UriKind.Relative ),
                Param1 = Main.Properties.Resources.MES_Streamer,
                Param2 = Main.Properties.Resources.MES_Label,
                SourceId = "sc2tv"
            } );

            Sources.Add(new Source()
            {
                Id = Sources.Count + 1,
                Title = "http://www.empiretv.org/ - UNTESTED!",
                Type = typeof(EmpireTV_ChatSource),
                Icon = new Uri("/Assets/empire.png", UriKind.Relative),
                Param1 = Main.Properties.Resources.MES_Streamer,
                Param2 = Main.Properties.Resources.MES_Label,
                SourceId = "empire"
            } );

            Sources.Add(new Source()
            {
                Id = Sources.Count + 1,
                Title = "http://www.goha.tv/ - UNTESTED!",
                Type = typeof(GohaTV_ChatSource),
                Icon = new Uri("/Assets/gohatv.png", UriKind.Relative),
                Param1 = Main.Properties.Resources.MES_Streamer,
                Param2 = Main.Properties.Resources.MES_Label,
                SourceId = "gohatv"
            } );

            Sources.Add(new Source()
            {
                Id = Sources.Count + 1,
                Title = "http://midlane.ru/",
                Type = typeof(Midlane_Source),
                Icon = new Uri("/Assets/midlane.png", UriKind.Relative),
                Param1 = Main.Properties.Resources.MES_StreamerOrID,
                Param2 = Main.Properties.Resources.MES_Label,
                SourceId = "midlane"
            } );

            Sources.Add( new Source() {
                Id = Sources.Count + 1,
                Title = "http://gipsyteam.ru/",
                Type = typeof( Gipsyteam_Source ),
                Icon = new Uri( "/Assets/gipsyteam.png", UriKind.Relative ),
                Param1 = Main.Properties.Resources.MES_Streamer,
                Param2 = Main.Properties.Resources.MES_Label,
                SourceId = "gipsyteam"
            } );

            Sources.Add( new Source() {
                Id = Sources.Count + 1,
                Title = "http://youtube.com/",
                Type = typeof( Youtube_ChatSource ),
                Icon = new Uri( "/Assets/youtube.png", UriKind.Relative ),
                Param1 = Main.Properties.Resources.MES_Streamer,
                Param2 = Main.Properties.Resources.MES_Label,
                SourceId = "youtube"
            } );

            Sources.Add( new Source() {
                Id = Sources.Count + 1,
                Title = "http://gaming.youtube.com/",
                Type = typeof( YoutubeGaming_ChatSource ),
                Icon = new Uri( "/Assets/youtube.png", UriKind.Relative ),
                Param1 = Main.Properties.Resources.MES_Streamer,
                Param2 = Main.Properties.Resources.MES_Label,
                SourceId = "youtube"
            } );


            Sources.Add( new Source() {
                Id = Sources.Count + 1,
                Title = "http://www.hitbox.tv/",
                Type = typeof( HitboxTV_ChatSource ),
                Icon = new Uri( "/Assets/hitboxtv.png", UriKind.Relative ),
                Param1 = Main.Properties.Resources.MES_Id,
                Param2 = Main.Properties.Resources.MES_Label,
                SourceId = "hitboxtv"
            } );

            Sources.Add( new Source() {
                Id = Sources.Count + 1,
                Title = "http://gamerstv.ru/ (BETA)",
                Type = typeof( GamersTV_Source ),
                Icon = new Uri( "/Assets/gamerstv.png", UriKind.Relative ),
                Param1 = Main.Properties.Resources.MES_StreamerOrID,
                Param2 = Main.Properties.Resources.MES_Label,
                SourceId = "gamerstv"
            } );

            Sources.Add( new Source() {
                Id = Sources.Count + 1,
                Title = "http://streamcube.tv",
                Type = typeof( StreamCube_ChatSource ),
                Icon = new Uri( "/Assets/streamcube.png", UriKind.Relative ),
                Param1 = Main.Properties.Resources.MES_ChannelOrID,
                Param2 = Main.Properties.Resources.MES_Label,
                SourceId = "streamcube"
            } );

            Sources.Add( new Source() {
                Id = Sources.Count + 1,
                Title = "http://peka2.tv",
                Type = typeof( Peka2tv_ChatSource ),
                Icon = new Uri( "/Assets/sc2tv.png", UriKind.Relative ),
                Param1 = Main.Properties.Resources.MES_Streamer,
                Param2 = Main.Properties.Resources.MES_Label,
                SourceId = "sc2tv"
            } );




            //Sources.Add( new Source() {
            //    Id = Sources.Count + 1,
            //    Title = "http://youtube.com/ (HACKED, MEMORY GREED!!!)",
            //    Type = typeof( YoutubeHack_ChatSource ),
            //    Icon = new Uri( "/Assets/youtube.png", UriKind.Relative ),
            //    Param1 = Main.Properties.Resources.MES_Id,
            //    Param2 = Main.Properties.Resources.MES_Label
            //} );
        }

 
    }
}
