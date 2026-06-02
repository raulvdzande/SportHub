using System.Windows.Input;
using SportHub.Shared.DTOs.Members;
using SportHub.App.Services.Auth;

namespace SportHub.App.ViewModels;

public class RegisterViewModel : ViewModelBase
{
    private readonly IAuthApiService _memberService;

    public RegisterViewModel(IAuthApiService memberService)
    {
        _memberService = memberService;
        RegisterCommand = new Command(async () => await RegisterAsync());
    }

    public ICommand RegisterCommand { get; }

    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    private string _error = string.Empty;
    public string Error { get => _error; set => SetProperty(ref _error, value); }

    private async Task RegisterAsync()
    {
        try
        {
            var req = new CreateMemberRequestDto
            {
                Email = Email,
                FirstName = FirstName,
                LastName = LastName,
                Password = Password
            };

            await _memberService.CreateAccountAsync(req);

            await Application.Current!.Windows[0].Page!.Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
}