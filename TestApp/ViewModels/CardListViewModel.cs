using Csv;
using Scryfall;
using Scryfall.Models;
using System.IO;

namespace TestApp.ViewModels;

public class CardListViewModel : ViewModelBase
{
    public List<Card> Cards { get; } = new List<Card>();

    public void Load(Dictionary<string, Dictionary<string, CardDefinition>> cache, string csvFileName)
    {
        using var stream = new FileStream(csvFileName, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(stream);

        foreach (var line in CsvReader.ReadFromStream(stream))
        {
            var setCode = line[3];
            var collectorId = line[5];

            if (cache.TryGetValue(setCode, out var cardsInSet))
            {
                if (cardsInSet.TryGetValue(collectorId, out var cardDefinition))
                {
                    var card = new Card(cardDefinition, false, "en", "near_mint", 1);
                    Cards.Add(card);
                }
            }
        }
    }
}