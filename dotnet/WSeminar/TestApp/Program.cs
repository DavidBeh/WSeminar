// See https://aka.ms/new-console-template for more information

using System.Dynamic;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;
using System.Text.Json.Serialization;
using Deedle;
using Microsoft.Data.Analysis;


var s = Enumerable.Range(0, 5).Select(i =>  KeyValuePair.Create(i, (int?)(i % 2 == 0 ? i : null))).ToSeries();
var a = s.SelectAll(pair => pair.Value ?? 0);
s.Print();
a.Print();

s.Print();
var s1 = Enumerable.Range(3, 5).Select(i =>  KeyValuePair.Create(i, (int?)i)).ToSeries();
s1.Print();

public static class DeedleExtensions
{
    // Selects all Entries, including missing ones
    public static Series<TKey, TValNew> SelectAll<TKey, TVal, TValNew>(this Series<TKey, TVal> series, Func<KeyValuePair<TKey, TVal?>, TValNew> func)
    {
        return series.SelectOptional(pair => OptionalValue.Create(func(new KeyValuePair<TKey, TVal?>(pair.Key, pair.Value.HasValue ? pair.Value.Value : default(TVal?)))));
    }
}

class Energy
{
    [Flags]
    public enum EnergySource
    {
        Bio = 0,
        Water = 1 << 0,
        WindOffshore = 1 << 1,
        WindOnshore = 1 << 2,
        Solar = 1 << 3,
        Nuclear = 1 << 4,
        BrownCoal = 1 << 5,
        HardCoal = 1 << 6,
        Gas = 1 << 7,
        PumpedHydro = 1 << 8,

        All = Bio | Water | WindOffshore | WindOnshore | Solar | Nuclear | BrownCoal | HardCoal | Gas | PumpedHydro,
        Renewable = Bio | Water | WindOffshore | WindOnshore | Solar | PumpedHydro,
        Conventional = Nuclear | BrownCoal | HardCoal | Gas
    }
}
