using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TwoRatChat.Main.Converters {
    [ValueConversion( typeof( int ), typeof(bool), ParameterType = typeof(int))]
    public class IntToBoolConveter : IValueConverter {
       
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
            return (int)value != int.Parse( (string)parameter );
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
            if ( (bool)value )
                return 1;
            return 0;
        }
    }
}
