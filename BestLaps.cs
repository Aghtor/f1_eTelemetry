using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Windows.Devices.Lights;

//  reference
// https://stackoverflow.com/questions/25801257/c-sharp-line-chart-how-to-create-vertical-line


namespace f1_eTelemetry
{

    public partial class BestLaps : Form
    {
        static UDP_f12021 dataf1_2021 = new UDP_f12021();

        UDP_Data dataUDP = new UDP_Data();
        Setting dataSetting = new Setting();

        int NumCar = 0;

        const byte Koefmoy = 3;

        RectangleAnnotation RA;

        struct StatValues
        {
            // https://blocnotes.iergo.fr/breve/mode-mediane-moyenne-variance-et-ecart-type/

            public List<double> intList;
            public double Moyenne;
            public double Ecart_type;
            public double Ecart_moyen;
            public double Variance;
            public double Mediane;  // https://fr.wikipedia.org/wiki/M%C3%A9diane_(statistiques)
            public double Mode;     // https://fr.wikipedia.org/wiki/Mode_(statistiques)
            public double Somme;
        }

        public BestLaps()
        {
            InitializeComponent();

            RA = new RectangleAnnotation();
            chartTime.Annotations.Add(RA);
            rdraw();           
        }

        public void SendData(UDP_Data _Data, Setting _dataSetting)
        {
            dataUDP = _Data;
            dataSetting = _dataSetting;

            if (dataUDP != null)
                NumCar = dataUDP.Get_playerCarIndex(0);//_Data.ListLapData[0].packetHeader.m_playerCarIndex;

            InitListDrivers();

            AffichageGrid();
        }

        private void InitListDrivers()
        {
            cBdrivers.Items.Clear();

            for (int IndexPlayer = 0; IndexPlayer < dataUDP.Get_NumofPlayers(); IndexPlayer++)
            {
                string Name = dataUDP.Get_m_s_name(IndexPlayer) +
                    " -" +
                    dataUDP.Get_m_driverId(IndexPlayer);
                Name = dataUDP.CleanString(dataUDP.Get_m_s_name(IndexPlayer));
                cBdrivers.Items.Add(Name);
            }

            // nécessaire sinon les selectedindexchanged sont appelés
            cBdrivers.SelectedIndexChanged -= cBdrivers_SelectedIndexChanged;

            cBdrivers.SelectedIndex = NumCar;

            cBdrivers.SelectedIndexChanged += cBdrivers_SelectedIndexChanged;
        }


        private string m6_theoLapTimeInMSF1(int m_numCars)
        {
            int Indextableau=0;
            ushort m_sector1TimeInMS=0;
            ushort m_sector2TimeInMS=0;
            ushort m_sector3TimeInMS=0;

            Indextableau = dataf1_2021.ListHistoric.Count - 1;
            while (dataf1_2021.ListHistoric[Indextableau].m_carIdx != m_numCars && Indextableau > 0)
                Indextableau--;

            if (    (dataf1_2021.ListHistoric[Indextableau].m_bestSector1LapNum < 1) ||
                    (dataf1_2021.ListHistoric[Indextableau].m_bestSector2LapNum < 1) ||
                    (dataf1_2021.ListHistoric[Indextableau].m_bestSector3LapNum < 1))
                return ("err");

            m_sector1TimeInMS = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[
                    dataf1_2021.ListHistoric[Indextableau].m_bestSector1LapNum - 1
                                                                                                 ].m_sector1TimeInMS;
            m_sector2TimeInMS = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[
                dataf1_2021.ListHistoric[Indextableau].m_bestSector2LapNum - 1
                                                                                             ].m_sector2TimeInMS;
            m_sector3TimeInMS = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[
                dataf1_2021.ListHistoric[Indextableau].m_bestSector3LapNum - 1
                                                                                             ].m_sector3TimeInMS;

            if (dataf1_2021.ListHistoric[Indextableau].m_bestSector1LapNum == 0)
            {
                return ("err 0");
            }
            else
                return (TimeSpan.FromMilliseconds(m_sector1TimeInMS + m_sector2TimeInMS + m_sector3TimeInMS).ToString());
        }

        private string m6_theoLapTimeInMS(int m_numCars)
        {
            int Indextableau = 0;
            ushort m_sector1TimeInMS = 0;
            ushort m_sector2TimeInMS = 0;
            ushort m_sector3TimeInMS = 0;

            Indextableau = dataf1_2021.ListHistoric.Count - 1;
            while (dataf1_2021.ListHistoric[Indextableau].m_carIdx != m_numCars && Indextableau > 0)
                Indextableau--;

            if ((dataf1_2021.ListHistoric[Indextableau].m_bestSector1LapNum < 1) ||
                    (dataf1_2021.ListHistoric[Indextableau].m_bestSector2LapNum < 1) ||
                    (dataf1_2021.ListHistoric[Indextableau].m_bestSector3LapNum < 1))
                return ("err");

            m_sector1TimeInMS = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[
                    dataf1_2021.ListHistoric[Indextableau].m_bestSector1LapNum - 1
                                                                                                 ].m_sector1TimeInMS;
            m_sector2TimeInMS = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[
                dataf1_2021.ListHistoric[Indextableau].m_bestSector2LapNum - 1
                                                                                             ].m_sector2TimeInMS;
            m_sector3TimeInMS = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[
                dataf1_2021.ListHistoric[Indextableau].m_bestSector3LapNum - 1
                                                                                             ].m_sector3TimeInMS;

            if (dataf1_2021.ListHistoric[Indextableau].m_bestSector1LapNum == 0)
            {
                return ("err 0");
            }
            else
                return (TimeSpan.FromMilliseconds(m_sector1TimeInMS + m_sector2TimeInMS + m_sector3TimeInMS).ToString());
        }

        private static void FreezeBand(DataGridViewBand band)
        {
            band.Frozen = true;
            DataGridViewCellStyle style = new DataGridViewCellStyle();
            style.BackColor = Color.WhiteSmoke;
            band.DefaultCellStyle = style;
        }

        private string validlap(byte Flag)
        {
            return dataUDP.GetvalidlapString(Flag);
        }

