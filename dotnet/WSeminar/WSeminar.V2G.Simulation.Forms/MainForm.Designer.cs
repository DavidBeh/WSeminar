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
            this.date_End.Location = new System.Drawing.Point(265, 12);
            this.date_End.Name = "date_End";
            this.date_End.Size = new System.Drawing.Size(200, 23);
            this.date_End.TabIndex = 1;
            // 
            // num_Days
            // 
            this.num_Days.Location = new System.Drawing.Point(218, 12);
            this.num_Days.Name = "num_Days";
            this.num_Days.Size = new System.Drawing.Size(41, 23);
            this.num_Days.TabIndex = 2;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.num_Days);
            this.Controls.Add(this.date_End);
            this.Controls.Add(this.date_Start);
            this.Name = "MainForm";
            this.Text = "V2G-Simulation";
            ((System.ComponentModel.ISupportInitialize)(this.num_Days)).EndInit();
            this.ResumeLayout(false);

    }

    #endregion

    private DateTimePicker date_Start;
    private DateTimePicker date_End;
    private NumericUpDown num_Days;
}