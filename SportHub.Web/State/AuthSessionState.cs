using SportHub.Shared.DTOs.Auth;

namespace SportHub.Web.State;

public class AuthSessionState
{
    public string? AccessToken { get; private set; }
    public StaffUserDto? User { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken) && User is not null;

    public event Action? Changed;

    public void SetSession(string accessToken, StaffUserDto user)
    {
        AccessToken = accessToken;
        User = user;
        Changed?.Invoke();
    }

    public void ClearSession()
    {
        AccessToken = null;
        User = null;
        Changed?.Invoke();
    }
}

