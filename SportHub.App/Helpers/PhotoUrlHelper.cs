namespace SportHub.App.Helpers;

public static class PhotoUrlHelper
{
    private static string ApiBase => DeviceInfo.Platform == DevicePlatform.Android
        ? "http://10.0.2.2:5099"
        : "https://localhost:7275";

    public static string? ToAbsolute(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return url;
        return ApiBase + (url.StartsWith('/') ? url : "/" + url);
    }

    public static ImageSource? ToImageSource(string? url)
    {
        var absolute = ToAbsolute(url);
        return absolute is null ? null : ImageSource.FromUri(new Uri(absolute));
    }
}
