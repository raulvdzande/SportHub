using SportHub.App.ViewModels;

namespace SportHub.App.Pages;

[QueryProperty(nameof(LessonId), "lessonId")]
public partial class CheckInPage : ContentPage
{
    private readonly CheckInViewModel _vm;

    public CheckInPage(CheckInViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    public string? LessonId
    {
        set
        {
            if (value is not null && Guid.TryParse(value, out var id))
                _ = _vm.LoadLessonAsync(id);
        }
    }
}
