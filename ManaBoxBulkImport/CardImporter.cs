using Scryfall;
using Scryfall.Models;
using System.Diagnostics.CodeAnalysis;

namespace ManaBoxBulkImport;

public class CardImporter(bool isFake)
{
    private const string ManaBoxPath = "ManaBox";
    private const string UserAgent = "ManaBox Bulk Importer";

    private bool _isFake = isFake;

    private ScryfallClient? _client;
    private Dictionary<string, Card> _allCards = [];
    private Dictionary<string, Dictionary<string, CardDefinition>> _cardCache = [];
    [NotNull]
    private Dictionary<string, CardSet>? _setDefinitions;
    private CardSet? _selectedCardSet = null;

    [NotNull]
    private string? _fileName;
    [NotNull]
    private string? _backupFileName;

    private bool Initialize()
    {
        var oneDrivePath = Environment.GetEnvironmentVariable("OneDriveConsumer");
        if (string.IsNullOrEmpty(oneDrivePath))
        {
            Console.WriteLine("Requires OneDrive.");
            return false;
        }

        _client = new ScryfallClient(UserAgent);

        var bulkData = _client.GetBulkData().First(bd => bd.Name == "Default Cards");

        var defaultCards = _client.GetBulkData(bulkData);

        _cardCache = defaultCards.GroupBy(cd => cd.Set).ToDictionary(g => g.Key, g => g.ToDictionary(gg => gg.CollectorNumber, gg => gg));

        Console.WriteLine("Downloading sets.");

        try
        {
            _setDefinitions = _client.GetSets() ?? [];
        }
        catch
        {
            // ignored
        }

        if (_setDefinitions is null)
        {
            Console.WriteLine("Downloading sets failed.");
            return false;
        }

        _fileName = GetFileName(oneDrivePath);
        _backupFileName = $"{_fileName}.backup";

        Console.WriteLine($"Writing data to {_fileName}");


        return true;
    }

    public void Import()
    {
        if (!Initialize())
        {
            return;
        }

        _selectedCardSet = null;

        while (true)
        {
            CardSpec? cardSpec = null;
            try
            {
                Console.WriteLine();
                var result = Prompt(_selectedCardSet, out cardSpec, out var exit);
                if (!result)
                {
                    continue;
                }

                if (exit)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (cardSpec == null)
            {
                continue;
            }

            if (cardSpec.SetId == null && _selectedCardSet == null)
            {
                Console.WriteLine("Set code and card ID required, e.g., FDN:240");
                continue;
            }

            if (cardSpec.SetId != null)
            {
                if (_setDefinitions.TryGetValue(cardSpec.SetId, out var newSetDefinition) && newSetDefinition != null)
                {
                    _selectedCardSet = newSetDefinition;
                    if (string.IsNullOrEmpty(cardSpec.CollectorId))
                    {
                        Console.WriteLine(_selectedCardSet.GetOutputString());
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine("Set not found.");
                    continue;
                }
            }

            if (!_cardCache[_selectedCardSet!.Code].TryGetValue(cardSpec.CollectorId, out var cardDef))
            {
                Console.WriteLine("Card not found.");
                continue;
            }

            var card = cardSpec.CreateCard(cardDef!);

            if (card != null)
            {
                PrintCardInformation(_selectedCardSet, card);

                if (!Confirm())
                {
                    continue;
                }

                if (!_isFake)
                {
                    if (_allCards.Count == 0)
                    {
                        File.WriteAllLines(_backupFileName, [Card.GetManaBoxHeader()]);
                    }
                    File.AppendAllLines(_fileName + ".backup", [card.GetManaBoxString()]);
                }

                if (_allCards.TryGetValue(card.GetKey(), out var existing))
                {
                    existing.Count += card.Count;
                    Console.WriteLine($"Quantity updated to {existing.Count}.");
                }
                else
                {
                    _allCards.Add(card.GetKey(), card);
                    if (card.Count > 1)
                    {
                        Console.WriteLine($"Quantity {card.Count}.");
                    }
                }
            }
        }
    }

    private static string GetFileName(string basePath)
    {
        var serial = 0;
        var path = Path.Join(basePath, ManaBoxPath);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        while (true)
        {
            serial++;
            var fileName = $"\\MTG{serial:D4}.csv";
            var filePath = Path.Join(path, fileName);
            if (!File.Exists(filePath))
            {
                return filePath;
            }
        }
    }

    private static void PrintCardInformation(CardSet set, Card card)
    {
        var cardDefinition = card.CardDefinition;
            
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{cardDefinition.CollectorNumber} {cardDefinition.Name}");
        Console.ResetColor();
        Console.WriteLine(set.GetOutputString());
        Console.WriteLine(card.GetManaBoxString());

        var nonFoilPrice = cardDefinition.Prices.Usd != null ? $"${cardDefinition.Prices.Usd}" : "n/a";
        var foilPrice = cardDefinition.Prices.UsdFoil != null ? $"${cardDefinition.Prices.UsdFoil}" : "n/a";

        Console.Write("Pricing (NM): non-foil ");
        if (!card.IsFoil)
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }

        Console.Write(nonFoilPrice);
        Console.ResetColor();

        Console.Write(" foil ");
        if (card.IsFoil)
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
        Console.WriteLine(foilPrice);
        Console.ResetColor();
    }

    private bool Prompt(CardSet? cardSet, out CardSpec? cardSpec, out bool exit)
    {
        cardSpec = null;
        exit = false;

        if (cardSet == null)
        {
            Console.Write("Enter card (? for help): ");
        }
        else
        {
            Console.Write($"Enter card (? for help) [{cardSet.Code}]: ");
        }

        var userInput = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(userInput))
        {
            return false;
        }

        if (userInput.StartsWith('.'))
        {
            return ProcessCommand(userInput[1..], out exit);
        }

        return CardSpec.TryCreate(userInput, out cardSpec);
    }