        private void AffichageGridF1()//object sender, EventArgs e)
        {
            float trigger=10000; // millisecondes soit 10 secondes pour éliminer les temps incorrects

            dataf1_2021 = dataUDP.dataf1_2021;

            double minTour = 999999;
            int iminTour = -1;
            double minTime = 9999999;
            double minTime1 = 9999999;
            double minTime2 = 9999999;
            double minTime3 = 9999999;
            int iminTime1 = -1;
            int iminTime2 = -1;
            int iminTime3 = -1;
            double lineHeight = 0;
            int bestlap;
            float Ktime;

            Ktime = (float)dataUDP.GetKtime();

            Chart chart;

            chart = chartTime;
            int NbSerie = 0;

            // mise à zéro du graphique
            chart.Series.Clear();

            SeriesChartType ChartType = SeriesChartType.FastLine;

            ChartArea CA = chart.ChartAreas[0];  // quick reference
            CA.AxisX.ScaleView.Zoomable = true;
            CA.CursorX.AutoScroll = true;
            CA.CursorX.IsUserSelectionEnabled = true;

            CA.AxisY.ScaleView.Zoomable = true;
            CA.CursorY.AutoScroll = true;
            CA.CursorY.IsUserSelectionEnabled = true;


            // https://docs.microsoft.com/fr-fr/dotnet/desktop/winforms/controls/walkthrough-creating-an-unbound-windows-forms-datagridview-control?view=netframeworkdesktop-4.8

            if (dataUDP != null)
            {
                chart.ApplyPaletteColors();

                if (rBgrille.Checked)
                {
                    CA.AxisY2.ScaleView.Zoomable = false;
                    CA.AxisY2.MajorGrid.Enabled = false;
                    CA.AxisY2.Enabled = AxisEnabled.False;

                    dGbestlap.Rows.Clear();
                    dGbestlap.Columns.Clear();

                    //https://stackoverflow.com/questions/10063770/how-to-add-a-new-row-to-datagridview-programmatically

                    dGbestlap.Columns.Add("m_driverId", "Driver name");
                    dGbestlap.Columns.Add("m_teamId", "Team");
                    dGbestlap.Columns.Add("m_raceNumber", "N°");
                    dGbestlap.Columns.Add("m_nationality", "Nationality");
                    dGbestlap.Columns.Add("m_bestLapTimeLapNum", "Best Lap Time");
                    dGbestlap.Columns.Add("m_bestLapTimeLapNum", "Best Lap N°");
                    dGbestlap.Columns.Add("m_bestSector1LapNum", "Sector 1");
                    dGbestlap.Columns.Add("m_bestSector2LapNum", "Sector 2");
                    dGbestlap.Columns.Add("m_bestSector3LapNum", "Sector 3");
                    dGbestlap.Columns.Add("m_theoLapTimeInMS ", "Best Theo Lap ");
                    dGbestlap.Columns.Add("m_totalRaceTime", "Total Race Time");
                    dGbestlap.Columns.Add("IndexPlayer", "i");
                    dGbestlap.Columns.Add("m_aiControlled", "AI");

                    // Figer la 1er colonne pour le défilement https://stackoverflow.com/questions/3240201/in-datagridview-how-to-2-column-that-will-freeze
                    FreezeBand(dGbestlap.Columns[0]);

                    int IndextableauListFinal = dataf1_2021.ListFinal.Count - 1;
                    if (IndextableauListFinal < 0) IndextableauListFinal = 0;

                    int IndextableauListPlayers = dataf1_2021.ListPlayers.Count - 1;
                    if (IndextableauListPlayers<0) IndextableauListPlayers = 0;

                    // Si practice il faut augmenter à 2 ou 3 pour intégrer Fantôme et best time
                    int maxNbPlayer = dataf1_2021.ListPlayers[0].m_numActiveCars; 
                    {
                        for (int IndexPlayer = 0; IndexPlayer < maxNbPlayer; IndexPlayer++)
                        {
                            dGbestlap.Rows.Add(
                            dataf1_2021.m_driverId(IndextableauListPlayers,IndexPlayer),
                            dataf1_2021.m_teamId(dataf1_2021.ListPlayers[IndextableauListPlayers].m_participants[IndexPlayer].m_teamId),
                            dataf1_2021.ListPlayers[IndextableauListPlayers].m_participants[IndexPlayer].m_raceNumber,
                            dataf1_2021.m_nationality(dataf1_2021.ListPlayers[IndextableauListPlayers].m_participants[IndexPlayer].m_nationality),
                            dataf1_2021.m_bestLapTimeLap(IndexPlayer),
                            dataf1_2021.m_bestLapTimeLapNum(IndexPlayer),
                            dataf1_2021.m_sector1TimeInMS(IndexPlayer),
                            dataf1_2021.m_sector2TimeInMS(IndexPlayer),
                            dataf1_2021.m_sector3TimeInMS(IndexPlayer),
                            dataf1_2021.m_theoLapTimeInMS(IndexPlayer),
                            TimeSpan.FromSeconds(dataf1_2021.m_totalRaceTime(IndextableauListFinal, IndexPlayer)),
                            (IndexPlayer + 1),
                            dataf1_2021.m_aiControlled(dataf1_2021.ListPlayers[IndextableauListPlayers].m_participants[IndexPlayer].m_aiControlled)
                            );
                            dGbestlap.AutoResizeColumns();
                            dGbestlap.Columns[0].Width = 100;
                            dGbestlap.Columns[1].Width = 100;
                            dGbestlap.Columns[2].Width = 30;
                            dGbestlap.Columns[3].Width = 100;
                            dGbestlap.Columns[4].Width = 100;
                            dGbestlap.Columns[5].Width = 30;

                            dGbestlap.Columns[6].Width = 100;
                            dGbestlap.Columns[7].Width = 100;
                            dGbestlap.Columns[8].Width = 100;
                            dGbestlap.Columns[9].Width = 100;
                            dGbestlap.Columns[10].Width = 100;
                            dGbestlap.Columns[11].Width = 30;
                            dGbestlap.Columns[12].Width = 60;

                            string chaine = dataUDP.Get_m_s_name(IndexPlayer) +
                                            " -" +
                                            dataUDP.Get_m_driverId(IndexPlayer);
                            chart.Series.Add(chaine);
                            chart.Series[NbSerie].ChartType = ChartType;
                            chart.Series[NbSerie].BorderWidth = 1;
                            chart.Series[NbSerie].ToolTip = "Driver [" + dataUDP.Get_m_s_name(IndexPlayer) + "]";

                            chart.Series[NbSerie].Points.AddXY("Sector1", dataf1_2021.g_BestSector1TimeInMS(IndexPlayer, out bestlap) / Ktime);
                            chart.Series[NbSerie].Points.AddXY("Sector2", dataf1_2021.g_BestSector2TimeInMS(IndexPlayer, out bestlap) / Ktime);
                            chart.Series[NbSerie].Points.AddXY("Sector3", dataf1_2021.g_BestSector3TimeInMS(IndexPlayer, out bestlap) / Ktime);

                            NbSerie++;
                            
                            if ((dataf1_2021.g_BestSector1TimeInMS(IndexPlayer,out bestlap) / Ktime) < minTime1)
                                minTime1 = dataf1_2021.g_BestSector1TimeInMS(IndexPlayer, out bestlap) / Ktime;
                            if ((dataf1_2021.g_BestSector2TimeInMS(IndexPlayer, out bestlap) / Ktime) < minTime2)
                                minTime2 = dataf1_2021.g_BestSector2TimeInMS(IndexPlayer, out bestlap) / Ktime;
                            if ((dataf1_2021.g_BestSector3TimeInMS(IndexPlayer, out bestlap) / Ktime) < minTime3)
                                minTime3 = dataf1_2021.g_BestSector3TimeInMS(IndexPlayer, out bestlap) / Ktime;

                            minTime = Math.Min(minTime1,Math.Min(minTime2, minTime3));
                            
                        }

                        chart.ChartAreas[0].AxisX.Minimum = 1;
                        chart.ChartAreas[0].AxisX.Maximum = 3;
                        chart.ChartAreas[0].AxisX.Interval = 1;
                        chart.Series[0].YValueType = ChartValueType.String;
                        chart.ChartAreas[0].AxisX.Title = "Sector";
                        chart.ChartAreas[0].AxisY.Title = "Time(s)";

                        chart.Series[0].YValueType = ChartValueType.Time;
                        chart.ChartAreas[0].AxisY.LabelStyle.Format = "mm:ss.fff";
                        chart.ChartAreas[0].AxisY.Minimum = minTime; /// Ktime; // Math.Truncate(minTour/ Ktime);
                        //chartTime.ChartAreas[0].AxisY.Minimum = minTour / Ktime; // Math.Truncate(minTour/ Ktime);
                    }
                }

                if (rBdriver.Checked)
                {
                    StatValues Stat_Secteur1 = new StatValues();
                    Stat_Secteur1.intList = new List<double>();

                    StatValues Stat_Secteur2 = new StatValues();
                    Stat_Secteur2.intList = new List<double>();

                    StatValues Stat_Secteur3 = new StatValues();
                    Stat_Secteur3.intList = new List<double>();

                    StatValues Stat_LapTime = new StatValues();
                    Stat_LapTime.intList = new List<double>();



                    //Création des séries chartTime 
                    chartTime.Series.Add("Tour");
                    chartTime.Series.Add("Sector 1");
                    chartTime.Series.Add("Sector 2");
                    chartTime.Series.Add("Sector 3");

                    chartTime.Series[0].ChartType = SeriesChartType.Line;
                    chartTime.Series[1].ChartType = SeriesChartType.Line;
                    chartTime.Series[2].ChartType = SeriesChartType.Line;
                    chartTime.Series[3].ChartType = SeriesChartType.Line;

                    chartTime.Series[0].BorderWidth = 2;
                    chartTime.Series[1].BorderWidth = 5;
                    chartTime.Series[2].BorderWidth = 2;
                    chartTime.Series[3].BorderWidth = 2;


                    chartTime.Series[0].Color = Color.Black;
                    chartTime.Series[1].Color = Color.Red;
                    chartTime.Series[2].Color = Color.Green;
                    chartTime.Series[3].Color = Color.Blue;

                    chartTime.Series[0].IsVisibleInLegend = true;
                    chartTime.Series[1].IsVisibleInLegend = true;
                    chartTime.Series[2].IsVisibleInLegend = true;
                    chartTime.Series[3].IsVisibleInLegend = true;

                    chartTime.Series[0].YAxisType = AxisType.Primary;
                    chartTime.Series[1].YAxisType = AxisType.Secondary;
                    chartTime.Series[2].YAxisType = AxisType.Secondary;
                    chartTime.Series[3].YAxisType = AxisType.Secondary;

                    chartTime.ChartAreas[0].AxisX.Title = "Tours";
                    chartTime.ChartAreas[0].AxisY.Title = "Time race (mn:sec)";
                    chartTime.ChartAreas[0].AxisY2.Title = "Time sector (s)";

                    chartTime.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                    chartTime.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;

                    chartTime.Series[0].YValueType = ChartValueType.Time;
                    chartTime.Series[1].YValueType = ChartValueType.Time;
                    chartTime.Series[2].YValueType = ChartValueType.Time;
                    chartTime.Series[3].YValueType = ChartValueType.Time;

                    chartTime.ChartAreas[0].AxisY2.LineColor = Color.Transparent;
                    chartTime.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
                    chartTime.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;

                    //Création des séries chartStat 
                    chartStat.Series.Clear();
                    chartStat.Series.Add("Tour");
                    chartStat.Series.Add("Secteur 1");
                    chartStat.Series.Add("Secteur 2");
                    chartStat.Series.Add("Secteur 3");

                    chartStat.Series[0].ChartType = SeriesChartType.Point;
                    chartStat.Series[1].ChartType = SeriesChartType.Point;
                    chartStat.Series[2].ChartType = SeriesChartType.Point;
                    chartStat.Series[3].ChartType = SeriesChartType.Point;

                    chartStat.Series[0].YAxisType = AxisType.Primary;
                    chartStat.Series[1].YAxisType = AxisType.Secondary;
                    chartStat.Series[2].YAxisType = AxisType.Secondary;
                    chartStat.Series[3].YAxisType = AxisType.Secondary;

                    chartStat.ChartAreas[0].AxisX.Title = "Tours";
                    chartStat.ChartAreas[0].AxisY.Title = "Time race (mn:sec)";
                    chartStat.ChartAreas[0].AxisY2.Title = "Time sector (s)";
                    chartStat.ChartAreas[0].AxisY.LabelStyle.Format = "mm:ss.fff";
                    chartStat.ChartAreas[0].AxisY2.LabelStyle.Format = "mm:ss.fff";


                    chartStat.Series[0].YValueType = ChartValueType.Time;
                    chartStat.Series[1].YValueType = ChartValueType.Time;
                    chartStat.Series[2].YValueType = ChartValueType.Time;
                    chartStat.Series[3].YValueType = ChartValueType.Time;

                    chartStat.ChartAreas[0].AxisX.Minimum = 0;
                    chartStat.ChartAreas[0].AxisX.Maximum = 3;
                    chartStat.ChartAreas[0].AxisX.Interval = 1;

                    chartStat.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                    chartStat.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;

                    dGbestlap.Rows.Clear();
                    dGbestlap.Columns.Clear();

                    //https://stackoverflow.com/questions/10063770/how-to-add-a-new-row-to-datagridview-programmatically

                    dGbestlap.Columns.Add("Lap", "Lap");
                    dGbestlap.Columns.Add("m_bestSector1LapNum", "Sector 1");
                    dGbestlap.Columns.Add("m_bestSector2LapNum", "Sector 2");
                    dGbestlap.Columns.Add("m_bestSector3LapNum", "Sector 3");
                    dGbestlap.Columns.Add("LapTimeInMS ", "Lap Time");
                    dGbestlap.Columns.Add("Valide ", "Valid");

                    // Figer les colonnes https://stackoverflow.com/questions/3240201/in-datagridview-how-to-2-column-that-will-freeze
                    FreezeBand(dGbestlap.Columns[0]);

                    int Indextableau = dataf1_2021.ListHistoric.Count - 1;
                    while (dataf1_2021.ListHistoric[Indextableau].m_carIdx != NumCar && Indextableau > 0)
                        Indextableau--;
                    ushort m_sector1TimeInMS = 0;
                    ushort m_sector2TimeInMS = 0;
                    ushort m_sector3TimeInMS = 0;

                    if (dataf1_2021.ListHistoric[Indextableau].m_bestSector1LapNum >0)
                        m_sector1TimeInMS = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[
                            dataf1_2021.ListHistoric[Indextableau].m_bestSector1LapNum - 1
                                                                                                   ].m_sector1TimeInMS;
                    if (dataf1_2021.ListHistoric[Indextableau].m_bestSector2LapNum > 0)
                        m_sector2TimeInMS = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[
                            dataf1_2021.ListHistoric[Indextableau].m_bestSector2LapNum - 1
                                                                                                   ].m_sector2TimeInMS;
                    if (dataf1_2021.ListHistoric[Indextableau].m_bestSector3LapNum > 0)
                            m_sector3TimeInMS = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[
                            dataf1_2021.ListHistoric[Indextableau].m_bestSector3LapNum - 1
                                                                                                       ].m_sector3TimeInMS;

                    for (int lap = 0; lap < dataf1_2021.ListHistoric[Indextableau].m_numLaps - 1; lap++)
                    {
                        dGbestlap.Rows.Add(
                            lap + 1,
                            TimeSpan.FromMilliseconds(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector1TimeInMS),
                            TimeSpan.FromMilliseconds(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector2TimeInMS),
                            TimeSpan.FromMilliseconds(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector3TimeInMS),
                            TimeSpan.FromMilliseconds(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapTimeInMS),
                            validlap(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapValidBitFlags)
                            );
                        dGbestlap.Columns[0].Width = 30;// AutoResizeColumns();

                        if ((dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapTimeInMS > 2000) &&// triger pour les temps trop court 15 secondes à mettre dans paramêtre
                            (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector1TimeInMS > 200) &&
                            (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector2TimeInMS > 2000) &&
                            (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector3TimeInMS > 2000))
                        {
                            chartTime.Series[0].Points.AddXY(lap + 1, dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapTimeInMS / Ktime);
                            chartTime.Series[1].Points.AddXY(lap + 1, dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector1TimeInMS / Ktime);
                            chartTime.Series[2].Points.AddXY(lap + 1, dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector2TimeInMS / Ktime);
                            chartTime.Series[3].Points.AddXY(lap + 1, dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector3TimeInMS / Ktime);

                            chartStat.Series[0].Points.AddXY(0, dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapTimeInMS / Ktime);
                            chartStat.Series[1].Points.AddXY(1, dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector1TimeInMS / Ktime);
                            chartStat.Series[2].Points.AddXY(2, dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector2TimeInMS / Ktime);
                            chartStat.Series[3].Points.AddXY(3, dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector3TimeInMS / Ktime);

                            // chart.Series[NbSerie].Points.AddXY("Sector1", dataf1_2021.g_sector1TimeInMS(IndexPlayer, out bestlap) / Ktime);


                            if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapTimeInMS < minTour)
                            {
                                minTour = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapTimeInMS;
                                iminTour = lap;
                            }
                            Stat_LapTime.intList.Add(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapTimeInMS);

                            if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector1TimeInMS < minTime1)
                            {
                                if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector1TimeInMS > trigger)
                                {
                                    minTime1 = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector1TimeInMS;
                                    iminTime1 = lap;
                                }
                            }
                            Stat_Secteur1.intList.Add(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector1TimeInMS);

                            if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector2TimeInMS < minTime2)
                            {
                                if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector2TimeInMS > trigger)
                                {
                                    minTime2 = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector2TimeInMS;
                                    iminTime2 = lap;
                                }
                            }
                            Stat_Secteur2.intList.Add(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector2TimeInMS);

