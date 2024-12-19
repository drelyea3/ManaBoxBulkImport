using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp;

public class ManaBoxCardCsv
{
    public string BinderName { get; set; }
    public string BinderType { get; set; }
    public string Name { get; set; }
    public string Setcode { get; set; }
    public string Setname { get; set; }
    public int Collectornumber { get; set; }
    public string Foil { get; set; }
    public string Rarity { get; set; }
    public int Quantity { get; set; }
    public int ManaBoxID { get; set; }
    public string ScryfallID { get; set; }
    public double Purchaseprice { get; set; }
    public bool Misprint { get; set; }
    public bool Altered { get; set; }
    public string Condition { get; set; }
    public string Language { get; set; }
    public string Purchasepricecurrency { get; set; }
}

//public class ManaBoxCardCsvClassMap : ClassMap<ManaBoxCardCsv>
//{
//    public ManaBoxCardCsvClassMap()
//    {
//        Map(m => m.BinderName).Name("Binder Name");
//        Map(m => m.BinderType).Name("Binder Type");
//        Map(m => m.Name).Name("Name");
//        Map(m => m.Setcode).Name("Set code");
//        Map(m => m.Setname).Name("Set name");
//        Map(m => m.Collectornumber).Name("Collector number");
//        Map(m => m.Foil).Name("Foil");
//        Map(m => m.Rarity).Name("Rarity");
//        Map(m => m.Quantity).Name("Quantity");
//        Map(m => m.ManaBoxID).Name("ManaBox ID");
//        Map(m => m.ScryfallID).Name("Scryfall ID");
//        Map(m => m.Purchaseprice).Name("Purchase price");
//        Map(m => m.Misprint).Name("Misprint");
//        Map(m => m.Altered).Name("Altered");
//        Map(m => m.Condition).Name("Condition");
//        Map(m => m.Language).Name("Language");
//        Map(m => m.Purchasepricecurrency).Name("Purchase price currency");
//    }
//}

