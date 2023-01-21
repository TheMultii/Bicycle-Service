using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Klient.Helpers {
    public class StringLengthToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if (value is string str) {
                return str.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
