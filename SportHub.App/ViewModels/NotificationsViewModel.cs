using SportHub.App.Services.Api;
using SportHub.Shared.DTOs.Notifications;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SportHub.App.ViewModels;

public class NotificationsViewModel : ViewModelBase
{
    private readonly INotificationsApiClient _notificationsApi;

    public NotificationsViewModel(INotificationsApiClient notificationsApi)
    {
        _notificationsApi = notificationsApi;

        RefreshCommand = new Command(async () => await LoadAsync());
        MarkAllReadCommand = new Command(async () => await MarkAllReadAsync());
        MarkReadCommand = new Command<NotificationDto>(async n => await MarkReadAsync(n));
        AcceptWaitlistSpotCommand = new Command<NotificationDto>(async n => await AcceptWaitlistSpotAsync(n));
        DeclineWaitlistSpotCommand = new Command<NotificationDto>(async n => await DeclineWaitlistSpotAsync(n));
    }

    private ObservableCollection<NotificationDto> _notifications = [];
    public ObservableCollection<NotificationDto> Notifications { get => _notifications; set => SetProperty(ref _notifications, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set { SetProperty(ref _isBusy, value); ((Command)RefreshCommand).ChangeCanExecute(); } }

    private string _error = string.Empty;
    public string Error { get => _error; set => SetProperty(ref _error, value); }

    private int _unreadCount;
    public int UnreadCount { get => _unreadCount; set => SetProperty(ref _unreadCount, value); }

    public ICommand RefreshCommand { get; }
    public ICommand MarkAllReadCommand { get; }
    public ICommand MarkReadCommand { get; }
    public ICommand AcceptWaitlistSpotCommand { get; }
    public ICommand DeclineWaitlistSpotCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        Error = string.Empty;
        try
        {
            var notifications = await _notificationsApi.GetMyNotificationsAsync();
            Notifications = new ObservableCollection<NotificationDto>(notifications ?? []);
            UnreadCount = Notifications.Count(n => !n.IsRead);
        }
        catch (TaskCanceledException)
        {
            Error = "Time-out — check network.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task MarkAllReadAsync()
    {
        IsBusy = true;
        try
        {
            var success = await _notificationsApi.MarkAllReadAsync();
            if (success)
            {
                await LoadAsync();
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task MarkReadAsync(NotificationDto notification)
    {
        IsBusy = true;
        try
        {
            var success = await _notificationsApi.MarkReadAsync(notification.Id);
            if (success)
            {
                await LoadAsync();
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task AcceptWaitlistSpotAsync(NotificationDto notification)
    {
        IsBusy = true;
        try
        {
            var success = await _notificationsApi.AcceptWaitlistSpotAsync(notification.Id);
            if (success)
            {
                await LoadAsync();
                await Shell.Current.DisplayAlert("Success", "You accepted the spot!", "OK");
            }
            else
            {
                Error = "Failed to accept waitlist spot. Please try again.";
                await Shell.Current.DisplayAlert("Error", Error, "OK");
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            await Shell.Current.DisplayAlert("Error", Error, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeclineWaitlistSpotAsync(NotificationDto notification)
    {
        IsBusy = true;
        try
        {
            var success = await _notificationsApi.DeclineWaitlistSpotAsync(notification.Id);
            if (success)
            {
                await LoadAsync();
                await Shell.Current.DisplayAlert("Noted", "You declined the spot.", "OK");
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
