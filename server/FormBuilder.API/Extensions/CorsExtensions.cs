using FormBuilder.Core.Options;

namespace FormBuilder.API.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddFormBuilderCors(this IServiceCollection services, IConfiguration configuration)
    {
        var corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();

        if (corsOptions.AllowedOrigins.Length == 0)
            throw new InvalidOperationException($"'{CorsOptions.SectionName}:{nameof(CorsOptions.AllowedOrigins)}' is required and must not be empty.");

        services.AddCors(options =>
        {
            options.AddPolicy(CorsOptions.AngularPolicyName, policy =>
            {
                policy.WithOrigins(corsOptions.AllowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
        return services;
    }
}
