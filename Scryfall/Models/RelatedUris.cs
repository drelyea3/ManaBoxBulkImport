using System.Text.Json.Serialization;

namespace Scryfall.Models;

public class RelatedUris
{
    [JsonPropertyName("gatherer")] public string Gatherer { get; set; }

    [JsonPropertyName("tcgplayer_infinite_articles")]
    public string TcgplayerInfiniteArticles { get; set; }

    [JsonPropertyName("tcgplayer_infinite_decks")]
    public string TcgplayerInfiniteDecks { get; set; }

    [JsonPropertyName("edhrec")] public string Edhrec { get; set; }
}