using System.Collections.ObjectModel;

namespace WSeminar.V2G.Simulator.Server.Smard;

public class EnergyCalculation
{
    private Dictionary<EnergySource, SeriesItem> _production;
    private readonly IEnumerable<SeriesItem> _consumption;
    private readonly Dictionary<EnergySource, SeriesItem> _installed;
    
    

    public EnergyCalculation(Dictionary<EnergySource, SeriesItem> production, IEnumerable<SeriesItem> consumption, Dictionary<EnergySource, SeriesItem> installed, DataResolution? resolution = null)
    {
        
        _production = production;
        _consumption = consumption;
        _installed = installed;
    }
}

