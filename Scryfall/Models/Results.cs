using System.Text.Json.Serialization;

namespace Scryfall.Models;

public class Results<T>
{
    [JsonPropertyName("object")] 
    public string Object { get; set; }

    [JsonPropertyName("total_cards")] 
    public int TotalCards { get; set; }

    [JsonPropertyName("has_more")] 
    public bool HasMore { get; set; }

    [JsonPropertyName("next_page")] 
    public Uri? NextPage { get; set; }

    [JsonPropertyName("data")] 
    public List<T> Data { get; set; }
}