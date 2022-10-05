using System.Collections;
using System.Runtime.CompilerServices;
using Deedle;
using Microsoft.FSharp.Core;
using MoreLinq.Extensions;
using DateTimeOffset = System.DateTimeOffset;

namespace WSeminar.V2G.Simulator.Server.Smard;

public class Scenario
{
    public class BatteryScenario
    {
        public int CarsCount;
        public double UsableCapacity;
    }
    public int UsableBatteryCapacity;
    public int 
}

public class SmardDataContext
{
    private readonly SmardClient _client;

    
    
    public SmardDataContext(SmardClient client)
    {
        _client = client;
    }

    public async Task UpdateInput()
    {
        
    }

    public Dictionary<EnergySourceId, Series<DateTimeOffset, double?>> Productions { get; set; }
    
    public Series<DateTimeOffset, double?> GetProductionSums(EnergySourceId sources)
    {
        var n = Productions.Where(pair => sources.HasFlag(pair.Key)).Select(pair => pair.Value)
            .Pairwise((series, series1) =>
                series.Zip(series1).SelectValues(tuple => tuple.Item1.OrDefault(0) + tuple.Item2.OrDefault(0)));
        return n.Last();

    }
    
    
    
    
    public void AddSeries(EnergySourceId sourceId, Series<DateTimeOffset, double?> series)
    {
        Productions.Add(sourceId, series.SortByKey());
    }
}

public static class CustomSeriesExtensions
{
    public static Series<T, OptionalValue<K>> AddMissingKeys<T, K>(this Series<T, K> series, T[] keys) where T : IComparable<T>
    {
        var newKeys = keys.Union(series.Keys).OrderBy(comparable => comparable).ToList();
        return new Series<T, OptionalValue<K>>(newKeys, keys.Select(series.TryGet));
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