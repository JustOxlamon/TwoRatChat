using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TwoRatChat.Main.Designer {
    public class DesignDocument {
        string ext;

        public bool IsModified { get; set; }
        public string FileName { get; set; }

        public string TemplateFileName { get; set; }

        public List<PropertyEditor> Editors { get; set; }

        public DesignDocument() {
            this.IsModified = true;
            this.FileName = string.Empty;
            this.ext = string.Empty;
            this.Editors = new List<PropertyEditor>();
        }

        public bool CreateNew( string template ) {
            this.IsModified = true;
            this.FileName = "";
            this.TemplateFileName = template;
            return parseTemplate();
        }


        public bool Open( string fileName ) {
            this.IsModified = false;
            this.FileName = fileName;

            Regex r = new Regex( "<!--%SAVEBEGIN%(.*?)%SAVEEND%-->");
            string[] data = r
                .Match( File.ReadAllText( FileName ) )
                .Groups[1]
                .Value
                .Split( new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries );

            if ( data.Length == 0 )
                return false;

            this.TemplateFileName = App.MapFileName( "\\templates\\" + data[0] );
            if ( parseTemplate() ) {
                foreach ( var item in data ) {
                    var kv = item.ToKeyValues( "," );
                    string key = kv.get( "id", "*" );
                    if( key != "*" ) {
                        PropertyEditor pe = Find( key );
                        if( pe != null ) {
                            pe.Load( kv );
                        }
                    }
                }

                return true;
            }

            return false;
          //  this.RawContent = File.ReadAllText( this.FileName );

            //parseContent();
        }

        PropertyEditor Find( string id) {
            foreach ( var e in Editors )
                if ( e.Id == id )
                    return e;
            return null;
        }

        public void Save() {

            string s =
                 System.IO.Path.GetDirectoryName( this.FileName ) + "\\" +
                 System.IO.Path.GetFileNameWithoutExtension( this.FileName ) + this.ext;


            File.WriteAllText( s, applyEditors() );
            this.IsModified = false;
        }

        private string applyEditors() {
            string templateContent = File.ReadAllText( this.TemplateFileName );
            string saveBlock = string.Format( "{0}", Path.GetFileName( this.TemplateFileName ) );

            foreach ( var x in this.Editors ) {
                templateContent = x.Apply( templateContent );
                saveBlock += "|" + x.Save();
            }

            templateContent = templateContent.Replace( "<!--%SAVEBLOCK%-->", "<!--%SAVEBEGIN%" + saveBlock + "|%SAVEEND%-->" );

            return templateContent;
        }

        private bool parseTemplate() {
            // Ищем <!--%SAVEBLOCK%-->
            if ( string.IsNullOrEmpty( this.TemplateFileName ) )
                return false;

            string templateContent = File.ReadAllText( this.TemplateFileName );

            if ( !templateContent.Contains( "<!--%SAVEBLOCK%-->" ) )
                return false; // Неверный шаблон

            if ( templateContent.Contains( "<!--%EXT=HTML%-->" ) )
                this.ext = ".html";
            else
                this.ext = ".htm";

            HashSet<string> ids = new HashSet<string>();
            Regex prop = new Regex( "%PROPSTART(.*?)PROPEND%" );
            foreach( Match m in prop.Matches( templateContent ) ) {
                var kv = m.Groups[1].Value.ToKeyValues( "|" );

                if ( ids.Contains( kv["id"] ) )
                    return false; // id совпадают

                ids.Add( kv["id"] );

                PropertyEditor pe = null;
                switch ( kv["type"] ) {
                    case "color":
                        pe = new ColorPropertyEditor( kv );
                        break;

                    case "enum":
                        pe = new EnumPropertyEditor( kv );
                        break;

                    case "margin":
                        pe = new MarginPropertyEditor( kv );
                        break;

                    case "font":
                        pe = new FontPropertyEditor( kv );
                        break;

                    case "back":
                        pe = new BackgroundPropertyEditor( kv );
                        break;

                    case "int":
                        pe = new IntPropertyEditor( kv );
                        break;
                }

                if ( pe != null ) {
                    pe.Replacer = m.Value;
                    this.Editors.Add( pe );

                }
                
            }

            return true;
        }

    }
}
