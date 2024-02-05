// https://learn.microsoft.com/fr-fr/dotnet/csharp/language-reference/preprocessor-directives
// https://learn.microsoft.com/fr-fr/dotnet/csharp/language-reference/preprocessor-directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml.Linq;
using CsvHelper.Configuration.Attributes;
using f1_eTelemetry.rFactor2Data;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;
using Microsoft.Office.Interop.Excel;
using SharpDX.DirectInput;
using SharpDX.Win32;
using static System.Net.WebRequestMethods;
using static f1_eTelemetry.fmRF2log;
using static f1_eTelemetry.FormTelemetry;
using static f1_eTelemetry.rFactor2Constants;
using static f1_eTelemetry.UDP_Data;
//using static f1_eTelemetry.UDP_RF2;
//using File = System.IO.File;


namespace f1_eTelemetry
{
    public partial class eTelemetry : Form
    {
        Stopwatch stopWatch = new Stopwatch();
        long stopWatchTS = 0;
        long stopWatchTSmax = 0;
        long stopWatchTSmin = 999999999999999999;
        long stopWatchTSavg = 0;
        Boolean FlagDebug = false;
        Header headerMax;
        Header headerMin;
        long nanosecPerTick = (1000L * 1000L * 1000L) / Stopwatch.Frequency;
        /* information
        double ticks = sw.ElapsedTicks;
        double seconds = ticks / Stopwatch.Frequency;
        double milliseconds = (ticks / Stopwatch.Frequency) * 1000;
        double nanoseconds = (ticks / Stopwatch.Frequency) * 1000000000;
        */
        long gNb = 0;
        long gNbmax = 0;
        long gNbmin = 0;

        Setting dataSetting = new Setting();
        UDP_Data dataUDP = new UDP_Data();

        DateTime date_of_file;

        Joystick joystick;
        Timer joystickTimer;

        // list envoyée à Form télémétrie pour le traitement des données Transféré dans UDP_Data
        // public List<TelemetryData> ListTelemetryData = new List<TelemetryData>();


        //*****************************************************************************************
        // F1 202X Data
        public struct FileInfoData
        {
            public string NameFile;
            public string FullNameFile;
            public int m_packetFormat;
            public string m_packetVersion;

            public string m_trackId;
            public double m_trackLength;
            public string m_networkGame;

            public sbyte m_trackTemperature;
            public sbyte m_airTemperature;
            public string sessiontypename;
            public string formulaname;
            public uint m_totalLaps;
            public string meteoname;
            public string m_aiDifficulty;
            public string m_playerCarIndex;
        }

        public List<FileInfoData> ListFileInfo = new List<FileInfoData>();

        List<byte[]> ListReceived = new List<byte[]>();
        //#if !UDPclass
        static UdpClient listener = new UdpClient(20777);                       //Create a UDPClient object
        public IPEndPoint RemoteIP;
        Boolean Flaglistener = true;
        UDP_f12021 dataf1_2021 = new UDP_f12021();
        //#endif

        //UDP_Read dataUDPread = new UDP_Read(); //à tester à honskirch
        // F1 202X Data
        //*****************************************************************************************


        //*****************************************************************************************
        // RFACTOR 2 Data

        UDP_RF2 RF2 = new UDP_RF2();
        rF2Telemetry telemetry;
        rF2Scoring scoring;
        rF2Rules rules;
        rF2ForceFeedback forceFeedback;
        rF2Graphics graphics;
        rF2PitInfo pitInfo;
        rF2Weather weather;
        rF2Extended extended;
        bool RF2connected = false;
        private bool backgroundWorkerRF2ReadisWorkerRunning = false;
        private bool conversionWorkerRunning = false;
        private BackgroundWorker backgroundWorkerProgress;

        // RFACTOR 2 Data
        //*****************************************************************************************

        //*****************************************************************************************
        // InfluxDB
        //https://github.com/influxdata/influxdb-client-csharp/blob/master/Client.Legacy.Test/ItFluxClientTest.cs
        [Measurement("mem")]
        private class Mem
        {
            [Column("host", IsTag = true)] public string Host { get; set; }
            [Column("used_percent")] public double? UsedPercent { get; set; }
            [Column(IsTimestamp = true)] public DateTime Time { get; set; }
        }

        public eTelemetry()
        {
            InitializeComponent();
            if (System.IO.File.Exists(".\\eTelemetry.conf"))
                dataSetting = Setting.Load(".\\eTelemetry.conf");
            else
                dataSetting.Save(".\\eTelemetry.conf");

            dataUDP.SelectSimulation(dataSetting.Simulation);
            rTB_Terminal.AppendText("Simulation : " + dataSetting.GetSimulationName() + "\n");
            rTB_Terminal.AppendText("---------------------------------------------------------- \n");

            rTB_Terminal.AppendText("stopWatch Information\n");
            if (Stopwatch.IsHighResolution)
            {
                rTB_Terminal.AppendText("Operations timed using the system's high-resolution performance counter.\n");
            }
            else
            {
                rTB_Terminal.AppendText("Operations timed using the DateTime class.\n");
            }
            long frequency = Stopwatch.Frequency;
            rTB_Terminal.AppendText("Timer frequency in ticks per second = " + frequency + "\n");
            rTB_Terminal.AppendText("---------------------------------------------------------- \n");

        }

        // https://chowdera.com/2022/02/202202150803112766.html
        // https://influxdata.github.io/influxdb-client-csharp/api/InfluxDB.Client.WriteApi.html
        // https://www.influxdata.com/blog/getting-started-with-c-and-influxdb/
        private void writedata() // ok
        {
            using var client = InfluxDBClientFactory.Create("http://25.42.134.226:8086", dataSetting.token);
            const string data = "mem,host=host91 used_percent=18.43234543";
            using (var writeApi = client.GetWriteApi())
            {
                writeApi.WriteRecord(data, WritePrecision.Ns, dataSetting.bucket, dataSetting.org);
                //writeApi.WriteRecord(bucket, org, WritePrecision.Ns, data);
            }
            Console.WriteLine("ok");
        }

        // https://influxdata.github.io/influxdb-client-java/influxdb-client-kotlin/dokka/influxdb-client-kotlin/com.influxdb.client.kotlin/-write-kotlin-api/write-measurement.html
        // https://coder.social/influxdata/influxdb-client-csharp

        private void writedata2() // ok
        {
            var mem = new Mem { Host = "host2", UsedPercent = 23.43234543, Time = DateTime.UtcNow };

            using var client = InfluxDBClientFactory.Create("http://25.42.134.226:8086", dataSetting.token);

            using (var writeApi = client.GetWriteApi())
            {
                writeApi.WriteMeasurement(mem, WritePrecision.Ns, dataSetting.bucket, dataSetting.org);
                //writeApi.WriteMeasurement(bucket, org, WritePrecision.Ns, mem);
            }
        }

        // InfluxDB
        //*****************************************************************************************

        //*****************************************************************************************
        //F1 202X traitement des données
        // Sauvegarde et chargement des données de télémétrie à partir d'un fichier
        private void saveF1data()
        {
            // Variante de sauvegarde des données dans le fichier DataStream.bin
            // déclaration global de writingStream
            //https://devstory.net/10535/csharp-binary-streams
            //

            DateTime time1;
            DateTime time2;
            DateTime time3;
            DateTime time4;

            time1 = DateTime.Now;
            time2 = DateTime.Now;
            time3 = DateTime.Now;
            time4 = DateTime.Now;
            string NameFile;
            Boolean EcritureOK = true;

            NameFile = DateTime.Now.Year.ToString() +
                DateTime.Now.Month.ToString() +
                DateTime.Now.Day.ToString() +
                DateTime.Now.Hour.ToString() + "_" +
                DateTime.Now.Minute.ToString("mm:ss");
            NameFile = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            tB_info.Text = "Préparation sauvegarde dans le fichier : DataStream" + NameFile + ".bin";

            sFD_RAWdata.InitialDirectory = dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata;
            sFD_RAWdata.FileName = NameFile;

            tB_info.AppendText("\nSauvegarde RAW dans un fichier " + NameFile + "\n");
            if (sFD_RAWdata.ShowDialog() == DialogResult.OK)
            {
                NameFile = sFD_RAWdata.FileName + ".bin";
                rTB_Terminal.AppendText("\nSauvegarde RAW dans un fichier " + NameFile + "\n");

                //Stream writingStream = new FileStream(@".\DataStream" + NameFile + ".bin", FileMode.Create);
                if (System.IO.File.Exists(NameFile)) /// 
                {
                    string fmessage = "ATTENTION fichier existant";
                    string fcaption = "voulez-vous l'écrasser?";
                    MessageBoxButtons fbuttons = MessageBoxButtons.OKCancel;
                    DialogResult fresult;

                    // Displays the MessageBox.
                    fresult = MessageBox.Show(fmessage, fcaption, fbuttons);
                    if (fresult != DialogResult.OK) EcritureOK = false;
                }
                if (EcritureOK == true)
                {
                    rTB_Terminal.AppendText("\nSauvegarde RAW lancée " + NameFile + "\n");
                    Stream writingStream = new FileStream(NameFile, FileMode.Create);

                    try
                    {
                        time3 = DateTime.Now;

                        if (writingStream.CanWrite) // vérifie si écriture possible dans le fichier
                        {
                            foreach (var ElemReceived in ListReceived)
                            {
                                time1 = DateTime.Now;

                                //writingStream.Write(ListReceived[0], 0, ListReceived[0].Length); // écrit dans le fichier
                                int longueur = ElemReceived.Length;
                                byte[] ByteArray = BitConverter.GetBytes(longueur);


                                // writingStream.WriteByte(((byte)longeur)); // écriture de la longueur des données à relire
                                writingStream.Write(BitConverter.GetBytes(longueur), 0, 4);

                                writingStream.Write(ElemReceived, 0, ElemReceived.Length); // écrit dans le fichier

                                time2 = DateTime.Now;
                            }
                        }
                        time4 = DateTime.Now;

                    }
                    catch (Exception ex)
                    {
                        rTB_Terminal.AppendText("\nError: " + ex + "\n");
                        Console.WriteLine("Error:" + ex); // A mettre dans fenêtre
                    }
                    finally
                    {
                        // Fermez Stream, libérez des ressources
                        writingStream.Close();
                        rTB_Terminal.AppendText("\nFin de sauvegarde RAW dans un fichier " + NameFile + "\n");
                        rTB_Terminal.AppendText("Time 1 : " + time1.ToString("ss.ffff") + "\n");
                        rTB_Terminal.AppendText("Time 2 : " + time2.ToString("ss.ffff") + "\n");
                        rTB_Terminal.AppendText("Time 3 : " + time3.ToString("ss.ffff") + "\n");
                        rTB_Terminal.AppendText("Time 4 : " + time4.ToString("ss.ffff") + "\n");

                    }
                }
                else
                {
                    rTB_Terminal.AppendText("\nEcritureOK == false fichier non suvegardé");
                }

                // https://docs.microsoft.com/fr-fr/dotnet/api/system.windows.forms.messagebox?view=windowsdesktop-5.0
                // Initializes the variables to pass to the MessageBox.Show method.
                string message = "Sauvegarde des données";
                string caption = "dans un fichier terminée";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result;

                // Displays the MessageBox.
                result = MessageBox.Show(message, caption, buttons);

            }
            else
            {
                string message = "Pas de sauvegarde des données";
                string caption = "CANCEL";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result;

                // Displays the MessageBox.
                result = MessageBox.Show(message, caption, buttons);

            }
        }
        private void loadF1data()
        {
            DateTime time1;
            DateTime time2;
            DateTime time3;
            DateTime time4;
            long Nb = 0;

            time1 = DateTime.Now;
            time2 = DateTime.Now;
            time3 = DateTime.Now;
            time4 = DateTime.Now;

            int longueur = 10;
            byte[] BufferByte = new byte[4];

            string NameFile; // = "test.bin";

            time1 = DateTime.Now;
            time2 = DateTime.Now;

            oFD_RAWdata.InitialDirectory = dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata;

            if (oFD_RAWdata.ShowDialog() == DialogResult.OK)
            {
                NameFile = oFD_RAWdata.FileName;
                rTB_Terminal.AppendText("Ouverture RAW dans un fichier " + NameFile + "\n");

                if (System.IO.File.Exists(NameFile)) /// 
                {
                    rTB_Terminal.AppendText("Chargement RAW lancée " + NameFile + "\n");
                    Stream writingStream = new FileStream(NameFile, FileMode.Open);

                    try
                    {
                        time3 = DateTime.Now;

                        if (writingStream.CanRead) // vérifie si lecture possible dans le fichier
                        {
                            ListReceived.Clear();

                            FileInfo file = new FileInfo(NameFile);
                            // file creation time
                            date_of_file = file.CreationTime;


                            while (longueur > -1) // lecture du  fichier
                            {
                                time1 = DateTime.Now;

                                writingStream.Read(BufferByte, 0, 4);
                                longueur = BitConverter.ToInt32(BufferByte, 0);
                                byte[] ElemReceived = new byte[longueur];

                                if (writingStream.Read(ElemReceived, 0, longueur) == 0) longueur = -2;
                                ListReceived.Add(ElemReceived); // stock les données brutes avant le traitement

                                time2 = DateTime.Now;
                                Nb++;
                            }

                        }
                        time4 = DateTime.Now;

                    }
                    catch (Exception ex)
                    {
                        rTB_Terminal.AppendText("Error: " + ex + "\n");
                        Console.WriteLine("Error:" + ex); // A mettre dans fenêtre
                    }
                    finally
                    {
                        // Fermez Stream, libérez des ressources
                        writingStream.Close();
                        rTB_Terminal.AppendText("Fin de lecture RAW dans fichier " + NameFile + "\n");

                        rTB_Terminal.AppendText("Chargement des données du fichier dans la partie tampon mémoire\n");
                        rTB_Terminal.AppendText("Time 1 : " + time1.ToString("ss.ffff") + "\n");
                        rTB_Terminal.AppendText("Time 2 : " + time2.ToString("ss.ffff") + "\n");
                        rTB_Terminal.AppendText("Delta : " + (time2 - time1).ToString() + "\n");
                        rTB_Terminal.AppendText("Time 3 : " + time3.ToString("ss.ffff") + "\n");
                        rTB_Terminal.AppendText("Time 4 : " + time4.ToString("ss.ffff") + "\n");
                        rTB_Terminal.AppendText("Delta : " + (time4 - time3).ToString() + "\n");
                        rTB_Terminal.AppendText("Delta t4-t1: " + (time4 - time3).ToString() + "\n");
                        rTB_Terminal.AppendText("Nb : " + (Nb).ToString() + "\n");

                    }
                }
                else
                {
                    rTB_Terminal.AppendText("\nFichier n existe pas");
                    string fmessage = "ATTENTION fichier n'existe pas";
                    string fcaption = "action annulée?";
                    MessageBoxButtons fbuttons = MessageBoxButtons.OK;
                    DialogResult fresult;
                    fresult = MessageBox.Show(fmessage, fcaption, fbuttons);
                    //if (fresult != DialogResult.OK) EcritureOK = false;
                }
            }
            else
            {
                rTB_Terminal.AppendText("\nActivité annulée");
            }
        }

