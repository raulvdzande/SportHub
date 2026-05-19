using SportHub.API.Application.Interfaces;

namespace SportHub.API.Infrastructure.Services;

public class ConsoleEmailService : IEmailService
{
    public Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("--- Email sent (console) ---");
        Console.WriteLine($"To: {toEmail}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine("Body:\n" + htmlBody);
        Console.WriteLine("---------------------------");
        return Task.CompletedTask;
    }
}

