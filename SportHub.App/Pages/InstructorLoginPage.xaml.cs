using SportHub.App.ViewModels;

namespace SportHub.App.Pages;

public partial class InstructorLoginPage : ContentPage
{
    private readonly InstructorLoginViewModel _vm;

    public InstructorLoginPage(InstructorLoginViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        ((App)Application.Current).ShowLogin();
    }
}