        private void traitementF1202X()
        {
            DateTime time1;
            DateTime time2;
            DateTime time3;
            DateTime time4;
            long Nb = 0;

            rTB_Terminal.AppendText("Traitement en cours \n");

            //dataf1_2021.DeletePackets();
            dataUDP.DetelePackets();
            time3 = DateTime.Now;
            time4 = DateTime.Now;

            time1 = DateTime.Now;

            foreach (var ElemReceived in ListReceived)
            {
                time3 = DateTime.Now;
                Stream stream = new MemoryStream(ElemReceived);
                var binaryReaderRAW = new BinaryReader(stream);

                //dataf1_2021.InitPackets(stream, binaryReaderRAW);
                dataUDP.InitPackets(stream, binaryReaderRAW);
                time4 = DateTime.Now;
                Nb++;
            }
            dataUDP.DriverListUpdate();

            time2 = DateTime.Now;

            rTB_Terminal.AppendText("Interprétation des données\n");
            rTB_Terminal.AppendText("Time 1 : " + time1.ToString("mm:ss.ffff") + "\n");
            rTB_Terminal.AppendText("Time 2 : " + time2.ToString("mm:ss.ffff") + "\n");
            rTB_Terminal.AppendText("Delta : " + (time2 - time1).ToString() + "\n");
            rTB_Terminal.AppendText("Time 3 : " + time3.ToString("mm:ss.ffff") + "\n");
            rTB_Terminal.AppendText("Time 4 : " + time4.ToString("mm:ss.ffff") + "\n");
            rTB_Terminal.AppendText("Delta : " + (time4 - time3).ToString() + "\n");
            rTB_Terminal.AppendText("Nb : " + (Nb).ToString() + "\n");

            rTB_Terminal.AppendText("Lecture terminée ----------------------------------------------\n");

            rTB_Terminal.AppendText("ListPlayers : " + dataUDP.dataf1_2021.ListPlayers.Count + "\n");
            rTB_Terminal.AppendText("ListTelemetry : " + dataUDP.dataf1_2021.ListTelemetry.Count + "\n");
            rTB_Terminal.AppendText("ListCarDamage : " + dataUDP.dataf1_2021.ListCarDamage.Count + "\n");
            rTB_Terminal.AppendText("ListHistoric : " + dataUDP.dataf1_2021.ListHistoric.Count + "\n");
            rTB_Terminal.AppendText("ListLapData : " + dataUDP.dataf1_2021.ListLapData.Count + "\n");
            rTB_Terminal.AppendText("ListMotion_packet : " + dataUDP.dataf1_2021.ListMotion_packet.Count + "\n");
            rTB_Terminal.AppendText("ListSession : " + dataUDP.dataf1_2021.ListSession.Count + "\n");

            rTB_Terminal.AppendText("dataUDP ----------------------------------------------\n");
            rTB_Terminal.AppendText("Format :" + dataUDP.Get_m_packetFormat().ToString() + "\n");
            rTB_Terminal.AppendText("Version :" + dataUDP.Get_m_packetVersion().ToString() + "\n");

            for (int i = 0; i < 12; i++)
                rTB_Terminal.AppendText("Packet [" + i + "] : " + dataUDP.Get_packet(i) + "\n");


            onglet_info();

            //onglet_infoOLD();
        }
        // traitement backgroundWorker non implémenté sur F1 car 1,3 secondes pour chargement d'un fichier et 7 secondes pour traitements
        private void backgroundWorkerF1Read_DoWork(object sender, DoWorkEventArgs e)
        {

        }
        private void backgroundWorkerF1Read_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }
        private void UpdateProgressLabelF1(int percentage)
        {
            if (!backgroundWorkerF1Read.IsBusy)
            {
                backgroundWorkerF1Read.RunWorkerAsync();
            }
            else
            {
                backgroundWorkerF1Read.ReportProgress(percentage);
            }
        }
        private void backgroundWorkerF1Read_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
        private bool ReadFileDataF1(string NameFile)
        {
            return true;
        }
        private void call_telemetrieF1202X()
        {
            if (dataUDP.checkdata() == false)
            {
                if (ListReceived.Count == 0)
                {
                    loadF1data();
                }
                if (ListReceived.Count > 0)
                {
                    traitementF1202X();
                }
            }

            FormTelemetry formTelemetry = new FormTelemetry();

            if (dataUDP.checkdata())
            {
                if (formTelemetry.Visible)
                {
                    formTelemetry.Focus();
                    formTelemetry.SendData(dataUDP, dataSetting);
                }
                else
                {
                    formTelemetry.Show();
                    formTelemetry.SendData(dataUDP, dataSetting);
                }
            }
            else
            {
                rTB_Terminal.AppendText("Pas de donnée de télémétrie \n");
            }
        }
        private void call_telemetrieold()
        {
            if (dataf1_2021.checkData() == false)
            {
                if (ListReceived.Count > 0)
                    traitementF1202X();
            }

            FormTelemetry formTelemetry = new FormTelemetry();

            if (dataf1_2021.checkData())
            {
                if (formTelemetry.Visible)
                {
                    formTelemetry.Focus();
                    formTelemetry.SendData(dataf1_2021, dataSetting);
                }
                else
                {
                    formTelemetry.Show();
                    formTelemetry.SendData(dataf1_2021, dataSetting);
                }
            }
            else
            {
                rTB_Terminal.AppendText("Pas de donnée de télémétrie \n");
            }
        }

        // Récupération via UDP des données de télémétrie RF2
        private void UDPread()
        {
            try
            {
                // utilisé juste pour le début de la communication aprés n'est plus utilisée
                rTB_Terminal.AppendText("\nLecture UDP engagée attente donnée");

                btn_uDP.BackColor = Color.Blue;
                Flaglistener = true;
                IPEndPoint RemoteIP = new IPEndPoint(IPAddress.Any, 20777);   //Start recieving data from any IP listening on port 5606 (port for PCARS2)

                listener.BeginReceive(new AsyncCallback(UDPrecevalt), null);
            }
            catch (Exception ex)
            {
                // en cas d'erreur affiche un message
                rTB_Terminal.AppendText("Message :" + ex.Message.ToString());
            }
        }
        // Version future éventuellement UDPreadClass();
        private void UDPreadClass()
        {
            //dataUDPread.UDPread(rTB_Terminal, btn_uDP);
        }
        // Lecture des informations UDP et traitement
        void UDPrecevalt(IAsyncResult res)
        {
            // Compatible PC2 et F1202X au port prêt

            if (Flaglistener) // seule façon trouvée pour le moment de ne pas réécouter l'UDP les autres bloquent le programme
            {
                btn_uDP.BackColor = Color.Green;

                byte[] receivedRAW = listener.EndReceive(res, ref RemoteIP);
                ListReceived.Add(receivedRAW); // stock les données brutes avant le traitement dans la list ListReceived

                listener.BeginReceive(new AsyncCallback(UDPrecevalt), null);
            }
        }



        //F1 202X traitement des données
        //*****************************************************************************************


        //*****************************************************************************************
        //rFactor2 traitement des données pour passage dans tableau télémétrie

        struct ProgressInfo
        {
            public int Percentage;
            //public DateTime Time1;
            //public TimeSpan Time2;
            //public TimeSpan Time3;

            public ProgressInfo(int percentage/* /*DateTime time1,  TimeSpan time2, TimeSpan time3*/)
            {
                Percentage = percentage;
                //Time1 = time1;
                //Time2 = time2;
                //Time3 = time3;
            }
        }

