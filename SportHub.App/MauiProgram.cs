using Microsoft.Extensions.Logging;
using SportHub.App.Pages;
using SportHub.App.Services.Api;
using SportHub.App.Services.Auth;
using SportHub.App.Services.Storage;
using SportHub.App.State;
using SportHub.App.ViewModels;

namespace SportHub.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<AppSessionState>();
		builder.Services.AddSingleton<AppLocalStorage>();
		builder.Services.AddSingleton<IAuthApiService, ApiAuthService>();
		builder.Services.AddSingleton<IMembersApiClient, MembersApiClient>();
		builder.Services.AddSingleton<IMemberSubscriptionsApiClient, MemberSubscriptionsApiClient>();
		builder.Services.AddSingleton<ILessonsApiClient, LessonsApiClient>();
		builder.Services.AddSingleton<IMembershipPlansApiClient, MembershipPlansApiClient>();
		builder.Services.AddSingleton<IStripeApiClient, StripeApiClient>();

		builder.Services.AddSingleton<AppShell>(); // ensure AppShell is resolvable

		builder.Services.AddTransient<ApiAuthorizationMessageHandler>();

		// ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<RegisterViewModel>();
		builder.Services.AddTransient<ProfileViewModel>();
		builder.Services.AddTransient<SubscriptionsViewModel>();
		builder.Services.AddTransient<ScheduleViewModel>();
		builder.Services.AddTransient<LessonDetailsViewModel>();

		// Pages
		builder.Services.AddSingleton<LoginPage>();
		builder.Services.AddTransient<RegisterPage>();
		builder.Services.AddSingleton<ProfilePage>();
		builder.Services.AddSingleton<SubscriptionsPage>();
		builder.Services.AddSingleton<SchedulePage>();
		builder.Services.AddSingleton<LessonDetailsPage>();
		builder.Services.AddSingleton<DiagnosticsPage>();

		// Determine API base URL per platform to avoid emulator/localhost mismatch
		string apiBaseUrl;
		if (DeviceInfo.Platform == DevicePlatform.Android)
		{
			apiBaseUrl = "http://10.0.2.2:5099/";
		}
		else if (DeviceInfo.Platform == DevicePlatform.WinUI)
		{
			apiBaseUrl = "https://localhost:7275/";
		}
		else if (DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.MacCatalyst)
		{
			apiBaseUrl = "https://localhost:7275/";
		}
		else
		{
			apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5099/";
		}

		// HttpClients with a shorter timeout so the app fails fast
		builder.Services.AddHttpClient("ApiAnonymous", client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.Timeout = TimeSpan.FromSeconds(20);
		});

		builder.Services.AddHttpClient("Api", client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.Timeout = TimeSpan.FromSeconds(20);
		}).AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

		#if DEBUG
		builder.Logging.AddDebug();
		#endif

		return builder.Build();
	}
}