using SportHub.App.Services.Api;
using SportHub.Shared.DTOs.Lessons;

namespace SportHub.App.ViewModels;

public class LessonDetailsViewModel : ViewModelBase
{
    private readonly ILessonsApiClient _lessons;

    public LessonDetailsViewModel(ILessonsApiClient lessons)
    {
        _lessons = lessons;
    }

    private MobileLessonDetailsDto? _details;
    public MobileLessonDetailsDto? Details { get => _details; set => SetProperty(ref _details, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    private string _error = string.Empty;
    public string Error { get => _error; set => SetProperty(ref _error, value); }

    public async Task LoadAsync(Guid id)
    {
        IsBusy = true;
        Details = null;
        Error = string.Empty;
        try
        {
            Details = await _lessons.GetMobileDetailsAsync(id);
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
