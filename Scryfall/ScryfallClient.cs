using Scryfall.Models;
using System.Net.Http.Json;

namespace Scryfall;

public static class ScryfallClient
{
    private static readonly Uri ScryfallUri = new("https://api.scryfall.com/");

    private static readonly HttpClient Client = new()
    {
        BaseAddress = ScryfallUri
    };

    public static void SetUserAgent(string userAgent)
    {
        Client.DefaultRequestHeaders.Add("User-Agent", userAgent);
    }

    public static Dictionary<string, CardSet>? GetSets()
    {
        return GetSetsAsync().Result;
    }

    public static async Task<Dictionary<string, CardSet>?> GetSetsAsync()
    {
        var results = await Client.GetFromJsonAsync<Results<CardSet>>("/sets");

        return results?.Data.ToDictionary(set => set.Code, set => set, StringComparer.InvariantCultureIgnoreCase);
    }

    public static CardDefinition? GetCardDefinition(CardSet cardSet, string number)
    {
        return GetCardDefinitionAsync(cardSet, number).Result;
    }

    public static Task<CardDefinition?> GetCardDefinitionAsync(CardSet cardSet, string number)
    {
        return Client.GetFromJsonAsync<CardDefinition>($"/cards/{cardSet.Code}/{number}");
    }
}