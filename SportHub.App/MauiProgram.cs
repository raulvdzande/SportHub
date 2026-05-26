using Microsoft.Extensions.Logging;
using SportHub.App.Services.Api;
using SportHub.App.Services.Auth;
using SportHub.App.Services.Storage;
using SportHub.App.State;

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
		builder.Services.AddTransient<ApiAuthorizationMessageHandler>();

		var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001/";

		builder.Services.AddHttpClient("ApiAnonymous", client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
		});

		builder.Services.AddHttpClient("Api", client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
		}).AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
