using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;
using System.Runtime.InteropServices.ComTypes;

// https://github.com/raweceek-temeletry/f1-2021-udp#data-output-from-f1-2021

namespace f1_eTelemetry
{
    public class packet
    {
        internal Setting dataSetting = new Setting();
        public PacketHeader packetHeader = new PacketHeader();

        internal void tranfertHeader(PacketHeader packetheader)
        {
            packetHeader.m_packetFormat = packetheader.m_packetFormat;
            packetHeader.m_gameMajorVersion = packetheader.m_gameMajorVersion;
            packetHeader.m_gameMinorVersion = packetheader.m_gameMinorVersion;
            packetHeader.m_packetVersion = packetheader.m_packetVersion;
            packetHeader.m_packetId = packetheader.m_packetId;
            packetHeader.m_sessionUID = packetheader.m_sessionUID;
            packetHeader.m_sessionTime = packetheader.m_sessionTime;
            packetHeader.m_frameIdentifier = packetheader.m_frameIdentifier;
            packetHeader.m_playerCarIndex = packetheader.m_playerCarIndex;
            packetHeader.m_secondaryPlayerCarIndex = packetheader.m_secondaryPlayerCarIndex;
        }
    }

    public class Event_packet : packet
    {
        const int length = 36;

        public List<string> ListPenalty = new List<string>() {
                "Drive through",
                "Stop Go",
                "Grid penalty",
                "Penalty reminder",
                "Time penalty",
                "Warning",
                "Disqualified",
                "Removed from formation lap",
                "Parked too long timer",
                "Tyre regulations",
                "This lap invalidated",
                "This and next lap invalidated",
                "This lap invalidated without reason",
                "This and next lap invalidated without reason",
                "This and previous lap invalidated",
                "This and previous lap invalidated without reason",
                "Retired",
                "Black flag timer" };
        public List<string> ListInfringement = new List<string>() {
            "Blocking by slow driving",
            "Blocking by wrong way driving",
            "Reversing off the start line",
            "Big Collision",
            "Small Collision",
            "Collision failed to hand back position single",
            "Collision failed to hand back position multiple",
            "Corner cutting gained time",
            "Corner cutting overtake single",
            "Corner cutting overtake multiple",
            "Crossed pit exit lane",
            "Ignoring blue flags",
            "Ignoring yellow flags",
            "Ignoring drive through",
            "Too many drive throughs",
            "Drive through reminder serve within n laps",
            "Drive through reminder serve this lap",
            "Pit lane speeding",
            "Parked for too long",
            "Ignoring tyre regulations",
            "Too many penalties",
            "Multiple warnings",
            "Approaching disqualification",
            "Tyre regulations select single",
            "Tyre regulations select multiple",
            "Lap invalidated corner cutting",
            "Lap invalidated running wide",
            "Corner cutting ran wide gained time minor",
            "Corner cutting ran wide gained time significant",
            "Corner cutting ran wide gained time extreme",
            "Lap invalidated wall riding",
            "Lap invalidated flashback used",
            "Lap invalidated reset to track",
            "Blocking the pitlane",
            "Jump start",
            "Safety car to car collision",
            "Safety car illegal overtake",
            "Safety car exceeding allowed pace",
            "Virtual safety car exceeding allowed pace",
            "Formation lap below allowed speed",
            "Retired mechanical failure",
            "Retired terminally damaged",
            "Safety car falling too far back",
            "Black flag timer",
            "Unserved stop go penalty",
            "Unserved drive through penalty",
            "Engine component change",
            "Gearbox change",
            "League grid penalty",
            "Retry penalty",
            "Illegal time gain",
            "Mandatory pitstop" };

        public struct FastestLap
        {
            public byte vehicleIdx; // Vehicle index of car achieving fastest lap
            public float lapTime;    // Lap time is in seconds
        };
        public struct Retirement
        {
            public byte vehicleIdx; // Vehicle index of car retiring
        };
        public struct TeamMateInPits
        {
            public byte vehicleIdx; // Vehicle index of team mate
        };
        public struct RaceWinner
        {
            public byte vehicleIdx; // Vehicle index of the race winner
        };
        public struct Penalty
        {
            public byte penaltyType;      // Penalty type – see Appendices
            public byte infringementType;     // Infringement type – see Appendices
            public byte vehicleIdx;           // Vehicle index of the car the penalty is applied to
            public byte otherVehicleIdx;      // Vehicle index of the other car involved
            public byte time;                 // Time gained, or time spent doing action in seconds
            public byte lapNum;               // Lap the penalty occurred on
            public byte placesGained;       	// Number of places gained by this
        };
        public struct SpeedTrap
        {
            public byte vehicleIdx;       // Vehicle index of the vehicle triggering speed trap
            public float speed;            // Top speed achieved in kilometres per hour
            public byte overallFastestInSession;   // Overall fastest speed in session = 1, otherwise 0
            public byte driverFastestInSession;    // Fastest speed for driver in session = 1, otherwise 0
        };
        public struct StartLIghts
        {
            public byte numLights; // Number of lights showing
        };
        public struct DriveThroughPenaltyServed
        {
            public byte vehicleIdx; // Vehicle index of the vehicle serving drive through
        };
        public struct StopGoPenaltyServed
        {
            public byte vehicleIdx; // Vehicle index of the vehicle serving stop go
        };
        public struct Flashback
        {
            public UInt32 flashbackFrameIdentifier;  // Frame identifier flashed back to
            public float flashbackSessionTime;       // Session time flashed back to
        };
        public struct Buttons
        {
            public UInt32 m_buttonStatus;  // Bit flags specifying which buttons are being pressed
                                           // currently - see appendices
        };


        //PacketHeader m_header;                   // Header

        public byte[] m_eventStringCode = new byte[4];   	// Event string code, see below
        public string Code;
        public byte[] EventData =new byte[36]; 
        public string Eventdetails;

        //public EventDataDetails m_eventDetails;

        public Event_packet(PacketHeader packetheader, Stream stream, BinaryReader binaryReader)
        {
            init(stream, binaryReader);

            tranfertHeader(packetheader);

        }

        public void init(Stream stream, BinaryReader binaryReader)
        {
            if (stream.Length == length)
                PacketData(stream, binaryReader);
            else
                Console.WriteLine("erreur inf " + length);
        }

        // On verra plus tard
        public String GetCode()
        {
            string str;

            switch (Code)
            {
                case "SSTA": // Sent when the session starts
                    str = "Démarrage de la session";
                    break;

                case "SEND": // Sent when the session ends
                    str = "Fin de la session";
                    break;

                case "FTLP": // When a driver achieves the fastest lap
                    str = "Tour le plus rapide";
                    break;

                case "RTMT": // When a driver retires   
                    str = "Abandon pilote";
                    break;

                case "DRSE": // Race control have enabled DRS
                    str = "DRS autorisé";
                    break;

                case "DRSD": // Race control have disabled DRS
                    str = "DRS Interdit";
                    break;

                case "TMPT": // Your team mate has entered the pits
                    str = "Coéquipier dans les stands";
                    break;

                case "CHQF": //he chequered flag has been waved
                    str = "Drapeau à damier";
                    break;

                case "RCWN": // The race winner is announced
                    str = "Vainqueur de la course annoncé";
                    break;

                case "PENA": // A penalty has been issued – details in event
                    str = "Une pénalité a été émise";
                    break;

                case "SPTP": // Speed trap has been triggered by fastest speed
                    str = "Vitesse la plus rapide relevée";
                    break;

                case "STLG": // Start lights – number shown
                    str = "Feu de départ";
                    break;

                case "LGOT": // Lights out
                    str = "Feu éteint - départ de la course";
                    break;

                case "DTSV": // Drive through penalty served
                    str = "Drive through penalty émis";
                    break;

                case "SGSV": // Stop go penalty served
                    str = "Pénalité effectuée";
                    break;

                case "FLBK": // Flashback activated
                    str = "Flashback en cours";
                    break;

                case "BUTN": // Button status changed
                    str = "Button status changed";
                    break;
                default:
                    Console.WriteLine($"Code value is {Code}.");
                    str = $"Inconnue {Code}";
                    break;

            }
            return str;
        }

        public void PacketData(Stream stream, BinaryReader binaryReader)
        {
            
            for (int index = 0; index < 4; index++)
                m_eventStringCode[index] = binaryReader.ReadByte();

            Code = Encoding.Default.GetString(m_eventStringCode);

            for (int index = 0; stream.Position < 36; index++)
                EventData[index] = binaryReader.ReadByte();
            Eventdetails = Encoding.Default.GetString(EventData);
        }
    }

    public class CarDamage_packet : packet
    {
        const int length = 882;

        public struct CarDamageData
        {
            public float[] m_tyresWear; // [4];                     // Tyre wear (percentage)
            public byte[] m_tyresDamage; // [4];                   // Tyre damage (percentage)
            public byte[] m_brakesDamage; // [4];                  // Brakes damage (percentage)
            public byte m_frontLeftWingDamage;              // Front left wing damage (percentage)
            public byte m_frontRightWingDamage;             // Front right wing damage (percentage)
            public byte m_rearWingDamage;                   // Rear wing damage (percentage)
            public byte m_floorDamage;                      // Floor damage (percentage)
            public byte m_diffuserDamage;                   // Diffuser damage (percentage)
            public byte m_sidepodDamage;                    // Sidepod damage (percentage)
            public byte m_drsFault;                         // Indicator for DRS fault, 0 = OK, 1 = fault
            public byte m_gearBoxDamage;                    // Gear box damage (percentage)
            public byte m_engineDamage;                     // Engine damage (percentage)
            public byte m_engineMGUHWear;                   // Engine wear MGU-H (percentage)
            public byte m_engineESWear;                     // Engine wear ES (percentage)
            public byte m_engineCEWear;                     // Engine wear CE (percentage)
            public byte m_engineICEWear;                    // Engine wear ICE (percentage)
            public byte m_engineMGUKWear;                   // Engine wear MGU-K (percentage)
            public byte m_engineTCWear;                     // Engine wear TC (percentage)


        };



        //PacketHeader m_header;                   // Header



        public CarDamageData[] m_carDamageData = new CarDamageData[22];

        public CarDamage_packet(PacketHeader packetheader, Stream stream, BinaryReader binaryReader)
        {
            init(stream, binaryReader);

            tranfertHeader(packetheader);

        }
        public CarDamage_packet()
        {

        }
        public void init(Stream stream, BinaryReader binaryReader)
        {
            if (stream.Length == length)
                PacketData(stream, binaryReader);
            else
                Console.WriteLine("erreur inf " + length);
        }

        public void PacketData(Stream stream, BinaryReader binaryReader)
        {
            for (int index = 0; index < 22; index++)
            {
                m_carDamageData[index].m_tyresWear = new float[4];
                for (int j = 0; j < 4; j++)
                    m_carDamageData[index].m_tyresWear[j] = binaryReader.ReadSingle();

                m_carDamageData[index].m_tyresDamage = new byte[4];
                for (int j = 0; j < 4; j++)
                    m_carDamageData[index].m_tyresDamage[j] = binaryReader.ReadByte();

                m_carDamageData[index].m_brakesDamage = new byte[4];
                for (int j = 0; j < 4; j++)
                    m_carDamageData[index].m_brakesDamage[j] = binaryReader.ReadByte();

                m_carDamageData[index].m_frontLeftWingDamage = binaryReader.ReadByte();
                m_carDamageData[index].m_frontRightWingDamage = binaryReader.ReadByte();
                m_carDamageData[index].m_rearWingDamage = binaryReader.ReadByte();
                m_carDamageData[index].m_floorDamage = binaryReader.ReadByte();
                m_carDamageData[index].m_diffuserDamage = binaryReader.ReadByte();
                m_carDamageData[index].m_sidepodDamage = binaryReader.ReadByte();
                m_carDamageData[index].m_drsFault = binaryReader.ReadByte();
                m_carDamageData[index].m_gearBoxDamage = binaryReader.ReadByte();
                m_carDamageData[index].m_engineDamage = binaryReader.ReadByte();
                m_carDamageData[index].m_engineMGUHWear = binaryReader.ReadByte();
                m_carDamageData[index].m_engineESWear = binaryReader.ReadByte();
                m_carDamageData[index].m_engineCEWear = binaryReader.ReadByte();
                m_carDamageData[index].m_engineICEWear = binaryReader.ReadByte();
                m_carDamageData[index].m_engineMGUKWear = binaryReader.ReadByte();
                m_carDamageData[index].m_engineTCWear = binaryReader.ReadByte();
            }

        }
    }