                            if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector3TimeInMS < minTime3)
                            {
                                if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector3TimeInMS > trigger)
                                {
                                    minTime3 = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector3TimeInMS;
                                    iminTime3 = lap;
                                }
                            }
                            Stat_Secteur3.intList.Add(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector3TimeInMS);


                            minTime = Math.Min(minTime1,
                                               Math.Min(minTime2, minTime3));//,
                                                                             //minTime);
                        }
                    }

                    if ((iminTour != -1) || (iminTime1 != -1) || (iminTime2 != -1) || (iminTime3 != -1))
                    {
                        chartTime.ChartAreas[0].AxisY.Minimum = minTour / Ktime; // Math.Truncate(minTour/ Ktime);
                        chartTime.ChartAreas[0].AxisY2.Minimum = minTime / Ktime; // Math.Truncate(minTime/ Ktime);
                    }

                    chart.ChartAreas[0].AxisX.Minimum = 1;
                    chart.ChartAreas[0].AxisX.Maximum = dataf1_2021.ListHistoric[Indextableau].m_numLaps;

                    chart.ChartAreas[0].AxisY.LabelStyle.Format = "mm:ss.fff";
                    chart.ChartAreas[0].AxisY2.LabelStyle.Format = "mm:ss.fff";

                    if (chart.ChartAreas[0].AxisX.Maximum > 10)
                        chart.ChartAreas[0].AxisX.Interval = 5;
                    else
                        chart.ChartAreas[0].AxisX.Interval = 1;

                    if (dataf1_2021.ListHistoric[Indextableau].m_bestSector1LapNum > 0)
                        dGbestlap.Rows[dataf1_2021.ListHistoric[Indextableau].m_bestSector1LapNum - 1].Cells[1].Style.BackColor = Color.Green;
                    if (dataf1_2021.ListHistoric[Indextableau].m_bestSector2LapNum > 0)
                        dGbestlap.Rows[dataf1_2021.ListHistoric[Indextableau].m_bestSector2LapNum - 1].Cells[2].Style.BackColor = Color.Green;
                    if (dataf1_2021.ListHistoric[Indextableau].m_bestSector3LapNum > 0)
                        dGbestlap.Rows[dataf1_2021.ListHistoric[Indextableau].m_bestSector3LapNum - 1].Cells[3].Style.BackColor = Color.Green;
                    if (dataf1_2021.ListHistoric[Indextableau].m_bestLapTimeLapNum > 0)
                        dGbestlap.Rows[dataf1_2021.ListHistoric[Indextableau].m_bestLapTimeLapNum - 1].Cells[4].Style.BackColor = Color.Green;
                    lineHeight = Math.Abs(chart.ChartAreas[0].AxisY.Maximum  - chart.ChartAreas[0].AxisY.Minimum) / 2 + chart.ChartAreas[0].AxisY.Minimum;

                    dGbestlap.AutoResizeColumns();
                    dGbestlap.Columns[0].Width = 30;
                    dGbestlap.Columns[1].Width = 100;
                    dGbestlap.Columns[2].Width = 100;
                    dGbestlap.Columns[3].Width = 100;
                    dGbestlap.Columns[4].Width = 100;
                    dGbestlap.Columns[5].Width = 100;

                    Stat_LapTime = EcartType(Stat_LapTime);
                    Stat_Secteur1 = EcartType(Stat_Secteur1);
                    Stat_Secteur2 = EcartType(Stat_Secteur2);
                    Stat_Secteur3 = EcartType(Stat_Secteur3);

                    dataGridViewStatistique.Rows.Clear();
                    dataGridViewStatistique.Columns.Clear();

                    dataGridViewStatistique.Columns.Add("Statistique", "-");
                    dataGridViewStatistique.Columns.Add("Secteur1", "Secteur1");
                    dataGridViewStatistique.Columns.Add("Secteur2", "Secteur2");
                    dataGridViewStatistique.Columns.Add("Secteur3", "Secteur3");
                    dataGridViewStatistique.Columns.Add("Lap", "Lap");

                    dataGridViewStatistique.Rows.Add(
                        "Moyenne",
                        TimeSpan.FromMilliseconds(Stat_Secteur1.Moyenne),
                        TimeSpan.FromMilliseconds(Stat_Secteur2.Moyenne),
                        TimeSpan.FromMilliseconds(Stat_Secteur3.Moyenne),
                        TimeSpan.FromMilliseconds(Stat_LapTime.Moyenne)
                        );

                    dataGridViewStatistique.Rows.Add(
                        "Ecart Type",
                        TimeSpan.FromMilliseconds(Stat_Secteur1.Ecart_type),
                        TimeSpan.FromMilliseconds(Stat_Secteur2.Ecart_type),
                        TimeSpan.FromMilliseconds(Stat_Secteur3.Ecart_type),
                        TimeSpan.FromMilliseconds(Stat_LapTime.Ecart_type)
                        );

                    dataGridViewStatistique.Rows.Add(
                        "Ecart Moyen",
                        TimeSpan.FromMilliseconds(Stat_Secteur1.Ecart_moyen),
                        TimeSpan.FromMilliseconds(Stat_Secteur2.Ecart_moyen),
                        TimeSpan.FromMilliseconds(Stat_Secteur3.Ecart_moyen),
                        TimeSpan.FromMilliseconds(Stat_LapTime.Ecart_moyen)
                        );

                    dataGridViewStatistique.Rows.Add(
                        "Variance",
                        TimeSpan.FromMilliseconds(Stat_Secteur1.Variance),
                        TimeSpan.FromMilliseconds(Stat_Secteur2.Variance),
                        TimeSpan.FromMilliseconds(Stat_Secteur3.Variance),
                        TimeSpan.FromMilliseconds(Stat_LapTime.Variance)
                        );

                    dataGridViewStatistique.AutoResizeColumns();
                    dataGridViewStatistique.Columns[0].Width = 100;
                    dataGridViewStatistique.Columns[1].Width = 100;
                    dataGridViewStatistique.Columns[2].Width = 100;
                    dataGridViewStatistique.Columns[3].Width = 100;
                    dataGridViewStatistique.Columns[4].Width = 100;
                    FreezeBand(dataGridViewStatistique.Columns[0]);
                }

                // https://stackoverflow.com/questions/1941713/datagridview-row-formatting-based-on-data
                // https://stackoverflow.com/questions/27787381/chart-vertical-line-and-string-on-x-label
                // https://stackoverflow.com/questions/25801257/c-sharp-line-chart-how-to-create-vertical-line
            }
        }

        private void AffichageGrid()//object sender, EventArgs e)
        {
            float trigger = 10000; // millisecondes soit 10 secondes pour éliminer les temps incorrects

            double maxtourTime = 0; // temps
            int imaxTourLap = -1;
            double minTourTime = 999999; //temps
            int iminTourLap = -1;

            double minTime = 9999999;
            double minSectorTime1 = 9999999;
            double minSectorTime2 = 9999999;
            double minSectorTime3 = 9999999;
            double maxSectorTime = 0; // temps
            int iminTime1Lap = -1;
            int iminTime2Lap = -1;
            int iminTime3Lap = -1;
            double maxTime = 0;
            double maxtimemoyenne = 0;
            double sector1time = 0;
            double sector2time = 0;
            double sector3time = 0;
            double Laptime = 0;
            double lineHeight = 0;
            int bestlap;
            float Ktime;

            Ktime = (float)dataUDP.GetKtime();

            int NbSerie = 0;

            // mise à zéro du graphique
            chartTime.Series.Clear();
            chartStat.Series.Clear();

            SeriesChartType ChartType = SeriesChartType.FastLine;

            ChartArea CA = chartTime.ChartAreas[0];  // quick reference
            CA.AxisX.ScaleView.Zoomable = true;
            CA.CursorX.AutoScroll = true;
            CA.CursorX.IsUserSelectionEnabled = true;

            CA.AxisY.ScaleView.Zoomable = true;
            CA.CursorY.AutoScroll = true;
            CA.CursorY.IsUserSelectionEnabled = true;


            // https://docs.microsoft.com/fr-fr/dotnet/desktop/winforms/controls/walkthrough-creating-an-unbound-windows-forms-datagridview-control?view=netframeworkdesktop-4.8

            if (dataUDP != null)
            {
                chartTime.ApplyPaletteColors();

                if (rBgrille.Checked)

                {
                    CA.AxisY2.ScaleView.Zoomable = false;
                    CA.AxisY2.MajorGrid.Enabled = false;
                    CA.AxisY2.Enabled = AxisEnabled.False;

                    dGbestlap =dataUDP.GridArrayDefineGrilleColumn(dGbestlap);

                    // Figer la 1er colonne pour le défilement https://stackoverflow.com/questions/3240201/in-datagridview-how-to-2-column-that-will-freeze
                    FreezeBand(dGbestlap.Columns[0]);

                    // Si practice il faut augmenter à 2 ou 3 pour intégrer Fantôme et best time
                    // int maxNbPlayer = dataf1_2021.ListPlayers[0].m_numActiveCars;
                    {
                        for (int IndexPlayer = 0; IndexPlayer < dataUDP.Get_NumofPlayers(); IndexPlayer++)
                        {
                            dGbestlap.Rows.Add(
                                dataUDP.BestLapsGridArray(IndexPlayer)
                            );
                            dGbestlap=dataUDP.GridArrayDefineSizeGrilleColumn(dGbestlap);
  
                            string chaine = dataUDP.Get_m_s_name(IndexPlayer);
                            chartTime.Series.Add(chaine);
                            chartTime.Series[NbSerie].ChartType = ChartType;
                            chartTime.Series[NbSerie].BorderWidth = 1;
                            chartTime.Series[NbSerie].ToolTip = "Driver [" + dataUDP.Get_m_s_name(IndexPlayer) + "]";

                            sector1time = dataUDP.Get_BestSector1TimeInMS(IndexPlayer, out bestlap) / Ktime;
                            sector2time = dataUDP.Get_BestSector2TimeInMS(IndexPlayer, out bestlap) / Ktime;
                            sector3time = dataUDP.Get_BestSector3TimeInMS(IndexPlayer, out bestlap) / Ktime;

                            chartTime.Series[NbSerie].Points.AddXY("Sector1", sector1time);
                            chartTime.Series[NbSerie].Points.AddXY("Sector2", sector2time);
                            chartTime.Series[NbSerie].Points.AddXY("Sector3", sector3time);

                            NbSerie++;

                            if ((sector1time) < minSectorTime1)
                                minSectorTime1 = sector1time;
                            if ((sector2time) < minSectorTime2)
                                minSectorTime2 = sector2time;
                            if ((sector3time) < minSectorTime3)
                                minSectorTime3 = sector3time;

                            // Traitement pour supprimer les anomalies - Driver qui abandone donc tour anormalement trop élevé
                            //Secteur 1
                            if (maxtimemoyenne == 0)
                                maxtimemoyenne = sector1time;
                            else
                                if ((maxtimemoyenne*Koefmoy) > sector1time)
                            {
                                // ce n'est pas une vrai moyenne mais ça fera le job
                                maxtimemoyenne = ((sector1time) + maxtimemoyenne) / 2;
                            }

                            if ((maxtimemoyenne * Koefmoy) > sector1time)
                                if ((sector1time) > maxTime)
                                    maxTime = sector1time;

                            //Secteur 2
                            if ((maxtimemoyenne * Koefmoy) > sector2time)
                            {
                                // ce n'est pas une vrai moyenne mais ça fera le job
                                maxtimemoyenne = ((sector2time) + maxtimemoyenne) / 2;
                                if ((sector2time) > maxTime)
                                    maxTime = sector2time;
                            }

                            //Secteur 3
                            if ((maxtimemoyenne * Koefmoy) > sector3time)
                            {
                                // ce n'est pas une vrai moyenne mais ça fera le job
                                maxtimemoyenne = ((sector3time) + maxtimemoyenne) / 2;
                                if ((sector3time) > maxTime)
                                    maxTime = sector3time;
                            }


                            minTime = Math.Min(minSectorTime1, Math.Min(minSectorTime2, minSectorTime3));
                        }

                        chartTime.ChartAreas[0].AxisX.Minimum = 1;
                        chartTime.ChartAreas[0].AxisX.Maximum = 3;
                        chartTime.ChartAreas[0].AxisX.Interval = 1;
                        chartTime.Series[0].YValueType = ChartValueType.String;
                        chartTime.ChartAreas[0].AxisX.Title = "Sector";
                        chartTime.ChartAreas[0].AxisY.Title = "Time(s)";

                        if (minTime < 60)
                        {
                            chartTime.Series[0].YValueType = ChartValueType.Double;
                            chartTime.ChartAreas[0].AxisY.LabelStyle.Format = "0";
                        }
                        else
                        {
                            chartTime.Series[0].YValueType = ChartValueType.Time;
                            chartTime.ChartAreas[0].AxisY.LabelStyle.Format = "mm:ss.fff";
                        }

                        chartTime.ChartAreas[0].AxisY.Minimum = minTime; /// Ktime; // Math.Truncate(minTour/ Ktime);
                        chartTime.ChartAreas[0].AxisY.Maximum = maxTime;
                        chartTime.ChartAreas[0].AxisY.Interval = 
                            Math.Round((maxTime - minTime)/10,0);
                        //chartTime.ChartAreas[0].AxisY.Minimum = minTour / Ktime; // Math.Truncate(minTour/ Ktime);
                    }
                }

                if (rBdriver.Checked)
                {
                    StatValues Stat_Secteur1 = new StatValues();
                    Stat_Secteur1.intList = new List<double>();

                    StatValues Stat_Secteur2 = new StatValues();
                    Stat_Secteur2.intList = new List<double>();

                    StatValues Stat_Secteur3 = new StatValues();
                    Stat_Secteur3.intList = new List<double>();

                    StatValues Stat_LapTime = new StatValues();
                    Stat_LapTime.intList = new List<double>();

                    //Création des séries chartTime 
                    chartTime.Series.Clear();
                    chartTime.ChartAreas[0].AxisX.Minimum = double.NaN;
                    chartTime.ChartAreas[0].AxisX.Maximum = double.NaN;
                    chartTime.ChartAreas[0].AxisY.Minimum = double.NaN;
                    chartTime.ChartAreas[0].AxisY.Maximum = double.NaN;
                    chartTime.ChartAreas[0].AxisY2.Maximum = double.NaN;

                    chartTime.Series.Add("Tour");
                    chartTime.Series.Add("Sector 1");
                    chartTime.Series.Add("Sector 2");
                    chartTime.Series.Add("Sector 3");

                    chartTime.Series[0].ChartType = SeriesChartType.Line;
                    chartTime.Series[1].ChartType = SeriesChartType.Line;
                    chartTime.Series[2].ChartType = SeriesChartType.Line;
                    chartTime.Series[3].ChartType = SeriesChartType.Line;

                    chartTime.Series[0].BorderWidth = 2;
                    chartTime.Series[1].BorderWidth = 2;
                    chartTime.Series[2].BorderWidth = 2;
                    chartTime.Series[3].BorderWidth = 2;


                    chartTime.Series[0].Color = Color.Black;
                    chartTime.Series[1].Color = Color.Red;
                    chartTime.Series[2].Color = Color.Green;
                    chartTime.Series[3].Color = Color.Blue;

                    chartTime.Series[0].IsVisibleInLegend = true;
                    chartTime.Series[1].IsVisibleInLegend = true;
                    chartTime.Series[2].IsVisibleInLegend = true;
                    chartTime.Series[3].IsVisibleInLegend = true;

                    chartTime.Series[0].YAxisType = AxisType.Primary;
                    chartTime.Series[1].YAxisType = AxisType.Secondary; //  2323-11-12 aucun effet positif sur le bug
                    chartTime.Series[2].YAxisType = AxisType.Secondary; //  2323-11-12 aucun effet positif sur le bug
                    chartTime.Series[3].YAxisType = AxisType.Secondary; //  2323-11-12 aucun effet positif sur le bug

                    chartTime.ChartAreas[0].AxisX.Title = "Tours";
                    chartTime.ChartAreas[0].AxisY.Title = "Time Lap (mn:sec)";
                    chartTime.ChartAreas[0].AxisY2.Title = "Time sector (s)";

                    chartTime.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                    chartTime.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                    
                    
                    chartTime.Series[0].YValueType = ChartValueType.DateTime;
                    chartTime.Series[1].YValueType = ChartValueType.DateTime;
                    chartTime.Series[2].YValueType = ChartValueType.DateTime;
                    chartTime.Series[3].YValueType = ChartValueType.DateTime;
                    

                    chartTime.ChartAreas[0].AxisY2.LineColor = Color.Transparent;
                    chartTime.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
                    chartTime.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;

                    chartTime.ChartAreas[0].AxisY.LabelStyle.Format = "mm:ss.ffff";
                    chartTime.ChartAreas[0].AxisY2.LabelStyle.Format = "mm:ss.fff";


                    //Création des séries chartStat 
                    chartStat.Series.Clear();
                    chartStat.Series.Add("Tour");
                    chartStat.Series.Add("Secteur 1");
                    chartStat.Series.Add("Secteur 2");
                    chartStat.Series.Add("Secteur 3");

                    chartStat.Series[0].ChartType = SeriesChartType.Point;
                    chartStat.Series[1].ChartType = SeriesChartType.Point;
                    chartStat.Series[2].ChartType = SeriesChartType.Point;
                    chartStat.Series[3].ChartType = SeriesChartType.Point;

                    chartStat.Series[0].YAxisType = AxisType.Primary;
                    chartStat.Series[1].YAxisType = AxisType.Secondary;
                    chartStat.Series[2].YAxisType = AxisType.Secondary;
                    chartStat.Series[3].YAxisType = AxisType.Secondary;

                    chartStat.ChartAreas[0].AxisX.Title = "Tours";
                    chartStat.ChartAreas[0].AxisY.Title = "Time Lap (mn:sec)";
                    chartStat.ChartAreas[0].AxisY2.Title = "Time sector (mn:sec)";
                    chartStat.ChartAreas[0].AxisY.LabelStyle.Format = "mm:ss.fff";
                    chartStat.ChartAreas[0].AxisY2.LabelStyle.Format = "mm:ss.fff";

                    
                    chartStat.Series[0].YValueType = ChartValueType.Time;
                    chartStat.Series[1].YValueType = ChartValueType.Time;
                    chartStat.Series[2].YValueType = ChartValueType.Time;
                    chartStat.Series[3].YValueType = ChartValueType.Time;
                    
                    chartStat.ChartAreas[0].AxisX.Minimum = 0;
                    chartStat.ChartAreas[0].AxisX.Maximum = 3;
                    chartStat.ChartAreas[0].AxisX.Interval = 1;

                    chartStat.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                    chartStat.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;

                    chartStat.Series[0].Color = Color.Black;
                    chartStat.Series[1].Color = Color.Red;
                    chartStat.Series[2].Color = Color.Green;
                    chartStat.Series[3].Color = Color.Blue;


                    dGbestlap = dataUDP.GridArrayDefineDriverColumn(dGbestlap);

                    //https://stackoverflow.com/questions/10063770/how-to-add-a-new-row-to-datagridview-programmatically

                        // Figer les colonnes https://stackoverflow.com/questions/3240201/in-datagridview-how-to-2-column-that-will-freeze
                    FreezeBand(dGbestlap.Columns[0]);

                    dataf1_2021 = dataUDP.dataf1_2021;
                    
                    // recherche de la voiture sélectionner dans la sélection.
                    // Indextableau = Numéro de la voiture

                    // Doit être débporté dans les fonctions
                    //int Indextableau = dataf1_2021.ListHistoric.Count - 1;
                    //while (dataf1_2021.ListHistoric[Indextableau].m_carIdx != NumCar && Indextableau > 0)
                    //    Indextableau--;

                    // Renseignement des meilleurs temps par secteur
                    double m_sector1TimeInMS = 0;
                    double m_sector2TimeInMS = 0;
                    double m_sector3TimeInMS = 0;


                    // Renseignement du meilleurs temps par secteur
                    //
                    // BestSecteur =  SecteurXTemps [ BestLap(Num Voiture) ]
                    //if (dataf1_2021.ListHistoric[Indextableau].m_bestSector1LapNum > 0)
                    //    m_sector1TimeInMS = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[
                    //        dataf1_2021.ListHistoric[Indextableau].m_bestSector1LapNum - 1
                    //                                                                               ].m_sector1TimeInMS;
                    float test;
                    m_sector1TimeInMS  = dataUDP.Get_BestSector1TimeInMS(NumCar, out int bestlap1); // = m_sector1TimeInMS;

                    //if (dataf1_2021.ListHistoric[Indextableau].m_bestSector2LapNum > 0)
                    //   m_sector2TimeInMS = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[
                    //        dataf1_2021.ListHistoric[Indextableau].m_bestSector2LapNum - 1
                    //                                                                               ].m_sector2TimeInMS;
                    m_sector2TimeInMS = dataUDP.Get_BestSector2TimeInMS(NumCar, out int bestlap2);

                    //if (dataf1_2021.ListHistoric[Indextableau].m_bestSector3LapNum > 0)
                    //    m_sector3TimeInMS = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[
                    //    dataf1_2021.ListHistoric[Indextableau].m_bestSector3LapNum - 1
                    //                                                                               ].m_sector3TimeInMS;
                    m_sector3TimeInMS = dataUDP.Get_BestSector3TimeInMS(NumCar, out int bestlap3);

                    // Affichage des temps par secteur dans le tableau et graphisme des temps + stats
                    //for (int lap = 0; lap < dataf1_2021.ListHistoric[Indextableau].m_numLaps - 1; lap++)
                    //test = dataf1_2021.ListHistoric[Indextableau].m_numLaps;
                    for (int lap = 0; lap < dataUDP.Get_m_numLaps(NumCar) ; lap++)
                    {
                        // Grilles des temps par secteur SecteurXTime = HistoricSecteurXTime [ Num Voiture , Lap ]
                        //dGbestlap.Rows.Add(
                        //    lap + 1,
                        //    TimeSpan.FromMilliseconds(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector1TimeInMS).ToString(),
                        //    TimeSpan.FromMilliseconds(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector2TimeInMS).ToString(),
                        //    TimeSpan.FromMilliseconds(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector3TimeInMS).ToString(),
                        //    TimeSpan.FromMilliseconds(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapTimeInMS).ToString(),
                        //    validlap(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapValidBitFlags)
                        //    );

                        sector1time = dataUDP.Get_m_sector1TimeInMS(lap, NumCar);
                        sector2time = dataUDP.Get_m_sector2TimeInMS(lap, NumCar);
                        sector3time = dataUDP.Get_m_sector3TimeInMS(lap, NumCar);
                        Laptime = dataUDP.Get_m_lapTimeInMS(lap, NumCar);


                        dGbestlap.Rows.Add(
                            lap + 1,
                            TimeSpan.FromMilliseconds(sector1time).ToString(),
                            TimeSpan.FromMilliseconds(sector2time).ToString(),
                            TimeSpan.FromMilliseconds(sector3time).ToString(),
                            TimeSpan.FromMilliseconds(Laptime).ToString(),
                            validlap(dataUDP.Get_LapValid(lap, NumCar))
                            );


                        dGbestlap.Columns[0].Width = 30;// AutoResizeColumns();
                        // référence à Indextableau qui est = Numcar dans la série
                        //                                  dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap   ].m_sector1TimeInMS
                        // dataUDP.Get_m_sector1TimeInMS =  dataf1_2021.ListHistoric[IndexData   ].m_lapHistoryData[NumCar].m_sector1TimeInMS;
                        //En  cours
                        // Graphiques des temps et stat par secteur 
                        //
                        // Graphe des temps : SecteurXTime for each Lap = HistoricSecteurXTime [ Num Voiture , Lap ] & SecteurXTime = HistoricLapTime [ Num Voiture , Lap ]
                        // Graphe des stats : SecteurXTime = HistoricSecteurXTime [ Num Voiture , Lap ] & SecteurXTime = HistoricLapTime [ Num Voiture , Lap ]
                        // 

                        // 2023-11-12 à étudier
                        //https://stackoverflow.com/questions/41936346/time-on-x-axis-in-c-sharp-charts

                        //if ((dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapTimeInMS > 2000) &&// triger pour les temps trop court 15 secondes à mettre dans paramêtre
                        //    (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector1TimeInMS > 200) &&
                        //    (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector2TimeInMS > 2000) &&
                        //    (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector3TimeInMS > 2000))


                        if ((Laptime > 50) &&// triger pour les temps trop court 15 secondes à mettre dans paramêtre certain circuit sont avec des secteurs à 10 sec 
                            (sector1time > 50) &&
                            (sector2time > 50) &&
                            (sector3time > 50))


                            {
                                // Conversion des temps en minutes:secondes:millisecondes
                                //DateTime dateTime = DateTime.MinValue.AddMilliseconds(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapTimeInMS);
                                DateTime dateTime = DateTime.MinValue.AddMilliseconds(Laptime);
                                chartTime.Series[0].Points.AddXY(lap + 1, dateTime);
                                chartStat.Series[0].Points.AddXY(0, dateTime);

                                //dateTime = DateTime.MinValue.AddMilliseconds(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector1TimeInMS);
                                dateTime = DateTime.MinValue.AddMilliseconds(sector1time);
                                chartTime.Series[1].Points.AddXY(lap + 1, dateTime);
                                chartStat.Series[1].Points.AddXY(1, dateTime);

                                //dateTime = DateTime.MinValue.AddMilliseconds(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector2TimeInMS);
                                dateTime = DateTime.MinValue.AddMilliseconds(sector2time);
                                chartTime.Series[2].Points.AddXY(lap + 1, dateTime);
                                chartStat.Series[2].Points.AddXY(2, dateTime);


                                //dateTime = DateTime.MinValue.AddMilliseconds(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector3TimeInMS);
                                dateTime = DateTime.MinValue.AddMilliseconds(sector3time);
                                chartTime.Series[3].Points.AddXY(lap + 1, dateTime);
                                chartStat.Series[3].Points.AddXY(3, dateTime);

                                // Analyse des temps min max pour les graphismes + info dans la list stats
                                //
                                // SecteurXTime for each Lap = HistoricSecteurXTime [ Num Voiture , Lap ] & SecteurXTime = HistoricLapTime [ Num Voiture , Lap ]
                                //if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapTimeInMS < minTourTime)
                                if (Laptime < minTourTime)
                                {
                                minTourTime = Laptime;// dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapTimeInMS;
                                    iminTourLap = lap;
                                }

                                //if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapTimeInMS > maxtourTime)
                                if (Laptime > maxtourTime)
                                {
                                    maxtourTime = Laptime;// dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapTimeInMS;
                                    imaxTourLap = lap;
                                }
                                //Stat_LapTime.intList.Add(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapTimeInMS);
                                Stat_LapTime.intList.Add(Laptime);

                                //if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector1TimeInMS < minSectorTime1)
                                if (sector1time < minSectorTime1)
                                {
                                    //if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector1TimeInMS > trigger)
                                    if (dataUDP.Get_m_sector1TimeInMS(lap, NumCar) > trigger)
                                    {
                                        //minSectorTime1 = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector1TimeInMS;
                                        minSectorTime1 = dataUDP.Get_m_sector1TimeInMS(lap, NumCar);
                                        iminTime1Lap = lap;
                                    }
                                }
                                //if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector1TimeInMS > maxSectorTime)

                                if (sector1time > maxSectorTime)
                                {
                                    //maxSectorTime = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector1TimeInMS;
                                    maxSectorTime = dataUDP.Get_m_sector1TimeInMS( lap, NumCar);
                                }
                                //Stat_Secteur1.intList.Add(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector1TimeInMS);
                                Stat_Secteur1.intList.Add(sector1time);

                                //if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector2TimeInMS < minSectorTime2)
                                if (sector2time < minSectorTime2)
                                {
                                    //if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector2TimeInMS > trigger)
                                    if (dataUDP.Get_m_sector2TimeInMS( lap, NumCar) > trigger)
                                    {
                                        //minSectorTime2 = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector2TimeInMS;
                                        minSectorTime2 = dataUDP.Get_m_sector2TimeInMS( lap, NumCar);
                                        iminTime2Lap = lap;
                                    }
                                }
                                //if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector2TimeInMS > maxSectorTime)

                                if (sector2time > maxSectorTime)
                                {
                                    //maxSectorTime = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector2TimeInMS;
                                    maxSectorTime = dataUDP.Get_m_sector2TimeInMS( lap, NumCar);
                                }
                                //Stat_Secteur2.intList.Add(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector2TimeInMS);
                                Stat_Secteur2.intList.Add(sector2time);

                                //if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector3TimeInMS < minSectorTime3)
                                if (sector3time < minSectorTime3)
                                {
                                    //if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector3TimeInMS > trigger)
                                    if (sector3time > trigger)
                                    {
                                        //minSectorTime3 = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector3TimeInMS;
                                        minSectorTime3 = sector3time;
                                        iminTime3Lap = lap;
                                    }
                                }

                                //if (dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector3TimeInMS > maxSectorTime)
                                if (sector3time > maxSectorTime)
                                {
                                    //maxSectorTime = dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector3TimeInMS;
                                    maxSectorTime = dataUDP.Get_m_sector3TimeInMS( lap, NumCar);
                                }

                                //Stat_Secteur3.intList.Add(dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_sector3TimeInMS);
                                Stat_Secteur3.intList.Add(sector3time);

                                //*********************************************************
                                // a vérifier on parle de best au lieu du tour en cours
                                if ((sector1time ) > maxTime)
                                    maxTime = sector1time ;
                                if ((sector2time ) > maxTime)
                                    maxTime = sector2time;
                                if ((sector3time) > maxTime)
                                    maxTime = sector3time ;

                            minTime = Math.Min(minSectorTime1,
                                                   Math.Min(minSectorTime2, minSectorTime3));//,
                                                                                 //minTime);
                        }
                    }


                    // Min MAX des axes 
                    if ((iminTourLap != -1) || (iminTime1Lap != -1) || (iminTime2Lap != -1) || (iminTime3Lap != -1))
                    {
                        chartTime.ChartAreas[0].AxisY.Minimum = (double)(minTourTime / (Ktime * 60 * 60 * 24 )); // Math.Truncate(minTour/ Ktime);
                        chartTime.ChartAreas[0].AxisY.Maximum = (double)(maxtourTime / (Ktime * 60 * 60 * 24)); // Math.Truncate(minTour/ Ktime);
                        //chartTime.ChartAreas[0].AxisY2.Minimum = (double)(minTime / (Ktime * 60 * 60 * 24)); // Math.Truncate(minTime/ Ktime);
                        //chartTime.ChartAreas[0].AxisY2.Maximum = (double)(maxTime / (Ktime * 60 * 60 * 24)); // Math.Truncate(minTime/ Ktime);
                    }

                    /*
                     * 
                            Oui, vous pouvez arrondir un nombre à une précision spécifique en utilisant la fonction `Math.Round`. 
                            Par exemple, pour arrondir 0.00235 à la précision de 0.0025, vous pouvez faire :

                            ```csharp
                            double valeur = 0.00235;
                            double precision = 0.0025;

                            double valeurArrondie = Math.Round(valeur / precision) * precision;
                            ```

                            Dans ce cas, `valeurArrondie` serait égal à 0.0025. De même, si vous voulez arrondir à la précision 
                            de 0.0020, vous pouvez ajuster la valeur de `precision` en conséquence.
                     
                     * 
                     */
                    //chartTime.ChartAreas[0].AxisY2.Interval =
                    //    Math.Round((chartTime.ChartAreas[0].AxisY2.Maximum - chartTime.ChartAreas[0].AxisY2.Minimum) / 10, 5);

                    chartTime.ChartAreas[0].AxisY.Interval =
                        Math.Round( (chartTime.ChartAreas[0].AxisY.Maximum  - chartTime.ChartAreas[0].AxisY.Minimum ) / 5, 5);

                    chartTime.ChartAreas[0].AxisX.Minimum = 1;
                    chartTime.ChartAreas[0].AxisX.Maximum = dataUDP.Get_m_numLaps(NumCar);//dataf1_2021.ListHistoric[Indextableau].m_numLaps;

                    if (chartTime.ChartAreas[0].AxisX.Maximum > 20)
                        chartTime.ChartAreas[0].AxisX.Interval = 5;
                    else
                    if (chartTime.ChartAreas[0].AxisX.Maximum > 10)
                            chartTime.ChartAreas[0].AxisX.Interval = 2; 
                    else
                        chartTime.ChartAreas[0].AxisX.Interval = 1; 


                    // affichage en vert des cellules avec les meilleurs temps
                    //if (dataf1_2021.ListHistoric[Indextableau].m_bestSector1LapNum > 0)
                    //    dGbestlap.Rows[dataf1_2021.ListHistoric[Indextableau].m_bestSector1LapNum - 1].Cells[1].Style.BackColor = Color.Green;
                    //if (dataf1_2021.ListHistoric[Indextableau].m_bestSector2LapNum > 0)
                    //    dGbestlap.Rows[dataf1_2021.ListHistoric[Indextableau].m_bestSector2LapNum - 1].Cells[2].Style.BackColor = Color.Green;
                    //if (dataf1_2021.ListHistoric[Indextableau].m_bestSector3LapNum > 0)
                    //    dGbestlap.Rows[dataf1_2021.ListHistoric[Indextableau].m_bestSector3LapNum - 1].Cells[3].Style.BackColor = Color.Green;
                    //if (dataf1_2021.ListHistoric[Indextableau].m_bestLapTimeLapNum > 0)
                    //    dGbestlap.Rows[dataf1_2021.ListHistoric[Indextableau].m_bestLapTimeLapNum - 1].Cells[4].Style.BackColor = Color.Green;

                    if (dataUDP.Get_m_bestSector1LapNum(NumCar) > 0)
                        dGbestlap.Rows[(int)dataUDP.Get_m_bestSector1LapNum(NumCar) - 1].Cells[1].Style.BackColor = Color.Green;

                    if (dataUDP.Get_m_bestSector2LapNum(NumCar) > 0)
                        dGbestlap.Rows[(int)dataUDP.Get_m_bestSector2LapNum(NumCar) - 1].Cells[2].Style.BackColor = Color.Green;

                    if (dataUDP.Get_m_bestSector3LapNum(NumCar) > 0)
                        dGbestlap.Rows[(int)dataUDP.Get_m_bestSector3LapNum(NumCar) - 1].Cells[3].Style.BackColor = Color.Green;

                    if (dataUDP.Get_m_bestLapTimeLapNum(NumCar) > 0)
                        dGbestlap.Rows[(int)dataUDP.Get_m_bestLapTimeLapNum(NumCar) - 1].Cells[4].Style.BackColor = Color.Green;



                    lineHeight = Math.Abs(chartTime.ChartAreas[0].AxisY.Maximum - chartTime.ChartAreas[0].AxisY.Minimum) / 2 + chartTime.ChartAreas[0].AxisY.Minimum;
                    
                    //dGbestlap.AutoResizeColumns();
                    dGbestlap.Columns[0].Width = 30;
                    dGbestlap.Columns[1].Width = 100;
                    dGbestlap.Columns[2].Width = 100;
                    dGbestlap.Columns[3].Width = 100;
                    dGbestlap.Columns[4].Width = 100;
                    dGbestlap.Columns[5].Width = 100;

                    // nécessaire sinon les précédents résultats conservés
                    dataGridViewStatistique.Rows.Clear();
                    dataGridViewStatistique.Columns.Clear();

                    // Dans le cas où aucune data des disponible, genre le gars qui ne fait pas la course
                    if ((Stat_LapTime.intList.Count > 0) &
                        (Stat_Secteur1.intList.Count > 0) &
                        (Stat_Secteur2.intList.Count > 0) &
                        (Stat_Secteur3.intList.Count > 0))
                    {
                        // DataGrid Statistiques
                        Stat_LapTime = EcartType(Stat_LapTime);
                        Stat_Secteur1 = EcartType(Stat_Secteur1);
                        Stat_Secteur2 = EcartType(Stat_Secteur2);
                        Stat_Secteur3 = EcartType(Stat_Secteur3);

                        dataGridViewStatistique.Columns.Add("Statistique", "-");
                        dataGridViewStatistique.Columns.Add("Secteur1", "Secteur1");
                        dataGridViewStatistique.Columns.Add("Secteur2", "Secteur2");
                        dataGridViewStatistique.Columns.Add("Secteur3", "Secteur3");
                        dataGridViewStatistique.Columns.Add("Lap", "Lap");

                        dataGridViewStatistique.Rows.Add(
                            "Moyenne",
                            TimeSpan.FromMilliseconds(Stat_Secteur1.Moyenne),
                            TimeSpan.FromMilliseconds(Stat_Secteur2.Moyenne),
                            TimeSpan.FromMilliseconds(Stat_Secteur3.Moyenne),
                            TimeSpan.FromMilliseconds(Stat_LapTime.Moyenne)
                            );

                        dataGridViewStatistique.Rows.Add(
                            "Ecart Type",
                            TimeSpan.FromMilliseconds(Stat_Secteur1.Ecart_type),
                            TimeSpan.FromMilliseconds(Stat_Secteur2.Ecart_type),
                            TimeSpan.FromMilliseconds(Stat_Secteur3.Ecart_type),
                            TimeSpan.FromMilliseconds(Stat_LapTime.Ecart_type)
                            );

                        dataGridViewStatistique.Rows.Add(
                            "Ecart Moyen",
                            TimeSpan.FromMilliseconds(Stat_Secteur1.Ecart_moyen),
                            TimeSpan.FromMilliseconds(Stat_Secteur2.Ecart_moyen),
                            TimeSpan.FromMilliseconds(Stat_Secteur3.Ecart_moyen),
                            TimeSpan.FromMilliseconds(Stat_LapTime.Ecart_moyen)
                            );

                        dataGridViewStatistique.Rows.Add(
                            "Variance",
                            TimeSpan.FromMilliseconds(Stat_Secteur1.Variance),
                            TimeSpan.FromMilliseconds(Stat_Secteur2.Variance),
                            TimeSpan.FromMilliseconds(Stat_Secteur3.Variance),
                            TimeSpan.FromMilliseconds(Stat_LapTime.Variance)
                            );

                        dataGridViewStatistique.AutoResizeColumns();
                        dataGridViewStatistique.Columns[0].Width = 100;
                        dataGridViewStatistique.Columns[1].Width = 100;
                        dataGridViewStatistique.Columns[2].Width = 100;
                        dataGridViewStatistique.Columns[3].Width = 100;
                        dataGridViewStatistique.Columns[4].Width = 100;
                        FreezeBand(dataGridViewStatistique.Columns[0]);

                    }

                    //chartStat.Visible = false;
                    //chartTime.Visible = false;
                }

                // https://stackoverflow.com/questions/1941713/datagridview-row-formatting-based-on-data
                // https://stackoverflow.com/questions/27787381/chart-vertical-line-and-string-on-x-label
                // https://stackoverflow.com/questions/25801257/c-sharp-line-chart-how-to-create-vertical-line
            }
        }

        private StatValues EcartType(StatValues t)
        {
            StatValues V = new StatValues();
            double delta;

            // calcul de la moyenne ok
            V.Somme = 0;
            for (int i = 0; i < t.intList.Count-1; i++)
            {
                V.Somme += t.intList[i];
            }
            V.Moyenne = V.Somme / (t.intList.Count);

            // Calcul écart type ok
            V.Ecart_type = 0.0;
            V.Somme = 0;
            for (int i = 0; i < t.intList.Count-1; i++)
            {
                delta = (double)t.intList[i] - V.Moyenne;
                V.Somme += delta * delta;
                V.Ecart_moyen += delta;
            }
            V.Variance = V.Somme / (t.intList.Count - 1);
            V.Ecart_type = Math.Sqrt(V.Variance);

            // Calcul écart moyen
            V.Ecart_moyen = V.Ecart_moyen / (t.intList.Count - 1);

            return V;
        }

        private void bUpdate_Click(object sender, EventArgs e)
        {
            rBgrille_CheckedChanged(sender,e);
        }

        private void rBgrille_CheckedChanged(object sender, EventArgs e)
        {
            AffichageGrid();
            rdraw();
        }

        private void rdraw() // taille du dbGestlap
        {
            if (dGbestlap.Width > 12) // pour obtenir la valeur 
                NumCar = NumCar + 1 - 1;
            // 1er colonne doit faire 40 de large
            if (rBgrille.Checked == false)
            {
                Size size = new Size(610, 500); // 574 
                dGbestlap.Size = size;
                tableLayoutPanel3.ColumnStyles[0].Width = size.Width;
                tableLayoutPanelStat.Visible= true;

                Size size2 = new Size(555, 135); // 574 
                dataGridViewStatistique.Size = size2;
                tableLayoutPanelStat.Width= size2.Width;    
            }
            else
            {
                Size size = new Size(1070, 500); // 1064
                dGbestlap.Size = size;
                tableLayoutPanel3.ColumnStyles[0].Width = size.Width;
                tableLayoutPanelStat.Visible = false;
            }
            tableLayoutPanel3.Refresh();
        }

        private void chartTime_MouseMove(object sender, MouseEventArgs e)
        {
            // https://stackoverflow.com/questions/33363565/c-sharp-chart-using-crosshair-cursor
            Point mousePoint = new Point(e.X, e.Y);

            try
            {
                chartTime.ChartAreas[0].CursorX.SetCursorPixelPosition(mousePoint, false);
                chartTime.ChartAreas[0].CursorY.SetCursorPixelPosition(mousePoint, false);
            }
            catch
            {
                tBetat.Text = "chartTime_MouseMove - Erreur lors de la récupération CursorX et CursorY ";
                tBetat.Text = "Un peu de patience le graphique arrive";
            }

            string values = string.Empty;

            var xPosCursor = e.X;
            DataPointCollection points = null;

            for (int i = 0; i < chartTime.Series.Count; i++)
            {
                points = chartTime.Series[i].Points;
            }
            if (points != null) 
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    if ((xPosCursor > points[i].XValue & xPosCursor < points[i + 1].XValue) | xPosCursor > points[i + 1].XValue & xPosCursor < points[i].XValue)
                    {
                        var Yval = (points[i].YValues[0] + points[i + 1].YValues[0]) / 2;

                        values += $"Y= {Yval} {Environment.NewLine}";
                    }
                }
                values += "X=" + " " + String.Format("{0:0.00}", xPosCursor);
            }
            else
                values = "No Curve Selected";

            tBvalue.Text = values;
        }

        private void cBdrivers_SelectedIndexChanged(object sender, EventArgs e)
        {
            NumCar = cBdrivers.SelectedIndex; // Int32.Parse(tBcar.Text);
            rBgrille_CheckedChanged(sender,e);
        }
    }
}
