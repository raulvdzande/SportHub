using Microsoft.Extensions.DependencyInjection;
using SportHub.App.Services.Api;

namespace SportHub.App.Pages;

public partial class DiagnosticsPage : ContentPage
{
    private readonly IMembersApiClient _membersClient;
    private readonly IHttpClientFactory _httpClientFactory;

    public DiagnosticsPage()
    {
        InitializeComponent();
        _membersClient = Application.Current!.Handler.MauiContext!.Services.GetRequiredService<IMembersApiClient>();
        _httpClientFactory = Application.Current!.Handler.MauiContext!.Services.GetRequiredService<IHttpClientFactory>();

        var client = _httpClientFactory.CreateClient("Api");
        BaseUrlLabel.Text = client.BaseAddress?.ToString() ?? "(none)";
    }

    private async void OnPingClicked(object? sender, EventArgs e)
    {
        ResultLabel.Text = "Pinging...";
        try
        {
            var member = await _membersClient.GetCurrentAsync();
            if (member is null)
            {
                ResultLabel.Text = "No member returned (401/403 or network error). Check logs.";
                return;
            }

            ResultLabel.Text = $"OK: {member.FirstName} {member.LastName} ({member.Email})";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Exception: {ex.Message}";
        }
    }
}
