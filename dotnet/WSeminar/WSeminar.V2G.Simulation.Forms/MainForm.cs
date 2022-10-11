using System.ComponentModel;
using System.Diagnostics;
using WSeminar.V2G.Simulator.Server.Smard;

namespace WSeminar.V2G.Simulation.Forms;

public partial class MainForm : Form
{
    private ScenarioInput CurrentInput;

    private DateTime _max => DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(2)).Date.AddDays(-1);
    private DateTime _min => new DateTimeOffset(2018, 1, 1, 0, 0, 0, TimeSpan.FromHours(2)).Date;
    public MainForm(ScenarioService service)
    {
        InitializeComponent();

        num_Days.Maximum = decimal.MaxValue;
        num_Days.Minimum = decimal.MinValue;
        OnDateChanged(null, EventArgs.Empty);
        date_End.ValueChanged += OnDateChanged;
        date_Start.ValueChanged += OnDateChanged;
        num_Days.ValueChanged += (sender, args) =>
        {
            date_Start.Value = date_End.Value.AddDays(-Convert.ToInt32(num_Days.Value));
            OnDateChanged(this, args);
        };
    }

    private void OnDateChanged(object? sender, EventArgs e)
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
        }
        else
        {
            num_Days.ResetBackColor();
        }
    }

    private void date_Start_ValueChanged(object sender, EventArgs e)
    {
        Debug.WriteLine("changed");
    }
}