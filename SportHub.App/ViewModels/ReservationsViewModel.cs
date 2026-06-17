using SportHub.App.Services;
using SportHub.App.Services.Api;
using SportHub.App.State;
using SportHub.Shared.DTOs.Reservations;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SportHub.App.ViewModels;

public class ReservationsViewModel : ViewModelBase
{
    private readonly IReservationsApiClient _reservationsApi;
    private readonly ILessonsApiClient _lessonsApi;
    private readonly AppSessionState _sessionState;

    public ReservationsViewModel(IReservationsApiClient reservationsApi, ILessonsApiClient lessonsApi, AppSessionState sessionState)
    {
        _reservationsApi = reservationsApi;
        _lessonsApi = lessonsApi;
        _sessionState = sessionState;

        RefreshCommand = new Command(async () => await LoadAsync());
        CancelCommand = new Command<LessonReservationDto>(async r => await CancelAsync(r));
        ReserveCommand = new Command(async () => await ReserveAsync());
    }

    private ObservableCollection<LessonReservationDto> _reservations = [];
    public ObservableCollection<LessonReservationDto> Reservations { get => _reservations; set => SetProperty(ref _reservations, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set { SetProperty(ref _isBusy, value); ((Command)RefreshCommand).ChangeCanExecute(); ((Command)CancelCommand).ChangeCanExecute(); } }

    private string _error = string.Empty;
    public string Error { get => _error; set => SetProperty(ref _error, value); }

    private string _statusMessage = string.Empty;
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

    public ICommand RefreshCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ReserveCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        Error = string.Empty;
        try
        {
            var reservations = await _reservationsApi.GetMyReservationsAsync();
            Reservations = new ObservableCollection<LessonReservationDto>(reservations ?? []);
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

    private async Task CancelAsync(LessonReservationDto reservation)
    {
        if (IsBusy) return;

        var confirmed = await Shell.Current.DisplayAlert("Cancel Reservation", $"Are you sure you want to cancel this lesson?", "Yes", "No");
        if (!confirmed) return;

        IsBusy = true;
        Error = string.Empty;
        try
        {
            var result = await _reservationsApi.CancelAsync(reservation.Id);
            if (result != null)
            {
                StatusMessage = "Successfully cancelled!";
                PushNotificationService.SendNotification("Cancellation Confirmed", $"You've cancelled {reservation.WorkoutName}");
                await LoadAsync();
            }
            else
            {
                Error = "Could not process cancellation.";
            }
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

    private async Task ReserveAsync()
    {
        Error = string.Empty;
        try
        {
            await Shell.Current.GoToAsync("//schedule");
        }
        catch (Exception ex)
        {
            Error = $"Navigation error: {ex.Message}";
        }
    }
}
