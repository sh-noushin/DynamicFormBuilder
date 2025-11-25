using FormBuilder.API.Extensions;
using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;

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

// Add OpenAPI for Scalar
builder.Services.AddOpenApi();

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

// Add built-in OpenAPI (can stay, not harmful)
builder.Services.AddEndpointsApiExplorer();

// Add Repositories and Services
builder.Services.AddFormBuilderRepositories();
builder.Services.AddFormBuilderServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Expose the OpenAPI document
    app.MapOpenApi();

    // Expose Scalar UI at /scalar
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("FormBuilder API");
        // you can customize more: theme, dark mode, etc.
    });
}

app.UseCors("AllowAngular");


// This will override previous CORS, maybe you only want one of them:
app.UseCors("AllowAll");

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

