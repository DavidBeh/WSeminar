using System.Drawing;

namespace WSeminar.V2G.Simulator.Server;


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
