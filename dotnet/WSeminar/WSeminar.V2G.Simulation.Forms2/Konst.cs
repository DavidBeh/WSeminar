namespace WSeminar.V2G.Simulation.Forms2
{
    internal class Konst
    {
        internal static readonly double[] ErtragProMonat;
        internal static readonly double[] HeizFaktorProMonat;
        internal static readonly TimeSpan[] TagesLänge;
        internal static readonly TimeOnly[] SonnenAufg;

        static Konst()
        {
            ErtragProMonat = new double[] { 28, 42, 96, 143, 137, 119, 131, 115, 100, 46, 37, 16 };
            var gradTagsZ = new double[]
                { 170, 150, 130, 80, 40, 13.33333333, 13.33333333, 13.33333333, 30, 80, 120, 160 };
            HeizFaktorProMonat = gradTagsZ.Select(d => d / 1000d).ToArray();

            TagesLänge = new TimeOnly[]
            {
                TimeOnly.Parse("8:16"), TimeOnly.Parse("10:00"), TimeOnly.Parse("11:53"), TimeOnly.Parse("14:00"),
                TimeOnly.Parse("15:50"), TimeOnly.Parse("16:53"), TimeOnly.Parse("16:25"), TimeOnly.Parse("14:47"),
                TimeOnly.Parse("12:45"), TimeOnly.Parse("10:44"), TimeOnly.Parse("8:49"), TimeOnly.Parse("7:46")
            }.Select(only => RundeMinute(only).ToTimeSpan()).ToArray();

            SonnenAufg = new TimeOnly[]
            {
                TimeOnly.Parse("08:07"), TimeOnly.Parse("07:20"), TimeOnly.Parse("06:18"), TimeOnly.Parse("06:06"),
                TimeOnly.Parse("05:07"), TimeOnly.Parse("04:40"), TimeOnly.Parse("04:59"), TimeOnly.Parse("05:47"),
                TimeOnly.Parse("06:39"), TimeOnly.Parse("07:30"), TimeOnly.Parse("07:26"), TimeOnly.Parse("08:08")
            }.Select(RundeMinute).ToArray();


        }

        internal static TimeOnly RundeMinute(TimeOnly t)
        {
            var n = t.Minute / 15d;

            var min = (int)Math.Round(n, MidpointRounding.ToPositiveInfinity) * 15;

            return new TimeOnly(min is 60 ? t.Hour + 1 : t.Hour, min is 60 ? 0 : min);
        }
    }
}