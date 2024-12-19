using Csv;
using Scryfall;
using Scryfall.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace TestApp.ViewModels
{
    public class CardListViewModel : ViewModelBase
    {
        //static Regex csv = new Regex("(?:,|\\n|^)(\"(?:(?:\"\")*[^\"]*)*\"|[^\",\\n]*|(?:\\n|$))", RegexOptions.Compiled);
        static Regex csv = new Regex(@"^\s*""((?:[^""\\]|\\.)*)""\s*.*$");
        public CardListViewModel() { }

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
                        var card = new Card(cardDefinition, false, false, 1);
                        Cards.Add(card);
                    }
                }
            }
            //// Skip header
            //var line = reader.ReadLine();
            //line = reader.ReadLine();

            //while (!string.IsNullOrWhiteSpace(line))
            //{
            //    var fields = line.Split(',');

            //    var offset = fields.Length > 17 ? fields.Length - 17 : 0;

            //    var setCode = fields[3 + offset];
            //    var collectorId = fields[5 + offset];

            //    if (cache.TryGetValue(setCode, out var cardsInSet))
            //    {
            //        if (cardsInSet.TryGetValue(collectorId, out var cardDefinition))
            //        {
            //            var card = new Card(cardDefinition, false, false, 1);
            //            Cards.Add(card);
            //        }
            //    }
            //    line = reader.ReadLine();          
            //}
        }
    }
}
