namespace SportHub.API.Configuration;

public class StripeOptions
{
    public const string SectionName = "Stripe";

    /// <summary>Stripe secret key (sk_test_... for developer mode).</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Webhook signing secret (whsec_...). Optional if you only use simulate endpoint.</summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>Optional publishable key (pk_test_...) returned to clients after creating a payment intent.</summary>
    public string PublishableKey { get; set; } = string.Empty;
}
