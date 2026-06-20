using System.Net;
using System.Net.Http.Json;
using SportHub.Shared.DTOs.Locations;

namespace SportHub.App.Services.Api;

public class LocationsApiClient : ILocationsApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LocationsApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyCollection<LocationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        using var response = await client.GetAsync("api/locations", cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new List<LocationDto>();
        }

        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<List<LocationDto>>(cancellationToken: cancellationToken);
        return items ?? new List<LocationDto>();
    }

    public async Task<LocationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync($"api/locations/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<LocationDto>(cancellationToken: cancellationToken);
    }

    public async Task<LocationDto> CreateAsync(CreateLocationRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsJsonAsync("api/locations", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<LocationDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("Could not read the created location.");
    }

    public async Task<LocationDto> UpdateAsync(Guid id, UpdateLocationRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PutAsJsonAsync($"api/locations/{id}", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<LocationDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("Could not read the updated location.");
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.DeleteAsync($"api/locations/{id}", cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
