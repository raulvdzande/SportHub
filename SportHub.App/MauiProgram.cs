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
		builder.Services.AddSingleton<IReservationsApiClient, ReservationsApiClient>();
		builder.Services.AddSingleton<ICheckInsApiClient, CheckInsApiClient>();
		builder.Services.AddSingleton<INotificationsApiClient, NotificationsApiClient>();
		builder.Services.AddSingleton<IInstructorAuthApiClient, InstructorAuthApiClient>();
		builder.Services.AddSingleton<IWorkoutsApiClient, WorkoutsApiClient>();
		builder.Services.AddSingleton<ILocationsApiClient, LocationsApiClient>();

		builder.Services.AddSingleton<AppShell>(); // ensure AppShell is resolvable

		builder.Services.AddTransient<ApiAuthorizationMessageHandler>();

		// ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<RegisterViewModel>();
		builder.Services.AddTransient<ProfileViewModel>();
		builder.Services.AddTransient<SubscriptionsViewModel>();
		builder.Services.AddTransient<ScheduleViewModel>();
		builder.Services.AddTransient<LessonDetailsViewModel>();
		builder.Services.AddTransient<ReservationsViewModel>();
		builder.Services.AddTransient<CheckInViewModel>();
		builder.Services.AddTransient<NotificationsViewModel>();
		builder.Services.AddTransient<HistoryViewModel>();
		builder.Services.AddTransient<InstructorLoginViewModel>();
		builder.Services.AddTransient<InstructorLessonListViewModel>();
		builder.Services.AddTransient<InstructorLessonDetailsViewModel>();
		builder.Services.AddTransient<InstructorLessonsManagementViewModel>();
		builder.Services.AddTransient<InstructorLessonEditViewModel>();

		// Pages
		builder.Services.AddSingleton<LoginPage>();
		builder.Services.AddTransient<RegisterPage>();
		builder.Services.AddSingleton<ProfilePage>();
		builder.Services.AddSingleton<SubscriptionsPage>();
		builder.Services.AddSingleton<SchedulePage>();
		builder.Services.AddSingleton<LessonDetailsPage>();
		builder.Services.AddSingleton<DiagnosticsPage>();
		builder.Services.AddSingleton<ReservationsPage>();
		builder.Services.AddTransient<CheckInPage>();
		builder.Services.AddSingleton<NotificationsPage>();
		builder.Services.AddSingleton<HistoryPage>();
		builder.Services.AddSingleton<InstructorLoginPage>();
		builder.Services.AddSingleton<InstructorLessonListPage>();
		builder.Services.AddTransient<InstructorLessonDetailsPage>();
		builder.Services.AddTransient<InstructorLessonsManagementPage>();
		builder.Services.AddTransient<InstructorLessonEditPage>();

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

		// HttpClients with adequate timeout for operations like account creation
		builder.Services.AddHttpClient("ApiAnonymous", client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.Timeout = TimeSpan.FromSeconds(30);
		});

		builder.Services.AddHttpClient("Api", client =>
		{
			client.BaseAddress = new Uri(apiBaseUrl);
			client.Timeout = TimeSpan.FromSeconds(30);
		}).AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

		#if DEBUG
		builder.Logging.AddDebug();
		#endif

		var app = builder.Build();
		RequestNotificationPermission();
		return app;
	}

	private static void RequestNotificationPermission()
	{
		try
		{
			if (DeviceInfo.Platform == DevicePlatform.Android)
			{
				MainThread.BeginInvokeOnMainThread(async () =>
				{
					try
					{
						var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
						if (status != PermissionStatus.Granted)
						{
							await Permissions.RequestAsync<Permissions.PostNotifications>();
						}
					}
					catch { }
				});
			}
		}
		catch { }
	}
}