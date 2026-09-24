using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticQa.Core.Serialization;

public static class AgenticJsonSerializer
{
    private static readonly JsonSerializerOptions DefaultOptions = CreateDefaultOptions();

    public static JsonSerializerOptions Default => DefaultOptions;

    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, DefaultOptions);

    public static T? Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, DefaultOptions);

    public static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
