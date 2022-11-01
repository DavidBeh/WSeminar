namespace WSeminar.V2G.Simulation.Forms2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            double res = 0;
            if (!double.TryParse(inp_PVLeistung.Text, out res))
            {
                MessageBox.Show(this, "PV Leistung ung�ltig");
                return;
            }

            _day.PVLeistung = res;
            if (!double.TryParse(inp_W�rmepumpe.Text, out res))
            {
                MessageBox.Show(this, "Jahresverbrauch W�rmepumpe ung�ltig");
                return;
            }

            _day.W�rmepumpe = res;
            if (!double.TryParse(inp_Jahresverbrauch.Text, out res))
            {
                MessageBox.Show(this, "Jahresverbrauch ung�ltig");
                return;
            }

            _day.SonstStromverbrauch = res;
        }

        private DayInput _day = new DayInput();
    }

    internal class DayInput
    {
        /// <summary>
        /// Jahresverbrauch in kWh
        /// </summary>
        internal double W�rmepumpe;

        /// <summary>
        /// PV-Anlagenleistung in kWp
        /// </summary>
        internal double PVLeistung;

        /// <summary>
        /// Sonstiger Stromverbrauch in kWp
        /// </summary>
        internal double SonstStromverbrauch;

        internal int Monat = 2;

        internal Dictionary<TimeOnly, DayRow> Calculate()
        {
            var sonnenAufgang = Konst.SonnenAufg[Monat];
            var mittag = Konst.RundeMinute(sonnenAufgang.AddMinutes(Konst.TagesL�nge[Monat].TotalMinutes / 2));
            var curr = new TimeOnly(0, 0);
            while (curr != new TimeOnly(23,45))
            {


                curr.AddMinutes(15);
            }
        }

    }

    internal class DayRow
    {
        internal double PVNutz;
        internal double PVUnterprod;
        internal double PV�berProd;
    }
}