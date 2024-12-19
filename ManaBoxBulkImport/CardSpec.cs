using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ManaBoxBulkImport
{
    public class CardSpec
    {
        private static char[] delimiters = [' ', ','];
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
        public string Language => _language ?? "EN";
        public string Condition => _condition ?? "NM";

        private CardSpec() { }

        
        public static bool TryCreate(string text, [NotNullWhen(true)] out CardSpec? cardSpec)
        {
            cardSpec = null;

            var result = new CardSpec();

            var parts = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length == 0)
            {
                return false;
            }

            int colonIndex = parts[0].IndexOf(':');
            if (colonIndex != -1)
            {
                result._setId = parts[0].Substring(0, colonIndex);
            }

            result._collectorId = parts[0].Substring(colonIndex + 1);           

            cardSpec = result;
            return true;
        }

    }
}