    private static bool Confirm()
    {
        Console.Write("Correct (Y/N)?");

        while (true)
        {
            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Y)
            {
                Console.WriteLine(key.KeyChar);
                return true;
            }

            if (key.Key == ConsoleKey.N)
            {
                Console.WriteLine(key.KeyChar);
                return false;
            }

            Console.Beep();
        }
    }

    private bool ProcessCommand(string command, out bool exit)
    {
        exit = false;

        var index = command.IndexOf(' ');

        var cmd = index == -1 ? command.ToUpper() : command[..index].ToUpper();
        var text = index == -1 ? null : command[(index + 1)..].Trim();

        switch (cmd)
        {
            case "LC":
                ListCardsByName(text);
                return true;

            case "LO":
                ListSimilarCards(_selectedCardSet, text);
                return true;

            case "LS":
                ListSets(text);
                return true;

            case "LSC":
                ListCardsBySet(_selectedCardSet, text);
                return true;

            case "Q":
                SaveAndQuit();
                exit = true;
                return true;

            default:
                Console.WriteLine("Unknown command.");
                return false;
        }
    }

    private void ListCardsBySet(CardSet? set, string? text)
    {
        if (set == null)
        {
            Console.WriteLine("No set selected.");
            return;
        }

        var source = text == null
            ? _cardCache[set.Code].Values
            : _cardCache[set.Code].Values.Where(c => c.Name.Contains(text, StringComparison.OrdinalIgnoreCase));

        foreach (var card in source.OrderBy(c => c.Name).ThenBy(c => c.CollectorNumber))
        {
            Console.WriteLine($"{card.CollectorNumber}\t{card.Name}");
        }
    }

    private void ListSets(string? substring)
    {
        var source = string.IsNullOrWhiteSpace(substring)
            ? _setDefinitions.Values.OrderBy(s => s.Name)
            : _setDefinitions.Values.Where(s => s.Name.Contains(substring, StringComparison.InvariantCultureIgnoreCase) || s.Code.Contains(substring, StringComparison.InvariantCultureIgnoreCase));

        foreach (var set in source.OrderBy(s => s.Name))
        {
            Console.WriteLine(set.GetOutputString());
        }
    }

    private void ListCardsByName(string? substring)
    {
        var source = string.IsNullOrWhiteSpace(substring)
            ? _cardCache.SelectMany(s => s.Value)
            : _cardCache.SelectMany(s => s.Value).Where(c => c.Value.Name.Contains(substring, StringComparison.InvariantCultureIgnoreCase));

        foreach (var card in source.Select(kvp => kvp.Value).OrderBy(c => c.Name).ThenBy(c => c.Set))
        {
            Console.WriteLine($"{card.Set}\t{card.CollectorNumber}\t{card.Name}");
        }
    }

    private void ListSimilarCards(CardSet? set, string? cardId)
    {
        if (set == null)
        {
            Console.WriteLine("No set selected");
            return;
        }

        if (cardId == null)
        {
            Console.WriteLine("No card selected");
            return;
        }
        
        if (!_cardCache[set.Code].TryGetValue(cardId, out var card) || card == null)
        {
            Console.WriteLine("Card not found");
        }

        var source = _cardCache.SelectMany(s => s.Value).Where(c => c.Value.OracleId == card.OracleId);

        foreach (var c in source.Select(kvp => kvp.Value).OrderBy(c => c.Name).ThenBy(c => c.Set))
        {
            Console.WriteLine($"{c.Set}\t{c.CollectorNumber}\t{c.Name}");
        }
    }

    private void SaveAndQuit()
    {
        if (_allCards.Count > 0)
        {
            Console.WriteLine($"Writing {_allCards.Sum(c => c.Value.Count)} total and {_allCards.Count} unique cards to {_fileName}");
            if (!_isFake)
            {
                File.WriteAllLines(_fileName, [Card.GetManaBoxHeader()]);
                File.AppendAllLines(_fileName, _allCards.Values.OrderBy(c => c.CardDefinition.Name).Select(c => c.GetManaBoxString()));
            }
        }
    }
}