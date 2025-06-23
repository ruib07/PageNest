using Microsoft.EntityFrameworkCore;
using PageNest.Application.Constants;
using PageNest.Infrastructure.Data.Context;

namespace PageNest.API.Configurations;

public static class DatabaseConfig
{
    public static void AddCustomDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (environment == "Testing") return;

        var connectionString = configuration.GetConnectionString(AppSettings.DatabaseConfigSection);

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlServerOptions =>
            {
                var assembly = typeof(ApplicationDbContext).Assembly;
                var assemblyName = assembly.GetName();

                sqlServerOptions.MigrationsAssembly(assemblyName.Name);
                sqlServerOptions.EnableRetryOnFailure(maxRetryCount: 2, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            });
        });
    }
}
