using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using AzureWatcher.Function.Application.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AzureWatcher.Function;

public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(IServiceScopeFactory scopeFactory, ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Job Monitor Worker running at: {time}", DateTimeOffset.Now);

        // Run every 1 hour
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var jobMonitorService = scope.ServiceProvider.GetRequiredService<JobMonitorService>();
                await jobMonitorService.ProcessLatestOffersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the latest job offers.");
            }

            if (await timer.WaitForNextTickAsync(stoppingToken))
            {
                _logger.LogInformation("Job Monitor Worker running next tick at: {time}", DateTimeOffset.Now);
            }
        }
    }
}
