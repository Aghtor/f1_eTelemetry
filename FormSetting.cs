using SharpDX.DirectInput;
using System;
using System.Management;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.ApplicationModel.Background;

namespace f1_eTelemetry
{
    public partial class FormSetting : Form
    {

        private DirectInput directInput;
        private Joystick joystick;
        private Timer timer1;
        private bool CheckNewButton = false;

        Setting dataSetting = new Setting();
        Setting dataSettingold = new Setting();


        private void btnOK_Click(object sender, EventArgs e)
        {
            if (joystick != null)
                dataSetting.Box = joystick.Information.ProductName;
            //dataSetting.ButtonBox déjà renseigné

            dataSetting.Xc = double.Parse(tBXv.Text);
            dataSetting.Yc = double.Parse(tBYv.Text);
            dataSetting.Zc = double.Parse(tBZv.Text);
            dataSetting.InfluxDBAdress = tBInfluxDBip.Text;
            dataSetting.Joystick = cBJoystick.Checked;

            dataSetting.Simulation = 0; // par défaut
            if (radioButtonF1.Checked) dataSetting.Simulation = 0;
            if (radioButtonRF2.Checked) dataSetting.Simulation = 2;

            dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata = FolderRawData.Text;
            dataSetting.Folders_simu[dataSetting.Simulation].folderDeepdata = FolderDeepData.Text;
            dataSetting.Folders_simu[dataSetting.Simulation].folderExportdata = FolderExportData.Text;

            dataSetting.datacompress = cbDataCompress.Checked;
            dataSetting.dataEssentielles = cBdataEssentielles.Checked;

            dataSetting.dataonefile = rB1File.Checked;
            dataSetting.datatwofiles = rB2Files.Checked;

            this.Close();
        }

        public FormSetting()
        {
            InitializeComponent();

            // Initialisation de DirectInput
            directInput = new DirectInput();

            // Recherche du premier joystick disponible
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
            {
                joystickGuid = deviceInstance.InstanceGuid;
                break;
            }

            //ListJoystick();
            // Si aucun joystick n'est disponible, on affiche un message d'erreur
            if (joystickGuid != Guid.Empty)
            {
                byte DeviceButtonBox = 0;

                TBinfo.AppendText("------------------\n");

                // Récupérez la liste des périphériques de jeu connectés de type Joystick
                //var devices = directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices);
                var devices = directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
                TBinfo.AppendText(devices.Count + " Joysticks disponibles\n");

                for (int i = 0; i < devices.Count; i++)
                {
                    TBinfo.AppendText(i + 1 + " - " + devices[i].ProductName+"\n");
                    if ("Generic   USB  Joystick  " == devices[i].ProductName)
                    {
                        DeviceButtonBox = (byte)i;
                        lnBox.Text = devices[i].ProductName;
                    }
                }
 
                // Création du joystick
                //joystick = new Joystick(directInput, joystickGuid);
                joystick = new Joystick(directInput, devices[DeviceButtonBox].InstanceGuid);

                // Configuration du joystick
                joystick.Properties.BufferSize = 128;
                joystick.Acquire();
                
                // Démarrage de la lecture des entrées joystick
                // Créer un nouvel objet Timer
                timer1 = new Timer();
                timer1.Interval = 10;
                timer1.Tick += Timer1_Tick;
                timer1.Start();
                TBinfo.AppendText("Fin Init \n");
            }
            else
            {
                TBinfo.AppendText("Pas de Joystick connecté\n"); 
            }


        }
        private void ListJoystick()
        {
            // Créez une instance de DirectInput
            var directInput = new DirectInput();

            // Récupérez la liste des périphériques de jeu connectés
            var devices = directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices);

            // Vérifiez s'il y a des périphériques disponibles
            if (devices.Count == 0)
            {
                TBinfo.AppendText("Aucun joystick USB trouvé.");
                return;
            }

            // Affichez la liste des joysticks disponibles
            TBinfo.AppendText(devices.Count+" Joysticks disponibles :");
            for (int i = 0; i < devices.Count; i++)
            {
                TBinfo.AppendText(i + 1+" - "+ devices[i].ProductName);
            }

        }
        private void LookJoystick()
        {
            // Créez une instance de DirectInput
            var directInput = new DirectInput();

            // Récupérez la liste des périphériques de jeu connectés
            var devices = directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices);

