using Scryfall.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Scryfall;

public class ScryfallClient
{
    private const string ScryfallUriText = "https://api.scryfall.com/";
    private static readonly Uri ScryfallUri = new(ScryfallUriText);

    private readonly HttpClient Client;

    public ScryfallClient(string userAgent)
    {
        Client = new HttpClient();
        Client.BaseAddress = ScryfallUri; 
        Client.DefaultRequestHeaders.Add("User-Agent", userAgent);
    }

    public Dictionary<string, CardSet>? GetSets()
    {
        return GetSetsAsync().Result;
    }

    public async Task<Dictionary<string, CardSet>?> GetSetsAsync()
    {
        var results = await Client.GetFromJsonAsync<Results<CardSet>>("/sets");

        return results?.Data.ToDictionary(set => set.Code, set => set, StringComparer.InvariantCultureIgnoreCase);
    }

    public CardDefinition? GetCardDefinition(CardSet cardSet, string number)
    {
        return GetCardDefinitionAsync(cardSet, number).Result;
    }

    public Task<CardDefinition?> GetCardDefinitionAsync(CardSet cardSet, string number)
    {
        return Client.GetFromJsonAsync<CardDefinition>($"/cards/{cardSet.Code}/{number}");
    }

    public Task<List<CardDefinition>> GetCards(CardSet cardSet)
    {
        return TryGetJson<CardDefinition>($"/cards/search?q=set:{cardSet.Code}+unique:prints+game:paper");
    }

    private async Task<List<TItem>> TryGetJson<TItem>(string requestUri)
       where TItem : class
    {
        var originalRequestUri = requestUri;
        var results = new List<TItem>();
        int page = 1;
        while (true)
        {
            Console.WriteLine(requestUri);
            var response = await Client.GetAsync(requestUri);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var items = JsonSerializer.Deserialize<Results<TItem>>(json);
                
                if (items != null)
                {
                    results.AddRange(items.Data);
                    Console.WriteLine($"Added {items.Data.Count} total {results.Count}");

                    if (items.NextPage != null && items.HasMore)
                    {
                        ++page;
                        requestUri = originalRequestUri + $"&page={page}";
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                break; 
            }
        }

        return results;
    }
}