        private void traitementRF2()
        {
            if (!backgroundWorkerRF2ReadisWorkerRunning)
                call_telemetrieRF2();
            else
                rTB_Terminal.AppendText("Une activité est déjà en cours\n");
        }
        private void backgroundWorkerRF2Read_DoWork(object sender, DoWorkEventArgs e)
        {
            bool operationSuccess = true;

            string NameFile = e.Argument as string;

            backgroundWorkerRF2ReadisWorkerRunning = true;

            operationSuccess = ReadFileDataRF2(NameFile);

            // Définir la valeur de retour
            e.Result = operationSuccess;
        }
        private void backgroundWorkerRF2Read_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lbRate.Text = e.ProgressPercentage.ToString() + "%";
            //ProgressInfo progressInfo = (ProgressInfo)e.UserState;
            //DateTime time1 = progressInfo.Time1;
            //TimeSpan time2 = progressInfo.Time2;
            //TimeSpan time3 = progressInfo.Time3;
            //label16.Text = time1.ToString("ss.ffff");
            //label18.Text = time2.ToString();//("ss.ffff");
            //label20.Text = time3.ToString();// "ss.ffff");
            //label21.Text = (time2 - time1).ToString();

        }
        private void UpdateProgressLabel(int percentage)
        {
            if (!backgroundWorkerRF2Read.IsBusy)
            {
                backgroundWorkerRF2Read.RunWorkerAsync();
            }
            else
            {
                backgroundWorkerRF2Read.ReportProgress(percentage);
            }
        }
        private void backgroundWorkerRF2Read_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool operationSuccess = (bool)e.Result;

            backgroundWorkerRF2ReadisWorkerRunning = false;

            rTB_Terminal.AppendText("Analyse ----------------------------------------------\n");
            rTB_Terminal.AppendText("stopWatch ticks : " + (stopWatch.ElapsedTicks).ToString() + "\n");
            rTB_Terminal.AppendText("Begin==End : " + dataUDP.dataRF2.cpt_beginend.ToString() + "\n");

            rTB_Terminal.AppendText("stopWatch min : " + (stopWatchTSmin).ToString() + " GnB : " + gNbmin.ToString() + "\n");
            rTB_Terminal.AppendText("stopWatch max : " + (stopWatchTSmax).ToString() + " GnB : " + gNbmax.ToString() + "\n");
            rTB_Terminal.AppendText("stopWatch avg : " + (stopWatchTSavg).ToString() + "\n");
            rTB_Terminal.AppendText("Nb : " + gNb.ToString() + "\n");
            rTB_Terminal.AppendText("Analyse ----------------------------------------------\n");

            if (operationSuccess)
            {
                rTB_Terminal.AppendText("Lecture terminée ----------------------------------------------\n");

                rTB_Terminal.AppendText("Telemetry : " + dataUDP.ListTelemetryData.Count + "\n");
                rTB_Terminal.AppendText("Telemetry : " + dataUDP.ListTelemetryData.Count + "\n");


                FormTelemetry formTelemetry = new FormTelemetry();

                if (dataUDP.ListTelemetryData.Count>0)
                {
                    if (formTelemetry.Visible)
                    {
                        formTelemetry.Focus();
                        formTelemetry.SendData(dataUDP, dataSetting);
                    }
                    else
                    {
                        formTelemetry.Show();
                        formTelemetry.SendData(dataUDP, dataSetting);
                    }
                }
                else
                {

                    rTB_Terminal.AppendText("Pas de donnée de télémétrie \n");
                }
            }
            else
            {
                rTB_Terminal.AppendText("CancellationPending " + dataUDP.ListTelemetryData.Count + "\n");
                //rTB_Terminal.AppendText("Telemetry " + Marshal.SizeOf(dataUDP.dataRF2.telemetry) + "\n");

                // Opération terminée avec erreur
                rTB_Terminal.AppendText("Traitement annulée \n");
            }

        }
        private void stopWatchLog(Header header)
        {
            stopWatchTS = stopWatch.ElapsedMilliseconds;
            stopWatchTS = stopWatch.ElapsedTicks;

            if (stopWatchTS < stopWatchTSmin)
            {
                stopWatchTSmin = stopWatchTS;
                gNbmin = gNb;
                headerMin = header;
            }
            if (stopWatchTS > stopWatchTSmax)
            {
                stopWatchTSmax = stopWatchTS;
                gNbmax = gNb;
                headerMax = header;
            }
            if (stopWatchTSavg > 0)
            {
                stopWatchTSavg += stopWatchTS;
                stopWatchTSavg = stopWatchTSavg / 2;
            }
            else
                stopWatchTSavg = stopWatchTS;
            //if (gNb>15000) backgroundWorkerRF2Read.CancelAsync();
        }

        private bool ReadFileDataRF2(string NameFile)
        {
            int rate;
            Boolean TreatmentGreen = false;     // false les données de scoring ne sont pas passées pas de traitement
                                                // true les données de scoring sont passées permettant d'identifier les paramêtres NbVehicule etc..
            using (FileStream stream = new FileStream(NameFile, FileMode.Open))
            {
                while (stream.Position < stream.Length)
                {
                    TelemetryData telemetryData = new TelemetryData();
                    Header header = dataUDP.dataRF2.ReadHeader(stream);
                    if (header.DataType == DataType.scoring)
                        TreatmentGreen = true;

                    if (TreatmentGreen)
                    {

                        telemetryData = dataUDP.dataRF2.InitPacketFormTelemetry(stream, header, TreatmentGreen);
                        dataUDP.ListTelemetryData.Add(telemetryData);
                        dataUDP.DriverListUpdate(telemetryData);
                    }
                    else
                        telemetryData = dataUDP.dataRF2.InitPacketFormTelemetry(stream, header, TreatmentGreen);


                    if (!FlagDebug)
                    if (backgroundWorkerRF2Read.CancellationPending)
                    {
                        return false;
                    }

                    rate = (int)((100 * stream.Position) / stream.Length);
                    if (!FlagDebug)
                        backgroundWorkerRF2Read.ReportProgress(rate);//, progressInfo);
                }
            }
            if (!FlagDebug)
                backgroundWorkerRF2Read.ReportProgress(100);//, progressInfo);
            return true;
        }

        private void call_telemetrieRF2()
        {
            string NameFile; // = "test.bin";

            oFD_RAWdata.InitialDirectory = dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata;

            if (oFD_RAWdata.ShowDialog() == DialogResult.OK)
            {
                NameFile = oFD_RAWdata.FileName;
                rTB_Terminal.AppendText("Ouverture des données RF2 dans un fichier " + NameFile + "\n");

                if (System.IO.File.Exists(NameFile)) /// 
                {
                    rTB_Terminal.AppendText("before dataUDP.ListTelemetryData :" + dataUDP.ListTelemetryData.Count());

                    dataUDP.ListTelemetryData.Clear();
                    dataUDP.ListDriver.Clear();
                    dataUDP.dataRF2.scoring.Clear();
                    dataUDP.dataRF2.telemetry.Clear();
                    dataUDP.dataRF2.weather.Clear();
                    dataUDP.dataRF2.rules.Clear();
                    dataUDP.dataRF2.pitInfo.Clear();
                    dataUDP.dataRF2.graphics.Clear();
                    dataUDP.dataRF2.forceFeedback.Clear();
                    dataUDP.dataRF2.extended.Clear();
                    dataUDP.dataRF2.telemetryLight.Clear();
                    dataUDP.dataRF2.telemetryList.Clear();
                    rTB_Terminal.AppendText("after dataUDP.ListTelemetryData :" + dataUDP.ListTelemetryData.Count());

                    backgroundWorkerRF2Read.RunWorkerAsync(NameFile);
                }
            }

        }
        //*****************************************************************************************

        /// Récupération en mémoire des données de télémétrie RF2 du  simulateur - Appel du Form rF2Log
        private void ReadRF2data()
        {
            fmRF2log _fmRF2log = new fmRF2log();


            if (_fmRF2log.Visible)
            {
                _fmRF2log.Focus();
                _fmRF2log.SendData(dataSetting);
            }
            else
            {
                _fmRF2log.Show();
                _fmRF2log.SendData(dataSetting);
            }
        }


        // A transférer dans rF2 si possible
        private static string GetStringFromBytes(byte[] bytes)
        {
            if (bytes == null)
                return "";

            var nullIdx = Array.IndexOf(bytes, (byte)0);

            return nullIdx >= 0
                ? Encoding.Default.GetString(bytes, 0, nullIdx)
                : Encoding.Default.GetString(bytes);
        }
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

        // Affichage des données de télémétrie rF2 en live dans l'onglet Info
        private void ReceiveRF2data(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var gameStateText = new StringBuilder();

            if (!this.RF2connected)
            {
                var brush = new SolidBrush(System.Drawing.Color.Black);
                g.DrawString("Not connected.", SystemFonts.DefaultFont, brush, 3.0f, 3.0f);
            }
            else
            {
                if (true)
                {
                    var brush = new SolidBrush(System.Drawing.Color.Green);

                    var currX = 3.0f;
                    var currY = 3.0f;
                    float yStep = SystemFonts.DefaultFont.Height;

                    gameStateText.Clear();


                    // Traitement Secondaire 
                    // Franck à comprende la gestion des véhicules in out 
                    // Build map of mID -> telemetry.mVehicles[i].
                    // They are typically matching values, however, we need to handle online cases and dropped vehicles (mID can be reused).
                    var idsToTelIndices = new Dictionary<long, int>();
                    for (int i = 0; i < this.telemetry.mNumVehicles; ++i)
                    {
                        if (!idsToTelIndices.ContainsKey(this.telemetry.mVehicles[i].mID))
                            idsToTelIndices.Add(this.telemetry.mVehicles[i].mID, i);
                    }

                    // Identification IA et autre???
                    var playerVehScoring = GetPlayerScoring(ref this.scoring);

                    var scoringPlrId = playerVehScoring.mID;
                    var playerVeh = new rF2VehicleTelemetry();
                    int resolvedPlayerIdx = -1;  // We're fine here with unitialized vehicle telemetry..
                    if (idsToTelIndices.ContainsKey(scoringPlrId))
                    {
                        resolvedPlayerIdx = idsToTelIndices[scoringPlrId];
                        playerVeh = this.telemetry.mVehicles[resolvedPlayerIdx];
                    }

                    // Figure out prev session end player mID
                    var playerSessionEndInfo = new rF2VehScoringCapture();
                    for (int i = 0; i < this.extended.mSessionTransitionCapture.mNumScoringVehicles; ++i)
                    {
                        var veh = this.extended.mSessionTransitionCapture.mScoringVehicles[i];
                        if (veh.mIsPlayer == 1)
                            playerSessionEndInfo = veh;
                    }
                    // Traitement Secondaire 

                    // Affichage
                    if (true)
                    {
                        gameStateText.Append(
                            "mElapsedTime:\n"
                            + "mCurrentET:\n"
                            + "mElapsedTime-mCurrentET:\n"
                            + "mDetlaTime:\n"
                            + "mInvulnerable:\n"
                            + "mVehicleName:\n"
                            + "mTrackName:\n"
                            + "mLapStartET:\n"
                            + "mLapDist:\n"
                            + "mEndET:\n"
                            + "mPlayerName:\n"
                            + "mPlrFileName:\n\n"
                            + "Session Started:\n"
                            + "Sess. End Session:\n"
                            + "Sess. End Phase:\n"
                            + "Sess. End Place:\n"
                            + "Sess. End Finish:\n"
                            + "Display msg capture:\n"
                            );

                        // Col 1 labels
                        g.DrawString(gameStateText.ToString(), SystemFonts.DefaultFont, brush, currX, currY += yStep);

                        gameStateText.Clear();

                        gameStateText.Append(
                                $"{playerVeh.mElapsedTime:N3}\n"
                                + $"{this.scoring.mScoringInfo.mCurrentET:N3}\n"
                                + $"{(playerVeh.mElapsedTime - this.scoring.mScoringInfo.mCurrentET):N3}\n"
                                + $"{playerVeh.mDeltaTime:N3}\n"
                                + (this.extended.mPhysics.mInvulnerable == 0 ? "off" : "on") + "\n"
                                + $"{GetStringFromBytes(playerVeh.mVehicleName)}\n"
                                + $"{GetStringFromBytes(playerVeh.mTrackName)}\n"
                                + $"{playerVeh.mLapStartET:N3}\n"
                                + $"{this.scoring.mScoringInfo.mLapDist:N3}\n"
                                + (this.scoring.mScoringInfo.mEndET < 0.0 ? "Unknown" : this.scoring.mScoringInfo.mEndET.ToString("N3")) + "\n"
                                + $"{GetStringFromBytes(this.scoring.mScoringInfo.mPlayerName)}\n"
                                + $"{GetStringFromBytes(this.scoring.mScoringInfo.mPlrFileName)}\n\n"
                                + $"{this.extended.mSessionStarted != 0}\n"
                                + $"{RF2.GetSessionString(this.extended.mSessionTransitionCapture.mSession)}\n"
                                + $"{(rFactor2Constants.rF2GamePhase)this.extended.mSessionTransitionCapture.mGamePhase}\n"
                                + $"{playerSessionEndInfo.mPlace}\n"
                                + $"{(rFactor2Constants.rF2FinishStatus)playerSessionEndInfo.mFinishStatus}\n"
                                + $"{GetStringFromBytes(this.extended.mDisplayedMessageUpdateCapture)}\n"
                                );

                        // Col1 values
                        g.DrawString(gameStateText.ToString(), SystemFonts.DefaultFont, Brushes.Purple, currX + 145, currY);
                    }

                    // Affichage
                    // Print buffer stats.
                    if (true)
                    {

                        if (this.extended.mDirectMemoryAccessEnabled == 1)
                        {
                            gameStateText.Clear();
                            gameStateText.Append(
                                "Status:\n"
                                + "Last MC msg:\n"
                                + "Pit Speed Limit:\n"
                                + "Last LSI Phase:\n"
                                + "Last LSI Pit:\n"
                                + "Last LSI Order:\n"
                                + "Last SCR Instr.:\n"
                                );

                            g.DrawString(gameStateText.ToString(), SystemFonts.DefaultFont, Brushes.Purple, 1500, 660);

                            gameStateText.Clear();
                            gameStateText.Append(
                                GetStringFromBytes(this.extended.mStatusMessage) + '\n'
                                + GetStringFromBytes(this.extended.mLastHistoryMessage) + '\n'
                                + (int)(this.extended.mCurrentPitSpeedLimit * 3.6f + 0.5f) + "kph\n"
                                + GetStringFromBytes(this.extended.mLSIPhaseMessage) + '\n'
                                + GetStringFromBytes(this.extended.mLSIPitStateMessage) + '\n'
                                + GetStringFromBytes(this.extended.mLSIOrderInstructionMessage) + '\n'
                                + GetStringFromBytes(this.extended.mLSIRulesInstructionMessage) + '\n'
                                );

                            g.DrawString(gameStateText.ToString(), SystemFonts.DefaultFont, Brushes.Purple, 1580, 660);

                            gameStateText.Clear();
                            gameStateText.Append(
                                "updated: " + this.extended.mTicksStatusMessageUpdated + '\n'
                                + "updated: " + this.extended.mTicksLastHistoryMessageUpdated + '\n'
                                + '\n'
                                + "updated: " + this.extended.mTicksLSIPhaseMessageUpdated + '\n'
                                + "updated: " + this.extended.mTicksLSIPitStateMessageUpdated + '\n'
                                + "updated: " + this.extended.mTicksLSIOrderInstructionMessageUpdated + '\n'
                                + "updated: " + this.extended.mTicksLSIRulesInstructionMessageUpdated + '\n');

                            g.DrawString(gameStateText.ToString(), SystemFonts.DefaultFont, Brushes.Purple, 1800, 660);
                        }

                        if ((this.extended.mUnsubscribedBuffersMask & (long)SubscribedBuffer.PitInfo) == 0)
                        {
                            // Print pit info:
                            gameStateText.Clear();

                            gameStateText.Append(
                                "PI Cat Index:\n"
                                + "PI Cat Name:\n"
                                + "PI Choice Index:\n"
                                + "PI Choice String:\n"
                                + "PI Num Choices:\n"
                                );

                            g.DrawString(gameStateText.ToString(), SystemFonts.DefaultFont, Brushes.Orange, 1500, 750);

                        }

                        gameStateText.Clear();
                        var catName = GetStringFromBytes(this.pitInfo.mPitMneu.mCategoryName);
                        var choiceStr = GetStringFromBytes(this.pitInfo.mPitMneu.mChoiceString);

                        gameStateText.Append(
                            this.pitInfo.mPitMneu.mCategoryIndex + "\n"
                            + (string.IsNullOrWhiteSpace(catName) ? "<empty>" : catName) + "\n"
                            + this.pitInfo.mPitMneu.mChoiceIndex + "\n"
                            + (string.IsNullOrWhiteSpace(choiceStr) ? "<empty>" : choiceStr) + "\n"
                            + this.pitInfo.mPitMneu.mNumChoices + "\n"
                            );

                        g.DrawString(gameStateText.ToString(), SystemFonts.DefaultFont, Brushes.Orange, 1600, 750);
                    }

                    if (this.scoring.mScoringInfo.mNumVehicles == 0
                        || resolvedPlayerIdx == -1)  // We need telemetry for stats below.
                        return;

                    gameStateText.Clear();

                    // Affichage et Traitement Secondaire télémétrie

                    if (true)
                    {
                        gameStateText.Append(
                            "mTimeIntoLap:\n"
                            + "mEstimatedLapTime:\n"
                            + "mTimeBehindNext:\n"
                            + "mTimeBehindLeader:\n"
                            + "mPitGroup:\n"
                            + "mLapDist(Plr):\n"
                            + "mLapDist(Est):\n"

                            + "X :\n"
                            + "Y :\n"
                            + "Z :\n"

                            + "yaw:\n"
                            + "pitch:\n"
                            + "roll:\n"
                            + "speed:\n");

                        // Col 2 labels
                        g.DrawString(gameStateText.ToString(), SystemFonts.DefaultFont, brush, currX += 275, currY);
                        gameStateText.Clear();


                        // Calculate derivatives:
                        var yaw = Math.Atan2(playerVeh.mOri[RowZ].x, playerVeh.mOri[RowZ].z);

                        var pitch = Math.Atan2(-playerVeh.mOri[RowY].z,
                            Math.Sqrt(playerVeh.mOri[RowX].z * playerVeh.mOri[RowX].z + playerVeh.mOri[RowZ].z * playerVeh.mOri[RowZ].z));

                        var roll = Math.Atan2(playerVeh.mOri[RowY].x,
                            Math.Sqrt(playerVeh.mOri[RowX].x * playerVeh.mOri[RowX].x + playerVeh.mOri[RowZ].x * playerVeh.mOri[RowZ].x));

                        var speed = Math.Sqrt((playerVeh.mLocalVel.x * playerVeh.mLocalVel.x)
                            + (playerVeh.mLocalVel.y * playerVeh.mLocalVel.y)
                            + (playerVeh.mLocalVel.z * playerVeh.mLocalVel.z));

                        // Estimate lapdist
                        // See how much ahead telemetry is ahead of scoring update
                        var delta = playerVeh.mElapsedTime - scoring.mScoringInfo.mCurrentET;
                        var lapDistEstimated = playerVehScoring.mLapDist;
                        if (delta > 0.0)
                        {
                            var localZAccelEstimated = playerVehScoring.mLocalAccel.z * delta;
                            var localZVelEstimated = playerVehScoring.mLocalVel.z + localZAccelEstimated;

                            lapDistEstimated = playerVehScoring.mLapDist - localZVelEstimated * delta;
                        }

                        gameStateText.Append(
                            $"{playerVehScoring.mTimeIntoLap:N3}\n"
                            + $"{playerVehScoring.mEstimatedLapTime:N3}\n"
                            + $"{playerVehScoring.mTimeBehindNext:N3}\n"
                            + $"{playerVehScoring.mTimeBehindLeader:N3}\n"
                            + $"{GetStringFromBytes(playerVehScoring.mPitGroup)}\n"
                            + $"{playerVehScoring.mLapDist:N3}\n"
                            + $"{lapDistEstimated:N3}\n"

                            + $"{playerVeh.mPos.x:N3}\n"
                            + $"{playerVeh.mPos.y:N3}\n"
                            + $"{playerVeh.mPos.z:N3}\n"


                            + $"{yaw:N3}\n"
                            + $"{pitch:N3}\n"
                            + $"{roll:N3}\n"
                            + string.Format("{0:n3} m/s {1:n4} km/h\n", speed, speed * 3.6));

                        // Col2 values
                        g.DrawString(gameStateText.ToString(), SystemFonts.DefaultFont, Brushes.Purple, currX + 120, currY);
                    }
                }

                // sauvegarde dans la liste les informations
                try
                {
                    //ListTelemetrie.Add(new DataTelemetry(this.telemetry));
                    //save_V3(); //this.telemetry);

                }
                catch (Exception ex)
                {
                    tB_info.Text = "\nError: " + ex + " " + ex.Message + "Objet :" + ex.Source + "Link :" + ex.HelpLink;
                    Console.WriteLine("Error:" + ex + " " + ex.Message + "Objet :" + ex.Source + "Link :" + ex.HelpLink); // A mettre dans fenêtre
                }

                gameStateText.Clear();

                gameStateText.Append(
                    "*************************\n"
                    + "Taille struc telemetry"
                    + $"{Marshal.SizeOf(telemetry):N3}\n"
                    + "Taille struc scoring"
                    + $"{Marshal.SizeOf(scoring):N3}\n"
                    + "Taille struc rules"
                    + $"{Marshal.SizeOf(rules):N3} \n"
                    + "Taille struc forceFeedback"
                    + $"{Marshal.SizeOf(forceFeedback):N3} \n"
                    + "Taille struc graphics"
                    + $"{Marshal.SizeOf(graphics):N3} \n"
                    + "Taille struc pitInfo"
                    + $"{Marshal.SizeOf(pitInfo):N3}  \n"
                    + "Taille struc weather"
                    + $"{Marshal.SizeOf(weather):N3}  \n"
                    + "Taille struc extended"
                    + $"{Marshal.SizeOf(extended):N3}  \n"
                    + "*************************\n"
                    );
                g.DrawString(gameStateText.ToString(), SystemFonts.DefaultFont, Brushes.Purple, 600, 370);

                if (false)
                    using (StreamWriter outputFile = new StreamWriter("E:\\Telemetrie\\RF2\\" + "Marshal.SizeOf5.txt", true))
                    {
                        outputFile.WriteLine(
                        "*************************\n"
                        + "Taille struc telemetry"
                        + $"{Marshal.SizeOf(telemetry):N3}\n"
                        + "Taille struc scoring"
                        + $"{Marshal.SizeOf(scoring):N3}\n"
                        + "Taille struc rules"
                        + $"{Marshal.SizeOf(rules):N3} \n"
                        + "Taille struc forceFeedback"
                        + $"{Marshal.SizeOf(forceFeedback):N3} \n"
                        + "Taille struc graphics"
                        + $"{Marshal.SizeOf(graphics):N3} \n"
                        + "Taille struc pitInfo"
                        + $"{Marshal.SizeOf(pitInfo):N3}  \n"
                        + "Taille struc weather"
                        + $"{Marshal.SizeOf(weather):N3}  \n"
                        + "Taille struc extended"
                        + $"{Marshal.SizeOf(extended):N3}  \n"
                        + "*************************\n"
                            );
                    }
            }

        }

        //rFactor2 traitement des données
        //*****************************************************************************************



        //*****************************************************************************************
        //Gestion des menus et boutons

        private void onglet_info()
        {
            int NumCar;
            int NumLap = 0;

            if (dataUDP != null)
            {
                // Mise à jour information
                ldate.Text = date_of_file.ToString();

                lm_trackId.Text = dataUDP.Get_trackname();
                lm_trackLength.Text = dataUDP.Get_trackLength().ToString();
                lm_networkGame.Text = dataUDP.Get_m_networkGame();
                lm_trackTemperature.Text = dataUDP.Get_m_trackTemperature().ToString();

                lm_airTemperature.Text = dataUDP.Get_m_airTemperature().ToString();
                lm_sessionType.Text = dataUDP.Get_m_sessionType();
                lm_formula.Text = dataUDP.Get_formulaname();
                lm_totalLaps.Text = dataUDP.Get_m_totalLaps().ToString();
                lm_weather.Text = dataUDP.Get_meteo();
                lm_aiDifficulty.Text = dataUDP.Get_m_aiDifficulty().ToString();
                lm_spectatorCarIndex.Text = dataUDP.Get_playerCarIndex(0).ToString();
                lm_playerCarIndex.Text = dataUDP.Get_NumofPlayers().ToString();

                NumCar = dataUDP.Get_NumofPlayers();

                lm_bestLapTimeLap.Text = TimeSpan.FromMilliseconds(dataUDP.Get_bestLapTime_Lap(NumCar, out NumLap)).ToString();
                lm_bestLapTimeLapNum.Text = NumLap.ToString();

                lm_bestSector1Lap.Text = TimeSpan.FromMilliseconds(dataUDP.Get_BestSector1TimeInMS(NumCar, out NumLap)).ToString();
                lm_bestSector1LapNum.Text = NumLap.ToString();

                lm_bestSector2Lap.Text = TimeSpan.FromMilliseconds(dataUDP.Get_BestSector2TimeInMS(NumCar, out NumLap)).ToString();
                lm_bestSector2LapNum.Text = NumLap.ToString();

                lm_bestSector3Lap.Text = TimeSpan.FromMilliseconds(dataUDP.Get_BestSector3TimeInMS(NumCar, out NumLap)).ToString();
                lm_bestSector3LapNum.Text = NumLap.ToString();

            }
        }
        private void onglet_infoOLD()
        {
            int NumCar;
            int NumLap = 0;

            if (dataf1_2021 != null)
            {
                // Mise à jour information
                ldate.Text = date_of_file.ToString();

                lm_trackId.Text = dataf1_2021.Trackname[dataf1_2021.ListSession[0].m_trackId]; //.ToString();
                lm_trackLength.Text = dataf1_2021.ListSession[0].m_trackLength.ToString() + " Mêtres";
                if (dataf1_2021.ListSession[0].m_networkGame == 1) lm_networkGame.Text = "online"; else lm_networkGame.Text = "offline";
                lm_trackTemperature.Text = dataf1_2021.ListSession[0].m_trackTemperature.ToString() + " °C";

                lm_airTemperature.Text = dataf1_2021.ListSession[0].m_airTemperature.ToString() + "°C";
                lm_sessionType.Text = dataf1_2021.sessiontypename[dataf1_2021.ListSession[0].m_sessionType];
                lm_formula.Text = dataf1_2021.formulaname[dataf1_2021.ListSession[0].m_formula];
                lm_totalLaps.Text = dataf1_2021.ListSession[0].m_totalLaps.ToString();
                lm_weather.Text = dataf1_2021.meteoname[dataf1_2021.ListSession[0].m_weather];
                lm_aiDifficulty.Text = dataf1_2021.ListSession[0].m_aiDifficulty.ToString();
                lm_spectatorCarIndex.Text = dataf1_2021.ListLapData[0].packetHeader.m_playerCarIndex.ToString(); //dataf1_2021.ListSession[0].m_spectatorCarIndex.ToString();
                lm_playerCarIndex.Text = dataf1_2021.packetHeader.m_playerCarIndex.ToString();

                NumCar = dataf1_2021.ListLapData[0].packetHeader.m_playerCarIndex;

                lm_bestLapTimeLap.Text = TimeSpan.FromMilliseconds(dataf1_2021.g_bestLapTimeLap(NumCar, out NumLap)).ToString();
                lm_bestLapTimeLapNum.Text = NumLap.ToString();

                lm_bestSector1Lap.Text = TimeSpan.FromMilliseconds(dataf1_2021.g_BestSector1TimeInMS(NumCar, out NumLap)).ToString();
                lm_bestSector1LapNum.Text = NumLap.ToString();

                lm_bestSector2Lap.Text = TimeSpan.FromMilliseconds(dataf1_2021.g_BestSector2TimeInMS(NumCar, out NumLap)).ToString();
                lm_bestSector2LapNum.Text = NumLap.ToString();

                lm_bestSector3Lap.Text = TimeSpan.FromMilliseconds(dataf1_2021.g_BestSector3TimeInMS(NumCar, out NumLap)).ToString();
                lm_bestSector3LapNum.Text = NumLap.ToString();

            }
        }

        private void btn_uDP_Click(object sender, EventArgs e) // bouton rouge RECEIVE
        {
            
            
            switch (dataSetting.Simulation)
            {
                case 0:         // F 202X
                    // Version future éventuellement UDPreadClass();
                    UDPread();
                    break;
                case 1:         // Project Car 2
                    break;
                case 2:         // RFactor 2
                    ReadRF2data();
                    break;
                case 3:         // Assetto Corsa Comp
                    break;
                case 4:         // Assetto Corsa 
                    break;
                case 5:         // Forza
                    break;
            }

        }

        private void saveDataRawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataSetting.Simulation == 0)
                saveF1data();
            else
                rTB_Terminal.AppendText("Ceux ne sont pas des données de F1 doncpas de sauvegarde pour le moment \n");
        }

        private void openDataRawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataSetting.Simulation == 0)
                loadF1data();
            else
                rTB_Terminal.AppendText("Ceux ne sont pas des données de F1 donc pas de lecture directe pour le moment \n");
        }

        // Appel du Form Télémétrie
        private void temetrieGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // On s'assure que Simulation est correctement initialisé dans DataUDP
            dataUDP.SelectSimulation(dataSetting.Simulation);

            switch (dataSetting.Simulation)
            {
                case 0:         // F 202X
                    //call_telemetrieold();
                    call_telemetrieF1202X();
                    break;
                case 1:         // Project Car 2
                    break;
                case 2:         // RFactor 2
                    if (!backgroundWorkerRF2ReadisWorkerRunning)
                        call_telemetrieRF2();
                    else
                        rTB_Terminal.AppendText("Une activité est déjà en cours\n");
                    break;
                case 3:         // Assetto Corsa Comp
                    break;
                case 4:         // Assetto Corsa 
                    break;
                case 5:         // Forza
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e) // bouton traitement
        {
            switch (dataSetting.Simulation)
            {
                case 0:         // F1 202X
                    loadF1data();
                    traitementF1202X();
                    rTB_Terminal.AppendText("\nfin du traitement F1 ");
                    break;
                case 1:         // Project Car 2
                    break;
                case 2:         // RFactor 2
                    traitementRF2();
                    break;
                case 3:         // Assetto Corsa Comp
                    break;
                case 4:         // Assetto Corsa 
                    break;
                case 5:         // Forza
                    break;
            }

        }

        private void generalSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSetting formSetting = new FormSetting();

            if (formSetting.Visible)
            {
                formSetting.Focus();
                formSetting.SendData(dataSetting);
            }
            else
            {
                formSetting.Show();
                formSetting.SendData(dataSetting);
            }
        }

        private void transfertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //writedata();
            dataUDP.dataf1_2021.InfluxDBwrite();
        }

        private void requestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            writedata2();
        }

        private void myIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormIP formIPg = new FormIP();

            if (formIPg.Visible)
            {
                formIPg.Focus();
                //formIPg.SendData(dataSetting);
            }
            else
            {
                formIPg.Show();
                //formIPg.SendData(dataSetting);
            }
        }

        private void traitementRawDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormRawAnalyse formRawAnalyse = new FormRawAnalyse();

            if (formRawAnalyse.Visible)
            {
                formRawAnalyse.Focus();
                formRawAnalyse.SendData(dataUDP.dataf1_2021, dataSetting);
                formRawAnalyse.SendRawData(ListReceived);
            }
            else
            {
                formRawAnalyse.Show();
                formRawAnalyse.SendData(dataUDP.dataf1_2021, dataSetting);
                formRawAnalyse.SendRawData(ListReceived);
            }
        }

        private void FilesAnalyzeOLD()
        {
            // https://www.delftstack.com/fr/howto/csharp/get-all-files-in-a-directory-in-csharp/#:~:text=GetFiles()%20en%20C%23%20obtient,les%20param%C3%A8tres%20de%20la%20m%C3%A9thode.
            dGVdata.Rows.Clear();
            dGVdata.Columns.Clear();
            dGVdata.Columns.Add("File Name", "File Name");
            dGVdata.Columns.Add("m_packetFormat", "Format");
            dGVdata.Columns.Add("m_trackId", "Track Name");
            dGVdata.Columns.Add("m_trackLength", "Track Length");
            dGVdata.Columns.Add("m_networkGame", "Network");
            dGVdata.Columns.Add("m_trackTemperature", "Track T°C");
            dGVdata.Columns.Add("m_airTemperature", "Air T°C");
            dGVdata.Columns.Add("sessiontypename", "Session");
            dGVdata.Columns.Add("formulaname", "Formule");
            dGVdata.Columns.Add("m_totalLaps", "Nb Laps");
            dGVdata.Columns.Add("meteoname", "Météo");
            dGVdata.Columns.Add("m_aiDifficulty", "AI Level");
            dGVdata.Columns.Add("m_playerCarIndex", "Car N°");

            void _traitement(string _Name, string _FullName)
            {
                string _m_networkGame;
                int i;

                rTB_Terminal.AppendText("Traitement en cours \n");

                // structure locale
                UDP_f12021 dataf1_2021 = new UDP_f12021();

                foreach (var ElemReceived in ListReceived)
                {
                    Stream stream = new MemoryStream(ElemReceived);
                    var binaryReaderRAW = new BinaryReader(stream);
                    try
                    {
                        dataf1_2021.InitPackets(stream, binaryReaderRAW);
                    }
                    catch
                    {
                        rTB_Terminal.AppendText("Probléme de traitement");
                        break;
                    }
                    if ((dataf1_2021.ListSession.Count > 0) &&
                        (dataf1_2021.ListCarDamage.Count > 0)) break;

                }
                if (dataf1_2021.ListSession.Count != 0)
                {
                    if (dataf1_2021.ListSession[0].m_networkGame == 1)
                        _m_networkGame = "online";
                    else
                        _m_networkGame = "offline";

                    ListFileInfo.Add(new FileInfoData()
                    {
                        NameFile = _Name,
                        FullNameFile = _FullName,
                        m_packetFormat = dataf1_2021.ListCarDamage[0].packetHeader.m_packetFormat,
                        m_trackId = dataf1_2021.Trackname[dataf1_2021.ListSession[0].m_trackId],
                        m_trackLength = dataf1_2021.ListSession[0].m_trackLength,
                        m_networkGame = _m_networkGame,
                        m_trackTemperature = dataf1_2021.ListSession[0].m_trackTemperature,
                        m_airTemperature = dataf1_2021.ListSession[0].m_airTemperature,
                        sessiontypename = dataf1_2021.sessiontypename[dataf1_2021.ListSession[0].m_sessionType],
                        formulaname = dataf1_2021.formulaname[dataf1_2021.ListSession[0].m_formula],
                        m_totalLaps = dataf1_2021.ListSession[0].m_totalLaps,
                        meteoname = dataf1_2021.meteoname[dataf1_2021.ListSession[0].m_weather],
                        m_aiDifficulty = dataf1_2021.ListSession[0].m_aiDifficulty.ToString(),
                        m_playerCarIndex = dataf1_2021.packetHeader.m_playerCarIndex.ToString()
                    });
                    i = ListFileInfo.Count - 1;
                    string[] rowData = {
                              ListFileInfo[i].NameFile,
                              ListFileInfo[i].m_packetFormat.ToString(), // = dataf1_2021.ListCarDamage[0].packetHeader.m_packetFormat.ToString(),
                              ListFileInfo[i].m_trackId , // dataf1_2021.Trackname[dataf1_2021.ListSession[0].m_trackId],
                              ListFileInfo[i].m_trackLength.ToString() , // dataf1_2021.ListSession[0].m_trackLength.ToString(),
                              ListFileInfo[i].m_networkGame , // _m_networkGame,
                              ListFileInfo[i].m_trackTemperature.ToString() , // dataf1_2021.ListSession[0].m_trackTemperature,
                              ListFileInfo[i].m_airTemperature.ToString() , // dataf1_2021.ListSession[0].m_airTemperature,
                              ListFileInfo[i].sessiontypename , // dataf1_2021.sessiontypename[dataf1_2021.ListSession[0].m_sessionType],
                              ListFileInfo[i].formulaname , // dataf1_2021.formulaname[dataf1_2021.ListSession[0].m_formula],
                              ListFileInfo[i].m_totalLaps.ToString() , // dataf1_2021.ListSession[0].m_totalLaps.ToString(),
                              ListFileInfo[i].meteoname , // dataf1_2021.meteoname[dataf1_2021.ListSession[0].m_weather],
                              ListFileInfo[i].m_aiDifficulty , // dataf1_2021.ListSession[0].m_aiDifficulty.ToString(),
                              ListFileInfo[i].m_playerCarIndex  // dataf1_2021.packetHeader.m_playerCarIndex.ToString()
                                };
                    dGVdata.Rows.Add(rowData);
                }
                else
                    rTB_Terminal.AppendText("Aucune donnée" + "\n");
            }

            void _Openfile(string NameFile)
            {
                int longueur = 10;
                byte[] BufferByte = new byte[4];

                rTB_Terminal.AppendText("Ouverture RAW dans un fichier " + NameFile + "\n");

                if (System.IO.File.Exists(NameFile)) /// 
                {
                    rTB_Terminal.AppendText("Chargement RAW lancée " + NameFile + "\n");
                    Stream writingStream = new FileStream(NameFile, FileMode.Open);

                    try
                    {
                        if (writingStream.CanRead) // vérifie si lecture possible dans le fichier
                        {
                            ListReceived.Clear();

                            FileInfo file = new FileInfo(NameFile);
                            // file creation time
                            date_of_file = file.CreationTime;

                            while (longueur > -1) // lecture du  fichier
                            {
                                writingStream.Read(BufferByte, 0, 4);
                                longueur = BitConverter.ToInt32(BufferByte, 0);
                                byte[] ElemReceived = new byte[longueur];

                                if (writingStream.Read(ElemReceived, 0, longueur) == 0) longueur = -2;
                                ListReceived.Add(ElemReceived); // stock les données brutes avant le traitement
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        rTB_Terminal.AppendText("Error: " + ex + "\n");
                        Console.WriteLine("Error:" + ex); // A mettre dans fenêtre
                    }
                    finally
                    {
                        // Fermez Stream, libérez des ressources
                        writingStream.Close();
                        rTB_Terminal.AppendText("Fin de lecture RAW dans fichier " + NameFile + "\n");
                    }
                }
            }

            fBDselect.SelectedPath = dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata;

            if (fBDselect.ShowDialog() == DialogResult.OK)
            {
                DirectoryInfo di = new DirectoryInfo(fBDselect.SelectedPath);
                FileInfo[] files = di.GetFiles("*.*");

                rTB_Terminal.AppendText("\n Liste et data des fichiers\n");
                foreach (FileInfo file in files)
                {
                    rTB_Terminal.AppendText("\n ***************************************************** \n");
                    rTB_Terminal.AppendText(file.Name + "\n");
                    _Openfile(file.FullName);
                    _traitement(file.Name, file.FullName);
                }
            }

        }

        private void FilesAnalyze()
        {
            switch (dataSetting.Simulation)
            {
                case 0:         // F 202X
                    FilesAnalyze_f12021();
                    break;
                case 1:         // Project Car 2
                    break;
                case 2:         // RFactor 2
                    FilesAnalyze_RF2();
                    break;
                case 3:         // Assetto Corsa Comp
                    break;
                case 4:         // Assetto Corsa 
                    break;
                case 5:         // Forza
                    break;
            }
        }
        private void FilesAnalyze_f12021()
        {
            List<byte[]> ListReceived = new List<byte[]>();

            // https://www.delftstack.com/fr/howto/csharp/get-all-files-in-a-directory-in-csharp/#:~:text=GetFiles()%20en%20C%23%20obtient,les%20param%C3%A8tres%20de%20la%20m%C3%A9thode.
            dGVdata.Rows.Clear();
            dGVdata.Columns.Clear();
            dGVdata.Columns.Add("File Name", "File Name");
            dGVdata.Columns.Add("m_packetFormat", "Format");
            dGVdata.Columns.Add("m_trackId", "Track Name");
            dGVdata.Columns.Add("m_trackLength", "Track Length");
            dGVdata.Columns.Add("m_networkGame", "Network");
            dGVdata.Columns.Add("m_trackTemperature", "Track T°C");
            dGVdata.Columns.Add("m_airTemperature", "Air T°C");
            dGVdata.Columns.Add("sessiontypename", "Session");
            dGVdata.Columns.Add("formulaname", "Formule");
            dGVdata.Columns.Add("m_totalLaps", "Nb Laps");
            dGVdata.Columns.Add("meteoname", "Météo");
            dGVdata.Columns.Add("m_aiDifficulty", "AI Level");
            dGVdata.Columns.Add("m_playerCarIndex", "Car N°");

            void _traitement(string _Name, string _FullName)
            {
                string _m_networkGame;
                int i;

                rTB_Terminal.AppendText("Traitement en cours \n");


                UDP_f12021 dataf1_2021 = new UDP_f12021();

                foreach (var ElemReceived in ListReceived)
                {
                    Stream stream = new MemoryStream(ElemReceived);
                    var binaryReaderRAW = new BinaryReader(stream);
                    try
                    {
                        dataf1_2021.InitPackets(stream, binaryReaderRAW);
                    }
                    catch
                    {
                        rTB_Terminal.AppendText("Probléme de traitement");
                        break;
                    }
                    if ((dataf1_2021.ListSession.Count > 0) &&
                        (dataf1_2021.ListCarDamage.Count > 0)) break;

                }
                if (dataf1_2021.ListSession.Count != 0)
                {
                    if (dataf1_2021.ListSession[0].m_networkGame == 1)
                        _m_networkGame = "online";
                    else
                        _m_networkGame = "offline";

                    ListFileInfo.Add(new FileInfoData()
                    {
                        NameFile = _Name,
                        FullNameFile = _FullName,
                        m_packetFormat = dataf1_2021.ListCarDamage[0].packetHeader.m_packetFormat,
                        m_trackId = dataf1_2021.Trackname[dataf1_2021.ListSession[0].m_trackId],
                        m_trackLength = dataf1_2021.ListSession[0].m_trackLength,
                        m_networkGame = _m_networkGame,
                        m_trackTemperature = dataf1_2021.ListSession[0].m_trackTemperature,
                        m_airTemperature = dataf1_2021.ListSession[0].m_airTemperature,
                        sessiontypename = dataf1_2021.sessiontypename[dataf1_2021.ListSession[0].m_sessionType],
                        formulaname = dataf1_2021.formulaname[dataf1_2021.ListSession[0].m_formula],
                        m_totalLaps = dataf1_2021.ListSession[0].m_totalLaps,
                        meteoname = dataf1_2021.meteoname[dataf1_2021.ListSession[0].m_weather],
                        m_aiDifficulty = dataf1_2021.ListSession[0].m_aiDifficulty.ToString(),
                        m_playerCarIndex = dataf1_2021.packetHeader.m_playerCarIndex.ToString()
                    });
                    i = ListFileInfo.Count - 1;
                    string[] rowData = {
                              ListFileInfo[i].NameFile,
                              ListFileInfo[i].m_packetFormat.ToString(), // = dataf1_2021.ListCarDamage[0].packetHeader.m_packetFormat.ToString(),
                              ListFileInfo[i].m_trackId , // dataf1_2021.Trackname[dataf1_2021.ListSession[0].m_trackId],
                              ListFileInfo[i].m_trackLength.ToString() , // dataf1_2021.ListSession[0].m_trackLength.ToString(),
                              ListFileInfo[i].m_networkGame , // _m_networkGame,
                              ListFileInfo[i].m_trackTemperature.ToString() , // dataf1_2021.ListSession[0].m_trackTemperature,
                              ListFileInfo[i].m_airTemperature.ToString() , // dataf1_2021.ListSession[0].m_airTemperature,
                              ListFileInfo[i].sessiontypename , // dataf1_2021.sessiontypename[dataf1_2021.ListSession[0].m_sessionType],
                              ListFileInfo[i].formulaname , // dataf1_2021.formulaname[dataf1_2021.ListSession[0].m_formula],
                              ListFileInfo[i].m_totalLaps.ToString() , // dataf1_2021.ListSession[0].m_totalLaps.ToString(),
                              ListFileInfo[i].meteoname , // dataf1_2021.meteoname[dataf1_2021.ListSession[0].m_weather],
                              ListFileInfo[i].m_aiDifficulty , // dataf1_2021.ListSession[0].m_aiDifficulty.ToString(),
                              ListFileInfo[i].m_playerCarIndex  // dataf1_2021.packetHeader.m_playerCarIndex.ToString()
                                };
                    dGVdata.Rows.Add(rowData);
                }
                else
                    rTB_Terminal.AppendText("Aucune donnée" + "\n");
            }

            void _Openfile(string NameFile)
            {
                int longueur = 10;
                byte[] BufferByte = new byte[4];

                rTB_Terminal.AppendText("Ouverture RAW dans un fichier " + NameFile + "\n");

                if (System.IO.File.Exists(NameFile)) /// 
                {
                    rTB_Terminal.AppendText("Chargement RAW lancée " + NameFile + "\n");
                    Stream writingStream = new FileStream(NameFile, FileMode.Open);

                    try
                    {
                        if (writingStream.CanRead) // vérifie si lecture possible dans le fichier
                        {
                            ListReceived.Clear();

                            FileInfo file = new FileInfo(NameFile);
                            // file creation time
                            date_of_file = file.CreationTime;

                            while (longueur > -1) // lecture du  fichier
                            {
                                writingStream.Read(BufferByte, 0, 4);
                                longueur = BitConverter.ToInt32(BufferByte, 0);
                                byte[] ElemReceived = new byte[longueur];

                                if (writingStream.Read(ElemReceived, 0, longueur) == 0) longueur = -2;
                                ListReceived.Add(ElemReceived); // stock les données brutes avant le traitement
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        rTB_Terminal.AppendText("Error: " + ex + "\n");
                        Console.WriteLine("Error:" + ex); // A mettre dans fenêtre
                    }
                    finally
                    {
                        // Fermez Stream, libérez des ressources
                        writingStream.Close();
                        rTB_Terminal.AppendText("Fin de lecture RAW dans fichier " + NameFile + "\n");
                    }
                }
            }

            fBDselect.SelectedPath = dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata;

            if (fBDselect.ShowDialog() == DialogResult.OK)
            {
                DirectoryInfo di = new DirectoryInfo(fBDselect.SelectedPath);
                FileInfo[] files = di.GetFiles("*.*");

                rTB_Terminal.AppendText("\n Liste et data des fichiers\n");
                foreach (FileInfo file in files)
                {
                    rTB_Terminal.AppendText("\n ***************************************************** \n");
                    rTB_Terminal.AppendText(file.Name + "\n");
                    _Openfile(file.FullName);
                    _traitement(file.Name, file.FullName);
                }
            }

        }
        private void FilesAnalyze_RF2()
        {
            // https://www.delftstack.com/fr/howto/csharp/get-all-files-in-a-directory-in-csharp/#:~:text=GetFiles()%20en%20C%23%20obtient,les%20param%C3%A8tres%20de%20la%20m%C3%A9thode.
            dGVdata.Rows.Clear();
            dGVdata.Columns.Clear();


            DataGridViewTextBoxColumn FileNameColumn = new DataGridViewTextBoxColumn();
            FileNameColumn.HeaderText = "File Name";
            FileNameColumn.Name = "File Name";
            FileNameColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            FileNameColumn.ValueType = typeof(string); // Indiquez que c'est une colonne numérique
            FileNameColumn.Width = 150;
            dGVdata.Columns.Add(FileNameColumn);

            DataGridViewTextBoxColumn packetFormatColumn = new DataGridViewTextBoxColumn();
            packetFormatColumn.HeaderText = "Nb Car"; 
            packetFormatColumn.Name = "Packet Format";
            packetFormatColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            packetFormatColumn.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            packetFormatColumn.Width = 50;
            dGVdata.Columns.Add(packetFormatColumn);

            DataGridViewTextBoxColumn m_trackIdColumn = new DataGridViewTextBoxColumn();
            m_trackIdColumn.HeaderText = "Track Name";
            m_trackIdColumn.Name = "m_trackId";
            m_trackIdColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            m_trackIdColumn.ValueType = typeof(string); // Indiquez que c'est une colonne numérique
            m_trackIdColumn.Width = 150;
            dGVdata.Columns.Add(m_trackIdColumn);

            DataGridViewTextBoxColumn m_trackLengthColumn = new DataGridViewTextBoxColumn();
            m_trackLengthColumn.HeaderText = "Track Length";
            m_trackLengthColumn.Name = "m_trackLength";
            m_trackLengthColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_trackLengthColumn.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            dGVdata.Columns.Add(m_trackLengthColumn);

            dGVdata.Columns.Add("m_networkGame", "Network");


            DataGridViewTextBoxColumn m_trackTemperatureColumn = new DataGridViewTextBoxColumn();
            m_trackTemperatureColumn.HeaderText = "Track T°C"; 
            m_trackTemperatureColumn.Name = "m_trackTemperature";
            m_trackTemperatureColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_trackTemperatureColumn.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            dGVdata.Columns.Add(m_trackTemperatureColumn);

            DataGridViewTextBoxColumn m_airTemperatureColumn = new DataGridViewTextBoxColumn();
            m_airTemperatureColumn.HeaderText = "Air T°C"; 
            m_airTemperatureColumn.Name = "m_airTemperature";
            m_airTemperatureColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_airTemperatureColumn.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            dGVdata.Columns.Add(m_airTemperatureColumn); 

            dGVdata.Columns.Add("sessiontypename", "Session");

            DataGridViewTextBoxColumn formulanameColumn = new DataGridViewTextBoxColumn();
            formulanameColumn.HeaderText = "Formule";
            formulanameColumn.Name = "formulaname";
            formulanameColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            formulanameColumn.ValueType = typeof(string); // Indiquez que c'est une colonne numérique
            formulanameColumn.Width = 150;

            dGVdata.Columns.Add(formulanameColumn);

            DataGridViewTextBoxColumn m_totalLapsColumn = new DataGridViewTextBoxColumn();
            m_totalLapsColumn.HeaderText = "Nb Laps"; 
            m_totalLapsColumn.Name = "m_totalLaps";
            m_totalLapsColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_totalLapsColumn.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            dGVdata.Columns.Add(m_totalLapsColumn);

            dGVdata.Columns.Add("meteoname", "Météo");
            dGVdata.Columns.Add("m_aiDifficulty", "AI Level");
            dGVdata.Columns.Add("m_playerCarIndex", "Car N°");

            void _traitement(string _Name, string _FullName, TelemetryData telemetryData)
            {
                string _m_networkGame;
                string _mGamePhase;
                string _formulaname;
                uint _m_totalLaps;
                string _meteo;

                int i;

                rTB_Terminal.AppendText("Traitement en cours \n");


                if (telemetryData.mLapDist> 0)
                {
                    // FAUX 1 = server, 2 = client, 3 = server and client
                    if (telemetryData.mGameMode == 0) 
                        _m_networkGame = "offline";
                    else
                        _m_networkGame = "online";
                    //_m_networkGame = telemetryData.mGameMode.ToString(); 

                    // current session (0=testday 1-4=practice 5-8=qual 9=warmup 10-13=race)
                    int session = telemetryData.mSession;
                    if (session == 0)
                    {
                        _mGamePhase = "Testday";
                    }
                    else if (session >= 1 && session <= 4)
                    {
                        _mGamePhase = "Practice";
                    }
                    else if (session >= 5 && session <= 8)
                    {
                        _mGamePhase = "Qualif.";
                    }
                    else if (session == 9)
                    {
                        _mGamePhase = "Warmup";
                    }
                    else if (session >= 10 && session <= 13)
                    {
                        _mGamePhase = "Race";
                    }
                    else
                    {
                        _mGamePhase = session.ToString();
                    }


                    _meteo = "NC";
                    if (telemetryData.mCloudiness == 0) _meteo = dataf1_2021.meteoname[0];
                    else if(telemetryData.mCloudiness > 0) _meteo = dataf1_2021.meteoname[1];
                    else if (telemetryData.mCloudiness > 0.2) _meteo = dataf1_2021.meteoname[2];
                    else if (telemetryData.mCloudiness > 0.4) _meteo = dataf1_2021.meteoname[3];
                    else if (telemetryData.mCloudiness > 0.6) _meteo = dataf1_2021.meteoname[4];
                    else if (telemetryData.mCloudiness > 0.8) _meteo = dataf1_2021.meteoname[5];

                    _formulaname = dataUDP.CleanString(telemetryData.mVehicles[telemetryData.NbPlayer].mVehicleName);

                    _m_totalLaps = (uint)telemetryData.mMaxLaps;
                    if (_m_totalLaps>100000)
                        _m_totalLaps = (uint)Math.Round(telemetryData.mEndET/60,2);

                    ListFileInfo.Add(new FileInfoData()
                    {
                        NameFile = _Name,
                        FullNameFile = _FullName,
                        m_packetFormat = telemetryData.NbVehicles,
                        m_trackId = dataUDP.CleanString(telemetryData.mTrackName),
                        m_trackLength = Math.Round(telemetryData.mLapDist / 1000, 2),

                        m_networkGame = _m_networkGame,

                        m_trackTemperature = (sbyte)telemetryData.mTrackTemp,
                        m_airTemperature = (sbyte)telemetryData.mAmbientTemp,
                        sessiontypename = _mGamePhase,

                        formulaname = dataUDP.CleanString(telemetryData.mVehicles[telemetryData.NbPlayer].mVehicleName),

                        //scoring mMaxLaps
                        m_totalLaps = _m_totalLaps,

                        // scoring mRaining et mDarkCloud et mWind
                        meteoname = _meteo,
                        //??? rf2
                        m_aiDifficulty = "AI Level??",

                        m_playerCarIndex = telemetryData.NbPlayer.ToString() + "-" + dataUDP.CleanString(telemetryData.mVehicles[telemetryData.NbPlayer].mDriverName),
                    });; ;

                    i = ListFileInfo.Count - 1;

                    dGVdata.Rows.Add(new object[] 
                        {
                                  ListFileInfo[i].NameFile,
                                  ListFileInfo[i].m_packetFormat,
                                  ListFileInfo[i].m_trackId ,
                                  ListFileInfo[i].m_trackLength,
                                  ListFileInfo[i].m_networkGame ,
                                  ListFileInfo[i].m_trackTemperature,
                                  ListFileInfo[i].m_airTemperature ,
                                  ListFileInfo[i].sessiontypename ,
                                  ListFileInfo[i].formulaname ,
                                  ListFileInfo[i].m_totalLaps,
                                  ListFileInfo[i].meteoname ,
                                  ListFileInfo[i].m_aiDifficulty ,
                                  ListFileInfo[i].m_playerCarIndex

                        }
                    );
                }
                else
                    rTB_Terminal.AppendText("Aucune donnée" + "\n");
            }

            void _Openfile(string NameFile)
            {
                Boolean TreatmentGreen = false;     // false les données de scoring ne sont pas passées pas de traitement
                                                    // true les données de scoring sont passées permettant d'identifier les paramêtres NbVehicule etc..

                byte[] BufferByte = new byte[4];

                rTB_Terminal.AppendText("Ouverture RAW dans un fichier " + NameFile + "\n");

                if (System.IO.File.Exists(NameFile)) /// 
                {
                    rTB_Terminal.AppendText("Chargement RAW lancée " + NameFile + "\n");
                    Stream writingStream = new FileStream(NameFile, FileMode.Open);

                    try
                    {
                        if (writingStream.CanRead) // vérifie si lecture possible dans le fichier
                        {
                            FileInfo file = new FileInfo(NameFile);
                            // file creation time
                            date_of_file = file.CreationTime;

                            while ((writingStream.Position < writingStream.Length) & (!TreatmentGreen))
                            {
                                TelemetryData telemetryData = new TelemetryData();
                                Header header = dataUDP.dataRF2.ReadHeader(writingStream);

                                if (header.DataType == DataType.scoring)
                                    TreatmentGreen = true;

                                if (TreatmentGreen)
                                {
                                    telemetryData = dataUDP.dataRF2.InitPacketFormTelemetry(writingStream, header, TreatmentGreen);
                                }
                                else
                                    telemetryData = dataUDP.dataRF2.InitPacketFormTelemetry(writingStream, header, TreatmentGreen);

                                switch (header.DataType)
                                {
                                    case DataType.scoring:
                                        _traitement(file.Name, file.FullName, telemetryData);
                                        break;
                                    case DataType.telemetry: break;
                                    case DataType.graphics: break;
                                    case DataType.rules: break;
                                    case DataType.extended: break;
                                    case DataType.weather: break;
                                    case DataType.pitInfo: break;
                                    case DataType.forceFeedback: break;

                                    default:
                                        TreatmentGreen = true; // sortie de la boucle imposée
                                        break;
                                } 

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        rTB_Terminal.AppendText("Error: " + ex + "\n");
                        Console.WriteLine("Error:" + ex); // A mettre dans fenêtre
                    }
                    finally
                    {
                        // Fermez Stream, libérez des ressources
                        writingStream.Close();
                        rTB_Terminal.AppendText("Fin de lecture RAW dans fichier " + NameFile + "\n");
                    }
                }
            }

            fBDselect.SelectedPath = dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata;

            if (fBDselect.ShowDialog() == DialogResult.OK)
            {
                DirectoryInfo di = new DirectoryInfo(fBDselect.SelectedPath);
                FileInfo[] files = di.GetFiles("*.*");

                rTB_Terminal.AppendText("\n Liste et data des fichiers\n");
                foreach (FileInfo file in files)
                {
                    rTB_Terminal.AppendText("\n ***************************************************** \n");
                    rTB_Terminal.AppendText(file.Name + "\n");
                    _Openfile(file.FullName);
                    
                }
            }

        }

        private void analyseFichierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FilesAnalyze();
        }

        private void bestTimeLapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BestLaps formBestLaps = new BestLaps();

            if (formBestLaps.Visible)
            {
                formBestLaps.Focus();
                formBestLaps.SendData(dataUDP, dataSetting);
            }
            else
            {
                formBestLaps.Show();
                formBestLaps.SendData(dataUDP, dataSetting);
            }
        }

        private void tPinfo_Paint(object sender, PaintEventArgs e)
        {
            switch (dataSetting.Simulation)
            {
                case 0:         // F 202X
                                //call_telemetrieF1202X();
                    break;
                case 1:         // Project Car 2
                    break;
                case 2:         // RFactor 2
                    ReceiveRF2data(sender, e);
                    break;
                case 3:         // Assetto Corsa Comp
                    break;
                case 4:         // Assetto Corsa 
                    break;
                case 5:         // Forza
                    break;
            }
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerRF2ReadisWorkerRunning)
            {
                backgroundWorkerRF2ReadisWorkerRunning = false;
                backgroundWorkerRF2Read.CancelAsync();
            }

            if (conversionWorkerRunning)
            {
                conversionWorkerRunning = false;
                backgroundWorkerRF2Read.CancelAsync();
            }
        }


        /// <summary>
        ///  Conversion Fichier RF2 en light certainement à abandonner
        /// </summary>
        public class ConversionArguments
        {
            public string NameFileInput { get; set; }
            public string NameFileOutput { get; set; }
        }
        private void conversionStripMenuItem1_Click(object sender, EventArgs e)
        {
            string NameFileInput = "";
            string NameFileOutput = "";
            Boolean EcritureOK = true;

            InitializeBackgroundWorker();

            oFD_RAWdata.InitialDirectory = dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata;

            if (oFD_RAWdata.ShowDialog() == DialogResult.OK)
            {
                NameFileInput = oFD_RAWdata.FileName;// + ".bin";
                rTB_Terminal.AppendText("\nConverstion du fichier " + NameFileInput + "\n");

                sFD_RAWdata.InitialDirectory = dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata;
                if (sFD_RAWdata.ShowDialog() == DialogResult.OK)
                {
                    NameFileOutput = sFD_RAWdata.FileName;
                    if (System.IO.File.Exists(NameFileOutput)) /// 
                    {
                        string fmessage = "ATTENTION fichier existant";
                        string fcaption = "voulez-vous l'écrasser?";
                        MessageBoxButtons fbuttons = MessageBoxButtons.OKCancel;
                        DialogResult fresult;

                        // Displays the MessageBox.
                        fresult = MessageBox.Show(fmessage, fcaption, fbuttons);
                        if (fresult != DialogResult.OK) EcritureOK = false;
                    }
                    if (EcritureOK == true)
                    {
                        rTB_Terminal.AppendText("\n Conversion des donnée du fichier " + NameFileInput + " lancée \n");
                        rTB_Terminal.AppendText("\n vers le fichier " + NameFileOutput + " lancée \n");
                        //Stream writingStream = new FileStream(NameFile, FileMode.Create);

                        try
                        {
                            rTB_Terminal.AppendText("\nTraitement:\n");

                            ConversionArguments arguments = new ConversionArguments
                            {
                                NameFileInput = NameFileInput,
                                NameFileOutput = NameFileOutput
                            };


                            //ConversionRF2(NameFileInput, NameFileOutput);
                            conversionWorker.RunWorkerAsync(arguments);

                        }
                        catch (Exception ex)
                        {
                            rTB_Terminal.AppendText("\nError: " + ex + "\n");
                            Console.WriteLine("Error:" + ex); // A mettre dans fenêtre
                        }
                        finally
                        {
                            rTB_Terminal.AppendText("\nFin de la conversion du fichier " + NameFileInput + "\n");
                        }
                    }
                    else
                    {
                        rTB_Terminal.AppendText("\nEcritureOK == false fichier non converti");
                    }
                }
            }
            else
            {
                string message = "Pas de Conversion de données";
                string caption = "CANCEL";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result;

                // Displays the MessageBox.
                result = MessageBox.Show(message, caption, buttons);

            }
        }

        private void UpdateRateLabel(int rate)
        {
            if (lbRate.InvokeRequired)
            {
                // Invoke the method on the UI thread
                lbRate.Invoke((MethodInvoker)delegate
                {
                    lbRate.Text = $"{rate}%";
                });
            }
            else
            {
                lbRate.Text = $"{rate}%";
            }
        }

        private BackgroundWorker conversionWorker;

        private void InitializeBackgroundWorker()
        {
            conversionWorker = new BackgroundWorker();
            conversionWorker.WorkerReportsProgress = true;
            conversionWorker.DoWork += ConversionWorker_DoWork;
            conversionWorker.ProgressChanged += ConversionWorker_ProgressChanged;
            conversionWorker.RunWorkerCompleted += ConversionWorker_RunWorkerCompleted;
        }

        private void ConversionWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ConversionArguments arguments = e.Argument as ConversionArguments;

            string nameFileInput = arguments.NameFileInput;
            string nameFileOutput = arguments.NameFileOutput;

            conversionWorkerRunning = true;

            using (FileStream streamoutput = new FileStream(nameFileOutput, FileMode.Create))
            using (GZipStream compressedStream = new GZipStream(streamoutput, CompressionMode.Compress))
            using (FileStream streaminput = new FileStream(nameFileInput, FileMode.Open))
            {
                BinaryReader binaryReader = new BinaryReader(streaminput);

                while (streaminput.Position < streaminput.Length)
                {
                    Header header = dataUDP.dataRF2.ReadHeader(streaminput);
                    if (header.DataType == DataType.telemetry)
                    {
                        dataUDP.dataRF2.SaveLightTelemetry(streaminput, compressedStream, header, 1);
                    }

                    int rate = (int)((100 * streaminput.Position) / streaminput.Length);
                    conversionWorker.ReportProgress(rate);
                }
            }
        }

        private void ConversionWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UpdateRateLabel(e.ProgressPercentage);
        }

        private void ConversionWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            conversionWorkerRunning = false;

            string caption = "Conversion des données";
            string message = "Action terminée";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            DialogResult result;

            // Displays the MessageBox.
            result = MessageBox.Show(message, caption, buttons);
        }

        /// <summary>
        /// ///////////// FIN Conversion
        /// </summary>


        /// Partie DEBUG analyse

        private BackgroundWorker DebugWorker;

        private void InitializeBackgroundDebugWorker()
        {
            DebugWorker = new BackgroundWorker();
            DebugWorker.WorkerReportsProgress = true;
            DebugWorker.DoWork += DebugWorker_DoWork;
            DebugWorker.ProgressChanged += DebugWorker_ProgressChanged;
            DebugWorker.RunWorkerCompleted += DebugWorker_RunWorkerCompleted;
        }

        private void DebugWorkerold_DoWork(object sender, DoWorkEventArgs e)
        {
            long i = 0;
            int Indextableau = 0;
            int IndexTelemetry = 0;
            int IndexScoring = 0;
            int NumCar = 5;


            ConversionArguments arguments = e.Argument as ConversionArguments;

            string nameFileInput = arguments.NameFileInput;
            string nameFileOutput = arguments.NameFileOutput;

            conversionWorkerRunning = true;

            //using (FileStream streamoutput = new FileStream(nameFileOutput, FileMode.Create))
            using (StreamWriter streamoutput = new StreamWriter(nameFileOutput))
            using (FileStream streaminput = new FileStream(nameFileInput, FileMode.Open))
            {
                BinaryReader binaryReader = new BinaryReader(streaminput);
                //streamoutput.WriteLine("  i  " + ";Type" + ";" + "mVersionUpdateBegin" + ";" + "mVersionUpdateEnd" + ";" + "Same or not");
                streamoutput.WriteLine("  i  ;type;Lap;Dist");

                while ((streaminput.Position < streaminput.Length) && (backgroundWorkerRF2Read.CancellationPending!=true))
                {
                    Header header = dataUDP.dataRF2.ReadHeader(streaminput);

                    dataUDP.dataRF2.InitPacket(streaminput, header);

                    if ((dataUDP.dataRF2.scoring.Count > IndexScoring) && (header.DataType == DataType.scoring))
                    {
                        //streamoutput.WriteLine(i + ";speed" + ";" + dataUDP.Get_m_speed(Indextableau, NumCar));

                        streamoutput.WriteLine(i + ";Get_m_lapDistance;-;" +
                                                dataUDP.Get_m_lapDistance(IndexScoring, NumCar)); 
                        Indextableau++;
                        IndexScoring++;
                    }
                    if ((dataUDP.dataRF2.telemetryLight.Count > IndexTelemetry) && (IndexTelemetry > 0) && (header.DataType == DataType.telemetry))
                    {
                        //streamoutput.WriteLine(i + ";speed" + ";" + dataUDP.Get_m_speed(Indextableau, NumCar));

                        streamoutput.WriteLine(i + ";Get_m_currentLapNum;" +
                                                dataUDP.Get_m_currentLapNum(IndexTelemetry - 1, NumCar)+";-");
                        Indextableau++; 
                        IndexTelemetry++;
                    }
                    if ((IndexTelemetry == 0) && (header.DataType == DataType.telemetry))
                        IndexTelemetry++;


                    int rate = (int)((100 * streaminput.Position) / streaminput.Length);
                    DebugWorker.ReportProgress(rate);
                    i++;
                }
            }
        }
        private void DebugWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int i = 0;

            ConversionArguments arguments = e.Argument as ConversionArguments;

            string nameFileInput = arguments.NameFileInput;
            string nameFileOutput = arguments.NameFileOutput;
            string Chaine = "";

            conversionWorkerRunning = true;

            FlagDebug = true;
            using (FileStream stream = new FileStream(nameFileInput, FileMode.Open))
            {
                int rate;
                stopWatch.Restart();
                rF2Telemetry _telemetry;

                while (stream.Position < stream.Length)
                {
                    //TelemetryData telemetryData = new TelemetryData();
                    Header header = dataUDP.dataRF2.ReadHeader(stream);

                    byte[] buffer = new byte[header.DataSize];

                    //stream.Read(buffer, 0, (int)header.DataSize); // 1 tick à 117 797 tick (12ms)
                                                                  //< rF2Telemetry > data = dataUDP.dataRF2.ByteArrayToStructure<rF2Telemetry>(buffer);
                    if (dataUDP.dataRF2.checksize(header))
                    {
                        switch (header.DataType)
                        {
                            case DataType.telemetry:
                                _telemetry = dataUDP.dataRF2.ReadWithHeader<rF2Telemetry>(stream, header); // 7 086 tick à 8985706 tick (898 ms ~1s)
                                break;
                            case DataType.scoring:
                                dataUDP.dataRF2.ReadWithHeader<rF2Scoring>(stream, header); //2 276 tick à 237991 tick (24ms)
                                break;
                            case DataType.rules:
                                dataUDP.dataRF2.ReadWithHeader<rF2Rules>(stream, header); // 734 tick à 227922 tick (23ms)
                                break;
                            case DataType.forceFeedback:
                                dataUDP.dataRF2.ReadWithHeader<rF2ForceFeedback>(stream, header);
                                break;
                            case DataType.graphics:
                                dataUDP.dataRF2.ReadWithHeader<rF2Graphics>(stream, header);
                                break;
                            case DataType.pitInfo:
                                dataUDP.dataRF2.ReadWithHeader<rF2PitInfo>(stream, header);
                                break;
                            case DataType.weather:
                                dataUDP.dataRF2.ReadWithHeader<rF2Weather>(stream, header);
                                break;
                            case DataType.extended:
                                dataUDP.dataRF2.ReadWithHeader<rF2Extended>(stream, header); // 128 tick à 151483 tick (15ms)
                                break;
                            default:
                                //MessageBox.Show($"Type de données non pris en charge : {header.DataType}");
                                break;
                                //throw new NotSupportedException($"Type de données non pris en charge : {header.DataType}");
                        }
                        // BeginEnd(header);
                    }


                    //ReadWithHeader<rF2Telemetry>(stream, header);


                    //rate = (int)((100 * stream.Position) / stream.Length);
                    //DebugWorker.ReportProgress(rate);//, progressInfo);
                    //int rate = (int)((100 * i) / dataUDP.ListTelemetryData.Count());
                    //DebugWorker.ReportProgress(rate);
                }
                // Utilisez ElapsedTicks pour obtenir le temps en ticks
                long ticks2 = stopWatch.ElapsedTicks;

                // Convertissez les ticks en millisecondes en utilisant la fréquence du timer
                double time2 = (double)ticks2 / Stopwatch.Frequency * 1000.0;

                //rTB_Terminal.AppendText("Durée :"+time2.ToString());

            }
        }

        private void DebugWorker_DoWork2(object sender, DoWorkEventArgs e)
        {
            // traitement des données FormTelemetry

            int i = 0;

            ConversionArguments arguments = e.Argument as ConversionArguments;

            string nameFileInput = arguments.NameFileInput;
            string nameFileOutput = arguments.NameFileOutput;
            string Chaine="";

            conversionWorkerRunning = true;

            FlagDebug = true;

            ReadFileDataRF2(nameFileInput);

            FlagDebug = false;

            using (StreamWriter streamoutput = new StreamWriter(nameFileOutput))
            {
                int NumCar = dataUDP.Get_playerCarIndex(0);


                Chaine = Chaine + "Lap;NumCar;m_clutch;m_engineRPM;m_gear;m_yaw;m_tyresPressure;m_suspensionVelocity";
                streamoutput.WriteLine(Chaine);

                int Lap;

                for (i=0; i<dataUDP.ListTelemetryData.Count();i++)
                {
                    if (backgroundWorkerRF2Read.CancellationPending == true)
                        return;
                    Lap = i;
                    Chaine = i+";"+NumCar + ";" +
                                dataUDP.ListTelemetryData[i].mVehicles[NumCar].m_brakesTemperature[0] + ";" +
                                dataUDP.ListTelemetryData[i].mVehicles[NumCar].m_brakesTemperature[1] + ";" +
                                dataUDP.ListTelemetryData[i].mVehicles[NumCar].m_brakesTemperature[2] + ";" +
                                dataUDP.ListTelemetryData[i].mVehicles[NumCar].m_brakesTemperature[3] + ";" +
                                dataUDP.ListTelemetryData[i].mVehicles[NumCar].m_tyresPressure[0] + ";" +
                                dataUDP.ListTelemetryData[i].mVehicles[NumCar].m_suspensionVelocity[0];

                    streamoutput.WriteLine(Chaine);

                    int rate = (int)((100 * i) / dataUDP.ListTelemetryData.Count());
                    DebugWorker.ReportProgress(rate);
                }
            }
            
        }
        private void DebugWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UpdateRateLabel(e.ProgressPercentage);
        }

        private void DebugWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            conversionWorkerRunning = false;

            string caption = "Analyse DEBUG des données";
            string message = "Action terminée";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            DialogResult result;

            // Displays the MessageBox.
            result = MessageBox.Show(message, caption, buttons);
        }


        private void btnDEBUG_Click(object sender, EventArgs e)
        {
            string NameFileInput = "";
            string NameFileXLSOutput = "";
            Boolean EcritureOK = true;

            InitializeBackgroundDebugWorker();

            oFD_RAWdata.InitialDirectory = dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata;

            if (oFD_RAWdata.ShowDialog() == DialogResult.OK)
            {
                NameFileInput = oFD_RAWdata.FileName;// + ".bin";
                rTB_Terminal.AppendText("\nAnalyse DEBUG du fichier " + NameFileInput + "\n");

                sFD_RAWdata.InitialDirectory = dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata;
                if (sFD_RAWdata.ShowDialog() == DialogResult.OK)
                {
                    NameFileXLSOutput = sFD_RAWdata.FileName;
                    if (System.IO.File.Exists(NameFileXLSOutput)) /// 
                    {
                        string fmessage = "ATTENTION fichier existant";
                        string fcaption = "voulez-vous l'écrasser?";
                        MessageBoxButtons fbuttons = MessageBoxButtons.OKCancel;
                        DialogResult fresult;

                        // Displays the MessageBox.
                        fresult = MessageBox.Show(fmessage, fcaption, fbuttons);
                        if (fresult != DialogResult.OK) EcritureOK = false;
                    }
                    if (EcritureOK == true)
                    {
                        rTB_Terminal.AppendText("\n Analyse DEBUG des donnée du fichier " + NameFileInput + " lancée \n");
                        rTB_Terminal.AppendText("\n vers le fichier " + NameFileXLSOutput + " lancée \n");
                        //Stream writingStream = new FileStream(NameFile, FileMode.Create);

                        try
                        {
                            rTB_Terminal.AppendText("\nTraitement d''analyse:\n");

                            ConversionArguments arguments = new ConversionArguments
                            {
                                NameFileInput = NameFileInput,
                                NameFileOutput = NameFileXLSOutput
                            };


                            //ConversionRF2(NameFileInput, NameFileOutput);
                            DebugWorker.RunWorkerAsync(arguments);

                        }
                        catch (Exception ex)
                        {
                            rTB_Terminal.AppendText("\nError: " + ex + "\n");
                            Console.WriteLine("Error:" + ex); // A mettre dans fenêtre
                        }
                        finally
                        {
                            rTB_Terminal.AppendText("\nAnalyse DEBUG  du fichier " + NameFileInput + "\n");
                        }
                    }
                    else
                    {
                        rTB_Terminal.AppendText("\nEcritureOK == false fichier non généré pour analyse");
                    }
                }
            }
            else
            {
                string message = "Pas de données à analyser";
                string caption = "CANCEL";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result;

                // Displays the MessageBox.
                result = MessageBox.Show(message, caption, buttons);


            }
        }

        private void InitializeJoystick()
        {
            DirectInput directInput = new DirectInput();

            // Recherche du premier joystick disponible
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
            {
                joystickGuid = deviceInstance.InstanceGuid;
                break;
            }

            if (joystickGuid != Guid.Empty)
            {
                // Initialisation du joystick
                joystick = new Joystick(directInput, joystickGuid);
                joystick.Properties.BufferSize = 128;
                joystick.Acquire();
            }

            // Initialisation du minuteur
            joystickTimer = new Timer();
            joystickTimer.Interval = 100; // Intervalle en millisecondes (ajustez selon vos besoins)
            joystickTimer.Tick += new EventHandler(joystickTimer_Tick);
            joystickTimer.Start();

            SeriesChartType ChartType = SeriesChartType.FastLine;

            // Graphique Frein
            chartFrein.ChartAreas.Clear();
            chartFrein.Show();
            chartFrein.Series.Clear();
            chartFrein.ChartAreas.Clear();
            System.Windows.Forms.DataVisualization.Charting.ChartArea caF = chartFrein.ChartAreas.Add("Brake");
            System.Windows.Forms.DataVisualization.Charting.Series sF = null;

            caF.AxisX.ScaleView.Zoomable = false;
            caF.CursorX.AutoScroll = true;
            caF.CursorX.IsUserSelectionEnabled = true;

            caF.AxisY.ScaleView.Zoomable = false;
            caF.CursorY.AutoScroll = true;
            caF.CursorY.IsUserSelectionEnabled = true;
            caF.AxisY.Minimum = 0;
            caF.AxisY.Maximum = 1;

            sF = chartFrein.Series.Add("Brake");
            sF.ChartType = ChartType;
            sF.BorderWidth = 1;
            sF.BorderDashStyle = ChartDashStyle.Solid;
            sF.ChartArea = caF.Name;
            //s.ToolTip = "Tour " + (NbSerie + 1) + " - Pilote : " + dataUDP.Get_m_s_name(0, NumCar);
            sF.Color = Color.Red;

            // Graphique Accelerateur
            chartAcceleration.ChartAreas.Clear();
            chartAcceleration.Show();
            chartAcceleration.Series.Clear();
            chartAcceleration.ChartAreas.Clear();
            System.Windows.Forms.DataVisualization.Charting.ChartArea caA = chartAcceleration.ChartAreas.Add("Acceleration");
            System.Windows.Forms.DataVisualization.Charting.Series sA = null;

            caA.AxisX.ScaleView.Zoomable = false;
            caA.CursorX.AutoScroll = true;
            caA.CursorX.IsUserSelectionEnabled = true;

            caA.AxisY.ScaleView.Zoomable = false;
            caA.CursorY.AutoScroll = true;
            caA.CursorY.IsUserSelectionEnabled = true;
            caA.AxisY.Minimum = 0;
            caA.AxisY.Maximum = 1;

            sA = chartAcceleration.Series.Add("Acceleration");
            sA.ChartType = ChartType;
            sA.BorderWidth = 1;
            sA.BorderDashStyle = ChartDashStyle.Solid;
            sA.ChartArea = caA.Name;
            //s.ToolTip = "Tour " + (NbSerie + 1) + " - Pilote : " + dataUDP.Get_m_s_name(0, NumCar);
            sA.Color = Color.Red;

            // Graphique Wheel
            chartWheel.ChartAreas.Clear();
            chartWheel.Show();
            chartWheel.Series.Clear();
            chartWheel.ChartAreas.Clear();
            System.Windows.Forms.DataVisualization.Charting.ChartArea caW = chartWheel.ChartAreas.Add("Wheel");
            System.Windows.Forms.DataVisualization.Charting.Series sW = null;

            caW.AxisX.ScaleView.Zoomable = false;
            caW.CursorX.AutoScroll = true;
            caW.CursorX.IsUserSelectionEnabled = true;

            caW.AxisY.ScaleView.Zoomable = false;
            caW.CursorY.AutoScroll = true;
            caW.CursorY.IsUserSelectionEnabled = true;
            caW.AxisY.Minimum = 0;
            caW.AxisY.Maximum = 2;

            sW = chartWheel.Series.Add("Wheel");
            sW.ChartType = ChartType;
            sW.BorderWidth = 1;
            sW.BorderDashStyle = ChartDashStyle.Solid;
            sW.ChartArea = caW.Name;
            //s.ToolTip = "Tour " + (NbSerie + 1) + " - Pilote : " + dataUDP.Get_m_s_name(0, NumCar);
            sW.Color = Color.Red;


            dataGV_BoutonsStatus.Rows.Clear();
            dataGV_BoutonsStatus.Columns.Clear();

            if (joystickGuid != Guid.Empty)
            {
                for (int i = 0; i < joystick.Capabilities.ButtonCount; i++)
                {
                    dataGV_BoutonsStatus.Columns.Add($"Btn {i + 1}", $"Btn {i + 1}");
                }                
                
                // Alignement au centre pour les titres de colonnes
                foreach (DataGridViewColumn col in dataGV_BoutonsStatus.Columns)
                {
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.Width = 70;
                }
                // Alignement au centre pour les cellules de données
                foreach (DataGridViewRow row in dataGV_BoutonsStatus.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }
                }
            }


            /*
            dataGV_BoutonsStatus.Columns.Add("X", "X");
            dataGV_BoutonsStatus.Columns.Add("Y", "Y");
            dataGV_BoutonsStatus.Columns.Add("Z", "Z");
            */
        }

        private int axisIndex = 0;
        private const int MaxDataPoints = 100;
        private Queue<double> brakeValues = new Queue<double>();
        private Queue<double> acceleratorValues = new Queue<double>();
        private Queue<double> wheelValues = new Queue<double>();
        private void joystickTimer_Tick(object sender, EventArgs e)
        {
            System.Windows.Forms.DataVisualization.Charting.Series sF,sA, sW;
            sF = chartFrein.Series[0];
            sA = chartAcceleration.Series[0];
            sW = chartWheel.Series[0];

            if (joystick != null)
            {
                JoystickState state = joystick.GetCurrentState();

                //tBinfoSimTool.AppendText(GetValues(state).ToString());
                double constante = 32767.0; // 65535.0 
                // Utilisez les noms des axes pour obtenir les valeurs
                double brakeValue = (double)(1.0-state.RotationZ / 65535.0);
                double acceleratorValue = (double)(1.0-state.Y/ 65535.0);
                double WheelValue = (double)(state.X / constante);
                double clutchValue = (double)(state.X / constante);


                label16.Text = brakeValue.ToString();
                label18.Text = acceleratorValue.ToString();
                label20.Text = clutchValue.ToString();
                label20.Text = WheelValue.ToString();

                sF.Points.AddXY(axisIndex++, brakeValue);
                sA.Points.AddXY(axisIndex++, acceleratorValue);
                sW.Points.AddXY(axisIndex++, WheelValue);

                // Utilisez ces valeurs pour mettre à jour votre interface utilisateur
                brakeValues.Enqueue(brakeValue);
                acceleratorValues.Enqueue(acceleratorValue);
                wheelValues.Enqueue(WheelValue);

                // Limitez le nombre de points affichés
                while (brakeValues.Count > MaxDataPoints)
                {
                    brakeValues.Dequeue();
                    acceleratorValues.Dequeue();
                    wheelValues.Dequeue();
                }

                // Mise à jour du graphique avec les 100 derniers points
                sF.Points.DataBindY(brakeValues);
                sA.Points.DataBindY(acceleratorValues);
                sW.Points.DataBindY(wheelValues);


                // Récupérer l'état des boutons
                bool[] buttonStates = state.Buttons;

                // Ajouter les valeurs des boutons à la ligne du DataGridView
                string[] rowData = buttonStates.Select(b => b.ToString()).ToArray();
                //dataGV_BoutonsStatus.Rows.Add(rowData);
                // Mettre à jour les cellules de la ligne existante avec les nouveaux états des boutons
                for (int i = 0; i < buttonStates.Length-1; i++)
                {
                    if (i < dataGV_BoutonsStatus.Rows[0].Cells.Count)
                    {
                        dataGV_BoutonsStatus.Rows[0].Cells[i].Value = buttonStates[i].ToString();
                    }
                }

            }
        }

        private void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControlMain.TabPages[tabControlMain.SelectedIndex].Text)
            {
                case "SimTool":
                    InitializeJoystick();
                    break;
                default:
                    if (joystick!=null) 
                    { 
                        joystick.Unacquire(); 
                    }
                    if (joystickTimer != null)
                    {
                        joystickTimer.Stop();
                        joystickTimer.Dispose();  // Libérer les ressources du timer
                    }
                    break;
            }
        }
    }

}


