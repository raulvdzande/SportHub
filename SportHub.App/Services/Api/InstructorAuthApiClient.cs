using SportHub.Shared.DTOs.Auth;
using System.Text.Json;

namespace SportHub.App.Services.Api;

public class InstructorAuthApiClient : IInstructorAuthApiClient
{
    private readonly HttpClient _httpClient;

    public InstructorAuthApiClient(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("ApiAnonymous");
    }

    public async Task<LoginInstructorResponseDto?> LoginAsync(LoginInstructorRequestDto request, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/instructorauth/login", content, ct);
            if (!response.IsSuccessStatusCode) return null;

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<LoginInstructorResponseDto>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { return null; }
    }
}
