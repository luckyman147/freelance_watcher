using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureWatcher.Function.Domain.Entities;

namespace AzureWatcher.Function.Domain.Interfaces;

/// <summary>
/// Service responsible for fetching job offers from the target website.
/// </summary>
public interface IJobScraperService
{
    Task<IEnumerable<JobOffer>> FetchLatestOffersAsync(CancellationToken cancellationToken = default);
}