/*
 * 
             dataGV_BoutonsStatus.Columns.Add("RotationX", "RotationX");
            dataGV_BoutonsStatus.Columns.Add("RotationY", "RotationY");
            dataGV_BoutonsStatus.Columns.Add("RotationZ", "RotationZ");

            dataGV_BoutonsStatus.Columns.Add("VelocityX", "VelocityX");
            dataGV_BoutonsStatus.Columns.Add("VelocityY", "VelocityY");
            dataGV_BoutonsStatus.Columns.Add("VelocityZ", "VelocityZ");

            dataGV_BoutonsStatus.Columns.Add("AngularVelocityX", "AngularVelocityX");
            dataGV_BoutonsStatus.Columns.Add("AngularVelocityY", "AngularVelocityY");
            dataGV_BoutonsStatus.Columns.Add("AngularVelocityZ", "AngularVelocityZ");

            dataGV_BoutonsStatus.Columns.Add("AccelerationX", "AccelerationX");
            dataGV_BoutonsStatus.Columns.Add("AccelerationY", "AccelerationY");
            dataGV_BoutonsStatus.Columns.Add("AccelerationZ", "AccelerationZ");

            dataGV_BoutonsStatus.Columns.Add("AngularAccelerationX", "AngularAccelerationX");
            dataGV_BoutonsStatus.Columns.Add("AngularAccelerationY", "AngularAccelerationY");
            dataGV_BoutonsStatus.Columns.Add("AngularAccelerationZ", "AngularAccelerationZ");

            dataGV_BoutonsStatus.Columns.Add("ForceX", "ForceX");
            dataGV_BoutonsStatus.Columns.Add("ForceY", "ForceY");
            dataGV_BoutonsStatus.Columns.Add("ForceZ", "ForceZ");

            dataGV_BoutonsStatus.Columns.Add("TorqueX", "TorqueX");
            dataGV_BoutonsStatus.Columns.Add("TorqueY", "TorqueY");
            dataGV_BoutonsStatus.Columns.Add("TorqueZ", "TorqueZ");

 * 
 */

