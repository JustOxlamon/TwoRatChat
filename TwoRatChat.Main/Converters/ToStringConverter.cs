using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TwoRatChat.Main.Converters {
    public class ToStringConverter: IValueConverter {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
            if (value == null)
                return string.Empty;
            if (value is float)
                return string.Format("{0:0.00}", value);
            return value.ToString();
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
            throw new NotImplementedException();
        }
    }
}
