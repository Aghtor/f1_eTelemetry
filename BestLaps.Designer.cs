namespace f1_eTelemetry
{
    partial class BestLaps
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.chartTime = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.dGbestlap = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanelStat = new System.Windows.Forms.TableLayoutPanel();
            this.dataGridViewStatistique = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.chartStat = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tBvalue = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rBdriver = new System.Windows.Forms.RadioButton();
            this.rBgrille = new System.Windows.Forms.RadioButton();
            this.cBdrivers = new System.Windows.Forms.ComboBox();
            this.bUpdate = new System.Windows.Forms.Button();
            this.tBetat = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartTime)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGbestlap)).BeginInit();
            this.tableLayoutPanelStat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStatistique)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartStat)).BeginInit();
            this.tableLayoutPanel4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1855, 30);
            this.menuStrip1.Stretch = false;
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 26);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 173F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tBvalue, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tBetat, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 30);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1855, 648);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.chartTime, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(177, 4);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 246F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1674, 603);
            this.tableLayoutPanel2.TabIndex = 10;
            // 
            // chartTime
            // 
            this.chartTime.BackColor = System.Drawing.SystemColors.Control;
            chartArea1.Name = "ChartArea1";
            this.chartTime.ChartAreas.Add(chartArea1);
            this.chartTime.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.chartTime.Legends.Add(legend1);
            this.chartTime.Location = new System.Drawing.Point(4, 4);
            this.chartTime.Margin = new System.Windows.Forms.Padding(4);
            this.chartTime.Name = "chartTime";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chartTime.Series.Add(series1);
            this.chartTime.Size = new System.Drawing.Size(1666, 238);
            this.chartTime.TabIndex = 1;
            this.chartTime.Text = "chart1";
            this.chartTime.MouseMove += new System.Windows.Forms.MouseEventHandler(this.chartTime_MouseMove);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 1325F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel5, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanelStat, 1, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(4, 250);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1666, 349);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 98.38057F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1.619433F));
            this.tableLayoutPanel5.Controls.Add(this.dGbestlap, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(1317, 341);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // dGbestlap
            // 
            this.dGbestlap.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dGbestlap.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dGbestlap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dGbestlap.Location = new System.Drawing.Point(4, 4);
            this.dGbestlap.Margin = new System.Windows.Forms.Padding(4);
            this.dGbestlap.Name = "dGbestlap";
            this.dGbestlap.RowHeadersWidth = 51;
            this.dGbestlap.Size = new System.Drawing.Size(1287, 333);
            this.dGbestlap.TabIndex = 0;
            // 
            // tableLayoutPanelStat
            // 
            this.tableLayoutPanelStat.ColumnCount = 3;
            this.tableLayoutPanelStat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 17F));
            this.tableLayoutPanelStat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelStat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tableLayoutPanelStat.Controls.Add(this.dataGridViewStatistique, 1, 1);
            this.tableLayoutPanelStat.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanelStat.Controls.Add(this.chartStat, 1, 2);
            this.tableLayoutPanelStat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelStat.Location = new System.Drawing.Point(1329, 4);
            this.tableLayoutPanelStat.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanelStat.Name = "tableLayoutPanelStat";
            this.tableLayoutPanelStat.RowCount = 3;
            this.tableLayoutPanelStat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelStat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 185F));
            this.tableLayoutPanelStat.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelStat.Size = new System.Drawing.Size(333, 341);
            this.tableLayoutPanelStat.TabIndex = 1;
            this.tableLayoutPanelStat.Visible = false;
            // 
            // dataGridViewStatistique
            // 
            this.dataGridViewStatistique.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewStatistique.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewStatistique.Location = new System.Drawing.Point(21, 29);
            this.dataGridViewStatistique.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridViewStatistique.Name = "dataGridViewStatistique";
            this.dataGridViewStatistique.RowHeadersWidth = 51;
            this.dataGridViewStatistique.Size = new System.Drawing.Size(280, 175);
            this.dataGridViewStatistique.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(21, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Statistiques";
            // 
            // chartStat
            // 
            this.chartStat.BackColor = System.Drawing.SystemColors.Control;
            chartArea2.Name = "ChartArea1";
            this.chartStat.ChartAreas.Add(chartArea2);
            this.chartStat.Dock = System.Windows.Forms.DockStyle.Fill;
            legend2.Name = "Legend1";
            this.chartStat.Legends.Add(legend2);
            this.chartStat.Location = new System.Drawing.Point(21, 214);
            this.chartStat.Margin = new System.Windows.Forms.Padding(4);
            this.chartStat.Name = "chartStat";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.chartStat.Series.Add(series2);
            this.chartStat.Size = new System.Drawing.Size(281, 123);
            this.chartStat.TabIndex = 3;
            this.chartStat.Text = "chart";
            // 
            // tBvalue
            // 
            this.tBvalue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tBvalue.Location = new System.Drawing.Point(4, 615);
            this.tBvalue.Margin = new System.Windows.Forms.Padding(4);
            this.tBvalue.Name = "tBvalue";
            this.tBvalue.Size = new System.Drawing.Size(165, 22);
            this.tBvalue.TabIndex = 2;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.cBdrivers, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.bUpdate, 0, 2);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 5;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 134F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 142F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(165, 603);
            this.tableLayoutPanel4.TabIndex = 11;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rBdriver);
            this.groupBox1.Controls.Add(this.rBgrille);
            this.groupBox1.Location = new System.Drawing.Point(4, 4);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(104, 123);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Data";
            // 
            // rBdriver
            // 
            this.rBdriver.AutoSize = true;
            this.rBdriver.Location = new System.Drawing.Point(9, 52);
            this.rBdriver.Margin = new System.Windows.Forms.Padding(4);
            this.rBdriver.Name = "rBdriver";
            this.rBdriver.Size = new System.Drawing.Size(64, 20);
            this.rBdriver.TabIndex = 1;
            this.rBdriver.Text = "Driver";
            this.rBdriver.UseVisualStyleBackColor = true;
            // 
            // rBgrille
            // 
            this.rBgrille.AutoSize = true;
            this.rBgrille.Checked = true;
            this.rBgrille.Location = new System.Drawing.Point(9, 23);
            this.rBgrille.Margin = new System.Windows.Forms.Padding(4);
            this.rBgrille.Name = "rBgrille";
            this.rBgrille.Size = new System.Drawing.Size(59, 20);
            this.rBgrille.TabIndex = 0;
            this.rBgrille.TabStop = true;
            this.rBgrille.Text = "Grille";
            this.rBgrille.UseVisualStyleBackColor = true;
            this.rBgrille.CheckedChanged += new System.EventHandler(this.rBgrille_CheckedChanged);
            // 
            // cBdrivers
            // 
            this.cBdrivers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cBdrivers.FormattingEnabled = true;
            this.cBdrivers.Location = new System.Drawing.Point(4, 138);
            this.cBdrivers.Margin = new System.Windows.Forms.Padding(4);
            this.cBdrivers.Name = "cBdrivers";
            this.cBdrivers.Size = new System.Drawing.Size(157, 24);
            this.cBdrivers.TabIndex = 8;
            this.cBdrivers.SelectedIndexChanged += new System.EventHandler(this.cBdrivers_SelectedIndexChanged);
            // 
            // bUpdate
            // 
            this.bUpdate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bUpdate.Image = global::f1_eTelemetry.Properties.Resources.refresh35x43;
            this.bUpdate.Location = new System.Drawing.Point(0, 171);
            this.bUpdate.Margin = new System.Windows.Forms.Padding(0);
            this.bUpdate.Name = "bUpdate";
            this.bUpdate.Size = new System.Drawing.Size(47, 53);
            this.bUpdate.TabIndex = 1;
            this.bUpdate.Text = "UP";
            this.bUpdate.UseVisualStyleBackColor = true;
            this.bUpdate.Click += new System.EventHandler(this.bUpdate_Click);
            // 
            // tBetat
            // 
            this.tBetat.Location = new System.Drawing.Point(177, 615);
            this.tBetat.Margin = new System.Windows.Forms.Padding(4);
            this.tBetat.Name = "tBetat";
            this.tBetat.Size = new System.Drawing.Size(1476, 22);
            this.tBetat.TabIndex = 12;
            // 
            // BestLaps
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1855, 678);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "BestLaps";
            this.Text = "BestLaps";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartTime)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dGbestlap)).EndInit();
            this.tableLayoutPanelStat.ResumeLayout(false);
            this.tableLayoutPanelStat.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStatistique)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartStat)).EndInit();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView dGbestlap;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartTime;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rBdriver;
        private System.Windows.Forms.RadioButton rBgrille;
        private System.Windows.Forms.Button bUpdate;
        private System.Windows.Forms.TextBox tBvalue;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TextBox tBetat;
        private System.Windows.Forms.ComboBox cBdrivers;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelStat;
        private System.Windows.Forms.DataGridView dataGridViewStatistique;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartStat;
    }
}