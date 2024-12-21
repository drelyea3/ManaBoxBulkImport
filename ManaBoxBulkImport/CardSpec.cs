using Scryfall;
using Scryfall.Models;
using System.Diagnostics.CodeAnalysis;

namespace ManaBoxBulkImport;

public class CardSpec
{
    private static readonly char[] Delimiters = [' ', ','];
    private string? _setId;
    private string _collectorId = string.Empty;
    private int? _count;
    private bool? _isFoil;
    private string? _language;
    private string? _condition;

    public string? SetId => _setId;
    public string CollectorId => _collectorId;
    public int Count => _count ?? 1;
    public bool IsFoil => _isFoil ?? false;
    public string Language => _language ?? "en";
    public string Condition => _condition ?? "near_mint";

    private CardSpec() { }
        
    public Card CreateCard(CardDefinition cardDefinition)
    {
        var fixedIsFoil = GetFixedFoil(cardDefinition, IsFoil);
        var card = new Card(cardDefinition, fixedIsFoil, Language, Condition, Count);
        return card;
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

    private static void SetCondition(CardSpec cardSpec, string condition)
    {
        if (cardSpec._condition is null)
        {
            cardSpec._condition = condition;
        }
        else
        {
            throw new Exception("Already set condition");
        }
    }

    public static bool TryCreate(string text, [NotNullWhen(true)] out CardSpec? cardSpec)
    {
        cardSpec = null;

        var result = new CardSpec();
            
        var parts = text.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return false;
        }

        var colonIndex = parts[0].IndexOf(':');
        if (colonIndex != -1)
        {
            result._setId = parts[0][..colonIndex];
        }

        result._collectorId = parts[0][(colonIndex + 1)..];

        foreach (var part in parts.Skip(1))
        {
            switch (part.ToUpper())
            {
                case "NM":
                    SetCondition(result, "near_mint");
                    break;
                case "EX":
                    SetCondition(result, "excellent");
                    break;
                case "G":
                    SetCondition(result, "good");
                    break;
                case "F":
                    result._isFoil = true;
                    break;
                case "J":
                    result._language = "ja";
                    break;
                default:
                    if (int.TryParse(part, out var count))
                    {
                        if (result._count is null)
                        {
                            if (count < 1)
                            {
                                throw new Exception("Count must be > 0");
                            }

                            result._count = count;
                        }
                    }
                    else
                    { 
                        throw new Exception($"Unknown option '{part}'"); 
                    }
                    break;
            }
        }

        cardSpec = result;
        return true;
    }
}