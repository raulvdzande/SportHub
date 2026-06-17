using SportHub.App.ViewModels;

namespace SportHub.App.Pages;

[QueryProperty(nameof(LessonId), "id")]
public partial class InstructorLessonEditPage : ContentPage
{
    private readonly InstructorLessonEditViewModel _vm;
    private string? _lessonId;

    public InstructorLessonEditPage(InstructorLessonEditViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    public string? LessonId
    {
        get => _lessonId;
        set
        {
            _lessonId = value;
            Title = string.IsNullOrEmpty(_lessonId) ? "Create Lesson" : "Edit Lesson";
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync(_lessonId);
    }
}
