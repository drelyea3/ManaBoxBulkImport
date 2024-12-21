using Scryfall.Models;

namespace ManaBoxBulkImport;

public static class Extensions
{
    public static string GetOutputString(this CardSet set)
    {
        return $"{set.Code}\t{set.Name} ({set.ReleasedAt})";
    }
}