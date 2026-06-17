using System.Globalization;

namespace SportHub.App.Converters;

public class UtcToLocalDateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dt)
        {
            var utc = dt.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dt, DateTimeKind.Utc)
                : dt;
            var local = utc.ToLocalTime();
            var format = parameter as string ?? "dd MMM yyyy";
            return local.ToString(format, culture);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
