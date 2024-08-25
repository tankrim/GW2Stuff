using System.Text.Json.Serialization;

using BarFoo.Infrastructure.Converters;

namespace BarFoo.Infrastructure.Models;

[JsonConverter(typeof(ApiResponseConverter))]
public class ApiResponse
{
    public int MetaProgressCurrent { get; set; }
    public int MetaProgressComplete { get; set; }
    public int MetaRewardItemId { get; set; }
    public int MetaRewardAstral { get; set; }
    public bool MetaRewardClaimed { get; set; }
    public List<ApiObjective> Objectives { get; set; } = new List<ApiObjective>();

    public bool HasMetaData { get; set; }
}