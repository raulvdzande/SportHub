using SportHub.App.ViewModels;

namespace SportHub.App.Pages;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}