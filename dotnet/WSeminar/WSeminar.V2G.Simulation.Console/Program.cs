// See https://aka.ms/new-console-template for more information

using System.Globalization;
using Deedle;
using XPlot.Plotly;

Console.WriteLine("Hello, World!");

using var erzeugungStream = File.OpenRead("Realisierte_Erzeugung_202209250000_202210052359.csv");
var erzeugung = Frame.ReadCsv(erzeugungStream, true, false, separators: ";", culture: "de-DE").InitSmardCsv().Where(pair => pair.Key < DateTime.Today);

erzeugung.Print();




var erneuerbare = erzeugung.Rows.SelectValues(series => series.IndexOrdinally().EndAt(5).Sum());
var konventionelle = erzeugung.Rows.SelectValues(series => series.IndexOrdinally().After(5).Sum());
Chart.Plot(new Bar() {}).


static class SmardCsvExtensions
{
    private static CultureInfo _cultureInfo = CultureInfo.CreateSpecificCulture("de-DE");
    

    internal static Frame<DateTime, string> InitSmardCsv(this Frame<int, string> source)
    {
        var indexed = source.IndexRowsUsing(series =>
        {
            var datum = DateOnly.Parse(series.GetAtAs<string>(0), _cultureInfo);
            var zeit = TimeOnly.Parse(series.GetAtAs<string>(1), _cultureInfo);

            return datum.ToDateTime(zeit);
        });
        indexed.RenameColumns(s => s.Replace("[MWh]", "").Trim());
        indexed.DropColumn("Datum");
        indexed.DropColumn("Uhrzeit");
        return indexed.SelectValues<string, double>(s =>
            double.TryParse(s, NumberStyles.Any, _cultureInfo, out var d) ? d : 0);
    }
}