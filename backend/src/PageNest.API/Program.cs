using PageNest.API.Configurations;
using PageNest.Application.Constants;
using PageNest.Application.Interfaces;
using PageNest.Infrastructure.Data.Seed;
using PageNest.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Seed.json", optional: true, reloadOnChange: false);

ConfigurationManager configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddCustomSecurity(configuration);
builder.Services.AddCustomDatabaseConfiguration(configuration);

builder.Services.AddScoped<ISeeder, Seeder>();

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddAuthorizationBuilder()
                .AddPolicy(AppSettings.AdminRole, policy => policy.RequireRole(AppSettings.AdminRole))
                .AddPolicy(AppSettings.UserRole, policy => policy.RequireRole(AppSettings.UserRole))
                .AddPolicy(AppSettings.AdminUserRole, policy => policy.RequireRole(AppSettings.AdminRole, AppSettings.UserRole));

builder.Services.AddCors(options =>
{
    options.AddPolicy(AppSettings.AllowLocalhost,
        builder =>
        {
            builder.WithOrigins(AppSettings.ClientOrigin)
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
        });
});

var app = builder.Build();

app.UseCors(AppSettings.AllowLocalhost);
SeedDatabase(app.Services);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

static void SeedDatabase(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var seedService = scope.ServiceProvider.GetRequiredService<ISeeder>();

    seedService.SeedGenres();
    seedService.SeedCategories();
}

public partial class Program { }