    public class CarSetups_packet : packet
    {
        const int length = 1102;

        public struct CarSetupData
        {
            public byte m_frontWing;                // Front wing aero
            public byte m_rearWing;                 // Rear wing aero
            public byte m_onThrottle;               // Differential adjustment on throttle (percentage)
            public byte m_offThrottle;              // Differential adjustment off throttle (percentage)
            public float m_frontCamber;              // Front camber angle (suspension geometry)
            public float m_rearCamber;               // Rear camber angle (suspension geometry)
            public float m_frontToe;                 // Front toe angle (suspension geometry)
            public float m_rearToe;                  // Rear toe angle (suspension geometry)
            public byte m_frontSuspension;          // Front suspension
            public byte m_rearSuspension;           // Rear suspension
            public byte m_frontAntiRollBar;         // Front anti-roll bar
            public byte m_rearAntiRollBar;          // Front anti-roll bar
            public byte m_frontSuspensionHeight;    // Front ride height
            public byte m_rearSuspensionHeight;     // Rear ride height
            public byte m_brakePressure;            // Brake pressure (percentage)
            public byte m_brakeBias;                // Brake bias (percentage)
            public float m_rearLeftTyrePressure;     // Rear left tyre pressure (PSI)
            public float m_rearRightTyrePressure;    // Rear right tyre pressure (PSI)
            public float m_frontLeftTyrePressure;    // Front left tyre pressure (PSI)
            public float m_frontRightTyrePressure;   // Front right tyre pressure (PSI)
            public byte m_ballast;                  // Ballast
            public float m_fuelLoad;                 // Fuel load

        };



        //PacketHeader m_header;                   // Header



        public CarSetupData[] m_carSetups = new CarSetupData[22];

        public CarSetups_packet(PacketHeader packetheader, Stream stream, BinaryReader binaryReader)
        {
            init(stream, binaryReader);

            tranfertHeader(packetheader);

        }

        public void init(Stream stream, BinaryReader binaryReader)
        {
            if (stream.Length == length)
                PacketData(stream, binaryReader);
            else
                Console.WriteLine("erreur inf " + length);
        }

        public void PacketData(Stream stream, BinaryReader binaryReader)
        {
            for (int index = 0; index < 22; index++)
            {
                m_carSetups[index].m_frontWing = binaryReader.ReadByte();
                m_carSetups[index].m_rearWing = binaryReader.ReadByte();
                m_carSetups[index].m_onThrottle = binaryReader.ReadByte();
                m_carSetups[index].m_offThrottle = binaryReader.ReadByte();
                m_carSetups[index].m_frontCamber = binaryReader.ReadSingle();
                m_carSetups[index].m_rearCamber = binaryReader.ReadSingle();
                m_carSetups[index].m_frontToe = binaryReader.ReadSingle();
                m_carSetups[index].m_rearToe = binaryReader.ReadSingle();
                m_carSetups[index].m_frontSuspension = binaryReader.ReadByte();
                m_carSetups[index].m_rearSuspension = binaryReader.ReadByte();
                m_carSetups[index].m_frontAntiRollBar = binaryReader.ReadByte();
                m_carSetups[index].m_rearAntiRollBar = binaryReader.ReadByte();
                m_carSetups[index].m_frontSuspensionHeight = binaryReader.ReadByte();
                m_carSetups[index].m_rearSuspensionHeight = binaryReader.ReadByte();
                m_carSetups[index].m_brakePressure = binaryReader.ReadByte();
                m_carSetups[index].m_brakeBias = binaryReader.ReadByte();
                m_carSetups[index].m_rearLeftTyrePressure = binaryReader.ReadSingle();
                m_carSetups[index].m_rearRightTyrePressure = binaryReader.ReadSingle();
                m_carSetups[index].m_frontLeftTyrePressure = binaryReader.ReadSingle();
                m_carSetups[index].m_frontRightTyrePressure = binaryReader.ReadSingle();
                m_carSetups[index].m_ballast = binaryReader.ReadByte();
                m_carSetups[index].m_fuelLoad = binaryReader.ReadSingle();
            }

        }
    }

    public class CarStatus_packet : packet
    {
        const int length = 1058;

        public struct CarStatusData
        {
            public byte m_tractionControl;          // Traction control - 0 = off, 1 = medium, 2 = full
            public byte m_antiLockBrakes;           // 0 (off) - 1 (on)
            public byte m_fuelMix;                  // Fuel mix - 0 = lean, 1 = standard, 2 = rich, 3 = max
            public byte m_frontBrakeBias;           // Front brake bias (percentage)
            public byte m_pitLimiterStatus;         // Pit limiter status - 0 = off, 1 = on
            public float m_fuelInTank;               // Current fuel mass
            public float m_fuelCapacity;             // Fuel capacity
            public float m_fuelRemainingLaps;        // Fuel remaining in terms of laps (value on MFD)
            public UInt16 m_maxRPM;                   // Cars max RPM, point of rev limiter
            public UInt16 m_idleRPM;                  // Cars idle RPM
            public byte m_maxGears;                 // Maximum number of gears
            public byte m_drsAllowed;               // 0 = not allowed, 1 = allowed
            public UInt16 m_drsActivationDistance;    // 0 = DRS not available, non-zero - DRS will be available
                                               // in [X] metres
            public byte m_actualTyreCompound;    // F1 Modern - 16 = C5, 17 = C4, 18 = C3, 19 = C2, 20 = C1
                                                 // 7 = inter, 8 = wet
                                                 // F1 Classic - 9 = dry, 10 = wet
                                                 // F2 – 11 = super soft, 12 = soft, 13 = medium, 14 = hard
                                                 // 15 = wet
            public byte m_visualTyreCompound;       // F1 visual (can be different from actual compound)
                                                    // 16 = soft, 17 = medium, 18 = hard, 7 = inter, 8 = wet
                                                    // F1 Classic – same as above
                                                    // F2 ‘19, 15 = wet, 19 – super soft, 20 = soft
                                                    // 21 = medium , 22 = hard
            public byte m_tyresAgeLaps;             // Age in laps of the current set of tyres
            public sbyte m_vehicleFiaFlags;    // -1 = invalid/unknown, 0 = none, 1 = green
                                       // 2 = blue, 3 = yellow, 4 = red
            public float m_ersStoreEnergy;           // ERS energy store in Joules
            public byte m_ersDeployMode;            // ERS deployment mode, 0 = none, 1 = medium
                                              // 2 = hotlap, 3 = overtake
            public float m_ersHarvestedThisLapMGUK;  // ERS energy harvested this lap by MGU-K
            public float m_ersHarvestedThisLapMGUH;  // ERS energy harvested this lap by MGU-H
            public float m_ersDeployedThisLap;       // ERS energy deployed this lap
            public byte m_networkPaused;            // Whether the car is paused in a network game
        };



        //PacketHeader m_header;                   // Header



        public CarStatusData[] m_carStatusData = new CarStatusData[22];  

        public CarStatus_packet(PacketHeader packetheader, Stream stream, BinaryReader binaryReader)
        {
            init(stream, binaryReader);

            tranfertHeader(packetheader);

        }

        public void init(Stream stream, BinaryReader binaryReader)
        {
            if (stream.Length == length)
                PacketData(stream, binaryReader);
            else
                Console.WriteLine("erreur inf " + length);
        }

        public void PacketData(Stream stream, BinaryReader binaryReader)
        {
            for (int index = 0; index < 22; index++)
            {
                m_carStatusData[index].m_tractionControl = binaryReader.ReadByte();
                m_carStatusData[index].m_antiLockBrakes = binaryReader.ReadByte();
                m_carStatusData[index].m_fuelMix = binaryReader.ReadByte();
                m_carStatusData[index].m_frontBrakeBias = binaryReader.ReadByte();
                m_carStatusData[index].m_pitLimiterStatus = binaryReader.ReadByte();
                m_carStatusData[index].m_fuelInTank = binaryReader.ReadSingle();
                m_carStatusData[index].m_fuelCapacity = binaryReader.ReadSingle();
                m_carStatusData[index].m_fuelRemainingLaps = binaryReader.ReadSingle();
                m_carStatusData[index].m_maxRPM = binaryReader.ReadUInt16();
                m_carStatusData[index].m_idleRPM = binaryReader.ReadUInt16();
                m_carStatusData[index].m_maxGears = binaryReader.ReadByte();
                m_carStatusData[index].m_drsAllowed = binaryReader.ReadByte();
                m_carStatusData[index].m_drsActivationDistance = binaryReader.ReadUInt16();
                m_carStatusData[index].m_actualTyreCompound = binaryReader.ReadByte();
                m_carStatusData[index].m_visualTyreCompound = binaryReader.ReadByte();
                m_carStatusData[index].m_tyresAgeLaps = binaryReader.ReadByte();
                m_carStatusData[index].m_vehicleFiaFlags = binaryReader.ReadSByte();
                m_carStatusData[index].m_ersStoreEnergy = binaryReader.ReadSingle();
                m_carStatusData[index].m_ersDeployMode = binaryReader.ReadByte();
                m_carStatusData[index].m_ersHarvestedThisLapMGUK = binaryReader.ReadSingle();
                m_carStatusData[index].m_ersHarvestedThisLapMGUH = binaryReader.ReadSingle();
                m_carStatusData[index].m_ersDeployedThisLap = binaryReader.ReadSingle();
                m_carStatusData[index].m_networkPaused = binaryReader.ReadByte();
            }

        }
    }

    public class Historic_packet : packet
    {
        const int length = 1155;

        public struct LapHistoryData
        {
            public UInt32 m_lapTimeInMS;           // Lap time in milliseconds
            public UInt16 m_sector1TimeInMS;       // Sector 1 time in milliseconds
            public UInt16 m_sector2TimeInMS;       // Sector 2 time in milliseconds
            public UInt16 m_sector3TimeInMS;       // Sector 3 time in milliseconds
            public byte m_lapValidBitFlags;      // 0x01 bit set-lap valid,      0x02 bit set-sector 1 valid
                                           // 0x04 bit set-sector 2 valid, 0x08 bit set-sector 3 valid
        };

        public struct TyreStintHistoryData
        {
            public byte m_endLap;                // Lap the tyre usage ends on (255 of current tyre)
            public byte m_tyreActualCompound;    // Actual tyres used by this driver
            public byte m_tyreVisualCompound;    // Visual tyres used by this driver
        };


        //PacketHeader m_header;                   // Header

        public byte m_carIdx;                   // Index of the car this lap data relates to
        public byte m_numLaps;                  // Num laps in the data (including current partial lap)
        public byte m_numTyreStints;            // Number of tyre stints in the data

        public byte m_bestLapTimeLapNum;        // Lap the best lap time was achieved on
        public byte m_bestSector1LapNum;        // Lap the best Sector 1 time was achieved on
        public byte m_bestSector2LapNum;        // Lap the best Sector 2 time was achieved on
        public byte m_bestSector3LapNum;        // Lap the best Sector 3 time was achieved on

        public LapHistoryData[] m_lapHistoryData = new LapHistoryData[100];   // 100 laps of data max
        public TyreStintHistoryData[] m_tyreStintsHistoryData = new TyreStintHistoryData[8];

        public Historic_packet(PacketHeader packetheader, Stream stream, BinaryReader binaryReader)
        {
            init(stream, binaryReader);

            tranfertHeader(packetheader);

        }
        public Historic_packet()
        {
        }

