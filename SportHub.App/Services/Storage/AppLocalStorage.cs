using System.Text.Json;

namespace SportHub.App.Services.Storage;

public class AppLocalStorage
{
    private const string TokenKey = "sporthub.token";
    private const string MemberKey = "sporthub.member";

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(TokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetTokenAsync(string token)
    {
        try
        {
            await SecureStorage.Default.SetAsync(TokenKey, token);
        }
        catch
        {
            Preferences.Set(TokenKey, token);
        }
    }

    public async Task RemoveTokenAsync()
    {
        try
        {
            SecureStorage.Default.Remove(TokenKey);
        }
        catch
        {
            Preferences.Remove(TokenKey);
        }

        await Task.CompletedTask;
    }

    public Task<string?> GetMemberJsonAsync()
    {
        var json = Preferences.Get(MemberKey, string.Empty);
        return Task.FromResult(string.IsNullOrWhiteSpace(json) ? null : json);
    }

    public Task SetMemberAsync<T>(T member)
    {
        var json = JsonSerializer.Serialize(member);
        Preferences.Set(MemberKey, json);
        return Task.CompletedTask;
    }

    public Task RemoveMemberAsync()
    {
        Preferences.Remove(MemberKey);
        return Task.CompletedTask;
    }
}

