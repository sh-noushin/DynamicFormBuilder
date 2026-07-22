using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Core.Options;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Repositories;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace FormBuilder.Infrastructure.Services;

// Wires the Organization entity to Stripe billing. Lives in Infrastructure
// so it can pull the Stripe SDK + repository together; callers reach it
// through IBillingService in Core.
public class BillingService : IBillingService
{
    private readonly BillingOptions _options;
    private readonly IOrganizationRepository _orgs;
    private readonly ICurrentUserService _currentUser;

    public BillingService(
        IOptions<BillingOptions> options,
        IOrganizationRepository orgs,
        ICurrentUserService currentUser)
    {
        _options = options.Value;
        _orgs = orgs;
        _currentUser = currentUser;
        // Stripe.net uses a static ApiKey — safe to set on every request
        // because we always assign the same value from config.
        if (!string.IsNullOrEmpty(_options.SecretKey))
        {
            StripeConfiguration.ApiKey = _options.SecretKey;
        }
    }

    public async Task<BillingStatusDto> GetStatusAsync()
    {
        var orgId = _currentUser.GetOrganizationId();
        var org = await _orgs.GetByIdAsync(orgId)
            ?? throw new InvalidOperationException("Organization not found.");
        var formCount = await _orgs.GetFormCountAsync(orgId);
        var subCount = await _orgs.GetSubmissionsThisMonthAsync(orgId);
        return new BillingStatusDto
        {
            Plan = org.Plan,
            Status = org.SubscriptionStatus,
            CurrentPeriodEnd = org.SubscriptionCurrentPeriodEnd,
            FormCount = formCount,
            MaxForms = PlanLimits.MaxForms(org.Plan),
            SubmissionsThisMonth = subCount,
            MaxSubmissionsPerMonth = PlanLimits.MaxSubmissionsPerMonth(org.Plan),
        };
    }

    public async Task<CheckoutSessionDto> CreateCheckoutSessionAsync()
    {
        var orgId = _currentUser.GetOrganizationId();
        var org = await _orgs.GetByIdAsync(orgId)
            ?? throw new InvalidOperationException("Organization not found.");

        // Reuse an existing Stripe Customer if we have one so the same
        // payment method survives across upgrade / downgrade cycles.
        var customerId = org.StripeCustomerId;
        if (string.IsNullOrEmpty(customerId))
        {
            var customer = await new CustomerService().CreateAsync(new CustomerCreateOptions
            {
                Name = org.Name,
                // Store the tenant id so we can recover the mapping if the
                // DB StripeCustomerId column ever gets wiped.
                Metadata = new Dictionary<string, string> { ["organizationId"] = org.Id.ToString() },
            });
            customerId = customer.Id;
            await _orgs.UpdateBillingAsync(org.Id, o => o.StripeCustomerId = customerId);
        }

        var session = await new SessionService().CreateAsync(new SessionCreateOptions
        {
            Mode = "subscription",
            Customer = customerId,
            LineItems = new List<SessionLineItemOptions>
            {
                new() { Price = _options.ProPriceId, Quantity = 1 },
            },
            SuccessUrl = _options.SuccessUrl,
            CancelUrl = _options.CancelUrl,
            // Metadata on the session propagates onto the subscription
            // via subscription_data.metadata below, giving us a hook
            // back to the tenant from any subscription webhook.
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string> { ["organizationId"] = org.Id.ToString() },
            },
        });

        return new CheckoutSessionDto { Url = session.Url };
    }

    public async Task<CheckoutSessionDto> CreatePortalSessionAsync()
    {
        var orgId = _currentUser.GetOrganizationId();
        var org = await _orgs.GetByIdAsync(orgId)
            ?? throw new InvalidOperationException("Organization not found.");
        if (string.IsNullOrEmpty(org.StripeCustomerId))
            throw new InvalidOperationException("No Stripe customer for this workspace yet.");

        var session = await new Stripe.BillingPortal.SessionService().CreateAsync(
            new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = org.StripeCustomerId,
                ReturnUrl = _options.SuccessUrl,
            });

        return new CheckoutSessionDto { Url = session.Url };
    }

    public async Task HandleWebhookAsync(string payload, string signature)
    {
        // Verifying the signature ensures the payload actually came from
        // Stripe — this endpoint is anonymous and would otherwise be a
        // trivial way to forge subscription state changes.
        var stripeEvent = EventUtility.ConstructEvent(payload, signature, _options.WebhookSecret);

        switch (stripeEvent.Type)
        {
            case "customer.subscription.created":
            case "customer.subscription.updated":
            {
                var sub = stripeEvent.Data.Object as Subscription;
                if (sub == null) break;
                var org = await FindOrgForSubscriptionAsync(sub);
                if (org == null) break;
                await _orgs.UpdateBillingAsync(org.Id, o =>
                {
                    o.StripeSubscriptionId = sub.Id;
                    o.SubscriptionStatus = sub.Status;
                    o.SubscriptionCurrentPeriodEnd = sub.Items?.Data?.FirstOrDefault()?.CurrentPeriodEnd;
                    // Plan flips based on subscription active-ness.
                    // trialing / active / past_due all count as Pro so a
                    // brief payment hiccup doesn't yank the plan out.
                    o.Plan = sub.Status is "active" or "trialing" or "past_due"
                        ? BillingPlan.Pro
                        : BillingPlan.Free;
                });
                break;
            }
            case "customer.subscription.deleted":
            {
                var sub = stripeEvent.Data.Object as Subscription;
                if (sub == null) break;
                var org = await FindOrgForSubscriptionAsync(sub);
                if (org == null) break;
                await _orgs.UpdateBillingAsync(org.Id, o =>
                {
                    o.Plan = BillingPlan.Free;
                    o.SubscriptionStatus = sub.Status;
                    o.StripeSubscriptionId = null;
                    o.SubscriptionCurrentPeriodEnd = null;
                });
                break;
            }
            // Every other event type (invoice.paid, etc.) is ignored for
            // MVP — we can add richer handling later if a customer flow
            // needs it.
        }
    }

    private async Task<Organization?> FindOrgForSubscriptionAsync(Subscription sub)
    {
        // Prefer the metadata pointer we stamped at checkout — most
        // reliable across Stripe API changes.
        if (sub.Metadata != null && sub.Metadata.TryGetValue("organizationId", out var raw)
            && Guid.TryParse(raw, out var id))
        {
            return await _orgs.GetByIdAsync(id);
        }
        // Fall back to the Stripe subscription/customer id we already
        // persisted from a previous webhook.
        var bySub = await _orgs.GetByStripeSubscriptionIdAsync(sub.Id);
        if (bySub != null) return bySub;
        return await _orgs.GetByStripeCustomerIdAsync(sub.CustomerId);
    }
}
