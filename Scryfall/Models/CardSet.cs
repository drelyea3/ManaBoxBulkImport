using System.Text.Json.Serialization;

namespace Scryfall.Models;

public class CardSet
{
    [JsonPropertyName("object")] public string Object { get; set; }

    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("code")] public string Code { get; set; }

    [JsonPropertyName("mtgo_code")] public string MtgoCode { get; set; }

    [JsonPropertyName("arena_code")] public string ArenaCode { get; set; }

    [JsonPropertyName("tcgplayer_id")] public int TcgplayerId { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("uri")] public Uri Uri { get; set; }

    [JsonPropertyName("scryfall_uri")] public Uri ScryfallUri { get; set; }

    [JsonPropertyName("search_uri")] public Uri SearchUri { get; set; }

    [JsonPropertyName("released_at")] public DateOnly ReleasedAt { get; set; }

    [JsonPropertyName("set_type")] public string SetType { get; set; }

    [JsonPropertyName("card_count")] public int CardCount { get; set; }

    [JsonPropertyName("digital")] public bool Digital { get; set; }

    [JsonPropertyName("nonfoil_only")] public bool NonfoilOnly { get; set; }

    [JsonPropertyName("foil_only")] public bool FoilOnly { get; set; }

    [JsonPropertyName("icon_svg_uri")] public Uri IconSvgUri { get; set; }
}