/* Debug Analyse des tours par driver
 * 

            int i = 0;

            ConversionArguments arguments = e.Argument as ConversionArguments;

            string nameFileInput = arguments.NameFileInput;
            string nameFileOutput = arguments.NameFileOutput;
            string Chaine="";

            conversionWorkerRunning = true;

            FlagDebug = true;

            ReadFileDataRF2(nameFileInput);

            FlagDebug = false;

            using (StreamWriter streamoutput = new StreamWriter(nameFileOutput))
            {
                int NumMaxCar = 0;
                for (i = 0; i < dataUDP.ListTelemetryData.Count(); i++)
                    if (NumMaxCar < dataUDP.Get_m_numActiveCars(i))
                        NumMaxCar = dataUDP.Get_m_numActiveCars(i);
                for (i = 0; i < NumMaxCar;i++)
                    Chaine = Chaine + ";[Tour;Car]" + i;
                streamoutput.WriteLine("  i  ;NbCar"+Chaine);
                for (i=0; i<dataUDP.ListTelemetryData.Count();i++)
                {
                    if (backgroundWorkerRF2Read.CancellationPending == true)
                        return;

                    Chaine = i + ";"+dataUDP.Get_m_numActiveCars(i)+";";
                    for (int j = 0; j < dataUDP.Get_m_numActiveCars(i); j++)
                        Chaine = Chaine + ";" + dataUDP.Get_m_currentLapNum(i, j)+";"+  dataUDP.Get_m_driverId(i, j)+"-"+dataUDP.Get_m_s_name(i,j);

                    streamoutput.WriteLine(Chaine);

                    int rate = (int)((100 * i) / dataUDP.ListTelemetryData.Count());
                    DebugWorker.ReportProgress(rate);
                }
            }


 * 
 */


