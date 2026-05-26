using System.Net.Http.Json;
using Microsoft.Extensions.Http;
using SportHub.Shared.DTOs.Members;

namespace SportHub.App.Services.Api;

public class MembersApiClient : IMembersApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MembersApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<MemberDto?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync("api/members/me", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<MemberDto>(cancellationToken);
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
            using var streamContent = new StreamContent(photo);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(photoContentType ?? "application/octet-stream");
            content.Add(streamContent, "Photo", photoFileName ?? "profile.jpg");
        }

        var response = await client.PutAsync("api/members/me", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<MemberDto>(cancellationToken);
    }
}
