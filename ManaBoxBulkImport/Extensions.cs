using Scryfall.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManaBoxBulkImport
{
    public static class Extensions
    {
        public static string GetOutputString(this CardSet set)
        {
            return $"{set.Code}\t{set.Name} ({set.ReleasedAt})";
        }
    }
}
