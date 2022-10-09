using Deedle;

namespace WSeminar.V2G.Simulator.Server;

public class BoxPair
{
    public DateTimeOffset Key;
    public double? Value { get; set; }

    public BoxPair(KeyValuePair<DateTimeOffset, double?> pair)
    {
        Key = pair.Key;
        Value = pair.Value;
    }

    public BoxPair(KeyValuePair<DateTimeOffset, double> pair)
    {
        Key = pair.Key;
        Value = pair.Value;
    }

    public BoxPair(KeyValuePair<DateTimeOffset, OptionalValue<double?>> pair)
    {
        Key = pair.Key;
        Value = pair.Value.OrDefault(null);
    }
}

public static class DeedleExtensions
{
    // Selects all Entries, including missing ones
    public static Series<TKey, TValNew> SelectAll<TKey, TVal, TValNew>(this Series<TKey, TVal> series,
        Func<KeyValuePair<TKey, TVal?>, TValNew> func)
    {
        return series.SelectOptional(pair =>
            OptionalValue.Create(func(new KeyValuePair<TKey, TVal?>(pair.Key,
                pair.Value.HasValue ? pair.Value.Value : default(TVal?)))));
    }

    public static Series<TKey, TValNew> SelectAllValues<TKey, TVal, TValNew>(this Series<TKey, TVal> series,
        Func<TVal?, TValNew> func)
    {
        return series.SelectOptional(pair =>
            OptionalValue.Create(func(pair.Value.HasValue ? pair.Value.Value : default(TVal?))));
    }

    public static Series<TKey, TVal> FillMissing2<TKey, TVal>(this Series<TKey, TVal?> series, TVal value = default)
        where TVal : struct
    {
        return series.SelectAllValues(val => val ?? value);
    }
}

public static class DictExtensions
{
    public static TValue? GetOr<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
    {
        return dictionary.GetValueOrDefault(key, default!);
    }

    public static TVal Get0<TKey, TVal>(this IReadOnlyDictionary<TKey, TVal?> dictionary, TKey key)
        where TVal : struct
    {
        return dictionary.TryGetValue(key, out var value) ? value ?? default : default;
    }


    public static TValue GetOr<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key,
        TValue defaultValue)
    {
        if (dictionary == null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }

        TValue? value;
        return dictionary.TryGetValue(key, out value) ? value : defaultValue;
    }
}

public static class CustomSeriesExtensions
{
    public static Series<T, OptionalValue<K>> AddMissingKeys<T, K>(this Series<T, K> series, T[] keys)
        where T : IComparable<T>
    {
        var newKeys = keys.Union(series.Keys).OrderBy(comparable => comparable).ToList();
        return new Series<T, OptionalValue<K>>(newKeys, keys.Select(series.TryGet));
    }
}