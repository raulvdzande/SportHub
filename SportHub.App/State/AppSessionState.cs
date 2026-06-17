using SportHub.Shared.DTOs.Members;

namespace SportHub.App.State;

public class AppSessionState
{
    public event Action? Changed;

    public string? AccessToken { get; private set; }
    public MemberDto? CurrentMember { get; private set; }
    public bool IsInstructor { get; private set; }
    public Guid? InstructorId { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken);

    public void SetSession(string accessToken, MemberDto? currentMember)
    {
        AccessToken = accessToken;
        CurrentMember = currentMember;
        IsInstructor = false;
        InstructorId = null;
        Changed?.Invoke();
    }

    public void SetInstructorSession(string accessToken, Guid instructorId)
    {
        AccessToken = accessToken;
        CurrentMember = null;
        IsInstructor = true;
        InstructorId = instructorId;
        Changed?.Invoke();
    }

    public void ClearSession()
    {
        AccessToken = null;
        CurrentMember = null;
        IsInstructor = false;
        InstructorId = null;
        Changed?.Invoke();
    }
}

