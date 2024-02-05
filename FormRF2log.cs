using f1_eTelemetry.rFactor2Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.DirectInput;
using System.Windows.Markup;
using Windows.Data.Text;
using System.Reactive;
using System.Windows.Interop;
using System.Windows.Forms.Integration;

namespace f1_eTelemetry
{
    public partial class fmRF2log : Form
    {
        string NameFile_ONEFILE;
        string NameFile_Telemetry;
        string NameFile_Others;
        string NameFile_TelemetryLight;

        Setting dataSetting = new Setting();


        private DirectInput directInput;
        private Joystick joystick;
        private UDP_RF2 dataRF2 = new UDP_RF2();
        private rF2TelemetryLight dataLight;


        // Connection fields
        private const int CONNECTION_RETRY_INTERVAL_MS = 1000;
        private const int DISCONNECTED_CHECK_INTERVAL_MS = 15000;

        public static bool useStockCarRulesPlugin = false;

        System.Windows.Forms.Timer connectTimer = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer disconnectTimer = new System.Windows.Forms.Timer();
        bool connected = false;

        // Read buffers:
        MappedBuffer<rF2Telemetry> telemetryBuffer = new MappedBuffer<rF2Telemetry>(rFactor2Constants.MM_TELEMETRY_FILE_NAME, true /*partial*/, true /*skipUnchanged*/);
        MappedBuffer<rF2Scoring> scoringBuffer = new MappedBuffer<rF2Scoring>(rFactor2Constants.MM_SCORING_FILE_NAME, true /*partial*/, true /*skipUnchanged*/);
        MappedBuffer<rF2Rules> rulesBuffer = new MappedBuffer<rF2Rules>(rFactor2Constants.MM_RULES_FILE_NAME, true /*partial*/, true /*skipUnchanged*/);
        MappedBuffer<rF2ForceFeedback> forceFeedbackBuffer = new MappedBuffer<rF2ForceFeedback>(rFactor2Constants.MM_FORCE_FEEDBACK_FILE_NAME, false /*partial*/, false /*skipUnchanged*/);
        MappedBuffer<rF2Graphics> graphicsBuffer = new MappedBuffer<rF2Graphics>(rFactor2Constants.MM_GRAPHICS_FILE_NAME, false /*partial*/, false /*skipUnchanged*/);
        MappedBuffer<rF2PitInfo> pitInfoBuffer = new MappedBuffer<rF2PitInfo>(rFactor2Constants.MM_PITINFO_FILE_NAME, false /*partial*/, true /*skipUnchanged*/);
        MappedBuffer<rF2Weather> weatherBuffer = new MappedBuffer<rF2Weather>(rFactor2Constants.MM_WEATHER_FILE_NAME, false /*partial*/, true /*skipUnchanged*/);
        MappedBuffer<rF2Extended> extendedBuffer = new MappedBuffer<rF2Extended>(rFactor2Constants.MM_EXTENDED_FILE_NAME, false /*partial*/, true /*skipUnchanged*/);

        // Write buffers:
        MappedBuffer<rF2HWControl> hwControlBuffer = new MappedBuffer<rF2HWControl>(rFactor2Constants.MM_HWCONTROL_FILE_NAME);
        MappedBuffer<rF2WeatherControl> weatherControlBuffer = new MappedBuffer<rF2WeatherControl>(rFactor2Constants.MM_WEATHER_CONTROL_FILE_NAME);
        MappedBuffer<rF2RulesControl> rulesControlBuffer = new MappedBuffer<rF2RulesControl>(rFactor2Constants.MM_RULES_CONTROL_FILE_NAME);
        MappedBuffer<rF2PluginControl> pluginControlBuffer = new MappedBuffer<rF2PluginControl>(rFactor2Constants.MM_PLUGIN_CONTROL_FILE_NAME);

        // Marshalled views:
        rF2Telemetry telemetry;
        rF2Scoring scoring;
        rF2Rules rules;
        rF2ForceFeedback forceFeedback;
        rF2Graphics graphics;
        rF2PitInfo pitInfo;
        rF2Weather weather;
        rF2Extended extended;

        // Marashalled output views:
        rF2HWControl hwControl;
        rF2WeatherControl weatherControl;
        rF2RulesControl rulesControl;
        rF2PluginControl pluginControl;

