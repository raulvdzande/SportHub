using System.Windows.Input;
using SportHub.App.Services.Api;
using SportHub.App.State;
using SportHub.Shared.DTOs.Members;
using SportHub.Shared.DTOs.Payments;

namespace SportHub.App.ViewModels;

public class SubscriptionsViewModel : ViewModelBase
{
    private readonly IMemberSubscriptionsApiClient _subClient;
    private readonly IMembershipPlansApiClient     _plansClient;
    private readonly IStripeApiClient              _stripe;
    private readonly AppSessionState               _sessionState;

    public SubscriptionsViewModel(
        IMemberSubscriptionsApiClient subClient,
        IMembershipPlansApiClient plansClient,
        IStripeApiClient stripe,
        AppSessionState sessionState)
    {
        _subClient    = subClient;
        _plansClient  = plansClient;
        _stripe       = stripe;
        _sessionState = sessionState;

        RefreshCommand          = new Command(async () => await LoadAsync());
        CancelSubscriptionCommand   = new Command<Guid>(async id => await CancelSubscriptionAsync(id));
        ReactivateSubscriptionCommand = new Command<Guid>(async id => await ReactivateSubscriptionAsync(id));
        BuyPlanCommand          = new Command<Guid>(async id => await StartStripeBuyAsync(id));
        UpgradeCommand          = new Command<Guid>(async id => await UpgradeAsync(id));
    }

    public ICommand RefreshCommand                { get; }
    public ICommand CancelSubscriptionCommand     { get; }
    public ICommand ReactivateSubscriptionCommand { get; }
    public ICommand BuyPlanCommand                { get; }
    public ICommand UpgradeCommand                { get; }

    // ── Collections ──────────────────────────────────────────────────────────

    private IReadOnlyCollection<SubscriptionDisplayItem> _mySubscriptions = Array.Empty<SubscriptionDisplayItem>();
    public IReadOnlyCollection<SubscriptionDisplayItem> MySubscriptions
    {
        get => _mySubscriptions;
        set
        {
            SetProperty(ref _mySubscriptions, value);
            OnPropertyChanged(nameof(HasSubscriptions));
            OnPropertyChanged(nameof(HasNoSubscriptions));
        }
    }

    private IReadOnlyCollection<MembershipPlanDto> _availablePlans = Array.Empty<MembershipPlanDto>();
    public IReadOnlyCollection<MembershipPlanDto> AvailablePlans
    {
        get => _availablePlans;
        set { SetProperty(ref _availablePlans, value); OnPropertyChanged(nameof(HasAvailablePlans)); }
    }

    public bool HasSubscriptions   => MySubscriptions.Any();
    public bool HasNoSubscriptions => !HasSubscriptions;
    public bool HasAvailablePlans  => AvailablePlans.Any();

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

    private string _statusMessage = string.Empty;
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

    private bool _isSuccess;
    public bool IsSuccess { get => _isSuccess; set => SetProperty(ref _isSuccess, value); }

    // ── Load ──────────────────────────────────────────────────────────────────

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var subs  = await _subClient.GetMySubscriptionsAsync();
            var plans = (await _plansClient.GetAllAsync())
                            ?.Where(p => p.IsActive).ToList()
                        ?? new List<MembershipPlanDto>();

            var items = subs
                .Select(s => new SubscriptionDisplayItem(s, plans.FirstOrDefault(p => p.Id == s.PlanId)))
                .OrderByDescending(s => s.Subscription.StartsAtUtc)
                .ToList();

            MySubscriptions = items;

