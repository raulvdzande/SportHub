using SportHub.App.ViewModels;

namespace SportHub.App.Pages;

public partial class InstructorLessonListPage : ContentPage
{
    private readonly InstructorLessonListViewModel _vm;

    public InstructorLessonListPage(InstructorLessonListViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
