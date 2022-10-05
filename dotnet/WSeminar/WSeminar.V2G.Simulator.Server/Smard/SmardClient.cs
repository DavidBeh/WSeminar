using System.Collections.ObjectModel;
using System.Dynamic;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using ApexCharts;
using CacheTower;
using Deedle;

// ReSharper disable InconsistentNaming

namespace WSeminar.V2G.Simulator.Server.Smard;

public class SmardClient
{
    private readonly HttpClient _httpClient;
    private readonly ICacheStack _cache;
    private readonly ILogger<SmardClient> _logger;

    public SmardClient(HttpClient c, ICacheStack cache, ILogger<SmardClient> logger)
    {
        _httpClient = c;
        _cache = cache;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("https://www.smard.de/app/chart_data/");
    }

    public async Task<List<DateTimeOffset>> GetIndex(int filter, DataResolution resolution)
    {
        var request = $"{filter}/DE/index_{resolution.ToSmardString()}.json";
        var data = await FetchData(request);

        if (data is null)
        {
            return new List<DateTimeOffset>();
        }

        var json = JsonSerializer.Deserialize<JsonDocument>(data);

        return json!.RootElement.GetProperty("timestamps").EnumerateArray().Select(element =>
            DateTimeOffset.FromUnixTimeMilliseconds(element.GetInt64())).ToList();
    }

    private async Task<string?> FetchData(string request)
    {
        var data = await _cache.GetOrSetAsync<string>(request, async i =>
        {
            HttpResponseMessage? message = null;
            try
            {
                message = await _httpClient.GetAsync(request);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while fetching data from Smard");
            }

            return message?.IsSuccessStatusCode is null or false ? "" : await message.Content.ReadAsStringAsync();
        }, new CacheSettings(TimeSpan.FromHours(1)));
        return data;
    }

    public async Task<Series<DateTimeOffset, double?>> GetSeries(int filter, DataResolution resolution,
        DateTimeOffset offset)
    {
        var request = $"{filter}/DE/{filter}_DE_{resolution.ToSmardString()}_{offset.ToUnixTimeMilliseconds()}.json";
        var data = await FetchData(request);

        if (data is null)
        {
            return new SeriesBuilder<DateTimeOffset, double?>().Series;
        }

        var json = JsonSerializer.Deserialize<JsonDocument>(data);

        return json!.RootElement.GetProperty("series").EnumerateArray().ToDictionary(
            element => DateTimeOffset.FromUnixTimeMilliseconds(element[0].GetInt64()), element =>
            {
                if (element[1].ValueKind == JsonValueKind.Null)
                {
                    return (double?)null;
                }

                return element[1].GetDouble();
            }).ToSeries();
    }

    public async Task<Series<DateTimeOffset, double?>> GetSeriesRange(int filter, DataResolution resolution,
        DateTimeOffset start, DateTimeOffset end)
    {
        Console.WriteLine("FILTER: " + filter);
        var index = await GetIndex(filter, resolution);
        index.Add(DateTimeOffset.MaxValue);
        /*
        index.Zip(index.Skip(1),
                (offset, timeOffset) => (begin: offset, next: timeOffset)).ToList()
            .ForEach(tuple => Console.WriteLine(tuple));
        */
        var timesToRequest = index.Zip(index.Skip(1),
                (offset, timeOffset) => (begin: offset, next: timeOffset))
            .Where(file => file.next > start && end >= file.begin);
        //var timesToRequest = index.Where(time => time >= start && time <= end).ToList();

        var tasks = timesToRequest.Select(time => GetSeries(filter, resolution, time.begin));
        var results = await Task.WhenAll(tasks);

        return results.Aggregate((series, series1) => series.Merge(series1, UnionBehavior.PreferLeft))
            .Where(pair => pair.Key >= start && pair.Key < end);
    }


    /// <summary>
    /// Returns the energy production for the given sources in the given time range.
    /// </summary>
    /// <param name="sources">Energy sources. Supports bitwise flags</param>
    /// <param name="resolution"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns>Returns a dictionary with the EnergySource as key and Dictionaries containing time and value as value in MWh</returns>
    public async Task<Dictionary<EnergySourceId, Series<DateTimeOffset, double?>>> GetProductions(
        EnergySourceId sources,
        DataResolution resolution,
        DateTimeOffset start, DateTimeOffset end)
    {
        var tuples = sources.GetEnergySourceFlags().Select(source =>
            (EnergySource: source, Task: GetSeriesRange(source.GetProductionId(), resolution, start, end))).ToList();

        await Task.WhenAll(tuples.Select(t => t.Task));
        return
            tuples.ToDictionary(
                t => t.EnergySource, // Key (EnergySource)
                t => t.Task.Result); // Value (List of SeriesItems)
    }
    
    public async Task<Series<DateTimeOffset, double?>> GetProduction(EnergySourceId source, DataResolution resolution, DateTimeOffset start, DateTimeOffset end)
    {
        return await GetSeriesRange(source.GetProductionId(), resolution, start, end);
    }

    public async Task<Series<DateTimeOffset, double?>> GetConsumption(DataResolution inputResolution, DateTimeOffset start, DateTimeOffset end)
    {
        return await GetSeriesRange(410, inputResolution, start, end);
    }
}

public enum DataResolution
{
    QuarterHour,
    Hour,
    Day,
    Week,
    Month,
    Year
}

