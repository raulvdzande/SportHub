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
        using var response = await client.GetAsync("api/instructors", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return new List<InstructorDto>();
        }

        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<List<InstructorDto>>(cancellationToken);
        return items ?? new List<InstructorDto>();
    }
    public async Task<InstructorDto> CreateAsync(CreateInstructorRequestDto request, IBrowserFile? photo = null, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(request.FullName), "FullName");
        content.Add(new StringContent(request.IsTbd.ToString()), "IsTbd");
        content.Add(new StringContent(request.IsActive.ToString()), "IsActive");
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            content.Add(new StringContent(request.Email), "Email");
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            content.Add(new StringContent(request.Password), "Password");
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            content.Add(new StringContent(request.PhoneNumber), "PhoneNumber");
        }

        if (!string.IsNullOrWhiteSpace(request.PhotoUrl))
        {
            content.Add(new StringContent(request.PhotoUrl), "PhotoUrl");
        }

        if (photo is not null)
        {
            await using var stream = photo.OpenReadStream(5 * 1024 * 1024);
            using var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(photo.ContentType);
            content.Add(streamContent, "Photo", photo.Name);
            using var withPhoto = await client.PostAsync("api/instructors", content, cancellationToken);
            withPhoto.EnsureSuccessStatusCode();
            return (await withPhoto.Content.ReadFromJsonAsync<InstructorDto>(cancellationToken))!;
        }
        using var response = await client.PostAsync("api/instructors", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<InstructorDto>(cancellationToken))!;
    }

    public async Task<InstructorDto> UpdateAsync(Guid id, UpdateInstructorRequestDto request, IBrowserFile? photo = null, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(request.FullName), "FullName");
        content.Add(new StringContent(request.IsTbd.ToString()), "IsTbd");
        content.Add(new StringContent(request.IsActive.ToString()), "IsActive");
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            content.Add(new StringContent(request.Email), "Email");
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            content.Add(new StringContent(request.Password), "Password");
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            content.Add(new StringContent(request.PhoneNumber), "PhoneNumber");
        }

        if (!string.IsNullOrWhiteSpace(request.PhotoUrl))
        {
            content.Add(new StringContent(request.PhotoUrl), "PhotoUrl");
        }

        if (photo is not null)
        {
            await using var stream = photo.OpenReadStream(5 * 1024 * 1024);
            using var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(photo.ContentType);
            content.Add(streamContent, "Photo", photo.Name);
        }

        using var response = await client.PutAsync($"api/instructors/{id}", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<InstructorDto>(cancellationToken))!;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        using var response = await client.DeleteAsync($"api/instructors/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
