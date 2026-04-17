using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzureWatcher.Function.Domain.Interfaces;

/// <summary>
/// Service responsible for persisting and retrieving known job offer IDs.
/// </summary>
public interface IStorageService
{
    Task<bool> HasOfferBeenProcessedAsync(string offerId, CancellationToken cancellationToken = default);
    Task SaveProcessedOffersAsync(IEnumerable<string> offerIds, CancellationToken cancellationToken = default);
}
