using SportHub.App.Services.Auth;

namespace SportHub.App;

public partial class App : Application
{
	public App(IAuthApiService authApiService)
	{
		InitializeComponent();
		_ = RestoreSessionAsync(authApiService);
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}

	private static async Task RestoreSessionAsync(IAuthApiService authApiService)
	{
		try
		{
			await authApiService.RestoreSessionAsync();
		}
		catch
		{
			// Keep startup resilient if storage/network is temporarily unavailable.
		}
	}
}