        // Capture of the max FFB force.
        double maxFFBValue = 0.0;
        Stopwatch watch = new Stopwatch();


        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr Handle;
            public uint Message;
            public IntPtr WParameter;
            public IntPtr LParameter;
            public uint Time;
            public Point Location;
        }

        [DllImport("user32.dll")]
        public static extern int PeekMessage(out NativeMessage message, IntPtr window, uint filterMin, uint filterMax, uint remove);


        public fmRF2log()
        {
            this.InitializeComponent();

            // Initialisation de DirectInput
            directInput = new DirectInput();
            //Overlay(false);

            // Recherche du premier joystick disponible
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
            {
                if ("Generic   USB  Joystick  " == deviceInstance.ProductName)
                {
                    joystickGuid = deviceInstance.InstanceGuid;
                    tB_info.Text = deviceInstance.ProductName;
                    break;
                }
            }
            // Si aucun joystick n'est disponible, on affiche un message d'erreur
            if (joystickGuid == Guid.Empty)
            {
                MessageBox.Show("Aucun joystick détecté");
                //Close();
                //return;
            }
            else
            {
                // Création du joystick
                joystick = new Joystick(directInput, joystickGuid);

                // Configuration du joystick
                joystick.Properties.BufferSize = 128;
                joystick.Acquire();
            }

            this.DoubleBuffered = true;

            // timers utilisé pour surveiller la connection avec RF2 Coeur de la récupératio de la télémétrie
            this.connectTimer.Interval = fmRF2log.CONNECTION_RETRY_INTERVAL_MS;
            this.connectTimer.Tick += this.ConnectTimer_Tick;
            this.disconnectTimer.Interval = fmRF2log.DISCONNECTED_CHECK_INTERVAL_MS;
            this.disconnectTimer.Tick += this.DisconnectTimer_Tick;
            this.connectTimer.Start();
            this.disconnectTimer.Start();

            this.view.BorderStyle = BorderStyle.Fixed3D;
            this.view.Paint += this.View_Paint;


            // Recherche le premier joystick connecté
            //var joystick = new Joystick(directInput, Guid.Empty);
            //joystick.Acquire();

            Application.Idle += this.HandleApplicationIdle;
        }

        private void NameFile()
        {
            string NameFile;

            NameFile = DateTime.Now.Year.ToString() +
            DateTime.Now.Month.ToString() +
            DateTime.Now.Day.ToString() +
            DateTime.Now.Hour.ToString() + "_" +
            DateTime.Now.Minute.ToString("mm:ss");
            NameFile = dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata + "\\" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss");// +".bin";
            NameFile_Telemetry = NameFile + "TELE.bin";
            NameFile_Others = NameFile + "OTH.bin";
            NameFile_TelemetryLight = NameFile + "LIGHT.bin";
            NameFile_ONEFILE = NameFile + "ALL.bin";

            rB1File.Checked = dataSetting.dataonefile;
            rB2Files.Checked = dataSetting.datatwofiles;

            cBTelLight.Checked = dataSetting.dataEssentielles;
        }

        public void SendData(Setting _dataSetting)
        {
            

            dataSetting = _dataSetting;

            NameFile();

        }


        private void Overlay(bool active)
        {
            if (active)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.BackColor = Color.FromArgb(0, 0, 0, 0);
                this.TransparencyKey = Color.Empty; // Rétablissez la transparence.
                this.TopMost = true; // Cette ligne maintient la fenêtre en premier plan.
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                //this.BackColor = Color.FromArgb(128, 0, 0, 0);
                this.TransparencyKey = Color.FromArgb(0, 0, 0, 0);
                this.TopMost = false; // Cette ligne maintient la fenêtre en premier plan.
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            if (disposing)
                Disconnect();

            base.Dispose(disposing);
        }

        // Amazing loop implementation by Josh Petrie from:
        // http://gamedev.stackexchange.com/questions/67651/what-is-the-standard-c-windows-forms-game-loop
        bool IsApplicationIdle()
        {
            NativeMessage result;
            return PeekMessage(out result, IntPtr.Zero, (uint)0, (uint)0, (uint)0) == 0;
        }

