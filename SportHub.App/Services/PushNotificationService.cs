namespace SportHub.App.Services;

/// <summary>
/// Service to send real push notifications to the device
/// Shows native device notifications (not in-app)
/// </summary>
public class PushNotificationService
{
    /// <summary>
    /// Send a real device notification that appears in notification center
    /// </summary>
    public static void SendNotification(string title, string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
#if ANDROID
                SendAndroidNotification(title, message);
#elif IOS
                SendiOSNotification(title, message);
#elif WINDOWS
                SendWindowsNotification(title, message);
#else
                if (Application.Current?.MainPage != null)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert(title, message, "OK");
                    });
                }
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending notification: {ex.Message}");
            }
        });
    }

#if ANDROID
    private static void SendAndroidNotification(string title, string message)
    {
        try
        {
            var context = Platform.AppContext;
            var channelId = "sporthub_notifications";
            var notificationManager = context.GetSystemService(Android.Content.Context.NotificationService) as Android.App.NotificationManager;

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                var channel = new Android.App.NotificationChannel(channelId, "SportHub", Android.App.NotificationImportance.High);
                notificationManager?.CreateNotificationChannel(channel);
            }

            var builder = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId)
                .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetAutoCancel(true)
                .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityHigh);

            notificationManager?.Notify(1, builder.Build());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Notification error: {ex.Message}");
        }
    }

    private static int GetAppIconResourceId(Android.Content.Context context)
    {
        try
        {
            var packageName = context.PackageName;
            var resources = context.Resources;
            string[] iconNames = { "appicon", "icon", "notification_icon", "ic_launcher", "ic_launcher_foreground" };

            foreach (var iconName in iconNames)
            {
                var resourceId = resources?.GetIdentifier(iconName, "drawable", packageName) ?? 0;
                if (resourceId != 0)
                    return resourceId;
            }

            var appInfo = context.ApplicationInfo;
            if (appInfo != null && appInfo.Icon != 0)
                return appInfo.Icon;

            return 0;
        }
        catch
        {
            return 0;
        }
    }
#endif

#if IOS
    private static void SendiOSNotification(string title, string message)
    {
        try
        {
            var content = new UserNotifications.UNMutableNotificationContent
            {
                Title = title,
                Body = message,
                Sound = UserNotifications.UNNotificationSound.Default,
                Badge = Foundation.NSNumber.FromInt32(1)
            };

            var trigger = UserNotifications.UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);
            var request = UserNotifications.UNNotificationRequest.FromIdentifier(
                System.Guid.NewGuid().ToString(),
                content,
                trigger
            );

            UserNotifications.UNUserNotificationCenter.Current.RequestAuthorization(
                UserNotifications.UNAuthorizationOptions.Alert |
                UserNotifications.UNAuthorizationOptions.Sound |
                UserNotifications.UNAuthorizationOptions.Badge,
                (approved, error) =>
                {
                    if (approved)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            UserNotifications.UNUserNotificationCenter.Current
                                .AddNotificationRequest(request, null);
                        });
                    }
                }
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"iOS notification error: {ex.Message}");
        }
    }
#endif

#if WINDOWS
    private static void SendWindowsNotification(string title, string message)
    {
        try
        {
            // For Windows, use MAUI's built-in notification or display in app
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
                    // Display as a toast-like notification using DisplayAlert
                    await Application.Current.MainPage.DisplayAlert(title, message, "OK");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Windows notification error: {ex.Message}");
        }
    }
#endif
}
