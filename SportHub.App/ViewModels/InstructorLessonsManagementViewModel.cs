using SportHub.App.Services.Api;
using SportHub.App.State;
using SportHub.Shared.DTOs.Lessons;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SportHub.App.ViewModels;

public class InstructorLessonsManagementViewModel : ViewModelBase
{
    private readonly ILessonsApiClient _lessonsApi;
    private readonly AppSessionState _sessionState;

    public InstructorLessonsManagementViewModel(ILessonsApiClient lessonsApi, AppSessionState sessionState)
    {
        _lessonsApi = lessonsApi;
        _sessionState = sessionState;

        RefreshCommand = new Command(async () => await LoadAsync());
        CreateCommand = new Command(async () => await CreateAsync());
        EditCommand = new Command<MobileLessonSummaryDto>(async l => await EditAsync(l));
        DeleteCommand = new Command<MobileLessonSummaryDto>(async l => await DeleteAsync(l));
    }

    private ObservableCollection<MobileLessonSummaryDto> _lessons = [];
    public ObservableCollection<MobileLessonSummaryDto> Lessons { get => _lessons; set => SetProperty(ref _lessons, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set { SetProperty(ref _isBusy, value); ((Command)RefreshCommand).ChangeCanExecute(); } }

    private string _error = string.Empty;
    public string Error { get => _error; set => SetProperty(ref _error, value); }

    public ICommand RefreshCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public async Task LoadAsync()
    {
        if (_sessionState.InstructorId == null)
        {
            Error = "Not logged in as instructor. Please log in again.";
            return;
        }

        if (IsBusy) return;
        IsBusy = true;
        Error = string.Empty;
        try
        {
            var startDate = DateTime.UtcNow.AddDays(-90);
            var endDate = DateTime.UtcNow.AddDays(365);
            var lessons = await _lessonsApi.GetMobileScheduleAsync(startDate, endDate, _sessionState.InstructorId);
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

    private async Task CreateAsync()
    {
        await Shell.Current.GoToAsync("instructor-lesson-edit");
    }

    private async Task EditAsync(MobileLessonSummaryDto lesson)
    {
        await Shell.Current.GoToAsync($"instructor-lesson-edit?id={lesson.Id}");
    }

    private async Task DeleteAsync(MobileLessonSummaryDto lesson)
    {
        var confirmed = await Shell.Current.DisplayAlert(
            "Delete Lesson",
            $"Are you sure you want to delete '{lesson.WorkoutName}'? This cannot be undone.",
            "Yes", "No");

        if (!confirmed) return;

        IsBusy = true;
        Error = string.Empty;

        try
        {
            await _lessonsApi.DeleteAsync(lesson.Id);
            Lessons.Remove(lesson);
            await Shell.Current.DisplayAlert("Success", "Lesson deleted.", "OK");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Error = "Lesson no longer exists.";
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            Error = "Cannot delete lesson - registrations already exist.";
        }
        catch (TaskCanceledException)
        {
            Error = "Time-out — check network.";
        }
        catch (Exception ex)
        {
            Error = $"Error deleting lesson: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
