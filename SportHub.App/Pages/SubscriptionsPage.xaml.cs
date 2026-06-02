using SportHub.App.ViewModels;

namespace SportHub.App.Pages;

public partial class SubscriptionsPage : ContentPage
{
    private readonly SubscriptionsViewModel _vm;

    public SubscriptionsPage(SubscriptionsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Unsubscribe first so re-entry (foreground resume) never double-registers
        DeepLinkBus.Received -= OnDeepLink;
        DeepLinkBus.Received += OnDeepLink;
        await _vm.LoadAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        DeepLinkBus.Received -= OnDeepLink;
    }

    private void OnDeepLink(Uri uri)
    {
        // sporthub://payment/success?planId=<guid>
        if (!uri.Host.Equals("payment", StringComparison.OrdinalIgnoreCase)) return;

        if (uri.AbsolutePath.Equals("/success", StringComparison.OrdinalIgnoreCase))
        {
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            if (Guid.TryParse(query["planId"], out var planId))
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                    await _vm.OnPaymentSuccessAsync(planId));
            }
        }
        else if (uri.AbsolutePath.Equals("/cancel", StringComparison.OrdinalIgnoreCase))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _vm.StatusMessage = "Betaling geannuleerd.";
                _vm.IsSuccess = false;
            });
        }
    }
}
