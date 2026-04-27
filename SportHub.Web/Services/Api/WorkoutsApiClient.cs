using System.Net.Http.Json;
using SportHub.Shared.DTOs.Workouts;
namespace SportHub.Web.Services.Api;
public class WorkoutsApiClient : IWorkoutsApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    public WorkoutsApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    public async Task<IReadOnlyCollection<WorkoutDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var items = await client.GetFromJsonAsync<List<WorkoutDto>>("api/workouts", cancellationToken);
        return items ?? new List<WorkoutDto>();
    }
    public async Task<WorkoutDto> CreateAsync(CreateWorkoutRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        using var response = await client.PostAsJsonAsync("api/workouts", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WorkoutDto>(cancellationToken))!;
    }
    public async Task<WorkoutDto> UpdateAsync(Guid id, UpdateWorkoutRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        using var response = await client.PutAsJsonAsync($"api/workouts/{id}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WorkoutDto>(cancellationToken))!;
    }
    public async Task<DeleteWorkoutResponseDto?> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        using var response = await client.DeleteAsync($"api/workouts/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DeleteWorkoutResponseDto>(cancellationToken);
    }
}