            // Vérifiez s'il y a des périphériques disponibles
            if (devices.Count == 0)
            {
                TBinfo.AppendText("Look : Aucun joystick USB trouvé.\n");
                return;
            }

            // Sélectionnez le joystick choisi par l'utilisateur
//            var joystick = new Joystick(directInput, devices[selectedJoystickIndex - 1].InstanceGuid);

            // Sélectionnez le premier joystick USB trouvé
            var joystick = new Joystick(directInput, devices[0].InstanceGuid);

            // Initialisez le joystick
            joystick.Acquire();

            // Boucle principale pour lire les états du joystick
            while (true)
            {
                // Mettez à jour l'état du joystick
                joystick.Poll();
                var state = joystick.GetCurrentState();

                // Affichez les états du joystick
                TBinfo.AppendText("Axes X: "+ state.X+" Y: "+state.Y+ " Z: "+ state.Z+"\n");

                // Vérifiez si un bouton est enfoncé
                for (int i = 0; i < state.Buttons.Length; i++)
                {
                    if (state.Buttons[i])
                    {
                        TBinfo.AppendText("Bouton "+i+" enfoncé.");
                    }
                }

                // Attendre un court instant avant la prochaine mise à jour
                System.Threading.Thread.Sleep(100);
            }

        }

        public void SendData(Setting _dataSetting)
        {
            if (_dataSetting != null)
            {
                dataSetting = _dataSetting;
                dataSettingold = _dataSetting;

                tBXv.Text = dataSetting.Xc.ToString();
                tBYv.Text = dataSetting.Yc.ToString();
                tBZv.Text = dataSetting.Zc.ToString();
                cBJoystick.Checked = dataSetting.Joystick;
                cbDataCompress.Checked = dataSetting.datacompress;
                cBdataEssentielles.Checked = dataSetting.DataCompress;
                rB1File.Checked = dataSetting.dataonefile;
                rB2Files.Checked = dataSetting.datatwofiles;


                tBInfluxDBip.Text = dataSetting.InfluxDBAdress.ToString();
                FolderRawData.Text = dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata;
                FolderDeepData.Text = dataSetting.Folders_simu[dataSetting.Simulation].folderDeepdata;
                FolderExportData.Text = dataSetting.Folders_simu[dataSetting.Simulation].folderExportdata;

                dataSetting.Simulation = _dataSetting.Simulation;
                switch (dataSetting.Simulation)
                {
                    case 0: radioButtonF1.Checked = true; break;
                    case 1: radioButtonPC2.Checked = true; break;
                    case 2: radioButtonRF2.Checked = true; break;
                    case 3: radioButtonACC.Checked = true; break;
                    case 4: radioButtonAC.Checked = true; break;
                    case 5: radioButtonForza.Checked = true; break;
                    case 6: radioButtonRaceroom.Checked = true; break;
                    case 7: radioButtonAutomobilista.Checked = true; break;
                }
                lnButtonLog1.Text = dataSetting.ButtonBox.ToString();
                lnBox.Text = dataSetting.Box.ToString();
            }

        }

        private void radioButtons_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton.Checked)
            {
                switch (radioButton.Text)
                {
                    case "F1 202x": dataSetting.Simulation = 0; break;
                    case "Project Cars 2": dataSetting.Simulation = 1; break;
                    case "RFactor 2": dataSetting.Simulation = 2; break;
                    case "Assetto Corsa Comp": dataSetting.Simulation = 3; break;
                    case "Assetto Corsa": dataSetting.Simulation = 4; break;
                    case "Forza": dataSetting.Simulation = 5; break;
                    case "Raceroom": dataSetting.Simulation = 6; break;
                    case "Automobilista": dataSetting.Simulation = 7; break;
                }
                FolderRawData.Text = dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata;
                FolderDeepData.Text = dataSetting.Folders_simu[dataSetting.Simulation].folderDeepdata;
                FolderExportData.Text = dataSetting.Folders_simu[dataSetting.Simulation].folderExportdata;
            }
        }

        private void FormSetting_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            dataSetting = dataSettingold;
            this.Close();
        }

        private void sauveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataSetting.Save(".\\eTelemetry.conf");
            TBinfo.Text = "Configuration save in eTelemetry.conf";
        }
        private void Timer1_Tick(object sender, EventArgs e)
        {
            // Récupération des données du joystick
            joystick.Poll();
            var state = joystick.GetCurrentState();

            var brushJ = new SolidBrush(System.Drawing.Color.Black);
            //g.DrawString($"Buttons : {string.Join(", ", state.Buttons)}", SystemFonts.DefaultFont, brushJ, 30.0f, 30.0f);
            //tB_info.Text = $"Buttons : {string.Join(", ", state.Buttons)}";
            
            if (CheckNewButton==true)
            for (int i=0; i<state.Buttons.Count(); i++)
            {
               // if (state.Buttons[i] != statecurrent.Buttons[i])
                    if (state.Buttons[i] == true)
                    {
                        lnButtonLog1.Text = "Buttons " + i;
                        dataSetting.ButtonBox = i;
                    }
            }
            var statecurrent = joystick.GetCurrentState();
                                  
        }

        private void btNewButtonBox_Click(object sender, EventArgs e)
        {
            if (joystick != null)
            {
                if (CheckNewButton == true)
                {
                    CheckNewButton = false;
                    btNewButtonBox.BackColor = System.Drawing.Color.DodgerBlue;
                }
                else
                {
                    CheckNewButton = true;
                    btNewButtonBox.BackColor = System.Drawing.Color.Red;
                }
            }
        }

        private void tBXv_ModifiedChanged(object sender, EventArgs e)
        {
        }

        private void tBYv_TextChanged(object sender, EventArgs e)
        {
            dataSetting.Yc = double.Parse(tBYv.Text);
        }

        private void tBZv_TextChanged(object sender, EventArgs e)
        {
            dataSetting.Zc = double.Parse(tBZv.Text);
        }

        private void FolderRawData_TextChanged(object sender, EventArgs e)
        {
            dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata = FolderRawData.Text;
        }

        private void FolderDeepData_TextChanged(object sender, EventArgs e)
        {
            dataSetting.Folders_simu[dataSetting.Simulation].folderDeepdata = FolderDeepData.Text;
        }

        private void tBInfluxDBip_TextChanged(object sender, EventArgs e)
        {
            dataSetting.InfluxDBAdress = tBInfluxDBip.Text;
        }

        private void tBXv_TextChanged(object sender, EventArgs e)
        {
            dataSetting.Xc = double.Parse(tBXv.Text);
        }

        private void FolderExportData_TextChanged(object sender, EventArgs e)
        {
            dataSetting.Folders_simu[dataSetting.Simulation].folderExportdata = FolderExportData.Text;
        }

        private void FolderRawData_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            fBDselect.SelectedPath = dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata;

            if (fBDselect.ShowDialog() == DialogResult.OK)
            {
                FolderRawData.Text = fBDselect.SelectedPath;
                dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata = FolderRawData.Text;
            }
        }

        private void FolderDeepData_DoubleClick(object sender, EventArgs e)
        {
            fBDselect.SelectedPath = dataSetting.Folders_simu[dataSetting.Simulation].folderDeepdata;

            if (fBDselect.ShowDialog() == DialogResult.OK)
            {
                FolderDeepData.Text = fBDselect.SelectedPath;
                dataSetting.Folders_simu[dataSetting.Simulation].folderDeepdata = FolderDeepData.Text;
            }
        }

        private void FolderExportData_DoubleClick(object sender, EventArgs e)
        {
            fBDselect.SelectedPath = dataSetting.Folders_simu[dataSetting.Simulation].folderExportdata;

            if (fBDselect.ShowDialog() == DialogResult.OK)
            {
                FolderExportData.Text = fBDselect.SelectedPath;
                dataSetting.Folders_simu[dataSetting.Simulation].folderExportdata = FolderExportData.Text;
            }
        }

        private void cbDataCompress_CheckedChanged(object sender, EventArgs e)
        {
            dataSetting.datacompress = cbDataCompress.Checked;
        }

        private void cBdataEssentielles_CheckedChanged(object sender, EventArgs e)
        {
            dataSetting.DataCompress = cBdataEssentielles.Checked;
        }

        private void cBJoystick_CheckedChanged(object sender, EventArgs e)
        {
            dataSetting.Joystick = cBJoystick.Checked;
        }

        private void rB1File_CheckedChanged(object sender, EventArgs e)
        {
            dataSetting.dataonefile = rB1File.Checked;
            dataSetting.datatwofiles = rB2Files.Checked;
        }

        private void chargeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
