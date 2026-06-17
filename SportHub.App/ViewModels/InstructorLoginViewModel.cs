using SportHub.App.Services.Api;
using SportHub.App.State;
using SportHub.Shared.DTOs.Auth;
using System.Windows.Input;

namespace SportHub.App.ViewModels;

public class InstructorLoginViewModel : ViewModelBase
{
    private readonly IInstructorAuthApiClient _authApi;
    private readonly AppSessionState _sessionState;

    public InstructorLoginViewModel(IInstructorAuthApiClient authApi, AppSessionState sessionState)
    {
        _authApi = authApi;
        _sessionState = sessionState;

        LoginCommand = new Command(async () => await LoginAsync());
    }

    private string _email = string.Empty;
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    private string _password = string.Empty;
    public string Password { get => _password; set => SetProperty(ref _password, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set { SetProperty(ref _isBusy, value); ((Command)LoginCommand).ChangeCanExecute(); } }

    private string _error = string.Empty;
    public string Error { get => _error; set => SetProperty(ref _error, value); }

    public ICommand LoginCommand { get; }

    private async Task LoginAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        Error = string.Empty;
        try
        {
            var request = new LoginInstructorRequestDto { Email = Email, Password = Password };
            var response = await _authApi.LoginAsync(request);

            if (response != null)
            {
                _sessionState.SetInstructorSession(response.AccessToken, response.InstructorId);
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var app = (App)Application.Current!;
                    await app.SetShellAndNavigateToInstructorLessonsAsync();
                });
            }
            else
            {
                Error = "Invalid login credentials.";
            }
        }
        catch (TaskCanceledException)
        {
            Error = "Time-out — check network.";
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
