using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureWatcher.Function.Domain.Entities;
using AzureWatcher.Function.Domain.Interfaces;
using AzureWatcher.Function.Domain.Interfaces.Email;
using Microsoft.Extensions.Logging;

namespace AzureWatcher.Function.Application.Services;

/// <summary>
/// Orchestrates the process of monitoring jobs, checking for new ones, and sending notifications.
/// </summary>
public class JobMonitorService
{
    private readonly IJobScraperService _scraperService;
    private readonly IStorageService _storageService;
    private readonly IEmailService _emailService;
    private readonly ILogger<JobMonitorService> _logger;

    public JobMonitorService(
        IJobScraperService scraperService,
        IStorageService storageService,
        IEmailService emailService,
        ILogger<JobMonitorService> logger)
    {
        _scraperService = scraperService;
        _storageService = storageService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task ProcessLatestOffersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting job monitor process.");

        // 1. Fetch
        var offers = await _scraperService.FetchLatestOffersAsync(cancellationToken);
        if (!offers.Any())
        {
            _logger.LogWarning("No offers found. Perhaps the website structure changed.");
            return;
        }

        // 2. Filter New Offers
        var newOffers = new List<JobOffer>();
        foreach (var offer in offers)
        {
            if (!await _storageService.HasOfferBeenProcessedAsync(offer.Id, cancellationToken))
            {
                newOffers.Add(offer);
            }
        }

        if (!newOffers.Any())
        {
            _logger.LogInformation("No new offers found.");
            return;
        }

        _logger.LogInformation($"Found {newOffers.Count} new offers. Sending email...");

        // 3. Notify
        await _emailService.SendNewOffersEmailAsync(newOffers, cancellationToken);

        // 4. Save to Storage
        var newOfferIds = newOffers.Select(o => o.Id).ToList();
        await _storageService.SaveProcessedOffersAsync(newOfferIds, cancellationToken);

        _logger.LogInformation("Job monitor process completed successfully.");
    }
}
