//using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Markup;
//using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using static f1_eTelemetry.UDP_Data;
using Excel = Microsoft.Office.Interop.Excel; //https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/interop/how-to-access-office-interop-objects
// https://electronique71.com/export-datagridview-vers-excel-avec-enregistrement-du-fichier-excel/

//https://www.youtube.com/watch?v=H0Kpx8Wafnw

//https://stackoverflow.com/questions/40244699/how-to-plot-a-3d-graph-to-represent-an-object-in-space


namespace f1_eTelemetry
{
    public partial class FormTelemetry : Form
    {

        public struct Flashback
        {
            public UInt32 flashbackFrameIdentifier;  // Frame identifier flashed back to
            public float flashbackSessionTime;       // Session time flashed back to
            public float m_sessionTime;
            public int lap;
        }

        bool legendshow = false;
        bool barreverticale = true; // plantage exception si actif à revoir le code
        //static UDP_f12021 dataf1_2021 = new UDP_f12021();
        UDP_Data dataUDP = new UDP_Data();
        Setting dataSetting = new Setting();

        public List<Color> LocalTrackColor = new List<Color>(); // couleurs utilisées dans les graphs pour les tours
        public List<Color> LocalTrackColorVS = new List<Color>(); // couleurs utilisées dans les graphs pour les tours
        public List<Flashback> ListFlashback = new List<Flashback>();
        
        int NumCar = 0;
        int NumCar_vs = 0;

        // gestion de la souris dans les graphiques
        string PreviousselectedSeries = "";     // store a class-scoped reference
        int PreviousselectedSeriesIndex = -1;     // store a class-scoped reference
        Color previouscolor = Color.Beige;
        int PreviousBorderWidth = 1;
        int AnalyseBorderWidth = 8;
        Point? prevPosition = null;
        ToolTip tooltip = new ToolTip();
        string tooltipX = "Odm=";
        string tooltipY = "M, Y=";

        double Xc,Yc, X1, Y1, X2, Y2;
        double Global_xval = 0;
        double Global_yval = 0;


        VerticalLineAnnotation VL = null; 

        public FormTelemetry()
        {
            InitializeComponent();

            //tabControlTelemetry.SelectedTab = tabControlTelemetry.TabPages["tPdata"];
            // les données sont envoyées dans un second temps donc
            // il faut afficher un onglet sans le besoin d'afficher des informations
        }

        public FormTelemetry(UDP_f12021 _Data, Setting _dataSetting)
        {
            InitializeComponent();

            // tabControlTelemetry.SelectedTab = tabControlTelemetry.TabPages["tPdata"];
            // les données sont envoyées dans un second temps donc
            // il faut afficher un onglet sans le besoin d'afficher des informations

            SendData(_Data, _dataSetting);
        }
       
        public void SendData(UDP_f12021 _Data, Setting _dataSetting)
        {
            //dataf1_2021 = _Data;
            dataSetting = _dataSetting;

            if (_Data != null)
                NumCar = _Data.ListLapData[0].packetHeader.m_playerCarIndex;
            if (NumCar > 21) NumCar = 0; // Franck corr

            NumCar_vs = NumCar;

            Xc = 0;
            Yc = 0;
            X1 = 0;
            Y1 = 0;
            X2 = 0;
            Y2 = 0;

            // N'affiche pas l'onglet Graph car inclus dans Multi
            tabControlTelemetry.TabPages.Remove(tPgraph);
            tabControlTelemetry.TabPages.Remove(tP3D);

            for (int i = 0; i < _Data.ListData1.Count; i++)
                listViewData.Items.Add(_Data.ListData1[i]);
            listViewData.Items[0].Checked = true;

            InitLegend();

            InitListDrivers();

            tabControlTelemetry.SelectedTab = tabControlTelemetry.TabPages["tPMulti"];
            GestionTab();
            // les données sont envoyées dans un second temps donc
            // il faut afficher un onglet sans le besoin d'afficher des informations

        }

        public void SendDataold(UDP_Data _Data, Setting _dataSetting)
        {
            dataUDP = _Data;
            dataSetting = _dataSetting;

            // Repérer quel est le numéro de la voiture du joueur
            if (_Data != null) // Franck corr
                NumCar = _Data.Get_playerCarIndex(0);
            if (NumCar > dataUDP.Get_NumofPlayers()-1) NumCar = 0;

            NumCar_vs = NumCar;

            Xc = 0;
            Yc = 0;
            X1 = 0;
            Y1 = 0;
            X2 = 0;
            Y2 = 0;

            // N'affiche pas l'onglet Graph car inclus dans Multi
            tabControlTelemetry.TabPages.Remove(tPgraph);
            tabControlTelemetry.TabPages.Remove(tP3D);

            if (_Data.Get_ListDataTelemetryCount() == 0)
            {
                MessageBox.Show(" Pas de donnée dans la liste des items de télémtérie", "Erreur", MessageBoxButtons.OK);
                return;
            }

            for (int i = 0; i < dataUDP.Get_ListDataTelemetryCount(); i++)// _Data.RF2.ListDataRF2_1.Count; i++)
                listViewData.Items.Add(dataUDP.Get_ListDataTelemetry(i));//  _Data.dataRF2.ListDataRF2_2[i]);
            listViewData.Items[0].Checked = true;

            InitLegend();

            InitListDrivers();

            tabControlTelemetry.SelectedTab = tabControlTelemetry.TabPages["tPMulti"];
            GestionTab();
            // les données sont envoyées dans un second temps donc
            // il faut afficher un onglet sans le besoin d'afficher des informations

        }
        //public List<TelemetryData> ListTelemetryData = new List<TelemetryData>();
        public void SendData(UDP_Data _Data, Setting _dataSetting)
        {
            dataUDP = _Data;
            dataSetting = _dataSetting;

            // Repérer quel est le numéro de la voiture du joueur
            NumCar = dataUDP.Get_playerCarIndex(0);

            if (NumCar > dataUDP.Get_NumofPlayers() - 1) 
                NumCar = 0;      
            
            NumCar_vs = NumCar;

            Xc = 0;
            Yc = 0;
            X1 = 0;
            Y1 = 0;
            X2 = 0;
            Y2 = 0;

            // N'affiche pas l'onglet Graph car inclus dans Multi
            tabControlTelemetry.TabPages.Remove(tPgraph);
            tabControlTelemetry.TabPages.Remove(tP3D);

            if (dataUDP.Get_ListDataTelemetryCount() == 0) //dataUDP.ListTelemetryData.Count() == 0)
            {
                MessageBox.Show(" Pas de donnée dans la liste des items de télémtérie", "Erreur", MessageBoxButtons.OK);
                return;
            }

            // renseignement des data pouvant être affichés
            for (int i = 0; i < dataUDP.Get_ListDataTelemetryCount(); i++)// _Data.RF2.ListDataRF2_1.Count; i++)
                listViewData.Items.Add(dataUDP.Get_ListDataTelemetry(i));//  _Data.dataRF2.ListDataRF2_2[i]);
            listViewData.Items[0].Checked = true;

            InitLegend();

            InitListDrivers();

            tabControlTelemetry.SelectedTab = tabControlTelemetry.TabPages["tPMulti"];
            GestionTab();
            // les données sont envoyées dans un second temps donc
            // il faut afficher un onglet sans le besoin d'afficher des informations

        }

        private void AffichageGraphTelemetry()  {}

