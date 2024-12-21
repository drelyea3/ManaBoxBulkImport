﻿using Scryfall;
using Scryfall.Models;

namespace ManaBoxBulkImport;

internal static class Program
{
    private static int Main(string[] args)
    {
        Console.WriteLine("ManaBox Bulk Importer 1.0.0");

        var importer = new CardImporter(true);
        importer.Import();

        return 0;
    }
}
