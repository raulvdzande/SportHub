using SportHub.App.Services.Storage;
using SportHub.App.ViewModels;
using System.Text.Json;

namespace SportHub.App.Pages;

public partial class NotificationsPage : ContentPage
{
    private readonly NotificationsViewModel _vm;
    private readonly AppLocalStorage _storage;

    public NotificationsPage(NotificationsViewModel vm, AppLocalStorage storage)
    {
        InitializeComponent();
        _vm = vm;
        _storage = storage;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Always reload notifications when page appears
        _vm.Notifications.Clear();
        await _vm.LoadAsync();

        // Check for locally stored waitlist notification
        var storedNotification = await _storage.GetWaitlistNotificationAsync();
        if (!string.IsNullOrEmpty(storedNotification))
        {
            try
            {
                var notification = JsonSerializer.Deserialize<WaitlistNotificationData>(storedNotification);
                if (notification != null)
                {
                    await ShowWaitlistPopupAsync(notification);
                }
            }
            catch
            {
                // Clear invalid data
                await _storage.RemoveWaitlistNotificationAsync();
            }
        }
    }

    private async Task ShowWaitlistPopupAsync(WaitlistNotificationData notification)
    {
        var result = await DisplayAlert(
            "Spot Available!",
            $"A spot opened for {notification.WorkoutName} at {notification.Time}.\n\nDo you want to accept?",
            "Accept",
            "Decline");

        if (result)
        {
            var notificationDto = new Shared.DTOs.Notifications.NotificationDto
            {
                Id = notification.NotificationId,
                Type = "WaitlistSpotOpened",
                Title = "Spot Available",
                Message = $"A spot opened for {notification.WorkoutName}"
            };

            await _vm.AcceptWaitlistSpotAsync(notificationDto);
        }

        await _storage.RemoveWaitlistNotificationAsync();
    }
}

public class WaitlistNotificationData
{
    public Guid NotificationId { get; set; }
    public string WorkoutName { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
}
