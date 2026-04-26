namespace SportHub.Shared.DTOs.Workouts;

public class DeleteWorkoutResponseDto
{
    public string Message { get; set; } = string.Empty;
    public int DeletedLessons { get; set; }
}

