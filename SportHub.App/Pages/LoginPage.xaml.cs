using SportHub.App.Services.Auth;
using SportHub.App.State;
using SportHub.App.ViewModels;
using SportHub.Shared.DTOs.Auth;

namespace SportHub.App.Pages;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _vm;

    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }
}
