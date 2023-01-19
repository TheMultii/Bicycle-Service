using IdGen;
using Microsoft.UI.Xaml.Data;

namespace Klient.Helpers;

public class UIDToDateTimeConverter : IValueConverter {

    private static readonly DateTime epoch = new(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly IdStructure structure = new(41, 10, 12);
    private static readonly IdGeneratorOptions options = new(structure, new DefaultTimeSource(epoch));
    private static readonly IdGenerator generator = new(0, options);

    public object Convert(object value, Type targetType, object parameter, string language) {
        if (value is long uid) {
            Id id = generator.FromId(uid);
            return id.DateTimeOffset.ToLocalTime().DateTime;
        }

        throw new ArgumentException("");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) {
        throw new NotImplementedException();
    }
}
