using System.Text.Json;
using System.Text.Json.Serialization;

using BarFoo.Infrastructure.Models;

namespace BarFoo.Infrastructure.Converters;

/// <summary>
/// Handles conversion of JSON responses containing objective data
/// </summary>
public class ApiResponseConverter : JsonConverter<ApiResponse>
{
    private static readonly HashSet<string> RequiredMetaProperties = new()
        {
            "meta_progress_current",
            "meta_progress_complete",
            "meta_reward_item_id",
            "meta_reward_astral",
            "meta_reward_claimed"
        };

    public override ApiResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        var response = new ApiResponse();
        var seenProperties = new HashSet<string>();
        var seenMetaProperties = new HashSet<string>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                // Ensure all required properties are present before returning
                ValidateProperties(seenProperties, seenMetaProperties);
                response.HasMetaData = seenMetaProperties.Count > 0;
                return response;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected property name");
            }

            string propertyName = reader.GetString();
            if (seenProperties.Contains(propertyName))
            {
                throw new JsonException($"Duplicate property: {propertyName}");
            }
            seenProperties.Add(propertyName);

            reader.Read();

            switch (propertyName)
            {
                case "meta_progress_current":
                    response.MetaProgressCurrent = reader.GetInt32();
                    seenMetaProperties.Add(propertyName);
                    break;
                case "meta_progress_complete":
                    response.MetaProgressComplete = reader.GetInt32();
                    seenMetaProperties.Add(propertyName);
                    break;
                case "meta_reward_item_id":
                    response.MetaRewardItemId = reader.GetInt32();
                    seenMetaProperties.Add(propertyName);
                    break;
                case "meta_reward_astral":
                    response.MetaRewardAstral = reader.GetInt32();
                    seenMetaProperties.Add(propertyName);
                    break;
                case "meta_reward_claimed":
                    response.MetaRewardClaimed = reader.GetBoolean();
                    seenMetaProperties.Add(propertyName);
                    break;
                case "objectives":
                    response.Objectives = ReadObjectives(ref reader, options);
                    break;
                default:
                    throw new JsonException($"Unexpected property: {propertyName}");
            }
        }

        throw new JsonException("JSON object was not closed");
    }

    private List<ApiObjective> ReadObjectives(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array for objectives");
        }

        var objectives = new List<ApiObjective>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return objectives;
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected start of object for objective");
            }

            var objective = JsonSerializer.Deserialize<ApiObjective>(ref reader, options);
            objectives.Add(objective);
        }

        throw new JsonException("Objectives array was not closed");
    }

    private void ValidateProperties(HashSet<string> seenProperties, HashSet<string> seenMetaProperties)
    {
        if (!seenProperties.Contains("objectives"))
        {
            throw new JsonException("Missing required property: objectives");
        }

        if (seenMetaProperties.Count > 0 && seenMetaProperties.Count != RequiredMetaProperties.Count)
        {
            var missingProperties = RequiredMetaProperties.Except(seenMetaProperties);
            throw new JsonException($"Missing required meta properties: {string.Join(", ", missingProperties)}");
        }
    }

    public override void Write(Utf8JsonWriter writer, ApiResponse value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}