        void HandleApplicationIdle(object sender, EventArgs e)
        {
            while (this.IsApplicationIdle())
            {
                try
                {
                    this.MainUpdate();

                    if (base.WindowState != FormWindowState.Minimized)
                    {
                        this.MainRender();
                    }
                }
                catch (Exception)
                {
                    this.Disconnect();
                }
            }
        }

        long delayAccMicroseconds = 0;
        long numDelayUpdates = 0;
        float avgDelayMicroseconds = 0.0f;

        void MainUpdate()
        {
            if (!this.connected)
                return;

            try
            {

                //var watch = System.Diagnostics.Stopwatch.StartNew();
                watch.Restart();
                extendedBuffer.GetMappedData(ref extended);     // Franck Semble être information sur le plugin 
                scoringBuffer.GetMappedData(ref scoring);       // Franck information sur les temps secteur etc peut être sauver ou recalculé avec télémétrie 
                telemetryBuffer.GetMappedData(ref telemetry);   // Franck Télémétrie de la voiture
                rulesBuffer.GetMappedData(ref rules);           // Franck information sur la course il me semble
                forceFeedbackBuffer.GetMappedDataUnsynchronized(ref forceFeedback); //Force feedback bon rien à branler normalement
                graphicsBuffer.GetMappedDataUnsynchronized(ref graphics); // Franck Info sur la caméra .. rien à branler 
                pitInfoBuffer.GetMappedData(ref pitInfo);       // info sdur le pit à clarifier
                weatherBuffer.GetMappedData(ref weather);       // Info sur le temps

                watch.Stop();
                var microseconds = watch.ElapsedTicks * 1000000 / System.Diagnostics.Stopwatch.Frequency;
                this.delayAccMicroseconds += microseconds;
                ++this.numDelayUpdates;

                if (this.numDelayUpdates == 0)
                {
                    this.numDelayUpdates = 1;
                    this.delayAccMicroseconds = microseconds;
                }

                this.avgDelayMicroseconds = (float)this.delayAccMicroseconds / this.numDelayUpdates;
            }
            catch (Exception)
            {
                this.Disconnect();
            }
        }

        void MainRender()
        {
            this.view.Refresh();
        }

        // Corrdinate conversion:
        // rF2 +x = screen +x
        // rF2 +z = screen -z
        // rF2 +yaw = screen -yaw
        // If I don't flip z, the projection will look from below.
        void View_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var state = (JoystickState) null;
            var gameStateText = new StringBuilder();

            var brushJ = new SolidBrush(System.Drawing.Color.Black);

            // Récupération des données du joystick
            if ((dataSetting.Joystick==true && joystick != null))
            {
                joystick.Poll();
                state = joystick.GetCurrentState();

                // information sur l'état des boutons
                //tB_info.Text = $"Buttons : {string.Join(", ", state.Buttons)}";
                //g.DrawString($"Buttons : {string.Join(", ", state.Buttons)}", SystemFonts.DefaultFont, brushJ, 3.0f, 100.0f);

                if (state.Buttons[dataSetting.ButtonBox] == true)
                {
                    g.DrawString($"Sauvegarde ok Bouton {string.Join(", ", dataSetting.ButtonBox)} ", SystemFonts.DefaultFont, brushJ, 3.0f, 40.0f);

                    if (rB1File.Checked)
                        g.DrawString($"Fichier Telemetry {string.Join(" : ", NameFile_ONEFILE)} ", SystemFonts.DefaultFont, brushJ, 3.0f, 60.0f);

                    if (rB2Files.Checked)
                    {
                        g.DrawString($"Fichier Telemetry {string.Join(" : ", NameFile_Telemetry)} ", SystemFonts.DefaultFont, brushJ, 3.0f, 80.0f);
                        g.DrawString($"Fichier Others {string.Join(" : ", NameFile_Others)} ", SystemFonts.DefaultFont, brushJ, 3.0f, 100.0f);
                    }
                    if (cBTelLight.Checked)
                        g.DrawString($"Fichier Others {string.Join(" : ", NameFile_TelemetryLight)} ", SystemFonts.DefaultFont, brushJ, 3.0f, 120.0f);
                }

            }

