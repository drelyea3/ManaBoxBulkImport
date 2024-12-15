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

    public async Task<List<Symbology>> GetSymbologyAsync()
    {
        var results = await TryGetJson<Symbology>("/symbology");

        return results ?? [];
    }

    public async Task FetchSymbols(string baseOutputDir)
    {
        var symbols = await GetSymbologyAsync();
        var sets = await GetSetsAsync();

        var symUris = symbols.Select(s => s.SvgUri).ToList();
        var setUris = sets!.Select(s => s.Value.IconSvgUri);

        using HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://svgs.scryfall.io");
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Symbol-Scraper");

        await GetSvgAsync(httpClient, symUris, Path.Join(baseOutputDir, "sym"));
        await GetSvgAsync(httpClient, setUris, Path.Join(baseOutputDir, "set"));
    }

    private static async Task GetSvgAsync(HttpClient httpClient, IEnumerable<Uri> svgUris, string dir)
    {
        Directory.CreateDirectory(dir);

        foreach (var svgUri in svgUris)
        {
            var uriPart = httpClient.BaseAddress!.MakeRelativeUri(svgUri);
            var svg = await httpClient.GetStringAsync(uriPart);
            var fn = Path.Join(dir, "_" + Path.GetFileName(svgUri.LocalPath));
            Console.WriteLine(fn);
            File.WriteAllText(fn, svg);
            Task.Delay(100).Wait();
        }
    }

    private async Task<List<TItem>> TryGetJson<TItem>(string requestUri)
        where TItem : class
    {
        var originalRequestUri = requestUri;
        var results = new List<TItem>();
        var page = 1;
        while (true)
        {
            var response = await Client.GetAsync(requestUri);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var items = JsonSerializer.Deserialize<Results<TItem>>(json);

                if (items != null)
                {
                    results.AddRange(items.Data);

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