using System.Diagnostics;
using System.Drawing;
using ApexCharts;
using Deedle;
using Color = System.Drawing.Color;

namespace WSeminar.V2G.Simulator.Server;

static class ColorHelper
{
    internal static string ToHex(this Color color, int alpha = 255)
    {
        return ColorTranslator.ToHtml(Color.FromArgb(Color.FromArgb(alpha, color).ToArgb()));
    }

    internal static SeriesStroke ToHexStroke(this Color color, double darken = 0.3d)
    {

        return new SeriesStroke()
        {
            Color = color.Lerp(Color.Black,(float) darken).ToHex(),
            Width = 2,
        };
    }

    public static float Lerp(this float start, float end, float amount)
    {
        float difference = end - start;
        float adjusted = difference * amount;
        return start + adjusted;
    }

    public static Color Lerp(this Color colour, Color to, float amount)
    {
        // start colours as lerp-able floats
        float sr = colour.R, sg = colour.G, sb = colour.B;

        // end colours as lerp-able floats
        float er = to.R, eg = to.G, eb = to.B;

        // lerp the colours to get the difference
        byte r = (byte)sr.Lerp(er, amount),
            g = (byte)sg.Lerp(eg, amount),
            b = (byte)sb.Lerp(eb, amount);

        // return the new colour
        return Color.FromArgb(r, g, b);
    }

    public static void ColorToHSV(this Color color, out double hue, out double saturation, out double value)
    {
        int max = Math.Max(color.R, Math.Max(color.G, color.B));
        int min = Math.Min(color.R, Math.Min(color.G, color.B));

        hue = color.GetHue();
        saturation = (max == 0) ? 0 : 1d - (1d * min / max);
        value = max / 255d;
    }

    public static Color ColorFromHSV(double hue, double saturation, double value)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60 - Math.Floor(hue / 60);

        value = value * 255;
        int v = Convert.ToInt32(value);
        int p = Convert.ToInt32(value * (1 - saturation));
        int q = Convert.ToInt32(value * (1 - f * saturation));
        int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        if (hi == 0)
            return Color.FromArgb(255, v, t, p);
        else if (hi == 1)
            return Color.FromArgb(255, q, v, p);
        else if (hi == 2)
            return Color.FromArgb(255, p, v, t);
        else if (hi == 3)
            return Color.FromArgb(255, p, q, v);
        else if (hi == 4)
            return Color.FromArgb(255, t, p, v);
        else
            return Color.FromArgb(255, v, p, q);
    }
}

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