        public void init(Stream stream, BinaryReader binaryReader)
        {
            if (stream.Length == length)
                PacketData(stream, binaryReader);
            else
                Console.WriteLine("erreur inf " + length);
        }

        public void PacketData(Stream stream, BinaryReader binaryReader)
        {
            m_carIdx = binaryReader.ReadByte();
            m_numLaps = binaryReader.ReadByte();
            m_numTyreStints = binaryReader.ReadByte();
            m_bestLapTimeLapNum = binaryReader.ReadByte();
            m_bestSector1LapNum = binaryReader.ReadByte();
            m_bestSector2LapNum = binaryReader.ReadByte();
            m_bestSector3LapNum = binaryReader.ReadByte();

            for (int index=0; index<100; index++)
            {
                m_lapHistoryData[index].m_lapTimeInMS = binaryReader.ReadUInt32();
                m_lapHistoryData[index].m_sector1TimeInMS = binaryReader.ReadUInt16();
                m_lapHistoryData[index].m_sector2TimeInMS = binaryReader.ReadUInt16();
                m_lapHistoryData[index].m_sector3TimeInMS = binaryReader.ReadUInt16();
                m_lapHistoryData[index].m_lapValidBitFlags = binaryReader.ReadByte();
            }
            for (int index = 0; index < 8; index++)
            {
                m_tyreStintsHistoryData[index].m_endLap = binaryReader.ReadByte();
                m_tyreStintsHistoryData[index].m_tyreActualCompound = binaryReader.ReadByte();
                m_tyreStintsHistoryData[index].m_tyreVisualCompound = binaryReader.ReadByte();
            }

        }
    }

    public class Players_packet : packet
    {
        const int length = 1257;

        public struct ParticipantData
        {
            public byte m_aiControlled;           // Whether the vehicle is AI (1) or Human (0) controlled
            public byte m_driverId;       // Driver id - see appendix, 255 if network human
            public byte m_networkId;      // Network id – unique identifier for network players
            public byte m_teamId;                 // Team id - see appendix
            public byte m_myTeam;                 // My team flag – 1 = My Team, 0 = otherwise
            public byte m_raceNumber;             // Race number of the car
            public byte m_nationality;            // Nationality of the driver
            public byte[] m_name; // [48];               // Name of participant in UTF-8 format – null terminated
                                  // Will be truncated with … (U+2026) if too long
            public string m_s_name;  // m_s_name = m_name si nécessaire
            public byte m_yourTelemetry;          // The player's UDP setting, 0 = restricted, 1 = public
        };


        public byte m_numActiveCars;    // Number of active cars in the data – should match number of
                                        // cars on HUD
        public ParticipantData[] m_participants = new ParticipantData[22];

        public Players_packet()
        {
        }

        public Players_packet(PacketHeader packetheader, Stream stream, BinaryReader binaryReader)
        {
            init(stream, binaryReader);

            tranfertHeader(packetheader);

        }

        public void init(Stream stream, BinaryReader binaryReader)
        {
            if (stream.Length == length)
                PacketData(stream, binaryReader);
            else
                Console.WriteLine("erreur inf " + length);
        }

        public void PacketData(Stream stream, BinaryReader binaryReader)
        {
            m_numActiveCars = binaryReader.ReadByte();

            for (int i = 0; i < 22; i++)
            {
                m_participants[i].m_aiControlled = binaryReader.ReadByte();
                m_participants[i].m_driverId = binaryReader.ReadByte();
                m_participants[i].m_networkId = binaryReader.ReadByte();
                m_participants[i].m_teamId = binaryReader.ReadByte();
                m_participants[i].m_myTeam = binaryReader.ReadByte();
                m_participants[i].m_raceNumber = binaryReader.ReadByte();
                m_participants[i].m_nationality = binaryReader.ReadByte();

                m_participants[i].m_name = new byte[48];
                for (int j = 0; j < 48; j++)
                    m_participants[i].m_name[j] = binaryReader.ReadByte();

                //m_participants[i].m_s_name = Encoding.Default.GetString(m_participants[i].m_name);
                m_participants[i].m_s_name = Encoding.UTF8.GetString(m_participants[i].m_name);

                m_participants[i].m_yourTelemetry = binaryReader.ReadByte();
            }
        }

    }

    // Lobby Info	9	Information about players in a multiplayer lobby
    public class Lobby_packet : packet
    {
        const int length = 1191;

        public struct LobbyInfoData
        {
            public byte m_aiControlled;            // Whether the vehicle is AI (1) or Human (0) controlled
            public byte m_teamId;                  // Team id - see appendix (255 if no team currently selected)
            public byte m_nationality;             // Nationality of the driver
            public byte[] m_name; //[48];         // Name of participant in UTF-8 format – null terminated
                                  // Will be truncated with ... (U+2026) if too long
            public string m_s_name;  // m_s_name = m_name si nécessaire
            public byte m_carNumber;               // Car number of the player
            public byte m_readyStatus;             // 0 = not ready, 1 = ready, 2 = spectating
        };

        public byte m_numPlayers;               // Number of players in the lobby data
        public LobbyInfoData[] m_lobbyPlayers = new LobbyInfoData[22];

        public Lobby_packet()
        {
        }

        public Lobby_packet(PacketHeader packetheader, Stream stream, BinaryReader binaryReader)
        {
            init(stream, binaryReader);

            tranfertHeader(packetheader);

        }

        public void init(Stream stream, BinaryReader binaryReader)
        {
            if (stream.Length == length)
                PacketData(stream, binaryReader);
            else
                Console.WriteLine("erreur inf " + length);
        }

        public void PacketData(Stream stream, BinaryReader binaryReader)
        {
            m_numPlayers = binaryReader.ReadByte();

            for (int i = 0; i < 22; i++)
            {
                m_lobbyPlayers[i].m_aiControlled = binaryReader.ReadByte();
                m_lobbyPlayers[i].m_teamId = binaryReader.ReadByte();
                m_lobbyPlayers[i].m_nationality = binaryReader.ReadByte();

                m_lobbyPlayers[i].m_name = new byte[48];
                for (int j = 0; j < 48; j++)
                    m_lobbyPlayers[i].m_name[j] = binaryReader.ReadByte();

                //m_lobbyPlayers[i].m_s_name = Encoding.Default.GetString(m_lobbyPlayers[i].m_name);
                m_lobbyPlayers[i].m_s_name = Encoding.UTF8.GetString(m_lobbyPlayers[i].m_name);

                m_lobbyPlayers[i].m_carNumber = binaryReader.ReadByte();
                m_lobbyPlayers[i].m_readyStatus = binaryReader.ReadByte();

 


            }
        }

    }

    public class Final_packet : packet
    {
        const int length = 839;

        public struct FinalClassificationData
        {
            public byte m_position;              // Finishing position
            public byte m_numLaps;               // Number of laps completed
            public byte m_gridPosition;          // Grid position of the car
            public byte m_points;                // Number of points scored
            public byte m_numPitStops;           // Number of pit stops made
            public byte m_resultStatus;          // Result status - 0 = invalid, 1 = inactive, 2 = active
                                                 // 3 = finished, 4 = didnotfinish, 5 = disqualified
                                                 // 6 = not classified, 7 = retired
            public UInt32 m_bestLapTimeInMS;       // Best lap time of the session in milliseconds
            public double m_totalRaceTime;         // Total race time in seconds without penalties
            public byte m_penaltiesTime;         // Total penalties accumulated in seconds
            public byte m_numPenalties;          // Number of penalties applied to this driver
            public byte m_numTyreStints;         // Number of tyres stints up to maximum
            public byte[] m_tyreStintsActual; // [8];   // Actual tyres used by this driver
            public byte[] m_tyreStintsVisual; // [8];   // Visual tyres used by this driver
        };

        public byte m_numCars;          // Number of cars in the final classification

        public FinalClassificationData[] m_classificationData = new FinalClassificationData[22];

        public Final_packet()
        {
        }

        public Final_packet(PacketHeader packetheader, Stream stream, BinaryReader binaryReader)
        {
            init(stream, binaryReader);

            tranfertHeader(packetheader);

        }

        public void init(Stream stream, BinaryReader binaryReader)
        {
            if (stream.Length == length)
                PacketData(stream, binaryReader);
            else
                Console.WriteLine("erreur inf " + length);
        }

        public void PacketData(Stream stream, BinaryReader binaryReader)
        {
            m_numCars = binaryReader.ReadByte();

            for (int i = 0; i < 22; i++)
            {
                m_classificationData[i].m_position = binaryReader.ReadByte();
                m_classificationData[i].m_numLaps = binaryReader.ReadByte();
                m_classificationData[i].m_gridPosition = binaryReader.ReadByte();
                m_classificationData[i].m_points = binaryReader.ReadByte();
                m_classificationData[i].m_numPitStops = binaryReader.ReadByte();
                m_classificationData[i].m_resultStatus = binaryReader.ReadByte();

                m_classificationData[i].m_bestLapTimeInMS = binaryReader.ReadUInt32();
                m_classificationData[i].m_totalRaceTime = binaryReader.ReadDouble();

                m_classificationData[i].m_penaltiesTime = binaryReader.ReadByte();
                m_classificationData[i].m_numPenalties = binaryReader.ReadByte();
                m_classificationData[i].m_numTyreStints = binaryReader.ReadByte();

                m_classificationData[i].m_tyreStintsActual = new byte[8];
                for (int j = 0; j < 8; j++)
                    m_classificationData[i].m_tyreStintsActual[j] = binaryReader.ReadByte();

                m_classificationData[i].m_tyreStintsVisual = new byte[8];
                for (int j = 0; j < 8; j++)
                    m_classificationData[i].m_tyreStintsVisual[j] = binaryReader.ReadByte();
            }
        }

    }

    public class Telemetry_packet : packet, IEquatable<Telemetry_packet>
    {
        const int length = 1347;

        public struct CarTelemetryData
        {
            public UInt16 m_speed;                    // Speed of car in kilometres per hour
            public float m_throttle;                 // Amount of throttle applied (0.0 to 1.0)
            public float m_steer;                    // Steering (-1.0 (full lock left) to 1.0 (full lock right))
            public float m_brake;                    // Amount of brake applied (0.0 to 1.0)
            public byte m_clutch;                   // Amount of clutch applied (0 to 100)
            public sbyte m_gear;                     // Gear selected (1-8, N=0, R=-1)
            public UInt16 m_engineRPM;                // Engine RPM
            public byte m_drs;                      // 0 = off, 1 = on
            public byte m_revLightsPercent;         // Rev lights indicator (percentage)
            public UInt16 m_revLightsBitValue;        // Rev lights (bit 0 = leftmost LED, bit 14 = rightmost LED)
            public UInt16[] m_brakesTemperature; // [4];     // Brakes temperature (celsius)
            public byte[] m_tyresSurfaceTemperature;// [4]; // Tyres surface temperature (celsius)
            public byte[] m_tyresInnerTemperature;// [4]; // Tyres inner temperature (celsius)
            public UInt16 m_engineTemperature;        // Engine temperature (celsius)
            public float[] m_tyresPressure;// [4];         // Tyres pressure (PSI)
            public byte[] m_surfaceType;// [4];           // Driving surface, see appendices
        };

        [Measurement("CarTelemetryData")]
        private class DBCarTelemetryData
        {
            [Column(IsTimestamp = true)] public DateTime Time { get; set; }
            [Column("m_carTelemetryData", IsTag = true)] public int? m_carTelemetryData { get; set; }
            [Column("m_speed")] public double? m_speed { get; set; }
            [Column("m_throttle")] public double? m_throttle { get; set; }
            [Column("m_steer")] public double? m_steer { get; set; }
            [Column("m_brake")] public double? m_brake { get; set; }
            [Column("m_clutch")] public double? m_clutch { get; set; }
            [Column("m_gear")] public double? m_gear { get; set; }
            [Column("m_engineRPM")] public double? m_engineRPM { get; set; }
            [Column("m_brakesTemperature")] public UInt16? m_brakesTemperature { get; set; }
        }

