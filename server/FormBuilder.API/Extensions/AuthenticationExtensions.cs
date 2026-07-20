using FormBuilder.API.Auth;
using FormBuilder.Core.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FormBuilder.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddFormBuilderJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException($"Missing '{JwtOptions.SectionName}' configuration section.");

        if (string.IsNullOrWhiteSpace(jwtOptions.Key))
            throw new InvalidOperationException($"'{JwtOptions.SectionName}:{nameof(JwtOptions.Key)}' is required.");
        if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
            throw new InvalidOperationException($"'{JwtOptions.SectionName}:{nameof(JwtOptions.Issuer)}' is required.");
        if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
            throw new InvalidOperationException($"'{JwtOptions.SectionName}:{nameof(JwtOptions.Audience)}' is required.");

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                ClockSkew = TimeSpan.Zero
            };
        })
        // Second auth scheme registered alongside JWT. Individual endpoints
        // opt in via [Authorize(AuthenticationSchemes = "Bearer,ApiKey")].
        .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationHandler.SchemeName, _ => { });
        return services;
    }
}
