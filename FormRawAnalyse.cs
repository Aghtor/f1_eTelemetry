using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace f1_eTelemetry
{
    public partial class FormRawAnalyse : Form
    {

        static UDP_f12021 dataf1_2021 = new UDP_f12021();
        Setting dataSetting = new Setting();
        List<byte[]> ListReceived = new List<byte[]>();

        public FormRawAnalyse()
        {
            InitializeComponent();
        }

        public void SendData(UDP_f12021 _Data, Setting _dataSetting)
        {
            dataf1_2021 = _Data;
            dataSetting = _dataSetting; 
        }

        public void SendRawData(List<byte[]> _listReceived)
        {
            ListReceived = _listReceived;
        }

        private void Traitement()
        {
            foreach (var ElemReceived in ListReceived)
            {
                Stream stream = new MemoryStream(ElemReceived);
                var binaryReaderRAW = new BinaryReader(stream);

                dataf1_2021.InitPackets(stream, binaryReaderRAW);
            }

           for (int i = 0; i < 12; i++)
                rTBterminal.AppendText("Packet [" + i + "] : " + dataf1_2021.packet[i] + "\n");
        }

        private void traitementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Traitement();
        }

        private void SplitMultiRace()
        {
            byte m_formula = 0;
            byte m_sessionType = 0;
            sbyte m_trackId = 0;
            byte m_totalLaps = 0;
            byte m_networkGame = 0;
            byte m_aiDifficulty = 0;

            dGVanalyse.Rows.Clear();
            dGVanalyse.Columns.Clear();

            dGVanalyse.Columns.Add("Index", "Index");
            dGVanalyse.Columns.Add("Packet", "Packet");
            dGVanalyse.Columns.Add("Session formula", "Session formula"); // m_formula
            dGVanalyse.Columns.Add("Session type", "Session type"); // m_sessionType
            dGVanalyse.Columns.Add("Circuit", "Circuit"); // m_trackId
            dGVanalyse.Columns.Add("Total Lap", "Nb Tour"); //m_totalLaps
            dGVanalyse.Columns.Add("m_networkGame", "m_networkGame"); // m_networkGame
            dGVanalyse.Columns.Add("m_aiDifficulty", "m_aiDifficulty"); // m_aiDifficulty

            for (int i = 0; i < dataf1_2021.ListSession.Count; i++)
            {
                if (    (m_formula != dataf1_2021.ListSession[i].m_formula) ||
                        (m_sessionType != dataf1_2021.ListSession[i].m_sessionType) ||
                        (m_trackId != dataf1_2021.ListSession[i].m_trackId) ||
                        (m_totalLaps != dataf1_2021.ListSession[i].m_totalLaps) ||
                        (m_networkGame != dataf1_2021.ListSession[i].m_networkGame) ||
                        (m_aiDifficulty != dataf1_2021.ListSession[i].m_aiDifficulty) ||
                        (i == 0)
                    )
                    {
                        if (dataf1_2021.ListSession[i].m_trackId > 21)
                            dGVanalyse.Rows.Add(
                                i,
                                dataf1_2021.ListSession[i].packetHeader.m_packetId,
                                dataf1_2021.formulaname[dataf1_2021.ListSession[i].m_formula],
                                dataf1_2021.sessiontypename[dataf1_2021.ListSession[i].m_sessionType],
                                "New circuit",
                                dataf1_2021.ListSession[i].m_totalLaps,
                                dataf1_2021.ListSession[i].m_networkGame,
                                dataf1_2021.ListSession[i].m_aiDifficulty
                            );
                        else
                            dGVanalyse.Rows.Add(
                                    i,
                                    dataf1_2021.ListSession[i].packetHeader.m_packetId,
                                    dataf1_2021.formulaname[dataf1_2021.ListSession[i].m_formula],
                                    dataf1_2021.sessiontypename[dataf1_2021.ListSession[i].m_sessionType],
                                    dataf1_2021.Trackname[dataf1_2021.ListSession[i].m_trackId],
                                    dataf1_2021.ListSession[i].m_totalLaps,
                                    dataf1_2021.ListSession[i].m_networkGame,
                                    dataf1_2021.ListSession[i].m_aiDifficulty
                                );

                        if (m_formula != dataf1_2021.ListSession[i].m_formula) m_formula = dataf1_2021.ListSession[i].m_formula;
                        if (m_sessionType != dataf1_2021.ListSession[i].m_sessionType) m_sessionType = dataf1_2021.ListSession[i].m_sessionType;
                        if (m_trackId != dataf1_2021.ListSession[i].m_trackId) m_trackId = dataf1_2021.ListSession[i].m_trackId;
                        if (m_totalLaps != dataf1_2021.ListSession[i].m_totalLaps) m_totalLaps = dataf1_2021.ListSession[i].m_totalLaps;
                        if (m_networkGame != dataf1_2021.ListSession[i].m_networkGame) m_networkGame = dataf1_2021.ListSession[i].m_networkGame;
                        if (m_aiDifficulty != dataf1_2021.ListSession[i].m_aiDifficulty) m_aiDifficulty = dataf1_2021.ListSession[i].m_aiDifficulty;
                }
            }

        }

        private void splitMutlRaceToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            SplitMultiRace();
        }

        private void FindPacketOne()
        {
            byte m_formula = 0;
            byte m_sessionType = 0;
            sbyte m_trackId = 0;
            byte m_totalLaps = 0;
            byte m_networkGame = 0;
            byte m_aiDifficulty = 0;
            //byte m_packetId = 0;

            dGVanalyse.Rows.Clear();
            dGVanalyse.Columns.Clear();

            dGVanalyse.Columns.Add("Index", "Index");
            dGVanalyse.Columns.Add("Packet", "Packet");
            dGVanalyse.Columns.Add("Session formula", "Session formula"); // m_formula
            dGVanalyse.Columns.Add("Session type", "Session type"); // m_sessionType
            dGVanalyse.Columns.Add("Circuit", "Circuit"); // m_trackId
            dGVanalyse.Columns.Add("Total Lap", "Nb Tour"); //m_totalLaps
            dGVanalyse.Columns.Add("m_networkGame", "m_networkGame"); // m_networkGame
            dGVanalyse.Columns.Add("m_aiDifficulty", "m_aiDifficulty"); // m_aiDifficulty

            for (int i = 0; i < dataf1_2021.ListSession.Count; i++)
            {
                if ((dataf1_2021.ListSession[i].packetHeader.m_packetId == 1) ||
                        (i == 0)
                    )
                {
                    if (dataf1_2021.ListSession[i].m_trackId > 21)
                        dGVanalyse.Rows.Add(
                            i,
                            dataf1_2021.ListSession[i].packetHeader.m_packetId,
                            dataf1_2021.formulaname[dataf1_2021.ListSession[i].m_formula],
                            dataf1_2021.sessiontypename[dataf1_2021.ListSession[i].m_sessionType],
                            "New circuit",
                            dataf1_2021.ListSession[i].m_totalLaps,
                            dataf1_2021.ListSession[i].m_networkGame,
                            dataf1_2021.ListSession[i].m_aiDifficulty
                        );
                    else
                        dGVanalyse.Rows.Add(
                                i,
                                dataf1_2021.ListSession[i].packetHeader.m_packetId,
                                dataf1_2021.formulaname[dataf1_2021.ListSession[i].m_formula],
                                dataf1_2021.sessiontypename[dataf1_2021.ListSession[i].m_sessionType],
                                dataf1_2021.Trackname[dataf1_2021.ListSession[i].m_trackId],
                                dataf1_2021.ListSession[i].m_totalLaps,
                                dataf1_2021.ListSession[i].m_networkGame,
                                dataf1_2021.ListSession[i].m_aiDifficulty
                            );

                    if (m_formula != dataf1_2021.ListSession[i].m_formula) m_formula = dataf1_2021.ListSession[i].m_formula;
                    if (m_sessionType != dataf1_2021.ListSession[i].m_sessionType) m_sessionType = dataf1_2021.ListSession[i].m_sessionType;
                    if (m_trackId != dataf1_2021.ListSession[i].m_trackId) m_trackId = dataf1_2021.ListSession[i].m_trackId;
                    if (m_totalLaps != dataf1_2021.ListSession[i].m_totalLaps) m_totalLaps = dataf1_2021.ListSession[i].m_totalLaps;
                    if (m_networkGame != dataf1_2021.ListSession[i].m_networkGame) m_networkGame = dataf1_2021.ListSession[i].m_networkGame;
                    if (m_aiDifficulty != dataf1_2021.ListSession[i].m_aiDifficulty) m_aiDifficulty = dataf1_2021.ListSession[i].m_aiDifficulty;
                }
            }

        }

        private void packetOneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FindPacketOne();
        }
    }
}
