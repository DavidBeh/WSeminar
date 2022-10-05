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

    public Series<DateTimeOffset, double> Solar { get; set; }
    public Series<DateTimeOffset, double> OnShore { get; set; }
    public Series<DateTimeOffset, double> OffShore { get; set; }
    public Series<DateTimeOffset, double> OtherRenewable { get; set; }
    public Series<DateTimeOffset, double> ConsumedEletricity { get; set; }

    public Series<DateTimeOffset, double> StoredDelta { get; set; }
    public Series<DateTimeOffset, double> StoredPower { get; set; }
    public Series<DateTimeOffset, double> Consumption { get; set; }
}

public enum CalculationColumn
{
    Production_Solar,
    Production_OnShore,
    Production_OffShore,
    Production_OtherRenewable,
    
    Consumption,
    
    Display_Solar,
    Display_OnShore,
    Display_OffShore,
    Display_OtherRenewable,
    
    Display_Fossil,
    
    Overproduction,
    Display_Overproduction,
    
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
        var fr = Frame.CreateEmpty<DateTimeOffset, CalculationColumn>();
        
        ScenarioResult result = new ScenarioResult();

        var produced = await _client.GetProductions(EnergySourceId.Renewable, input.Resolution, input.Start, input.End);
        
        fr.AddColumn(CalculationColumn.Production_OtherRenewable, produced[EnergySourceId.OtherRenewable]);
        fr.AddColumn(CalculationColumn.Production_OffShore, produced[EnergySourceId.WindOffshore].SelectValues(d => d * input.WindFactor));
        fr.AddColumn(CalculationColumn.Production_OnShore, produced[EnergySourceId.WindOnshore].SelectValues(d => d * input.WindFactor));
        fr.AddColumn(CalculationColumn.Production_Solar, produced[EnergySourceId.Solar].SelectValues(d => d * input.SolarFactor));
        fr.FillMissing(0);

        var sum = fr.Columns.Observations.Select(pair => pair.Value.As<double>())
            .Aggregate((series, series1) => series + series1);
        
        
        /*
        result.Solar = produced[EnergySourceId.Solar].SelectAllValues(d => (d ?? 0) * input.SolarFactor);
        result.OffShore = produced[EnergySourceId.WindOffshore]
            .SelectAllValues(d => (d ?? 0) * input.WindFactor);
        result.OnShore = produced[EnergySourceId.WindOnshore]
            .SelectAllValues(d => (d ?? 0) * input.WindFactor);
        result.OtherRenewable = produced.Where(pair =>
                pair.Key is EnergySourceId.OtherRenewable or EnergySourceId.Water or EnergySourceId.PumpedHydro)
            .Select(pair => pair.Value.FillMissing2())
            .Aggregate((series, series1) => series + series1);
*/
        var consumption = (await _client.GetConsumption(input.Resolution, input.Start, input.End)).FillMissing2();
        
        result.Consumption = consumption;
        fr.AddColumn(CalculationColumn.Consumption, consumption);

        fr.AddColumn(CalculationColumn.Display_OtherRenewable, fr.Rows.SelectValues(series => Math.Max(
            Math.Min(series.GetAs<double>(CalculationColumn.Production_OtherRenewable), series.GetAs<double>(CalculationColumn.Consumption)),0)));
        fr.AddColumn(CalculationColumn.Display_OnShore, fr.Rows.SelectValues(series => Math.Max(
            Math.Min(series.GetAs<double>(CalculationColumn.Production_OtherRenewable) + series.GetAs<double>(CalculationColumn.Production_OnShore), series.GetAs<double>(CalculationColumn.Consumption) - series.GetAs<>() ),0)));
        fr.AddColumn(CalculationColumn.Display_OtherRenewable, fr.Rows.SelectValues(series => Math.Max(
            Math.Min(series.GetAs<double>(CalculationColumn.Production_OtherRenewable), series.GetAs<double>(CalculationColumn.Consumption)),0)));
        fr.AddColumn(CalculationColumn.Display_OtherRenewable, fr.Rows.SelectValues(series => Math.Max(
            Math.Min(series.GetAs<double>(CalculationColumn.Production_OtherRenewable), series.GetAs<double>(CalculationColumn.Consumption)),0)));
        
        
        var renewableCombined =
            new[] { result.Solar, result.OffShore, result.OnShore, result.OtherRenewable }.Aggregate(
                (series, series1) => series + series1);

        var overproduction = renewableCombined.Zip(result.Consumption)
            .SelectValues(tuple => tuple.Item1.OrDefault(0) - tuple.Item2.OrDefault(0)).FillMissing();

        
        
        var storedBuilder = new SeriesBuilder<DateTimeOffset, double>();
        var powerBuilder = new SeriesBuilder<DateTimeOffset, double>();
        double stored = 0;
        foreach (var (time, cap) in overproduction.Observations)
        {
            var deltaUncapped = Math.Clamp(stored + cap, 0, input.BatteryCapacity * input.BatteryCount) - stored;
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

    private static Series<DateTimeOffset, double> CalculateDisplay(Frame<DateTimeOffset,CalculationColumn> fra, CalculationColumn display, params CalculationColumn[] columns)
    {
        var before = columns.Skip(1).Select(fra.Rows.Where(pair => pair.k))
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