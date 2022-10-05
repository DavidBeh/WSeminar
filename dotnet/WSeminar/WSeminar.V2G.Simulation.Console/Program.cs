// See https://aka.ms/new-console-template for more information

using System.Globalization;
using Deedle;

Console.WriteLine("Hello, World!");

var erzeugungStream = File.OpenRead("Realisierte_Erzeugung_202209250000_202210052359.csv");
var culture = CultureInfo.CreateSpecificCulture("de-DE");
var erzeugungRoh = Frame.ReadCsv(erzeugungStream, true, separators:";");
var indexed = erzeugungRoh.IndexRowsUsing(series =>
{
    var datum = DateOnly.Parse(series.GetAtAs<string>(0), culture);
    var zeit = TimeOnly.Parse(series.GetAtAs<string>(1), culture);
    return new DateTime(datum.Year, datum.Month, datum.Day, zeit.Hour, zeit.Minute, zeit.Second);


});

indexed.Print();