using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureWatcher.Function.Domain.Entities;
using AzureWatcher.Function.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureWatcher.Function.Infrastructure.Scraping;

/// <summary>
/// Fetches job offers using the Tunisie Freelance JSON API.
/// </summary>
public class TunisieFreelanceApiScraper : IJobScraperService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TunisieFreelanceApiScraper> _logger;

    public TunisieFreelanceApiScraper(HttpClient httpClient, IConfiguration configuration, ILogger<TunisieFreelanceApiScraper> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        
        // Add a realistic User-Agent to avoid being blocked
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    }

    public async Task<IEnumerable<JobOffer>> FetchLatestOffersAsync(CancellationToken cancellationToken = default)
    {
        var apiUrl = _configuration["TargetWebsiteUrl"];
        if (string.IsNullOrEmpty(apiUrl))
        {
            throw new InvalidOperationException("TargetWebsiteUrl configuration is missing.");
        }

        try
        {
            _logger.LogInformation("Fetching job offers from Tunisie Freelance API...");
            
            var response = await _httpClient.GetFromJsonAsync<TunisieFreelanceResponse>(apiUrl, cancellationToken);
            
            if (response == null || response.Jobs == null || !response.Jobs.Any())
            {
                _logger.LogWarning("No jobs found in API response.");
                return Enumerable.Empty<JobOffer>();
            }

            var offers = response.Jobs.Select(job => new JobOffer
            {
                Id = job.Id,
                Title = job.Title,
                Link = $"https://tunisiefreelance.tn/fr/jobs/{job.Slug}",
                DiscoveredAt = DateTime.UtcNow
            }).ToList();

            _logger.LogInformation("Successfully fetched {Count} job offers from API.", offers.Count);
            return offers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch or deserialize job API response.");
            throw;
        }
    }
}
