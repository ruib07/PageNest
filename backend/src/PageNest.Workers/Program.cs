using PageNest.API.Configurations;
using PageNest.Workers.Jobs;

var builder = Host.CreateApplicationBuilder(args);
ConfigurationManager configuration = builder.Configuration;

builder.Services.AddCustomDatabaseConfiguration(configuration);

builder.Services.AddHostedService<RefreshTokensCleanupJob>();

var host = builder.Build();
host.Run();
