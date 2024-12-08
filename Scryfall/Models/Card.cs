namespace Scryfall.Models;

public class Card
{
    public CardDefinition CardDefinition { get; }

    public int Count { get; set; } = 1;
    public bool IsFoil { get; }
    public string Language => "en";
    public string Condition => "near_mint";

    public string IsFoilText => IsFoil ? "foil" : "normal";

    public string GetKey()
    {
        return $"\"{CardDefinition.Name}\",{CardDefinition.Set},{IsFoilText},{CardDefinition.CollectorNumber},{Language},{Condition}";
    }

    public string GetManaBoxString()
    {
        return
            $"\"{CardDefinition.Name}\",{CardDefinition.Set},{Count},{IsFoilText},{CardDefinition.CollectorNumber},{Language},{Condition}";
    }

    public static string GetManaBoxHeader()
    {
        return "Name,Set code,Quantity,Foil,Card number,Language,Condition";
    }

    public Card(CardDefinition cardDefinition, bool isFoil)
    {
        CardDefinition = cardDefinition;
        IsFoil = isFoil;
    }
}