using SportHub.Shared.DTOs.Members;

namespace SportHub.App.State;

public class AppSessionState
{
    public event Action? Changed;

    public string? AccessToken { get; private set; }
    public MemberDto? CurrentMember { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken);

    public void SetSession(string accessToken, MemberDto? currentMember)
    {
        AccessToken = accessToken;
        CurrentMember = currentMember;
        Changed?.Invoke();
    }

    public void ClearSession()
    {
        AccessToken = null;
        CurrentMember = null;
        Changed?.Invoke();
    }
}