        //            PacketHeader m_header;        // Header

        public CarTelemetryData[] m_carTelemetryData = new CarTelemetryData[22];

        public byte m_mfdPanelIndex;       // Index of MFD panel open - 255 = MFD closed
                                           // Single player, race – 0 = Car setup, 1 = Pits
                                           // 2 = Damage, 3 =  Engine, 4 = Temperatures
                                           // May vary depending on game mode
        public byte m_mfdPanelIndexSecondaryPlayer;   // See above
        public sbyte m_suggestedGear;       // Suggested gear for the player (1-8)
                                            // 0 if no gear suggested

        public Telemetry_packet()
        {
        }

        public Telemetry_packet(PacketHeader packetheader, Stream stream, BinaryReader binaryReader)
        {
            init(stream, binaryReader);

            tranfertHeader(packetheader);

        }

        public void init(Stream stream, BinaryReader binaryReader)
        {
            if (stream.Length == length)
                PacketData(stream, binaryReader);
            else
                Console.WriteLine("erreur inf " + length);
        }

        public void PacketData(Stream stream, BinaryReader binaryReader)
        {
            for (int i = 0; i < 22; i++)
            {
                m_carTelemetryData[i].m_speed = binaryReader.ReadUInt16();
                m_carTelemetryData[i].m_throttle = binaryReader.ReadSingle();
                m_carTelemetryData[i].m_steer = binaryReader.ReadSingle();
                m_carTelemetryData[i].m_brake = binaryReader.ReadSingle();
                m_carTelemetryData[i].m_clutch = binaryReader.ReadByte();
                m_carTelemetryData[i].m_gear = binaryReader.ReadSByte();
                m_carTelemetryData[i].m_engineRPM = binaryReader.ReadUInt16();
                m_carTelemetryData[i].m_drs = binaryReader.ReadByte();
                m_carTelemetryData[i].m_revLightsPercent = binaryReader.ReadByte();
                m_carTelemetryData[i].m_revLightsBitValue = binaryReader.ReadUInt16();

                m_carTelemetryData[i].m_brakesTemperature = new UInt16[4];
                for (int j = 0; j < 4; j++)
                    m_carTelemetryData[i].m_brakesTemperature[j] = binaryReader.ReadUInt16();

                m_carTelemetryData[i].m_tyresSurfaceTemperature = new byte[4];
                for (int j = 0; j < 4; j++)
                    m_carTelemetryData[i].m_tyresSurfaceTemperature[j] = binaryReader.ReadByte();

                m_carTelemetryData[i].m_tyresInnerTemperature = new byte[4];
                for (int j = 0; j < 4; j++)
                    m_carTelemetryData[i].m_tyresInnerTemperature[j] = binaryReader.ReadByte();

                m_carTelemetryData[i].m_engineTemperature = binaryReader.ReadUInt16();

                m_carTelemetryData[i].m_tyresPressure = new float[4];
                for (int j = 0; j < 4; j++)
                    m_carTelemetryData[i].m_tyresPressure[j] = binaryReader.ReadSingle();

                m_carTelemetryData[i].m_surfaceType = new byte[4];
                for (int j = 0; j < 4; j++)
                    m_carTelemetryData[i].m_surfaceType[j] = binaryReader.ReadByte();
            }
        }

        public bool Equals(Telemetry_packet other)
        {
            if (other == null) return false;
            return (this.packetHeader.m_sessionTime.Equals(other.packetHeader.m_sessionTime));
        }

        public void writetemetrie()
        {
            int i;

            for (i = 0; i < 22; i++)
            {
                var mem = new DBCarTelemetryData
                {
                    Time = DateTime.UtcNow,
                    m_carTelemetryData = i,
                    m_speed = m_carTelemetryData[i].m_speed,
                    m_throttle = m_carTelemetryData[i].m_throttle,
                    m_steer = m_carTelemetryData[i].m_steer,
                    m_brake = m_carTelemetryData[i].m_brake,
                    m_clutch = m_carTelemetryData[i].m_clutch,
                    m_gear = m_carTelemetryData[i].m_gear,
                    m_engineRPM = m_carTelemetryData[i].m_engineRPM,
                    m_brakesTemperature = m_carTelemetryData[i].m_engineTemperature
                };

                using var client = InfluxDBClientFactory.Create(dataSetting.InfluxDBAdress, dataSetting.token);

                using (var writeApi = client.GetWriteApi())
                {
                    writeApi.WriteMeasurement(mem, WritePrecision.Ns, dataSetting.bucket, dataSetting.org);
                    //writeApi.WriteMeasurement(bucket, org, WritePrecision.Ns, mem);
                }
            }
        }
    }

    public class Session_packet : packet
    {
        const int length = 625;

        public struct MarshalZone
        {
            public float m_zoneStart;   // Fraction (0..1) of way through the lap the marshal zone starts
            public byte m_zoneFlag;    // -1 = invalid/unknown, 0 = none, 1 = green, 2 = blue, 3 = yellow, 4 = red
        };

        public struct WeatherForecastSample
        {
            public byte m_sessionType;              // 0 = unknown, 1 = P1, 2 = P2, 3 = P3, 4 = Short P, 5 = Q1
                                                    // 6 = Q2, 7 = Q3, 8 = Short Q, 9 = OSQ, 10 = R, 11 = R2
                                                    // 12 = Time Trial
            public byte m_timeOffset;               // Time in minutes the forecast is for
            public byte m_weather;                  // Weather - 0 = clear, 1 = light cloud, 2 = overcast
                                                    // 3 = light rain, 4 = heavy rain, 5 = storm
            public sbyte m_trackTemperature;         // Track temp. in degrees Celsius
            public sbyte m_trackTemperatureChange;   // Track temp. change – 0 = up, 1 = down, 2 = no change
            public sbyte m_airTemperature;           // Air temp. in degrees celsius
            public sbyte m_airTemperatureChange;     // Air temp. change – 0 = up, 1 = down, 2 = no change
            public byte m_rainPercentage;           // Rain percentage (0-100)
        };


        //PacketHeader m_header;                  // Header

        public byte m_weather;                // Weather - 0 = clear, 1 = light cloud, 2 = overcast
                                              // 3 = light rain, 4 = heavy rain, 5 = storm
        public sbyte m_trackTemperature;        // Track temp. in degrees celsius
        public sbyte m_airTemperature;          // Air temp. in degrees celsius
        public byte m_totalLaps;              // Total number of laps in this race
        public UInt16 m_trackLength;               // Track length in metres
        public byte m_sessionType;            // 0 = unknown, 1 = P1, 2 = P2, 3 = P3, 4 = Short P
                                              // 5 = Q1, 6 = Q2, 7 = Q3, 8 = Short Q, 9 = OSQ
                                              // 10 = R, 11 = R2, 12 = R3, 13 = Time Trial
        public sbyte m_trackId;                 // -1 for unknown, 0-21 for tracks, see appendix
        public byte m_formula;                    // Formula, 0 = F1 Modern, 1 = F1 Classic, 2 = F2,
                                                  // 3 = F1 Generic
        public UInt16 m_sessionTimeLeft;       // Time left in session in seconds
        public UInt16 m_sessionDuration;       // Session duration in seconds
        public byte m_pitSpeedLimit;          // Pit speed limit in kilometres per hour
        public byte m_gamePaused;                // Whether the game is paused
        public byte m_isSpectating;           // Whether the player is spectating
        public byte m_spectatorCarIndex;      // Index of the car being spectated
        public byte m_sliProNativeSupport;    // SLI Pro support, 0 = inactive, 1 = active
        public byte m_numMarshalZones;            // Number of marshal zones to follow
        public MarshalZone[] m_marshalZones = new MarshalZone[21];             // List of marshal zones – max 21

        public byte m_safetyCarStatus;           // 0 = no safety car, 1 = full
                                                 // 2 = virtual, 3 = formation lap
        public byte m_networkGame;               // 0 = offline, 1 = online
        public byte m_numWeatherForecastSamples; // Number of weather samples to follow
        public WeatherForecastSample[] m_weatherForecastSamples = new WeatherForecastSample[56];   // Array of weather forecast samples
        public byte m_forecastAccuracy;          // 0 = Perfect, 1 = Approximate
        public byte m_aiDifficulty;              // AI Difficulty rating – 0-110
        public UInt32 m_seasonLinkIdentifier;      // Identifier for season - persists across saves
        public UInt32 m_weekendLinkIdentifier;     // Identifier for weekend - persists across saves
        public UInt32 m_sessionLinkIdentifier;     // Identifier for session - persists across saves
        public byte m_pitStopWindowIdealLap;     // Ideal lap to pit on for current strategy (player)
        public byte m_pitStopWindowLatestLap;    // Latest lap to pit on for current strategy (player)
        public byte m_pitStopRejoinPosition;     // Predicted position to rejoin at (player)
        public byte m_steeringAssist;            // 0 = off, 1 = on
        public byte m_brakingAssist;             // 0 = off, 1 = low, 2 = medium, 3 = high
        public byte m_gearboxAssist;             // 1 = manual, 2 = manual & suggested gear, 3 = auto
        public byte m_pitAssist;                 // 0 = off, 1 = on
        public byte m_pitReleaseAssist;          // 0 = off, 1 = on
        public byte m_ERSAssist;                 // 0 = off, 1 = on
        public byte m_DRSAssist;                 // 0 = off, 1 = on
        public byte m_dynamicRacingLine;         // 0 = off, 1 = corners only, 2 = full
        public byte m_dynamicRacingLineType;     // 0 = 2D, 1 = 3D

        public Session_packet()
        {
        }

        public Session_packet(PacketHeader packetheader, Stream stream, BinaryReader binaryReader)
        {
            init(stream, binaryReader);
            tranfertHeader(packetheader);

        }
        public void init(Stream stream, BinaryReader binaryReader)
        {
            if (stream.Length == length)
                PacketData(stream, binaryReader);
            else
                Console.WriteLine("erreur inf " + length);
        }

