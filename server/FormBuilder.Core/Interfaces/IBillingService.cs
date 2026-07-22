using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

public interface IBillingService
{
    // Reports the caller's tenant plan + usage. Used by the billing
    // page to render limits and the enforce-limit guardrails.
    Task<BillingStatusDto> GetStatusAsync();

    // Creates a Stripe Checkout Session for the caller's tenant to
    // upgrade to Pro. Returns the URL the frontend should redirect to.
    Task<CheckoutSessionDto> CreateCheckoutSessionAsync();

    // Opens Stripe's Billing Portal so paying customers can manage
    // payment method / cancel / view invoices. Returns the URL.
    Task<CheckoutSessionDto> CreatePortalSessionAsync();

    // Verifies the webhook signature and applies the subscription
    // event to the matching Organization. Called from an anonymous
    // endpoint so signature verification is critical.
    Task HandleWebhookAsync(string payload, string signature);
}
