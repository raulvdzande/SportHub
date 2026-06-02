using SportHub.App.Pages;
using SportHub.App.Services.Auth;
using SportHub.App.State;

namespace SportHub.App;

// Called by MainActivity when a sporthub:// deep link arrives.
// Subscribers (e.g. SubscriptionsPage) hook this to react to payment success/cancel.
public static class DeepLinkBus
{
    public static event Action<Uri>? Received;
    public static void Dispatch(Uri uri) => Received?.Invoke(uri);
}

public partial class App : Application
{
    private readonly LoginPage _loginPage;
    private readonly AppShell _shell;
    private readonly IAuthApiService _authApiService;
    private readonly AppSessionState _sessionState;

    private bool _initialized;

    public App(
        LoginPage loginPage,
        AppShell shell,
        IAuthApiService authApiService,
        AppSessionState sessionState)
    {
        InitializeComponent();

        _loginPage = loginPage;
        _shell = shell;
        _authApiService = authApiService;
        _sessionState = sessionState;

        _sessionState.Changed += OnSessionChanged;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Start always on LoginPage — no NavigationPage wrapper (avoid double-parent crash)
        return new Window(_loginPage);
    }

    protected override void OnStart()
    {
        base.OnStart();

        if (_initialized)
            return;

        _initialized = true;

        _ = Task.Run(InitializeSessionSafe);
    }

    private async Task InitializeSessionSafe()
    {
        try
        {
            var member = await _authApiService.RestoreSessionAsync();

            if (member != null)
                await MainThread.InvokeOnMainThreadAsync(SetShell);
            else
                await MainThread.InvokeOnMainThreadAsync(ShowLogin);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Startup auth failed: {ex}");
            await MainThread.InvokeOnMainThreadAsync(ShowLogin);
        }
    }

    private void OnSessionChanged()
    {
        if (_sessionState.IsAuthenticated && _sessionState.CurrentMember != null)
            MainThread.BeginInvokeOnMainThread(SetShell);
        else if (!_sessionState.IsAuthenticated)
            MainThread.BeginInvokeOnMainThread(ShowLogin);
    }

    // Called from MainActivity (Android) when a sporthub:// URL arrives
    public static void HandleDeepLink(Uri uri) => DeepLinkBus.Dispatch(uri);

    internal void ShowLogin()
    {
        if (Windows.Count > 0)
            Windows[0].Page = _loginPage;
    }

    private void SetShell()
    {
        if (Windows.Count > 0)
            Windows[0].Page = _shell;
    }
}
