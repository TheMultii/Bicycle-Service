using Microsoft.UI.Xaml.Data;

namespace Klient.Helpers;

public class BoolToLoadingMarginConverter : IValueConverter {

    public object Convert(object value, Type targetType, object parameter, string language) {
        if (value is bool b) {
            if (parameter != null) {
                return b ? "0 3 0 0" : "0 0 0 0";
            }
            return b ? "0 0 0 0" : "0 3 0 0";
        }

        throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) {
        throw new NotImplementedException();
    }
}
