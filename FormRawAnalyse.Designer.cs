namespace f1_eTelemetry
{
    partial class FormRawAnalyse
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
            this.rTBterminal = new System.Windows.Forms.RichTextBox();
            this.caurelien = new System.Windows.Forms.TableLayoutPanel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fichierToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.analyseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.traitementToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitMutlRaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dGVanalyse = new System.Windows.Forms.DataGridView();
            this.packetOneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.caurelien.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGVanalyse)).BeginInit();
            this.SuspendLayout();
            // 
            // rTBterminal
            // 
            this.rTBterminal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rTBterminal.Location = new System.Drawing.Point(103, 289);
            this.rTBterminal.Name = "rTBterminal";
            this.rTBterminal.Size = new System.Drawing.Size(594, 114);
            this.rTBterminal.TabIndex = 0;
            this.rTBterminal.Text = "";
            // 
            // caurelien
            // 
            this.caurelien.ColumnCount = 3;
            this.caurelien.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.caurelien.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.caurelien.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.caurelien.Controls.Add(this.rTBterminal, 1, 1);
            this.caurelien.Controls.Add(this.dGVanalyse, 1, 0);
            this.caurelien.Dock = System.Windows.Forms.DockStyle.Fill;
            this.caurelien.Location = new System.Drawing.Point(0, 24);
            this.caurelien.Name = "caurelien";
            this.caurelien.RowCount = 3;
            this.caurelien.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.caurelien.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.caurelien.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.caurelien.Size = new System.Drawing.Size(800, 426);
            this.caurelien.TabIndex = 1;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fichierToolStripMenuItem,
            this.analyseToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fichierToolStripMenuItem
            // 
            this.fichierToolStripMenuItem.Name = "fichierToolStripMenuItem";
            this.fichierToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.fichierToolStripMenuItem.Text = "Fichier";
            // 
            // analyseToolStripMenuItem
            // 
            this.analyseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.traitementToolStripMenuItem,
            this.splitMutlRaceToolStripMenuItem,
            this.packetOneToolStripMenuItem});
            this.analyseToolStripMenuItem.Name = "analyseToolStripMenuItem";
            this.analyseToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
            this.analyseToolStripMenuItem.Text = "Analyse";
            // 
            // traitementToolStripMenuItem
            // 
            this.traitementToolStripMenuItem.Name = "traitementToolStripMenuItem";
            this.traitementToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.traitementToolStripMenuItem.Text = "Traitement";
            this.traitementToolStripMenuItem.Click += new System.EventHandler(this.traitementToolStripMenuItem_Click);
            // 
            // splitMutlRaceToolStripMenuItem
            // 
            this.splitMutlRaceToolStripMenuItem.Name = "splitMutlRaceToolStripMenuItem";
            this.splitMutlRaceToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.splitMutlRaceToolStripMenuItem.Text = "Split mutlti race";
            this.splitMutlRaceToolStripMenuItem.Click += new System.EventHandler(this.splitMutlRaceToolStripMenuItem_Click_1);
            // 
            // dGVanalyse
            // 
            this.dGVanalyse.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dGVanalyse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dGVanalyse.Location = new System.Drawing.Point(103, 3);
            this.dGVanalyse.Name = "dGVanalyse";
            this.dGVanalyse.Size = new System.Drawing.Size(594, 280);
            this.dGVanalyse.TabIndex = 1;
            // 
            // packetOneToolStripMenuItem
            // 
            this.packetOneToolStripMenuItem.Name = "packetOneToolStripMenuItem";
            this.packetOneToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.packetOneToolStripMenuItem.Text = "Packet One";
            this.packetOneToolStripMenuItem.Click += new System.EventHandler(this.packetOneToolStripMenuItem_Click);
            // 
            // FormRawAnalyse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.caurelien);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormRawAnalyse";
            this.Text = "Traitement raw data";
            this.caurelien.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGVanalyse)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rTBterminal;
        private System.Windows.Forms.TableLayoutPanel caurelien;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fichierToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem analyseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem traitementToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem splitMutlRaceToolStripMenuItem;
        private System.Windows.Forms.DataGridView dGVanalyse;
        private System.Windows.Forms.ToolStripMenuItem packetOneToolStripMenuItem;
    }
}