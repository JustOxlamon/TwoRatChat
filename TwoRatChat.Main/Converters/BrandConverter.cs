using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace TwoRatChat.Main.Converters {
    public class BrandConverter: IValueConverter {
        static Dictionary<string, BitmapImage> _icons = new Dictionary<string, BitmapImage>();

        static BitmapImage getIcon( string id ) {
            BitmapImage bi;
            if (_icons.TryGetValue(id, out bi))
                return bi;

            bi = new BitmapImage();
            bi.BeginInit();
            var content = Application.GetResourceStream(new Uri("/Assets/" + id + ".png", UriKind.Relative));
            bi.StreamSource = content.Stream;
            bi.EndInit();
            bi.Freeze();
            _icons[id] = bi;
            return bi;
        }

        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
            return getIcon(value as string);
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
            throw new NotImplementedException();
        }
    }
}
