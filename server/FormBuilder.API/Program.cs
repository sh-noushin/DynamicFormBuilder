using FormBuilder.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddFormBuilderCors();

// Add Controllers and JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Add DbContext
builder.Services.AddFormBuilderDbContext(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=FormBuilder.db");

// Add Identity
builder.Services.AddFormBuilderIdentity();

// Add AutoMapper
builder.Services.AddFormBuilderAutoMapper();

// Add Authentication (JWT)
builder.Services.AddFormBuilderJwtAuthentication(builder.Configuration);

// Add OpenAPI (minimal, .NET 10 built-in)
builder.Services.AddEndpointsApiExplorer();

// Add Repositories and Services
builder.Services.AddFormBuilderRepositories();
builder.Services.AddFormBuilderServices();

var app = builder.Build();

// Configure the HTTP request pipeline.


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAngular");

// Add CSP header for development to allow Chrome DevTools and localhost connections
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; connect-src 'self' http://localhost:4200");
    await next();
});

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure database is migrated and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FormBuilder.Infrastructure.Data.FormBuilderDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<FormBuilder.Models.Entities.User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
    await FormBuilder.Infrastructure.Data.DataSeeder.SeedAsync(context, userManager, roleManager);
}

app.Run();
