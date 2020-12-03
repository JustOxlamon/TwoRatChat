using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace TwoRatChat.Main.Converters {
    public class ColorToUIntConverter: IValueConverter {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
            uint col = (uint)value;

            return FromUInt( col );
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
            Color c = (Color)value;

            return FromColor( c );
        }

        public static uint FromColor( Color col ) {
            return (((uint)col.A) << 24) + (((uint)col.R) << 16) + (((uint)col.G) << 8) + ((uint)col.B);
        }

        public static Color FromUInt( uint col ) {
            return Color.FromArgb( (byte)((col >> 24) & 0xFF),
                   (byte)((col >> 16) & 0xFF),
                   (byte)((col >> 8) & 0xFF),
                   (byte)(col & 0xFF) );
        }
    }
}
