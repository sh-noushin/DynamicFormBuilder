using FormBuilder.Models.Entities;

namespace FormBuilder.Core.DTOs;

// Returned by GET /api/billing/plan so the frontend can render the
// billing page: current plan, usage bars, and per-plan limits.
public class BillingStatusDto
{
    public BillingPlan Plan { get; set; }
    // Mirror of Stripe's subscription status ("active" / "trialing" /
    // "past_due" / "canceled"). Null for tenants still on Free.
    public string? Status { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public int FormCount { get; set; }
    public int MaxForms { get; set; }
    public int SubmissionsThisMonth { get; set; }
    public int MaxSubmissionsPerMonth { get; set; }
}

// Response for POST /api/billing/checkout — the URL to redirect the
// admin's browser to for Stripe Checkout.
public class CheckoutSessionDto
{
    public string Url { get; set; } = string.Empty;
}
