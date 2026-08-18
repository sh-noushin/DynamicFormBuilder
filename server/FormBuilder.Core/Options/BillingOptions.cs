namespace FormBuilder.Core.Options;

// Stripe credentials + product IDs used by BillingService. All values
// are secrets or environment-specific — set in appsettings.Development
// for local dev, in environment variables in production. Never commit
// real keys to source control.
public sealed class BillingOptions
{
    public const string SectionName = "Stripe";

    // Server-side secret key (sk_test_... in dev, sk_live_... in prod).
    // Used to make all Stripe API calls.
    public string SecretKey { get; set; } = string.Empty;

    // Publishable key (pk_test_... / pk_live_...). Not currently used
    // server-side but reserved for the future case where the frontend
    // creates a PaymentIntent directly.
    public string PublishableKey { get; set; } = string.Empty;

    // Signing secret from the Stripe webhook endpoint. Verifies that
    // incoming POST /api/billing/webhook payloads actually came from
    // Stripe (not a spoof).
    public string WebhookSecret { get; set; } = string.Empty;

    // Price object id for the Pro tier (price_...). Passed to Checkout
    // Session creation. Configure separate Products/Prices in the
    // Stripe dashboard for test vs live environments.
    public string ProPriceId { get; set; } = string.Empty;

    // URL customers return to after Checkout completes / cancels.
    // Typically the app's admin billing page.
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

// Per-plan limits. Hard-coded because they change infrequently and
// duplicating them into config would just create drift with the
// pricing page copy. Change here + on the pricing page together.
public static class PlanLimits
{
    public static int MaxForms(FormBuilder.Models.Entities.BillingPlan plan) => plan switch
    {
        FormBuilder.Models.Entities.BillingPlan.Free => 3,
        FormBuilder.Models.Entities.BillingPlan.Pro => 25,
        _ => 0,
    };

    public static int MaxSubmissionsPerMonth(FormBuilder.Models.Entities.BillingPlan plan) => plan switch
    {
        FormBuilder.Models.Entities.BillingPlan.Free => 100,
        FormBuilder.Models.Entities.BillingPlan.Pro => 5000,
        _ => 0,
    };
}

// List price per plan, in USD per month. Stripe remains the source of
// truth for what a customer is actually charged — this is display copy
// for the super-admin tenants table and the upgrade CTA. Keep it in sync
// with the Stripe Price behind BillingOptions.ProPriceId and with the
// pricing line in billing.component.html.
public static class PlanPricing
{
    public static decimal MonthlyPriceUsd(FormBuilder.Models.Entities.BillingPlan plan) => plan switch
    {
        FormBuilder.Models.Entities.BillingPlan.Free => 0m,
        FormBuilder.Models.Entities.BillingPlan.Pro => 19m,
        _ => 0m,
    };
}
