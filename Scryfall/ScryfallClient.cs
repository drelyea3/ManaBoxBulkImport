using Scryfall.Models;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;

namespace Scryfall;

public class ScryfallClient
{
    private const string ScryfallUriText = "https://api.scryfall.com/";

    private static readonly Uri ScryfallUri = new(ScryfallUriText);

    private readonly string _userAgent;

    [NotNull]
    private HttpClient? _client;

    private HttpClient Client => _client ??= _client ??= CreateClient(ScryfallUri);

    public ScryfallClient(string userAgent)
    {
        _userAgent = userAgent;
    }

    private HttpClient CreateClient(Uri baseUri)
    {
        var client = new HttpClient();
        client.BaseAddress = baseUri;
        client.DefaultRequestHeaders.Add("User-Agent", _userAgent);

        return client;
    }
    public List<BulkData> GetBulkData()
    {
        return GetBulkDataAsync().Result;
    }

    public async Task<List<BulkData>> GetBulkDataAsync()
    {
        var results = await TryGetJsonAsync<BulkData>("bulk-data");

        return results ?? [];
    }

    public List<CardDefinition> GetBulkData(BulkData bulkData)
    {
        if (File.Exists("Default_cards.json"))
        {
            Console.WriteLine("Reading card definitions from cache.");
            using var jsonStream = new FileStream("Default_cards.json", FileMode.Open, FileAccess.Read);
            
            return JsonSerializer.Deserialize<List<CardDefinition>>(jsonStream) ?? [];
        }
        else
        {
            Console.WriteLine("Downloading card definitions.");
            var json = Client.GetStringAsync(bulkData.DownloadUri.ToString()).Result;

            Console.WriteLine("Writing card definitions to cache.");
            File.WriteAllText("Default_cards.json", json);

            return JsonSerializer.Deserialize<List<CardDefinition>>(json) ?? [];
        }
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
        return TryGetJsonAsync<CardDefinition>($"/cards/search?q=set:{cardSet.Code}+unique:prints+game:paper");
    }

    public async Task<List<Symbology>> GetSymbologyAsync()
    {
        var results = await TryGetJsonAsync<Symbology>("/symbology");

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

    private async Task<List<TItem>> TryGetJsonAsync<TItem>(string requestUri)
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