[AttributeUsage(AttributeTargets.Field)]
public class Production : Attribute
{
    public Production(int dataId)
    {
        DataId = dataId;
    }

    public readonly int DataId;
}

[AttributeUsage(AttributeTargets.Field)]
public class Consumption : Attribute
{
    public Consumption(int dataId)
    {
        DataId = dataId;
    }

    public readonly int DataId;
}

[AttributeUsage(AttributeTargets.Field)]
public class Installed : Attribute
{
    public Installed(int dataId)
    {
        DataId = dataId;
    }

    public readonly int DataId;
}

[Flags]
public enum EnergySourceId
{
    [Production(4066), Installed(189)] // Same as in GetProductionId()
    Bio = 1 << 0,
    [Production(1226), Installed(3792)] Water = 1 << 1,
    [Production(1225), Installed(4076)] WindOffshore = 1 << 2,
    [Production(4067), Installed(186)] WindOnshore = 1 << 3,
    [Production(4068), Installed(188)] Solar = 1 << 4,
    [Production(1228), Installed(194)] OtherRenewable = 1 << 5,
    [Production(1224), Installed(4073)] Nuclear = 1 << 6,
    [Production(1223), Installed(4072)] BrownCoal = 1 << 7,
    [Production(4069), Installed(4075)] HardCoal = 1 << 8,
    [Production(4071), Installed(198)] Gas = 1 << 9,
    [Production(4070), Installed(4074)] PumpedHydro = 1 << 10,
    [Production(1227), Installed(207)] OtherFossil = 1 << 11,

    All = Bio | Water | WindOffshore | WindOnshore | Solar | OtherRenewable | Nuclear | BrownCoal | HardCoal | Gas |
          PumpedHydro | OtherFossil,
    Renewable = Bio | Water | WindOffshore | WindOnshore | Solar | PumpedHydro,
    Conventional = Nuclear | BrownCoal | HardCoal | Gas
}

[Flags]
public enum ConsumptionId
{
    [Consumption(410)] All = 1 << 0,
    [Consumption(4359)] Residual = 1 << 1,
    [Consumption(4387)] PumpedHydro = 1 << 2,
}

public class SeriesItem
{
    public readonly DateTimeOffset Time;
    public readonly decimal? Value;
    public readonly DataResolution Resolution;


    private const int a = 3;
    private const int b = 4;
    private const int c = a + b;

    public SeriesItem(DateTimeOffset time, decimal? value, DataResolution resolution)
    {
        Time = time;
        Value = value;
        Resolution = resolution;
    }

    public SeriesItem(SeriesItem item)
    {
        Time = item.Time;
        Value = item.Value;
        Resolution = item.Resolution;
    }
}

public static class SmardEnumHelpers
{
    public static IEnumerable<EnergySourceId> GetEnergySourceFlags(this EnergySourceId source)
    {
        return Enum.GetValues<EnergySourceId>()
            .Where(energySource => energySource.GetFlags().Count() == 1 && source.HasFlag(energySource));
    }

    public static IEnumerable<T> GetFlags<T>(this T e) where T : System.Enum
        => e.GetType().GetEnumValues().Cast<T>().Where(o => e.HasFlag(o));

    public static int GetProductionId(this EnergySourceId source) =>
        source switch
        {
            EnergySourceId.Bio => 4066,
            EnergySourceId.Water => 1226,
            EnergySourceId.WindOffshore => 1225,
            EnergySourceId.WindOnshore => 4067,
            EnergySourceId.Solar => 4068,
            EnergySourceId.OtherRenewable => 1228,
            EnergySourceId.Nuclear => 1224,
            EnergySourceId.BrownCoal => 1223,
            EnergySourceId.HardCoal => 4069,
            EnergySourceId.Gas => 4071,
            EnergySourceId.PumpedHydro => 4070,
            EnergySourceId.OtherFossil => 1227,
            _ => throw new IndexOutOfRangeException(
                "Invalid EnergySource. Bitflags are not supported. Use GetEnergySourceFlags() before calling this method."),
        };

    private static int GetInstalledId(this EnergySourceId source) => source switch
    {
        EnergySourceId.Bio => 189,
        EnergySourceId.Water => 3792,
        EnergySourceId.WindOffshore => 4076,
        EnergySourceId.WindOnshore => 186,
        EnergySourceId.Solar => 188,
        EnergySourceId.OtherRenewable => 194,
        EnergySourceId.Nuclear => 4073,
        EnergySourceId.BrownCoal => 4072,
        EnergySourceId.HardCoal => 4075,
        EnergySourceId.Gas => 198,
        EnergySourceId.PumpedHydro => 4074,
        EnergySourceId.OtherFossil => 207,
        _ => throw new IndexOutOfRangeException(
            "Invalid EnergySource. Bitflags are not supported. Use GetEnergySourceFlags() before calling this method."),
    };
}

public static class FilterExtensions
{
    public static string ToSmardString(this DataResolution resolution) => resolution.ToString().ToLower();

    public static TimeSpan ToTimeSpan(this DataResolution resolution) => resolution switch
    {
        DataResolution.QuarterHour => TimeSpan.FromMinutes(15),
        DataResolution.Hour => TimeSpan.FromHours(1),
        DataResolution.Day => TimeSpan.FromDays(1),
        DataResolution.Week => TimeSpan.FromDays(7),
        DataResolution.Month => TimeSpan.FromDays(30),
        DataResolution.Year => TimeSpan.FromDays(365),
        _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution, null)
    };
}