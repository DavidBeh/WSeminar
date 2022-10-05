// See https://aka.ms/new-console-template for more information

using System.Dynamic;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;
using System.Text.Json.Serialization;
using Deedle;
using Microsoft.Data.Analysis;



var s = Enumerable.Range(0, 5).Select(i =>  KeyValuePair.Create(i, (int?)(i % 2 == 0 ? i : null))).ToSeries();
s.Print();
var s1 = Enumerable.Range(3, 5).Select(i =>  KeyValuePair.Create(i, (int?)i)).ToSeries();
s1.Print();
var left = s.Zip(s1, JoinKind.);
left.Print();


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