            if (!this.connected)
            {
                g.DrawString("Not connected to RFactor 2.", SystemFonts.DefaultFont, brushJ, 3.0f, 3.0f);
            }
            else
            {
                // sauvegarde dans la liste les informations
                g.DrawString("Connected to RFactor 2.", SystemFonts.DefaultFont, brushJ, 3.0f, 3.0f);

                try
                {
                    if (telemetry.mVehicles != null)
                    {
                        g.DrawString($"current number of vehicles {string.Join(" ", telemetry.mNumVehicles)} ", SystemFonts.DefaultFont, brushJ, 3.0f, 140.0f);
                        g.DrawString($"Header {string.Join(" ", telemetry.mVersionUpdateBegin)} ", SystemFonts.DefaultFont, brushJ, 3.0f, 160.0f);
                        g.DrawString($"FFB {string.Join(" ", forceFeedback.mForceValue)} ", SystemFonts.DefaultFont, brushJ, 3.0f, 180.0f);
                        g.DrawString($"RPM Player 0 {string.Join(" ", telemetry.mVehicles[0].mClutchRPM)} ", SystemFonts.DefaultFont, brushJ, 3.0f, 200.0f);
                    }
                    if (state != null)
                    {
                        if (state.Buttons[dataSetting.ButtonBox] == true)
                        {
                            if (dataRF2 != null)
                            {
                                g.DrawString($"Données RF2 sauvegardées", SystemFonts.DefaultFont, brushJ, 3.0f, 260.0f);
                                save_V3(); //this.telemetry);
                                tB_info.Text = "Sauvegarde en cours";
                            }
                            else
                                g.DrawString($"Erreur d initialisation dataRF2", SystemFonts.DefaultFont, brushJ, 3.0f, 240.0f);
                        }
                        else
                        {
                            g.DrawString($"Données RF2 non sauvegardées", SystemFonts.DefaultFont, brushJ, 3.0f, 260.0f);
                            NameFile(); // nouveau nom de fichier car bouton relalché
                        }
                    }
                }
                catch (Exception ex)
                {
                    tB_info.Text = "\nError: " + ex + " " + ex.Message + "Objet :" + ex.Source + "Link :" + ex.HelpLink;
                    Console.WriteLine("Error:" + ex + " " + ex.Message + "Objet :" + ex.Source + "Link :" + ex.HelpLink); // A mettre dans fenêtre
                }
            }
        }


        // 17/04/2023 *****************************************
        public struct rF2Data
        {
            public rF2Telemetry telemetry;
            public rF2Scoring scoring;
            public rF2Rules rules;
            public rF2ForceFeedback forceFeedback;
            public rF2Graphics graphics;
            public rF2PitInfo pitInfo;
            public rF2Weather weather;
            public rF2Extended extended;
        }
        public void save_V3()
        {
            dataSetting.datacompress = false; // pas decompression pour le moment

            // Save data to file with header
            if (rB2Files.Checked) // Plusieurs fichiers sauvegardés Télémetrie et les autres
            {
                using (FileStream stream = new FileStream(NameFile_Telemetry, FileMode.Append))
                {
                    if (dataSetting.datacompress)
                        using (GZipStream compressedStream = new GZipStream(stream, CompressionMode.Compress))
                        {
                            SaveWithHeader(compressedStream, telemetry, DataType.telemetry, telemetry.mVersionUpdateEnd);
                        }
                    else
                        SaveWithHeader(stream, telemetry, DataType.telemetry,telemetry.mVersionUpdateEnd);

                }
                using (FileStream stream = new FileStream(NameFile_Others, FileMode.Append))
                {
                    if (dataSetting.datacompress)
                        using (GZipStream compressedStream = new GZipStream(stream, CompressionMode.Compress))
                        {
                            SaveWithHeader(compressedStream, scoring, DataType.scoring,scoring.mVersionUpdateEnd);
                            SaveWithHeader(compressedStream, rules, DataType.rules, rules.mVersionUpdateEnd);
                            SaveWithHeader(compressedStream, forceFeedback, DataType.forceFeedback, forceFeedback.mVersionUpdateEnd);
                            SaveWithHeader(compressedStream, graphics, DataType.graphics, graphics.mVersionUpdateEnd);
                            SaveWithHeader(compressedStream, pitInfo, DataType.pitInfo, pitInfo.mVersionUpdateEnd);
                            SaveWithHeader(compressedStream, weather, DataType.weather, weather.mVersionUpdateEnd);
                            SaveWithHeader(compressedStream, extended, DataType.extended, extended.mVersionUpdateEnd);
                        }
                    else
                    {
                        SaveWithHeader(stream, scoring, DataType.scoring, scoring.mVersionUpdateEnd);
                        SaveWithHeader(stream, rules, DataType.rules, rules.mVersionUpdateEnd);
                        SaveWithHeader(stream, forceFeedback, DataType.forceFeedback, forceFeedback.mVersionUpdateEnd);
                        SaveWithHeader(stream, graphics, DataType.graphics, graphics.mVersionUpdateEnd);
                        SaveWithHeader(stream, pitInfo, DataType.pitInfo, pitInfo.mVersionUpdateEnd);
                        SaveWithHeader(stream, weather, DataType.weather, weather.mVersionUpdateEnd);
                        SaveWithHeader(stream, extended, DataType.extended, extended.mVersionUpdateEnd);
                    }
                }
            }
            if (rB1File.Checked) // Un seul fichier sauvegardé avecs les données Télémetrie et les autres
            {
                // un seul fichier télémétrie avec toutes les données
                using (FileStream stream = new FileStream(NameFile_ONEFILE, FileMode.Append))
                {
                    if (dataSetting.datacompress)
                        using (GZipStream compressedStream = new GZipStream(stream, CompressionMode.Compress))
                        {
                            SaveWithHeader(compressedStream, telemetry, DataType.telemetry, telemetry.mVersionUpdateEnd);
                            SaveWithHeader(compressedStream, scoring, DataType.scoring, scoring.mVersionUpdateEnd);
                            SaveWithHeader(compressedStream, rules, DataType.rules, rules.mVersionUpdateEnd);
                            SaveWithHeader(compressedStream, forceFeedback, DataType.forceFeedback, forceFeedback.mVersionUpdateEnd);
                            SaveWithHeader(compressedStream, graphics, DataType.graphics, graphics.mVersionUpdateEnd);
                            SaveWithHeader(compressedStream, pitInfo, DataType.pitInfo, pitInfo.mVersionUpdateEnd);
                            SaveWithHeader(compressedStream, weather, DataType.weather, weather.mVersionUpdateEnd);
                            SaveWithHeader(compressedStream, extended, DataType.extended, extended.mVersionUpdateEnd);
                        }
                    else
                    {
                        SaveWithHeader(stream, telemetry, DataType.telemetry, telemetry.mVersionUpdateEnd);
                        SaveWithHeader(stream, scoring, DataType.scoring, scoring.mVersionUpdateEnd);
                        SaveWithHeader(stream, rules, DataType.rules, rules.mVersionUpdateEnd);
                        SaveWithHeader(stream, forceFeedback, DataType.forceFeedback, forceFeedback.mVersionUpdateEnd);
                        SaveWithHeader(stream, graphics, DataType.graphics, graphics.mVersionUpdateEnd);
                        SaveWithHeader(stream, pitInfo, DataType.pitInfo   , pitInfo.mVersionUpdateEnd);
                        SaveWithHeader(stream, weather, DataType.weather, weather.mVersionUpdateEnd);
                        SaveWithHeader(stream, extended, DataType.extended, extended.mVersionUpdateEnd);
                    }
                }
            }
            if (cBTelLight.Checked || dataSetting.dataEssentielles) // Données essentielles uniquements pour générer le database Telemetrie à revoir
            {
                using (FileStream stream = new FileStream(NameFile_TelemetryLight, FileMode.Append))
                {
                    if (dataSetting.datacompress)
                        using (GZipStream compressedStream = new GZipStream(stream, CompressionMode.Compress))
                        {
                            SaveLightTelemetry(compressedStream, telemetry, DataType.telemetry);
                        }
                    else
                        SaveLightTelemetry(stream, telemetry, DataType.telemetry);

                }
            }
        }
        void SaveLightTelemetry(Stream stream, rF2Telemetry data, DataType type)
        {
            long check_size;

            Header header = new Header();
            header.DataType = type;
            byte[] bufferHeader = StructureToByteArray(header);

            stream.Write(bufferHeader, 0, bufferHeader.Length);

            dataLight = dataRF2.TransfertRF2dataLight(data);

            header.DataSize = Marshal.SizeOf(dataLight);
            byte[] bufferData = StructureToByteArray(dataLight);

            // la longueur doit être égale à 8 à voir pour modifier int en long
            if (bufferHeader.Length < 0)
            {
                using (StreamWriter outputFile = new StreamWriter("E:\\Telemetrie\\RF2\\" + "ErreurLight.txt", true))
                {
                    outputFile.WriteLine(
                    "*************************\n"
                    + $"{DateTime.Now.ToString("yyyy-MM-dd_HHmmss")}\n"
                    + "-------------------------\n"
                    + "DataType :"
                    + $"{(int)header.DataType:N0}\n"
                    + "DataSize :"
                    + $"{(uint)header.DataSize:N0}\n"
                    + "bufferHeader.Length :"
                    + $"{bufferHeader.Length:N0}\n"
                    + "bufferData.Length"
                    + $"{bufferData.Length:N0}\n"
                    + "stream.Position"
                    + $"{stream.Position:N0}\n"
                    + "*************************\n"
                        );
                }
            }

            check_size = stream.Length;
            stream.Write(bufferData, 0, bufferData.Length);
            if (cBcheckfile.Checked)
                if (stream.Length == check_size)
                {
                    MessageBox.Show("Attention sauvegarde incompléte");
                    cBcheckfile.Checked = false; // on déselectionne pour ne pas avoir une boucle de boite de dialogue
                    using (StreamWriter outputFile = new StreamWriter("E:\\Telemetrie\\RF2\\" + "ErreurLight.txt", true))
                    {
                        outputFile.WriteLine(
                        "*************************\n"
                        + "Attention sauvegarde incompléte Ligne 438 SaveLightTelemetry\n"
                        + $"{DateTime.Now.ToString("yyyy-MM-dd_HHmmss")}\n"
                        + "-------------------------\n"
                        + "DataType :"
                        + $"{(int)header.DataType:N0}\n"
                        + "DataSize :"
                        + $"{(uint)header.DataSize:N0}\n"
                        + "bufferHeader.Length :"
                        + $"{bufferHeader.Length:N0}\n"
                        + "bufferData.Length"
                        + $"{bufferData.Length:N0}\n"
                        + "stream.Position"
                        + $"{stream.Position:N0}\n"
                        + "*************************\n"
                            );
                    }
                }
        }

        static uint mVersionUpdateEndTelemetry = 0;
        static uint mVersionUpdateEndScoring = 0;
        static uint mVersionUpdateEndrules = 0;
        static uint mVersionUpdateEndforceFeedback = 0;
        static uint mVersionUpdateEndgraphics = 0;
        static uint mVersionUpdateEndpitInfo = 0;
        static uint mVersionUpdateEndweather = 0;
        static uint mVersionUpdateEndextended = 0;

        static void SaveWithHeader<T>(Stream stream, T data, DataType type, uint mVersionUpdateEnd)
        {
            bool shouldSave = false;

            switch (type)
            {
                case DataType.telemetry:
                    if (mVersionUpdateEnd != mVersionUpdateEndTelemetry)
                    {
                        mVersionUpdateEndTelemetry = mVersionUpdateEnd;
                        shouldSave = true;
                    }
                    break;
                case DataType.scoring:
                    if (mVersionUpdateEnd != mVersionUpdateEndScoring)
                    {
                        mVersionUpdateEndScoring = mVersionUpdateEnd;
                        shouldSave = true;
                    }
                    break;
                case DataType.rules:
                    if (mVersionUpdateEnd != mVersionUpdateEndrules)
                    {
                        mVersionUpdateEndrules = mVersionUpdateEnd;
                        shouldSave = true;
                    }
                    break;
                case DataType.forceFeedback:
                    if (mVersionUpdateEnd != mVersionUpdateEndforceFeedback)
                    {
                        mVersionUpdateEndforceFeedback = mVersionUpdateEnd;
                        shouldSave = true;
                    }
                    break;
                case DataType.graphics:
                    if (mVersionUpdateEnd != mVersionUpdateEndgraphics)
                    {
                        mVersionUpdateEndgraphics = mVersionUpdateEnd;
                        shouldSave = true;
                    }
                    break;
                case DataType.pitInfo:
                    if (mVersionUpdateEnd != mVersionUpdateEndpitInfo)
                    {
                        mVersionUpdateEndpitInfo = mVersionUpdateEnd;
                        shouldSave = true;
                    }
                    break;
                case DataType.weather:
                    if (mVersionUpdateEnd != mVersionUpdateEndweather)
                    {
                        mVersionUpdateEndweather = mVersionUpdateEnd;
                        shouldSave = true;
                    }
                    break;
                case DataType.extended:
                    if (mVersionUpdateEnd != mVersionUpdateEndextended)
                    {
                        mVersionUpdateEndextended = mVersionUpdateEnd;
                        shouldSave = true;
                    }
                    break;
            }

            if (shouldSave)
            {
                Header header = new Header();
                header.DataType = type;
                header.DataSize = Marshal.SizeOf<T>(data);

                byte[] bufferHeader = StructureToByteArray(header);
                byte[] bufferData = StructureToByteArray(data);

                stream.Write(bufferHeader, 0, bufferHeader.Length);

                stream.Write(bufferData, 0, bufferData.Length);
            }
        }

        private static byte[] StructureToByteArray(object obj)
        {
            int size = Marshal.SizeOf(obj);
            byte[] buffer = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, buffer, 0, size);
            Marshal.FreeHGlobal(ptr);
            return buffer;
        }

        private const int HeaderSize = 8; // utilisé uniquement pour la lecture

        public struct Header
        {
            public DataType DataType { get; set; }
            public int DataSize { get; set; }   // pour modifier int en long
        }

        public enum DataType
        {
            telemetry = 1,
            scoring = 2,
            rules = 3,
            forceFeedback = 4,
            graphics = 5,
            pitInfo = 6,
            weather = 7,
            extended = 8
        }
        // 17/04/2023 *****************************************

        public static rF2VehicleScoring GetPlayerScoring(ref rF2Scoring scoring)
        {
            var playerVehScoring = new rF2VehicleScoring();
            for (int i = 0; i < scoring.mScoringInfo.mNumVehicles; ++i)
            {
                var vehicle = scoring.mVehicles[i];
                switch ((rFactor2Constants.rF2Control)vehicle.mControl)
                {
                    case rFactor2Constants.rF2Control.AI:
                    case rFactor2Constants.rF2Control.Player:
                    case rFactor2Constants.rF2Control.Remote:
                        if (vehicle.mIsPlayer == 1)
                            playerVehScoring = vehicle;

                        break;

                    default:
                        continue;
                }

                if (playerVehScoring.mIsPlayer == 1)
                    break;
            }

            return playerVehScoring;
        }

        private void ConnectTimer_Tick(object sender, EventArgs e)
        {
            if (!this.connected)
            {
                try
                {
                    // Extended buffer is the last one constructed, so it is an indicator RF2SM is ready.
                    this.extendedBuffer.Connect();

                    this.telemetryBuffer.Connect();
                    this.scoringBuffer.Connect();
                    this.rulesBuffer.Connect();
                    this.forceFeedbackBuffer.Connect();
                    this.graphicsBuffer.Connect();
                    this.pitInfoBuffer.Connect();
                    this.weatherBuffer.Connect();

                    this.hwControlBuffer.Connect();
                    this.hwControlBuffer.GetMappedData(ref this.hwControl);
                    this.hwControl.mLayoutVersion = rFactor2Constants.MM_HWCONTROL_LAYOUT_VERSION;

                    this.weatherControlBuffer.Connect();
                    this.weatherControlBuffer.GetMappedData(ref this.weatherControl);
                    this.weatherControl.mLayoutVersion = rFactor2Constants.MM_WEATHER_CONTROL_LAYOUT_VERSION;

                    this.rulesControlBuffer.Connect();
                    this.rulesControlBuffer.GetMappedData(ref this.rulesControl);
                    this.rulesControl.mLayoutVersion = rFactor2Constants.MM_RULES_CONTROL_LAYOUT_VERSION;

                    this.pluginControlBuffer.Connect();
                    this.pluginControlBuffer.GetMappedData(ref this.pluginControl);
                    this.pluginControl.mLayoutVersion = rFactor2Constants.MM_PLUGIN_CONTROL_LAYOUT_VERSION;

                    // Scoring cannot be enabled on demand.
                    this.pluginControl.mRequestEnableBuffersMask = /*(int)SubscribedBuffer.Scoring | */(int)SubscribedBuffer.Telemetry | (int)SubscribedBuffer.Rules
                      | (int)SubscribedBuffer.ForceFeedback | (int)SubscribedBuffer.Graphics | (int)SubscribedBuffer.Weather | (int)SubscribedBuffer.PitInfo;
                    this.pluginControl.mRequestHWControlInput = 1;
                    this.pluginControl.mRequestRulesControlInput = 1;
                    this.pluginControl.mRequestWeatherControlInput = 1;
                    this.pluginControl.mVersionUpdateBegin = this.pluginControl.mVersionUpdateEnd = this.pluginControl.mVersionUpdateBegin + 1;
                    this.pluginControlBuffer.PutMappedData(ref this.pluginControl);

                    this.connected = true;

                }
                catch (Exception)
                {
                    this.Disconnect();
                }
            }
        }

        private void DisconnectTimer_Tick(object sender, EventArgs e)
        {
            if (!this.connected)
                return;

            try
            {
                // Alternatively, I could release resources and try re-acquiring them immidiately.
                var processes = Process.GetProcessesByName(f1_eTelemetry.rFactor2Constants.RFACTOR2_PROCESS_NAME);
                if (processes.Length == 0)
                    Disconnect();
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        private void Disconnect()
        {
            this.extendedBuffer.Disconnect();
            this.scoringBuffer.Disconnect();
            this.rulesBuffer.Disconnect();
            this.telemetryBuffer.Disconnect();
            this.forceFeedbackBuffer.Disconnect();
            this.pitInfoBuffer.Disconnect();
            this.weatherBuffer.Disconnect();
            this.graphicsBuffer.Disconnect();

            this.hwControlBuffer.Disconnect();
            this.weatherControlBuffer.Disconnect();
            this.rulesControlBuffer.Disconnect();
            this.pluginControlBuffer.Disconnect();

            this.view.Paint -= this.View_Paint;
            this.connectTimer.Stop();
            this.disconnectTimer.Stop();

            this.connected = false;

        }

        private void fmRF2log_KeyPress(object sender, KeyPressEventArgs e)
        {
            tB_info.Text = "\nKey: " + e.KeyChar;

        }

        private void btnOverlay_Click(object sender, EventArgs e)
        {
            /*
            var wpfForm = new f1_eTelemetry.Overlay.MainWindow();
            System.Windows.Interop.ComponentDispatcher.RunAsync(() =>
            {
                wpfForm.Show();
            });
            */


        }
    }
    /*
    public class JoystickSample
    {
        private Joystick joystick;

        public async void InitializeJoystick()
        {
            var joysticks = await Joystick.GetControllersAsync();
            if (joysticks.Count > 0)
            {
                joystick = joysticks[0];

                joystick.ButtonPressed += Joystick_ButtonPressed;
                joystick.ButtonReleased += Joystick_ButtonReleased;
                joystick.ThumbstickMoved += Joystick_ThumbstickMoved;

                // Définir la sensibilité des joysticks
                joystick.ThumbstickPivotMode = JoystickPivotMode.Absolute;
                joystick.ThumbstickMagnitudeMode = JoystickMagnitudeMode.Signed;

                // Définir la sensibilité des boutons
                joystick.ButtonPressThreshold = 0.5f;
            }
        }

        private void Joystick_ButtonPressed(Joystick sender, JoystickButtonPressedEventArgs args)
        {
            Console.WriteLine($"Button {args.Button.Number} pressed");
        }

        private void Joystick_ButtonReleased(Joystick sender, JoystickButtonReleasedEventArgs args)
        {
            Console.WriteLine($"Button {args.Button.Number} released");
        }

        private void Joystick_ThumbstickMoved(Joystick sender, JoystickThumbstickMovedEventArgs args)
        {
            Console.WriteLine($"Thumbstick moved: X={args.X}, Y={args.Y}");
        }
    }
    */
}
