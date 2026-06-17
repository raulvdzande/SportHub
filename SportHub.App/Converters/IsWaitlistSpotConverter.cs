using System.Globalization;

namespace SportHub.App.Converters;

public class IsWaitlistSpotConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string notificationType)
        {
            return notificationType == "WaitlistSpotOpened";
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