/* Debug analyse code
 *
  
            rF2Telemetry dataT;
            rF2Scoring dataS;
            rF2Rules dataR;
            rF2ForceFeedback dataF;
            rF2Graphics dataG;
            rF2PitInfo dataP;
            rF2Weather dataW;
            rF2Extended dataE;

                    switch (header.DataType)
                    {
                        case DataType.telemetry: // vérifier si données identique pour même point
                            dataT = dataUDP.dataRF2.ReadWithHeader<rF2Telemetry>(streaminput, header);
                            streamoutput.WriteLine(i+";Telemetry"+";"+dataT.mVersionUpdateBegin+";"+dataT.mVersionUpdateEnd );
                            break;
                        case DataType.scoring:
                            dataS = dataUDP.dataRF2.ReadWithHeader<rF2Scoring>(streaminput, header);
                            streamoutput.WriteLine(i + ";Scoring"+ ";" + dataS.mVersionUpdateBegin + ";" + dataS.mVersionUpdateEnd);
                            break;
                        case DataType.rules:
                            dataR = dataUDP.dataRF2.ReadWithHeader<rF2Rules>(streaminput, header);
                            streamoutput.WriteLine(i + ";Rules" + ";" + dataR.mVersionUpdateBegin + ";" + dataR.mVersionUpdateEnd);
                            break;
                        case DataType.forceFeedback:
                            dataF = dataUDP.dataRF2.ReadWithHeader<rF2ForceFeedback>(streaminput, header);
                            streamoutput.WriteLine(i + ";FB" + ";" + dataF.mVersionUpdateBegin + ";" + dataF.mVersionUpdateEnd);
                            break;
                        case DataType.graphics:
                            dataG = dataUDP.dataRF2.ReadWithHeader<rF2Graphics>(streaminput, header);
                            streamoutput.WriteLine(i + ";Graphic" + ";" + dataG.mVersionUpdateBegin + ";" + dataG.mVersionUpdateEnd);
                            break;
                        case DataType.pitInfo:
                            dataP = dataUDP.dataRF2.ReadWithHeader<rF2PitInfo>(streaminput, header);
                            streamoutput.WriteLine(i + ";PitInfo" + ";" + dataP.mVersionUpdateBegin + ";" + dataP.mVersionUpdateEnd);
                            break;
                        case DataType.weather:
                            dataW = dataUDP.dataRF2.ReadWithHeader<rF2Weather>(streaminput, header);
                            streamoutput.WriteLine(i + ";Weather" + ";" + dataW.mVersionUpdateBegin + ";" + dataW.mVersionUpdateEnd);
                            break;
                        case DataType.extended:
                            dataE = dataUDP.dataRF2.ReadWithHeader<rF2Extended>(streaminput, header);
                            streamoutput.WriteLine(i + ";Extended" + ";" + dataE.mVersionUpdateBegin + ";" + dataE.mVersionUpdateEnd);
                            break;
                    }

 *
 */

