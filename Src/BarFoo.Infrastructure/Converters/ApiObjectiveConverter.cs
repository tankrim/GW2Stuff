using System.Text.Json;

using BarFoo.Infrastructure.Models;

namespace BarFoo.Infrastructure.Converters;

/// <summary>
/// Converts JSON into ApiObjective objects with strict property validation
/// </summary>
public class ApiObjectiveConverter : StrictGuildWars2ApiJsonConverterBase<ApiObjective>
{
    protected override Dictionary<string, (Type Type, bool IsArray, bool IsRequired)> AllowedProperties => new()
        {
            {"id", (typeof(int), false, true)},
            {"title", (typeof(string), false, true)},
            {"track", (typeof(string), false, true)},
            {"acclaim", (typeof(int), false, true)},
            {"progress_current", (typeof(int), false, true)},
            {"progress_complete", (typeof(int), false, true)},
            {"claimed", (typeof(bool), false, true)}
        };

    public override ApiObjective Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) //
    {
        return base.Read(ref reader, typeToConvert, options);
    }
}
