using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SportHub.Web;
using SportHub.Web.Services.Api;
using SportHub.Web.Services.Auth;
using SportHub.Web.Services.Localization;
using SportHub.Web.Services.Storage;
using SportHub.Web.State;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001/";

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthSessionState>();
builder.Services.AddScoped<BrowserStorageService>();
builder.Services.AddScoped<LanguageService>();
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
builder.Services.AddScoped<ApiAuthorizationMessageHandler>();

builder.Services.AddHttpClient("ApiAnonymous", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

builder.Services.AddScoped<IAuthApiService, AuthApiService>();
builder.Services.AddScoped<IWorkoutsApiClient, WorkoutsApiClient>();
builder.Services.AddScoped<IInstructorsApiClient, InstructorsApiClient>();
builder.Services.AddScoped<ILocationsApiClient, LocationsApiClient>();
builder.Services.AddScoped<ILessonsApiClient, LessonsApiClient>();
builder.Services.AddScoped<IMembersApiClient, MembersApiClient>();
builder.Services.AddScoped<IMembershipPlansApiClient, MembershipPlansApiClient>();
builder.Services.AddScoped<IAuthMemberApiClient, AuthMemberApiClient>();
builder.Services.AddScoped<IPaymentsApiClient, PaymentsApiClient>();
builder.Services.AddScoped<IMemberSubscriptionsApiClient, MemberSubscriptionsApiClient>();

await builder.Build().RunAsync();
