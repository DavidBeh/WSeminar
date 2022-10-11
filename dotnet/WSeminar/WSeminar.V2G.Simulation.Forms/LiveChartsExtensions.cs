using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using WSeminar.V2G.Simulator.Server.Smard;

namespace WSeminar.V2G.Simulation.Forms
{
    internal static class LiveChartsExtensions
    {
        internal static StackedAreaSeries<ScenarioRow> CreateStackedSeries(this ScenarioResult r, Func<ScenarioRow, double> sel, String name,
            Color color)
        {
            var fill = Color.FromArgb(200, color);
            var stroke = Color.FromArgb(255, color);

            return new StackedAreaSeries<ScenarioRow>()
            {
                Values = r.Rows,
                Mapping = (row, point) =>
                {
                    point.PrimaryValue = sel(row);
                    point.SecondaryValue = row.Time.Ticks;
                },
                Fill = new SolidColorPaint(fill.ToSKColor()),
                Stroke = new SolidColorPaint(stroke.ToSKColor(), 5),
                Name = name,
            };
        }
    }
}