        public void PacketData(Stream stream, BinaryReader binaryReader)
        {
            m_weather = binaryReader.ReadByte();                   // Weather - 0 = clear, 1 = light cloud, 2 = overcast
                                                                   // 3 = light rain, 4 = heavy rain, 5 = storm
            m_trackTemperature = binaryReader.ReadSByte();         // Track temp. in degrees celsius
            m_airTemperature = binaryReader.ReadSByte();           // Air temp. in degrees celsius
            m_totalLaps = binaryReader.ReadByte();                 // Total number of laps in this race
            m_trackLength = binaryReader.ReadUInt16();             // Track length in metres
            m_sessionType = binaryReader.ReadByte();               // 0 = unknown, 1 = P1, 2 = P2, 3 = P3, 4 = Short P
                                                                   // 5 = Q1, 6 = Q2, 7 = Q3, 8 = Short Q, 9 = OSQ
                                                                   // 10 = R, 11 = R2, 12 = R3, 13 = Time Trial
            m_trackId = binaryReader.ReadSByte();                  // -1 for unknown, 0-21 for tracks, see appendix
            m_formula = binaryReader.ReadByte();                   // Formula, 0 = F1 Modern, 1 = F1 Classic, 2 = F2,
                                                                   // 3 = F1 Generic
            m_sessionTimeLeft = binaryReader.ReadUInt16();         // Time left in session in seconds
            m_sessionDuration = binaryReader.ReadUInt16();         // Session duration in seconds
            m_pitSpeedLimit = binaryReader.ReadByte();             // Pit speed limit in kilometres per hour
            m_gamePaused = binaryReader.ReadByte();                // Whether the game is paused
            m_isSpectating = binaryReader.ReadByte();              // Whether the player is spectating
            m_spectatorCarIndex = binaryReader.ReadByte();         // Index of the car being spectated
            m_sliProNativeSupport = binaryReader.ReadByte();       // SLI Pro support, 0 = inactive, 1 = active
            m_numMarshalZones = binaryReader.ReadByte();           // Number of marshal zones to follow
            for (int i = 0; i < 21; i++)
            {
                m_marshalZones[i].m_zoneStart = binaryReader.ReadSingle();
                m_marshalZones[i].m_zoneFlag = binaryReader.ReadByte();
            }
            m_safetyCarStatus = binaryReader.ReadByte();           // 0 = no safety car, 1 = full
                                                                   // 2 = virtual, 3 = formation lap
            m_networkGame = binaryReader.ReadByte();               // 0 = offline, 1 = online
            m_numWeatherForecastSamples = binaryReader.ReadByte(); // Number of weather samples to follow
            for (int i = 0; i < 56; i++)
            {
                m_weatherForecastSamples[i].m_sessionType = binaryReader.ReadByte();              // 0 = unknown, 1 = P1, 2 = P2, 3 = P3, 4 = Short P, 5 = Q1
                                                                                                  // 6 = Q2, 7 = Q3, 8 = Short Q, 9 = OSQ, 10 = R, 11 = R2
                                                                                                  // 12 = Time Trial
                m_weatherForecastSamples[i].m_timeOffset = binaryReader.ReadByte();               // Time in minutes the forecast is for
                m_weatherForecastSamples[i].m_weather = binaryReader.ReadByte();                  // Weather - 0 = clear, 1 = light cloud, 2 = overcast
                                                                                                  // 3 = light rain, 4 = heavy rain, 5 = storm
                m_weatherForecastSamples[i].m_trackTemperature = binaryReader.ReadSByte();         // Track temp. in degrees Celsius
                m_weatherForecastSamples[i].m_trackTemperatureChange = binaryReader.ReadSByte();   // Track temp. change – 0 = up, 1 = down, 2 = no change
                m_weatherForecastSamples[i].m_airTemperature = binaryReader.ReadSByte();           // Air temp. in degrees celsius
                m_weatherForecastSamples[i].m_airTemperatureChange = binaryReader.ReadSByte();     // Air temp. change – 0 = up, 1 = down, 2 = no change
                m_weatherForecastSamples[i].m_rainPercentage = binaryReader.ReadByte();           // Rain percentage (0-100)

            }
            m_forecastAccuracy = binaryReader.ReadByte();          // 0 = Perfect, 1 = Approximate
            m_aiDifficulty = binaryReader.ReadByte();              // AI Difficulty rating – 0-110
            m_seasonLinkIdentifier = binaryReader.ReadUInt32();    // Identifier for season - persists across saves
            m_weekendLinkIdentifier = binaryReader.ReadUInt32();   // Identifier for weekend - persists across saves
            m_sessionLinkIdentifier = binaryReader.ReadUInt32();   // Identifier for session - persists across saves
            m_pitStopWindowIdealLap = binaryReader.ReadByte();     // Ideal lap to pit on for current strategy (player)
            m_pitStopWindowLatestLap = binaryReader.ReadByte();    // Latest lap to pit on for current strategy (player)
            m_pitStopRejoinPosition = binaryReader.ReadByte();     // Predicted position to rejoin at (player)
            m_steeringAssist = binaryReader.ReadByte();            // 0 = off, 1 = on
            m_brakingAssist = binaryReader.ReadByte();             // 0 = off, 1 = low, 2 = medium, 3 = high
            m_gearboxAssist = binaryReader.ReadByte();             // 1 = manual, 2 = manual & suggested gear, 3 = auto
            m_pitAssist = binaryReader.ReadByte();                 // 0 = off, 1 = on
            m_pitReleaseAssist = binaryReader.ReadByte();          // 0 = off, 1 = on
            m_ERSAssist = binaryReader.ReadByte();                 // 0 = off, 1 = on
            m_DRSAssist = binaryReader.ReadByte();                 // 0 = off, 1 = on
            m_dynamicRacingLine = binaryReader.ReadByte();         // 0 = off, 1 = corners only, 2 = full
            m_dynamicRacingLineType = binaryReader.ReadByte();     // 0 = 2D, 1 = 3D
        }
    }

    public class PacketLapData : packet
    {
        const int length = 970;

        public struct LapData
        {

            public uint m_lastLapTimeInMS;            // Last lap time in milliseconds
            public uint m_currentLapTimeInMS;     // Current time around the lap in milliseconds
            public UInt16 m_sector1TimeInMS;           // Sector 1 time in milliseconds
            public UInt16 m_sector2TimeInMS;           // Sector 2 time in milliseconds
            public float m_lapDistance;         // Distance vehicle is around current lap in metres – could
                                                // be negative if line hasn’t been crossed yet
            public float m_totalDistance;       // Total distance travelled in session in metres – could
                                                // be negative if line hasn’t been crossed yet
            public float m_safetyCarDelta;            // Delta in seconds for safety car
            public byte m_carPosition;             // Car race position
            public byte m_currentLapNum;       // Current lap number
            public byte m_pitStatus;               // 0 = none, 1 = pitting, 2 = in pit area
            public byte m_numPitStops;                 // Number of pit stops taken in this race
            public byte m_sector;                  // 0 = sector1, 1 = sector2, 2 = sector3
            public byte m_currentLapInvalid;       // Current lap invalid - 0 = valid, 1 = invalid
            public byte m_penalties;               // Accumulated time penalties in seconds to be added
            public byte m_warnings;                  // Accumulated number of warnings issued
            public byte m_numUnservedDriveThroughPens;  // Num drive through pens left to serve
            public byte m_numUnservedStopGoPens;        // Num stop go pens left to serve
            public byte m_gridPosition;            // Grid position the vehicle started the race in
            public byte m_driverStatus;            // Status of driver - 0 = in garage, 1 = flying lap (tour lancé)
                                                   // 2 = in lap, 3 = out lap, 4 = on track
            public byte m_resultStatus;              // Result status - 0 = invalid, 1 = inactive, 2 = active
                                                     // 3 = finished, 4 = didnotfinish, 5 = disqualified
                                                     // 6 = not classified, 7 = retired
            public byte m_pitLaneTimerActive;          // Pit lane timing, 0 = inactive, 1 = active
            public UInt16 m_pitLaneTimeInLaneInMS;      // If active, the current time spent in the pit lane in ms
            public UInt16 m_pitStopTimerInMS;           // Time of the actual pit stop in ms
            public byte m_pitStopShouldServePen;       // Whether the car should serve a penalty at this stop
        }

        public LapData[] m_lapData = new LapData[22];         // Lap data for all cars on track

        public PacketLapData()
        {
        }

        public PacketLapData(PacketHeader packetheader, Stream stream, BinaryReader binaryReader)
        {
            init(stream, binaryReader);
            tranfertHeader(packetheader);

        }

        public void init(Stream stream, BinaryReader binaryReader)
        {
            if (stream.Length == length)
                PacketData(stream, binaryReader);
            else
                Console.WriteLine("erreur inf " + length);
        }

        public void PacketData(Stream stream, BinaryReader binaryReader)
        {
            for (int i = 0; i < 22; i++)
            {
                m_lapData[i].m_lastLapTimeInMS = binaryReader.ReadUInt32();
                m_lapData[i].m_currentLapTimeInMS = binaryReader.ReadUInt32();
                m_lapData[i].m_sector1TimeInMS = binaryReader.ReadUInt16();
                m_lapData[i].m_sector2TimeInMS = binaryReader.ReadUInt16();
                m_lapData[i].m_lapDistance = binaryReader.ReadSingle();
                m_lapData[i].m_totalDistance = binaryReader.ReadSingle();
                m_lapData[i].m_safetyCarDelta = binaryReader.ReadSingle();
                m_lapData[i].m_carPosition = binaryReader.ReadByte();
                m_lapData[i].m_currentLapNum = binaryReader.ReadByte();
                m_lapData[i].m_pitStatus = binaryReader.ReadByte();
                m_lapData[i].m_numPitStops = binaryReader.ReadByte();
                m_lapData[i].m_sector = binaryReader.ReadByte();
                m_lapData[i].m_currentLapInvalid = binaryReader.ReadByte();
                m_lapData[i].m_penalties = binaryReader.ReadByte();
                m_lapData[i].m_warnings = binaryReader.ReadByte();
                m_lapData[i].m_numUnservedDriveThroughPens = binaryReader.ReadByte();
                m_lapData[i].m_numUnservedStopGoPens = binaryReader.ReadByte();
                m_lapData[i].m_gridPosition = binaryReader.ReadByte();
                m_lapData[i].m_driverStatus = binaryReader.ReadByte();
                m_lapData[i].m_resultStatus = binaryReader.ReadByte();
                m_lapData[i].m_pitLaneTimerActive = binaryReader.ReadByte();
                m_lapData[i].m_pitLaneTimeInLaneInMS = binaryReader.ReadUInt16();
                m_lapData[i].m_pitStopTimerInMS = binaryReader.ReadUInt16();
                m_lapData[i].m_pitStopShouldServePen = binaryReader.ReadByte();
            }
        }
    }

    public  class Motion_packet : packet
    {
        const int length = 1464;

        public struct CarMotionData
        {
            public float m_worldPositionX;           // World space X position
            public float m_worldPositionY;           // World space Y position
            public float m_worldPositionZ;           // World space Z position
            public float m_worldVelocityX;           // Velocity in world space X
            public float m_worldVelocityY;           // Velocity in world space Y
            public float m_worldVelocityZ;           // Velocity in world space Z
            public Int16 m_worldForwardDirX;         // World space forward X direction (normalised)
            public Int16 m_worldForwardDirY;         // World space forward Y direction (normalised)
            public Int16 m_worldForwardDirZ;         // World space forward Z direction (normalised)
            public Int16 m_worldRightDirX;           // World space right X direction (normalised)
            public Int16 m_worldRightDirY;           // World space right Y direction (normalised)
            public Int16 m_worldRightDirZ;           // World space right Z direction (normalised)
            public float m_gForceLateral;            // Lateral G-Force component
            public float m_gForceLongitudinal;       // Longitudinal G-Force component
            public float m_gForceVertical;           // Vertical G-Force component
            public float m_yaw;                      // Yaw angle in radians
            public float m_pitch;                    // Pitch angle in radians
            public float m_roll;                     // Roll angle in radians
        }

        public CarMotionData[] m_carMotionData = new CarMotionData[22];

        //PacketHeader m_header;                  // Header

        // Extra player car ONLY data
        public float[] m_suspensionPosition = new float[4];       // Note: All wheel arrays have the following order:
        public float[] m_suspensionVelocity = new float[4];       // RL, RR, FL, FR
        public float[] m_suspensionAcceleration = new float[4];  // RL, RR, FL, FR
        public float[] m_wheelSpeed = new float[4];              // Speed of each wheel
        public float[] m_wheelSlip = new float[4];                // Slip ratio for each wheel
        public float m_localVelocityX;             // Velocity in local space
        public float m_localVelocityY;             // Velocity in local space
        public float m_localVelocityZ;             // Velocity in local space
        public float m_angularVelocityX;           // Angular velocity x-component
        public float m_angularVelocityY;            // Angular velocity y-component
        public float m_angularVelocityZ;            // Angular velocity z-component
        public float m_angularAccelerationX;        // Angular velocity x-component
        public float m_angularAccelerationY;        // Angular velocity y-component
        public float m_angularAccelerationZ;        // Angular velocity z-component
        public float m_frontWheelsAngle;            // Current front wheels angle in radians

        public Motion_packet()
        {
        }

        public Motion_packet(PacketHeader packetheader, Stream stream, BinaryReader binaryReader)
        {
            init(stream, binaryReader);
            tranfertHeader(packetheader);

        }

        public void init(Stream stream, BinaryReader binaryReader)
        {
            if (stream.Length == length)
                PacketData(stream, binaryReader);
            else
                Console.WriteLine("erreur inf "+ length);
        }

