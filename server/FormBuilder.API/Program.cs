using FormBuilder.API.Extensions;
using FormBuilder.API.Middleware;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddFormBuilderCors();

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

// Use only the CORS policy you really want
app.UseCors("AllowAngular");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure database is migrated and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<FormBuilder.Infrastructure.Data.FormBuilderDbContext>();
    var userManager = scope.ServiceProvider
        .GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<FormBuilder.Models.Entities.User>>();
    var roleManager = scope.ServiceProvider
        .GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
    await FormBuilder.Infrastructure.Data.DataSeeder.SeedAsync(context, userManager, roleManager);
}

app.Run();
