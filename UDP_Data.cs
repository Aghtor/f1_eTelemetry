using f1_eTelemetry.rFactor2Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reactive;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Devices.Lights;
using Windows.UI.Xaml.Controls.Maps;
using static f1_eTelemetry.eTelemetry;
using static f1_eTelemetry.FormTelemetry;
using static f1_eTelemetry.rFactor2Constants;
using static f1_eTelemetry.UDP_Data;

namespace f1_eTelemetry
{
    public class UDP_Data
    {
        int NumPlayer =22; //22 pour F1 202x et variable pour les autres

        public int Simulation; // 0 F1 - 1 PC2 - 2 RF2 - 3 ACC - 4 AC 5 Raceroom - 6 Forza

        public UDP_f12021 dataf1_2021 = new UDP_f12021();
        public List<PCars2_UDP> ListDataPC2;
        public UDP_RF2 dataRF2 = new UDP_RF2();

        public struct TelemetryData_Vehicles
        {

            public float m_speed;          // Speed of car in kilometres per hour
            public float m_throttle;       // Amount of throttle applied (0 to 100)
            public float m_steer;          // Steering (-100 (full lock left) to 100 (full lock right))
            public float m_brake;          // Amount of brake applied (0 to 100)
            public float m_clutch;         // Amount of clutch applied (0 to 100)
            public sbyte m_gear;           // Gear selected (1-8, N=0, R=-1)
            public int m_engineRPM;
            public byte m_drs;             // 0 = off, 1 = on
            public ushort m_revLightsPercent;          // Rev lights indicator (percentage)
            public byte m_revLightsBitValue;
            public ushort[] m_brakesTemperature;      // Brakes temperature (celsius)
            public ushort[] m_tyresSurfaceTemperature;// surface wheel temperature (celsius)
            public ushort[] m_tyresInnerTemperature;   // Inner wheel temperature (celsius)
            public uint m_engineTemperature;           // Engine temperature (celsius)
            public float[] m_tyresPressure;            // Tyres pressure (PSI)
            public byte[] m_surfaceType;
            public float m_worldPositionX;
            public float m_worldPositionY;
            public float m_worldPositionZ;
            public float m_worldVelocityX;
            public float m_worldVelocityY;
            public float m_worldVelocityZ;
            public float m_worldForwardDirX;
            public float m_worldForwardDirY;
            public float m_worldForwardDirZ;
            public float m_worldRightDirX;
            public float m_worldRightDirY;
            public float m_worldRightDirZ;
            public float m_gForceLateral;
            public float m_gForceLongitudinal;
            public float m_gForceVertical;
            public float m_yaw;
            public float m_pitch;
            public float m_roll;
            public float[] m_suspensionPosition;
            public float[] m_suspensionVelocity;
            public float[] m_suspensionAcceleration;
            public float[] m_wheelSpeed;
            public float[] m_wheelSlip;
            public float[] mTireLoad;
            public float[] mWear;
            public float localVelocityX;
            public float localVelocityY;
            public float localVelocityZ;
            public float angularVelocityX;
            public float angularVelocityY;
            public float angularVelocityZ;
            public float angularAccelerationX;
            public float angularAccelerationY;
            public float angularAccelerationZ;
            public float frontWheelsAngle;

            public string mDriverName;
            public int mID;
            public uint InternalmID;

            internal int mLapNumber;
            internal float mLapDist;
            internal byte mPlace;
            internal short mTotalLaps;
            internal byte m_gridPosition;
            internal float mElapsedTime;
            internal sbyte mControl;
            internal byte m_resultStatus;
            internal int bestLapTimeLap;
            internal float bestLapTime;
            internal float mBestSector1;
            internal float mBestSector2;
            internal float mBestSector3;

            internal float mLastSector1;
            internal float mLastSector2;
            internal float mLastSector3;
            internal float mLastLapTime;


            internal string mVehicleName;
            internal string mVehicleClass;
            internal byte mCountLapFlag; // 0 = do not count lap or time, 1 = count lap but not time, 2 = count lap and time
        }
        public struct TelemetryDriver
        {
            public string mDriverName;
            public int mID;
            public uint InternalmID;
        }
        public struct TelemetryData
        {
            public uint IndexFrame;                     //uint	de 0 à 4 294 967 295	Entier 32 bits non signé
            public int NbVehicles;                      // nombre de véhicule
            public int NbPlayer;                        // numéro du joueur
            public string mTrackName;
            public float mLapDist;
            public TelemetryData_Vehicles[] mVehicles;
            internal float mTrackTemp;
            internal int mMaxLaps;
            public byte mGameMode;
            public double mAmbientTemp;
            public double mRaining;
            public double mDarkCloud;
            public rF2Vec3 mWind;
            public byte mGamePhase;
            public string mServerName;
            public int mSession;
            public float mCloudiness;
            public double mEndET;

        }


        // list envoyée à Form télémétrie pour le traitement des données
        public List<TelemetryData> ListTelemetryData = new List<TelemetryData>();
        public List<TelemetryDriver>ListDriver = new List<TelemetryDriver>();

        // F1 2021
        public void DetelePackets()
        {
            dataf1_2021.DeletePackets();
        }

        // F1 2021
        public void InitPackets(Stream stream, BinaryReader binaryReader)
        {
            dataf1_2021.InitPackets(stream, binaryReader);
        }

        // F1 2021
        public void InitPackets(rF2Data data)
        {
            // transfert les données RF2 vers la structure de données F1
            Telemetry_packet dataTelemetry = new Telemetry_packet();
            Historic_packet dataHistoric = new Historic_packet();
            CarDamage_packet dataCarDamage = new CarDamage_packet();
            PacketLapData dataLapData = new PacketLapData();
            Players_packet dataPlayers = new Players_packet();
            Motion_packet dataMotion = new Motion_packet();
            Session_packet dataSession = new Session_packet();

            var playerVeh = new rF2VehicleTelemetry();
            var playerVehScoring = new rF2VehicleScoring();

            //dataf1_2021.InitPackets(stream, binaryReader);
            for (int i = 0; i < 22; i++) 
            {
                playerVeh = data.telemetry.mVehicles[i];
                playerVehScoring = data.scoring.mVehicles[i];

                // Calculate derivatives:
                var yaw = Math.Atan2(playerVeh.mOri[RowZ].x, playerVeh.mOri[RowZ].z);

                var pitch = Math.Atan2(-playerVeh.mOri[RowY].z,
                    Math.Sqrt(playerVeh.mOri[RowX].z * playerVeh.mOri[RowX].z + playerVeh.mOri[RowZ].z * playerVeh.mOri[RowZ].z));

                var roll = Math.Atan2(playerVeh.mOri[RowY].x,
                    Math.Sqrt(playerVeh.mOri[RowX].x * playerVeh.mOri[RowX].x + playerVeh.mOri[RowZ].x * playerVeh.mOri[RowZ].x));

                var speed = Math.Sqrt((playerVeh.mLocalVel.x * playerVeh.mLocalVel.x)
                    + (playerVeh.mLocalVel.y * playerVeh.mLocalVel.y)
                    + (playerVeh.mLocalVel.z * playerVeh.mLocalVel.z)) * 3.6; // conversion nécessaire pour m/s en KM/h

                dataMotion.m_carMotionData[i].m_worldPositionX = (float)playerVeh.mPos.x;
                dataMotion.m_carMotionData[i].m_worldPositionY = (float)playerVeh.mPos.y;
                dataMotion.m_carMotionData[i].m_worldPositionZ = (float)playerVeh.mPos.z;
                dataMotion.m_carMotionData[i].m_gForceLateral = (float)playerVeh.mFrontDownforce;
                dataMotion.m_carMotionData[i].m_gForceLongitudinal = (float)playerVeh.mFront3rdDeflection;
                dataMotion.m_carMotionData[i].m_gForceVertical = (float)playerVeh.mRear3rdDeflection;

                dataTelemetry.m_carTelemetryData[i].m_speed = (ushort)speed;
                dataMotion.m_carMotionData[i].m_yaw = (float)yaw;
                dataMotion.m_carMotionData[i].m_roll = (float)roll;
                dataMotion.m_carMotionData[i].m_pitch = (float)pitch;

                dataTelemetry.m_carTelemetryData[i].m_throttle = (float)(playerVeh.mUnfilteredThrottle*100.0);
                dataTelemetry.m_carTelemetryData[i].m_steer = (float)(playerVeh.mUnfilteredSteering*100);
                dataTelemetry.m_carTelemetryData[i].m_brake = (float)(playerVeh.mUnfilteredBrake*100.0);
                dataTelemetry.m_carTelemetryData[i].m_clutch = (byte)(playerVeh.mUnfilteredClutch*100);
                dataTelemetry.m_carTelemetryData[i].m_gear = (sbyte)playerVeh.mGear;
                dataTelemetry.m_carTelemetryData[i].m_engineRPM = (ushort)playerVeh.mEngineRPM;


                //dataTelemetry.m_carTelemetryData[i].m_drs = (byte)playerVeh.;
                //dataTelemetry.m_carTelemetryData[i].m_revLightsPercent = (byte)playerVeh.;
                //dataTelemetry.m_carTelemetryData[i].m_revLightsBitValue = (ushort)playerVeh.;

                dataTelemetry.m_carTelemetryData[i].m_brakesTemperature = new UInt16[4];
                for (int j = 0; j < 4; j++)
                    dataTelemetry.m_carTelemetryData[i].m_brakesTemperature[j] = (ushort)playerVeh.mWheels[j].mBrakeTemp;

                dataTelemetry.m_carTelemetryData[i].m_tyresSurfaceTemperature = new byte[4];
                for (int j = 0; j < 4; j++)
                    dataTelemetry.m_carTelemetryData[i].m_tyresSurfaceTemperature[j] = (byte)playerVeh.mWheels[j].mTireCarcassTemperature;

                dataTelemetry.m_carTelemetryData[i].m_tyresInnerTemperature = new byte[4];
                for (int j = 0; j < 4; j++)
                    dataTelemetry.m_carTelemetryData[i].m_tyresInnerTemperature[j] = (byte)playerVeh.mWheels[j].mTireInnerLayerTemperature[1];

                dataTelemetry.m_carTelemetryData[i].m_engineTemperature = (ushort)playerVeh.mEngineOilTemp;

                dataTelemetry.m_carTelemetryData[i].m_tyresPressure = new float[4];
                for (int j = 0; j < 4; j++)
                    dataTelemetry.m_carTelemetryData[i].m_tyresPressure[j] = (float)playerVeh.mWheels[j].mPressure;

                dataTelemetry.m_carTelemetryData[i].m_surfaceType = new byte[4];
                for (int j = 0; j < 4; j++)
                    dataTelemetry.m_carTelemetryData[i].m_surfaceType[j] = (byte)playerVeh.mWheels[j].mSurfaceType;

                dataCarDamage.m_carDamageData[i].m_tyresWear = new float[4];
                for (int j = 0; j < 4; j++)
                    dataCarDamage.m_carDamageData[i].m_tyresWear[j]= (float)playerVeh.mWheels[j].mWear;
                dataHistoric.m_bestLapTimeLapNum = 0;

                dataPlayers.m_participants[i].m_s_name = Encoding.Default.GetString(playerVehScoring.mDriverName);//.ToString();
                dataPlayers.m_participants[i].m_driverId = (byte)playerVehScoring.mID;
                dataLapData.m_lapData[i].m_currentLapNum = (byte)playerVeh.mLapNumber;
                dataLapData.m_lapData[i].m_lapDistance = (float)playerVehScoring.mLapDist;
                dataSession.m_trackLength = (ushort)playerVehScoring.mLapDist;
                dataSession.m_totalLaps = (byte)playerVehScoring.mTotalLaps;
                //dataSession.
                //dataSession.m_airTemperature = (byte)playerVehScoring.

            }
            dataLapData.packetHeader.m_playerCarIndex = 1;

            dataf1_2021.ListTelemetry.Add(dataTelemetry);
            dataf1_2021.ListCarDamage.Add(dataCarDamage);
            dataf1_2021.ListHistoric.Add(dataHistoric);
            dataf1_2021.ListLapData.Add(dataLapData);
            dataf1_2021.ListPlayers.Add(dataPlayers);
            dataf1_2021.ListMotion_packet.Add(dataMotion);
            dataf1_2021.ListSession.Add(dataSession);
        }

