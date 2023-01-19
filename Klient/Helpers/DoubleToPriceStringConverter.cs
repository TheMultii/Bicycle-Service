using Microsoft.UI.Xaml.Data;
using System.Globalization;

namespace Klient.Helpers;

public class DoubleToPriceStringConverter : IValueConverter {

    public object Convert(object value, Type targetType, object parameter, string language) {
        if (value is double d) {
            CultureInfo pl = new("pl-PL");
            if (parameter != null) return d.ToString("C2", pl).ToString();
            return $"W sumie do zapłaty: {d.ToString("C2", pl)}.";
        }

        throw new ArgumentException("");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) {
        throw new NotImplementedException();
    }
}
