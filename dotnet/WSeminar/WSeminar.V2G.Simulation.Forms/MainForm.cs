using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp.Views.Desktop;
using WSeminar.V2G.Simulator.Server.Smard;

namespace WSeminar.V2G.Simulation.Forms;

public partial class MainForm : Form
{
    private readonly ScenarioService _scenario;
    private ScenarioInput CurrentInput;

    private DateTime _max => DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(2)).Date.AddDays(-1);
    private DateTime _min => new DateTimeOffset(2018, 1, 1, 0, 0, 0, TimeSpan.FromHours(2)).Date;

    public MainForm(ScenarioService scenario)
    {
        _scenario = scenario;
        InitializeComponent();
        num_Days.Maximum = decimal.MaxValue;
        num_Days.Minimum = decimal.MinValue;
        ResetDates();

        date_End.ValueChanged += (sender, args) => UpdateDates(sender, args);
        date_Start.ValueChanged += (sender, args) => UpdateDates(sender, args);
        num_Days.ValueChanged += (sender, args) =>
        {
            date_Start.Value = date_End.Value.AddDays(-Convert.ToInt32(num_Days.Value));
            UpdateDates(this, args);
        };
    }

    private void ResetDates()
    {
        UpdateDates();
        date_End.Value = date_End.MaxDate;
        UpdateDates();
        var tryEndDate = date_End.Value.AddDays(-7);
        date_Start.Value = date_Start.MinDate > tryEndDate ? date_Start.MinDate : tryEndDate;
        UpdateDates();
    }

    private bool UpdateDates(object? sender = null, EventArgs? e = null)
    {
        date_End.MaxDate = _max;
        date_End.MinDate = _min.AddDays(1);

        date_Start.MaxDate = _max.AddDays(-1);
        date_Start.MinDate = _min;

        num_Days.Maximum = (date_End.Value.Date - date_Start.MinDate).Days;
        num_Days.Minimum = (date_End.Value.Date - date_Start.MaxDate).Days;

        var difference = date_End.Value - date_Start.Value;
        num_Days.Value = difference.Days;

        if (difference.Days < 1)
        {
            num_Days.BackColor = Color.Orange;
            btn_Start.Enabled = false;
            return false;
        }
        else
        {
            num_Days.ResetBackColor();
            btn_Start.Enabled = true;
            return true;
        }
    }

    private async void btn_Start_Click(object? sender, EventArgs e)
    {
        if (UpdateDates() is false) return;
        try
        {
            var input = new ScenarioInput()
            {
                Start = new DateTimeOffset(date_Start.Value),
                End = new DateTimeOffset(date_End.Value),
                Resolution = DataResolution.Hour,
            };
            btn_Start.Enabled = false;

            var res = await _scenario.Calculate(input);
            UpdateResult(res);
        }
        finally
        {
            btn_Start.Enabled = true;
        }
    }

    private void UpdateResult(ScenarioResult result)
    {
        chart_Main.XAxes = new[]
        {
            new Axis()
            {

                MinLimit = result.Input.Start.Ticks,
                UnitWidth = TimeSpan.FromDays(1).Ticks,
                LabelsRotation = 60,
                MinStep = TimeSpan.FromDays(1).Ticks,
                Labeler = d => new DateTime((long) d).ToString("dd.MM HH:mm"),
                //MinStep = TimeSpan.FromDays(1).Ticks,
                SeparatorsPaint = new SolidColorPaint(Color.Gray.ToSKColor(), 2),
            }
        };
        chart_Main.Series = new[]
        {
            result.CreateStackedSeries(row => row.DisplayOtherRenewable, "Andere Erneuerbare", Color.LightSeaGreen),
            result.CreateStackedSeries(row => row.DisplayOffShore, "Offshore", Color.LightSkyBlue),
            result.CreateStackedSeries(row => row.OnShore, "Onshore", Color.DeepSkyBlue),
            result.CreateStackedSeries(row => row.Solar, "Solar", Color.Yellow),
        };

        chart_Main.TooltipFindingStrategy = TooltipFindingStrategy.CompareOnlyXTakeClosest;
        chart_Main.TooltipPosition = TooltipPosition.Top;


    }
}