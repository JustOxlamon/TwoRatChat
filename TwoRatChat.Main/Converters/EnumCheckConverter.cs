using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TwoRatChat.Main.Converters {
    public class EnumCheckConverter: IValueConverter {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
            string p = parameter.ToString();
            if( p.Length>0 && p[0] == '!' )
                return value.ToString() != p;
            return value.ToString() == p;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
            throw new NotImplementedException();
        }
    }
}
