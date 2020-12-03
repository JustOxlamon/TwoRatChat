using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace TwoRatChat.Main.Converters {
    public class FlagValueConverter : IValueConverter {
        private int targetValue;

        public FlagValueConverter() {
        }

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
            int mask = (int)parameter;
            this.targetValue = (int)value;
            return ((mask & this.targetValue) != 0);
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
            this.targetValue ^= (int)parameter;
            return Enum.Parse(targetType, this.targetValue.ToString());
        }
    }
}
