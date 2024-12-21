using Scryfall;
using Scryfall.Models;

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

        var allCards = new Dictionary<string, Card>();

        CardSet? selectedCardSet = null;

        while (true)
        {
            CardSpec? cardSpec = null;
            try
            {
                Console.WriteLine();
                var result = Prompt(selectedCardSet, out cardSpec);
                if (!result)
                {
                    continue;
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

            if (cardSpec.SetId == null && selectedCardSet == null)
            {
                Console.WriteLine("Set code and card ID required, e.g., FDN:240");
                continue;
            }

            if (cardSpec.SetId != null)
            {
                if (setDefinitions.TryGetValue(cardSpec.SetId, out var newSetDefinition) && newSetDefinition != null)
                {
                    selectedCardSet = newSetDefinition;
                }
                else
                {
                    Console.WriteLine("Set not found.");
                    continue;
                }
            }

            if (!cardCache[selectedCardSet!.Code].TryGetValue(cardSpec.CollectorId, out var cardDef))
            {
                Console.WriteLine("Card not found.");
                continue;
            }

            var card = cardSpec.CreateCard(cardDef!);

            if (card != null)
            {
                PrintCardInformation(card);

                if (!Confirm())
                {
                    continue;
                }

                if (!isFake)
                {
                    if (allCards.Count == 0)
                    {
                        File.WriteAllLines(backupFileName, [Card.GetManaBoxHeader()]);
                    }
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

        if (allCards.Count > 0)
        {
            Console.WriteLine($"Writing {allCards.Sum(c => c.Value.Count)} total and {allCards.Count} unique cards to {fileName}");
            if (!isFake)
            {
                File.WriteAllLines(fileName, [Card.GetManaBoxHeader()]);
                File.AppendAllLines(fileName, allCards.Values.OrderBy(c => c.CardDefinition.Name).Select(c => c.GetManaBoxString()));
            }
        }

        return 0;
    }

    private static void PrintCardInformation(Card card)
    {
        var cardDefinition = card.CardDefinition;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{cardDefinition.CollectorNumber} {cardDefinition.Name}");
        Console.ResetColor();
        Console.WriteLine(card.GetManaBoxString());

        var nonFoilPrice = cardDefinition.Prices.Usd != null ? $"${cardDefinition.Prices.Usd}" : "n/a";
        var foilPrice = cardDefinition.Prices.UsdFoil != null ? $"${cardDefinition.Prices.UsdFoil}" : "n/a";

        Console.Write("Pricing: non-foil ");
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

    public static bool Prompt(CardSet? cardSet, out CardSpec? cardSpec)
    {
        cardSpec = null;

        if (cardSet == null)
        {
            Console.Write("Enter card (? for help): ");
        }
        else
        {
            Console.Write($"Enter card (? for help) [{cardSet.Code}]: ");
        }

        var userInput = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userInput))
        {
            return false;
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