        public void ReadCarMotionData(int i, Stream stream, BinaryReader binaryReader)
        {
            m_carMotionData[i].m_worldPositionX = binaryReader.ReadSingle();
            m_carMotionData[i].m_worldPositionY = binaryReader.ReadSingle();
            m_carMotionData[i].m_worldPositionZ = binaryReader.ReadSingle();
            m_carMotionData[i].m_worldVelocityX = binaryReader.ReadSingle();
            m_carMotionData[i].m_worldVelocityY = binaryReader.ReadSingle();
            m_carMotionData[i].m_worldVelocityZ = binaryReader.ReadSingle();
            m_carMotionData[i].m_worldForwardDirX = binaryReader.ReadInt16();
            m_carMotionData[i].m_worldForwardDirY = binaryReader.ReadInt16();
            m_carMotionData[i].m_worldForwardDirZ = binaryReader.ReadInt16();
            m_carMotionData[i].m_worldRightDirX = binaryReader.ReadInt16();
            m_carMotionData[i].m_worldRightDirY = binaryReader.ReadInt16();
            m_carMotionData[i].m_worldRightDirZ = binaryReader.ReadInt16();
            m_carMotionData[i].m_gForceLateral = binaryReader.ReadSingle();
            m_carMotionData[i].m_gForceLongitudinal = binaryReader.ReadSingle();
            m_carMotionData[i].m_gForceVertical = binaryReader.ReadSingle();
            m_carMotionData[i].m_yaw = binaryReader.ReadSingle();
            m_carMotionData[i].m_pitch = binaryReader.ReadSingle();
            m_carMotionData[i].m_roll = binaryReader.ReadSingle();
        }

        public void PacketData(Stream stream, BinaryReader binaryReader)
        {
            // https://github.com/ChadRMacLean/PyF1Telemetry/blob/master/src/packets/motion.py
            //m_header: PacketHeader;

            // Data for all cars on track
            // Extra player car ONLY data

            //Console.WriteLine("pos :" + stream.Position);

            for (int i = 0; i < 22; i++)
                ReadCarMotionData(i,stream, binaryReader) ;

            for (int i = 0; i < 4; i++)
                m_suspensionPosition[i] = binaryReader.ReadSingle();

            for (int i = 0; i < 4; i++)
                m_suspensionVelocity[i] = binaryReader.ReadSingle();

            for (int i = 0; i < 4; i++)
                m_suspensionAcceleration[i] = binaryReader.ReadSingle();

            for (int i = 0; i < 4; i++)
                m_wheelSpeed[i] = binaryReader.ReadSingle();

            for (int i = 0; i < 4; i++)
                m_wheelSlip[i] = binaryReader.ReadSingle();

            m_localVelocityX = binaryReader.ReadSingle();
            m_localVelocityY = binaryReader.ReadSingle();
            m_localVelocityZ = binaryReader.ReadSingle();
            m_angularVelocityX = binaryReader.ReadSingle();
            m_angularVelocityY = binaryReader.ReadSingle();
            m_angularVelocityZ = binaryReader.ReadSingle();
            m_angularAccelerationX = binaryReader.ReadSingle();
            m_angularAccelerationY = binaryReader.ReadSingle();
            m_angularAccelerationZ = binaryReader.ReadSingle();
            m_frontWheelsAngle = binaryReader.ReadSingle();
        }

    }

    public class PacketHeader
    {
        public UInt16 m_packetFormat;          // 2021
        public byte m_gameMajorVersion;        // Game major version - "X.00"
        public byte m_gameMinorVersion;        // Game minor version - "1.XX"
        public byte m_packetVersion;           // Version of this packet type,
                                               // all start from 1
        public byte m_packetId;                // Identifier for the packet type,
                                               // see below
        public ulong m_sessionUID;              // Unique identifier for the session

        public float m_sessionTime;             // Session timestamp
        public UInt32 m_frameIdentifier;         // Identifier for the frame the data
                                                 // was retrieved on
        public byte m_playerCarIndex;          // Index of player's car in the array
        public byte m_secondaryPlayerCarIndex; // Index of secondary player's car in
                                               // the array (split-screen)
                                               // 255 if no second player
    }

    public class TeamID : IEquatable<TeamID>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
        
