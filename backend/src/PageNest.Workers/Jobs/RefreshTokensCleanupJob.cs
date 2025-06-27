using Microsoft.EntityFrameworkCore;
using PageNest.Infrastructure.Data.Context;

namespace PageNest.Workers.Jobs;

public class RefreshTokensCleanupJob : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<RefreshTokensCleanupJob> _logger;
    private Timer _timer;

    public RefreshTokensCleanupJob(IServiceScopeFactory serviceScopeFactory, ILogger<RefreshTokensCleanupJob> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Refresh Tokens Cleanup Job is starting...");
        ScheduleNextCleanup(stoppingToken);

        return Task.CompletedTask;
    }

    private async Task ExecuteCleanup(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timer elapsed, executing delete command.");

        try
        {
            await DeleteRevokedTokens(stoppingToken);
            _logger.LogInformation("Delete command executed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing the delete command.");
        }
        finally
        {
            ScheduleNextCleanup(stoppingToken);
        }
    }

    private static TimeSpan GetTimeUntilCleanup()
    {
        // ToDo: In production, this should be set to run at midnight.
        //DateTime now = DateTime.Now;
        //DateTime nextRun = now.Date.AddDays(1);

        // return nextRun - now;

        // Temporary for testing purposes, runs every minute.
        return TimeSpan.FromMinutes(1);
    }

    private void ScheduleNextCleanup(CancellationToken stoppingToken)
    {
        TimeSpan timeUntilMidnight = GetTimeUntilCleanup();

        _timer = new Timer(async _ =>
        {
            if (!stoppingToken.IsCancellationRequested) await ExecuteCleanup(stoppingToken);

        }, null, timeUntilMidnight, Timeout.InfiniteTimeSpan);

        _logger.LogInformation($"Task scheduled to run in {timeUntilMidnight.TotalMinutes} minutes.");
    }

    private async Task DeleteRevokedTokens(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var revokedTokens = await context.RefreshTokens.Where(rt => rt.IsRevoked).ToListAsync(stoppingToken);

        if (!revokedTokens.Any())
        {
            _logger.LogInformation("No revoked tokens found to delete.");
            return;
        }

        context.RefreshTokens.RemoveRange(revokedTokens);
        await context.SaveChangesAsync(stoppingToken);

        _logger.LogInformation($"{revokedTokens.Count} revoked tokens have been deleted.");
    }
}
