using SportHub.App.Pages;
using SportHub.App.Services;
using SportHub.App.Services.Api;
using SportHub.App.Services.Storage;
using SportHub.App.State;
using SportHub.Shared.DTOs.Lessons;
using SportHub.Shared.DTOs.Reservations;
using System.Text.Json;
using System.Windows.Input;

namespace SportHub.App.ViewModels;

public class LessonDetailsViewModel : ViewModelBase
{
    private readonly ILessonsApiClient _lessons;
    private readonly IReservationsApiClient _reservations;
    private readonly AppSessionState _sessionState;
    private readonly AppLocalStorage _storage;

    public LessonDetailsViewModel(ILessonsApiClient lessons, IReservationsApiClient reservations, AppSessionState sessionState, AppLocalStorage storage)
    {
        _lessons = lessons;
        _reservations = reservations;
        _sessionState = sessionState;
        _storage = storage;

        ReserveCommand = new Command(async () => await ReserveAsync());
        SelectBikeCommand = new Command(async () => await SelectBikeAsync());
        CheckInCommand = new Command(async () => await CheckInAsync());
        RemoveParticipantCommand = new Command<Guid>(async (memberId) => await RemoveParticipantAsync(memberId));
    }

    private MobileLessonDetailsDto? _details;
    public MobileLessonDetailsDto? Details
    {
        get => _details;
        set
        {
            if (SetProperty(ref _details, value))
            {
                OnPropertyChanged(nameof(HasCurrentMemberReserved));
            }
        }
    }

    public bool HasCurrentMemberReserved
    {
        get
        {
            if (_sessionState.CurrentMember?.Id == null)
                return false;
            return Details?.Participants.Any(p => p.MemberId == _sessionState.CurrentMember.Id) ?? false;
        }
    }

    private int? _selectedBikeNumber;
    public int? SelectedBikeNumber { get => _selectedBikeNumber; set => SetProperty(ref _selectedBikeNumber, value); }

    private IReadOnlyCollection<int> _availableBikes = [];
    public IReadOnlyCollection<int> AvailableBikes { get => _availableBikes; set => SetProperty(ref _availableBikes, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set { SetProperty(ref _isBusy, value); ((Command)ReserveCommand).ChangeCanExecute(); ((Command)CheckInCommand).ChangeCanExecute(); } }

    private string _error = string.Empty;
    public string Error { get => _error; set => SetProperty(ref _error, value); }

    private string _statusMessage = string.Empty;
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

    public ICommand ReserveCommand { get; }
    public ICommand SelectBikeCommand { get; }
    public ICommand CheckInCommand { get; }
    public ICommand RemoveParticipantCommand { get; }

    public async Task LoadAsync(Guid id)
    {
        IsBusy = true;
        Details = null;
        Error = string.Empty;
        try
        {
            Details = await _lessons.GetMobileDetailsAsync(id);

            if (Details?.Id != null)
            {
                var takenBikes = await _reservations.GetTakenBikesAsync(Details.Id);
                var allBikes = Enumerable.Range(1, Details.RegisteredCount + 10).ToList();
                AvailableBikes = allBikes.Except(takenBikes ?? []).ToList();
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

    private async Task ReserveAsync()
    {
        if (IsBusy || Details == null) return;
        IsBusy = true;
        Error = string.Empty;
        try
        {
            var request = new CreateReservationRequestDto
            {
                LessonId = Details.Id,
                SpinningBikeNumber = SelectedBikeNumber
            };

            var result = await _reservations.CreateAsync(request);
            if (result != null)
            {
                StatusMessage = result.Status == "Reserved"
                    ? "Successfully reserved!"
                    : "You've been added to the waitlist.";

                PushNotificationService.SendNotification(
                    result.Status == "Reserved" ? "Reservation Confirmed" : "Waitlist",
                    result.Status == "Reserved"
                        ? $"You've reserved {Details.WorkoutName}"
                        : $"You're on the waitlist for {Details.WorkoutName}");

                await Task.Delay(1500);
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                Error = "Could not process reservation.";
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            Error = ex.Message ?? "Cannot reserve this lesson.";
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

    private async Task SelectBikeAsync()
    {
        var options = AvailableBikes.Select(b => b.ToString()).ToList();
        var result = await Shell.Current.DisplayActionSheet("Select bike", "Cancel", null, options.ToArray());
        if (result != null && int.TryParse(result, out var bike))
        {
            SelectedBikeNumber = bike;
        }
    }

    private async Task CheckInAsync()
    {
        if (Details != null)
        {
            await Shell.Current.GoToAsync($"check-in?lessonId={Details.Id}");
        }
    }

    private async Task RemoveParticipantAsync(Guid memberId)
    {
        if (IsBusy || Details == null) return;

        var result = await Shell.Current.DisplayAlert(
            "Remove Participant",
            "Are you sure? The waitlisted member will be notified and can accept or decline the spot.",
            "Yes", "Cancel");

        if (!result) return;

        IsBusy = true;
        Error = string.Empty;
        try
        {
            var notificationId = await _reservations.RemoveParticipantAsync(Details.Id, memberId);
            StatusMessage = "Participant removed. Waitlisted member notified!";

            // Persist the spot offer locally using the real notification id returned by the API
            if (notificationId.HasValue)
            {
                var workoutName = Details?.WorkoutName ?? "Unknown Workout";
                var time = Details?.StartTimeUtc.ToString("HH:mm") ?? "N/A";
                var notificationData = new WaitlistNotificationData
                {
                    NotificationId = notificationId.Value,
                    WorkoutName = workoutName,
                    Time = time
                };
                var json = System.Text.Json.JsonSerializer.Serialize(notificationData);
                await _storage.SetWaitlistNotificationAsync(json);

                PushNotificationService.SendNotification(
                    "Spot Available",
                    "A spot opened! Check notifications to accept or decline.");
            }

            await LoadAsync(Details.Id);
        }
        catch (TaskCanceledException)
        {
            Error = "Timeout — check your network.";
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
