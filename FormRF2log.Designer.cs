namespace f1_eTelemetry
{
    partial class fmRF2log
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        /// 
        /* Franck 2023 04 27
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        */
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.view = new System.Windows.Forms.PictureBox();
            this.tB_info = new System.Windows.Forms.TextBox();
            this.oFD_RAWdata = new System.Windows.Forms.OpenFileDialog();
            this.sFD_RAWdata = new System.Windows.Forms.SaveFileDialog();
            this.mS_MenuPrincipal = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.analyseFichierToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.saveDataRawToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDataRawToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uDPSettingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.myIPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.generalSettingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.temetrieToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.temetrieGraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bestTimeLapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.traitementRawDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.databaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.transfertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.requestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.databaseSettingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enablePitInputsToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rB2Files = new System.Windows.Forms.RadioButton();
            this.rB1File = new System.Windows.Forms.RadioButton();
            this.cBTelLight = new System.Windows.Forms.CheckBox();
            this.cBcheckfile = new System.Windows.Forms.CheckBox();
            this.btnOverlay = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.view)).BeginInit();
            this.mS_MenuPrincipal.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // view
            // 
            this.view.BackColor = System.Drawing.Color.Transparent;
            this.view.ErrorImage = null;
            this.view.Location = new System.Drawing.Point(247, 49);
            this.view.Margin = new System.Windows.Forms.Padding(4);
            this.view.Name = "view";
            this.view.Size = new System.Drawing.Size(804, 401);
            this.view.TabIndex = 0;
            this.view.TabStop = false;
            // 
            // tB_info
            // 
            this.tB_info.Location = new System.Drawing.Point(12, 482);
            this.tB_info.Margin = new System.Windows.Forms.Padding(0);
            this.tB_info.Multiline = true;
            this.tB_info.Name = "tB_info";
            this.tB_info.Size = new System.Drawing.Size(1041, 34);
            this.tB_info.TabIndex = 3;
            // 
            // oFD_RAWdata
            // 
            this.oFD_RAWdata.FileName = "*.bin";
            // 
            // mS_MenuPrincipal
            // 
            this.mS_MenuPrincipal.AutoSize = false;
            this.mS_MenuPrincipal.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mS_MenuPrincipal.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.temetrieToolStripMenuItem,
            this.databaseToolStripMenuItem});
            this.mS_MenuPrincipal.Location = new System.Drawing.Point(0, 0);
            this.mS_MenuPrincipal.Name = "mS_MenuPrincipal";
            this.mS_MenuPrincipal.Size = new System.Drawing.Size(1067, 30);
            this.mS_MenuPrincipal.TabIndex = 7;
            this.mS_MenuPrincipal.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.toolStripSeparator1,
            this.analyseFichierToolStripMenuItem,
            this.toolStripSeparator2,
            this.saveDataRawToolStripMenuItem,
            this.openDataRawToolStripMenuItem,
            this.toolStripSeparator3,
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 26);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(196, 26);
            this.newToolStripMenuItem.Text = "New";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(193, 6);
            // 
            // analyseFichierToolStripMenuItem
            // 
            this.analyseFichierToolStripMenuItem.Name = "analyseFichierToolStripMenuItem";
            this.analyseFichierToolStripMenuItem.Size = new System.Drawing.Size(196, 26);
            this.analyseFichierToolStripMenuItem.Text = "Analyse fichiers";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(193, 6);
            // 
            // saveDataRawToolStripMenuItem
            // 
            this.saveDataRawToolStripMenuItem.Name = "saveDataRawToolStripMenuItem";
            this.saveDataRawToolStripMenuItem.Size = new System.Drawing.Size(196, 26);
            this.saveDataRawToolStripMenuItem.Text = "Save Data Raw";
            // 
            // openDataRawToolStripMenuItem
            // 
            this.openDataRawToolStripMenuItem.Name = "openDataRawToolStripMenuItem";
            this.openDataRawToolStripMenuItem.Size = new System.Drawing.Size(196, 26);
            this.openDataRawToolStripMenuItem.Text = "Open Data Raw";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(193, 6);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(196, 26);
            this.quitToolStripMenuItem.Text = "Quit";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.uDPSettingToolStripMenuItem,
            this.myIPToolStripMenuItem,
            this.toolStripSeparator4,
            this.generalSettingToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(49, 26);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // uDPSettingToolStripMenuItem
            // 
            this.uDPSettingToolStripMenuItem.Name = "uDPSettingToolStripMenuItem";
            this.uDPSettingToolStripMenuItem.Size = new System.Drawing.Size(194, 26);
            this.uDPSettingToolStripMenuItem.Text = "UDP Setting";
            // 
            // myIPToolStripMenuItem
            // 
            this.myIPToolStripMenuItem.Name = "myIPToolStripMenuItem";
            this.myIPToolStripMenuItem.Size = new System.Drawing.Size(194, 26);
            this.myIPToolStripMenuItem.Text = "My IP";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(191, 6);
            // 
            // generalSettingToolStripMenuItem
            // 
            this.generalSettingToolStripMenuItem.Name = "generalSettingToolStripMenuItem";
            this.generalSettingToolStripMenuItem.Size = new System.Drawing.Size(194, 26);
            this.generalSettingToolStripMenuItem.Text = "General Setting";
            // 
            // temetrieToolStripMenuItem
            // 
            this.temetrieToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.temetrieGraphToolStripMenuItem,
            this.bestTimeLapToolStripMenuItem,
            this.traitementRawDataToolStripMenuItem});
            this.temetrieToolStripMenuItem.Name = "temetrieToolStripMenuItem";
            this.temetrieToolStripMenuItem.Size = new System.Drawing.Size(64, 26);
            this.temetrieToolStripMenuItem.Text = "Panels";
            // 
            // temetrieGraphToolStripMenuItem
            // 
            this.temetrieGraphToolStripMenuItem.Name = "temetrieGraphToolStripMenuItem";
            this.temetrieGraphToolStripMenuItem.Size = new System.Drawing.Size(225, 26);
            this.temetrieGraphToolStripMenuItem.Text = "Telemetry";
            // 
            // bestTimeLapToolStripMenuItem
            // 
            this.bestTimeLapToolStripMenuItem.Name = "bestTimeLapToolStripMenuItem";
            this.bestTimeLapToolStripMenuItem.Size = new System.Drawing.Size(225, 26);
            this.bestTimeLapToolStripMenuItem.Text = "Best time Lap";
            // 
            // traitementRawDataToolStripMenuItem
            // 
            this.traitementRawDataToolStripMenuItem.Name = "traitementRawDataToolStripMenuItem";
            this.traitementRawDataToolStripMenuItem.Size = new System.Drawing.Size(225, 26);
            this.traitementRawDataToolStripMenuItem.Text = "Traitement raw data";
            // 
            // databaseToolStripMenuItem
            // 
            this.databaseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.transfertToolStripMenuItem,
            this.requestToolStripMenuItem,
            this.databaseSettingToolStripMenuItem});
            this.databaseToolStripMenuItem.Name = "databaseToolStripMenuItem";
            this.databaseToolStripMenuItem.Size = new System.Drawing.Size(86, 26);
            this.databaseToolStripMenuItem.Text = "Database";
            // 
            // transfertToolStripMenuItem
            // 
            this.transfertToolStripMenuItem.Name = "transfertToolStripMenuItem";
            this.transfertToolStripMenuItem.Size = new System.Drawing.Size(206, 26);
            this.transfertToolStripMenuItem.Text = "Transfert";
            // 
            // requestToolStripMenuItem
            // 
            this.requestToolStripMenuItem.Name = "requestToolStripMenuItem";
            this.requestToolStripMenuItem.Size = new System.Drawing.Size(206, 26);
            this.requestToolStripMenuItem.Text = "Request";
            // 
            // databaseSettingToolStripMenuItem
            // 
            this.databaseSettingToolStripMenuItem.Name = "databaseSettingToolStripMenuItem";
            this.databaseSettingToolStripMenuItem.Size = new System.Drawing.Size(206, 26);
            this.databaseSettingToolStripMenuItem.Text = "Database Setting";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rB2Files);
            this.groupBox1.Controls.Add(this.rB1File);
            this.groupBox1.Location = new System.Drawing.Point(16, 49);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(184, 130);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Data File";
            // 
            // rB2Files
            // 
            this.rB2Files.AutoSize = true;
            this.rB2Files.Checked = true;
            this.rB2Files.Location = new System.Drawing.Point(1, 73);
            this.rB2Files.Margin = new System.Windows.Forms.Padding(4);
            this.rB2Files.Name = "rB2Files";
            this.rB2Files.Size = new System.Drawing.Size(166, 20);
            this.rB2Files.TabIndex = 1;
            this.rB2Files.TabStop = true;
            this.rB2Files.Text = "Two Files (Tel - Others)";
            this.rB2Files.UseVisualStyleBackColor = true;
            // 
            // rB1File
            // 
            this.rB1File.AutoSize = true;
            this.rB1File.Location = new System.Drawing.Point(1, 43);
            this.rB1File.Margin = new System.Windows.Forms.Padding(4);
            this.rB1File.Name = "rB1File";
            this.rB1File.Size = new System.Drawing.Size(78, 20);
            this.rB1File.TabIndex = 0;
            this.rB1File.Text = "One File";
            this.rB1File.UseVisualStyleBackColor = true;
            // 
            // cBTelLight
            // 
            this.cBTelLight.AutoSize = true;
            this.cBTelLight.Checked = true;
            this.cBTelLight.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cBTelLight.Location = new System.Drawing.Point(17, 187);
            this.cBTelLight.Margin = new System.Windows.Forms.Padding(4);
            this.cBTelLight.Name = "cBTelLight";
            this.cBTelLight.Size = new System.Drawing.Size(159, 20);
            this.cBTelLight.TabIndex = 9;
            this.cBTelLight.Text = "Données essentielles";
            this.cBTelLight.UseVisualStyleBackColor = true;
            // 
            // cBcheckfile
            // 
            this.cBcheckfile.AutoSize = true;
            this.cBcheckfile.Location = new System.Drawing.Point(17, 251);
            this.cBcheckfile.Margin = new System.Windows.Forms.Padding(4);
            this.cBcheckfile.Name = "cBcheckfile";
            this.cBcheckfile.Size = new System.Drawing.Size(143, 20);
            this.cBcheckfile.TabIndex = 10;
            this.cBcheckfile.Text = "Check sauvegarde";
            this.cBcheckfile.UseVisualStyleBackColor = true;
            // 
            // btnOverlay
            // 
            this.btnOverlay.Location = new System.Drawing.Point(16, 301);
            this.btnOverlay.Name = "btnOverlay";
            this.btnOverlay.Size = new System.Drawing.Size(167, 38);
            this.btnOverlay.TabIndex = 11;
            this.btnOverlay.Text = "Overlay";
            this.btnOverlay.UseVisualStyleBackColor = true;
            this.btnOverlay.Click += new System.EventHandler(this.btnOverlay_Click);
            // 
            // fmRF2log
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1067, 554);
            this.Controls.Add(this.btnOverlay);
            this.Controls.Add(this.cBcheckfile);
            this.Controls.Add(this.cBTelLight);
            this.Controls.Add(this.mS_MenuPrincipal);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.tB_info);
            this.Controls.Add(this.view);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "fmRF2log";
            this.Text = "eTelemetry RF2 log Telemetry";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.fmRF2log_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.view)).EndInit();
            this.mS_MenuPrincipal.ResumeLayout(false);
            this.mS_MenuPrincipal.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox view;
        private System.Windows.Forms.TextBox tB_info;
        private System.Windows.Forms.OpenFileDialog oFD_RAWdata;
        private System.Windows.Forms.SaveFileDialog sFD_RAWdata;
        private System.Windows.Forms.MenuStrip mS_MenuPrincipal;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem analyseFichierToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem saveDataRawToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openDataRawToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uDPSettingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem myIPToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem generalSettingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem temetrieToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem temetrieGraphToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bestTimeLapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem traitementRawDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem databaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem transfertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem requestToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem databaseSettingToolStripMenuItem;
        private System.Windows.Forms.ToolTip enablePitInputsToolTip;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rB2Files;
        private System.Windows.Forms.RadioButton rB1File;
        private System.Windows.Forms.CheckBox cBTelLight;
        private System.Windows.Forms.CheckBox cBcheckfile;
        private System.Windows.Forms.Button btnOverlay;
    }
}