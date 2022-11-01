using System.Diagnostics;

namespace WSeminar.V2G.Simulator.Server.Smard;

public record ScenarioInput
{
    public DateTimeOffset Start;
    public DateTimeOffset End;
    public DataResolution Resolution;

    public double BatteryCapacity_kWh;
    public double BatteryCount_Millionen;
    public double MaxAllowedDrainFactor;
    public double TotalMaxPower => MaxAllowedDrainFactor * TotalMaxCapacity_MWh;
    public double TotalMaxCapacity_MWh => BatteryCount_Millionen * Math.Pow(10d, 6d) * (BatteryCapacity_kWh / 1000d);
    
    public double SolarFactor;
    public double WindFactor;

    public ScenarioInput Copy()
    {
        return this with { };
    }
}

public class ScenarioResult
{
    public List<ScenarioRow> Rows { get; init; }
    public ScenarioInput Input { get; init; }
}
/*
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
}*/

public record ScenarioRow
{
    public DateTimeOffset Time { get; init; }
    public double Solar { get; init; }
    public double OnShore { get; init; }
    public double OffShore { get; init; }
    public double OtherRenewable { get; init; }
    public double Consumption { get; init; }

    public double DisplaySolar { get; init; }
    public double DisplayOnShore { get; init; }
    public double DisplayOffShore { get; init; }
    public double DisplayOtherRenewable { get; init; }
    /// <summary>
    /// Derzeit gespeicherte Kapazität. Zwischen 0 und TotalMaxCapacity_MWh
    /// </summary>
    public double CurrentCapacity { get; init; }
    /// <summary>
    /// Stromstärke in MWh. Zwischen -TotalMaxPower und +TotalMaxPower
    /// </summary>
    public double CurrentPower { get; init; }
    /// <summary>
    /// Positiv: Unbenutzte Kapazität; Negativ: Residuallast
    /// </summary>
    public double UnusedProduction { get; init; }
    /// <summary>
    /// Positiv: Batterien werden geladen; Negativ: Batterien werden entladen
    /// </summary>
    public double BatteryCapacityDelta { get; init; }
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

        
        var solarProd = 
            await _client.GetProduction(EnergySourceId.Solar, input.Resolution, input.Start, input.End);
        var onShoreProd =
            await _client.GetProduction(EnergySourceId.WindOnshore, input.Resolution, input.Start, input.End);
        var offShoreProd =
            await _client.GetProduction(EnergySourceId.WindOffshore, input.Resolution, input.Start, input.End);
        var waterProd = 
            await _client.GetProduction(EnergySourceId.Water, input.Resolution, input.Start, input.End);
        var pumpedProd =
            await _client.GetProduction(EnergySourceId.PumpedHydro, input.Resolution, input.Start, input.End);
        var otherRenewableProd =
            await _client.GetProduction(EnergySourceId.OtherRenewable, input.Resolution, input.Start, input.End);
        var consumption = 
            await _client.GetConsumption(input.Resolution, input.Start, input.End);


        var keys = new[] { solarProd, onShoreProd, offShoreProd, waterProd, pumpedProd, otherRenewableProd }
            .SelectMany(series => series.Keys)
            .Distinct().OrderBy(offset => offset).ToList();
        
        var currentCapacity = 0d;
        var rows = new List<ScenarioRow>();
        foreach (var key in keys)
        {
            double otherSum = otherRenewableProd.Get0(key) + pumpedProd.Get0(key) + waterProd.Get0(key);
            double c = consumption.Get0(key);
            double otherDisplay = otherSum - Math.Max(0, otherSum - c);
            double onShore = onShoreProd.Get0(key) * input.WindFactor;
            double onShoreDisplay = onShore - Math.Max(0, otherSum + onShore - c);
            double offShore = offShoreProd.Get0(key) * input.WindFactor;
            double offShoreDisplay = offShore - Math.Max(0, otherSum + onShore + offShore - c);
            double solar = solarProd.Get0(key) * input.SolarFactor;
            double solarDisplay = solar - Math.Max(0, otherSum + offShore + onShore + solar - c);
            double total = otherSum + offShore + onShore + solar;
            
            // Difference between production and consumption. Positive => charge, negative => discharge
            double overproduction = total - c;
            // How much we could store if we could charge at unlimited power (MWh)
            double unlimitedCapacityDelta =
                Math.Clamp(overproduction + currentCapacity, 0, input.TotalMaxCapacity_MWh) - currentCapacity;
            // Converted to Power (Watt)
            double unlimitedPower = unlimitedCapacityDelta / input.Resolution.ToTimeSpan().TotalHours;
            // Power Limited by maxPower
            double limitedPower = Math.Clamp(unlimitedPower, -input.TotalMaxPower, input.TotalMaxPower);
            // Converted back to Capacity (MWh)
            double limitedCapacityDelta = limitedPower * input.Resolution.ToTimeSpan().TotalHours;
            currentCapacity += limitedCapacityDelta;
            double unusedCapacity = overproduction - limitedCapacityDelta;
            Debug.WriteLine(input.TotalMaxPower + "|" + input.TotalMaxCapacity_MWh + "|" + limitedPower + "|" + unusedCapacity);

            rows.Add(new ScenarioRow()
            {
                Consumption = c,
                Time = key,
                Solar = solar,
                OnShore = onShore,
                OffShore = offShore,
                OtherRenewable = otherSum,
                DisplaySolar = solarDisplay,
                DisplayOnShore = onShoreDisplay,
                DisplayOffShore = offShoreDisplay,
                DisplayOtherRenewable = otherDisplay,
                CurrentCapacity = currentCapacity,
                CurrentPower = limitedPower,
                UnusedProduction = unusedCapacity,
                BatteryCapacityDelta = limitedCapacityDelta
            });
        }

        var result = new ScenarioResult()
        {
            Input = input.Copy(),
            Rows = rows,
        };
        
        return result;
    }
}
/*
public static class ScenarioHelpers
{
    public static void AddDisplay(this Frame<DateTimeOffset, CalculationColumn> fra,
        CalculationColumn display, params CalculationColumn[] columns)
    {
        fra.AddColumn(display, fra.CalculateDisplay(display, columns));
    }

    public static Series<DateTimeOffset, double> CalculateDisplay(this Frame<DateTimeOffset, CalculationColumn> fra,
        CalculationColumn display, params CalculationColumn[] columns)
    {
        var beforeCumultative = fra.Rows.Select(pair =>
            columns.Skip(1).Aggregate(0d, (i, column) => i + pair.Value.GetAs<double>(column)));
        var nowCumultative = beforeCumultative + fra[columns.First()];
        var consumption = fra[CalculationColumn.Consumption];
        // Min(now, consumption)
        var min = consumption.Zip(nowCumultative)
            .SelectValues(tuple => Math.Min(tuple.Item1.OrDefault(0), tuple.Item2.OrDefault(0)));
        // Max(min - before, 0)
        var max = min.Zip(beforeCumultative)
            .SelectValues(tuple => Math.Max(tuple.Item1.OrDefault(0) - tuple.Item2.OrDefault(0), 0));

        // Max(Min(now, consumption) - before, 0)
        // C
        return max;
    }
}
*/

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