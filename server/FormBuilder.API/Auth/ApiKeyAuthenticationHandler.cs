using System.Security.Claims;
using System.Text.Encodings.Web;
using FormBuilder.Core.Constants;
using FormBuilder.Core.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FormBuilder.API.Auth;

// Accepts an API key via the X-Api-Key header. Successful validation
// produces a ClaimsPrincipal with the Admin role - API keys are trusted
// the same as the admin that minted them, matching the mental model of
// a personal-access-token issued through the admin UI.
public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-Api-Key";

    private readonly IApiKeyService _apiKeyService;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyService apiKeyService)
        : base(options, logger, encoder)
    {
        _apiKeyService = apiKeyService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var headerValues))
            return AuthenticateResult.NoResult();

        var rawKey = headerValues.ToString();
        if (string.IsNullOrWhiteSpace(rawKey))
            return AuthenticateResult.NoResult();

        var validated = await _apiKeyService.ValidateAsync(rawKey);
        if (validated == null)
            return AuthenticateResult.Fail("Invalid API key.");

        // Synthetic identity - not a real user in the Identity store. Name
        // reflects the admin-set label so audit logs are legible.
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, $"apikey:{validated.Id}"),
            new Claim(ClaimTypes.Name, validated.Name),
            new Claim(ClaimTypes.Role, Roles.Admin),
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return AuthenticateResult.Success(ticket);
    }
}

public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions { }
