// See https://aka.ms/new-console-template for more information

using System.Collections.Immutable;
using System.Text.Json;
using Deedle;

var listA = Enumerable.Range(1, 10).ToDictionary(i => i, i => i);
var listB = Enumerable.Range(6, 15).ToDictionary(i => i, i => i);
var listC = Enumerable.Range(11, 20).ToDictionary(i => i, i => i);


var x = new[] { listA, listB, listC }.

var keys = new[] { listA, listB, listC }.SelectMany(dictionary => dictionary.Keys).Distinct().OrderBy(i => i).ToList();

var n =
    from k in keys
    let a = listA.TryGetValue(k, out var aVal) ? aVal : null;




n = n.DistinctBy(pair => pair.Key);

var serialized = JsonSerializer.Serialize(n, new JsonSerializerOptions { WriteIndented = true });

Console.WriteLine(serialized);

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
