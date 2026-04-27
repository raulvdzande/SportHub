using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using SportHub.Shared.DTOs.Instructors;
namespace SportHub.Web.Services.Api;
public class InstructorsApiClient : IInstructorsApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    public InstructorsApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    public async Task<IReadOnlyCollection<InstructorDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var items = await client.GetFromJsonAsync<List<InstructorDto>>("api/instructors", cancellationToken);
        return items ?? new List<InstructorDto>();
    }
    public async Task<InstructorDto> CreateAsync(string fullName, IBrowserFile? photo, bool isTbd, bool isActive, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(fullName), "FullName");
        content.Add(new StringContent(isTbd.ToString()), "IsTbd");
        content.Add(new StringContent(isActive.ToString()), "IsActive");
        if (photo is not null)
        {
            await using var stream = photo.OpenReadStream(5 * 1024 * 1024);
            using var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(photo.ContentType);
            content.Add(streamContent, "Photo", photo.Name);
            using var responseWithPhoto = await client.PostAsync("api/instructors", content, cancellationToken);
            responseWithPhoto.EnsureSuccessStatusCode();
            return (await responseWithPhoto.Content.ReadFromJsonAsync<InstructorDto>(cancellationToken))!;
        }
        using var response = await client.PostAsync("api/instructors", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<InstructorDto>(cancellationToken))!;
    }
}
