using System.Text.Json.Serialization;

namespace Scryfall.Models;

public class PurchaseUris
{
    [JsonPropertyName("tcgplayer")] public string Tcgplayer { get; set; }

    [JsonPropertyName("cardmarket")] public string Cardmarket { get; set; }

    [JsonPropertyName("cardhoarder")] public string Cardhoarder { get; set; }
}