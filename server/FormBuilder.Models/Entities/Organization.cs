namespace FormBuilder.Models.Entities;

// A tenant / workspace. Every Form, User, and ApiKey belongs to exactly
// one Organization; queries in the authenticated API are always scoped
// by the current caller's OrganizationId so tenants never see each
// other's data. Public form endpoints (/f/:slug) are addressed by slug,
// which is globally unique across all orgs.
public class Organization
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // Short URL-safe identifier reserved for future white-label / custom
    // domain features. Not used for routing today.
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // -- Billing --------------------------------------------------------
    // Current subscription plan. Defaults to Free — every new tenant
    // starts here and upgrades via Stripe Checkout. Downgrades happen
    // in the Stripe billing portal or after a failed payment.
    public BillingPlan Plan { get; set; } = BillingPlan.Free;
    // Stripe Customer object id (cus_xxx). Created on first checkout
    // and kept for the life of the tenant so the same customer record
    // is reused across upgrades / downgrades.
    public string? StripeCustomerId { get; set; }
    // Stripe Subscription object id (sub_xxx). Null for tenants that
    // never upgraded past Free. Populated by the customer.subscription
    // webhook, cleared when the subscription is canceled and expires.
    public string? StripeSubscriptionId { get; set; }
    // Mirrors Stripe's subscription status so the app can flag past-due
    // accounts without a live API call. Null for Free tenants.
    public string? SubscriptionStatus { get; set; }
    // End of the currently paid billing period. When UTC now passes
    // this, we honor the plan until the next webhook confirms renewal
    // or cancellation.
    public DateTime? SubscriptionCurrentPeriodEnd { get; set; }
}

public enum BillingPlan
{
    Free,
    Pro,
}
