using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Klient.Helpers;

public class BoolToVisibilityConverter : IValueConverter {

    public object Convert(object value, Type targetType, object parameter, string language) {
        if (value is bool b) {
            if (parameter != null) {
                return b ? Visibility.Collapsed : Visibility.Visible;
            }
            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        throw new ArgumentException("");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) {
        throw new NotImplementedException();
    }
}
