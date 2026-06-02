using System.Globalization;

namespace SportHub.App.Converters;

public class StringNotEmptyToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string s && !string.IsNullOrWhiteSpace(s);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}