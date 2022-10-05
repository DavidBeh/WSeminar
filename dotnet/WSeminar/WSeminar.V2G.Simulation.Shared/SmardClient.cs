using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Deedle;

namespace WSeminar.V2G.Simulation.Shared;

public class SmardApiDataAttribute : Attribute
{
    public readonly int Id;

    public SmardApiDataAttribute(int id)
    {
        Id = id;
    }

    public SmardApiDataAttribute(Type type, string nameof)
    {
        var p = type.GetProperty(nameof, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        Id = p?.GetCustomAttribute<SmardApiDataAttribute>()?.Id ??
             throw new NotSupportedException("Property does not have SmardApiDataAttribute");
    }
}

public enum Auflösung
{
    [JsonPropertyName("quarterhour")] ViertelStunde,
    [JsonPropertyName("hour")] Stunde,
    [JsonPropertyName("day")] Tag,
    [JsonPropertyName("week")] Woche,
    [JsonPropertyName("month")] Monat,
    [JsonPropertyName("year")] Jahr
}

public class SmardClient
{
    private readonly HttpClient _client;


    public SmardClient()
    {
        _client = new HttpClient();
    }

    private async Task<Series<DateTimeOffset, double?>> GetSingleSeriesAsync(int id, DateTimeOffset start,
        DateTimeOffset end,
        Auflösung auflösung = Auflösung.ViertelStunde)
    {
        // Konvertiert auflösung in einen string, der der API entspricht
        var resolutionString = auflösung.GetAttribute<JsonPropertyNameAttribute>()?.Name ??
                               throw new NotSupportedException($"Auflösung {auflösung} not supported");
        // Ruft alle Indexe ab
        var indexUrl = $"https://smard.de/app/chart_data/{id}/DE/index_{resolutionString}.json;";
        var result = await _client.GetAsync(indexUrl);
        result.EnsureSuccessStatusCode();
        // Filtert die Indexe nach dem Zeitraum
        var index = (await JsonSerializer.DeserializeAsync<JsonDocument>(await result.Content.ReadAsStreamAsync()))!
            .RootElement.GetProperty("timestamps").EnumerateArray()
            .Select(element => DateTimeOffset.FromUnixTimeMilliseconds(element.GetInt64())).ToList();
        var filesToRequest = index.Zip(index.Skip(1).Append(DateTimeOffset.MaxValue))
            .Where(t => t.Second < start && t.First <= end).Select(t => t.First).ToList();

        // Ruft die Daten ab. TODO Multithreading hinzufügen?
        var series = filesToRequest.Select(async offset =>
        {
            var url =
                $"https://smard.de/app/chart_data/{id}/DE/{id}_DE_{resolutionString}_{offset.ToUnixTimeMilliseconds()}.json";
            var result2 = (await _client.GetAsync(url)).EnsureSuccessStatusCode();
            var json = await JsonSerializer.DeserializeAsync<JsonDocument>(await result2.Content.ReadAsStreamAsync());
            return json!.RootElement.GetProperty("series").EnumerateArray().Select(element =>
            {
                var a = element.EnumerateArray();
                var time = a.Current;
                a.MoveNext();
                return new KeyValuePair<DateTimeOffset, double?>(
                    DateTimeOffset.FromUnixTimeMilliseconds(time.GetInt64()),
                    a.Current.ValueKind == JsonValueKind.Number
                        ? a.Current.GetDouble()
                        : null);
            });
        }).SelectMany(t => t.Result).ToSeries().StartAt(start).Before(end);
        return series;
    }

    public Series<DateTimeOffset, T> GetData<T>(DateTimeOffset start, DateTimeOffset end,
        Auflösung auflösung = Auflösung.ViertelStunde) where T : class, new()
    {
        Series<DateTimeOffset, T> rows = new SeriesBuilder<DateTimeOffset, T>().Series;

        var props = typeof(T).GetProperties()
            .Select(info => (Prop: info, Attr: info.GetCustomAttribute<SmardApiDataAttribute>()))
            .Where(tuple => tuple.Item2 != null).ToList();
        foreach (var (prop, attr) in props)
        {
            var series = GetSingleSeriesAsync(attr!.Id, start, end, auflösung).Result;
            rows = series.Zip(rows).SelectValues(tuple =>
            {
                var obj = tuple.Item2.HasValue ? tuple.Item2.Value : Activator.CreateInstance<T>();
                prop.SetValue(obj, tuple.Item1.OrDefault(null));
                return obj;
            });
        }
        return rows;
    }
}

/// <summary>
/// Realisierte Erzeugung in MWh
/// </summary>
public record RealisierteErzeugungRow
{
    [SmardApiData(4066)] public double? Biomasse { get; init; }
    [SmardApiData(1226)] public double? Wasserkraft { get; init; }
    [SmardApiData(1225)] public double? WindOffshore { get; init; }
    [SmardApiData(4067)] public double? WindOnshore { get; init; }
    [SmardApiData(4068)] public double? Photovoltaik { get; init; }
    [SmardApiData(1228)] public double? SonstigeErneuerbare { get; init; }
    [SmardApiData(1224)] public double? Kernenergie { get; init; }
    [SmardApiData(1223)] public double? Braunkohle { get; init; }
    [SmardApiData(4069)] public double? Steinkohle { get; init; }
    [SmardApiData(4071)] public double? Erdgas { get; init; }
    [SmardApiData(4070)] public double? Pumpspeicher { get; init; }
    [SmardApiData(1227)] public double? SonstigeKonventionell { get; init; }
}

/// <summary>
/// Realisierter Verbrauch in MWh
/// </summary>
public record RealisierterVerbrauchRow
{
    [SmardApiData(410)] public double? Gesamt { get; init; }
    [SmardApiData(4387)] public double? Pumpspeicher { get; init; }
    [SmardApiData(4359)] public double? Residuallast { get; init; }
}

internal static class EnumExtensions
{
    // This extension method is broken out so you can use a similar pattern with 
    // other MetaData elements in the future. This is your base method for each.
    public static T? GetAttribute<T>(this Enum value) where T : Attribute
    {
        var type = value.GetType();
        var memberInfo = type.GetMember(value.ToString());
        var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);
        return attributes.Length > 0
            ? (T)attributes[0]
            : null;
    }
}