using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureWatcher.Function.Domain.Entities;
using AzureWatcher.Function.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AzureWatcher.Function.Infrastructure.Storage;

public class DatabaseStorageService : IStorageService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DatabaseStorageService> _logger;

    public DatabaseStorageService(AppDbContext dbContext, ILogger<DatabaseStorageService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> HasOfferBeenProcessedAsync(string offerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProcessedJobOffers.AnyAsync(o => o.Id == offerId, cancellationToken);
    }

    public async Task SaveProcessedOffersAsync(IEnumerable<string> offerIds, CancellationToken cancellationToken = default)
    {
        // Use Distinct() to handle potential duplicates in the input list
        foreach (var id in offerIds.Distinct())
        {
            // Check if it's already in the database
            var inDatabase = await _dbContext.ProcessedJobOffers.AnyAsync(o => o.Id == id, cancellationToken);
            
            // Also check if it's already being tracked in the current DbContext's ChangeTracker
            var isTracked = _dbContext.ProcessedJobOffers.Local.Any(o => o.Id == id);

            if (!inDatabase && !isTracked)
            {
                _dbContext.ProcessedJobOffers.Add(new JobOffer { Id = id });
            }
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