        // GRID Data
        private object[] BestLapsGridArrayF1(int IndexPlayer)
        {
            object[] dataArray = new object[]
            {
                //Get_m_driverId(0, IndexPlayer), // 0 Changer avec le nom
                Get_m_s_name(IndexPlayer),
                dataf1_2021.ListTeamID[Get_m_teamId(0, IndexPlayer)],   // n'existe pas sur rf2
                Get_m_raceNumber(0, IndexPlayer), // n'existe pas sur rf2 - Numéro de la voiture
                dataf1_2021.Nationality[Get_m_nationality(0, IndexPlayer)], // n'existe pas sur rf2
                TimeSpan.FromMilliseconds(Get_m_bestLapTimeInMS(-1, IndexPlayer)).ToString(),
                TimeSpan.FromMilliseconds(Get_m_bestLapTimeLapNum(IndexPlayer)).ToString(), //  rf2 codé
                TimeSpan.FromMilliseconds(Get_BestSector1TimeInMS(IndexPlayer,out int lap1)).ToString(), //  rf2 codé
                TimeSpan.FromMilliseconds(Get_BestSector2TimeInMS(IndexPlayer,out int lap2)).ToString(), // rf2 codé
                TimeSpan.FromMilliseconds(Get_BestSector3TimeInMS(IndexPlayer,out int lap3)).ToString(),  // rf2 un poil complexe
                TimeSpan.FromMilliseconds(Get_m_BestTheoLapTimeInMS(IndexPlayer)).ToString(),
                TimeSpan.FromMilliseconds(Get_m_totalRaceTime(IndexPlayer)).ToString(),
                (IndexPlayer + 1),
                Get_m_aiControlled(0, IndexPlayer)
            };

            // Ajoutez ici la logique pour modifier ou personnaliser les données en fonction des circonstances

            return dataArray;
        }
        private object[] BestLapsGridArrayRF2(int IndexPlayer)
        {
            double raceTimeInSeconds;
            raceTimeInSeconds = Get_m_totalRaceTime(IndexPlayer);

            int minutes = (int)raceTimeInSeconds / 60;
            double seconds = raceTimeInSeconds % 60;
            string formattedRaceTime = string.Format("{0:00}:{1:00.000}", minutes, seconds);


            object[] dataArray = new object[]
            {
                Get_m_s_name(IndexPlayer),
                // Get_m_teamId(0, IndexPlayer),    // n'existe pas sur rf2
                Get_m_raceNumber(0, IndexPlayer),  //n'existe pas sur rf2 - Numéro de la voiture
                // Get_m_nationality(0, IndexPlayer), // n'existe pas sur rf2
                TimeSpan.FromMilliseconds(Get_m_bestLapTimeInMS(0, IndexPlayer)).ToString(),
                Get_m_bestLapTimeLapNum(IndexPlayer), //  rf2 codé
                TimeSpan.FromMilliseconds(Get_BestSector1TimeInMS(IndexPlayer,out int lap1)).ToString(), //  rf2 codé
                TimeSpan.FromMilliseconds(Get_BestSector2TimeInMS(IndexPlayer,out int lap2)).ToString(), // rf2 codé
                TimeSpan.FromMilliseconds(Get_BestSector3TimeInMS(IndexPlayer,out int lap3)).ToString(),  // rf2 un poil complexe
                TimeSpan.FromMilliseconds(Get_m_BestTheoLapTimeInMS(IndexPlayer)).ToString(),
                TimeSpan.FromMilliseconds(Get_m_totalRaceTime(IndexPlayer)).ToString(),
                (IndexPlayer + 1),
                Get_m_aiControlled(0, IndexPlayer)
            };

            // Ajoutez ici la logique pour modifier ou personnaliser les données en fonction des circonstances

            return dataArray;
        }
        public object[] BestLapsGridArray(int IndexPlayer)
        {
            switch (Simulation)
            {
                case 0: return BestLapsGridArrayF1(IndexPlayer);
                case 2: return BestLapsGridArrayRF2(IndexPlayer);
            }
            return BestLapsGridArrayF1(IndexPlayer);
        }

        // GRID Driver
        public DataGridView GridArrayDefineDriverColumnF1(DataGridView dGbestlap)
        {
            dGbestlap.Rows.Clear();
            dGbestlap.Columns.Clear();

            //https://stackoverflow.com/questions/10063770/how-to-add-a-new-row-to-datagridview-programmatically

            DataGridViewTextBoxColumn Lap = new DataGridViewTextBoxColumn();
            Lap.HeaderText = "Lap N°";
            Lap.Name = "LapNum";
            Lap.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            Lap.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            Lap.Width = 150;
            dGbestlap.Columns.Add(Lap);

            DataGridViewTextBoxColumn m_bestSector1LapNum = new DataGridViewTextBoxColumn();
            m_bestSector1LapNum.HeaderText = "Sector 1";
            m_bestSector1LapNum.Name = "m_bestSector1LapNum";
            m_bestSector1LapNum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_bestSector1LapNum.ValueType = typeof(string); //typeof(int); // Indiquez que c'est une colonne numérique
            m_bestSector1LapNum.Width = 150;
            m_bestSector1LapNum.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_bestSector1LapNum);

            DataGridViewTextBoxColumn m_bestSector2LapNum = new DataGridViewTextBoxColumn();
            m_bestSector2LapNum.HeaderText = "Sector 2";
            m_bestSector2LapNum.Name = "m_bestSector2LapNum";
            m_bestSector2LapNum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_bestSector2LapNum.ValueType = typeof(string); //typeof(int); // Indiquez que c'est une colonne numérique
            m_bestSector2LapNum.Width = 150;
            m_bestSector2LapNum.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_bestSector2LapNum);

