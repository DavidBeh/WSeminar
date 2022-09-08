using System.Dynamic;
using System.Net.Http.Json;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

// ReSharper disable InconsistentNaming

namespace WSeminar.V2G.Simulator.WASM.Smard;

public class SmardClient
{
    private HttpClient _httpClient = new HttpClient() { BaseAddress = new Uri("https://www.smard.de/app/chart_data/") };

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

    public async Task<List<DateTime>> GetIndex(int filter, Resolution resolution)
    {

        var request = new HttpRequestMessage(HttpMethod.Get, $"{filter}/DE/index_{resolution.ToSmardString()}.json");
        request.SetBrowserRequestMode(BrowserRequestMode.NoCors);
        var res = await _httpClient.SendAsync(request);
        if (!res.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Error code returned from Smard api: " + res.ToString());
        }

        var json = await res.Content.ReadFromJsonAsync<JsonDocument>();

        return json!.RootElement.GetProperty("timestamps").EnumerateArray().Select(element =>
            DateTimeOffset.FromUnixTimeMilliseconds(element.GetInt64()).DateTime).ToList();
    }

    public enum Resolution
    {
        QuarterHour,
        Hour,
        Day,
        Week,
        Month,
        Year
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

public static class FilterExtensions
{
    public static SmardClient.FilterGroup[] GetGroups(this SmardClient.Filter filter)
    {
        var f = filter | SmardClient.Filter.Consumption_All;
        var filterName = filter.ToString();
        var filterGroup = filterName.Split('_')[0];
        return Enum.GetValues<SmardClient.FilterGroup>().Where(fg => fg.ToString() == filterGroup).ToArray();
    }

    public static string ToSmardString(this SmardClient.Resolution resolution) => resolution.ToString().ToLower();
}