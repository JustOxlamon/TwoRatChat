//// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
//// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Linq;

//namespace TwoRatChat.Main.Model {
//    public class Viewer {
//        public string Name;
//        public long Exp;
//        public int level;
//    }

//    public class Level {
//        public string Name;
//        public string Icon;
//        public long Exp;
//        public int level;
//    }


//    public class LevelManager {
//        Dictionary<string, List<string>> _awards;
//        Dictionary<string, Viewer> _viewers = new Dictionary<string, Viewer>();
//        Level[] _levels;

//        public LevelManager() {
//            string fileName = App.MapFileName( "/levels.xml" );
//            if ( !string.IsNullOrEmpty( fileName ) ) {
//                XElement x = XElement.Load( fileName );
//                _levels = (from b in x.Elements( "level" )
//                           orderby long.Parse( b.Attribute( "exp" ).Value )
//                           select new Level() {
//                               Exp = long.Parse( b.Attribute( "exp" ).Value ),
//                               Icon = b.Attribute( "icon" ).Value,
//                               Name = b.Attribute( "name" ).Value
//                           }).ToArray();

//                _viewers = new Dictionary<string, Viewer>();

//                for ( int j = 0; j < _levels.Length; ++j )
//                    _levels[j].level = j;
//            } else
//                _levels = new Level[] { new Level() { Exp = 0, Icon = "", level = 0, Name = "" } };

//            fileName = Load();

//        }

//        public string Load() {
//            string fileName = App.MapFileName( "/viewerstats.xml" );
//            if ( !string.IsNullOrEmpty( fileName ) ) {
//                XElement xx = XElement.Load( fileName );

//                foreach ( var i in xx.Elements( "viewer" ) ) {
//                    Viewer v = new Viewer() {
//                        Exp = long.Parse( i.Attribute( "exp" ).Value ),
//                        Name = i.Attribute( "name" ).Value
//                    };
//                    _viewers[v.Name] = v;
//                    updateLevel( v );
//                }
//            }

//            return fileName;
//        }

//        public Viewer GetViewer( string name) {
//            Viewer v;
//            if ( _viewers.TryGetValue( name, out v ) )
//                return v;

//            v = new Viewer() { Exp = 0, Name = name };
//            _viewers[v.Name] = v;

//            return v;
//        }

//        public string GetLevelName(string name) {
//            Viewer v = GetViewer( name );
//            return _levels[v.level].Name;
//        }

//        void updateLevel(Viewer v) {
//            Level l = _levels[0];
//            for ( int j = 1; j < _levels.Length; ++j )
//                if ( v.Exp >= _levels[j].Exp )
//                    l = _levels[j];
//                else
//                    break;
//            v.level = l.level;
//        }

//        public void Save() {
//            string file = App.DataLocalFolder + "/viewerstats.xml";
//            XElement x = new XElement( "viewers" );
//            foreach ( var kv in _viewers )
//                x.Add( new XElement( "viewer", new XAttribute( "name", kv.Key ), new XAttribute( "exp", kv.Value.Exp ) ) );
//            x.Save( file );
//        }

//        public bool OnMessage(TwoRatChat.Model.ChatMessage message) {
//            var v = GetViewer( message.Name );

//            int oldLevel = v.level;
//            v.Exp++;
//            updateLevel( v );

//            message.Level = v.level;
//            message.Exp = v.Exp;

//            if ( !string.IsNullOrEmpty( _levels[v.level].Icon ) ) {
//                message.AddBadge( _levels[v.level].Icon );
//            }

//            if ( oldLevel != v.level )
//                return true;
//            return false;
//        }
//    }

//    //public class AwardManager {
//    //    Dictionary<string, List<string>> _awards;

//    //    public AwardManager() {

//    //    }

//    //    public void Load() {
//    //        _awards = new Dictionary<string, List<string>>();
//    //    }
//    //}
//}
