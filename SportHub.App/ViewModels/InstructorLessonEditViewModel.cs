using SportHub.App.Services.Api;
using SportHub.App.State;
using SportHub.Shared.DTOs.Lessons;
using SportHub.Shared.DTOs.Workouts;
using SportHub.Shared.DTOs.Locations;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SportHub.App.ViewModels;

public class InstructorLessonEditViewModel : ViewModelBase
{
    private readonly ILessonsApiClient _lessonsApi;
    private readonly IWorkoutsApiClient _workoutsApi;
    private readonly ILocationsApiClient _locationsApi;
    private readonly AppSessionState _sessionState;
    private Guid? _editingLessonId;

    public InstructorLessonEditViewModel(ILessonsApiClient lessonsApi, IWorkoutsApiClient workoutsApi, ILocationsApiClient locationsApi, AppSessionState sessionState)
    {
        _lessonsApi = lessonsApi;
        _workoutsApi = workoutsApi;
        _locationsApi = locationsApi;
        _sessionState = sessionState;

        SaveCommand = new Command(async () => await SaveAsync());
        CancelCommand = new Command(async () => await CancelAsync());

        Workouts = new ObservableCollection<WorkoutDto>();
        Locations = new ObservableCollection<LocationDto>();

        LessonDate = DateTime.Today.AddDays(1);
        LessonTime = new TimeSpan(18, 0, 0);
        DurationMinutes = "45";
        Capacity = "20";
    }

    private ObservableCollection<WorkoutDto> _workouts = [];
    public ObservableCollection<WorkoutDto> Workouts { get => _workouts; set => SetProperty(ref _workouts, value); }

    private ObservableCollection<LocationDto> _locations = [];
    public ObservableCollection<LocationDto> Locations { get => _locations; set => SetProperty(ref _locations, value); }

    private WorkoutDto? _selectedWorkout;
    public WorkoutDto? SelectedWorkout { get => _selectedWorkout; set => SetProperty(ref _selectedWorkout, value); }

    private LocationDto? _selectedLocation;
    public LocationDto? SelectedLocation { get => _selectedLocation; set => SetProperty(ref _selectedLocation, value); }

    private DateTime _lessonDate = DateTime.Today;
    public DateTime LessonDate { get => _lessonDate; set => SetProperty(ref _lessonDate, value); }

    private TimeSpan _lessonTime = new TimeSpan(18, 0, 0);
    public TimeSpan LessonTime { get => _lessonTime; set => SetProperty(ref _lessonTime, value); }

    private string _durationMinutes = "45";
    public string DurationMinutes { get => _durationMinutes; set => SetProperty(ref _durationMinutes, value); }

    private string _capacity = "20";
    public string Capacity { get => _capacity; set => SetProperty(ref _capacity, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set { SetProperty(ref _isBusy, value); ((Command)SaveCommand).ChangeCanExecute(); ((Command)CancelCommand).ChangeCanExecute(); } }

    private string _error = string.Empty;
    public string Error { get => _error; set => SetProperty(ref _error, value); }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public async Task LoadAsync(string? id)
    {
        _editingLessonId = null;
        IsBusy = true;
        Error = string.Empty;

        try
        {
            var workoutsTask = _workoutsApi.GetAllAsync();
            var locationsTask = _locationsApi.GetAllAsync();

            await Task.WhenAll(workoutsTask, locationsTask);

            Workouts = new ObservableCollection<WorkoutDto>(workoutsTask.Result);
            Locations = new ObservableCollection<LocationDto>(locationsTask.Result);

            if (string.IsNullOrEmpty(id)) return;

            if (Guid.TryParse(id, out var lessonId))
            {
                _editingLessonId = lessonId;
                var lessonDetails = await _lessonsApi.GetMobileDetailsAsync(lessonId);

                if (lessonDetails != null)
                {
                    SelectedWorkout = Workouts.FirstOrDefault(w => w.Name == lessonDetails.WorkoutName);
                    SelectedLocation = Locations.FirstOrDefault(l => l.Name == lessonDetails.LocationName);
                    var utcStart = lessonDetails.StartTimeUtc.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(lessonDetails.StartTimeUtc, DateTimeKind.Utc)
                        : lessonDetails.StartTimeUtc;
                    var localStart = utcStart.ToLocalTime();
                    LessonDate = localStart.Date;
                    LessonTime = localStart.TimeOfDay;
                    DurationMinutes = lessonDetails.DurationMinutes.ToString();
                    Capacity = (lessonDetails.CapacityOverride ?? lessonDetails.RegisteredCount).ToString();
                }
            }
        }
        catch (Exception ex)
        {
            Error = $"Error loading data: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync()
    {
        if (_sessionState.InstructorId == null)
        {
            Error = "Session error: not logged in as instructor. Please log in again.";
            return;
        }

        if (SelectedWorkout == null || SelectedLocation == null)
        {
            Error = "Please select a workout type and location.";
            return;
        }

        if (!int.TryParse(DurationMinutes, out var duration) || duration <= 0)
        {
            Error = "Please enter a valid duration.";
            return;
        }

        if (!int.TryParse(Capacity, out var capacity) || capacity <= 0)
        {
            Error = "Please enter a valid capacity.";
            return;
        }

        IsBusy = true;
        Error = string.Empty;

        try
        {
            var startTime = DateTime.SpecifyKind(LessonDate.Add(LessonTime), DateTimeKind.Local).ToUniversalTime();

            if (_editingLessonId == null)
            {
                // Create new lesson
                var request = new CreateLessonRequestDto
                {
                    WorkoutId = SelectedWorkout.Id,
                    LocationId = SelectedLocation.Id,
                    InstructorId = _sessionState.InstructorId,
                    StartTimeUtc = startTime,
                    DurationMinutes = duration,
                    CapacityOverride = capacity
                };

                await _lessonsApi.CreateAsync(request);
                await Shell.Current.DisplayAlert("Success", "Lesson created.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                // Update existing lesson
                var request = new UpdateLessonRequestDto
                {
                    WorkoutId = SelectedWorkout.Id,
                    LocationId = SelectedLocation.Id,
                    InstructorId = _sessionState.InstructorId,
                    StartTimeUtc = startTime,
                    DurationMinutes = duration,
                    CapacityOverride = capacity
                };

                await _lessonsApi.UpdateAsync(_editingLessonId.Value, request);
                await Shell.Current.DisplayAlert("Success", "Lesson updated.", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (TaskCanceledException)
        {
            Error = "Time-out — check network.";
        }
        catch (Exception ex)
        {
            Error = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
