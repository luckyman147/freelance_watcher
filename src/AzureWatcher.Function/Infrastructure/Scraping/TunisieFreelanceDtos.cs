using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AzureWatcher.Function.Infrastructure.Scraping;

public class TunisieFreelanceResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("jobs")]
    public List<TunisieJobDto> Jobs { get; set; } = new();
}

public class TunisieJobDto
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("openedAt")]
    public DateTime? OpenedAt { get; set; }
}
