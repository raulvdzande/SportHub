using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SportHub.Shared.DTOs.Members;

namespace SportHub.App.Services.Api;

public class MembersApiClient : IMembersApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MembersApiClient> _logger;

    public MembersApiClient(IHttpClientFactory httpClientFactory, ILogger<MembersApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<MemberDto?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        try
        {
            var response = await client.GetAsync("api/members/me", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetCurrentAsync returned {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<MemberDto>(cancellationToken);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "GetCurrentAsync timed out");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GetCurrentAsync");
            return null;
        }
    }

    public async Task<MemberDto?> UpdateCurrentAsync(UpdateMemberProfileRequestDto request, Stream? photo = null, string? photoFileName = null, string? photoContentType = null, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(request.Email), "Email");
        content.Add(new StringContent(request.FirstName), "FirstName");
        content.Add(new StringContent(request.LastName), "LastName");

        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            content.Add(new StringContent(request.Username), "Username");
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            content.Add(new StringContent(request.PhoneNumber), "PhoneNumber");
        }

        if (photo is not null)
        {
            var streamContent = new StreamContent(photo);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(photoContentType ?? "application/octet-stream");
            content.Add(streamContent, "Photo", photoFileName ?? "profile.jpg");
            // streamContent is owned by content and disposed with it
        }

        try
        {
            var response = await client.PutAsync("api/members/me", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("UpdateCurrentAsync returned {StatusCode}: {Body}", response.StatusCode, body);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<MemberDto>(cancellationToken);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "UpdateCurrentAsync timed out");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UpdateCurrentAsync");
            return null;
        }
    }
    
    public async Task<MemberDto> CreateAsync(CreateMemberRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsJsonAsync("api/members", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MemberDto>(cancellationToken) ?? throw new InvalidOperationException("Failed to parse created member.");
    }
}
