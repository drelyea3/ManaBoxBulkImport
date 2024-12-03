using System.Text.Json.Serialization;

namespace Scryfall.Models;

// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public class AllPart
{
    [JsonPropertyName("object")] public string Object { get; set; }

    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("component")] public string Component { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("type_line")] public string TypeLine { get; set; }

    [JsonPropertyName("uri")] public string Uri { get; set; }
}