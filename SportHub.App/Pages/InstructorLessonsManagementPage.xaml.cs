using SportHub.App.ViewModels;

namespace SportHub.App.Pages;

public partial class InstructorLessonsManagementPage : ContentPage
{
    private readonly InstructorLessonsManagementViewModel _vm;

    public InstructorLessonsManagementPage(InstructorLessonsManagementViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await _vm.LoadAsync();
    }
}