        private void AffichageMultiGraph_Add(ref Chart chartMulti, ref int indexSeries, string name)
        {
            // https://plasserre.developpez.com/cours/chart/

            int NbSerie = 0;
            int Indextableau = 0, Indextableau_vs = 0;

            double X, Y=0, X_vs, Y_vs=0;

            Chart chart = chartMulti;

            SeriesChartType ChartType = SeriesChartType.FastLine;

            ChartArea ca = chartMulti.ChartAreas.Add(name);
            Series s, s_vs = null;

            ca.Position = new ElementPosition(0, indexSeries * 23 + 5, 90, 25);
            chartMultiGraph.MouseWheel += chartMultiGraph_MouseWheel;

            ca.AxisX.ScaleView.Zoomable = true;
            ca.CursorX.AutoScroll = true;
            ca.CursorX.IsUserSelectionEnabled = true;

            ca.AxisY.ScaleView.Zoomable = true;
            ca.CursorY.AutoScroll = true;
            ca.CursorY.IsUserSelectionEnabled = true;

            //tracer un trait sur le graph sur et l'axe des Y toutes les 5 unités et un petit trait (TickMark) toutes les 2 unités :
            //Chart1.ChartAreas(0).AxisY.MajorGrid.Interval = 5
            //Chart1.ChartAreas(0).AxisY.MajorTickMark.Interval = 2

            void SerieAdd(int NbSerie)
            {
                s = chart.Series.Add("Tour  " + (NbSerie + 1).ToString() + "/" + name);
                s.ChartType = ChartType;
                s.BorderWidth = 1;
                s.BorderDashStyle = ChartDashStyle.Solid;
                s.ChartArea = ca.Name;
                s.ToolTip = "Tour " + (NbSerie + 1) + " - Pilote : " + dataUDP.Get_m_s_name(0,NumCar);
                if (cBbi.Checked)
                    s.Color = Color.Red;
                else
                {
                    if (NbSerie > (LocalTrackColor.Count - 1))
                        LocalTrackColor.Add(LocalTrackColor[0]);
                    s.Color = LocalTrackColor[NbSerie];
                }
                s.IsVisibleInLegend = legendshow;
                if (NbSerie < listViewLap.Items.Count)
                    if (listViewLap.Items[NbSerie].Checked == true)
                        s.Enabled = true;
                    else
                        s.Enabled = false;

                if (cBvs.Checked)
                    SerieAdd_vs(NbSerie);
            }

            void SerieAdd_vs(int NbSerie)
            {
                s_vs = chart.Series.Add("VS Tour  " + (NbSerie + 1).ToString() + "/" + name);
                s_vs.ChartType = ChartType;
                s_vs.BorderWidth = 2;
                s_vs.BorderDashStyle = ChartDashStyle.DashDotDot;
                s_vs.ChartArea = ca.Name;
                s_vs.ToolTip = "Tour " + (NbSerie + 1) + " - Pilote : " + dataUDP.Get_m_s_name(0,NumCar_vs);
                if (cBbi.Checked)
                    s_vs.Color = Color.Blue;
                else
                {
                    if (NbSerie > (LocalTrackColorVS.Count - 1))
                        LocalTrackColorVS.Add(LocalTrackColorVS[0]);

                    if ((LocalTrackColorVS[NbSerie].A + 40) < 255)
                        s_vs.Color = Color.FromArgb(LocalTrackColorVS[NbSerie].A + 40,
                                                        LocalTrackColorVS[NbSerie].R,
                                                        LocalTrackColorVS[NbSerie].G,
                                                        LocalTrackColorVS[NbSerie].B);
                    else
                        s_vs.Color = Color.FromArgb(LocalTrackColorVS[NbSerie].A - 40,
                                                        LocalTrackColorVS[NbSerie].R,
                                                        LocalTrackColorVS[NbSerie].G,
                                                        LocalTrackColorVS[NbSerie].B);
                }

                s_vs.IsVisibleInLegend = legendshow;
                if (NbSerie < listViewLapVS.Items.Count)
                    if (listViewLapVS.Items[NbSerie].Checked == true)
                        s_vs.Enabled = true;
                    else
                        s_vs.Enabled = false;
            }

            SerieAdd(NbSerie);

            int Synchro(int Indextableauref)
            {
                int _Indextableau_vs = Indextableauref;
                bool flag = false;
                int NbMaxLoop = 10000;
                int NbLoop = 0;

                // synchro avec pilote vs
                if (cBvs.Checked)
                {
                    while (flag == false)
                    {
                        // Vérification si même distance sur le tour 
                        if (_Indextableau_vs < dataUDP.Get_ListLapData_count())
                        {
                            if ((   dataUDP.Get_m_lapDistance(Indextableauref, NumCar)   -
                                    dataUDP.Get_m_lapDistance(_Indextableau_vs /*-1*/, NumCar_vs)) < dataSetting.synchroTolerance)
                                flag = true;
                        }
                        else
                            _Indextableau_vs--;

                        // vérification si dans le même tour avec avancement sur donnée data_vs + ou - si devant ou derriére
                        if (        dataUDP.Get_m_currentLapNum(Indextableau, NumCar)  >
                                    dataUDP.Get_m_currentLapNum(Indextableau_vs, NumCar_vs))
                            _Indextableau_vs++;

                        if (_Indextableau_vs < dataUDP.Get_ListLapData_count())
                        {
                            if (    dataUDP.Get_m_currentLapNum(Indextableau, NumCar) <
                                    dataUDP.Get_m_currentLapNum(Indextableau_vs, NumCar_vs))
                                _Indextableau_vs--;
                        }
                        else
                            _Indextableau_vs--;

                        if (        dataUDP.Get_m_currentLapNum(Indextableau, NumCar) ==
                                    dataUDP.Get_m_currentLapNum(Indextableau_vs, NumCar_vs))
                            _Indextableau_vs++;

                        if (_Indextableau_vs >= dataUDP.Get_ListLapData_count())
                        {
                            flag = true;
                            _Indextableau_vs = Indextableau;
                            tBetat.Text = "Erreur synchro - Tour " + dataUDP.Get_m_currentLapNum(Indextableau, NumCar);
                        }
                        NbLoop++;
                        if (NbLoop > NbMaxLoop) 
                        {
                            flag = true;
                            _Indextableau_vs = Indextableau;
                            tBetat.Text = "Erreur synchro too long - Tour 1st : " + Indextableau + " 2nd :" +  _Indextableau_vs;
                        }
                    }
                }
                return _Indextableau_vs;
            }

            for (Indextableau = 0; Indextableau < Math.Min(dataUDP.Get_ListTelemetry_count(), dataUDP.Get_ListLapData_count()); Indextableau++)  
            {
              if (dataUDP.Get_m_lapDistance(Indextableau, NumCar) > 0)
              {
                if (Indextableau > 1)
                    if ((dataUDP.Get_m_currentLapNum(Indextableau - 1,NumCar) -
                        dataUDP.Get_m_currentLapNum(Indextableau, NumCar)) < 0)
                    // nouveau tour
                    {
                        NbSerie++;
                        SerieAdd(NbSerie);

                            // synchro
                            // si pilote 1 devant pilote2 (vs) alors augmenter Indextableau_vs
                            // si pilote 1 derriére pilote2 (vs) alors diminuer Indextableau_vs
                            Indextableau_vs = Synchro(Indextableau);
                     }
                
                switch (name)
                {
                    case "m_speed": // multiG
                            Y = dataUDP.Get_m_speed(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_speed(Indextableau, NumCar_vs);
                            break;
                    case "m_engineRPM": // multiG
                            Y = dataUDP.Get_m_engineRPM(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_engineRPM(Indextableau, NumCar_vs);
                            break;
                    case "m_steer": // multiG
                            Y = dataUDP.Get_m_steer(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_steer(Indextableau, NumCar_vs);
                            break;
                    case "m_gear": // multiG
                            Y = dataUDP.Get_m_gear(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_gear(Indextableau, NumCar_vs);
                            break;
                    case "m_drs": // multiG
                            Y = dataUDP.Get_m_drs(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_drs(Indextableau, NumCar_vs);
                            break;
                    case "m_brakesTemperature[0]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_brakesTemperature(Indextableau,NumCar,0);
                            Y_vs = dataUDP.Get_m_brakesTemperature(Indextableau,NumCar_vs,0);
                        break;
                    case "m_brakesTemperature[1]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_brakesTemperature(Indextableau,NumCar,1);
                            Y_vs = dataUDP.Get_m_brakesTemperature(Indextableau,NumCar_vs,1);
                        break;
                    case "m_brakesTemperature[2]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_brakesTemperature(Indextableau,NumCar,2);
                            Y_vs = dataUDP.Get_m_brakesTemperature(Indextableau,NumCar_vs,2);
                        break;
                    case "m_brakesTemperature[3]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_brakesTemperature(Indextableau,NumCar,3);
                            Y_vs = dataUDP.Get_m_brakesTemperature(Indextableau,NumCar_vs,3);
                        break;

                        case "mWear[0]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_Wear(Indextableau, NumCar, 0);
                            Y_vs = dataUDP.Get_m_Wear(Indextableau, NumCar_vs, 0);
                            break;
                        case "mWear[1]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_Wear(Indextableau, NumCar, 1);
                            Y_vs = dataUDP.Get_m_Wear(Indextableau, NumCar_vs, 1);
                            break;
                        case "mWear[2]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_Wear(Indextableau, NumCar, 2);
                            Y_vs = dataUDP.Get_m_Wear(Indextableau, NumCar_vs, 2);
                            break;
                        case "mWear[3]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_Wear(Indextableau, NumCar, 3);
                            Y_vs = dataUDP.Get_m_Wear(Indextableau, NumCar_vs, 3);
                            break;

                        case "mTireLoad[0]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_mTireLoad(Indextableau, NumCar, 0);
                            Y_vs = dataUDP.Get_mTireLoad(Indextableau, NumCar_vs, 0);
                            break;
                        case "mTireLoad[1]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_mTireLoad(Indextableau, NumCar, 1);
                            Y_vs = dataUDP.Get_mTireLoad(Indextableau, NumCar_vs, 1);
                            break;
                        case "mTireLoad[2]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_mTireLoad(Indextableau, NumCar, 2);
                            Y_vs = dataUDP.Get_mTireLoad(Indextableau, NumCar_vs, 2);
                            break;
                        case "mTireLoad[3]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_mTireLoad(Indextableau, NumCar, 3);
                            Y_vs = dataUDP.Get_mTireLoad(Indextableau, NumCar_vs, 3);
                            break;


                        case "m_tyresSurfaceTemperature[0]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_tyresSurfaceTemperature(Indextableau,NumCar,0);
                            Y_vs = dataUDP.Get_m_tyresSurfaceTemperature(Indextableau,NumCar_vs,0);
                        break;
                    case "m_tyresSurfaceTemperature[1]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_tyresSurfaceTemperature(Indextableau,NumCar,1);
                            Y_vs = dataUDP.Get_m_tyresSurfaceTemperature(Indextableau,NumCar_vs,1);
                        break;
                    case "m_tyresSurfaceTemperature[2]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_tyresSurfaceTemperature(Indextableau,NumCar,2);
                            Y_vs = dataUDP.Get_m_tyresSurfaceTemperature(Indextableau,NumCar_vs,2);
                        break;
                    case "m_tyresSurfaceTemperature[3]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_tyresSurfaceTemperature(Indextableau,NumCar,3);
                            Y_vs = dataUDP.Get_m_tyresSurfaceTemperature(Indextableau,NumCar_vs,3);
                        break;
                    case "m_tyresInnerTemperature[0]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_tyresInnerTemperature(Indextableau,NumCar,0);
                            Y_vs = dataUDP.Get_m_tyresInnerTemperature(Indextableau,NumCar_vs,0);
                        break;
                    case "m_tyresInnerTemperature[1]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_tyresInnerTemperature(Indextableau,NumCar,1);
                            Y_vs = dataUDP.Get_m_tyresInnerTemperature(Indextableau,NumCar_vs,1);
                        break;
                    case "m_tyresInnerTemperature[2]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_tyresInnerTemperature(Indextableau, NumCar, 2);
                            Y_vs = dataUDP.Get_m_tyresInnerTemperature(Indextableau, NumCar_vs, 2);
                            break;
                    case "m_tyresInnerTemperature[3]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_tyresInnerTemperature(Indextableau,NumCar,3);
                            Y_vs = dataUDP.Get_m_tyresInnerTemperature(Indextableau,NumCar_vs,3);
                        break;
                    case "m_engineTemperature": // multiG
                            Y = dataUDP.Get_m_engineTemperature(Indextableau,NumCar);
                            Y_vs = dataUDP.Get_m_engineTemperature(Indextableau,NumCar_vs);
                    break;
                    case "m_tyresPressure[0]": // multiG - TABLEAU A REVOIR
                        Y = dataUDP.Get_m_tyresPressure(Indextableau,NumCar,0);
                        Y_vs = dataUDP.Get_m_tyresPressure(Indextableau,NumCar_vs,0);
                        break;
                    case "m_tyresPressure[1]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_tyresPressure(Indextableau, NumCar, 1);
                            Y_vs = dataUDP.Get_m_tyresPressure(Indextableau, NumCar_vs, 1);
                            break;
                    case "m_tyresPressure[2]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_tyresPressure(Indextableau, NumCar, 2);
                            Y_vs = dataUDP.Get_m_tyresPressure(Indextableau, NumCar_vs, 2);
                            break;
                    case "m_tyresPressure[3]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_tyresPressure(Indextableau, NumCar, 3);
                            Y_vs = dataUDP.Get_m_tyresPressure(Indextableau, NumCar_vs, 3);
                            break;
                    case "m_surfaceType[0]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_surfaceType(Indextableau, NumCar, 0);
                            Y_vs = dataUDP.Get_m_surfaceType(Indextableau, NumCar_vs, 0);
                            break;
                    case "m_surfaceType[1]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_surfaceType(Indextableau, NumCar, 1);
                            Y_vs = dataUDP.Get_m_surfaceType(Indextableau, NumCar_vs, 1);
                            break;
                    case "m_surfaceType[2]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_surfaceType(Indextableau, NumCar, 2);
                            Y_vs = dataUDP.Get_m_surfaceType(Indextableau, NumCar_vs, 2);
                            break;
                    case "m_surfaceType[3]": // multiG - TABLEAU A REVOIR
                            Y = dataUDP.Get_m_surfaceType(Indextableau, NumCar, 3);
                            Y_vs = dataUDP.Get_m_surfaceType(Indextableau, NumCar_vs, 3);
                            break;
                    case "m_revLightsBitValue": // multiG
                        Y = dataUDP.Get_m_revLightsBitValue(Indextableau,NumCar);
                        Y_vs = dataUDP.Get_m_revLightsBitValue(Indextableau,NumCar_vs);
                        break;
                    case "m_revLightsPercent": // multiG
                            Y = dataUDP.Get_m_revLightsPercent(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_revLightsPercent(Indextableau, NumCar_vs);
                            break;
                    case "m_brake": // multiG
                            Y = dataUDP.Get_m_brake(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_brake(Indextableau, NumCar_vs);
                            break;
                    case "m_throttle": // multiG
                            Y = dataUDP.Get_m_throttle(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_throttle(Indextableau, NumCar_vs);
                            break;
                    case "m_clutch": // multiG
                            Y = dataUDP.Get_m_clutch(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_clutch(Indextableau, NumCar_vs);
                            break;
                    case "m_yaw": // multiG
                            Y = dataUDP.Get_m_yaw(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_yaw(Indextableau, NumCar_vs);
                            break;
                    case "m_pitch": // multiG
                            Y = dataUDP.Get_m_pitch(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_pitch(Indextableau, NumCar_vs);
                            break;
                    case "m_roll": // multiG
                            Y = dataUDP.Get_m_roll(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_roll(Indextableau, NumCar_vs);
                            break;
                        case "m_gForceLateral": // multiG
                            Y = dataUDP.Get_m_gForceLateral(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_gForceLateral(Indextableau, NumCar_vs);
                            break;
                    case "m_gForceLongitudinal": // multiG
                            Y = dataUDP.Get_m_gForceLongitudinal(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_gForceLongitudinal(Indextableau, NumCar_vs);
                            break;
                    case "m_gForceVertical": // multiG
                            Y = dataUDP.Get_m_gForceVertical(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_gForceVertical(Indextableau, NumCar_vs);
                            break;
                    case "m_worldPositionX": // multiG
                            Y = dataUDP.Get_m_worldPositionX(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_worldPositionX(Indextableau,NumCar_vs);
                            break;
                    case "m_worldPositionY": // multiG
                            Y = dataUDP.Get_m_worldPositionY(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_worldPositionY(Indextableau, NumCar_vs);
                            break;
                    case "m_worldPositionZ": // multiG
                            Y = dataUDP.Get_m_worldPositionZ(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_worldPositionZ(Indextableau, NumCar_vs); 
                            break;
                    case "m_worldVelocityX": // multiG
                            Y = dataUDP.Get_m_worldVelocityX(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_worldVelocityX(Indextableau, NumCar_vs); 
                            break;
                    case "m_worldVelocityY": // multiG
                            Y = dataUDP.Get_m_worldVelocityY(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_worldVelocityY(Indextableau, NumCar_vs); 
                            break;
                    case "m_worldVelocityZ": // multiG
                            Y = dataUDP.Get_m_worldVelocityZ(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_worldVelocityZ(Indextableau, NumCar_vs); 
                            break;
                    case "m_worldForwardDirX": // multiG
                            Y = dataUDP.Get_m_worldForwardDirX(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_worldForwardDirX(Indextableau, NumCar_vs); 
                            break;
                    case "m_worldForwardDirY": // multiG
                            Y = dataUDP.Get_m_worldForwardDirX(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_worldForwardDirX(Indextableau, NumCar_vs); 
                            break;
                    case "m_worldForwardDirZ": // multiG
                            Y = dataUDP.Get_m_worldForwardDirX(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_worldForwardDirX(Indextableau, NumCar_vs); 
                            break;
                    case "m_worldRightDirX": // multiG
                            Y = dataUDP.Get_m_worldRightDirX(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_worldRightDirX(Indextableau, NumCar_vs); 
                            break;
                    case "m_worldRightDirY": // multiG
                            Y = dataUDP.Get_m_worldRightDirY(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_worldRightDirY(Indextableau, NumCar_vs); 
                            break;
                    case "m_worldRightDirZ": // multiG
                            Y = dataUDP.Get_m_worldRightDirZ(Indextableau, NumCar);
                            Y_vs = dataUDP.Get_m_worldRightDirZ(Indextableau, NumCar_vs); 
                            break;
                    }

                X = dataUDP.Get_m_lapDistance(Indextableau, NumCar);
                X = dataUDP.Get_m_lapDistance(Indextableau, NumCar);
                X_vs = dataUDP.Get_m_lapDistance(Indextableau, NumCar_vs); 

                s.Points.AddXY(X, Y);
                if (cBvs.Checked)
                    s_vs.Points.AddXY(X_vs,Y_vs);
              }

              if (chart.Name == "chartMultiGraph")
                {
                    tooltipX = "";
                    tooltipY = "";
                }
                else
                {
                    tooltipX = "Dist =";
                    tooltipY = " M =";
                }
            }

            indexSeries++;

            ca.AxisX.Minimum = 0;

            // for (int i = 0; i <= chart.Series.Count - 1; i++)
            if (chart.Series.Count > 0)
            {
                for (int i = 0; i <= LocalTrackColor.Count - 1; i++)
                    if (i < listViewLap.Items.Count)
                        listViewLap.Items[i].BackColor = LocalTrackColor[i];

                for (int i = 0; i <= LocalTrackColorVS.Count - 1; i++)
                    if (i < listViewLapVS.Items.Count)
                        listViewLapVS.Items[i].BackColor = LocalTrackColorVS[i];
            }
        }

        private void AffichageMultiGraph_Show(ref Chart chartMulti, int name)
        {
            chartMulti.ChartAreas[name].Position.X = 0;
            chartMulti.ChartAreas[name].Position.Y = name * (100 / chartMulti.ChartAreas.Count);
            chartMulti.ChartAreas[name].Position.Width = 100; // chartMulti.Size.Width - 8;
            chartMulti.ChartAreas[name].Position.Height = (100 / chartMulti.ChartAreas.Count); // (chartMulti.Size.Height - 32) / 2;
        }

        private void AffichageMultiGraph()
        {
            int indexSeries = 0;

            // https://stackoverflow.com/questions/49872756/vertical-line-across-multiple-line-charts-with-value-display-for-each-chart-in-w
            // https://github.com/Live-Charts/Live-Charts/issues/662
            // https://www.dotnetcharting.com/documentation/vCurrent/Multiple%20Chart%20Areas.html
            // https://betterdashboards.wordpress.com/2009/02/11/align-multiple-chart-areas/
            // https://docs.microsoft.com/fr-fr/dotnet/desktop/winforms/controls/how-to-determine-checked-items-in-the-windows-forms-checkedlistbox-control?view=netframeworkdesktop-4.8

            chartMultiGraph.ChartAreas.Clear();
            chartMultiGraph.Show();

            // Clear all series and chart areas so we can re-add them
            chartMultiGraph.Series.Clear();
            chartMultiGraph.ChartAreas.Clear();

            if (listViewData.CheckedItems.Count != 0)
            {
                for (int x = 0; x < listViewData.CheckedItems.Count; x++)
                { 
                    //tBetat.Text = listViewData.CheckedItems[x].Text;
                    AffichageMultiGraph_Add(ref chartMultiGraph, ref indexSeries, listViewData.CheckedItems[x].Text);
                }
            }
            else
            {
               // tBetat.Text = listViewData.CheckedItems[6].ToString();
                listViewData.Items[6].Checked = true;
                AffichageMultiGraph_Add(ref chartMultiGraph, ref indexSeries, listViewData.CheckedItems[6].Text);
            }

            // Set the chart area position 
            for (int index = 0; index < chartMultiGraph.ChartAreas.Count; index++)
            {
                AffichageMultiGraph_Show(ref chartMultiGraph, index);
                chartMultiGraph.ChartAreas[index].AlignWithChartArea = chartMultiGraph.ChartAreas[0].Name;
            }

            if (barreverticale)
                if ((chartMultiGraph.ChartAreas.Count > 0) && (chartMultiGraph.Series[0].Points.Count)>0)
                {
                    VL = new VerticalLineAnnotation();  // the annotation
                    VL.AllowMoving = true;              // make it interactive

                    //VL.AnchorDataPoint = chartMultiGraph.Series[0].Points[0];  // start at the 1st point
                    VL.AxisX = chartMultiGraph.ChartAreas[0].AxisX;
                    VL.LineColor = Color.Red;
                    VL.LineWidth = 2;
                    VL.IsInfinitive = true;             // let it go all over the chart
                                                        // Gérer l'événement PositionChanged
                    
                    chartMultiGraph.Annotations.Add(VL);
                }

            AffichageMap(CMAPsmall);

        }

        // non utilisé car conflit avec le zoom

        private void AffichageMultiGraph_Paint_1(object sender, PaintEventArgs e)
        {
            int j;

            if (barreverticale && VL != null)
            {
                double xv = VL.X;
                if ((listViewLap.Items.Count > 1) || (listViewLapVS.Items.Count > 0))
                    tBetat.Text = "Attention valeur 1er tour prise en compte";
                else
                    tBetat.Text = "";
                for (int i = 0; i < chartMultiGraph.ChartAreas.Count; i++)
                {
                    ChartArea ca = chartMultiGraph.ChartAreas[i];

                    j= 0;
                    for(int k=0;k< listViewLap.Items.Count; k++)
                    {
                        if (listViewLap.Items[k].Checked)
                        {
                            j = k + (listViewLap.Items.Count) * i; // gestion des multigraphiques
                            break;
                        }
                    }
                    // attention i dépend du nombre de tours affichés et si VS
                    /*
                    if (cBvs.Checked)
                        j = (listViewLap.Items.Count+ listViewLapVS.Items.Count)*i;
                    else
                        j = (listViewLap.Items.Count) * i;
                    */
                    Series s = chartMultiGraph.Series[j];

                    int px = (int)xv;
                                     
                    int px2 = (int)ca.AxisX.ValueToPixelPosition(xv);

                    var dp2 = s.Points.Where(x => x.XValue >= xv).FirstOrDefault();
                    double tolerance = 7.0; // Ajustez la tolérance selon vos besoins

                    var dp = s.Points.FirstOrDefault(x => Math.Abs(x.XValue - xv) <= tolerance);

                    if (dp != null)
                    {
                        int py = (int)ca.AxisY.ValueToPixelPosition(s.Points[0].YValues[0]) - 20;
                        e.Graphics.DrawString(ca.Name+":"+dp.YValues[0].ToString("0.00"), Font, Brushes.Black, px2, py);
                        //Debug.WriteLine("J :"+j+" VL.X :" + VL.X + " px :" + px + " py :" + py+" dp :" + dp.YValues[0]);
                    }
                }
                Analyse_position_pilote(2, CMAPsmall, VL.X);
            }
        }

        private void AffichageMultiGraph_Paint(object sender, PaintEventArgs e)
        {
            AffichageMultiGraph_Paint_1(sender,e);
        }

        private void InitLegend()
        {
            int NbSerie = 0;

            listViewLap.Items.Clear();
            listViewLap.CheckBoxes = true;
            listViewLap.Items.Add("■ Tour " + (NbSerie + 1).ToString());
            listViewLap.Items[0].Checked = true;

            listViewLapVS.Items.Clear();
            listViewLapVS.CheckBoxes = true;
            listViewLapVS.Items.Add("■ Tour " + (NbSerie + 1).ToString());
            listViewLapVS.Items[0].Checked = true;


            LocalTrackColor.Add(dataSetting.ColorGet(NbSerie));
            LocalTrackColorVS.Add(dataSetting.ColorGet(NbSerie));

            if (dataUDP.checkdata()) // Vérification si des données sont disponibles
            {
                for (int Indextableau = 0; Indextableau < dataUDP.Get_ListLapData_count() - 1; Indextableau++)
                {
                    if (Indextableau > 1)
                    {
                        var  LapBefore = (dataUDP.Get_m_currentLapNum(Indextableau - 1, NumCar));
                        var  Lap = (dataUDP.Get_m_currentLapNum(Indextableau, NumCar));

                        if ((dataUDP.Get_m_currentLapNum(Indextableau - 1,NumCar)-
                            dataUDP.Get_m_currentLapNum(Indextableau, NumCar)) < 0)
                        // nouveau tour
                        {
                            NbSerie++;
                            listViewLap.Items.Add("■ Tour " + (NbSerie + 1).ToString());
                            listViewLap.Items[NbSerie].Checked = true;

                            listViewLapVS.Items.Add("■ Tour " + (NbSerie + 1).ToString());
                            listViewLapVS.Items[NbSerie].Checked = true;

                            // Ajoute des couleurs si la course a plus de 30 tours
                            if (NbSerie > (dataSetting.TrackColor.Count-1))
                                dataSetting.TrackColor.Add(dataSetting.TrackColor[dataSetting.TrackColor.Count-NbSerie]);

                            // transfert des couleur dans setting vers local pour 30 tours
                            LocalTrackColor.Add(dataSetting.ColorGet(NbSerie));
                            LocalTrackColorVS.Add(dataSetting.ColorGet(NbSerie));
                        }
                    }
                }
            }
            else
            {
                tBetat.Text = "Pas de données pour la simulation";
            }

        }

        private void InitListDrivers()
        {
            cBdrivers.Items.Clear();
            cBdriversVS.Items.Clear();

            for (int IndexPlayer = 0; IndexPlayer < dataUDP.Get_NumofPlayers(); IndexPlayer++)
            {
                cBdrivers.Items.Add(
                    dataUDP.Get_m_s_name(IndexPlayer) + 
                    " -" +
                    dataUDP.Get_m_driverId(IndexPlayer));

                cBdriversVS.Items.Add(
                    dataUDP.Get_m_s_name(IndexPlayer) +
                    " -" +
                    dataUDP.Get_m_driverId(IndexPlayer));
            }

            // nécessaire sinon les selectedindexchanged sont appelés
            cBdriversVS.SelectedIndexChanged -= cBdriversVS_SelectedIndexChanged;
            cBdrivers.SelectedIndexChanged -= cBdrivers_SelectedIndexChanged;

            cBdrivers.SelectedIndex = NumCar;     
            cBdriversVS.SelectedIndex = NumCar_vs;  

            cBdriversVS.SelectedIndexChanged += cBdriversVS_SelectedIndexChanged;
            cBdrivers.SelectedIndexChanged += cBdrivers_SelectedIndexChanged;

        }

        private void AffichageMap3d()
        {
            // https://www.developpez.net/forums/d126527/applications/developpement-2d-3d-jeux/theorie-conversion-coordonnees-3d-vers-2d/#:~:text=La%20formule%20math%C3%A9matique%20qui%20permet,Voil%C3%A0%20pour%20la%20th%C3%A9orie.
            // https://jeux.developpez.com/faq/math/
            // https://stackoverflow.com/questions/724219/how-to-convert-a-3d-point-into-2d-perspective-projection


            //checkedListBoxTours

            ToolTip tip = new ToolTip();

            Chart chart;

            int NbSerie = 0; // la 1er série est créée dans l'init donc pas besoin de le récupérer de la fonction initgraph

            chart = cMAP3D;

            AnalyseBorderWidth = 10;
            PreviousBorderWidth = 3;

            float trigger;
            Color triggercolor;

            double X, Y;
            double Xc3d, Yc3d, Zc3d;

            Xc3d = 500;
            Yc3d = 500;
            Zc3d = 500;

            // mise à zéro du graphique
            chart.Series.Clear();

            // https://www.vbforums.com/showthread.php?810735-RESOLVED-Problem-Changing-Data-Point-Color-MSChart
            SeriesChartType ChartType = SeriesChartType.Line;

            chart.Series.Add("Tour  " + (NbSerie + 1).ToString());

            chart.Series[NbSerie].ChartType = ChartType;
            chart.Series[NbSerie].BorderWidth = 2;

            //https://stackoverflow.com/questions/29985796/chart-zoom-in-to-show-more-precise-data
            ChartArea CA = chart.ChartAreas[0];  // quick reference
            CA.AxisX.ScaleView.Zoomable = true;
            CA.CursorX.AutoScroll = true;
            CA.CursorX.IsUserSelectionEnabled = true;

            CA.AxisY.ScaleView.Zoomable = true;
            CA.CursorY.AutoScroll = true;
            CA.CursorY.IsUserSelectionEnabled = true;
            tooltipX = "X (M) =";
            tooltipY = ", Y (M) =";


            if (dataUDP.checkdata())
            {

                for (int Indextableau = 0; Indextableau < dataUDP.Get_ListMotion_packet_count() - 1; Indextableau++)
                {
                    if (Indextableau > 1)
                        if ((dataUDP.Get_m_currentLapNum(Indextableau-1,NumCar)-
                            dataUDP.Get_m_currentLapNum(Indextableau, NumCar)) < 0)
                        // nouveau tour
                        {
                            NbSerie++;
                            chart.Series.Add("Tour  " + (NbSerie + 1).ToString());
                            chart.Series[NbSerie].ChartType = ChartType;
                            chart.Series[NbSerie].BorderWidth = 2;
                        }

                    if (listViewLap.Items[NbSerie].Checked == true)
                        chart.Series[NbSerie].Enabled = true;
                    else
                        chart.Series[NbSerie].Enabled = false;

                    // https://www.construct-french.fr/2013/01/30/soyons-ludique-la-projection-2d/

                    X = ((Zc3d*(- dataUDP.Get_m_worldPositionX(Indextableau,NumCar) - Xc3d)) / 
                        (Zc3d+ dataUDP.Get_m_worldPositionY(Indextableau, NumCar))) * Xc3d;

                    Y = ((Zc3d * (dataUDP.Get_m_worldPositionZ(Indextableau, NumCar)  - Yc3d)) /
                        (Zc3d + dataUDP.Get_m_worldPositionY(Indextableau, NumCar))) * Yc3d;


                    chart.Series[NbSerie].Points.AddXY(X, Y);

                    // https://stackoverflow.com/questions/26729843/c-sharp-change-color-in-chart
                    // https://askcodez.com/comment-modifier-les-couleurs-de-certains-points-de-donnees-dans-une-serie-dun-graphique.html

                    if (speedToolStripMenuItem.Checked)
                    {
                        trigger = dataUDP.Get_m_speed(Indextableau, NumCar);

                        triggercolor = Color.BlueViolet;

                        if (trigger > 50)
                            triggercolor = Color.Blue;
                        if (trigger > 100)
                            triggercolor = Color.Cyan;
                        if (trigger > 150)
                            triggercolor = Color.Yellow;
                        if (trigger > 150)
                            triggercolor = Color.Orange;
                        if (trigger > 200)
                            triggercolor = Color.OrangeRed;
                        if (trigger > 300)
                            triggercolor = Color.Red;

                        chart.Series[NbSerie].Points[chart.Series[NbSerie].Points.Count - 1].Color = triggercolor;
                    }

                    if (brakeToolStripMenuItem.Checked)
                    {
                        trigger = dataUDP.Get_m_brake(Indextableau, NumCar);

                        triggercolor = Color.BlueViolet;

                        if (trigger > 0.05)
                            triggercolor = Color.Blue;
                        if (trigger > 0.1)
                            triggercolor = Color.Cyan;
                        if (trigger > 0.5)
                            triggercolor = Color.Yellow;
                        if (trigger > 0.7)
                            triggercolor = Color.Orange;
                        if (trigger > 0.8)
                            triggercolor = Color.OrangeRed;
                        if (trigger > 0.9)
                            triggercolor = Color.Red;

                        chart.Series[NbSerie].Points[chart.Series[NbSerie].Points.Count - 1].Color = triggercolor;
                    }
                }

            }


        }

        private void AffichageMap(Chart chart)
        {
            //checkedListBoxTours
            double x1 = 0;
            double x2 = 0;
            bool Trace = true;

            ToolTip tip = new ToolTip();
            Series s, s_vs = null;

            int NbSerie = 0; // la 1er série est créée dans l'init donc pas besoin de le récupérer de la fonction initgraph

            AnalyseBorderWidth = 10;
            PreviousBorderWidth = 3;

            float trigger;
            Color triggercolor;

            // mise à zéro du graphique
            chart.Series.Clear();

            // https://www.vbforums.com/showthread.php?810735-RESOLVED-Problem-Changing-Data-Point-Color-MSChart
            SeriesChartType ChartType = SeriesChartType.Line;

            s = chart.Series.Add("Tour  " + (NbSerie + 1).ToString());
            s.ChartType = ChartType;
            s.BorderWidth = 2;
            s.BorderDashStyle = ChartDashStyle.Solid;

            if (cBvs.Checked)
            {
                s_vs = chart.Series.Add("VS Tour  " + (NbSerie + 1).ToString());
                s_vs.ChartType = ChartType;
                s_vs.BorderWidth = 2;
                s_vs.BorderDashStyle = ChartDashStyle.DashDotDot;
            }

            //https://stackoverflow.com/questions/29985796/chart-zoom-in-to-show-more-precise-data
            ChartArea CA = chart.ChartAreas[0];  // quick reference
            CA.AxisX.ScaleView.Zoomable = true;
            CA.AxisX.Enabled = AxisEnabled.True;
            CA.CursorX.AutoScroll = true;
            CA.CursorX.IsUserSelectionEnabled = true;

            CA.AxisY.ScaleView.Zoomable = true;
            CA.AxisY.Enabled = AxisEnabled.True;
            CA.CursorY.AutoScroll = true;
            CA.CursorY.IsUserSelectionEnabled = true;

            if (chart == cMAP) 
            { 
                tooltipX = "X (M) =";
                tooltipY = ", Y (M) =";
            }
            else
            {
                CA.AxisX.Enabled = AxisEnabled.False;
                CA.AxisY.Enabled = AxisEnabled.False;

                if ((chartMultiGraph.ChartAreas.Count > 0))
                {
                    x1 = chartMultiGraph.ChartAreas[0].AxisX.ScaleView.ViewMinimum;
                    x2 = chartMultiGraph.ChartAreas[0].AxisX.ScaleView.ViewMaximum;
                    // tBetat.Text = "x1 :" + x1 + " x2 + " + x2;
                }
                else
                {
                    //tBetat.Text = "rien";
                }
            }

            // https://georezo.net/forum/viewtopic.php?id=51233
            // https://learn.microsoft.com/fr-fr/dotnet/api/system.windows.media.lineargradientbrush?view=windowsdesktop-8.0
            //Getcolor(trigger, 0, 400, 6, "L");
            Color GetColorold(float valeur,float min,float max,char Curve)
            {
                double Step;
                int i=0, rAverage, gAverage, bAverage;

                if (Curve == 'C')
                {
                    Step = (max - min) / dataSetting.ColorDegradee.Count;
                    i = 1;
                    while ((valeur > (Step * Math.Log(i))) && (i<1000)) // i<1000 pour éviter une boucle infinie
                        i++;
                    i--;
                }
                else
                {
                    Step = (max - min) / dataSetting.ColorDegradee.Count;

                    while (valeur > (Step * i)) i++;
                    i--;
                }

                if (i < 0) i=0;
                if (i > (dataSetting.ColorDegradee.Count-2)) i = dataSetting.ColorDegradee.Count - 2;


                rAverage = dataSetting.ColorDegradee[i].R + (int)((dataSetting.ColorDegradee[i+1].R - dataSetting.ColorDegradee[i].R) * i / dataSetting.ColorDegradee.Count);
                gAverage = dataSetting.ColorDegradee[i].G + (int)((dataSetting.ColorDegradee[i+1].G - dataSetting.ColorDegradee[i].G) * i / dataSetting.ColorDegradee.Count);
                bAverage = dataSetting.ColorDegradee[i].B + (int)((dataSetting.ColorDegradee[i+1].B - dataSetting.ColorDegradee[i].B) * i / dataSetting.ColorDegradee.Count);
               

                return Color.FromArgb(rAverage, gAverage, bAverage);
            }
            Color GetColor(float valeur, float min, float max, char Curve)
            {
                static int Interpolate(int start, int end, float ratio)
                {
                    return (int)(start + (end - start) * ratio);
                }

                //int r, g, b;

                float normalizedValue = valeur / (max-min); // Normalisation entre 0 et 1

                Color[] colors;
                Color[] colorsS = { Color.Blue, Color.Green, Color.Yellow, Color.Red };
                Color[] colorsB = { Color.Purple, Color.MediumPurple,  Color.BlueViolet, Color.Blue, Color.Cyan, Color.Green, Color.GreenYellow, Color.Yellow, Color.Orange, Color.OrangeRed, Color.Red };

                float[] positions;
                float[] positionsS = { 0.0f, 0.3f, 0.6f, 1.0f };
                float[] positionsB = { 0.0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };

                positions = positionsS;
                colors = colorsS;
                if (Curve=='B')
                {
                    positions = positionsB;
                    colors = colorsB;
                }

                int index = 0;
                while (index < positions.Length - 1 && normalizedValue > positions[index + 1])
                    index++;

                if (index < positions.Length - 1)
                {
                    float range = positions[index + 1] - positions[index];
                    float ratio = (float)((normalizedValue - positions[index]) / range);

                    Color startColor = colors[index];
                    Color endColor = colors[index + 1];

                    int r = Interpolate(startColor.R, endColor.R, ratio);
                    int g = Interpolate(startColor.G, endColor.G, ratio);
                    int b = Interpolate(startColor.B, endColor.B, ratio);

                    //int alpha = (int)(255 * normalizedValue);
                    int alpha = (int)(255 * Math.Max(normalizedValue, 0.2)); // Utilisation d'une valeur minimale pour l'alpha
                    //Debug.WriteLine(normalizedValue+" - "+valeur);
                    return Color.FromArgb(alpha, r, g, b);
                }

                // En cas d'erreur ou de dépassement, retournez une couleur par défaut (par exemple, rouge)
                return Color.Red;
            }

            if (dataUDP.checkdata())
            {

                for (int Indextableau = 0; ((Indextableau < dataUDP.Get_ListMotion_packet_count() - 1) && (Indextableau < dataUDP.Get_ListTelemetry_count() - 1)) ; Indextableau++)
                {
                    if (chart != cMAP)
                    {
                        if ((dataUDP.Get_m_lapDistance(Indextableau,NumCar)< (float)x1) ||
                            (dataUDP.Get_m_lapDistance(Indextableau, NumCar) > (float)x2))
                            Trace = false;
                        else
                            Trace = true;
                    }
                    else
                        Trace = true;

                    if (Indextableau > 1)
                    {
                        // pb de synchro des tours enrtre Numcar et Numcar_vs
                        // changement de méthodes à étudier
                        if ((dataUDP.Get_m_currentLapNum(Indextableau-1,NumCar) -
                            dataUDP.Get_m_currentLapNum(Indextableau , NumCar)) < 0)
                        { // nouveau tour
                            NbSerie++;
                            s = chart.Series.Add("Tour  " + (NbSerie + 1).ToString());
                            s.ChartType = ChartType;
                            s.BorderWidth = 2;
                            s.BorderDashStyle = ChartDashStyle.Solid;
                            s.ToolTip = "Tour " + (NbSerie + 1) + " - Pilote : " + dataUDP.Get_m_s_name(0,NumCar);

                            if (cBvs.Checked)
                            {
                                s_vs = chart.Series.Add("VS Tour  " + (NbSerie + 1).ToString());
                                s_vs.ChartType = ChartType;
                                s_vs.BorderWidth = 2;
                                s_vs.BorderDashStyle = ChartDashStyle.DashDotDot;
                                s_vs.ToolTip = "Tour " + (NbSerie + 1) + " - Pilote : " + dataUDP.Get_m_s_name(0, NumCar_vs);
                            }
                        }
                    }

                    if (listViewLap.Items[NbSerie].Checked == true)
                    {
                        s.Enabled = true;
                        if (cBvs.Checked)
                            s_vs.Enabled = true;
                    }
                    else
                    {
                        s.Enabled = false;
                        if (cBvs.Checked)
                            s_vs.Enabled = false;
                    }

                    if (Trace) // trace les points sur le graphique
                    {
                        // position de la voiture sur le circuit
                        s.Points.AddXY(
                                - dataUDP.Get_m_worldPositionX(Indextableau,NumCar), // - nécessaire pour être cohérent avec la réalité
                                  dataUDP.Get_m_worldPositionZ(Indextableau, NumCar));
                        if (cBvs.Checked)
                            s_vs.Points.AddXY(
                                -dataUDP.Get_m_worldPositionX(Indextableau, NumCar_vs), // - nécessaire pour être cohérent avec la réalité
                                 dataUDP.Get_m_worldPositionZ(Indextableau, NumCar_vs));

                        // https://stackoverflow.com/questions/26729843/c-sharp-change-color-in-chart
                        // https://askcodez.com/comment-modifier-les-couleurs-de-certains-points-de-donnees-dans-une-serie-dun-graphique.html

                        // couleur des points en fonction de l'intensité
                        if (speedToolStripMenuItem.Checked)
                        {
                            trigger = dataUDP.Get_m_speed(Indextableau,NumCar);

                            triggercolor = GetColor(trigger, 0, 300, 'S');

                            s.Points[s.Points.Count - 1].Color = triggercolor;
                            if (cBvs.Checked)
                            {
                                trigger = dataUDP.Get_m_speed(Indextableau, NumCar_vs);

                                triggercolor = GetColor(trigger, 0, 300, 'S');

                                s_vs.Points[s.Points.Count - 1].Color = triggercolor;
                            }
                        }

                        if (brakeToolStripMenuItem.Checked)
                        {
                            trigger = dataUDP.Get_m_brake(Indextableau, NumCar);

                            triggercolor = GetColor(trigger, 0, 1, 'B');

                            s.Points[s.Points.Count - 1].Color = triggercolor;
                            if (cBvs.Checked)
                            {
                                trigger = dataUDP.Get_m_brake(Indextableau, NumCar_vs);

                                triggercolor = GetColor(trigger, 0, 300, 'B');

                                s_vs.Points[s.Points.Count - 1].Color = triggercolor;
                            }

                        }
                    }
                }


                if (s.Points.Count == 0) 
                {
                    tBetat.Text =
                        "X Max : " + chart.ChartAreas[0].AxisX.Maximum + " X Min : " + chart.ChartAreas[0].AxisX.Minimum +
                        "Y Max : " + chart.ChartAreas[0].AxisY.Maximum + " Y Min : " + chart.ChartAreas[0].AxisY.Minimum + 
                        "s.Points.Count = 0 Ligne 1034";
                } 
                else
                {
                    tBetat.Text =
                        "X Max : " + chart.ChartAreas[0].AxisX.Maximum + " X Min : " + chart.ChartAreas[0].AxisX.Minimum +
                        "Y Max : " + chart.ChartAreas[0].AxisY.Maximum + " Y Min : " + chart.ChartAreas[0].AxisY.Minimum
                    + s.Points.FindMaxByValue().ToString() + s.Points.FindMinByValue().ToString();
                }

                chart.ChartAreas[0].AxisX.RoundAxisValues();

                /*
                chart.ChartAreas[0].AxisX.Maximum = 1500; // s.Points.FindMaxByValue().YValues[0];
                chart.ChartAreas[0].AxisX.Minimum = -500;// s.Points.FindMinByValue().YValues[0];

                chart.ChartAreas[0].AxisY.Maximum = 1500;// s.Points.FindMaxByValue().YValues[0];
                chart.ChartAreas[0].AxisY.Minimum = -500;// s.Points.FindMaxByValue().XValue;

                /*
                
                else
                    chart.ChartAreas[0].AxisY.Maximum = chart.ChartAreas[0].AxisX.Maximum;

                if (Math.Abs(chart.ChartAreas[0].AxisX.Minimum) < Math.Abs(chart.ChartAreas[0].AxisY.Minimum))
                    chart.ChartAreas[0].AxisX.Minimum = chart.ChartAreas[0].AxisY.Minimum;
                else
                    chart.ChartAreas[0].AxisY.Minimum = chart.ChartAreas[0].AxisX.Minimum;
               */
            }
        }

        private void Analyse_position_pilote(byte _typeView, Chart chart, double Distance)
        {
            ToolTip tip = new ToolTip();
            SeriesChartType ChartType = SeriesChartType.Point;

            Series s = null;

            int NbSerie = 0;
            for (int i = 0; i < chart.Series.Count; i++)
                if (chart.Series[i].Name == "PILOTES")
                    chart.Series.Remove(chart.Series["PILOTES"]);
                    
            s = chart.Series.Add("PILOTES");

            s.ChartType = ChartType;
            s.BorderWidth = 5;


            if (_typeView == 0)
            {
                for (int Indextableau = 0; Indextableau < dataUDP.Get_ListMotion_packet_count() - 1; Indextableau++)
                {
                    if (Indextableau > 1)
                    {

                        if ((   dataUDP.Get_m_currentLapNum(Indextableau - 1,NumCar) -
                                dataUDP.Get_m_currentLapNum(Indextableau, NumCar)) < 0)
                        { // nouveau tour
                            if (listViewLap.Items[NbSerie].Checked == true)
                                for (int _NumCar = 0; _NumCar < dataUDP.Get_m_numActiveCars(0) ; _NumCar++)
                                {
                                    s.Points.AddXY(
                                        -   dataUDP.Get_m_worldPositionX(Indextableau,_NumCar),
                                            dataUDP.Get_m_worldPositionZ(Indextableau, _NumCar));
                                    s.Points[s.Points.Count - 1].Color = Color.Black;
                                    s.Points[s.Points.Count - 1].MarkerSize = 10;
                                    s.Points[s.Points.Count - 1].MarkerStyle = MarkerStyle.Circle;
                                    s.Points[s.Points.Count - 1].Label = //_NumCar+" : " +
                                        dataUDP.Get_m_carPosition(Indextableau,_NumCar) +
                                        " : " +
                                        dataUDP.Get_m_s_name(0,_NumCar);
                                }
                            NbSerie++;
                        }
                    }
                }
            }

            if (_typeView == 1)
            {
                int Indextableau;

                hScrollBarLap.Visible = true;
                hScrollBarLap.Minimum = 0;
                hScrollBarLap.Maximum = dataUDP.Get_ListMotion_packet_count()- 1;

                Indextableau = hScrollBarLap.Value;

                //r (int Indextableau = 0; Indextableau < dataf1_2021.ListMotion_packet.Count - 1; Indextableau++)
                {
                    if (Indextableau > 1)
                    {

                        if ((   dataUDP.Get_m_currentLapNum(Indextableau - 1, NumCar)
                                 -
                                 dataUDP.Get_m_currentLapNum(Indextableau, NumCar)) < 0)
                            NbSerie++; // nouveau tour détecté pour gestion si affichage à faire
                        if (listViewLap.Items[NbSerie].Checked == true)
                            for (int _NumCar = 0; _NumCar < dataUDP.Get_m_numActiveCars(0); _NumCar++)
                            {
                                s.Points.AddXY(
                                    -   dataUDP.Get_m_worldPositionX(Indextableau,_NumCar),
                                        dataUDP.Get_m_worldPositionZ(Indextableau, _NumCar));
                                s.Points[s.Points.Count - 1].Color = Color.Black;
                                s.Points[s.Points.Count - 1].MarkerSize = 10;
                                s.Points[s.Points.Count - 1].MarkerStyle = MarkerStyle.Circle;
                                s.Points[s.Points.Count - 1].Label = //_NumCar+" : " +
                                    dataUDP.Get_m_carPosition(Indextableau/* - 1*/, _NumCar).ToString() +
                                    " : " +
                                    dataUDP.Get_m_s_name(0, _NumCar);
                            }
                    }
                }
            }

            if (_typeView == 2)
            {
                
                int Indextableau=-1;
                int _NumCar = cBdrivers.SelectedIndex;
                double tempo;
                long NbItem;
                
                if (Distance < dataUDP.Get_trackLength())
                {
                    
                    if (dataUDP.Simulation == 2)
                       NbItem = dataUDP.ListTelemetryData.Count;
                    else
                        NbItem = 0;


                    for (int i = 0; i < NbItem; i++)
                    {
                        tempo = dataUDP.Get_m_lapDistance(i, _NumCar);
                        if ((tempo+7 > Distance) && (tempo - 7 < Distance))
                        {
                            //Debug.WriteLine("Lap (" + i + ") :" + tempo + " "+Distance);
                            Indextableau = i;
                            break;
                        }   
                    }

                  
                    
                    if ((Indextableau > 1) & (_NumCar < dataUDP.Get_m_numActiveCars(0)))
                    {
                        {
                            var X = dataUDP.Get_m_worldPositionX(Indextableau, _NumCar);
                            var Y = dataUDP.Get_m_worldPositionZ(Indextableau, _NumCar);
                            //Debug.WriteLine("(" + Indextableau + ") : X:" + X + " Y:" + Y);
                            s.Points.AddXY(-X,Y);
                            s.Points[s.Points.Count - 1].Color = Color.Black;
                            s.Points[s.Points.Count - 1].MarkerSize = 10;
                            s.Points[s.Points.Count - 1].MarkerStyle = MarkerStyle.Circle;
                            s.Points[s.Points.Count - 1].Label = //_NumCar+" : " +
                                dataUDP.Get_m_carPosition(Indextableau, _NumCar).ToString() +
                                " : " +
                                dataUDP.Get_m_s_name(0, _NumCar);
                        }
                    }
                    else
                        Debug.WriteLine("Erreur (" + Indextableau + ") : Distance :" + Distance);

                }
            }
        }

        private void AffichageGrid()//object sender, EventArgs e)
        {

            // https://docs.microsoft.com/fr-fr/dotnet/desktop/winforms/controls/walkthrough-creating-an-unbound-windows-forms-datagridview-control?view=netframeworkdesktop-4.8

            if (dataUDP.checkdata())
            {
                // divers x y z à remplacer
                if (toolStripMenuItem1.Checked)
                {
                    dGVdata.Rows.Clear();
                    dGVdata.Columns.Clear();
                    dGVdata.Columns.Add("TimeLap", "TimeLap");
                    dGVdata.Columns.Add("Position", "Position");
                    dGVdata.Columns.Add("Lap dist", "Lap dist");
                    dGVdata.Columns.Add("Speed", "Speed");
                    dGVdata.Columns.Add("x", "x");
                    dGVdata.Columns.Add("z", "z");

                    dGVdata.Columns.Add("vs TimeLap", "TimeLap");
                    dGVdata.Columns.Add("vs Position", "Position");
                    dGVdata.Columns.Add("vs vs Lap dist", "Lap dist");
                    dGVdata.Columns.Add("vs Speed", "Speed");
                    dGVdata.Columns.Add("vs x", "x");
                    dGVdata.Columns.Add("vs z", "z");

                    int IndexTour = 0;

                    for (int Indextableau = 0; Indextableau <  dataUDP.Get_ListMotion_packet_count() - 1; Indextableau++)
                    {
                        if (Indextableau > 1) 
                        {
                           if ((dataUDP.Get_m_currentLapNum(Indextableau - 1, NumCar) -
                                dataUDP.Get_m_currentLapNum(Indextableau,NumCar)) < 0)
                            // nouveau tour
                                IndexTour++;
                            
                            if (listViewLap.Items[IndexTour].Checked == true)
                            {
                                string[] rowData = {
                                            dataUDP.Get_m_sessionTime(Indextableau,NumCar).ToString(), //packetHeader.m_sessionTime
                                            dataUDP.Get_m_carPosition(Indextableau,NumCar).ToString(),
                                            dataUDP.Get_m_lapDistance(Indextableau,NumCar).ToString(),
                                            dataUDP.Get_m_speed(Indextableau,NumCar).ToString(),
                                            dataUDP.Get_m_worldPositionX(Indextableau,NumCar).ToString(),
                                            dataUDP.Get_m_worldPositionZ(Indextableau,NumCar).ToString(),

                                            dataUDP.Get_m_sessionTime(Indextableau,NumCar_vs).ToString(), //packetHeader.m_sessionTime
                                            dataUDP.Get_m_carPosition(Indextableau,NumCar_vs).ToString(),
                                            dataUDP.Get_m_lapDistance(Indextableau,NumCar_vs).ToString(),
                                            dataUDP.Get_m_speed(Indextableau,NumCar_vs).ToString(),
                                            dataUDP.Get_m_worldPositionX(Indextableau,NumCar_vs).ToString(),
                                            dataUDP.Get_m_worldPositionZ(Indextableau,NumCar_vs).ToString(),

                                        };
                                dGVdata.Rows.Add(rowData);
                            }
                        }
                    }
                }

                // Time by secteur / lap
                if (toolStripMenuItem2.Checked)
                    if (dataSetting.Simulation==0)
                    {
                        dGVdata.Rows.Clear();
                        dGVdata.Columns.Clear();
                        dGVdata.Columns.Add("LapNum", "LapNum");
                        dGVdata.Columns.Add("m_carIdx", "m_carIdx");
                        dGVdata.Columns.Add("m_bestLapTimeLapNum", "m_bestLapTimeLapNum");
                        dGVdata.Columns.Add("m_bestSector1LapNum", "m_bestSector1LapNum");
                        dGVdata.Columns.Add("m_bestSector2LapNum", "m_bestSector2LapNum");
                        dGVdata.Columns.Add("m_bestSector3LapNum", "m_bestSector3LapNum");
                        dGVdata.Columns.Add("m_lapTimeInMS", "m_lapTimeInMS");
                        dGVdata.Columns.Add("m_sector1TimeInMS", "m_sector1TimeInMS");
                        dGVdata.Columns.Add("m_sector2TimeInMS", "m_sector2TimeInMS");
                        dGVdata.Columns.Add("m_sector3TimeInMS", "m_sector3TimeInMS");

                        int Indextableau = dataUDP.Get_ListHistoric_count() - 1;
                        while (dataUDP.Get_m_carIdx(Indextableau) != NumCar && Indextableau>0)
                        {
                            Indextableau--;
                        }
                        for (int Lap = 0; Lap < 100; Lap++)
                        {
                            // uniquement pour F1
                            if (dataUDP.Get_m_carIdx(Indextableau) == NumCar)
                            {
                                //IndexData = dataf1_2021.ListHistoric[Indextableau].m_numLaps-1;
                                //IndexData = dataf1_2021.ListHistoric[Indextableau].m_bestLapTimeLapNum;
                            
                                string[] rowData = {
                                    Lap.ToString(),
                                    dataUDP.Get_m_carIdx(Indextableau).ToString(),
                                    dataUDP.Get_m_bestLapTimeLapNum(Indextableau).ToString(),
                                    dataUDP.Get_m_bestSector1LapNum(Indextableau).ToString(),
                                    dataUDP.Get_m_bestSector2LapNum(Indextableau).ToString(),
                                    dataUDP.Get_m_bestSector3LapNum(Indextableau).ToString(),

                                    TimeSpan.FromMilliseconds(dataUDP.Get_m_lapTimeInMS(Indextableau,Lap)).ToString(),
                                    TimeSpan.FromMilliseconds(dataUDP.Get_m_sector1TimeInMS(Lap,NumCar)).ToString(),
                                    TimeSpan.FromMilliseconds(dataUDP.Get_m_sector2TimeInMS(Lap,NumCar)).ToString(),
                                    TimeSpan.FromMilliseconds(dataUDP.Get_m_sector3TimeInMS(Lap,NumCar)).ToString()
                                };
                                dGVdata.Rows.Add(rowData);
                            }
                        }
                    }

                // liste des pilotes
                if (toolStripMenuItem3.Checked)
                {
                    dGVdata.Rows.Clear();
                    dGVdata.Columns.Clear();
                    dGVdata.Columns.Add("m_aiControlled", "m_aiControlled");
                    dGVdata.Columns.Add("Name", "Name");
                    dGVdata.Columns.Add("m_driverId", "m_driverId");
                    dGVdata.Columns.Add("m_teamId", "m_teamId");
                    dGVdata.Columns.Add("m_myTeam", "m_myTeam");
                    dGVdata.Columns.Add("m_raceNumber", "m_raceNumber");
                    dGVdata.Columns.Add("m_nationality", "m_nationality");
                    dGVdata.Columns.Add("m_yourTelemetry", "m_yourTelemetry");

                    int Indextableau = 0;
                    for (int IndexPlayer = 0; IndexPlayer < dataUDP.Get_NumofPlayers(); IndexPlayer++)
                    {
                        string[] rowData = {
                                dataUDP.Get_m_aiControlled(Indextableau,IndexPlayer).ToString(),
                                dataUDP.Get_m_s_name(Indextableau,IndexPlayer).ToString(),
                                dataUDP.Get_m_driverId(Indextableau,IndexPlayer).ToString(),
                                dataUDP.Get_m_teamId(Indextableau,IndexPlayer).ToString(),
                                dataUDP.Get_m_myTeam(Indextableau,IndexPlayer).ToString(),
                                dataUDP.Get_m_raceNumber(Indextableau,IndexPlayer).ToString(),
                                dataUDP.Get_m_nationality(Indextableau,IndexPlayer).ToString(),
                                dataUDP.Get_m_yourTelemetry(Indextableau,IndexPlayer).ToString(),
                            };
                        dGVdata.Rows.Add(rowData);
                    }
                }

                // performance par pilote
                if (toolStripMenuItem4.Checked) 
                {
                    dGVdata.Rows.Clear();
                    dGVdata.Columns.Clear();

                    dGVdata.Columns.Add("m_aiControlled", "m_aiControlled");
                    dGVdata.Columns.Add("Name", "Name");

                    dGVdata.Columns.Add("m_position", "m_position");
                    dGVdata.Columns.Add("m_numLaps", "m_numLaps");
                    dGVdata.Columns.Add("m_gridPosition", "m_gridPosition");
                    dGVdata.Columns.Add("m_bestLapTimeInMS", "m_bestLapTimeInMS");
                    dGVdata.Columns.Add("m_totalRaceTime", "m_totalRaceTime");
                    dGVdata.Columns.Add("m_numPitStops", "m_numPitStops");
                    dGVdata.Columns.Add("m_resultStatus", "m_resultStatus");

                    int Indextableau = 0; 
                    {
                        for(int IndexPlayer = 0; IndexPlayer < dataUDP.Get_NumofPlayers(); IndexPlayer++)
                        {
                            string[] rowData = {

                                    dataUDP.Get_m_aiControlled(Indextableau,IndexPlayer).ToString(),
                                    dataUDP.Get_m_s_name(Indextableau,IndexPlayer).ToString(),

                                    dataUDP.Get_m_position(Indextableau,IndexPlayer).ToString(),
                                    dataUDP.Get_m_numLaps(IndexPlayer).ToString(),
                                    dataUDP.Get_m_gridPosition(Indextableau,IndexPlayer).ToString(),


                                    TimeSpan.FromSeconds(dataUDP.Get_m_bestLapTimeInMS(Indextableau,IndexPlayer)).ToString(),
                                    TimeSpan.FromSeconds(dataUDP.Get_m_totalRaceTime(IndexPlayer)).ToString(),


                                    dataUDP.Get_m_numPitStops(Indextableau,IndexPlayer).ToString(),
                                    dataUDP.Get_m_resultStatus(Indextableau,IndexPlayer).ToString(),
                                };
                            dGVdata.Rows.Add(rowData);
                        }
                    }
                }

                // Analyse feedback
                if (toolStripMenuItem5.Checked)
                {
                    dGVdata.Rows.Clear();
                    dGVdata.Columns.Clear();
                    dGVdata.Columns.Add("Time", "Time");
                    dGVdata.Columns.Add("Code", "Code");
                    dGVdata.Columns.Add("Texte", "Texte");
                    dGVdata.Columns.Add("Détail", "Detail");

                    for (int Indextableau = 0; Indextableau <= dataUDP.Get_ListLapData_count() - 2; Indextableau++)
                    {
                        if (Indextableau< dataUDP.Get_ListEvent_count())
                        {
                            string[] rowData = {
                            TimeSpan.FromSeconds(   dataUDP.Get_m_sessionTime(Indextableau,1)).ToString(),
                            dataUDP.Get_m_Code(Indextableau),
                            dataUDP.Get_m_GetCode(Indextableau),
                            dataUDP.Get_Eventdetails(Indextableau)
                            };
                            if (dataUDP.Get_m_Code(Indextableau) != "BUTN")
                                dGVdata.Rows.Add(rowData);
                        }
                    }
                }


            }

        }

        private void findFlashback()
        {
            dGVdata.Rows.Clear();
            dGVdata.Columns.Clear();
            dGVdata.Columns.Add("Time", "Time");
            dGVdata.Columns.Add("Code", "Code");
            dGVdata.Columns.Add("Texte", "Texte");
            dGVdata.Columns.Add("Détail", "Detail");
            dGVdata.Columns.Add("lap", "lap");

            for (int Indextableau = 0; Indextableau <= dataUDP.Get_ListLapData_count() - 2; Indextableau++)
            {
                if (Indextableau < dataUDP.Get_ListEvent_count())
                {
                    if (dataUDP.Get_m_Code(Indextableau) == "FLBK")
                    {
                        int _lap = 0;
                        int Indextableau2 = 0;
                        while ( Indextableau2 < dataUDP.Get_ListLapData_count() - 1) 
                        {
                            Indextableau2++;
                            if (dataUDP.Get_m_lapDistance(Indextableau2,NumCar) > 0)
                            {
                                if (Indextableau2 > 1)
                                    if ((dataUDP.Get_m_currentLapNum(Indextableau2-1, NumCar) -
                                        dataUDP.Get_m_currentLapNum(Indextableau2, NumCar))<0)
                                    // nouveau tour
                                        _lap++;
                                if (dataUDP.Get_m_sessionTime(Indextableau,1) < dataUDP.Get_m_sessionTime(Indextableau, 0))
                                    break;
                            }
                        }
                        _lap++;
                        ListFlashback.Add(new Flashback()
                        {
                            flashbackFrameIdentifier = 0,
                            flashbackSessionTime = 0,
                            m_sessionTime = dataUDP.Get_m_sessionTime(Indextableau, 1),
                            lap = _lap,
                        });;
                        string[] rowData = {
                                TimeSpan.FromSeconds(dataUDP.Get_m_sessionTime(Indextableau, 1)).ToString(),
                                dataUDP.Get_m_Code(Indextableau),
                                dataUDP.Get_m_GetCode(Indextableau),
                                dataUDP.Get_Eventdetails(Indextableau),
                                _lap.ToString()
                                };
                        dGVdata.Rows.Add(rowData);
                    }
                }
            }
        }

        private void GestionTab()
        {
            hScrollBarLap.Visible = false;

            if (VL != null)
                VL.Visible = false;

            if (dataUDP.checkdata())
            {
                switch (tabControlTelemetry.TabPages[tabControlTelemetry.SelectedIndex].Text)
                {
                    case "Raw Data":
                        CMAPsmall.Visible = true;
                        AffichageGrid();//sender, e);
                        break;
                    case "Map":
                        CMAPsmall.Visible = false;
                        AffichageMap(cMAP);//sender, e);
                        break;
                    case "Map 3D":
                        CMAPsmall.Visible = false;
                        AffichageMap3d();
                        break;
                    case "Comp": 
                        CMAPsmall.Visible = true;
                        AffichageMultiGraph();
                        break;
                    case "Telemetrie":
                        CMAPsmall.Visible = true;
                        AffichageGraphTelemetry();
                        break;
                    default:
                        CMAPsmall.Visible = true;
                        AffichageMultiGraph();
                        break;
                }
            }
        }

        private void tabControlTelemetry_Click(object sender, EventArgs e)
        {
            GestionTab();
        }


        private void chartMultiGraph_MouseWheel(object sender, MouseEventArgs e)
        {
            Chart chart = (Chart)sender;

            // Obtenez la position du curseur dans l'espace du graphique
            Point cursorPos = e.Location;
            double cursorXValue = chart.ChartAreas[0].AxisX.PixelPositionToValue(cursorPos.X);

            // Déterminez le facteur de zoom
            double zoomFactor = 1.2; // Vous pouvez ajuster cela selon vos besoins

            // Zoom avant avec la molette vers le haut
            if (e.Delta > 0)
            {
                chart.ChartAreas[0].AxisX.ScaleView.Zoom(cursorXValue - (cursorXValue - chart.ChartAreas[0].AxisX.ScaleView.ViewMinimum) / zoomFactor, cursorXValue + (chart.ChartAreas[0].AxisX.ScaleView.ViewMaximum - cursorXValue) / zoomFactor);
            }
            // Zoom arrière avec la molette vers le bas
            else if (e.Delta < 0)
            {
                chart.ChartAreas[0].AxisX.ScaleView.Zoom(cursorXValue - (cursorXValue - chart.ChartAreas[0].AxisX.ScaleView.ViewMinimum) * zoomFactor, cursorXValue + (chart.ChartAreas[0].AxisX.ScaleView.ViewMaximum - cursorXValue) * zoomFactor);
            }

        }
        private void chart_MouseMove(object sender, MouseEventArgs e)
        {
            // appelé pour donner des informations sur la position de la souris en fonction du graphique
            Chart chart;

            chart = (Chart)sender;

            chartSeriesMouseMove(chart, sender, e);
        }

        private void chartSeriesMouseMove(Chart chart, object sender, MouseEventArgs e)
        {
            // https://stackoverflow.com/questions/52104306/using-mouseevents-to-change-chart-series-appearance-in-c-sharp-winforms

            // Mise en valeur de la série avec la souris
            // à étudier 
            // https://social.msdn.microsoft.com/Forums/Lync/en-US/6768ed48-027e-4162-8180-16721ec6559d/how-to-display-information-of-a-line-chart-by-doing-mouse-over?forum=vbgeneral

            // gestion des couleurs des tracés des tours
            if (dataUDP.checkdata())
            {
                HitTestResult result = chart.HitTest(e.X, e.Y);

                if (result != null && result.Object != null)
                {
                    if (result.ChartElementType == ChartElementType.LegendItem)
                    {
                        string selseries = result.Series.Name;

                        if (PreviousselectedSeries != "")
                        {
                            // retablisement des paramêtres précédents
                            chart.Series[PreviousselectedSeries].Color = previouscolor;
                            chart.Series[PreviousselectedSeries].BorderWidth = PreviousBorderWidth;
                        }
                        PreviousselectedSeries = selseries;
                        previouscolor = chart.Series[selseries].Color;
                        PreviousBorderWidth = chart.Series[selseries].BorderWidth;

                        chart.Series[selseries].Color = Color.Red;
                        chart.Series[selseries].BorderWidth = AnalyseBorderWidth;
                    }
                }
                else
                {
                    if (PreviousselectedSeries != "")
                    {
                        // retablisement des paramêtres précédents
                        chart.Series[PreviousselectedSeries].Color = previouscolor;
                        chart.Series[PreviousselectedSeries].BorderWidth = PreviousBorderWidth;
                        PreviousselectedSeries = "";

                    }
                }
                chartMultiGraph.Invalidate(); // Redessinez le graphique

            }

             // Gestion des info sur les graphiques
            if (chart.Name != "chartMultiGraph")
            {
                // Affichage des infos sur le graphique
                var pos = e.Location;

                if (prevPosition.HasValue && pos == prevPosition.Value)
                    return;
                tooltip.RemoveAll();
                prevPosition = pos;
                var results = chart.HitTest(pos.X, pos.Y, false,
                                             ChartElementType.PlottingArea);
                try
                {
                    foreach (var result in results)
                    {
                        if (result.ChartElementType == ChartElementType.PlottingArea)
                        {
                            var xVal = Math.Round(result.ChartArea.AxisX.PixelPositionToValue(pos.X), 3);
                            var yVal = Math.Round(result.ChartArea.AxisY.PixelPositionToValue(pos.Y), 3);
                            Xc = xVal;
                            Yc = yVal;

                            tooltip.Show(tooltipX + xVal + tooltipY + yVal, chart,
                                         pos.X + 5, pos.Y - 15 + 5);
                            Global_xval = xVal;
                            Global_yval = yVal;
                        }
                    }
                }
                catch
                {
                    tBetat.Text = "chartSeriesMouseMove - Erreur lors de la récupération Xc et Yc ";
                }
            }
            else
            {
               // tBetat.Text = VL.X.ToString();//chart.Name;
            }

        }

        private void chartMultiGraph_Paint(object sender, PaintEventArgs e)
        {
            AffichageMultiGraph_Paint(sender, e);
        }

        private void chartMultiGraph_AnnotationPositionChanged(object sender, EventArgs e)
        {
            chartMultiGraph.Invalidate();
        }

        private void chartMultiGraph_AnnotationPositionChanging(object sender, AnnotationPositionChangingEventArgs e)
        {
            chartMultiGraph.Invalidate();
        }

        private void chartMultiGraph_AxisViewChanged(object sender, ViewEventArgs e)
        {
            AffichageMap(CMAPsmall);
        }

        private void checkAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i=0;i< listViewLap.Items.Count;i++)
            {
                listViewLap.Items[i].Checked = true;
            }
        }

        private void checkRemoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listViewLap.Items.Count; i++)
            {
                listViewLap.Items[i].Checked = false;
            }
        }

        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //https://stackoverflow.com/questions/13437889/showing-a-context-menu-for-an-item-in-a-listview
            var focusedItem = listViewLap.FocusedItem;
            if (cDset.ShowDialog() == DialogResult.OK)
            {
                if ((focusedItem.Index < chartMultiGraph.Series.Count) & (focusedItem.Index < listViewLap.Items.Count))
                {
                    listViewLap.Items[focusedItem.Index].BackColor = cDset.Color;
                    chartMultiGraph.Series[focusedItem.Index].Color = cDset.Color;
                    LocalTrackColor[focusedItem.Index] = cDset.Color;
                }
            }
        }
        private void checkAllToolStripMenuItemVS_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listViewLapVS.Items.Count; i++)
            {
                listViewLapVS.Items[i].Checked = true;
            }
        }

        private void checkRemoveToolStripMenuItemVS_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listViewLapVS.Items.Count; i++)
            {
                listViewLapVS.Items[i].Checked = false;
            }
        }

        private void colorToolStripMenuItemVS_Click(object sender, EventArgs e)
        {
            //https://stackoverflow.com/questions/13437889/showing-a-context-menu-for-an-item-in-a-listview
            var focusedItem = listViewLapVS.FocusedItem;
            if (cDset.ShowDialog() == DialogResult.OK)
            {
                if ((focusedItem.Index< chartMultiGraph.Series.Count) & (focusedItem.Index< listViewLapVS.Items.Count))
                {
                    listViewLapVS.Items[focusedItem.Index].BackColor = cDset.Color;
                    chartMultiGraph.Series[focusedItem.Index].Color = cDset.Color;
                    LocalTrackColorVS[focusedItem.Index] = cDset.Color;
                }
            }
        }

        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            if (cBhighLight.Checked)
            if (dataUDP.checkdata())
            {
                ListViewHitTestInfo result = listViewLap.HitTest(e.X, e.Y);

                if (result != null && result.Item != null)
                {
                    int selseries = result.Item.Index;

                    if (PreviousselectedSeriesIndex != -1)
                    {
                            // retablisement des paramêtres précédents
                            chartMultiGraph.Series[PreviousselectedSeriesIndex].Color = previouscolor;
                            chartMultiGraph.Series[PreviousselectedSeriesIndex].BorderWidth = PreviousBorderWidth;
                            //tBetat.Text = "retablisement1 : previouscolor:" + previouscolor + " PreviousBorderWidth:" + PreviousBorderWidth;
                    }
                    PreviousselectedSeriesIndex = selseries;
                    previouscolor = chartMultiGraph.Series[selseries].Color;
                    PreviousBorderWidth = chartMultiGraph.Series[selseries].BorderWidth;

                    chartMultiGraph.Series[selseries].Color = Color.Red;
                    chartMultiGraph.Series[selseries].BorderWidth = AnalyseBorderWidth;
                }
                else
                {
                    if (PreviousselectedSeriesIndex != -1)
                    {
                            // retablisement des paramêtres précédents
                        chartMultiGraph.Series[PreviousselectedSeriesIndex].Color = previouscolor;
                        chartMultiGraph.Series[PreviousselectedSeriesIndex].BorderWidth = PreviousBorderWidth;
                        PreviousselectedSeriesIndex = -1;
                        //tBetat.Text = "Retablisement2 : previouscolor:" + previouscolor + " PreviousBorderWidth:" + PreviousBorderWidth;

                    }
                }
            }
        }

        private void cBdrivers_SelectedIndexChanged(object sender, EventArgs e)
        {
            NumCar = cBdrivers.SelectedIndex; // Int32.Parse(tBcar.Text);
            NumCar_vs = cBdriversVS.SelectedIndex; // Int32.Parse(tBcar.Text);

            //tBetat.Text = "NumCar : " + NumCar + "NumCar_vs : " + NumCar_vs;

            InitLegend();
            GestionTab();
        }

        private void cBdriversVS_SelectedIndexChanged(object sender, EventArgs e)
        {
            NumCar = cBdrivers.SelectedIndex; // Int32.Parse(tBcar.Text);
            NumCar_vs = cBdriversVS.SelectedIndex; // Int32.Parse(tBcar.Text);
            GestionTab();
        }

        private void brakeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)CMmap.Items[0]).Checked)
                ((ToolStripMenuItem)CMmap.Items[1]).Checked = false;
            else
                ((ToolStripMenuItem)CMmap.Items[1]).Checked = true;

            tBetat.Text = "0 : " + ((ToolStripMenuItem)CMmap.Items[0]).Checked.ToString() + "1 : " + ((ToolStripMenuItem)CMmap.Items[1]).Checked.ToString();

            GestionTab();
        }

        private void speedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)CMmap.Items[1]).Checked)
                ((ToolStripMenuItem)CMmap.Items[0]).Checked = false;
            else
                ((ToolStripMenuItem)CMmap.Items[0]).Checked = true;

            //tBetat.Text = "0 : " + ((ToolStripMenuItem)CMmap.Items[0]).Checked.ToString() + "1 : " + ((ToolStripMenuItem)CMmap.Items[1]).Checked.ToString();

            GestionTab();
        }

        private void toolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Traitement du menu contextuel
            for (int i = 0; i < CMrawdata.Items.Count; i++)
            {
                // Vérification si ce n'est pas un séparateur (false)
                if (CMrawdata.Items[i].CanSelect)
                {
                    ((ToolStripMenuItem)CMrawdata.Items[i]).Checked = false;
                    if (((ToolStripMenuItem)CMrawdata.Items[i]).Text == ((ToolStripMenuItem)sender).Text)
                        ((ToolStripMenuItem)CMrawdata.Items[i]).Checked = true;
                }
            }

            GestionTab();
        }

        private void findFlashbackLapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            findFlashback();
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            for(int i=0; i< listViewLap.Items.Count;i++) // )
            {
                for (int j = 0; j < ListFlashback.Count; j++)
                    if (ListFlashback[j].lap == (i+1))
                        listViewLap.Items[i].Checked = false;
            }
        }

        private void positionsPilotesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Analyse_position_pilote(0,cMAP,0) ; 
        }

        private void synchroDistanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            synchroDistance();
        }

        private void synchroDistance()
        {
            if (tabControlTelemetry.TabPages[tabControlTelemetry.SelectedIndex].Text == "Comp")
            {
                tBetat.Text = "X="+Global_xval+" Y="+Global_yval;
            }

        }

        private void CMgraph_Opening(object sender, CancelEventArgs e)
        {
            // desactive le suivi de souris pour garder les informations
            chartMultiGraph.MouseMove -= chart_MouseMove;


        }

        private void CMgraph_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            // reactive le suivi de souris pour garder les informations
            chartMultiGraph.MouseMove += chart_MouseMove;
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            string name;
            int NbTrack = 0;
            int NbTrackChecked = 0;
            int indexdatalap = 0;
            double X, Y = 0, X_vs, Y_vs = 0;
            int coef = 7;


            void _ajoutdata()
            {
                // on débute à 1 car pb sur la valeur à 0, m_lapDistance = longueur du circuit
                for (int Indextableau = 1; Indextableau < Math.Min(dataUDP.Get_ListTelemetry_count() - 1, dataUDP.Get_ListLapData_count()); Indextableau++)
                {
                    if (dataUDP.Get_m_lapDistance(Indextableau,NumCar) > 0)
                    {
                        if (Indextableau > 1)
                            if ((   dataUDP.Get_m_currentLapNum(Indextableau-1,NumCar) - 
                                    dataUDP.Get_m_currentLapNum(Indextableau, NumCar)) < 0)
                            // nouveau tour
                            {
                                NbTrack++;
                                indexdatalap = 0;
                                if (listViewLap.Items[NbTrack].Checked == true)
                                {
                                    NbTrackChecked++;

                                    dGVdata.Columns.Add("LapData [" + (NbTrack + 1) + "]", "LapData [" + (NbTrack + 1) + "]");
                                    dGVdata.Columns.Add("Telemetry [" + (NbTrack + 1) + "]", "Telemetry [" + (NbTrack + 1) + "]");
                                    dGVdata.Columns.Add("Motion [" + (NbTrack + 1) + "]", "Motion [" + (NbTrack + 1) + "]");

                                    dGVdata.Columns.Add("Distance [" + (NbTrack + 1) + "]", "Distance[" + (NbTrack + 1) + "]");
                                    dGVdata.Columns.Add("Speed [" + (NbTrack + 1) + "]", "Speed[" + (NbTrack + 1) + "]");
                                    dGVdata.Columns.Add("X [" + (NbTrack + 1) + "]", "X [" + (NbTrack + 1) + "]");
                                    dGVdata.Columns.Add("Z [" + (NbTrack + 1) + "]", "Z [" + (NbTrack + 1) + "]");

                                }

                                // synchro
                                // si pilote 1 devant pilote2 (vs) alors augmenter Indextableau_vs
                                // si pilote 1 derriére pilote2 (vs) alors diminuer Indextableau_vs
                                // à travailler plus tard Indextableau_vs = Synchro(Indextableau);
                            }

                        if (listViewLap.Items[NbTrack].Checked == true)
                        {
                            switch (name)
                            {
                                case "m_speed": // multiG
                                    Y = dataUDP.Get_m_speed(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_speed(Indextableau, NumCar_vs);
                                    break;
                                case "m_engineRPM": // multiG
                                    Y = dataUDP.Get_m_engineRPM(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_engineRPM(Indextableau, NumCar_vs);
                                    break;
                                case "m_steer": // multiG
                                    Y = dataUDP.Get_m_steer(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_steer(Indextableau, NumCar_vs);
                                    break;
                                case "m_gear": // multiG
                                    Y = dataUDP.Get_m_gear(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_gear(Indextableau, NumCar_vs);
                                    break;
                                case "m_drs": // multiG
                                    Y = dataUDP.Get_m_drs(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_drs(Indextableau, NumCar_vs);
                                    break;
                                case "m_brakesTemperature[0]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_brakesTemperature(Indextableau, NumCar, 0);
                                    Y_vs = dataUDP.Get_m_brakesTemperature(Indextableau, NumCar_vs, 0);
                                    break;
                                case "m_brakesTemperature[1]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_brakesTemperature(Indextableau, NumCar, 1);
                                    Y_vs = dataUDP.Get_m_brakesTemperature(Indextableau, NumCar_vs, 1);
                                    break;
                                case "m_brakesTemperature[2]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_brakesTemperature(Indextableau, NumCar, 2);
                                    Y_vs = dataUDP.Get_m_brakesTemperature(Indextableau, NumCar_vs, 2);
                                    break;
                                case "m_brakesTemperature[3]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_brakesTemperature(Indextableau, NumCar, 3);
                                    Y_vs = dataUDP.Get_m_brakesTemperature(Indextableau, NumCar_vs, 3);
                                    break;
                                case "m_tyresSurfaceTemperature[0]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_tyresSurfaceTemperature(Indextableau, NumCar, 0);
                                    Y_vs = dataUDP.Get_m_tyresSurfaceTemperature(Indextableau, NumCar_vs, 0);
                                    break;
                                case "m_tyresSurfaceTemperature[1]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_tyresSurfaceTemperature(Indextableau, NumCar, 1);
                                    Y_vs = dataUDP.Get_m_tyresSurfaceTemperature(Indextableau, NumCar_vs, 1);
                                    break;
                                case "m_tyresSurfaceTemperature[2]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_tyresSurfaceTemperature(Indextableau, NumCar, 2);
                                    Y_vs = dataUDP.Get_m_tyresSurfaceTemperature(Indextableau, NumCar_vs, 2);
                                    break;
                                case "m_tyresSurfaceTemperature[3]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_tyresSurfaceTemperature(Indextableau, NumCar, 3);
                                    Y_vs = dataUDP.Get_m_tyresSurfaceTemperature(Indextableau, NumCar_vs, 3);
                                    break;
                                case "m_tyresInnerTemperature[0]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_tyresInnerTemperature(Indextableau, NumCar, 0);
                                    Y_vs = dataUDP.Get_m_tyresInnerTemperature(Indextableau, NumCar_vs, 0);
                                    break;
                                case "m_tyresInnerTemperature[1]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_tyresInnerTemperature(Indextableau, NumCar, 1);
                                    Y_vs = dataUDP.Get_m_tyresInnerTemperature(Indextableau, NumCar_vs, 1);
                                    break;
                                case "m_tyresInnerTemperature[2]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_tyresInnerTemperature(Indextableau, NumCar, 2);
                                    Y_vs = dataUDP.Get_m_tyresInnerTemperature(Indextableau, NumCar_vs, 2);
                                    break;
                                case "m_tyresInnerTemperature[3]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_tyresInnerTemperature(Indextableau, NumCar, 3);
                                    Y_vs = dataUDP.Get_m_tyresInnerTemperature(Indextableau, NumCar_vs, 3);
                                    break;
                                case "m_engineTemperature": // multiG
                                    Y = dataUDP.Get_m_engineTemperature(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_engineTemperature(Indextableau, NumCar_vs);
                                    break;
                                case "m_tyresPressure[0]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_tyresPressure(Indextableau, NumCar, 0);
                                    Y_vs = dataUDP.Get_m_tyresPressure(Indextableau, NumCar_vs, 0);
                                    break;
                                case "m_tyresPressure[1]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_tyresPressure(Indextableau, NumCar, 1);
                                    Y_vs = dataUDP.Get_m_tyresPressure(Indextableau, NumCar_vs, 1);
                                    break;
                                case "m_tyresPressure[2]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_tyresPressure(Indextableau, NumCar, 2);
                                    Y_vs = dataUDP.Get_m_tyresPressure(Indextableau, NumCar_vs, 2);
                                    break;
                                case "m_tyresPressure[3]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_tyresPressure(Indextableau, NumCar, 3);
                                    Y_vs = dataUDP.Get_m_tyresPressure(Indextableau, NumCar_vs, 3);
                                    break;
                                case "m_surfaceType[0]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_surfaceType(Indextableau, NumCar, 0);
                                    Y_vs = dataUDP.Get_m_surfaceType(Indextableau, NumCar_vs, 0);
                                    break;
                                case "m_surfaceType[1]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_surfaceType(Indextableau, NumCar, 1);
                                    Y_vs = dataUDP.Get_m_surfaceType(Indextableau, NumCar_vs, 1);
                                    break;
                                case "m_surfaceType[2]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_surfaceType(Indextableau, NumCar, 2);
                                    Y_vs = dataUDP.Get_m_surfaceType(Indextableau, NumCar_vs, 2);
                                    break;
                                case "m_surfaceType[3]": // multiG - TABLEAU A REVOIR
                                    Y = dataUDP.Get_m_surfaceType(Indextableau, NumCar, 3);
                                    Y_vs = dataUDP.Get_m_surfaceType(Indextableau, NumCar_vs, 3);
                                    break;
                                case "m_revLightsBitValue": // multiG
                                    Y = dataUDP.Get_m_revLightsBitValue(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_revLightsBitValue(Indextableau, NumCar_vs);
                                    break;
                                case "m_revLightsPercent": // multiG
                                    Y = dataUDP.Get_m_revLightsPercent(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_revLightsPercent(Indextableau, NumCar_vs);
                                    break;
                                case "m_brake": // multiG
                                    Y = dataUDP.Get_m_brake(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_brake(Indextableau, NumCar_vs);
                                    break;
                                case "m_throttle": // multiG
                                    Y = dataUDP.Get_m_throttle(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_throttle(Indextableau, NumCar_vs);
                                    break;
                                case "m_clutch": // multiG
                                    Y = dataUDP.Get_m_clutch(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_clutch(Indextableau, NumCar_vs);
                                    break;
                                case "m_yaw": // multiG
                                    Y = dataUDP.Get_m_yaw(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_yaw(Indextableau, NumCar_vs);
                                    break;
                                case "m_pitch": // multiG
                                    Y = dataUDP.Get_m_pitch(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_pitch(Indextableau, NumCar_vs);
                                    break;
                                case "m_roll": // multiG
                                    Y = dataUDP.Get_m_roll(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_roll(Indextableau, NumCar_vs);
                                    break;
                                case "m_gForceLateral": // multiG
                                    Y = dataUDP.Get_m_gForceLateral(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_gForceLateral(Indextableau, NumCar_vs);
                                    break;
                                case "m_gForceLongitudinal": // multiG
                                    Y = dataUDP.Get_m_gForceLongitudinal(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_gForceLongitudinal(Indextableau, NumCar_vs);
                                    break;
                                case "m_gForceVertical": // multiG
                                    Y = dataUDP.Get_m_gForceVertical(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_gForceVertical(Indextableau, NumCar_vs);
                                    break;
                                case "m_worldPositionX": // multiG
                                    Y = dataUDP.Get_m_worldPositionX(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_worldPositionX(Indextableau, NumCar_vs);
                                    break;
                                case "m_worldPositionY": // multiG
                                    Y = dataUDP.Get_m_worldPositionY(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_worldPositionY(Indextableau, NumCar_vs);
                                    break;
                                case "m_worldPositionZ": // multiG
                                    Y = dataUDP.Get_m_worldPositionZ(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_worldPositionZ(Indextableau, NumCar_vs);
                                    break;
                                case "m_worldVelocityX": // multiG
                                    Y = dataUDP.Get_m_worldVelocityX(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_worldVelocityX(Indextableau, NumCar_vs);
                                    break;
                                case "m_worldVelocityY": // multiG
                                    Y = dataUDP.Get_m_worldVelocityY(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_worldVelocityY(Indextableau, NumCar_vs);
                                    break;
                                case "m_worldVelocityZ": // multiG
                                    Y = dataUDP.Get_m_worldVelocityZ(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_worldVelocityZ(Indextableau, NumCar_vs);
                                    break;
                                case "m_worldForwardDirX": // multiG
                                    Y = dataUDP.Get_m_worldForwardDirX(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_worldForwardDirX(Indextableau, NumCar_vs);
                                    break;
                                case "m_worldForwardDirY": // multiG
                                    Y = dataUDP.Get_m_worldForwardDirX(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_worldForwardDirX(Indextableau, NumCar_vs);
                                    break;
                                case "m_worldForwardDirZ": // multiG
                                    Y = dataUDP.Get_m_worldForwardDirX(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_worldForwardDirX(Indextableau, NumCar_vs);
                                    break;
                                case "m_worldRightDirX": // multiG
                                    Y = dataUDP.Get_m_worldRightDirX(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_worldRightDirX(Indextableau, NumCar_vs);
                                    break;
                                case "m_worldRightDirY": // multiG
                                    Y = dataUDP.Get_m_worldRightDirY(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_worldRightDirY(Indextableau, NumCar_vs);
                                    break;
                                case "m_worldRightDirZ": // multiG
                                    Y = dataUDP.Get_m_worldRightDirZ(Indextableau, NumCar);
                                    Y_vs = dataUDP.Get_m_worldRightDirZ(Indextableau, NumCar_vs);
                                    break;
                            }

                            if (NbTrackChecked == 0)
                                dGVdata.Rows.Add(indexdatalap.ToString());

                            if (indexdatalap > dGVdata.RowCount - 1)
                                dGVdata.Rows.Add(indexdatalap.ToString());

                            X = dataUDP.Get_m_lapDistance(Indextableau, NumCar);
                            X = dataUDP.Get_m_lapDistance(Indextableau, NumCar);
                            X_vs = dataUDP.Get_m_lapDistance(Indextableau, NumCar_vs);

                            // Edition d'une cellule => dataGridView1[colonne,ligne]
                            dGVdata[1 + NbTrackChecked * coef, indexdatalap].Value = dataUDP.Get_m_sessionTime(Indextableau,0).ToString();
                            dGVdata[2 + NbTrackChecked * coef, indexdatalap].Value = dataUDP.Get_m_sessionTime(Indextableau, 2).ToString();
                            dGVdata[3 + NbTrackChecked * coef, indexdatalap].Value = dataUDP.Get_m_sessionTime(Indextableau, 3).ToString();

                            dGVdata[4 + NbTrackChecked * coef, indexdatalap].Value = dataUDP.Get_m_lapDistance(Indextableau, NumCar).ToString();
                            dGVdata[5 + NbTrackChecked * coef, indexdatalap].Value = dataUDP.Get_m_speed(Indextableau, NumCar).ToString();
                            dGVdata[6 + NbTrackChecked * coef, indexdatalap].Value = dataUDP.Get_m_worldPositionX(Indextableau,NumCar).ToString();
                            dGVdata[7 + NbTrackChecked * coef, indexdatalap].Value = dataUDP.Get_m_worldPositionY(Indextableau, NumCar).ToString();

                            indexdatalap++;
                        }
                    }
                }
            }

            dGVdata.Rows.Clear();
            dGVdata.Columns.Clear();


            if (listViewData.CheckedItems.Count != 0)
            {
                // initialisation des colonnes
                dGVdata.Columns.Add("index", "index");
                dGVdata.Columns.Add("LapData [1]", "LapData [1]");
                dGVdata.Columns.Add("Telemetry [1]", "Telemetry [1]");
                dGVdata.Columns.Add("Motion [1]", "Motion [1]");
                dGVdata.Columns.Add("Distance [1]", "Distance [1]");
                dGVdata.Columns.Add("Speed [1]", "Speed [1]");
                dGVdata.Columns.Add("X [1]", "X [1]");
                dGVdata.Columns.Add("Z [1]", "Z [1]");

                name = listViewData.CheckedItems[0].Text;
                _ajoutdata();

                /*
                for (int x = 0; x < listViewData.CheckedItems.Count; x++)
                {
                    name = listViewData.CheckedItems[x].Text;
                    dGVdata.Columns.Add(name,name);
                }

                for (int x = 0; x < listViewData.CheckedItems.Count; x++)
                {
                    name =  listViewData.CheckedItems[x].Text;
                    _ajoutdata();
                }
                */
            }
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            //DatatoExcel();
            ExporterDataGridVersExcel(dGVdata, dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata + "\\data.xlsx","Entête") ;
        }
        private void DatatoExcel()
        {
            // https://electronique71.com/export-datagridview-vers-excel-avec-enregistrement-du-fichier-excel/

            var excelApp = new Excel.Application();

            excelApp.Visible = true;

            Excel._Worksheet workBooks = (Excel.Worksheet)excelApp.ActiveSheet;

            //Ouverture du fichier Excel, à vous de choisir l'emplacement ou est situé le fichier excel ainsi que son nom!!

            Microsoft.Office.Interop.Excel._Workbook workbook = excelApp.Workbooks.Open(dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata+ "\\ExportData.xlsx");// @"C:\Users\Julien\Desktop\Classeur1.xlsx");

            workBooks = workbook.Sheets["Feuil1"]; // On sélectionne la Feuil1

            workBooks = workbook.ActiveSheet;

            workBooks.Name = "TelemetrieData"; // on renomme la Feuil1 

            dGVdata.RowHeadersVisible = false;

            for (int Rows = 1; Rows < dGVdata.Columns.Count + 1; Rows++)
            {
                workBooks.Cells[1, Rows] = dGVdata.Columns[Rows - 1].HeaderText;
            }

            // on recopie toutes les valeurs du DataGridView dans le fichier Excel

            for (int Rows = 0; Rows < dGVdata.Rows.Count - 1; Rows++)
            {

                for (int Columns = 0; Columns < dGVdata.Columns.Count; Columns++)
                {

                    workBooks.Cells[Rows + 2, Columns + 1] = dGVdata.Rows[Rows].Cells[Columns].Value.ToString();

                }
            }

            // sauvegarde du fichier Excel (volontairement j'ai créer un dossier sur le bureau nommé Electronique71 
            // puis dans ce dossier j'ai renommé le classeur Excel "Classeur1.xlsx" sous le nom "Fichier")

            workbook.SaveAs(dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            // Fermeture de l'application Excel
            excelApp.Quit();
        }
        private void DataToExcelviaClipboard()
        {
            DatatoClipboard();
            Microsoft.Office.Interop.Excel.Application xlexcel;
            Microsoft.Office.Interop.Excel.Workbook xlWorkBook;
            Microsoft.Office.Interop.Excel.Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;
            xlexcel = new Excel.Application();
            xlexcel.Visible = true;
            xlWorkBook = xlexcel.Workbooks.Add(misValue);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            Excel.Range CR = (Excel.Range)xlWorkSheet.Cells[1, 1];
            CR.Select();
            xlWorkSheet.PasteSpecial(CR, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, true);
        }
        private void copieVersPressepapierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DatatoClipboard();
        }
        private void DatatoClipboard()
        {
            // https://askcodez.com/comment-exporter-datagridview-instantanement-les-donnees-a-excel-sur-cliquez-sur-le-bouton.html
            dGVdata.SelectAll();
            DataObject dataObj = dGVdata.GetClipboardContent();
            if (dataObj != null)
                Clipboard.SetDataObject(dataObj);
        }
        private void exportVersExcelPressepapierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataToExcelviaClipboard();
        }

        private void ExporterDataGridVersExcel(DataGridView dgView, String unFichier, string strEnteteDeFichier)
        {
            // https://codes-sources.commentcamarche.net/source/39038-exporter-un-datagridview-vers-excel
            ///<summary>
            ///Permet d'exporter un DataGrid vers excel
            ///</summary>
            /// <param name="dgView">Data Grid Source des données à Exporter vers Excel</param>
            ///<param name="unFichier">Fichier Excel de destination des données</param>
            ///<param name="strEnteteDeFichier">Libellé de l'en-tête du fichier à générer</param>

            int i = 0;
            int j = 0;
            Microsoft.Office.Interop.Excel.Application excel;
            Microsoft.Office.Interop.Excel.Workbook exbook;
            Microsoft.Office.Interop.Excel.Worksheet exsheet;
            Microsoft.Office.Interop.Excel.Range r;
            try
            {
                excel = new Excel.Application();
                exbook = (Microsoft.Office.Interop.Excel.Workbook)excel.Workbooks.Add(Missing.Value);
                exsheet = (Microsoft.Office.Interop.Excel.Worksheet)excel.ActiveSheet;
                //Double[] Totaux= new Double[4];

                //Mise en forme de l'en-tête de la feuille Excel
                exsheet.Cells[1, 1] = strEnteteDeFichier;
                r = exsheet.get_Range(Convert.ToChar(65 + i).ToString() + "1", Missing.Value);
                //r.Interior.ColorIndex = XlColorIndex.xlColorIndexAutomatic;
                r.Font.Bold = true;
                //r.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic, Missing.Value);
                r.EntireColumn.AutoFit();//Fin de la mise en forme de l'en-tête.

                foreach (DataGridViewColumn ch in dgView.Columns)
                {
                    r = exsheet.get_Range(Convert.ToChar(65 + i).ToString() + "1", Missing.Value);
                    exsheet.Cells[2, i + 1] = ch.Name.Trim();
                    //r.Interior.ColorIndex = XlColorIndex.xlColorIndexAutomatic;
                    r.Font.Bold = true;
                    //r.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic, Missing.Value);
                    r.EntireColumn.AutoFit();
                    i++;
                }
                j = 3;

                foreach (DataGridViewRow uneLigne in dgView.Rows)
                {
                    i = 1;
                    foreach (DataGridViewColumn uneColonne in dgView.Columns)
                    {
                        r = exsheet.get_Range(Convert.ToChar(65 + i - 1).ToString() + j.ToString(), Missing.Value);
                        exsheet.Cells[j, i] = "'" + uneLigne.Cells[uneColonne.Name].Value.ToString().Trim();
                        //r.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic, Missing.Value);
                        r.EntireColumn.AutoFit();
                        i++;
                    }
                    exsheet.Columns.AutoFit();
                    j++;
                }
                exsheet.SaveAs(unFichier, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                excel.Quit();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }//ExporterDataGridVersExcel

        private void piloteDynamiqueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Analyse_position_pilote(1,cMAP,0);
            hScrollBarLap.Visible = true;
        }

        private void hScrollBarLap_MouseLeave(object sender, EventArgs e)
        {
            Analyse_position_pilote(1,cMAP,0);
        }

        private void SaveListLapSelected_Click(object sender, EventArgs e)
        {
            // https://stackoverflow.com/questions/63831416/how-to-save-list-to-binary-file

            string NameFile = "*.cfg";
            Boolean EcritureOK = true;

            NameFile = DateTime.Now.Year.ToString() +
                DateTime.Now.Month.ToString() +
                DateTime.Now.Day.ToString() +
                DateTime.Now.Hour.ToString() + "_" +
                DateTime.Now.Minute.ToString("mm:ss");
            NameFile = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            tBetat.Text = "Préparation sauvegarde dans le fichier : Configuration" + NameFile + ".cfg";

            saveFileLapSelected.InitialDirectory = dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata; 
            saveFileLapSelected.FileName = "LAP "+ NameFile + ".cfg";


            if (saveFileLapSelected.ShowDialog() == DialogResult.OK)
            {
                NameFile = saveFileLapSelected.FileName;

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
                    List<Boolean> ListLap = new List<Boolean>();
                    BinaryFormatter formatter = new BinaryFormatter();

                    for (int i = 0; i < listViewLap.Items.Count; i++)// TRansfert vers une list du statut des tours
                       ListLap.Add(listViewLap.Items[i].Checked);

                    // listViewLap.Items[NbTrack].Checked
                    FileStream stream = new FileStream(NameFile, FileMode.Create);
                    try
                    {
                        formatter.Serialize(stream, ListLap);

                    }
                    catch (Exception ex)
                    {
                        tBetat.AppendText("Error: " + ex + "\n");
                        Console.WriteLine("Error:" + ex); // A mettre dans fenêtre
                    }
                    finally
                    {
                        // Fermez Stream, libérez des ressources
                        stream.Close();
                        tBetat.AppendText("Fin de sauvegarde config des tours dans un fichier " + NameFile + "\n");
                    }
                }
                else
                {
                    tBetat.AppendText("\nEcritureOK == false fichier non suvegardé");
                }

            }
        }

        private void OpenListLapSelected_Click(object sender, EventArgs e)
        {
            string NameFile = "*.cfg";

            openFileLapSelected.FileName = NameFile;

            openFileLapSelected.InitialDirectory = dataSetting.Folders_simu[dataSetting.Simulation].folderRawdata;

            if (openFileLapSelected.ShowDialog() == DialogResult.OK)
            {
                //deserialize
                List<Boolean> ListLap = new List<Boolean>();
                BinaryFormatter formatter = new BinaryFormatter();

                NameFile = openFileLapSelected.FileName;
                FileStream stream = new FileStream(NameFile, FileMode.Open);
                try
                {
                    ListLap = (List<bool>)formatter.Deserialize(stream);

                    if (ListLap.Count == listViewLap.Items.Count)
                        for (int i = 0; i < listViewLap.Items.Count; i++)// TRansfert vers une list du statut des tours
                            listViewLap.Items[i].Checked = ListLap[i];
                    else
                        tBetat.Text = "Erreur quantité tours non identique";
                }
                catch (Exception ex)
                {
                    tBetat.AppendText("Error: " + ex + "\n");
                    Console.WriteLine("Error:" + ex); // A mettre dans fenêtre
                }
                finally
                {
                    // Fermez Stream, libérez des ressources
                    stream.Close();
                    tBetat.AppendText("Fin de Lecture configuration à partir du fichier " + NameFile + "\n");
                }
            }
        }

        private void chartMultiGraph_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.V) 
            { 
                  
            }
        }

        private void chartMultiGraph_PostPaint(object sender, ChartPaintEventArgs e)
        {
            // appelé pour donner des informations sur la position de la souris en fonction du graphique
            Chart chart;

            chart = (Chart)sender;

            if (chart.Name == "chartMultiGraph")
            {
                // exception de temps en temps

                var length = dataUDP.Get_trackLength();
                var min = chartMultiGraph.ChartAreas[0].AxisX.Minimum;
                var max = chartMultiGraph.ChartAreas[0].AxisX.Maximum;

                Point mousePoint = chartMultiGraph.PointToClient(Control.MousePosition);

                if (mousePoint.X >= 49 && mousePoint.X <= chartMultiGraph.Width - 3)
                //if ((e.X > 0) & (e.X < length))
                {
                    // Convertir la position de la souris en valeur de l'axe X
                    // VL.X = chartMultiGraph.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
                    VL.X = chartMultiGraph.ChartAreas[0].AxisX.PixelPositionToValue(mousePoint.X);
                }
                // Debug.WriteLine("Borne min :"+min+" E :"+e.X+" - "+e.Y+" Mouse :"+ mousePoint.X+" - "+ mousePoint.Y+" max :"+max);
                // Debug.WriteLine("Borne min :" + 0 + " Mouse :" + mousePoint.X + " - " + mousePoint.Y + " max :" + chartMultiGraph.Width);
                // Debug.WriteLine("VL.X :" + VL.X);
            }

        }

        private void bZoomReset_Click(object sender, EventArgs e)
        {
            return; // non utilisé
        }
 
        private void bUpdate_Click(object sender, EventArgs e)
        {
            GestionTab();
        }

        private void bestTimeLapToolStripMenuItem1_Click(object sender, EventArgs e)
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

        private void cbChlick(object sender, EventArgs e)
        {
            GestionTab();
        }

        private void cMAP_Click(object sender, EventArgs e)
        {
            double distance;

            // lex valeurs Xc et Yc proviennent de l'événement chartSeriesMouseMove

            if (X1 == 0)
            {
                tBX1.Text = Xc.ToString();
                tBY1.Text = Yc.ToString();
                X1 = Xc;
                Y1 = Yc;
                tBX2.Text = "";
                tBY2.Text = "";
            }
            else
            {
                tBX2.Text = Xc.ToString();
                tBY2.Text = Yc.ToString();
                X2 = Xc;
                Y2 = Yc;

                distance = Math.Sqrt((X1-X2)*(X1-X2) + (Y1-Y2)*(Y1-Y2));
                tBdist.Text = distance.ToString();
                X1 = 0;
            }
        }
    }
}

