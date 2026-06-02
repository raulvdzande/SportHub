using System.Globalization;
using SportHub.App.Helpers;

namespace SportHub.App.Converters;

public class AbsolutePhotoUrlConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => PhotoUrlHelper.ToImageSource(value as string);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
