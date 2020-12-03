using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TwoRatChat.Main.Designer {

    public static class DictHelper {
        public static Dictionary<string, string> ToKeyValues(this string text, string separator ) {
            string[] pp = text.Split( new string[] { separator }, StringSplitOptions.RemoveEmptyEntries );
            Dictionary<string, string> x = new Dictionary<string, string>();
            for ( int j = 0; j < pp.Length; ++j ) {
                string[] y = pp[j].Split( '=' );
                if( y.Length == 2)
                    x[y[0]] = y[1];
            }
            return x;
        }


        public static string get(this Dictionary<string, string> kv, string name, string def) {
            string x;
            if ( kv.TryGetValue( name, out x ) )
                return x;
            return def;
        }
    }

    public class PropertyEditor {
        public string Caption;
        public string Tooltip;
        public string Replacer;
        public string Id;
        public string Group;

        public PropertyEditor(Dictionary<string, string> kv) {
            this.Caption = kv.get( "caption", "Нет заголовка" );
            this.Group = kv.get( "group", "Прочие" );
            this.Tooltip = kv.get( "tooltip", this.Caption );
            this.Id = kv["id"];
        }


        public virtual UIElement CreateControl() {
            return null;
        }

        public string Apply( string content) {
            return content.Replace( this.Replacer, this.ToString() );
        }

        public virtual string Save() {
            return string.Empty;
        }

        public virtual void Load(Dictionary<string, string> kv) {

        }
    }

    public class ColorPropertyEditor: PropertyEditor {
        public Color CurrentColor;
        public Color DefaultColor;
        public int Mode;

        public ColorPropertyEditor(Dictionary<string, string> kv) : base( kv ) {
            this.CurrentColor =
            this.DefaultColor =
            (Color)ColorConverter.ConvertFromString( kv.get(  "default", "#ffffffff" ) );

            this.Mode = int.Parse( kv.get( "mode", "0" ) );
        }

        public override UIElement CreateControl() {
            Controls.ColorPropEditor e = new Controls.ColorPropEditor();
            e.Setup( this );
            return e;
        }

        public override string ToString() {
            switch( Mode ) {
                case 0:
                    return string.Format( CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", CurrentColor.R, CurrentColor.G, CurrentColor.B );

                case 1:
                    return string.Format( CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}{3:X2}", CurrentColor.A, CurrentColor.R, CurrentColor.G, CurrentColor.B );

                case 2:
                    return string.Format( CultureInfo.InvariantCulture, "rgba( {0}, {1}, {2}, {3} )", CurrentColor.R, CurrentColor.G, CurrentColor.B, CurrentColor.ScA );
            }

            return "#ffffff";            
        }

        public override string Save() {
            return string.Format( "id={0},value={1}", this.Id, 
                string.Format( "#{0:X2}{1:X2}{2:X2}{3:X2}", CurrentColor.A, CurrentColor.R, CurrentColor.G, CurrentColor.B ) );
        }

        public override void Load(Dictionary<string, string> kv) {
            this.CurrentColor =
                (Color)ColorConverter.ConvertFromString( kv.get( "value", "#ffffffff" ) );
        }

    }

    public class EnumPropertyEditor : PropertyEditor {
        public string[] Variants;
        public string CurrentValue;
        public string DefaultValue;

        public EnumPropertyEditor(Dictionary<string, string> kv): base(kv) {
            this.CurrentValue =
            this.DefaultValue = kv.get(  "default", "" );

            this.Variants = kv.get(  "variants", "" ).Split( '¶' );
        }

        public override string ToString() {
            return CurrentValue;
        }

        public override UIElement CreateControl() {
            Controls.EnumPropEditor e = new Controls.EnumPropEditor();
            e.Setup( this );
            return e;
        }

        public override string Save() {
            return string.Format( "id={0},value={1}", this.Id, this.CurrentValue );
        }

        public override void Load(Dictionary<string, string> kv) {
            this.CurrentValue = kv.get( "value", "" );
        }
    }

    public class MarginPropertyEditor: PropertyEditor {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }

        public MarginPropertyEditor(Dictionary<string, string> kv) : base( kv ) {
            this.Top = int.Parse( kv.get( "top", "32" ) );
            this.Left = int.Parse( kv.get( "left", "32" ) );
            this.Right = int.Parse( kv.get( "right", "32" ) );
            this.Bottom = int.Parse( kv.get( "bottom", "32" ) );
        }

        public override string ToString() {
            return string.Format( "{0}px {1}px {2}px {3}px", 
                this.Top, this.Right, this.Bottom, this.Left );
        }

        public override UIElement CreateControl() {
            Controls.BorderPropEditor e = new Controls.BorderPropEditor();
            e.Setup( this );
            return e;
            //return null;
        }

        public override string Save() {
            return string.Format( "id={0},top={1},left={2},right={3},bottom={4}", this.Id,
                this.Top, this.Left, this.Right, this.Bottom );
        }

        public override void Load(Dictionary<string, string> kv) {
            // this.CurrentValue = kv.get( "value", "" );
            this.Top = int.Parse( kv.get( "top", "32" ) );
            this.Left = int.Parse( kv.get( "left", "32" ) );
            this.Right = int.Parse( kv.get( "right", "32" ) );
            this.Bottom = int.Parse( kv.get( "bottom", "32" ) );
        }
    }

    public class IntPropertyEditor : PropertyEditor {
        public int Current;
        public int Default;
        public int Min, Max;

        public IntPropertyEditor(Dictionary<string, string> kv) : base( kv ) {
            this.Current =
            this.Default = int.Parse( kv.get( "default", "0" ) );

            this.Min = int.Parse( kv.get( "min", "0" ) );
            this.Max = int.Parse( kv.get( "max", "100" ) );
        }

        public override UIElement CreateControl() {
            Controls.IntPropEditor e = new Controls.IntPropEditor();
            e.Setup( this );
            return e;
        }

        public override string ToString() {
            return this.Current.ToString();
        }

        public override string Save() {
            return string.Format( "id={0},value={1}", this.Id, Current );
        }

        public override void Load(Dictionary<string, string> kv) {
            this.Current = int.Parse( kv.get( "value", "0" ) );
        }

    }

    //public class ImagePropertyEditor : VectorPropertyEditor {
    //    public ImagePropertyEditor(Dictionary<string, string> kv) : base( kv ) {
    //    }

    //    public override UIElement CreateControl() {
    //        Controls.BorderPropEditor e = new Controls.BorderPropEditor();
    //        e.Setup( this );
    //        return e;
    //    }

    //    public override string ToString() {
    //        return "";
    //    }
    //}

    public class FontPropertyEditor : PropertyEditor {
        public string FontFamily { get; set; }
        public string DefaultFontFamily { get; set; }
        public int Size { get; set; }

        public FontPropertyEditor(Dictionary<string, string> kv) : base( kv ) {
            this.FontFamily = this.DefaultFontFamily = kv.get( "default", "Arial" );
            this.Size = 10;
        }

        public override UIElement CreateControl() {
            Controls.FontPropEditor e = new Controls.FontPropEditor();
            e.Setup( this );
            return e;
        }

        public override string Save() {
            return string.Format( "id={0},value={1},size={2}", this.Id, FontFamily, this.Size );
        }

        public override void Load(Dictionary<string, string> kv) {
            this.FontFamily = kv.get( "value", "Arial" );
            this.Size = int.Parse( kv.get( "size", "10" ) );
        }

        public override string ToString() {
            if ( !string.IsNullOrEmpty( FontFamily ) )
                return string.Format( "font-family: '{0}'; font-size: {1}pt;", FontFamily, Size );
            return "";
        }
    }

    public class BackgroundPropertyEditor : PropertyEditor {
        public string FileName1 { get; set; }
        public string FileName2 { get; set; }

        public int[] Offset { get; set; }
      //  public int[] Margin { get; set; }

        public BackgroundPropertyEditor(Dictionary<string, string> kv) : base( kv ) {
            Offset = new int[4];
        //    Margin = new int[4];
        }

        public BitmapImage Image1 {
            get {
                if ( string.IsNullOrEmpty( FileName1 ) )
                    return null;
                var x = App.MapFileName( "/img/borders/" + FileName1 );

                if ( !string.IsNullOrEmpty( x ) ) {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = new System.IO.MemoryStream( System.IO.File.ReadAllBytes( x ) );
                    bi.EndInit();

                    return bi;
                }

                return null;
            }
        }


        public BitmapImage Image2 {
            get {
                if ( string.IsNullOrEmpty( FileName2 ) )
                    return null;

                var x = App.MapFileName( "/img/borders/" + FileName2 );

                if ( !string.IsNullOrEmpty( x ) ) {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = new System.IO.MemoryStream( System.IO.File.ReadAllBytes( x ) );
                    bi.EndInit();

                    return bi;
                }

                return null;
            }
        }

        public void SetBackground( string fileName ) {
            if ( string.IsNullOrEmpty( fileName ) ) {
                FileName1 = "";
            } else {
                FileName1 = System.IO.Path.GetFileName( fileName ); 
            }
        }

        public void SetBorder(string fileName) {
            if ( string.IsNullOrEmpty( fileName ) ) {
                FileName2 = "";
            } else {
                FileName2 = System.IO.Path.GetFileName( fileName );
            }
        }

        public override UIElement CreateControl() {
            Controls.BackImagePropEditor e = new Controls.BackImagePropEditor();
            e.Setup( this );
            return e;
        }

        public override string Save() {
            string s = string.Format( "id={0},img1={1},t1={2},t2={3},t3={4},t4={5},img2={6}",
                this.Id, 
                (FileName1??"").Replace("=", "¶" ), 
                Offset[0], 
                Offset[1], 
                Offset[2], 
                Offset[3], 
                (FileName2 ?? "").Replace( "=", "¶" ) );

            return s;
        }

        public override void Load(Dictionary<string, string> kv) {
            this.FileName1 = kv.get( "img1", "" ).Replace( "¶", "=" );
            this.FileName2 = kv.get( "img2", "" ).Replace( "¶", "=" );

            this.Offset[0] = int.Parse( kv.get( "t1", "0" ) );
            this.Offset[1] = int.Parse( kv.get( "t2", "0" ) );
            this.Offset[2] = int.Parse( kv.get( "t3", "0" ) );
            this.Offset[3] = int.Parse( kv.get( "t4", "0" ) );

          
        }

        public override string ToString() {
            string s = "";
            if ( !string.IsNullOrEmpty( FileName1 ) ) {
                s += string.Format( "background:url(/tworat/img/borders/{0});", FileName1 );
            }

            if ( !string.IsNullOrEmpty( FileName2 ) ) {
                /*

-moz-border-image: url(http://www.w3.org/TR/css3-background/border.png) 27 31 27 27 stretch round;
-webkit-border-image: url(http://www.w3.org/TR/css3-background/border.png) 27 31 27 27 stretch round;*/

                s += string.Format( "border-style: solid;border-width: {0}px {1}px {2}px {3}px;-webkit-border-image: url(/tworat/img/borders/{4}) {0} {1} {2} {3} round round;",
                    this.Offset[0],
                    this.Offset[1],
                    this.Offset[2],
                    this.Offset[3],
                    FileName2 );
            }


            return s;
        }
    }
}
