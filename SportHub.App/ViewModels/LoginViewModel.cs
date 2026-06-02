using System.Windows.Input;
using SportHub.App.Services.Auth;
using SportHub.Shared.DTOs.Auth;

namespace SportHub.App.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly IAuthApiService _authService;
    private readonly IServiceProvider _services;

    public LoginViewModel(IAuthApiService authService, IServiceProvider services)
    {
        _authService = authService;
        _services = services;

        LoginCommand = new Command(async () => await LoginAsync());
        NavigateRegisterCommand = new Command(async () => await GoToRegisterAsync());
    }

    public ICommand LoginCommand { get; }
    public ICommand NavigateRegisterCommand { get; }

    private string _email = string.Empty;
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    private string _password = string.Empty;
    public string Password { get => _password; set => SetProperty(ref _password, value); }

    private string _error = string.Empty;
    public string Error { get => _error; set => SetProperty(ref _error, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    private async Task GoToRegisterAsync()
    {
        var page = _services.GetRequiredService<SportHub.App.Pages.RegisterPage>();
        await Application.Current!.Windows[0].Page!.Navigation.PushModalAsync(page);
    }

    private async Task LoginAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        Error = string.Empty;

        try
        {
            var req = new LoginMemberRequestDto
            {
                Email = Email?.Trim() ?? string.Empty,
                Password = Password
            };

            var member = await _authService.LoginAsync(req);

            if (member is null)
            {
                Error = "Inloggegevens onjuist";
                return;
            }

            // App.OnSessionChanged handles switching to the Shell
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