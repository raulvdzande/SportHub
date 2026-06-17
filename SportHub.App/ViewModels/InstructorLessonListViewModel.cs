using SportHub.App.Services.Api;
using SportHub.Shared.DTOs.Lessons;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SportHub.App.ViewModels;

public class InstructorLessonListViewModel : ViewModelBase
{
    private readonly ILessonsApiClient _lessonsApi;

    public InstructorLessonListViewModel(ILessonsApiClient lessonsApi)
    {
        _lessonsApi = lessonsApi;

        RefreshCommand = new Command(async () => await LoadAsync());
        LessonTappedCommand = new Command<MobileLessonSummaryDto>(async l => await LessonTappedAsync(l));
        LogoutCommand = new Command(async () => await LogoutAsync());
    }

    private ObservableCollection<MobileLessonSummaryDto> _lessons = [];
    public ObservableCollection<MobileLessonSummaryDto> Lessons { get => _lessons; set => SetProperty(ref _lessons, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set { SetProperty(ref _isBusy, value); ((Command)RefreshCommand).ChangeCanExecute(); } }

    private string _error = string.Empty;
    public string Error { get => _error; set => SetProperty(ref _error, value); }

    private DateTime _currentDate = DateTime.Today;
    public DateTime CurrentDate { get => _currentDate; set => SetProperty(ref _currentDate, value); }

    public ICommand RefreshCommand { get; }
    public ICommand LessonTappedCommand { get; }
    public ICommand LogoutCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        Error = string.Empty;
        try
        {
            var tomorrow = DateTime.UtcNow.AddDays(1);
            var endDate = tomorrow.AddDays(7);
            var lessons = await _lessonsApi.GetMobileScheduleAsync(tomorrow, endDate);
            Lessons = new ObservableCollection<MobileLessonSummaryDto>(lessons ?? []);
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

    private async Task LessonTappedAsync(MobileLessonSummaryDto lesson)
    {
        await Shell.Current.GoToAsync($"instructor-lesson-details?id={lesson.Id}");
    }

    private async Task LogoutAsync()
    {
        var confirmed = await Shell.Current.DisplayAlert("Uitloggen", "Wil je jezelf uitloggen?", "Ja", "Nee");
        if (confirmed)
        {
            ((App)Application.Current).ShowLogin();
        }
    }
}
