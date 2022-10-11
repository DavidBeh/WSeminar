namespace WSeminar.V2G.Simulation.Forms;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.date_Start = new System.Windows.Forms.DateTimePicker();
            this.date_End = new System.Windows.Forms.DateTimePicker();
            this.num_Days = new System.Windows.Forms.NumericUpDown();
            this.chart_Main = new LiveChartsCore.SkiaSharpView.WinForms.CartesianChart();
            this.btn_Start = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.num_Days)).BeginInit();
            this.SuspendLayout();
            // 
            // date_Start
            // 
            this.date_Start.Location = new System.Drawing.Point(12, 12);
            this.date_Start.Name = "date_Start";
            this.date_Start.Size = new System.Drawing.Size(200, 23);
            this.date_Start.TabIndex = 0;
            // 
            // date_End
            // 
            this.date_End.Location = new System.Drawing.Point(287, 12);
            this.date_End.Name = "date_End";
            this.date_End.Size = new System.Drawing.Size(200, 23);
            this.date_End.TabIndex = 2;
            // 
            // num_Days
            // 
            this.num_Days.Location = new System.Drawing.Point(218, 12);
            this.num_Days.Name = "num_Days";
            this.num_Days.Size = new System.Drawing.Size(63, 23);
            this.num_Days.TabIndex = 1;
            // 
            // chart_Main
            // 
            this.chart_Main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chart_Main.Location = new System.Drawing.Point(12, 41);
            this.chart_Main.Name = "chart_Main";
            this.chart_Main.Size = new System.Drawing.Size(776, 397);
            this.chart_Main.TabIndex = 3;
            // 
            // btn_Start
            // 
            this.btn_Start.Location = new System.Drawing.Point(493, 12);
            this.btn_Start.Name = "btn_Start";
            this.btn_Start.Size = new System.Drawing.Size(75, 23);
            this.btn_Start.TabIndex = 3;
            this.btn_Start.Text = "Start";
            this.btn_Start.UseVisualStyleBackColor = true;
            this.btn_Start.Click += new System.EventHandler(this.btn_Start_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btn_Start);
            this.Controls.Add(this.chart_Main);
            this.Controls.Add(this.num_Days);
            this.Controls.Add(this.date_End);
            this.Controls.Add(this.date_Start);
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Text = "V2G-Simulation";
            ((System.ComponentModel.ISupportInitialize)(this.num_Days)).EndInit();
            this.ResumeLayout(false);

    }

    #endregion

    private DateTimePicker date_Start;
    private DateTimePicker date_End;
    private NumericUpDown num_Days;
    private LiveChartsCore.SkiaSharpView.WinForms.CartesianChart chart_Main;
    private Button btn_Start;
}