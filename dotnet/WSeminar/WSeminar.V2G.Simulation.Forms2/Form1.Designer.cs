namespace WSeminar.V2G.Simulation.Forms2
{
    partial class Form1
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
            this.formsPlot1 = new ScottPlot.FormsPlot();
            this.inp_PVLeistung = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.inp_Wärmepumpe = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.inp_Jahresverbrauch = new System.Windows.Forms.TextBox();
            this.btn_Start = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // formsPlot1
            // 
            this.formsPlot1.Location = new System.Drawing.Point(13, 54);
            this.formsPlot1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.formsPlot1.Name = "formsPlot1";
            this.formsPlot1.Size = new System.Drawing.Size(774, 384);
            this.formsPlot1.TabIndex = 0;
            // 
            // inp_PVLeistung
            // 
            this.inp_PVLeistung.Location = new System.Drawing.Point(24, 27);
            this.inp_PVLeistung.Name = "inp_PVLeistung";
            this.inp_PVLeistung.Size = new System.Drawing.Size(100, 23);
            this.inp_PVLeistung.TabIndex = 1;
            this.inp_PVLeistung.Text = "12";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "PV-Leistung (kWp)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(185, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(206, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Jahresverbrauch Wärmepumpe (kWh)";
            // 
            // inp_Wärmepumpe
            // 
            this.inp_Wärmepumpe.Location = new System.Drawing.Point(185, 27);
            this.inp_Wärmepumpe.Name = "inp_Wärmepumpe";
            this.inp_Wärmepumpe.Size = new System.Drawing.Size(100, 23);
            this.inp_Wärmepumpe.TabIndex = 3;
            this.inp_Wärmepumpe.Text = "4200";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(419, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(179, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "Sonstiger Jahresverbrauch (kWh)";
            // 
            // inp_Jahresverbrauch
            // 
            this.inp_Jahresverbrauch.Location = new System.Drawing.Point(419, 27);
            this.inp_Jahresverbrauch.Name = "inp_Jahresverbrauch";
            this.inp_Jahresverbrauch.Size = new System.Drawing.Size(100, 23);
            this.inp_Jahresverbrauch.TabIndex = 5;
            this.inp_Jahresverbrauch.Text = "3500";
            // 
            // btn_Start
            // 
            this.btn_Start.Location = new System.Drawing.Point(713, 12);
            this.btn_Start.Name = "btn_Start";
            this.btn_Start.Size = new System.Drawing.Size(75, 23);
            this.btn_Start.TabIndex = 7;
            this.btn_Start.Text = "Berechnen";
            this.btn_Start.UseVisualStyleBackColor = true;
            this.btn_Start.Click += new System.EventHandler(this.btn_Start_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btn_Start);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.inp_Jahresverbrauch);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.inp_Wärmepumpe);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.inp_PVLeistung);
            this.Controls.Add(this.formsPlot1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ScottPlot.FormsPlot formsPlot1;
        private TextBox inp_PVLeistung;
        private Label label1;
        private Label label2;
        private TextBox inp_Wärmepumpe;
        private Label label3;
        private TextBox inp_Jahresverbrauch;
        private Button btn_Start;
    }
}