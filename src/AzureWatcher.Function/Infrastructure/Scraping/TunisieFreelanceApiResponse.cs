using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AzureWatcher.Function.Infrastructure.Scraping;

public class TunisieFreelanceApiResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("jobs")]
    public List<ApiJobEntity> Jobs { get; set; } = new();
}

public class ApiJobEntity
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