            var active = items.FirstOrDefault(s => s.IsActive);
            if (active is not null)
            {
                // Upgrades: more expensive plans than the current
                AvailablePlans = plans
                    .Where(p => p.Price > active.Price && p.Id != active.Subscription.PlanId)
                    .OrderBy(p => p.Price)
                    .ToList();
            }
            else
            {
                // No active subscription → show all plans to buy
                AvailablePlans = plans.OrderBy(p => p.Price).ToList();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fout bij laden: {ex.Message}";
            IsSuccess = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Annuleren (auto-verlenging uit, abonnement nog actief tot einddatum) ─

    private async Task CancelSubscriptionAsync(Guid id)
    {
        var item = MySubscriptions.FirstOrDefault(s => s.Id == id);
        var endDate = item?.EndDate ?? "de einddatum";

        var ok = await Shell.Current.DisplayAlert(
            "Abonnement opzeggen",
            $"Je abonnement blijft actief tot {endDate}. Daarna stopt het automatisch.\n\nJe kunt het altijd nog heractiveren zolang het nog loopt.",
            "Opzeggen", "Annuleren");
        if (!ok) return;

        IsBusy = true;
        try
        {
            await _subClient.DisableAutoRenewAsync(id);
            StatusMessage = $"Opgezegd. Abonnement actief tot {endDate}.";
            IsSuccess = true;
            await LoadAsync();
        }
        catch (Exception ex) { SetError(ex); }
        finally { IsBusy = false; }
    }

    // ── Heractiveren (gratis, zolang abonnement nog loopt) ────────────────────

    private async Task ReactivateSubscriptionAsync(Guid id)
    {
        var item = MySubscriptions.FirstOrDefault(s => s.Id == id);
        var endDate = item?.EndDate ?? "de einddatum";

        var ok = await Shell.Current.DisplayAlert(
            "Abonnement heractiveren",
            $"Auto-verlenging wordt ingeschakeld. Na {endDate} wordt je abonnement automatisch verlengd.",
            "Heractiveren", "Annuleren");
        if (!ok) return;

        IsBusy = true;
        try
        {
            await _subClient.EnableAutoRenewAsync(id);
            StatusMessage = "Auto-verlenging ingeschakeld!";
            IsSuccess = true;
            await LoadAsync();
        }
        catch (Exception ex) { SetError(ex); }
        finally { IsBusy = false; }
    }

    // ── Nieuw abonnement kopen via Stripe ─────────────────────────────────────

    private async Task StartStripeBuyAsync(Guid planId)
    {
        var plan = AvailablePlans.FirstOrDefault(p => p.Id == planId);
        if (plan is null) return;

        var memberId = _sessionState.CurrentMember?.Id
            ?? throw new InvalidOperationException("Niet ingelogd.");
        var period   = plan.PeriodType == "Monthly" ? "maand" : "jaar";
        var sessions = plan.SessionsPerWeekLimit.HasValue ? $"{plan.SessionsPerWeekLimit}x per week" : "Onbeperkt";

        var ok = await Shell.Current.DisplayAlert(
            "Abonnement kopen",
            $"{plan.Name}\n€{plan.Price:F2} / {period}\n{sessions}\n\nJe wordt doorgestuurd naar Stripe om veilig te betalen.",
            "Betalen", "Annuleren");
        if (!ok) return;

        IsBusy = true;
        StatusMessage = string.Empty;
        try
        {
            // Pass planId as SubscriptionId so the success deep-link carries it back
            var session = await _stripe.CreateCheckoutSessionAsync(
                new CreateStripePaymentIntentRequestDto
                {
                    MemberId       = memberId,
                    SubscriptionId = planId,    // planId travels in the success URL
                    Amount         = plan.Price,
                    Currency       = "eur"
                });

            if (session is null || string.IsNullOrWhiteSpace(session.CheckoutUrl))
            {
                StatusMessage = "Kon Stripe-sessie niet aanmaken. Controleer de server.";
                IsSuccess = false;
                IsBusy = false;
                return;
            }

            IsBusy = false;
            StatusMessage = "Stripe geopend — betaal en keer terug naar de app.";
            IsSuccess = true;

            await Browser.Default.OpenAsync(session.CheckoutUrl, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex) { SetError(ex); IsBusy = false; }
    }

    // Called by deep-link handler (sporthub://payment/success?planId=…)
    public async Task OnPaymentSuccessAsync(Guid planId)
    {
        var memberId = _sessionState.CurrentMember?.Id
            ?? throw new InvalidOperationException("Sessie verlopen — heropen de app en log opnieuw in.");
        IsBusy = true;
        StatusMessage = string.Empty;
        try
        {
            var sub = await _subClient.CreateAsync(new CreateMemberSubscriptionRequestDto
            {
                MemberId  = memberId,
                PlanId    = planId,
                AutoRenew = true
            });

            StatusMessage = sub is not null
                ? "Betaling geslaagd! Abonnement is actief."
                : "Betaling ontvangen maar abonnement aanmaken mislukt. Neem contact op.";
            IsSuccess = sub is not null;
            await LoadAsync();
        }
        catch (Exception ex) { SetError(ex); IsBusy = false; }
    }

    // ── Upgrade ───────────────────────────────────────────────────────────────

    private async Task UpgradeAsync(Guid targetPlanId)
    {
        var current = MySubscriptions.FirstOrDefault(s => s.IsActive);
        if (current is null) return;

        IsBusy = true;
        SubscriptionUpgradeQuoteDto? quote = null;
        try
        {
            quote = await _subClient.GetUpgradeQuoteAsync(new SubscriptionUpgradeQuoteRequestDto
            {
                CurrentSubscriptionId = current.Id,
                TargetPlanId          = targetPlanId
            });
        }
        catch (Exception ex) { SetError(ex); IsBusy = false; return; }
        finally { IsBusy = false; }

        if (quote is null) { StatusMessage = "Kan upgrade niet berekenen."; IsSuccess = false; return; }

        var plan = AvailablePlans.FirstOrDefault(p => p.Id == targetPlanId);
        var ok = await Shell.Current.DisplayAlert(
            $"Upgraden naar {quote.TargetPlanName}",
            $"Resterende credit: €{quote.RemainingCredit:F2}\nNog te betalen: €{quote.AmountToPay:F2}\n({quote.RemainingDays} resterende dagen)",
            "Betalen via Stripe", "Annuleren");
        if (!ok) return;

        var memberId = _sessionState.CurrentMember?.Id ?? Guid.Empty;

        IsBusy = true;
        try
        {
            // Pass targetPlanId so the deep link knows which plan to create
            var session = await _stripe.CreateCheckoutSessionAsync(
                new CreateStripePaymentIntentRequestDto
                {
                    MemberId       = memberId,
                    SubscriptionId = targetPlanId,
                    Amount         = quote.AmountToPay,
                    Currency       = "eur"
                });

            if (session is null || string.IsNullOrWhiteSpace(session.CheckoutUrl))
            {
                StatusMessage = "Stripe sessie mislukt.";
                IsSuccess = false;
                IsBusy = false;
                return;
            }

            IsBusy = false;
            StatusMessage = "Stripe geopend — betaal en keer terug.";
            IsSuccess = true;
            await Browser.Default.OpenAsync(session.CheckoutUrl, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex) { SetError(ex); IsBusy = false; }
    }

    private void SetError(Exception ex)
    {
        StatusMessage = $"Fout: {ex.Message}";
        IsSuccess = false;
    }
}
