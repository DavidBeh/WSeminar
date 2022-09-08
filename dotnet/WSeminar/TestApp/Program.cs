// See https://aka.ms/new-console-template for more information

using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

Energy.EnergySource s = Energy.EnergySource.Conventional | Energy.EnergySource.Water;

var n = s switch
{
    Energy.EnergySource.Conventional => 2,
    _ => 0,
};

Console.WriteLine(n);

class Energy
{
    [Flags]
    public enum EnergySource
    {
        Bio = 0,
        Water = 1 << 0,
        WindOffshore = 1 << 1,
        WindOnshore = 1 << 2,
        Solar = 1 << 3,
        Nuclear = 1 << 4,
        BrownCoal = 1 << 5,
        HardCoal = 1 << 6,
        Gas = 1 << 7,
        PumpedHydro = 1 << 8,

        All = Bio | Water | WindOffshore | WindOnshore | Solar | Nuclear | BrownCoal | HardCoal | Gas | PumpedHydro,
        Renewable = Bio | Water | WindOffshore | WindOnshore | Solar | PumpedHydro,
        Conventional = Nuclear | BrownCoal | HardCoal | Gas
    }
}
