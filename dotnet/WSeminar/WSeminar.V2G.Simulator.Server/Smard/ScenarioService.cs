using System.Collections;
using System.Runtime.CompilerServices;
using Deedle;
using DateTimeOffset = System.DateTimeOffset;

namespace WSeminar.V2G.Simulator.Server.Smard;

public class ScenarioInput
{
    public DateTimeOffset Start;
    public DateTimeOffset End;
    public DataResolution Resolution;

    public double BatteryCapacity;
    public double BatteryCount;
    public double MaxAllowedDrainFactor;

    public double SolarFactor;
    public double WindFactor;
}

public class ScenarioResult
{
    public Dictionary<EnergySourceId, Series<DateTimeOffset, double?>> Produced;
    //public Series<DateTimeOffset, double?> ProducedSum;

    public Series<DateTimeOffset, double?> Solar { get; set; }
    public Series<DateTimeOffset, double?> OnShore { get; set; }
    public Series<DateTimeOffset, double?> OffShore { get; set; }
    public Series<DateTimeOffset, double?> OtherRenewable { get; set; }
    public Series<DateTimeOffset, double?> ConsumedEletricity { get; set; }

    public Series<DateTimeOffset, double> StoredDelta { get; set; }
    public Series<DateTimeOffset, double> StoredPower { get; set; }
    public Series<DateTimeOffset, double?> Consumption { get; set; }
}

public class ScenarioService
{
    private readonly SmardClient _client;


    public ScenarioService(SmardClient client)
    {
        _client = client;
    }

    public async Task<ScenarioResult> Calculate(ScenarioInput input)
    {
        ScenarioResult result = new ScenarioResult();
        
        var produced = await _client.GetProductions(EnergySourceId.Renewable, input.Resolution, input.Start, input.End);

        result.Solar = produced[EnergySourceId.Solar].SelectValues(d => d == null ? null : d * input.SolarFactor);
        result.OffShore = produced[EnergySourceId.WindOffshore]
            .SelectValues(d => d == null ? null : d * input.WindFactor);
        result.OnShore = produced[EnergySourceId.WindOnshore]
            .SelectValues(d => d == null ? null : d * input.WindFactor);
        result.OtherRenewable = produced
            .Where(pair =>
                pair.Key is EnergySourceId.OtherRenewable or EnergySourceId.Water or EnergySourceId.PumpedHydro)
            .Select(pair => pair.Value)
            .Aggregate((series, series1) => series.Zip(series1)
                .SelectValues(pair => pair.Item1.OrDefault(0) + pair.Item2.OrDefault(0)));

        result.Consumption = await _client.GetConsumption(input.Resolution, input.Start, input.End);

        var renewableCombined = new[] { result.Solar, result.OffShore, result.OnShore, result.OtherRenewable }.Aggregate(
            (series, series1) =>
                series.Zip(series1).SelectValues(tuple => tuple.Item1.OrDefault(0) + tuple.Item2.OrDefault(0))).SelectAllValues(d => d??0);

        var overproduction = renewableCombined.Zip(result.Consumption).SelectValues(tuple => tuple.Item1.OrDefault(0) - tuple.Item2.OrDefault(0));

        var storedBuilder = new SeriesBuilder<DateTimeOffset, double>();
        var powerBuilder = new SeriesBuilder<DateTimeOffset, double>();
        double stored = 0;
        foreach (var (time, cap) in overproduction.SelectAllValues(d => d??0).Observations)
        {
            var deltaUncapped = Math.Clamp(stored+cap, 0, input.BatteryCapacity * input.BatteryCount) - stored;
            powerBuilder.Add(time, deltaUncapped / input.Resolution.ToTimeSpan().TotalHours);
            storedBuilder.Add(time, stored + deltaUncapped);
        }
        
        result.StoredDelta = storedBuilder.Series;
        result.StoredPower = powerBuilder.Series;

        return result;
        /*
        result.ProducedSum = prods.Where(pair => EnergySourceId.All.HasFlag(pair.Key)).Select(pair => pair.Value)
            .Aggregate((series, series1) =>
                series.Zip(series1).SelectValues(tuple => tuple.Item1.OrDefault(0) + tuple.Item2.OrDefault(0)));
        */
    }
}


/*
public class TimeSeries : ICollection<SeriesItem>
{
    private ICollection<SeriesItem> _collectionImplementation;
    public DateTimeOffset? Start { get; private set; }
    public DateTimeOffset? End { get; private set; }

    public TimeSeries()
    {
        
    }
    
    public IEnumerator<SeriesItem> GetEnumerator()
    {
        return _collectionImplementation.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_collectionImplementation).GetEnumerator();
    }

    public void Add(SeriesItem item)
    {
        _collectionImplementation.Add(item);
        _collectionImplementation = _collectionImplementation.OrderBy(i => i.Time).ToList();
    }

    public void Clear()
    {
        Start = null;
        End = null;
        _collectionImplementation.Clear();
    }

    public bool Contains(SeriesItem item)
    {
        return _collectionImplementation.Contains(item);
    }

    public void CopyTo(SeriesItem[] array, int arrayIndex)
    {
        _collectionImplementation.CopyTo(array, arrayIndex);
    }

    public bool Remove(SeriesItem item)
    {
        var res = _collectionImplementation.Remove(item);
        if (res) 
            
            
    }

    public int Count => _collectionImplementation.Count;

    public bool IsReadOnly => _collectionImplementation.IsReadOnly;
}*/