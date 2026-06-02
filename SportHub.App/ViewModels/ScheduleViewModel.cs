using System.Globalization;
using System.Windows.Input;
using SportHub.App.Services.Api;
using SportHub.Shared.DTOs.Lessons;

namespace SportHub.App.ViewModels;

public class ScheduleViewModel : ViewModelBase
{
    private static readonly CultureInfo NlNl = new("nl-NL");

    private readonly ILessonsApiClient _lessons;

    public ScheduleViewModel(ILessonsApiClient lessons)
    {
        _lessons = lessons;
        _currentWeekStart = GetMondayOfCurrentWeek();

        NextWeekCommand     = new Command(async () => { NextWeek();     await LoadAsync(); });
        PreviousWeekCommand = new Command(async () => { PreviousWeek(); await LoadAsync(); });
        LessonTappedCommand = new Command<MobileLessonSummaryDto>(async lesson =>
        {
            if (lesson is null) return;
            try
            {
                await Shell.Current.GoToAsync($"lesson-details?id={lesson.Id}");
            }
            catch (Exception ex)
            {
                Error = $"Navigatiefout: {ex.Message}";
            }
        });
    }

    private DateTime _currentWeekStart;
    public DateTime CurrentWeekStart
    {
        get => _currentWeekStart;
        set
        {
            SetProperty(ref _currentWeekStart, value);
            OnPropertyChanged(nameof(WeekLabel));
        }
    }

    public string WeekLabel =>
        $"{CurrentWeekStart:dd/MM} – {CurrentWeekStart.AddDays(6):dd/MM/yyyy}";

    private IReadOnlyCollection<ScheduleDayGroup> _groupedLessons = Array.Empty<ScheduleDayGroup>();
    public IReadOnlyCollection<ScheduleDayGroup> GroupedLessons
    {
        get => _groupedLessons;
        set => SetProperty(ref _groupedLessons, value);
    }

    private string _error = string.Empty;
    public string Error { get => _error; set => SetProperty(ref _error, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    public ICommand NextWeekCommand     { get; }
    public ICommand PreviousWeekCommand { get; }
    public ICommand LessonTappedCommand { get; }

    public async Task LoadAsync()
    {
        IsBusy = true;
        Error  = string.Empty;
        try
        {
            var to      = CurrentWeekStart.AddDays(7);
            var lessons = await _lessons.GetMobileScheduleAsync(
                              CurrentWeekStart.ToUniversalTime(),
                              to.ToUniversalTime());

            GroupedLessons = lessons
                .GroupBy(l => l.StartTimeUtc.ToLocalTime().Date)
                .OrderBy(g => g.Key)
                .Select(g => new ScheduleDayGroup(
                    g.Key.ToString("dddd d MMMM", NlNl),
                    g.OrderBy(l => l.StartTimeUtc).ToList()))
                .ToList();
        }
        catch (TaskCanceledException)
        {
            Error = "Time-out — controleer netwerk of API-server.";
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

    public void NextWeek()     => CurrentWeekStart = CurrentWeekStart.AddDays(7);
    public void PreviousWeek() => CurrentWeekStart = CurrentWeekStart.AddDays(-7);

    private static DateTime GetMondayOfCurrentWeek()
    {
        var today = DateTime.Now;
        var diff  = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        return today.AddDays(-diff).Date;
    }
}
