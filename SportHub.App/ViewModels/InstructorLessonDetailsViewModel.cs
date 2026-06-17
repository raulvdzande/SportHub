using SportHub.App.Services.Api;
using SportHub.Shared.DTOs.Lessons;
using SportHub.Shared.DTOs.Reservations;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SportHub.App.ViewModels;

public class InstructorLessonDetailsViewModel : ViewModelBase
{
    private readonly ILessonsApiClient _lessonsApi;
    private readonly IReservationsApiClient _reservationsApi;
    private readonly ICheckInsApiClient _checkInsApi;
    private Guid _currentLessonId;

    public InstructorLessonDetailsViewModel(ILessonsApiClient lessonsApi, IReservationsApiClient reservationsApi, ICheckInsApiClient checkInsApi)
    {
        _lessonsApi = lessonsApi;
        _reservationsApi = reservationsApi;
        _checkInsApi = checkInsApi;

        RefreshCommand = new Command(async () => await LoadAsync(_currentLessonId));
        CheckInCommand = new Command(async () => await CheckInAsync());
    }

    private MobileLessonDetailsDto? _lesson;
    public MobileLessonDetailsDto? Lesson { get => _lesson; set => SetProperty(ref _lesson, value); }

    private ObservableCollection<LessonReservationDto> _reservations = [];
    public ObservableCollection<LessonReservationDto> Reservations { get => _reservations; set => SetProperty(ref _reservations, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set { SetProperty(ref _isBusy, value); ((Command)RefreshCommand).ChangeCanExecute(); } }

    private string _error = string.Empty;
    public string Error { get => _error; set => SetProperty(ref _error, value); }

    public ICommand RefreshCommand { get; }
    public ICommand CheckInCommand { get; }

    private async Task CheckInAsync()
    {
        if (Lesson != null)
        {
            await Shell.Current.GoToAsync($"check-in?lessonId={Lesson.Id}");
        }
    }

    public async Task LoadAsync(Guid lessonId)
    {
        if (IsBusy) return;
        _currentLessonId = lessonId;
        IsBusy = true;
        Error = string.Empty;
        try
        {
            var lesson = await _lessonsApi.GetMobileDetailsAsync(lessonId);
            Lesson = lesson;

            var reservations = await _reservationsApi.GetByLessonAsync(lessonId);
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
}
