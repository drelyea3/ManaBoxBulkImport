using Scryfall.Models;

namespace Scryfall;

public class Card
{
    public CardDefinition CardDefinition { get; }

    public int Count { get; set; } = 1;
    public bool IsFoil { get; }
    public string Language { get; }
    public string Condition { get; }

    public string IsFoilText => IsFoil ? "foil" : "normal";

    public string GetKey()
    {
        return $"\"{CardDefinition.Name}\",{CardDefinition.Set},{IsFoilText},{CardDefinition.CollectorNumber},{Language},{Condition}";
    }

    public string GetManaBoxString()
    {
        return $"\"{CardDefinition.Name}\",{CardDefinition.Set},{Count},{IsFoilText},{CardDefinition.CollectorNumber},{Language},{Condition}";
    }

    public static string GetManaBoxHeader()
    {
        return "Name,Set code,Quantity,Foil,Card number,Language,Condition";
    }

    public Card(CardDefinition cardDefinition, bool isFoil, string language, string condition, int count)
    {
        CardDefinition = cardDefinition;
        IsFoil = isFoil;
        Language = language;
        Condition = condition;
        Count = count;
    }
}