using Scryfall.Models;
using Scryfall;

namespace ManaBoxBulkImport;

internal static class Program
{
    private const string ManaBoxPath = "ManaBox";
    private const string UserAgent = "ManaBox Bulk Importer";

    private static int Main(string[] _)
    {
        Console.WriteLine("ManaBox Bulk Importer v.1.0.0");

        var oneDrivePath = Environment.GetEnvironmentVariable("OneDriveConsumer");
        if (string.IsNullOrEmpty(oneDrivePath))
        {
            Console.WriteLine("Requires OneDrive.");
            return -1;
        }

        ScryfallClient.SetUserAgent(UserAgent);

        Console.WriteLine("Downloading sets.");
        Dictionary<string, CardSet>? setDefinitions = null;
        try
        {
            setDefinitions = ScryfallClient.GetSets();
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

        File.WriteAllLines(fileName, [Card.GetManaBoxHeader()]);
        File.WriteAllLines(backupFileName, [Card.GetManaBoxHeader()]);

        var allCards = new Dictionary<string, Card>();

        while (true)
        {
            if (!TryGetCardSet(setDefinitions, out var selectedSet) || selectedSet == null)
            {
                break;
            }

            while (TryGetCard(selectedSet, out var card))
            {
                if (card != null)
                {
                    File.AppendAllLines(fileName + ".backup", [card.GetManaBoxString()]);

                    if (allCards.TryGetValue(card.GetKey(), out var existing))
                    {
                        ++existing.Count;
                        Console.WriteLine($"Quantity updated to {existing.Count}");
                    }
                    else
                    {
                        allCards.Add(card.GetKey(), card);
                    }
                }
            }
        }

        if (allCards.Count > 0)
        {
            Console.WriteLine(
                $"Writing {allCards.Sum(c => c.Value.Count)} total and {allCards.Count} unique cards to {fileName}");
            File.AppendAllLines(fileName,
                allCards.Values.OrderBy(c => c.CardDefinition.Name).Select(c => c.GetManaBoxString()));
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

    private static bool TryGetCard(CardSet cardSet, out Card? card)
    {
        while (true)
        {
            Console.Write($"Enter card ID (add 'F' for foil) [{cardSet.Code}]: ");
            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
            {
                card = null;
                return false;
            }

            var isFoil = input.EndsWith('f') || input.EndsWith('F');
            if (isFoil)
            {
                input = input[..^1].Trim();
            }

            CardDefinition? cardDefinition;
            try
            {
                cardDefinition = ScryfallClient.GetCardDefinition(cardSet, input);
            }
            catch
            {
                Console.WriteLine("Card not found.");
                continue;
            }

            if (cardDefinition != null)
            {
                isFoil = GetFixedFoil(cardDefinition, isFoil);

                card = new Card(cardDefinition, isFoil);

                Console.WriteLine(card.GetManaBoxString());

                var nonFoilPrice = cardDefinition.Prices.Usd != null ? $"${cardDefinition.Prices.Usd}" : "n/a";
                var foilPrice = cardDefinition.Prices.UsdFoil != null ? $"${cardDefinition.Prices.UsdFoil}" : "n/a";
                Console.WriteLine($"Pricing: non-foil {nonFoilPrice} foil {foilPrice}");

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