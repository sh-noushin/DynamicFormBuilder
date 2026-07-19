using FormBuilder.Core.Interfaces;
using FormBuilder.Core.Services;
using FormBuilder.Core.Services.FieldRules;
using FormBuilder.Infrastructure.Data;
using FormBuilder.Infrastructure.Repositories;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFormBuilderDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<FormBuilderDbContext>(options =>
            options.UseSqlite(connectionString));
        return services;
    }

    public static IServiceCollection AddFormBuilderIdentity(this IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;
        })
        .AddEntityFrameworkStores<FormBuilderDbContext>()
        .AddDefaultTokenProviders();
        return services;
    }

    public static IServiceCollection AddFormBuilderRepositories(this IServiceCollection services)
    {
        services.AddScoped<IFormRepository, FormRepository>();
        services.AddScoped<IFormVersionRepository, FormVersionRepository>();
        services.AddScoped<IFormFieldRepository, FormFieldRepository>();
        services.AddScoped<IFormSubmissionRepository, FormSubmissionRepository>();
        return services;
    }

    public static IServiceCollection AddFormBuilderServices(this IServiceCollection services)
    {
        services.AddScoped<IFormService, FormService>();
        services.AddScoped<IFormVersionService, FormVersionService>();
        services.AddScoped<IFormFieldService, FormFieldService>();
        services.AddScoped<IFormSubmissionService, FormSubmissionService>();
        services.AddScoped<IFieldRule, RequiredRule>();
        services.AddScoped<IFieldRule, PatternRule>();
        services.AddScoped<IFieldRule, LengthRule>();
        services.AddScoped<IFieldRule, NumericRangeRule>();
        services.AddScoped<IFieldRule, AllowedValuesRule>();
        services.AddScoped<IFieldValidator, FieldValidator>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        return services;
    }
}
