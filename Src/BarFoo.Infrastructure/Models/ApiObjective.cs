using System.Text.Json.Serialization;

using BarFoo.Infrastructure.Converters;

namespace BarFoo.Infrastructure.Models;

[JsonConverter(typeof(ApiObjectiveConverter))]
public class ApiObjective
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("track")]
    public string Track { get; set; } = string.Empty;

    [JsonPropertyName("acclaim")]
    public int Acclaim { get; set; }

    [JsonPropertyName("progress_current")]
    public int ProgressCurrent { get; set; }

    [JsonPropertyName("progress_complete")]
    public int ProgressComplete { get; set; }

    [JsonPropertyName("claimed")]
    public bool Claimed { get; set; }
}
