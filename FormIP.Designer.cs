namespace f1_eTelemetry
{
    partial class FormIP
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
            this.label1 = new System.Windows.Forms.Label();
            this.lBAdresseIP = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Adresse IP cible :";
            // 
            // lBAdresseIP
            // 
            this.lBAdresseIP.AutoSize = true;
            this.lBAdresseIP.Location = new System.Drawing.Point(125, 42);
            this.lBAdresseIP.Name = "lBAdresseIP";
            this.lBAdresseIP.Size = new System.Drawing.Size(35, 13);
            this.lBAdresseIP.TabIndex = 2;
            this.lBAdresseIP.Text = "label2";
            // 
            // FormIP
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(279, 108);
            this.Controls.Add(this.lBAdresseIP);
            this.Controls.Add(this.label1);
            this.Name = "FormIP";
            this.Text = "Information Adresse IP";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lBAdresseIP;
    }
}