        public bool Equals(TeamID other)
        {
            if (other == null) return false;
            return (this.Id.Equals(other.Id));
        }
    }

    public class ButtonID : IEquatable<ButtonID>
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(ButtonID other)
        {
            if (other == null) return false;
            return (this.Id.Equals(other.Id));
        }
    }

    public class UDP_f12021
    {
        int NumPlayer = 22; //22 pour F1 202x et variable pour les autres

        internal Setting dataSetting = new Setting();

        private const float Ktime = 1000 * 60 * 60 * 24;

        public List<TeamID> ListTeamID;
        public List<ButtonID> ListButtonID;
        public List<string> Trackname = new List<string>() { 
                "Melbourne",
                "Paul Ricard",
                "Shanghai",
                "Sakhir (Bahrain)",
                "Catalunya",
                "Monaco",
                "Montreal",
                "Silverstone",
                "Hockenheim",
                "Hungaroring",
                "Spa",
                "Monza",
                "Singapore",
                "Suzuka",
                "Abu Dhabi",
                "Texas",
                "Brazil",
                "Austria",
                "Sochi",
                "Mexico",
                "Baku (Azerbaijan)",
                "Sakhir Short",
                "Silverstone Short",
                "Texas Short",
                "Suzuka Short",
                "Hanoi",
                "Zandvoort",
                "Imola",
                "Portimão",
                "Jeddah",
                "Miami",
                "Las Vegas",
                "Losail"
            };
        public List<string> sessiontypename = new List<string>()
        {
            "unknown","P1","P2","P3","Short P",
            "Q1", "Q2", "Q3", "Short Q","OSQ",
            "R", "R2", "R3", "Time Trial"
        };
        public List<string> formulaname = new List<string>()
        {
            "F1 Modern",
            "F1 Classic",
            "F2",
            "F1 Generic",
            "F1 2022",
            "F1 2022 HyperCar"
        };
        public List<string> meteoname = new List<string>()
        {
            "clear",
            "light cloud",
            "overcast","light rain", "heavy rain", "storm"
        };
        public List<string> Trackdriver = new List<string>() {
            "Carlos Sainz",
            "Daniil Kvyat",
            "Daniel Ricciardo",
            "Fernando Alonso",
            "Felipe Massa",
            "",
            "Kimi Räikkönen",
            "Lewis Hamilton",
            "",
            "Max Verstappen",
            "Nico Hulkenburg",
            "Kevin Magnussen",
            "Romain Grosjean",
            "Sebastian Vettel",
            "Sergio Perez",
            "Valtteri Bottas",
            "",
            "Esteban Ocon",
            "",
            "Lance Stroll",
            "Arron Barnes",
            "Martin Giles",
            "Alex Murray",
            "Lucas Roth",
            "Igor Correia",
            "Sophie Levasseur",
            "Jonas Schiffer",
            "Alain Forest",
            "Jay Letourneau",
            "Esto Saari",
            "Yasar Atiyeh",
            "Callisto Calabresi",
            "Naota Izum",
            "Howard Clarke",
            "Wilheim Kaufmann",
            "Marie Laursen",
            "Flavio Nieves",
            "Peter Belousov",
            "Klimek Michalski",
            "Santiago Moreno",
            "Benjamin Coppens",
            "Noah Visser",
            "Gert Waldmuller",
            "Julian Quesada",
            "Daniel Jones",
            "Artem Markelov",
            "Tadasuke Makino",
            "Sean Gelael",
            "Nyck De Vries",
            "Jack Aitken",
            "George Russell",
            "Maximilian Günther",
            "Nirei Fukuzumi",
            "Luca Ghiotto",
            "Lando Norris",
            "Sérgio Sette Câmara",
            "Louis Delétraz",
            "Antonio Fuoco",
            "Charles Leclerc",
            "Pierre Gasly",
            "",
            "",
            "Alexander Albon",
            "Nicholas Latifi",
            "Dorian Boccolacci",
            "Niko Kari",
            "Roberto Merhi",
            "Arjun Maini",
            "Alessio Lorandi",
            "Ruben Meijer",
            "Rashid Nair",
            "Jack Tremblay",
            "Devon Butler",
            "Lukas Weber",
            "Antonio Giovinazzi",
            "Robert Kubica",
            "Alain Prost",
            "Ayrton Senna",
            "Nobuharu Matsushita",
            "Nikita Mazepin",
            "Guanya Zhou",
            "Mick Schumacher",
            "Callum Ilott",
            "Juan Manuel Correa",
            "Jordan King",
            "Mahaveer Raghunathan",
            "Tatiana Calderon",
            "Anthoine Hubert",
            "Guiliano Alesi",
            "Ralph Boschung",
            "Michael Schumacher",
            "Dan Ticktum",
            "Marcus Armstrong",
            "Christian Lundgaard",
            "Yuki Tsunoda",
            "Jehan Daruvala",
            "Gulherme Samaia",
            "Pedro Piquet",
            "Felipe Drugovich",
            "Robert Schwartzman",
            "Roy Nissany",
            "Marino Sato",
            "Aidan Jackson",
            "Casper Akkerman",
            "",
            "",
            "",
            "",
            "",
            "Jenson Button",
            "David Coulthard",
            "Nico Rosberg" };
        public List<string> Nationality = new List<string>() {
            "",
            "American",
            "Argentinean",
            "Australian",
            "Austrian",
            "Azerbaijani",
            "Bahraini",
            "Belgian",
            "Bolivian",
            "Brazilian",
            "British",
            "Bulgarian",
            "Cameroonian",
            "Canadian",
            "Chilean",
            "Chinese",
            "Colombian",
            "Costa Rican",
            "Croatian",
            "Cypriot",
            "Czech",
            "Danish",
            "Dutch",
            "Ecuadorian",
            "English",
            "Emirian",
            "Estonian",
            "Finnish",
            "French",
            "German",
            "Ghanaian",
            "Greek",
            "Guatemalan",
            "Honduran",
            "Hong Konger",
            "Hungarian",
            "Icelander",
            "Indian",
            "Indonesian",
            "Irish",
            "Israeli",
            "Italian",
            "Jamaican",
            "Japanese",
            "Jordanian",
            "Kuwaiti",
            "Latvian",
            "Lebanese",
            "Lithuanian",
            "Luxembourger",
            "Malaysian",
            "Maltese",
            "Mexican",
            "Monegasque",
            "New Zealander",
            "Nicaraguan",
            "Northern Irish",
            "Norwegian",
            "Omani",
            "Pakistani",
            "Panamanian",
            "Paraguayan",
            "Peruvian",
            "Polish",
            "Portuguese",
            "Qatari",
            "Romanian",
            "Russian",
            "Salvadoran",
            "Saudi",
            "Scottish",
            "Serbian",
            "Singaporean",
            "Slovakian",
            "Slovenian",
            "South Korean",
            "South African",
            "Spanish",
            "Swedish",
            "Swiss",
            "Thai",
            "Turkish",
            "Uruguayan",
            "Ukrainian",
            "Venezuelan",
            "Barbadian",
            "Welsh",
            "Vietnamese" };
        public string[] TrackID = new string[30]
        {
             "Melbourne",
             "Paul Ricard",
             "Shanghai",
             "Sakhir (Bahrain)",
             "Catalunya",
             "Monaco",
             "Montreal",
             "Silverstone",
             "Hockenheim",
             "Hungaroring",
             "Spa",
             "Monza",
             "Singapore",
             "Suzuka",
             "Abu Dhabi",
             "Texas",
             "Brazil",
             "Austria",
             "Sochi",
             "Mexico",
             "Baku (Azerbaijan)",
             "Sakhir Short",
             "Silverstone Short",
             "Texas Short",
             "Suzuka Short",
             "Hanoi",
             "Zandvoort",
             "Imola",
             "Portimão",
             "Jeddah"
        };
        public List<string> Surface = new List<string>() {
            "Tarmac",
            "Rumble strip",
            "Concrete",
            "Rock",
            "Gravel",
            "Mud",
            "Sand",
            "Grass",
            "Water",
            "Cobblestone",
            "Metal",
            "Ridged" };


        //  adding an array in a List
        // var Teams = new List<string>();
        // Teams.AddRange(TeamID);

        public List<Motion_packet> ListMotion_packet;
        public List<PacketLapData> ListLapData;
        public List<Session_packet> ListSession;
        public List<Telemetry_packet> ListTelemetry;
        public List<Final_packet> ListFinal;
        public List<Lobby_packet> ListLobby;
        public List<Players_packet> ListPlayers;
        public List<Historic_packet> ListHistoric;
        public List<CarStatus_packet> ListCarStatus;
        public List<CarSetups_packet> ListCarSetup;
        public List<CarDamage_packet> ListCarDamage;
        public List<Event_packet> ListEvent;
        public PacketHeader packetHeader;

        public int [] packet = new int[20];

        public List<string> ListData1 = new List<string>()
        {
            "m_speed",
            "m_throttle",
            "m_steer",
            "m_brake",
            "m_clutch",
            "m_gear",
            "m_engineRPM",
            "m_drs",
            "m_revLightsPercent",
            "m_revLightsBitValue",
            "m_brakesTemperature[0]",
            "m_brakesTemperature[1]",
            "m_brakesTemperature[2]",
            "m_brakesTemperature[3]",
            "m_tyresSurfaceTemperature[0]",
            "m_tyresSurfaceTemperature[1]",
            "m_tyresSurfaceTemperature[2]",
            "m_tyresSurfaceTemperature[3]",
            "m_tyresInnerTemperature[0]",
            "m_tyresInnerTemperature[1]",
            "m_tyresInnerTemperature[2]",
            "m_tyresInnerTemperature[3]",
            "m_engineTemperature",
            "m_tyresPressure[0]",
            "m_tyresPressure[1]",
            "m_tyresPressure[2]",
            "m_tyresPressure[3]",
            "m_surfaceType[0]",
            "m_surfaceType[1]",
            "m_surfaceType[2]",
            "m_surfaceType[3]",
            "m_worldPositionX",
            "m_worldPositionY",
            "m_worldPositionZ",
            "m_worldVelocityX",
            "m_worldVelocityY",
            "m_worldVelocityZ",
            "m_worldForwardDirX",
            "m_worldForwardDirY",
            "m_worldForwardDirZ",
            "m_worldRightDirX",
            "m_worldRightDirY",
            "m_worldRightDirZ",
            "m_gForceLateral",
            "m_gForceLongitudinal",
            "m_gForceVertical",
            "m_yaw",
            "m_pitch",
            "m_roll"
        };
        public List<string> ListData2 = new List<string>()
        {
            "m_suspensionPosition",
            "m_suspensionVelocity",
            "m_suspensionAcceleration",
            "m_wheelSpeed",
            "m_wheelSlip",
            "localVelocityX",
            "localVelocityY",
            "localVelocityZ",
            "angularVelocityX",
            "angularVelocityY",
            "angularVelocityZ",
            "angularAccelerationX",
            "angularAccelerationY",
            "angularAccelerationZ",
            "frontWheelsAngle"
        };

        internal void InitListTrack()
        {
            List<string> Trackname = new List<string>() { "Melbourne",
                "Paul Ricard",
                "Shanghai",
                "Sakhir (Bahrain)",
                "Catalunya",
                "Monaco",
                "Montreal",
                "Silverstone",
                "Hockenheim",
                "Hungaroring",
                "Spa",
                "Monza",
                "Singapore",
                "Suzuka",
                "Abu Dhabi",
                "Texas",
                "Brazil",
                "Austria",
                "Sochi",
                "Mexico",
                "Baku (Azerbaijan)",
                "Sakhir Short",
                "Silverstone Short",
                "Texas Short",
                "Suzuka Short",
                "Hanoi",
                "Zandvoort",
                "Imola",
                "Portimão",
                "Jeddah"
            };
        }

        internal void InitListButton()
        {
            ListButtonID.Add(new ButtonID() { Id = "0x00000001", Name = "Cross or A" });
            ListButtonID.Add(new ButtonID() { Id = "0x00000002", Name = "Triangle or Y" });
            ListButtonID.Add(new ButtonID() { Id = "0x00000004", Name = "Circle or B" });
            ListButtonID.Add(new ButtonID() { Id = "0x00000008", Name = "Square or X" });
            ListButtonID.Add(new ButtonID() { Id = "0x00000010", Name = "D-pad Left" });
            ListButtonID.Add(new ButtonID() { Id = "0x00000020", Name = "D-pad Right" });
            ListButtonID.Add(new ButtonID() { Id = "0x00000040", Name = "D-pad Up" });
            ListButtonID.Add(new ButtonID() { Id = "0x00000080", Name = "D-pad Down" });
            ListButtonID.Add(new ButtonID() { Id = "0x00000100", Name = "Options or Menu" });
            ListButtonID.Add(new ButtonID() { Id = "0x00000200", Name = "L1 or LB" });
            ListButtonID.Add(new ButtonID() { Id = "0x00000400", Name = "R1 or RB" });
            ListButtonID.Add(new ButtonID() { Id = "0x00000800", Name = "L2 or LT" });
            ListButtonID.Add(new ButtonID() { Id = "0x00001000", Name = "R2 or RT" });
            ListButtonID.Add(new ButtonID() { Id = "0x00002000", Name = "Left Stick Click" });
            ListButtonID.Add(new ButtonID() { Id = "0x00004000", Name = "Right Stick Click" });
            ListButtonID.Add(new ButtonID() { Id = "0x00008000", Name = "Right Stick Left" });
            ListButtonID.Add(new ButtonID() { Id = "0x00010000", Name = "Right Stick Right" });
            ListButtonID.Add(new ButtonID() { Id = "0x00020000", Name = "Right Stick Up" });
            ListButtonID.Add(new ButtonID() { Id = "0x00040000", Name = "Right Stick Down" });
            ListButtonID.Add(new ButtonID() { Id = "0x00080000", Name = "Special" });

        }

        internal void InitListTeam()
        {
            ListTeamID.Add(new TeamID() { Id = 0, Name = "Mercedes" });
            ListTeamID.Add(new TeamID() { Id = 1, Name = "Ferrari" });
            ListTeamID.Add(new TeamID() { Id = 2, Name = "Red Bull Racing" });
            ListTeamID.Add(new TeamID() { Id = 3, Name = "Williams" });
            ListTeamID.Add(new TeamID() { Id = 4, Name = "Aston Martin" });
            ListTeamID.Add(new TeamID() { Id = 5, Name = "Alpine" });
            ListTeamID.Add(new TeamID() { Id = 6, Name = "Alpha Tauri" });
            ListTeamID.Add(new TeamID() { Id = 7, Name = "Haas" });
            ListTeamID.Add(new TeamID() { Id = 8, Name = "McLaren" });
            ListTeamID.Add(new TeamID() { Id = 9, Name = "Alfa Romeo" });
            ListTeamID.Add(new TeamID() { Id = 42, Name = "Art GP ’19" });
            ListTeamID.Add(new TeamID() { Id = 43, Name = "Campos ’19" });
            ListTeamID.Add(new TeamID() { Id = 44, Name = "Carlin ’19" });
            ListTeamID.Add(new TeamID() { Id = 45, Name = "Sauber Junior Charouz ’19" });
            ListTeamID.Add(new TeamID() { Id = 46, Name = "Dams ’19" });
            ListTeamID.Add(new TeamID() { Id = 47, Name = "Uni-Virtuosi ‘19" });
            ListTeamID.Add(new TeamID() { Id = 48, Name = "MP Motorsport ‘19" });
            ListTeamID.Add(new TeamID() { Id = 49, Name = "Prema ’19" });
            ListTeamID.Add(new TeamID() { Id = 50, Name = "Trident ’19" });
            ListTeamID.Add(new TeamID() { Id = 51, Name = "Arden ’19" });
            ListTeamID.Add(new TeamID() { Id = 70, Name = "Art GP ‘20" });
            ListTeamID.Add(new TeamID() { Id = 71, Name = "Campos ‘20" });
            ListTeamID.Add(new TeamID() { Id = 72, Name = "Carlin ‘20" });
            ListTeamID.Add(new TeamID() { Id = 73, Name = "Charouz ‘20" });
            ListTeamID.Add(new TeamID() { Id = 74, Name = "Dams ‘20" });
            ListTeamID.Add(new TeamID() { Id = 75, Name = "Uni-Virtuosi ‘20" });
            ListTeamID.Add(new TeamID() { Id = 76, Name = "MP Motorsport ‘20" });
            ListTeamID.Add(new TeamID() { Id = 77, Name = "Prema ‘20" });
            ListTeamID.Add(new TeamID() { Id = 78, Name = "Trident ‘20" });
            ListTeamID.Add(new TeamID() { Id = 79, Name = "BWT ‘20" });
            ListTeamID.Add(new TeamID() { Id = 80, Name = "Hitech ‘20" });
            ListTeamID.Add(new TeamID() { Id = 85, Name = "Mercedes 2020" });
            ListTeamID.Add(new TeamID() { Id = 86, Name = "Ferrari 2020" });
            ListTeamID.Add(new TeamID() { Id = 87, Name = "Red Bull 2020" });
            ListTeamID.Add(new TeamID() { Id = 88, Name = "Williams 2020" });
            ListTeamID.Add(new TeamID() { Id = 89, Name = "Racing Point 2020" });
            ListTeamID.Add(new TeamID() { Id = 90, Name = "Renault 2020" });
            ListTeamID.Add(new TeamID() { Id = 91, Name = "Alpha Tauri 2020" });
            ListTeamID.Add(new TeamID() { Id = 92, Name = "Haas 2020" });
            ListTeamID.Add(new TeamID() { Id = 93, Name = "McLaren 2020" });
            ListTeamID.Add(new TeamID() { Id = 94, Name = "Alfa Romeo 2020" });
        }

        public UDP_f12021() // Constructeur avec ini
        {
            //string Team;

            ListMotion_packet = new List<Motion_packet>();
            ListLapData = new List<PacketLapData>();
            ListSession = new List<Session_packet>();
            ListTelemetry = new List<Telemetry_packet>();
            ListFinal = new List<Final_packet>();
            ListLobby = new List<Lobby_packet>();
            ListPlayers = new List<Players_packet>();
            packetHeader = new PacketHeader();
            ListTeamID = new List<TeamID>();
            ListButtonID = new List<ButtonID>();
            ListHistoric = new List<Historic_packet>();
            ListCarStatus = new List<CarStatus_packet>();
            ListCarSetup = new List<CarSetups_packet>();
            ListCarDamage = new List<CarDamage_packet>();
            ListEvent = new List<Event_packet>();

            InitListTeam();
            InitListTrack();
            InitListButton();

            // Fonctionne
            //int numberIndex = ListTeamID.FindIndex(x => x.Id == 47);
            //Team = ListTeamID[numberIndex].Name;
            //Console.WriteLine(Team);
        }

        public void DeletePackets()
        {
            ListMotion_packet.Clear();
            ListSession.Clear();
            ListLapData.Clear();
            ListEvent.Clear();
            ListPlayers.Clear();
            ListCarSetup.Clear();
            ListTelemetry.Clear();
            ListCarStatus.Clear();
            ListFinal.Clear();
            ListLobby.Clear();
            ListCarDamage.Clear();
            ListHistoric.Clear();
        }
        public void InitPackets(Stream stream, BinaryReader binaryReader)
        {
            stream.Position = 0;

            packetHeader.m_packetFormat = binaryReader.ReadUInt16();
            packetHeader.m_gameMajorVersion = binaryReader.ReadByte();
            packetHeader.m_gameMinorVersion = binaryReader.ReadByte();
            packetHeader.m_packetVersion = binaryReader.ReadByte();
            packetHeader.m_packetId = binaryReader.ReadByte();
            packetHeader.m_sessionUID = binaryReader.ReadUInt64();
            packetHeader.m_sessionTime = binaryReader.ReadSingle();
            packetHeader.m_frameIdentifier = binaryReader.ReadUInt32();
            packetHeader.m_playerCarIndex = binaryReader.ReadByte();
            packetHeader.m_secondaryPlayerCarIndex = binaryReader.ReadByte();

            packet[packetHeader.m_packetId]++;

            switch (packetHeader.m_packetId)
            {
                case 0: //Motion	0	Contains all motion data for player’s car – only sent while player is in control
                    ListMotion_packet.Add(new Motion_packet(packetHeader, stream, binaryReader));
                    break;
                case 1: // Session	1	Data about the session – track, time left
                    ListSession.Add(new Session_packet(packetHeader, stream, binaryReader));
                    break;
                case 2: // Lap Data	2	Data about all the lap times of cars in the session
                    ListLapData.Add(new PacketLapData(packetHeader, stream, binaryReader));
                    break;
                case 3: // Event	3	Various notable events that happen during a session
                    ListEvent.Add(new Event_packet(packetHeader, stream, binaryReader));
                    break;
                case 4: // Participants	4	List of participants in the session, mostly relevant for multiplayer
                    ListPlayers.Add(new Players_packet(packetHeader, stream, binaryReader));
                    break;
                case 5: // Car Setups	5	Packet detailing car setups for cars in the race
                    ListCarSetup.Add(new CarSetups_packet(packetHeader, stream, binaryReader));
                    break;
                case 6: // Car Telemetry	6	Telemetry data for all cars
                    ListTelemetry.Add(new Telemetry_packet(packetHeader, stream, binaryReader));
                    break;
                case 7: // Car Status	7	Status data for all cars
                    ListCarStatus.Add(new CarStatus_packet(packetHeader, stream, binaryReader));
                    break;
                case 8: // Final Classification	8	Final classification confirmation at the end of a race
                    ListFinal.Add(new Final_packet(packetHeader, stream, binaryReader));
                    break;
                case 9: // Lobby Info	9	Information about players in a multiplayer lobby
                    ListLobby.Add(new Lobby_packet(packetHeader, stream, binaryReader));
                    break;
                case 10:// Car Damage	10	Damage status for all cars
                    ListCarDamage.Add(new CarDamage_packet(packetHeader, stream, binaryReader));
                    break;
                case 11: // Session History	11	Lap and tyre data for session
                    ListHistoric.Add(new Historic_packet(packetHeader, stream, binaryReader));
                    break;
                default:
                    Console.WriteLine($"m_packetId value is {packetHeader.m_packetId}.");
                    break;
            }
        }
        public float g_bestLapTimeLap(int m_numCars, out int pnumLap)
        {
            int Indextableau = ListHistoric.Count - 1;
            while (ListHistoric[Indextableau].m_carIdx != m_numCars && Indextableau > 0)
                Indextableau--;
            pnumLap = ListHistoric[Indextableau].m_bestLapTimeLapNum;
            if (ListHistoric[Indextableau].m_bestLapTimeLapNum == 0)
            {
                return (0);
            }
            else
                return (ListHistoric[Indextableau].m_lapHistoryData[
                ListHistoric[Indextableau].m_bestLapTimeLapNum - 1
                                                                                 ].m_lapTimeInMS);
        }
        public float g_BestSector1TimeInMS(int m_numCars, out int pnumLap)
        {
            int Indextableau = ListHistoric.Count - 1;

            while (ListHistoric[Indextableau].m_carIdx != m_numCars && Indextableau > 0)
                Indextableau--;
            pnumLap = ListHistoric[Indextableau].m_bestSector1LapNum;
            if (ListHistoric[Indextableau].m_bestSector1LapNum == 0)
            {
                return (0);// baseDate.Add(TimeSpan.Zero));
            }
            else
                return (ListHistoric[Indextableau].m_lapHistoryData[
                ListHistoric[Indextableau].m_bestSector1LapNum - 1
                                                                                ].m_sector1TimeInMS) ;
        }
        public float g_BestSector2TimeInMS(int m_numCars, out int pnumLap)
        {
            int Indextableau = ListHistoric.Count - 1;

            while (ListHistoric[Indextableau].m_carIdx != m_numCars && Indextableau > 0)
                Indextableau--;
            pnumLap = ListHistoric[Indextableau].m_bestSector2LapNum;
            if (ListHistoric[Indextableau].m_bestSector2LapNum == 0)
            {
                return (0);// baseDate.AddMilliseconds(0));
            }
            else
                return (ListHistoric[Indextableau].m_lapHistoryData[
                ListHistoric[Indextableau].m_bestSector2LapNum - 1
                                                                                ].m_sector2TimeInMS);
        }
        public float g_BestSector3TimeInMS(int m_numCars, out int pnumLap)
        {
            int Indextableau = ListHistoric.Count - 1;

            while (ListHistoric[Indextableau].m_carIdx != m_numCars && Indextableau > 0)
                Indextableau--;
            pnumLap = ListHistoric[Indextableau].m_bestSector3LapNum;
            if (ListHistoric[Indextableau].m_bestSector3LapNum == 0)
            {
                return (0);// baseDate.AddMilliseconds(0));
            }
            else
                return (ListHistoric[Indextableau].m_lapHistoryData[
                ListHistoric[Indextableau].m_bestSector3LapNum - 1
                                                                               ].m_sector3TimeInMS);
        }
        public string m_aiControlled(byte _m_aiControlled)
        {
            if (_m_aiControlled == 1)
                return ("AI");
            else
                return ("Human");
        }
        public string m_driverId(int IndextableauListPlayers, int IndexPlayer)
        {
            byte _m_driverId = ListPlayers[IndextableauListPlayers].m_participants[IndexPlayer].m_driverId;

            if (_m_driverId < 112)
                return (Trackdriver[_m_driverId]);
            else
                if (_m_driverId == 255)
                return (ListPlayers[IndextableauListPlayers].m_participants[IndexPlayer].m_s_name);
            else
                return ("err " + _m_driverId);
        }
        public string m_teamId(byte _m_teamId)
        {
            if (_m_teamId > ListTeamID.Count-1)
                return ("unknow");
            else
                return (ListTeamID[
                ListTeamID.FindIndex(x => x.Id == _m_teamId)].Name);
        }
        public string m_nationality(byte _m_nationality)
        {
            if (_m_nationality > 0 && _m_nationality < 87)
                return (Nationality[_m_nationality]);
            else
                return ("err " + _m_nationality);
        }
        public string m_bestLapTimeLap(int m_numCars)
        {
            int Indextableau = ListHistoric.Count - 1;
            while (ListHistoric[Indextableau].m_carIdx != m_numCars && Indextableau > 0)
                Indextableau--;
            if (ListHistoric[Indextableau].m_bestLapTimeLapNum == 0)
            {
                return ("err 0");
            }
            else
                return (TimeSpan.FromMilliseconds(ListHistoric[Indextableau].m_lapHistoryData[
                ListHistoric[Indextableau].m_bestLapTimeLapNum - 1
                                                                      ].m_lapTimeInMS).ToString()
                        );
        }
        public string m_bestLapTimeLapNum(int m_numCars)
        {
            int Indextableau = ListHistoric.Count - 1;
            while (ListHistoric[Indextableau].m_carIdx != m_numCars && Indextableau > 0)
                Indextableau--;
            if (ListHistoric[Indextableau].m_bestLapTimeLapNum == 0)
            {
                return ("err 0");
            }
            else
                return ( ListHistoric[Indextableau].m_bestLapTimeLapNum.ToString() );
        }
        public string m_sector1TimeInMS(int m_numCars)
        {
            int Indextableau = ListHistoric.Count - 1;
            while (ListHistoric[Indextableau].m_carIdx != m_numCars && Indextableau > 0)
                Indextableau--;

            if (ListHistoric[Indextableau].m_bestSector1LapNum == 0)
            {
                return ("err 0");
            }
            else
                return ( /*"["+ dataf1_2021.ListHistoric[Indextableau].m_bestSector1LapNum+"] "+*/
            TimeSpan.FromMilliseconds(ListHistoric[Indextableau].m_lapHistoryData[
                ListHistoric[Indextableau].m_bestSector1LapNum - 1
                                                                     ].m_sector1TimeInMS).ToString()
                        );
        }
        public string m_sector2TimeInMS(int m_numCars)
        {
            int Indextableau = ListHistoric.Count - 1;
            while (ListHistoric[Indextableau].m_carIdx != m_numCars && Indextableau > 0)
                Indextableau--;

            if (ListHistoric[Indextableau].m_bestSector2LapNum == 0)
            {
                return ("err 0");
            }
            else
                return (/*"[" + dataf1_2021.ListHistoric[Indextableau].m_bestSector2LapNum + "] " +*/
            TimeSpan.FromMilliseconds(ListHistoric[Indextableau].m_lapHistoryData[
                ListHistoric[Indextableau].m_bestSector2LapNum - 1
                                                                    ].m_sector2TimeInMS).ToString()
                        );
        }
        public string m_sector3TimeInMS(int m_numCars)
        {
            int Indextableau = ListHistoric.Count - 1;
            while (ListHistoric[Indextableau].m_carIdx != m_numCars && Indextableau > 0)
                Indextableau--;

            if (ListHistoric[Indextableau].m_bestSector3LapNum == 0)
            {
                return ("err 0");
            }
            else
                return (/*"[" + dataf1_2021.ListHistoric[Indextableau].m_bestSector3LapNum + "] " +*/
            TimeSpan.FromMilliseconds(ListHistoric[Indextableau].m_lapHistoryData[
                ListHistoric[Indextableau].m_bestSector3LapNum - 1
                                                                    ].m_sector3TimeInMS).ToString()
                        );
        }
        public double m_theoLapTimeInMS(int m_numCars)
        {
            int Indextableau;
            ushort m_sector1TimeInMS;
            ushort m_sector2TimeInMS;
            ushort m_sector3TimeInMS;

            Indextableau = ListHistoric.Count - 1;
            while (ListHistoric[Indextableau].m_carIdx != m_numCars && Indextableau > 0)
                Indextableau--;

            if ((ListHistoric[Indextableau].m_bestSector1LapNum < 1) ||
                    (ListHistoric[Indextableau].m_bestSector2LapNum < 1) ||
                    (ListHistoric[Indextableau].m_bestSector3LapNum < 1))
                return (-1);

            m_sector1TimeInMS = ListHistoric[Indextableau].m_lapHistoryData[
                    ListHistoric[Indextableau].m_bestSector1LapNum - 1
                                                                                                 ].m_sector1TimeInMS;
            m_sector2TimeInMS = ListHistoric[Indextableau].m_lapHistoryData[
                ListHistoric[Indextableau].m_bestSector2LapNum - 1
                                                                                             ].m_sector2TimeInMS;
            m_sector3TimeInMS = ListHistoric[Indextableau].m_lapHistoryData[
                ListHistoric[Indextableau].m_bestSector3LapNum - 1
                                                                                             ].m_sector3TimeInMS;

            if (ListHistoric[Indextableau].m_bestSector1LapNum == 0)
            {
                return (0);
            }
            else
                return TimeSpan.FromMilliseconds(m_sector1TimeInMS + m_sector2TimeInMS + m_sector3TimeInMS).TotalMilliseconds;
        }
        public double m_totalRaceTime(int IndextableauListFinal, int m_numCars)
        {
            if (IndextableauListFinal == 0)
                return 0;
            else
                return ListFinal[IndextableauListFinal].m_classificationData[m_numCars].m_totalRaceTime;
        }
        public bool checkData()
        {
            if (    (ListHistoric.Count > 0) && 
                    (ListTelemetry.Count > 0) && 
                    (ListCarDamage.Count > 0))
                return true;
            return false;
        }
        public void SendSetting(Setting _dataSetting)
        {
            dataSetting = _dataSetting;
        }
        public void InfluxDBwrite()
        {
            for (int i = 0; i < ListTelemetry.Count; i++)
            {
                ListTelemetry[i].dataSetting = dataSetting;
                ListTelemetry[i].writetemetrie();
            }
        }

    }
}
