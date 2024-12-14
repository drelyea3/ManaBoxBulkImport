using System.Text.Json.Serialization;

namespace Scryfall.Models;

public class Symbology
{
    [JsonPropertyName("object")]
    public string ObjectType { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("svg_uri")]
    public Uri SvgUri { get; set; }

    [JsonPropertyName("loose_variant")]
    public object LooseVariant { get; set; }

    [JsonPropertyName("english")]
    public string English { get; set; }

    [JsonPropertyName("transposable")]
    public bool? Transposable { get; set; }

    [JsonPropertyName("represents_mana")]
    public bool? RepresentsMana { get; set; }

    [JsonPropertyName("appears_in_mana_costs")]
    public bool? AppearsInManaCosts { get; set; }

    [JsonPropertyName("mana_value")]
    public double? ManaValue { get; set; }

    [JsonPropertyName("hybrid")]
    public bool? Hybrid { get; set; }

    [JsonPropertyName("phyrexian")]
    public bool? Phyrexian { get; set; }

    [JsonPropertyName("cmc")]
    public double? Cmc { get; set; }

    [JsonPropertyName("funny")]
    public bool? Funny { get; set; }

    [JsonPropertyName("colors")]
    public List<string> Colors { get; set; } = new List<string>();

    [JsonPropertyName("gatherer_alternates")]
    public List<string> GathererAlternates { get; set; } = new List<string>();
}
