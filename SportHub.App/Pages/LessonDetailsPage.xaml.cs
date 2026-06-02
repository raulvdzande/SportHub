using SportHub.App.ViewModels;

namespace SportHub.App.Pages;

[QueryProperty(nameof(LessonId), "id")]
public partial class LessonDetailsPage : ContentPage
{
    private readonly LessonDetailsViewModel _vm;

    public LessonDetailsPage(LessonDetailsViewModel vm)
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
                _ = _vm.LoadAsync(id);
        }
    }
}
