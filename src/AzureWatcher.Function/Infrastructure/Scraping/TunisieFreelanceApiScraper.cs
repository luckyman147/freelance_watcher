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
/// Scrapes job offers by calling the internal JSON API of tunisiefreelance.tn.
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
        
        // Ensure the API returns JSON
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<IEnumerable<JobOffer>> FetchLatestOffersAsync(CancellationToken cancellationToken = default)
    {
        var targetUrl = _configuration["TargetWebsiteUrl"];
        if (string.IsNullOrEmpty(targetUrl))
            throw new InvalidOperationException("TargetWebsiteUrl is missing in configuration.");

        try
        {
            _logger.LogInformation("Fetching job offers from API: {TargetUrl}", targetUrl);
            
            var response = await _httpClient.GetFromJsonAsync<TunisieFreelanceApiResponse>(targetUrl, cancellationToken);
            
            if (response == null || response.Jobs == null)
            {
                _logger.LogWarning("Got empty response or null jobs from API.");
                return Enumerable.Empty<JobOffer>();
            }

            var offers = response.Jobs.Select(job => new JobOffer
            {
                // Use slug as part of the link and maybe ID
                Id = job.Id ?? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(job.Slug)),
                Title = job.Title,
                Link = $"https://tunisiefreelance.tn/fr/jobs/{job.Slug}",
                DiscoveredAt = DateTime.UtcNow
            }).ToList();

            _logger.LogInformation("Found {Count} job offers in API response.", offers.Count);
            return offers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch or parse API response.");
            throw;
        }
    }
}
