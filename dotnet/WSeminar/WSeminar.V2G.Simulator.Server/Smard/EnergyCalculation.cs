using System.Collections.ObjectModel;

namespace WSeminar.V2G.Simulator.Server.Smard;

public class EnergyCalculation
{
    private Dictionary<EnergySourceId, SeriesItem> _production;
    private readonly IEnumerable<SeriesItem> _consumption;
    private readonly Dictionary<EnergySourceId, SeriesItem> _installed;
    
    

    public EnergyCalculation(Dictionary<EnergySourceId, SeriesItem> production, IEnumerable<SeriesItem> consumption, Dictionary<EnergySourceId, SeriesItem> installed, DataResolution? resolution = null)
    {
        
        _production = production;
        _consumption = consumption;
        _installed = installed;
    }
}

