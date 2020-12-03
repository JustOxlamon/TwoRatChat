using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TwoRatChat.Main.Converters {
    public class BoolToThicknesConverter: IValueConverter {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
            if ((bool)value)
                return new Thickness( 1.0 );
            return new Thickness();
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
            throw new NotImplementedException();
        }
    }
}
