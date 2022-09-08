using System.Collections.ObjectModel;
using System.Dynamic;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using ApexCharts;

// ReSharper disable InconsistentNaming

namespace WSeminar.V2G.Simulator.Server.Smard;

public class SmardClient
{
    private readonly HttpClient _httpClient;

    public SmardClient(HttpClient c)
    {
        _httpClient = c;
        _httpClient.BaseAddress = new Uri("https://www.smard.de/app/chart_data/");
    }

    public async Task<List<DateTimeOffset>> GetIndex(int filter, DataResolution resolution)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{filter}/DE/index_{resolution.ToSmardString()}.json");
        var res = await _httpClient.SendAsync(request);
        if (!res.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Error code returned from Smard api: " + res.ToString());
        }

        var json = await res.Content.ReadFromJsonAsync<JsonDocument>();

        return json!.RootElement.GetProperty("timestamps").EnumerateArray().Select(element =>
            DateTimeOffset.FromUnixTimeMilliseconds(element.GetInt64())).ToList();
    }

    public async Task<Dictionary<DateTimeOffset, decimal?>> GetSeries(int filter, DataResolution resolution,
        DateTimeOffset offset)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"{filter}/DE/{filter}_DE_{resolution.ToSmardString()}_{offset.ToUnixTimeMilliseconds()}.json");
        var res = await _httpClient.SendAsync(request);
        if (!res.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Error code returned from Smard api: " + res.ToString());
        }

        var json = await res.Content.ReadFromJsonAsync<JsonDocument>();

        return json!.RootElement.GetProperty("series").EnumerateArray().ToDictionary(
            element => DateTimeOffset.FromUnixTimeMilliseconds(element[0].GetInt64()), element =>
            {
                if (element[1].ValueKind == JsonValueKind.Null)
                {
                    return (decimal?)null;
                }

                return element[1].GetDecimal();
            });
    }

    public async Task<IEnumerable<SeriesItem>> GetSeriesRange(int filter, DataResolution resolution,
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

        return results.SelectMany(dict => dict).Where(pair => pair.Key >= start && pair.Key < end)
            .Select(pair => new SeriesItem(pair.Key, pair.Value, resolution));
    }


    /// <summary>
    /// Returns the energy production for the given sources in the given time range.
    /// </summary>
    /// <param name="sources">Energy sources. Supports bitwise flags</param>
    /// <param name="resolution"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns>Returns a dictionary with the EnergySource as key and Dictionaries containing time and value as value in MWh</returns>
    public async Task<Dictionary<EnergySource, List<SeriesItem>>> GetProduction(
        EnergySource sources,
        DataResolution resolution,
        DateTimeOffset start, DateTimeOffset end)
    {
        var tuples = sources.GetEnergySourceFlags().Select(source =>
            (EnergySource: source, Task: GetSeriesRange(source.GetProductionId(), resolution, start, end))).ToList();

        await Task.WhenAll(tuples.Select(t => t.Task));
        return
            tuples.ToDictionary(
                t => t.EnergySource, // Key (EnergySource)
                t => t.Task.Result.ToList()); // Value (List of SeriesItems)
    }




    [Flags]
    public enum Filter
    {
        Installed_Bio = 189,
        Installed_Water = 3792,
        Installed_WindOffshore = 4076,
        Installed_WindOnshore = 186,
        Installed_Solar = 188,
        Installed_OtherRenewable = 194,
        Installed_Nuclear = 4073,
        Installed_BrownCoal = 4072,
        Installed_StoneCoal = 4075,
        Installed_Gas = 198,
        Installed_Pump = 4074,
        Installed_OtherFossil = 207,

        Production_Bio = 4066,
        Production_Water = 1226,
        Production_WindOffshore = 1225,
        Production_WindOnshore = 4067,
        Production_Solar = 4068,
        Production_OtherRenewable = 1228,
        Production_Nuclear = 1224,
        Production_BrownCoal = 1223,
        Production_StoneCoal = 4069,
        Production_Gas = 4071,
        Production_Pump = 4070,
        Production_Other = 1227,

        Consumption_All = 410,
        Consumption_Residual = 4359,
        Consumption_Pump = 4387,
    }

    [Flags]
    public enum FilterGroup
    {
        Installed = 0,
        Production = 1 << 0,
        Consumption = 1 << 1,

        Bio = 1 << 2,
        Water = 1 << 3,
        WindOffshore = 1 << 4,
        WindOnshore = 1 << 5,
        Solar = 1 << 6,
        OtherRenewable = 1 << 7,
        Nuclear = 1 << 8,
        BrownCoal = 1 << 9,
        StoneCoal = 1 << 10,
        Gas = 1 << 11,
        Pump = 1 << 12,
        OtherFossil = 1 << 13,

        Renewable = Bio | Water | WindOffshore | WindOnshore | Solar | OtherRenewable,
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

[Flags]
public enum EnergySource
{
    Bio = 1 << 0,
    Water = 1 << 1,
    WindOffshore = 1 << 2,
    WindOnshore = 1 << 3,
    Solar = 1 << 4,
    OtherRenewable = 1 << 5,
    Nuclear = 1 << 6,
    BrownCoal = 1 << 7,
    HardCoal = 1 << 8,
    Gas = 1 << 9,
    PumpedHydro = 1 << 10,
    OtherFossil = 1 << 11,

    All = Bio | Water | WindOffshore | WindOnshore | Solar | OtherRenewable | Nuclear | BrownCoal | HardCoal | Gas | PumpedHydro | OtherFossil,
    Renewable = Bio | Water | WindOffshore | WindOnshore | Solar | PumpedHydro,
    Conventional = Nuclear | BrownCoal | HardCoal | Gas
}

public class SeriesItem
{
    public readonly DateTimeOffset Time;
    public readonly decimal? Value;
    public readonly DataResolution Resolution;

    public SeriesItem(DateTimeOffset time, decimal? value, DataResolution resolution)
    {
        Time = time;
        Value = value;
        Resolution = resolution;
    }
}

public static class SmardEnumHelpers
{
    public static SmardClient.FilterGroup[] GetGroups(this SmardClient.Filter filter)
    {
        var f = filter | SmardClient.Filter.Consumption_All;
        var filterName = filter.ToString();
        var filterGroup = filterName.Split('_')[0];
        return Enum.GetValues<SmardClient.FilterGroup>().Where(fg => fg.ToString() == filterGroup).ToArray();
    }

    public static IEnumerable<EnergySource> GetEnergySourceFlags(this EnergySource source)
    {
        return Enum.GetValues<EnergySource>()
            .Where(energySource => energySource.GetFlags().Count() == 1 && source.HasFlag(energySource));
    }

    public static IEnumerable<T> GetFlags<T>(this T e) where T : System.Enum
        => e.GetType().GetEnumValues().Cast<T>().Where(o => e.HasFlag(o));

    public static int GetProductionId(this EnergySource source) =>
        source switch
        {
            EnergySource.Bio => 4066,
            EnergySource.Water => 1226,
            EnergySource.WindOffshore => 1225,
            EnergySource.WindOnshore => 4067,
            EnergySource.Solar => 4068,
            EnergySource.OtherRenewable => 1228,
            EnergySource.Nuclear => 1224,
            EnergySource.BrownCoal => 1223,
            EnergySource.HardCoal => 4069,
            EnergySource.Gas => 4071,
            EnergySource.PumpedHydro => 4070,
            EnergySource.OtherFossil => 1227,
            _ => throw new IndexOutOfRangeException(
                "Invalid EnergySource. Bitflags are not supported. Use GetEnergySourceFlags() before calling this method."),
        };

    private static int GetInstalledId(this EnergySource source) => source switch
    {
        EnergySource.Bio => 189,
        EnergySource.Water => 3792,
        EnergySource.WindOffshore => 4076,
        EnergySource.WindOnshore => 186,
        EnergySource.Solar => 188,
        EnergySource.OtherRenewable => 194,
        EnergySource.Nuclear => 4073,
        EnergySource.BrownCoal => 4072,
        EnergySource.HardCoal => 4075,
        EnergySource.Gas => 198,
        EnergySource.PumpedHydro => 4074,
        EnergySource.OtherFossil => 207,
        _ => throw new IndexOutOfRangeException(
            "Invalid EnergySource. Bitflags are not supported. Use GetEnergySourceFlags() before calling this method."),
    };
}

public static class FilterExtensions
{
    public static string ToSmardString(this DataResolution resolution) => resolution.ToString().ToLower();
}