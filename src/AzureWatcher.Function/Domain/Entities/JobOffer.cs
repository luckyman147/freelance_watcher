using System;

namespace AzureWatcher.Function.Domain.Entities;

/// <summary>
/// Represents a scraped job offer from the target website.
/// </summary>
public class JobOffer
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;
}
