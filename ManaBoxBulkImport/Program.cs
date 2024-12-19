using Scryfall.Models;
using Scryfall;

namespace ManaBoxBulkImport;

internal static class Program
{
    private static bool isFake = true;

    private const string ManaBoxPath = "ManaBox";
    private const string UserAgent = "ManaBox Bulk Importer";

    private static int Main(string[] args)
    {
        isFake = args.Length > 10;

        Console.WriteLine("ManaBox Bulk Importer 1.0.0");

        var oneDrivePath = Environment.GetEnvironmentVariable("OneDriveConsumer");
        if (string.IsNullOrEmpty(oneDrivePath))
        {
            Console.WriteLine("Requires OneDrive.");
            return -1;
        }

        var client = new ScryfallClient(UserAgent);

        var bulkData = client.GetBulkData().First(bd => bd.Name == "Default Cards");

        var defaultCards = client.GetBulkData(bulkData);

        var cardCache = defaultCards.GroupBy(cd => cd.Set).ToDictionary(g => g.Key, g => g.ToDictionary(gg => gg.CollectorNumber, gg => gg));
        int bad = 0;
        int good = 0;

        foreach (var card in defaultCards)
        {
            if (!int.TryParse(card.CollectorNumber, out var n))
            {
                ++bad;
                Console.WriteLine($"Failed to parse '{card.CollectorNumber}' {card.SetName}");
            }
            else
            {
                ++good;
            }
        }

        Console.WriteLine($"Bad: {bad} Good: {good}");

        Console.WriteLine("Downloading sets.");
        Dictionary<string, CardSet>? setDefinitions = null;
        try
        {
            setDefinitions = client.GetSets();
        }
        catch
        {
            // ignored
        }

        if (setDefinitions is null)
        {
            Console.WriteLine("Downloading sets failed.");
            return -2;
        }

        var fileName = GetFileName(oneDrivePath);
        var backupFileName = $"{fileName}.backup";

        Console.WriteLine($"Writing data to {fileName}");

        if (!isFake)
        {
            File.WriteAllLines(fileName, [Card.GetManaBoxHeader()]);
            File.WriteAllLines(backupFileName, [Card.GetManaBoxHeader()]);
        }

        var allCards = new Dictionary<string, Card>();

        Dictionary<string, CardDefinition>? selectedCardSet;
        while (true)
        {
            if (!TryGetCardSet(setDefinitions, out var selectedSet) || selectedSet == null)
            {
                break;
            }
            else if (!cardCache.TryGetValue(selectedSet.Code, out selectedCardSet))
            {
                Console.WriteLine($"Loading cards for {selectedSet.Code} {selectedSet.Name}.");
                var cards = client.GetCards(selectedSet).Result;
                Console.WriteLine($"Loaded {cards.Count} cards.");
                selectedCardSet = cards.ToDictionary(c => c.CollectorNumber, c => c);
                cardCache[selectedSet.Code] = selectedCardSet;
            }

            while (TryGetCard(selectedSet.Code, selectedCardSet, out var card))
            {
                if (card != null)
                {
                    if (!isFake)
                    { 
                        File.AppendAllLines(fileName + ".backup", [card.GetManaBoxString()]); 
                    }

                    if (allCards.TryGetValue(card.GetKey(), out var existing))
                    {
                        existing.Count += card.Count;
                        Console.WriteLine($"Quantity updated to {existing.Count}.");
                    }
                    else
                    {
                        allCards.Add(card.GetKey(), card);
                        if (card.Count > 1)
                        {
                            Console.WriteLine($"Quantity {card.Count}.");
                        }
                    }
                }
            }
        }

        if (allCards.Count > 0)
        {
            Console.WriteLine(
                $"Writing {allCards.Sum(c => c.Value.Count)} total and {allCards.Count} unique cards to {fileName}");
            if (!isFake)
            {
                File.AppendAllLines(fileName,
                allCards.Values.OrderBy(c => c.CardDefinition.Name).Select(c => c.GetManaBoxString()));
            }
        }

        return 0;
    }

    private static bool TryGetCardSet(Dictionary<string, CardSet> setDefinitions, out CardSet? cardSet)
    {
        while (true)
        {
            Console.Write("Enter set ID: ");
            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
            {
                cardSet = null;
                return false;
            }

            var code = input.Trim();

            if (setDefinitions.TryGetValue(code, out cardSet))
            {
                Console.WriteLine($"{cardSet.Code}\t{cardSet.Name}");
                return true;
            }

            Console.WriteLine("Set not found.");
        }
    }

    private static bool TryGetCard(string setCode, Dictionary<string, CardDefinition> cardSet, out Card? card)
    {
        while (true)
        {
            Console.Write($"Enter card ID (? for help) [{setCode}]: ");
            var input = Console.ReadLine();

            if (input?.Trim() == "?")
            {
                Console.WriteLine("<code>[flags]");
                Console.WriteLine("\tFlags are not case-sensitive and can be in any order.");
                Console.WriteLine("\tf\tFoil");
                Console.WriteLine("\tj\tJapanese");
                Console.WriteLine("\t<N>\tCount (default count is 1)");
                Console.WriteLine("Examples:");
                Console.WriteLine("272 272,13 272 2f 272jf 272jf2");
                Console.WriteLine();
                Console.WriteLine("Enter a blank line to return to selecting card set.");
                Console.WriteLine();
                continue;
            }
            if (string.IsNullOrEmpty(input))
            {
                card = null;
                return false;
            }

            InputParser.Parse(input, out var set, out var code, out var isJapanese, out var isFoil, out var count);

            if (set != null)
            {

            }

            if (!cardSet.TryGetValue(code, out var cardDefinition))
            {
                Console.WriteLine("Card not found.");
                continue;
            }

            if (cardDefinition != null)
            {
                isFoil = GetFixedFoil(cardDefinition, isFoil);

                card = new Card(cardDefinition, isFoil, isJapanese, count);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{card.CardDefinition.CollectorNumber} {card.CardDefinition.Name}");
                Console.ResetColor();
                Console.WriteLine(card.GetManaBoxString());

                var nonFoilPrice = cardDefinition.Prices.Usd != null ? $"${cardDefinition.Prices.Usd}" : "n/a";
                var foilPrice = cardDefinition.Prices.UsdFoil != null ? $"${cardDefinition.Prices.UsdFoil}" : "n/a";

                Console.Write("Pricing: non-foil ");
                if (!isFoil)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                Console.Write(nonFoilPrice);
                Console.ResetColor();

                Console.Write(" foil ");
                if (isFoil)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                Console.WriteLine(foilPrice);
                Console.ResetColor();

                while (true)
                {
                    Console.Write("Correct (Y/N)?");
                    var key = Console.ReadKey();
                    Console.WriteLine();

                    if (key.Key == ConsoleKey.Y)
                    {
                        Console.WriteLine();
                        return true;
                    }

                    if (key.Key == ConsoleKey.N)
                    {
                        Console.WriteLine();
                        break;
                    }
                }
            }
            else
            {
                card = null;
                return false;
            }
        }
    }

    private static bool GetFixedFoil(CardDefinition cardDefinition, bool isFoil)
    {
        switch (isFoil)
        {
            case true when !cardDefinition.Foil:
                Console.WriteLine("Card is not available in foil");
                return false;
            case false when !cardDefinition.Nonfoil:
                Console.WriteLine("Card is only available in foil");
                return true;
            default:
                return isFoil;
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
}
