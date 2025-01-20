using System.Text.Json;
using System.Text.Json.Serialization;

namespace BarFoo.Infrastructure.Converters;

/// <summary>
/// Base converter class that enforces strict JSON validation for Guild Wars 2 API responses
/// </summary>
public abstract class StrictGuildWars2ApiJsonConverterBase<T> : JsonConverter<T> where T : class, new()
{
    protected abstract Dictionary<string, (Type Type, bool IsArray, bool IsRequired)> AllowedProperties { get; }

    private static readonly string[] _separator = ["_"];

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected start of object, but got {reader.TokenType}");
        }

        var result = new T();
        var seenProperties = new HashSet<string>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                CheckAllPropertiesPresent(seenProperties, typeToConvert);
                return result;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"Expected property name, but got {reader.TokenType}");
            }

            string propertyName = reader.GetString();

            if (!AllowedProperties.TryGetValue(propertyName, out var propertyInfo))
            {
                throw new JsonException($"Unknown property: {propertyName}");
            }

            if (seenProperties.Contains(propertyName))
            {
                throw new JsonException($"Duplicate property: {propertyName}");
            }

            seenProperties.Add(propertyName);

            reader.Read();

            if (propertyInfo.IsArray)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw new JsonException($"Expected start of array for property {propertyName}, but got {reader.TokenType}");
                }
                ParseArray(ref reader, result, propertyName, propertyInfo.Type, options);
            }
            else
            {
                ValidateAndSetProperty(ref reader, result, propertyName, propertyInfo.Type);
            }
        }

        throw new JsonException("JSON object was not closed");
    }

    protected virtual void CheckAllPropertiesPresent(HashSet<string> seenProperties, Type typeToConvert)
    {
        var expectedProperties = AllowedProperties.Keys
            .Where(p => typeof(T).GetProperty(ToPascalCase(p)) != null);

        var missingProperties = expectedProperties.Except(seenProperties).ToList();

        if (missingProperties.Count != 0)
        {
            throw new JsonException($"Missing required properties for {typeToConvert.Name}: {string.Join(", ", missingProperties)}");
        }
    }

    protected virtual void ParseArray(ref Utf8JsonReader reader, T result, string propertyName, Type elementType, JsonSerializerOptions options)
    {
        var list = new List<object>();
        bool isEmptyArray = true;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                if (isEmptyArray)
                {
                    throw new JsonException($"The {propertyName} array cannot be empty");
                }

                var prop = typeof(T).GetProperty(ToPascalCase(propertyName));

                if (prop != null)
                {
                    prop.SetValue(result, list);
                }

                return;
            }

            isEmptyArray = false;

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Expected start of object in array {propertyName}, but got {reader.TokenType}");
            }

            var element = JsonSerializer.Deserialize(ref reader, elementType, options);
            list.Add(element);
        }

        throw new JsonException($"Array {propertyName} was not closed");
    }

    protected virtual void ValidateAndSetProperty(ref Utf8JsonReader reader, T result, string propertyName, Type expectedType, bool isRequired = true)
    {
        if (expectedType == typeof(int))
        {
            if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt32(out _))
            {
                throw new JsonException($"Expected integer for property {propertyName}, but got {reader.TokenType}");
            }
        }
        else if (expectedType == typeof(bool))
        {
            if (reader.TokenType != JsonTokenType.True && reader.TokenType != JsonTokenType.False)
            {
                throw new JsonException($"Expected boolean for property {propertyName}, but got {reader.TokenType}");
            }
        }
        else if (expectedType == typeof(string))
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Expected string for property {propertyName}, but got {reader.TokenType}");
            }
        }

        var prop = typeof(T).GetProperty(ToPascalCase(propertyName));
        if (prop != null)
        {
            var value = JsonSerializer.Deserialize(ref reader, prop.PropertyType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            prop.SetValue(result, value);
        }
    }

    protected static string ToPascalCase(string s)
    {
        string[] parts = s.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i][1..];
        }
        return string.Join("", parts);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}