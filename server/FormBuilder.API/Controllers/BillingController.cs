using FormBuilder.Core.Constants;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billing;
    private readonly ILogger<BillingController> _logger;

    public BillingController(IBillingService billing, ILogger<BillingController> logger)
    {
        _billing = billing;
        _logger = logger;
    }

    // Returns the caller tenant's current plan + usage. Any admin can
    // hit this; used to render the billing page and usage bars.
    [HttpGet("plan")]
    [Authorize(AuthenticationSchemes = "Bearer,ApiKey", Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BillingStatusDto), 200)]
    public async Task<ActionResult<BillingStatusDto>> GetPlan()
    {
        var status = await _billing.GetStatusAsync();
        return Ok(status);
    }

    // Creates a Stripe Checkout Session for upgrading to Pro. Returns
    // the URL the frontend redirects to. Only tenant admins can invoke.
    [HttpPost("checkout")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CheckoutSessionDto), 200)]
    public async Task<ActionResult<CheckoutSessionDto>> Checkout()
    {
        var session = await _billing.CreateCheckoutSessionAsync();
        return Ok(session);
    }

    // Opens Stripe's billing portal so paying customers can manage
    // their subscription (change card, cancel, view invoices). Only
    // available after the tenant has completed at least one Checkout.
    [HttpPost("portal")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CheckoutSessionDto), 200)]
    public async Task<ActionResult<CheckoutSessionDto>> Portal()
    {
        var session = await _billing.CreatePortalSessionAsync();
        return Ok(session);
    }

    // Stripe webhook receiver. Anonymous — signature verification in
    // the service handles authentication. The raw body is required
    // for signature verification, so we read it directly.
    [HttpPost("webhook")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(void), 400)]
    public async Task<IActionResult> Webhook()
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();
        try
        {
            await _billing.HandleWebhookAsync(payload, signature);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe webhook processing failed");
            return BadRequest();
        }
    }
}