            DataGridViewTextBoxColumn m_bestSector3LapNum = new DataGridViewTextBoxColumn();
            m_bestSector3LapNum.HeaderText = "Sector 3";
            m_bestSector3LapNum.Name = "m_bestSector3LapNum";
            m_bestSector3LapNum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_bestSector3LapNum.ValueType = typeof(string); //typeof(int); // Indiquez que c'est une colonne numérique
            m_bestSector3LapNum.Width = 150;
            m_bestSector3LapNum.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_bestSector3LapNum);


            DataGridViewTextBoxColumn LapTimeInMS = new DataGridViewTextBoxColumn();
            LapTimeInMS.HeaderText = "lap Time";
            LapTimeInMS.Name = "m_bestLapNum";
            LapTimeInMS.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            LapTimeInMS.ValueType = typeof(string); //typeof(int); // Indiquez que c'est une colonne numérique
            LapTimeInMS.Width = 150;
            LapTimeInMS.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(LapTimeInMS);

            DataGridViewTextBoxColumn Valid = new DataGridViewTextBoxColumn();
            Valid.HeaderText = "Valid";
            Valid.Name = "Valid";
            Valid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            Valid.ValueType = typeof(string); //typeof(int); // Indiquez que c'est une colonne numérique
            Valid.Width = 150;
            dGbestlap.Columns.Add(Valid);

            return dGbestlap;
        }
        public DataGridView GridArrayDefineDriverColumnRF2(DataGridView dGbestlap)
        {
            dGbestlap.Rows.Clear();
            dGbestlap.Columns.Clear();

            //https://stackoverflow.com/questions/10063770/how-to-add-a-new-row-to-datagridview-programmatically

            DataGridViewTextBoxColumn Lap = new DataGridViewTextBoxColumn();
            Lap.HeaderText = "Best Lap N°";
            Lap.Name = "m_bestLapTimeLapNum";
            Lap.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            Lap.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            Lap.Width = 150;
            dGbestlap.Columns.Add(Lap);

            DataGridViewTextBoxColumn m_bestSector1LapNum = new DataGridViewTextBoxColumn();
            m_bestSector1LapNum.HeaderText = "Sector 1";
            m_bestSector1LapNum.Name = "m_bestSector1LapNum";
            m_bestSector1LapNum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_bestSector1LapNum.ValueType = typeof(string); //typeof(int); // Indiquez que c'est une colonne numérique
            m_bestSector1LapNum.Width = 150;
            m_bestSector1LapNum.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_bestSector1LapNum);

            DataGridViewTextBoxColumn m_bestSector2LapNum = new DataGridViewTextBoxColumn();
            m_bestSector2LapNum.HeaderText = "Sector 2";
            m_bestSector2LapNum.Name = "m_bestSector2LapNum";
            m_bestSector2LapNum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_bestSector2LapNum.ValueType = typeof(string); //typeof(int); // Indiquez que c'est une colonne numérique
            m_bestSector2LapNum.Width = 150;
            m_bestSector2LapNum.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_bestSector2LapNum);

            DataGridViewTextBoxColumn m_bestSector3LapNum = new DataGridViewTextBoxColumn();
            m_bestSector3LapNum.HeaderText = "Sector 3";
            m_bestSector3LapNum.Name = "m_bestSector3LapNum";
            m_bestSector3LapNum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_bestSector3LapNum.ValueType = typeof(string); //typeof(int); // Indiquez que c'est une colonne numérique
            m_bestSector3LapNum.Width = 150;
            m_bestSector3LapNum.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_bestSector3LapNum);


            DataGridViewTextBoxColumn LapTimeInMS = new DataGridViewTextBoxColumn();
            LapTimeInMS.HeaderText = "Lap";
            LapTimeInMS.Name = "LapTime";
            LapTimeInMS.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            LapTimeInMS.ValueType = typeof(string); //typeof(int); // Indiquez que c'est une colonne numérique
            LapTimeInMS.Width = 150;
            LapTimeInMS.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(LapTimeInMS);

            DataGridViewTextBoxColumn Valid = new DataGridViewTextBoxColumn();
            Valid.HeaderText = "Valid";
            Valid.Name = "Valid";
            Valid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            Valid.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            Valid.Width = 150;
            dGbestlap.Columns.Add(Valid);

            return dGbestlap;
        }
        public DataGridView GridArrayDefineDriverColumn(DataGridView dGbestlap)
        {
            switch (Simulation)
            {
                case 0: return GridArrayDefineDriverColumnF1(dGbestlap);
                case 2: return GridArrayDefineDriverColumnRF2(dGbestlap);
            }
            return dGbestlap;
        }
        public DataGridView GridArrayDefineSizeDriverColumnF1(DataGridView dGbestlap)
        {
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

            return dGbestlap;
        }
        public DataGridView GridArrayDefineSizeDriverColumnRF2(DataGridView dGbestlap)
        {
            dGbestlap.AutoResizeColumns();
            dGbestlap.Columns[0].Width = 100;
            //dGbestlap.Columns[1].Width = 100;
            dGbestlap.Columns[1].Width = 30;
            //dGbestlap.Columns[3].Width = 100;
            dGbestlap.Columns[2].Width = 100;
            dGbestlap.Columns[3].Width = 30;

            dGbestlap.Columns[4].Width = 100;
            dGbestlap.Columns[5].Width = 100;
            dGbestlap.Columns[6].Width = 100;
            dGbestlap.Columns[7].Width = 100;
            dGbestlap.Columns[8].Width = 100;
            dGbestlap.Columns[9].Width = 30;
            dGbestlap.Columns[10].Width = 60;

            return dGbestlap;
        }
        public DataGridView GridArrayDefineSizeDriverColumn(DataGridView dGbestlap)
        {
            switch (Simulation)
            {
                case 0: return GridArrayDefineSizeDriverColumnF1(dGbestlap);
                case 2: return GridArrayDefineSizeDriverColumnRF2(dGbestlap);
            }
            return dGbestlap;
        }


        // DataGrid Grille
        public DataGridView GridArrayDefineGrilleColumnF1(DataGridView dGbestlap)
        {
            dGbestlap.Rows.Clear();
            dGbestlap.Columns.Clear();

            //https://stackoverflow.com/questions/10063770/how-to-add-a-new-row-to-datagridview-programmatically

            dGbestlap.Columns.Add("m_driverId", "Driver name");
            dGbestlap.Columns.Add("m_teamId", "Team");
            dGbestlap.Columns.Add("m_raceNumber", "N°");
            dGbestlap.Columns.Add("m_nationality", "Nationality");

            DataGridViewTextBoxColumn m_bestLapTimeLap = new DataGridViewTextBoxColumn();
            m_bestLapTimeLap.HeaderText = "Best Lap Time";
            m_bestLapTimeLap.Name = "m_bestLapTimeLap";
            m_bestLapTimeLap.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_bestLapTimeLap.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            m_bestLapTimeLap.Width = 150;
            m_bestLapTimeLap.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_bestLapTimeLap);

            DataGridViewTextBoxColumn m_bestLapTimeLapNum = new DataGridViewTextBoxColumn();
            m_bestLapTimeLapNum.HeaderText = "Best Lap N°";
            m_bestLapTimeLapNum.Name = "m_bestLapTimeLapNum";
            m_bestLapTimeLapNum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_bestLapTimeLapNum.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            m_bestLapTimeLapNum.Width = 150;
            dGbestlap.Columns.Add(m_bestLapTimeLapNum);

            DataGridViewTextBoxColumn m_bestSector1LapNum = new DataGridViewTextBoxColumn();
            m_bestSector1LapNum.HeaderText = "Sector 1";
            m_bestSector1LapNum.Name = "m_bestSector1LapNum";
            m_bestSector1LapNum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_bestSector1LapNum.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            m_bestSector1LapNum.Width = 150;
            m_bestSector1LapNum.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_bestSector1LapNum);

            DataGridViewTextBoxColumn m_bestSector2LapNum = new DataGridViewTextBoxColumn();
            m_bestSector2LapNum.HeaderText = "Sector 2";
            m_bestSector2LapNum.Name = "m_bestSector2LapNum";
            m_bestSector2LapNum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_bestSector2LapNum.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            m_bestSector2LapNum.Width = 150;
            m_bestSector2LapNum.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_bestSector2LapNum);

            DataGridViewTextBoxColumn m_bestSector3LapNum = new DataGridViewTextBoxColumn();
            m_bestSector3LapNum.HeaderText = "Sector 3";
            m_bestSector3LapNum.Name = "m_bestSector3LapNum";
            m_bestSector3LapNum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_bestSector3LapNum.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            m_bestSector3LapNum.Width = 150;
            m_bestSector3LapNum.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_bestSector3LapNum);

            DataGridViewTextBoxColumn m_theoLapTimeInMS = new DataGridViewTextBoxColumn();
            m_theoLapTimeInMS.HeaderText = "Best Theo Lap";
            m_theoLapTimeInMS.Name = "m_theoLapTimeInMS";
            m_theoLapTimeInMS.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_theoLapTimeInMS.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            m_theoLapTimeInMS.Width = 150;
            m_theoLapTimeInMS.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_theoLapTimeInMS);


            DataGridViewTextBoxColumn m_totalRaceTime = new DataGridViewTextBoxColumn();
            m_totalRaceTime.HeaderText = "Total Race Time";
            m_totalRaceTime.Name = "m_totalRaceTime";
            m_totalRaceTime.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_totalRaceTime.ValueType = typeof(DateTime); // Indiquez que c'est une colonne numérique
            m_totalRaceTime.Width = 150;
            m_totalRaceTime.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_totalRaceTime);

            dGbestlap.Columns.Add("IndexPlayer", "i");
            dGbestlap.Columns.Add("m_aiControlled", "AI");

            return dGbestlap;
        }
        public DataGridView GridArrayDefineGrilleColumnRF2(DataGridView dGbestlap)
        {
            dGbestlap.Rows.Clear();
            dGbestlap.Columns.Clear();

            //https://stackoverflow.com/questions/10063770/how-to-add-a-new-row-to-datagridview-programmatically

            dGbestlap.Columns.Add("m_driverId", "Driver name");
            //dGbestlap.Columns.Add("m_teamId", "Team");
            dGbestlap.Columns.Add("m_raceNumber", "N°");
            //dGbestlap.Columns.Add("m_nationality", "Nationality");

            DataGridViewTextBoxColumn m_bestLapTimeLap = new DataGridViewTextBoxColumn();
            m_bestLapTimeLap.HeaderText = "Best Lap Time";
            m_bestLapTimeLap.Name = "m_bestLapTimeLap";
            m_bestLapTimeLap.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_bestLapTimeLap.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            m_bestLapTimeLap.Width = 150;
            m_bestLapTimeLap.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_bestLapTimeLap);

            DataGridViewTextBoxColumn m_bestLapTimeLapNum = new DataGridViewTextBoxColumn();
            m_bestLapTimeLapNum.HeaderText = "Best Lap N°";
            m_bestLapTimeLapNum.Name = "m_bestLapTimeLapNum";
            m_bestLapTimeLapNum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_bestLapTimeLapNum.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            m_bestLapTimeLapNum.Width = 150;
            dGbestlap.Columns.Add(m_bestLapTimeLapNum);

            DataGridViewTextBoxColumn m_bestSector1LapNum = new DataGridViewTextBoxColumn();
            m_bestSector1LapNum.HeaderText = "Sector 1";
            m_bestSector1LapNum.Name = "m_bestSector1LapNum";
            m_bestSector1LapNum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_bestSector1LapNum.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            m_bestSector1LapNum.Width = 150;
            m_bestSector1LapNum.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_bestSector1LapNum);

            DataGridViewTextBoxColumn m_bestSector2LapNum = new DataGridViewTextBoxColumn();
            m_bestSector2LapNum.HeaderText = "Sector 2";
            m_bestSector2LapNum.Name = "m_bestSector2LapNum";
            m_bestSector2LapNum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_bestSector2LapNum.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            m_bestSector2LapNum.Width = 150;
            m_bestSector2LapNum.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_bestSector2LapNum);

            DataGridViewTextBoxColumn m_bestSector3LapNum = new DataGridViewTextBoxColumn();
            m_bestSector3LapNum.HeaderText = "Sector 3";
            m_bestSector3LapNum.Name = "m_bestSector3LapNum";
            m_bestSector3LapNum.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_bestSector3LapNum.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            m_bestSector3LapNum.Width = 150;
            m_bestSector3LapNum.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_bestSector3LapNum);

            DataGridViewTextBoxColumn m_theoLapTimeInMS = new DataGridViewTextBoxColumn();
            m_theoLapTimeInMS.HeaderText = "Best Theo Lap";
            m_theoLapTimeInMS.Name = "m_theoLapTimeInMS";
            m_theoLapTimeInMS.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_theoLapTimeInMS.ValueType = typeof(int); // Indiquez que c'est une colonne numérique
            m_theoLapTimeInMS.Width = 150;
            m_theoLapTimeInMS.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_theoLapTimeInMS);


            DataGridViewTextBoxColumn m_totalRaceTime = new DataGridViewTextBoxColumn();
            m_totalRaceTime.HeaderText = "Total Race Time";
            m_totalRaceTime.Name = "m_totalRaceTime";
            m_totalRaceTime.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            m_totalRaceTime.ValueType = typeof(DateTime); // Indiquez que c'est une colonne numérique
            m_totalRaceTime.Width = 150;
            m_totalRaceTime.DefaultCellStyle.Format = "N3"; // Format pour trois chiffres après la virgule
            dGbestlap.Columns.Add(m_totalRaceTime);

            dGbestlap.Columns.Add("IndexPlayer", "index");
            dGbestlap.Columns.Add("m_aiControlled", "AI");

            return dGbestlap;
        }
        public DataGridView GridArrayDefineGrilleColumn(DataGridView dGbestlap)
        {
            switch (Simulation)
            {
                case 0: return GridArrayDefineGrilleColumnF1(dGbestlap);
                case 2: return GridArrayDefineGrilleColumnRF2(dGbestlap); 
            }
            return dGbestlap;
        }
        public DataGridView GridArrayDefineSizeGrilleColumnF1(DataGridView dGbestlap)
        {
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

            return dGbestlap;
        }
        public DataGridView GridArrayDefineSizeGrilleColumnRF2(DataGridView dGbestlap)
        {
            dGbestlap.AutoResizeColumns();
            dGbestlap.Columns[0].Width = 100;
            //dGbestlap.Columns[1].Width = 100;
            dGbestlap.Columns[1].Width = 30;
            //dGbestlap.Columns[3].Width = 100;
            dGbestlap.Columns[2].Width = 100;
            dGbestlap.Columns[3].Width = 30;

            dGbestlap.Columns[4].Width = 100;
            dGbestlap.Columns[5].Width = 100;
            dGbestlap.Columns[6].Width = 100;
            dGbestlap.Columns[7].Width = 100;
            dGbestlap.Columns[8].Width = 100;
            dGbestlap.Columns[9].Width = 30;
            dGbestlap.Columns[10].Width = 60;

            return dGbestlap;
        }
        public DataGridView GridArrayDefineSizeGrilleColumn(DataGridView dGbestlap)
        {
            switch (Simulation)
            {
                case 0: return GridArrayDefineSizeGrilleColumnF1(dGbestlap);
                case 2: return GridArrayDefineSizeGrilleColumnRF2(dGbestlap);
            }
            return dGbestlap;
        }

        // Uniquement pour F1
        public void DriverListUpdate()
        {
            for (int i = 0; i < 22; i++)
            {

                TelemetryDriver telemetryDriver = new TelemetryDriver
                {
                    mID = i,
                    mDriverName = dataf1_2021.Trackdriver[i],
                    InternalmID = (uint)i
                };
                ListDriver.Add(telemetryDriver);
            }
        }
        public void DriverListUpdate(TelemetryData telemetryData)
        {
            for (int i = 0; i < telemetryData.NbVehicles; i++)
            {
                //if (telemetryData.mVehicles[i].InternalmID;
                uint currentInternalmID = telemetryData.mVehicles[i].InternalmID;

                // Vérifier si currentInternalmID n'existe pas dans ListDriver
                if (!ListDriver.Exists(data => data.InternalmID == currentInternalmID))
                {
                    TelemetryDriver telemetryDriver = new TelemetryDriver
                    {
                        mID = telemetryData.mVehicles[i].mID,
                        mDriverName = telemetryData.mVehicles[i].mDriverName,
                        InternalmID = telemetryData.mVehicles[i].InternalmID
                    };
                    ListDriver.Add(telemetryDriver);
                }       
            }
        }

        // GET FUNCTIONS
        public void SelectSimulation(int simulation)
        {
            Simulation = simulation; // 0 F1 - 1 PC2 - 2 RF2 - 3 ACC - 4 AC 5 Raceroom - 6 Forza
        }
        public bool checkdata()
        {
            switch (Simulation)
            {
                case 0:
                    return dataf1_2021.checkData();
                case 1:
                    return false;
                case 2:
                    return (ListTelemetryData.Count > 0);
                    return dataRF2.checkData();
                case 3:
                    return false;
            }
            return dataf1_2021.checkData();
        }
        public string CleanString(string input)
        {
            string output;

            int nullTerminatorIndex = input.IndexOf('\0');

            if (nullTerminatorIndex >= 0)
            {
                output = input.Substring(0, nullTerminatorIndex);
                // Maintenant, "cleanedTrackName" contient la chaîne "Bahrain" sans les caractères après le premier caractère nul.
            }
            else
            {
                // Si aucun caractère nul n'est trouvé, vous pouvez utiliser la chaîne d'origine.
                output = input;
            }

            return output;
        }
        public double GetKtime()
        {
            switch (Simulation)
            {
                case 0: return 1000.0;// * 60 * 60 * 24;
                case 1: return 0;
                case 2: return 1000.0;
                case 3: return 0;
            }
            return 1;
        }
        public int Get_NumofPlayers()
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListFinal[0].m_numCars; //22;
                case 1: return 0;
                case 2:
                    return ListTelemetryData.Max(data => data.NbVehicles); ; //  ListTelemetryData[0].NbVehicles;
                    return dataRF2.scoring[dataRF2.scoring.Count-1].mScoringInfo.mNumVehicles;
                case 3: return 0;
            }
            return 22;
        }
        public int Get_ListLapData_count()
        {
            
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListLapData.Count;
                case 1: return 0;
                case 2: return ListTelemetryData.Count;
                case 3: return 0;
            }
            return 0;
        }
        public int Get_ListTelemetry_count()
        {
            
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListTelemetry.Count;
                case 1: return 0;
                case 2: return ListTelemetryData.Count;
                case 3: return 0;
            }
            return 0;
        }
        public int Get_ListHistoric_count()
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListHistoric.Count;
                case 1: return 0;
                case 2: return ListTelemetryData.Count;
                case 3: return 0;
            }
            return dataf1_2021.ListHistoric.Count;
        }
        public int Get_ListMotion_packet_count()
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet.Count;
                case 1: return 0;
                case 2: return ListTelemetryData.Count;
                case 3: return 0;
            }
            return dataf1_2021.ListMotion_packet.Count;
        }
        public int Get_ListEvent_count()
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListEvent.Count;
                case 1: return 0;
                case 2: return dataRF2.scoring.Count;
                case 3: return 0;
            }
            return dataf1_2021.ListEvent.Count;
        }
        public int Get_ListDataTelemetryCount()
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListData1.Count;
                case 1: return 0;
                case 2: return dataRF2.ListDataRF2_1.Count; 
                case 3: return 0;
            }
            return dataf1_2021.ListEvent.Count;
        }
        public string Get_ListDataTelemetry(int index)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListData1[index];
                case 1: return "erreur";
                case 2: return dataRF2.ListDataRF2_1[index];
                case 3: return "erreur";
            }
            return dataf1_2021.ListData1[index];
        }
        public ushort Get_m_packetFormat()
        {
            switch (Simulation)
            {
                case 0:
                    return dataf1_2021.ListCarDamage[0].packetHeader.m_packetFormat;
                case 1:
                    return 0;
                case 2:
                    return 0;
                case 3:
                    return 0;
            }
            return dataf1_2021.ListCarDamage[0].packetHeader.m_packetFormat;
        }
        public ushort Get_m_packetVersion()
        {
            switch (Simulation)
            {
                case 0:
                    return dataf1_2021.ListCarDamage[0].packetHeader.m_packetVersion;
                case 1:
                    return 0;
                case 2:
                    return 0;
                case 3:
                    return 0;
            }
            return dataf1_2021.ListCarDamage[0].packetHeader.m_packetVersion;
        }
        public int Get_packet(int i)
        {
            switch (Simulation)
            {
                case 0:
                    return dataf1_2021.packet[i];
                case 1:
                    return 0;
                case 2:
                    return 0;
                case 3:
                    return 0;
            }
            return dataf1_2021.packet[i];
        }
        public string Get_trackname()
        {
            switch (Simulation)
            {
                case 0:
                    return dataf1_2021.Trackname[dataf1_2021.ListSession[0].m_trackId];
                case 1:
                    return "err";
                case 2:
                    return ListTelemetryData[0].mTrackName;
                    return BitConverter.ToString(dataRF2.telemetryLight[0].mVehicles[0].mTrackName);
                case 3:
                    return "err";
            }
            return dataf1_2021.Trackname[dataf1_2021.ListSession[0].m_trackId];
        }
        public float Get_trackLength()
        {
            switch (Simulation)
            {
                case 0:
                    return dataf1_2021.ListSession[0].m_trackLength;
                case 1:
                    return 0;
                case 2:
                    return ListTelemetryData[0].mLapDist;
                case 3:
                    return 0;
            }
            return 0;
        }

        // Manque RF2
        public string Get_m_networkGame()
        {
            string lm_networkGame;

            switch (Simulation)
            {
                case 0:
                    if (dataf1_2021.ListSession[0].m_networkGame == 1)
                        lm_networkGame = "online";
                    else
                        lm_networkGame = "offline";

                    return lm_networkGame;
                case 1:
                    return "Err";
                case 2:
                    return "Err RF2 Get_m_networkGame() Ligne 1000UDP Data()";
                case 3:
                    return "Err";
            }
            return "Err";

        }
        public float Get_m_trackTemperature()
        {
            switch (Simulation)
            {
                case 0:
                    return dataf1_2021.ListSession[0].m_trackTemperature;
                case 1:
                    return 0;
                case 2:
                    return ListTelemetryData[0].mTrackTemp;
                case 3:
                    return 0;
            }
            return dataf1_2021.ListSession[0].m_trackTemperature;
        }
        public sbyte Get_m_airTemperature()
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListSession[0].m_airTemperature;
                case 1: return 0;
                case 2: return (sbyte)dataRF2.scoring[0].mScoringInfo.mAmbientTemp;
                case 3: return 0;
            }
            return dataf1_2021.ListSession[0].m_airTemperature;
        }
        public string Get_m_sessionType()
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.sessiontypename[dataf1_2021.ListSession[0].m_sessionType];
                case 1: return "err";
                case 2: return dataRF2.GetSessionString(dataRF2.scoring[0].mScoringInfo.mSession);
                case 3: return "err";
            }
            return dataf1_2021.sessiontypename[dataf1_2021.ListSession[0].m_sessionType];
        }

        // Manque RF2
        public string Get_formulaname()
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.formulaname[dataf1_2021.ListSession[0].m_formula];
                case 1: return "err";
                case 2: return "RFactor 2 Get_formulaname() UDP_Data Ligne 1051";
                case 3: return "err";
            }
            return dataf1_2021.formulaname[dataf1_2021.ListSession[0].m_formula];
        }
        public int Get_m_totalLaps()
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListSession[0].m_totalLaps;
                case 1: return 0;
                case 2: return ListTelemetryData[0].mMaxLaps; 
                case 3: return 0;
            }
            return dataf1_2021.ListSession[0].m_totalLaps;
        }
        public string Get_meteo()
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.meteoname[dataf1_2021.ListSession[0].m_weather];
                case 1: return "err";
                case 2:
                    { //mCloudiness;                   // general cloudiness (0.0=clear to 1.0=dark)
                        if (dataRF2.weather[0].mWeatherInfo.mCloudiness == 0) return dataf1_2021.meteoname[0];
                        if (dataRF2.weather[0].mWeatherInfo.mCloudiness < 0.2) return dataf1_2021.meteoname[1];
                        if (dataRF2.weather[0].mWeatherInfo.mCloudiness < 0.4) return dataf1_2021.meteoname[2];
                        if (dataRF2.weather[0].mWeatherInfo.mCloudiness <0.6) return dataf1_2021.meteoname[3];
                        if (dataRF2.weather[0].mWeatherInfo.mCloudiness <0.8) return dataf1_2021.meteoname[4];
                        if (dataRF2.weather[0].mWeatherInfo.mCloudiness <0.9) return dataf1_2021.meteoname[5];
                        if (dataRF2.weather[0].mWeatherInfo.mCloudiness <1.1) return dataf1_2021.meteoname[6];
                        return dataf1_2021.meteoname[0];

                    }
                case 3: return "err";
            }
            return dataf1_2021.meteoname[dataf1_2021.ListSession[0].m_weather];
        }

        // Manque FR2
        public byte Get_m_aiDifficulty()
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListSession[0].m_aiDifficulty;
                case 1: return 0;
                case 2: return 0;
                case 3: return 0;
            }
            return 0;
        }

        // Manque F1
        public float Get_mElapsedTime(int index,int m_numCars)
        {
            switch (Simulation)
            {
                case 0: return 0;
                case 1: return 0;
                case 2: return ListTelemetryData[index].mVehicles[m_numCars].mElapsedTime;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_bestLapTime_Lap(int m_numCars, out int pnumLap)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.g_bestLapTimeLap(m_numCars, out pnumLap);
                case 1: pnumLap = -1; return -1;
                case 2: return dataRF2.g_bestLapTime_Lap(m_numCars, out pnumLap);
                case 3: pnumLap = -1; return -1;
            }
            return dataf1_2021.g_bestLapTimeLap(m_numCars, out pnumLap);
        }
        public float Get_m_bestLapTimeInMS(int Indextableau, int IndexPlayer)
        {
            //return ListTelemetryData[Indextableau].mVehicles[IndexPlayer].bestLapTime;
            int pnumLap; // non utilisé pour cette procédure

            switch (Simulation)
            {
                case 0: if (Indextableau>-1)
                            return (float)dataf1_2021.ListFinal[Indextableau].m_classificationData[IndexPlayer].m_bestLapTimeInMS;
                    return (float)dataf1_2021.ListFinal[dataf1_2021.ListFinal.Count-1].m_classificationData[IndexPlayer].m_bestLapTimeInMS;

                case 1: return 0;
                case 2: return dataRF2.g_bestLapTime_Lap(IndexPlayer, out pnumLap)*1000; // converstion en milliseconde
                case 3: return 0;
            }
            return 0;
        }
        public double Get_m_totalRaceTime(int IndexPlayer)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListFinal[0].m_classificationData[IndexPlayer].m_totalRaceTime*1000; // conversion en milliseconde
                case 1: return 0;
                case 2: return dataRF2.g_m_totalRaceTime(IndexPlayer) * 1000; // converstion en milliseconde
                case 3: return 0;
            }
            return 0;
        }
        public float Get_BestSector1TimeInMS(int m_numCars, out int pnumLap)
        {
            //return ListTelemetryData[Indextableau].mVehicles[m_numCars].mBestSector1;

            switch (Simulation)
            {
                case 0: return dataf1_2021.g_BestSector1TimeInMS(m_numCars, out pnumLap);
                case 1: pnumLap = -1; return -1;
                case 2: return dataRF2.g_BestSector1TimeInMS(m_numCars,out pnumLap) * 1000; // converstion en milliseconde
                case 3: pnumLap = -1; return -1;
            }
            pnumLap = -1; return -1;
        }
        public float Get_BestSector2TimeInMS(int m_numCars, out int pnumLap)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.g_BestSector2TimeInMS(m_numCars, out pnumLap); 
                case 1: pnumLap = -1; return -1;
                case 2: return dataRF2.g_BestSector2TimeInMS(m_numCars, out pnumLap) * 1000; // converstion en milliseconde
                case 3: pnumLap = -1; return -1;
            }
            pnumLap = -1; return -1;
        }
        public float Get_BestSector3TimeInMS(int m_numCars, out int pnumLap)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.g_BestSector3TimeInMS(m_numCars, out pnumLap); 
                case 1: pnumLap = -1;  return - 1;
                case 2: return dataRF2.g_BestSector3TimeInMS(m_numCars, out pnumLap) * 1000; // converstion en milliseconde
                case 3: pnumLap = -1; return -1;
            }
            pnumLap = -1; return -1;
        }


        public string GetvalidlapString(byte Flag)
        {
            switch (Simulation)
            {
                case 0:
                    if (Flag == 15)
                        return ("Valide");
                    return ("Non valide [") + Flag + "]";
                case 1: return ("Non valide [") + Flag + "]";
                case 2:
                    if (Flag == 0)
                        return ("Non valide [") + Flag + "]";
                    return ("Valide");
                    
            }
            return ("Erreur Validlap Ligne 1217");

        }
        public byte Get_LapValid(int lap, int NumCar)
        {
            switch (Simulation)
            {
                case 0:
                    int Indextableau = dataf1_2021.ListHistoric.Count - 1;
                    while (dataf1_2021.ListHistoric[Indextableau].m_carIdx != NumCar && Indextableau > 0)
                        Indextableau--;
                    if (Indextableau > -1)
                        return dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[lap].m_lapValidBitFlags; // 0x01 bit set-lap valid,      0x02 bit set-sector 1 valid
                                                                                                                // 0x04 bit set-sector 2 valid, 0x08 bit set-sector 3 valid
                    return 0;
                case 1: return 0;
                case 2: return dataRF2.scoring[lap].mVehicles[NumCar].mCountLapFlag;                            // 0 = do not count lap or time, 1 = count lap but not time, 2 = count lap and time
                case 3: return 0;
            }
            return 0;
        }

        public double Get_m_BestTheoLapTimeInMS(int NumCar)
        {
            int pnumLap;
            switch (Simulation)
            {
                case 0: return dataf1_2021.m_theoLapTimeInMS(NumCar);
                case 1: return -1;
                case 2: float TimeTheo =    dataRF2.g_BestSector1TimeInMS(NumCar, out pnumLap) + 
                                            dataRF2.g_BestSector2TimeInMS(NumCar, out pnumLap) +
                                            dataRF2.g_BestSector3TimeInMS(NumCar, out pnumLap);

                        return TimeTheo*1000;
                case 3: return -1;
            }

            return 0;
        }
        internal int Get_playerCarIndex(int v) 
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListLapData[v].packetHeader.m_playerCarIndex;  break;
                case 1: return 0; 
                case 2:
                    {
                        return ListTelemetryData[0].NbPlayer;
                        // who's in control: -1=nobody (shouldn't get this), 0=local player, 1=local AI, 2=remote, 3=replay (shouldn't get this)
                        for (int i = 0; i < (dataRF2.scoring[v].mVehicles.Count()-1); i++)
                        {
                            if (dataRF2.scoring[v].mVehicles[i].mControl==0)
                                return i;
                        }
                            
                        return -1;
                    }
                case 3: return 0; 
                case 4: return 0; 
                case 5: return 0; 
                case 6: return 0; 
            }
            return -1;
        }
        public int Get_m_currentLapNum(int Indextableau, int NumCar)
        { // return current lap number
          // possible erreur sur une analyse avec un -1 disparue dans FormTelemetry

            switch (Simulation)
            {
                case 0: return dataf1_2021.ListLapData[Indextableau].m_lapData[NumCar].m_currentLapNum;
                case 1: return 0;
                case 2:
                    if ((Indextableau < ListTelemetryData.Count) & (NumCar < ListTelemetryData[Indextableau].mVehicles.Length))
                        return ListTelemetryData[Indextableau].mVehicles[NumCar].mLapNumber;
                    else
                        return -1;
                case 3: return 0;
            }
            return -1;
        }
        public float Get_m_lapDistance(int Indextableau, int NumCar)
        {
            switch (Simulation)
            { 
                case 0: return dataf1_2021.ListLapData[Indextableau].m_lapData[NumCar].m_lapDistance;
                case 1: return 0;
                case 2:
                    if ((Indextableau < ListTelemetryData.Count) & (Indextableau>-1))
                        if (NumCar < ListTelemetryData[Indextableau].mVehicles.Length)
                        return ListTelemetryData[Indextableau].mVehicles[NumCar].mLapDist;
                    
                    return -1;
                case 3: return 0;
            }
            return -1;
        }
        public string Get_m_s_name(int IndexPlayer)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListPlayers[0].m_participants[IndexPlayer].m_s_name;
                   // ListDriver[IndexPlayer].mDriverName; //dataf1_2021.ListPlayers[0].m_participants[IndexPlayer].m_name;
                case 1: return "";
                case 2: return ListDriver[IndexPlayer].mDriverName;// ListTelemetryData[Indextableau].mVehicles[IndexPlayer].mDriverName;
                case 3: return "";
            }
            return "";

        }
        public string Get_m_s_name(int Indextableau, int IndexPlayer)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_s_name.ToString();
                case 1: return "erreur";
                case 2:
                    if ((Indextableau < ListTelemetryData.Count) & (IndexPlayer < ListTelemetryData[Indextableau].mVehicles.Length))
                        return ListTelemetryData[Indextableau].mVehicles[IndexPlayer].mDriverName;
                    else
                        return ("Err Get_m_s_name UDP_data Ligne 1290 "); 
                case 3: return "erreur";
            }
            return dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_s_name.ToString();
        }
        public int Get_m_numActiveCars(int Indextableau)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListPlayers[Indextableau].m_numActiveCars;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].NbVehicles;
                case 3: return 0;
            }
            return dataf1_2021.ListPlayers[Indextableau].m_numActiveCars;
        }
        public byte Get_m_carPosition(int Indextableau, int _NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListLapData[Indextableau-1].m_lapData[_NumCar].m_carPosition;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[_NumCar].mPlace;
                case 3: return 0;
            }
            return 0;
            
        }

        // Manque F1
        public uint Get_m_driverId(int IndexPlayer)
        {
            switch (Simulation)
            {
                case 0: return 0;
                case 1: return 0;
                case 2: return ListDriver[IndexPlayer].InternalmID;
                case 3: return 0;
            }
            return 0;

        }

        public uint Get_m_driverId(int Indextableau, int IndexPlayer)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_driverId;
                case 1: return 0;
                case 2:
                    return ListTelemetryData[Indextableau].mVehicles[IndexPlayer].InternalmID;
                case 3: return 0;
            }
            return dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_driverId;
        }
        public string Get_m_aiControlled(int Indextableau, int IndexPlayer)
        {
            //return ListTelemetryData[Indextableau].mVehicles[IndexPlayer].mControl;
            switch (Simulation)
            {
                case 0: 
                    {
                        if (dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_aiControlled == 0)
                            return ("Me");
                        if (dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_aiControlled == 1)
                            return ("AI");
                        return ("F1 ??");

                    }
                case 1: return "Simu 1";
                case 2:
                    { //0=local player, 1=local AI, 2=remote, 3=replay 
                        if (ListTelemetryData[Indextableau].mVehicles[IndexPlayer].mControl == 0)
                            return ("Me");
                        if (ListTelemetryData[Indextableau].mVehicles[IndexPlayer].mControl == 1)
                            return ("AI");
                        if (ListTelemetryData[Indextableau].mVehicles[IndexPlayer].mControl == 2)
                            return ("Player");
                        if (ListTelemetryData[Indextableau].mVehicles[IndexPlayer].mControl == 3)
                            return ("Replay");

                        return "??";
                    }
                case 3: return "Simu 3";
            }
            return "Err";
        }

        // Manque RF2
        public byte Get_m_networkId(int Indextableau, int IndexPlayer)
        {

            switch (Simulation)
            {
                case 0: return dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_networkId;
                case 1: return 0;
                case 2: return 0;
                case 3: return 0;
            }
            return dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_networkId;
        }

        // Manque RF2
        public byte Get_m_teamId(int Indextableau, int IndexPlayer)
        {
            switch (Simulation)
            {
                case 0: if (dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_teamId<200)
                         return dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_teamId;
                    return 0;
                case 1: return 0;
                case 2: return 0;
                case 3: return 0;
            }
            return dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_teamId;
        }

        // Manque RF2
        public byte Get_m_myTeam(int Indextableau, int IndexPlayer)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_myTeam;
                case 1: return 0;
                case 2: return 0;
                case 3: return 0;
            }
            return dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_myTeam;
        }

        // Manque F1
        public string Get_CarName(int IndexPlayer)
        {
            switch (Simulation)
            { 
                case 0: return "F1";
                case 2: return ListTelemetryData[0].mVehicles[IndexPlayer].mVehicleName; // Ne fonctionne pas sur les practices
            }
            return "ERR";
        }
        public byte Get_m_raceNumber(int Indextableau, int IndexPlayer)
        {
            
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_raceNumber;
                case 1: return 0;
                case 2:
                    string carName;
                    carName = Get_CarName(IndexPlayer);

                    if (carName[0] == '#')
                    {
                        char[] delims = { ' ' };
                        string[] parts = carName.Split(delims, 2);
                        string raceNumber = parts[0].Split('#')[1].Trim();
                        byte result;

                        if (byte.TryParse(raceNumber, out result))
                        {
                            return result;
                        }
                        else
                        {
                            return 0; // Valeur par défaut en cas d'échec de la conversion en byte.
                        }
                    }
                    else if (carName.Contains("#"))
                    {
                        string raceNumber = carName.Split('#')[1].Trim().Split(' ')[0].Trim();
                        byte result;

                        if (byte.TryParse(raceNumber, out result))
                        {
                            return result;
                        }
                        else
                        {
                            return 0; // Valeur par défaut en cas d'échec de la conversion en byte.
                        }
                    }
                    return 0;
                case 3:
                    return 0;
            }
            return 0;
        }

        // Manque RF2
        public byte Get_m_nationality(int Indextableau, int IndexPlayer)
        {
            switch (Simulation)
            {
                case 0: if (dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_nationality<255)
                            return dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_nationality;
                        return 0;
                case 1: return 0;
                case 2: return 0;
                case 3: return 0;
            }
            return dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_nationality;
        }

        // Manque RF2
        public byte Get_m_yourTelemetry(int Indextableau, int IndexPlayer)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_yourTelemetry;
                case 1: return 0;
                case 2: return 0;
                case 3: return 0;
            }
            return dataf1_2021.ListPlayers[Indextableau].m_participants[IndexPlayer].m_yourTelemetry;
        }
        public byte Get_m_position(int Indextableau, int IndexPlayer)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListFinal[Indextableau].m_classificationData[IndexPlayer].m_position;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[IndexPlayer].mPlace;
                case 3: return 0;
            }
            return 0;
        }

        // Manque RF2
        public int Get_m_numLaps(int NumCar)//(int Indextableau, int IndexPlayer)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListFinal[0].m_classificationData[NumCar].m_numLaps; // dataf1_2021.ListFinal[Indextableau].m_classificationData[IndexPlayer].m_numLaps;
                case 1: return 0;
                case 2: return ListTelemetryData[ListTelemetryData.Count-1].mVehicles[NumCar].mTotalLaps-1;
                case 3: return 0;
            }
            return 0;
        }
        public byte Get_m_gridPosition(int Indextableau, int IndexPlayer)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListFinal[Indextableau].m_classificationData[IndexPlayer].m_gridPosition;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[IndexPlayer].m_gridPosition;
                case 3: return 0;
            }
            return 0;
        }

        // Revoir RF2 car appel à dataRF2
        public byte Get_m_numPitStops(int Indextableau, int IndexPlayer)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListFinal[Indextableau].m_classificationData[IndexPlayer].m_numPitStops;
                case 1: return 0;
                case 2: return dataRF2.scoring[Indextableau].mVehicles[IndexPlayer].mPitState;
                case 3: return 0;
            }
            return dataf1_2021.ListFinal[Indextableau].m_classificationData[IndexPlayer].m_numPitStops;
        }
        public byte Get_m_resultStatus(int Indextableau, int IndexPlayer)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListFinal[Indextableau].m_classificationData[IndexPlayer].m_resultStatus;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[IndexPlayer].m_resultStatus;
                case 3: return 0;
            }
            return 0;
        }

        // Telemetry
        public float Get_m_speed(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListTelemetry[Indextableau].m_carTelemetryData[NumCar].m_speed;
                case 1: return 0;

                 //   var speed = Math.Sqrt((playerVeh.mLocalVel.x * playerVeh.mLocalVel.x)
                 //       + (playerVeh.mLocalVel.y * playerVeh.mLocalVel.y)
                 //       + (playerVeh.mLocalVel.z * playerVeh.mLocalVel.z));
                case 2:
                    if ((Indextableau < ListTelemetryData.Count) & (NumCar < ListTelemetryData[Indextableau].mVehicles.Length))
                        return ListTelemetryData[Indextableau].mVehicles[NumCar].m_speed;
                    else
                        return (-1);

                  /*  return (float)(Math.Sqrt((   dataRF2.telemetryLight[Indextableau].mVehicles[NumCar].mLocalVel.x *
                                                    dataRF2.telemetryLight[Indextableau].mVehicles[NumCar].mLocalVel.x)
                                                    + (dataRF2.telemetryLight[Indextableau].mVehicles[NumCar].mLocalVel.y*
                                                    dataRF2.telemetryLight[Indextableau].mVehicles[NumCar].mLocalVel.y)
                                                    + (dataRF2.telemetryLight[Indextableau].mVehicles[NumCar].mLocalVel.z*
                                                    dataRF2.telemetryLight[Indextableau].mVehicles[NumCar].mLocalVel.z))*3.6);
                   */             
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_engineRPM(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListTelemetry[Indextableau].m_carTelemetryData[NumCar].m_engineRPM;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_engineRPM;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_steer(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListTelemetry[Indextableau].m_carTelemetryData[NumCar].m_steer;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_steer;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_gear(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListTelemetry[Indextableau].m_carTelemetryData[NumCar].m_gear;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_gear;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_drs(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListTelemetry[Indextableau].m_carTelemetryData[NumCar].m_drs;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_drs;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_brakesTemperature(int Indextableau, int NumCar,byte wheel)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListTelemetry[Indextableau].m_carTelemetryData[NumCar].m_brakesTemperature[wheel];
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_brakesTemperature[wheel];
                               // telemetryData.mVehicles[i].m_brakesTemperature[j]
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_Wear(int Indextableau, int NumCar, byte wheel)
        {
            switch (Simulation)
            {
                case 0: return 0;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].mWear[wheel];
                case 3: return 0;
            }
            return 0;
        }
        public float Get_mTireLoad(int Indextableau, int NumCar, byte wheel)
        {
            switch (Simulation)
            {
                case 0: return 0;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].mTireLoad[wheel];
                case 3: return 0;
            }
            return 0;
        }

        public float Get_m_tyresSurfaceTemperature(int Indextableau, int NumCar, byte wheel)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListTelemetry[Indextableau].m_carTelemetryData[NumCar].m_tyresSurfaceTemperature[wheel];
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_tyresSurfaceTemperature[wheel];
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_tyresInnerTemperature(int Indextableau, int NumCar, byte wheel)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListTelemetry[Indextableau].m_carTelemetryData[NumCar].m_tyresInnerTemperature[wheel];
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_tyresInnerTemperature[wheel];
                case 3: return 0;
            }
            return 0;
        }
        public uint Get_m_engineTemperature(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListTelemetry[Indextableau].m_carTelemetryData[NumCar].m_engineTemperature;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_engineTemperature;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_tyresPressure(int Indextableau, int NumCar, byte wheel)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListTelemetry[Indextableau].m_carTelemetryData[NumCar].m_tyresPressure[wheel];
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_tyresPressure[wheel];
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_surfaceType(int Indextableau, int NumCar, byte wheel)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListTelemetry[Indextableau].m_carTelemetryData[NumCar].m_surfaceType[wheel];
                case 1: return 0;
                case 2: return (float)dataRF2.telemetryLight[Indextableau].mVehicles[NumCar].mWheels[wheel].mSurfaceType;
                case 3: return 0;
            }
            return dataf1_2021.ListTelemetry[Indextableau].m_carTelemetryData[NumCar].m_surfaceType[wheel];
        }

        // Manque RF2
        public float Get_m_revLightsPercent(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListTelemetry[Indextableau].m_carTelemetryData[NumCar].m_revLightsPercent;
                case 1: return 0;
                case 2: return 0;
                case 3: return 0;
            }
            return 0;
        }

        //Manque RF2
        public float Get_m_revLightsBitValue(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListTelemetry[Indextableau].m_carTelemetryData[NumCar].m_revLightsBitValue;
                case 1: return 0;
                case 2: return 0;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_brake(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListTelemetry[Indextableau].m_carTelemetryData[NumCar].m_brake;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_brake;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_throttle(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListTelemetry[Indextableau].m_carTelemetryData[NumCar].m_throttle;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_throttle;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_clutch(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListTelemetry[Indextableau].m_carTelemetryData[NumCar].m_clutch;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_clutch;
                case 3: return 0;
            }
            return 0;
        }
        public double Get_m_yaw(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_yaw * 180.0 / Math.PI;
                case 1: return 0;

                // var yaw = Math.Atan2(playerVeh.mOri[RowZ].x, playerVeh.mOri[RowZ].z);
                case 2:
                    return ListTelemetryData[Indextableau].mVehicles[NumCar].m_yaw;
                    /*
                    return (float)Math.Atan2(   dataRF2.telemetryLight[Indextableau].mVehicles[NumCar].mOri[RowZ].x,
                                                    dataRF2.telemetryLight[Indextableau].mVehicles[NumCar].mOri[RowZ].z);
                    */
                case 3: return 0;
            }
            return 0;
        }
        public double Get_m_pitch(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_pitch * 180.0 / Math.PI;
                case 1: return 0;

                    // var pitch = Math.Atan2(-playerVeh.mOri[RowY].z,
                    //Math.Sqrt(playerVeh.mOri[RowX].z * playerVeh.mOri[RowX].z + playerVeh.mOri[RowZ].z * playerVeh.mOri[RowZ].z));

                case 2:
                    return ListTelemetryData[Indextableau].mVehicles[NumCar].m_pitch;
                    /*
                    return (float)Math.Atan2(   -dataRF2.telemetryLight[Indextableau].mVehicles[NumCar].mOri[RowY].z,
                                                    Math.Sqrt(  dataRF2.telemetryLight[Indextableau].mVehicles[NumCar].mOri[RowX].z*
                                                                dataRF2.telemetryLight[Indextableau].mVehicles[NumCar].mOri[RowX].z+
                                                                dataRF2.telemetryLight[Indextableau].mVehicles[NumCar].mOri[RowZ].z *
                                                                dataRF2.telemetryLight[Indextableau].mVehicles[NumCar].mOri[RowZ].z
                                                            )
                                                    ) ;
                    */
                case 3: return 0;
            }
            return 0;
        }
        public double Get_m_roll(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_roll * 180.0 / Math.PI;
                case 1: return 0;

                    //  var roll = Math.Atan2(playerVeh.mOri[RowY].x,
                    //  Math.Sqrt(playerVeh.mOri[RowX].x * playerVeh.mOri[RowX].x + playerVeh.mOri[RowZ].x * playerVeh.mOri[RowZ].x));

                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_roll;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_gForceLateral(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_gForceLateral;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_gForceLateral;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_gForceLongitudinal(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_gForceLongitudinal;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_gForceLongitudinal;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_gForceVertical(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_gForceVertical;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_gForceVertical;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_worldPositionX(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_worldPositionX;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_worldPositionX;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_worldPositionY(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_worldPositionY;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_worldPositionY;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_worldPositionZ(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_worldPositionZ;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_worldPositionZ;
                case 3: return 0;
            }
            return  0;
        }
        public float Get_m_worldVelocityX(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_worldVelocityX;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_worldVelocityX;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_worldVelocityY(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_worldVelocityY;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_worldVelocityY;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_worldVelocityZ(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_worldVelocityZ;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_worldVelocityZ;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_worldForwardDirX(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_worldForwardDirX;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_worldForwardDirX;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_worldForwardDirY(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_worldForwardDirY;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_worldForwardDirY;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_worldForwardDirZ(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_worldForwardDirZ;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_worldForwardDirZ;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_worldRightDirX(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_worldRightDirX;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_worldRightDirX;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_worldRightDirZ(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_worldRightDirZ;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_worldRightDirZ;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_worldRightDirY(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListMotion_packet[Indextableau].m_carMotionData[NumCar].m_worldRightDirY;
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].m_worldRightDirY;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_sessionTime(int Indextableau, int NumCar)
        {
            switch (Simulation)
            {
                case 0:
                    {
                        /*
                        switch (type)
                        {
                            case 0: return dataf1_2021.ListLapData[Indextableau /-1/].packetHeader.m_sessionTime;
                            case 1: return dataf1_2021.ListEvent[Indextableau].packetHeader.m_sessionTime;
                            case 2: return dataf1_2021.ListTelemetry[Indextableau].packetHeader.m_sessionTime;
                            case 3: return dataf1_2021.ListMotion_packet[Indextableau].packetHeader.m_sessionTime;
                        }
                        */
                        return dataf1_2021.ListLapData[Indextableau /*-1*/].packetHeader.m_sessionTime;
                    }
                case 1: return 0;
                case 2: return ListTelemetryData[Indextableau].mVehicles[NumCar].mElapsedTime;
                case 3: return 0;
            }
            return 0;
        }

        // Manque RF2
        public float Get_m_carIdx(int Indextableau)
        { 
            // uniquement pour F1
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListHistoric[Indextableau].m_carIdx;
                case 1: return 0;
                case 2: return 0;
                case 3: return 0;
            }
            return 0;
        }
        public float Get_m_bestLapTimeLapNum(int NumCar)
        {
            int pnumLap;
            switch (Simulation)
            {
                case 0: dataf1_2021.g_bestLapTimeLap(NumCar, out pnumLap); //return dataf1_2021.ListHistoric[NumCar].m_bestLapTimeLapNum;
                    return pnumLap;
                case 1: return 0;
                case 2: dataRF2.g_bestLapTime_Lap(NumCar, out pnumLap);
                    return pnumLap;
                case 3: return 0;
            }
            return 0;
        }

        // Manque RF2
        public float Get_m_bestSector1LapNum(int NumCar)
        {
            switch (Simulation)
            {
                case 0:
                    int Indextableau = dataf1_2021.ListHistoric.Count - 1;
                    while (dataf1_2021.ListHistoric[Indextableau].m_carIdx != NumCar && Indextableau > 0)
                        Indextableau--;
                    if (Indextableau > -1)
                        return dataf1_2021.ListHistoric[Indextableau].m_bestSector1LapNum;
                    return 0;
                case 1: return 0;
                case 2: int Lap;
                        dataRF2.g_BestSector1TimeInMS(NumCar, out Lap);
                        return Lap;
                case 3: return 0;
            }
            return 0;
        }

        // Manque RF2
        public float Get_m_bestSector2LapNum(int NumCar)
        {
            switch (Simulation)
            {
                case 0:
                    int Indextableau = dataf1_2021.ListHistoric.Count - 1;
                    while (dataf1_2021.ListHistoric[Indextableau].m_carIdx != NumCar && Indextableau > 0)
                        Indextableau--;
                    if (Indextableau > -1)
                        return dataf1_2021.ListHistoric[Indextableau].m_bestSector2LapNum;
                    return 0;
                case 1: return 0;
                case 2:
                    int Lap;
                    dataRF2.g_BestSector2TimeInMS(NumCar, out Lap);
                    return Lap;
                case 3: return 0;
            }
            return 0;
        }

        // Manque RF2
        public float Get_m_bestSector3LapNum(int NumCar)
        {
            switch (Simulation)
            {
                case 0:
                    int Indextableau = dataf1_2021.ListHistoric.Count - 1;
                    while (dataf1_2021.ListHistoric[Indextableau].m_carIdx != NumCar && Indextableau > 0)
                        Indextableau--;
                    if (Indextableau > -1)
                        return dataf1_2021.ListHistoric[Indextableau].m_bestSector3LapNum;
                    return 0;
                case 1: return 0;
                case 2:
                    int Lap;
                    dataRF2.g_BestSector3TimeInMS(NumCar, out Lap);
                    return Lap;
                case 3: return 0;
            }
            return 0;
        }

        // Manque RF2
        public float Get_m_lapTimeInMS(int Lap, int NumCar)
        {
            switch (Simulation)
            {
                case 0:
                    int Indextableau = dataf1_2021.ListHistoric.Count - 1;
                    while (dataf1_2021.ListHistoric[Indextableau].m_carIdx != NumCar && Indextableau > 0)
                        Indextableau--; 
                    if (Indextableau > -1)
                        return dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[Lap].m_lapTimeInMS;
                    return 0;
                case 1: return 0;
                case 2: return (float)dataRF2.g_LapTimeInMS(NumCar, Lap);
                case 3: return 0;
            }
            return 0;
        }

        // manque RF2
        public float Get_m_sector1TimeInMS(int Lap, int NumCar)
        {
            switch (Simulation)
            {
                case 0:
                    int Indextableau = dataf1_2021.ListHistoric.Count - 1;
                    while (dataf1_2021.ListHistoric[Indextableau].m_carIdx != NumCar && Indextableau > 0)
                        Indextableau--;
                    if (Indextableau > -1) 
                            return dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[Lap].m_sector1TimeInMS;
                    return 0; // On retourne rien plutôt qu'une connerie
                case 1: return 0;

                    // revoir le codage RF2 car le besoin est d'avoir le temps du secteur 1 pour le tour Lap pour la voiture NumCar
                case 2: //return 0;// ListTelemetryData[Indextableau].mVehicles[NumCar].mLastSector1;//
                        return (float)dataRF2.g_Sector1TimeInMS(NumCar, Lap);
                case 3: return 0;
            }
            return 0;
        }

        // manque RF2
        public float Get_m_sector2TimeInMS(int Lap, int NumCar)
        {
            switch (Simulation)
            {
                case 0:
                    int Indextableau = dataf1_2021.ListHistoric.Count - 1;
                    while (dataf1_2021.ListHistoric[Indextableau].m_carIdx != NumCar && Indextableau > 0)
                        Indextableau--;
                    if (Indextableau > -1)
                            return dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[Lap].m_sector2TimeInMS;
                    return 0;
                case 1: return 0;
                case 2: return (float)dataRF2.g_Sector2TimeInMS(NumCar, Lap); // ListTelemetryData[IndexData].mVehicles[NumCar].mLastSector2;// dataRF2.g_sector2TimeInMS(NumCar, out pnumLap);
                case 3: return 0;
            }
            return 0;
        }

        // manque RF2
        public float Get_m_sector3TimeInMS(int Lap, int NumCar)
        {
            switch (Simulation)
            {
                case 0:
                    int Indextableau = dataf1_2021.ListHistoric.Count - 1;
                    while (dataf1_2021.ListHistoric[Indextableau].m_carIdx != NumCar && Indextableau > 0)
                        Indextableau--; if (Indextableau > -1)
                    if (Indextableau > -1)
                        return dataf1_2021.ListHistoric[Indextableau].m_lapHistoryData[Lap].m_sector3TimeInMS;
                    return 0;
                case 1: return 0;
                case 2: return (float)dataRF2.g_Sector3TimeInMS(NumCar, Lap); // ListTelemetryData[IndexData].mVehicles[NumCar].mLastSector3;// dataRF2.g_sector3TimeInMS(NumCar, out pnumLap);
                case 3: return 0;
            }
            return 0;
        }

        // Manque RF2
        public string Get_m_Code(int Indextableau)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListEvent[Indextableau].Code;
            }
            return "erreur";
        }

        // Manque RF2
        public string Get_m_GetCode(int Indextableau)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListEvent[Indextableau].GetCode();
            }
            return "erreur";
        }

        // Manque RF2
        public string Get_Eventdetails(int Indextableau)
        {
            switch (Simulation)
            {
                case 0: return dataf1_2021.ListEvent[Indextableau].Eventdetails;
            }
            return "erreur";
        }
    }
}
/*
                    case "F1 202x": dataSetting.Simulation = 0; break;
                    case "Project Cars 2": dataSetting.Simulation = 1; break;
                    case "RFactor 2": dataSetting.Simulation = 2; break;
                    case "Assetto Corsa Comp": dataSetting.Simulation = 3; break;
                    case "Assetto Corsa": dataSetting.Simulation = 4; break;
                    case "Forza": dataSetting.Simulation = 5; break;
                    case "Raceroom": dataSetting.Simulation = 6; break;

*/