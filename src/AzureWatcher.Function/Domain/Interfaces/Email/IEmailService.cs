using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureWatcher.Function.Domain.Entities;

namespace AzureWatcher.Function.Domain.Interfaces.Email;

/// <summary>
/// Service responsible for sending email notifications.
/// </summary>
public interface IEmailService
{
    Task SendNewOffersEmailAsync(IEnumerable<JobOffer> newOffers, CancellationToken cancellationToken = default);
}