/* Debug analyse 
 * 
  
            // traitement des données FormTelemetry

            int i = 0;

            ConversionArguments arguments = e.Argument as ConversionArguments;

            string nameFileInput = arguments.NameFileInput;
            string nameFileOutput = arguments.NameFileOutput;
            string Chaine="";

            conversionWorkerRunning = true;

            FlagDebug = true;

            ReadFileDataRF2(nameFileInput);

            FlagDebug = false;

            using (StreamWriter streamoutput = new StreamWriter(nameFileOutput))
            {
                int NumMaxCar = 0;
                for (i = 0; i < dataUDP.ListTelemetryData.Count(); i++)
                    if (NumMaxCar < dataUDP.Get_m_numActiveCars(i))
                        NumMaxCar = dataUDP.Get_m_numActiveCars(i);
                for (i = 0; i < NumMaxCar;i++)
                    Chaine = Chaine + ";[Tour;Car]" + i;
                streamoutput.WriteLine("  i  ;NbCar"+Chaine);
                for (i=0; i<dataUDP.ListTelemetryData.Count();i++)
                {
                    if (backgroundWorkerRF2Read.CancellationPending == true)
                        return;

                    Chaine = i + ";"+dataUDP.Get_m_numActiveCars(i)+";";
                    for (int j = 0; j < dataUDP.Get_m_numActiveCars(i); j++)
                        Chaine = Chaine + ";" + dataUDP.Get_m_currentLapNum(i, j)+";"+  dataUDP.Get_m_driverId(i, j)+"-"+dataUDP.Get_m_s_name(i,j);

                    streamoutput.WriteLine(Chaine);

                    int rate = (int)((100 * i) / dataUDP.ListTelemetryData.Count());
                    DebugWorker.ReportProgress(rate);
                }
            }
 

 *
 */