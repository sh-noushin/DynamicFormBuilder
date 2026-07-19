using FormBuilder.API.Extensions;
using FormBuilder.API.Middleware;
using FormBuilder.Core.Options;
using FormBuilder.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddFormBuilderCors(builder.Configuration);

// Add Controllers and JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Add OpenAPI for Scalar � force OpenAPI 3.0
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
});

// Add DbContext
builder.Services.AddFormBuilderDbContext(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=FormBuilder.db");

// Add Identity
builder.Services.AddFormBuilderIdentity();

// Add AutoMapper
builder.Services.AddFormBuilderAutoMapper();

// Add Authentication (JWT)
builder.Services.AddFormBuilderJwtAuthentication(builder.Configuration);

// Endpoints API explorer (optional)
builder.Services.AddEndpointsApiExplorer();

// Add Repositories and Services
builder.Services.AddFormBuilderRepositories();
builder.Services.AddFormBuilderServices();

// Add global exception handling
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Expose the OpenAPI document
    app.MapOpenApi();

    // Expose Scalar UI at /scalar
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("FormBuilder API");
    });
}

app.UseCors(CorsOptions.AngularPolicyName);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Always ensure the schema is up to date; only seed sample data in Development.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FormBuilderDbContext>();
    await DataSeeder.EnsureDatabaseAsync(context);

    if (app.Environment.IsDevelopment())
    {
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<FormBuilder.Models.Entities.User>>();
        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();
        var seedOptions = app.Configuration.GetSection(SeedOptions.SectionName).Get<SeedOptions>()
            ?? throw new InvalidOperationException($"Missing '{SeedOptions.SectionName}' configuration section for development seeding.");
        await DataSeeder.SeedAsync(context, userManager, roleManager, seedOptions);
    }
}

app.Run();
