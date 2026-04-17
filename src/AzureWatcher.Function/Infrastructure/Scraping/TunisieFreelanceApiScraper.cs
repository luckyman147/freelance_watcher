using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AzureWatcher.Function.Domain.Entities;
using AzureWatcher.Function.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;

namespace AzureWatcher.Function.Infrastructure.Scraping;

/// <summary>
/// Scrapes job offers by parsing the HTML search page of tunisiefreelance.tn.
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
        var targetUrl = _configuration["TargetWebsiteUrl"];
        if (string.IsNullOrEmpty(targetUrl))
            throw new InvalidOperationException("TargetWebsiteUrl is missing in configuration.");

        try
        {
            _logger.LogInformation("Fetching job offers from: {TargetUrl}", targetUrl);
            
            var html = await _httpClient.GetStringAsync(targetUrl, cancellationToken);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Select all <article> elements which represent job items
            var jobNodes = doc.DocumentNode.SelectNodes("//article");
            
            if (jobNodes == null || !jobNodes.Any())
            {
                _logger.LogWarning("No job articles found on the page.");
                return Enumerable.Empty<JobOffer>();
            }

            var offers = new List<JobOffer>();
            foreach (var node in jobNodes)
            {
                try
                {
                    // Find the title and link anchor
                    var titleAnchor = node.SelectSingleNode(".//h3/a") 
                                      ?? node.SelectSingleNode(".//a[contains(@class, 'text-primary')]");
                    
                    if (titleAnchor == null) continue;

                    var title = titleAnchor.InnerText.Trim();
                    var relativeLink = titleAnchor.GetAttributeValue("href", string.Empty);
                    
                    if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(relativeLink)) continue;

                    var absoluteLink = relativeLink.StartsWith("/") 
                        ? $"https://tunisiefreelance.tn{relativeLink}" 
                        : relativeLink;

                    // Generate a deterministic ID based on the URL
                    var id = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(absoluteLink));

                    offers.Add(new JobOffer
                    {
                        Id = id,
                        Title = title,
                        Link = absoluteLink,
                        DiscoveredAt = DateTime.UtcNow
                    });
                }
                catch (Exception nodeEx)
                {
                    _logger.LogWarning(nodeEx, "Failed to parse a specific job node.");
                }
            }

            _logger.LogInformation("Successfully parsed {Count} job offers from HTML.", offers.Count);
            return offers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch or parse job search page.");
            throw;
        }
    }
}
