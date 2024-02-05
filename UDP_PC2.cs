using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


//https://forum.projectcarsgame.com/showthread.php?40113-HowTo-Companion-App-UDP-Streaming
// [HowTo] Companion App - UDP Streaming
// UDP off
// UDP 1 60 / sec(16ms)
// UDP 2 50 / sec(20ms)
// UDP 3 40 / sec(25ms)
// UDP 4 30 / sec(32ms)
// UDP 5 20 / sec(50ms)
// UDP 6 15 / sec(66ms)
// UDP 7 10 / sec(100ms)
// UDP 8 05 / sec(200ms)
// UDP 9 01 / sec(1000ms)

// https://forum.projectcarsgame.com/showthread.php?43125-UDP-Compatible-Apps-List-Yours-Now!


//https://forum.projectcarsgame.com/showthread.php?63374-C-Udp-V2-library/page3
//Donc, pour comprendre un lecteur binaire, vous devez comprendre la structure d'udp.
//Un udp est un message qui diffuse un message en binaire.
//La taille du message binaire dépend de la quantité de données envoyées.
//Disons que nous voulons recevoir un message udp avec deux entiers.
//Nous savons que le message aurait une longueur de 8 octets. Parce qu'un int a une longueur de 4 octets et 2x4 =8.
//Nous pouvons supprimer les octets 0 à 3 et les enregistrer dans la variable 1, puis enregistrer les octets 4 à 8 dans la variable 2.
//Dans le code où vous voyez stream.index = 12, nous indiquons le code du message que nous voulons commencer à l'octet 12
//(le paquet de base sont les octets 0-12 dont nous ne nous soucions pas), puis lorsque vous voyez binaryReader.convertTo.
//Il s'agit de prendre notre position de flux actuelle et de déplacer l'index sur la quantité de type de données utilisée dans la
//conversion et d'enregistrer ces octets dans une variable. Fondamentalement, vous disséquez le message en morceaux.

//Rappelez-vous que le PacketBase contient 3 valeurs importantes. mPartialPacketIndex, mPartialPacketNumber, mPacketType.
//Sans ceux-ci, vous ne pouvez pas déchiffrer correctement les paquets udp. À moins que votre bibliothèque ne le fasse pour l'utilisateur, bien sûr.
// Objectif récupérer les packets 

/// <summa// Wheels / Tyres - https://forum.projectcarsgame.com/archive/index.php/t-56181.html
// unsigned int mTyreFlags[TYRE_MAX]; // [ enum (Type#10) Tyre Flags ]
// unsigned int mTerrain[TYRE_MAX]; // [ enum (Type#11) Terrain Materials ]
// float mTyreY[TYRE_MAX]; // [ UNITS = Local Space Y ]
// float mTyreRPS[TYRE_MAX]; // [ UNITS = Revolutions per second ]
// float mTyreSlipSpeed[TYRE_MAX]; // OBSOLETE, kept for backward compatibility only
// float mTyreTemp[TYRE_MAX]; // [ UNITS = Celsius ] [ UNSET = 0.0f ]
// float mTyreGrip[TYRE_MAX]; // OBSOLETE, kept for backward compatibility only
// float mTyreHeightAboveGround[TYRE_MAX]; // [ UNITS = Local Space Y ]
// float mTyreLateralStiffness[TYRE_MAX]; // OBSOLETE, kept for backward compatibility only
// float mTyreWear[TYRE_MAX]; // [ RANGE = 0.0f->1.0f ]
// float mBrakeDamage[TYRE_MAX]; // [ RANGE = 0.0f->1.0f ]
// float mSuspensionDamage[TYRE_MAX]; // [ RANGE = 0.0f->1.0f ]
// float mBrakeTempCelsius[TYRE_MAX]; // [ UNITS = Celsius ]
// float mTyreTreadTemp[TYRE_MAX]; // [ UNITS = Kelvin ]
// float mTyreLayerTemp[TYRE_MAX]; // [ UNITS = Kelvin ]
// float mTyreCarcassTemp[TYRE_MAX]; // [ UNITS = Kelvin ]
// float mTyreRimTemp[TYRE_MAX]; // [ UNITS = Kelvin ]
// float mTyreInternalAirTemp[TYRE_MAX]; // [ UNITS = Kelvin ]ry>

// https://forum.projectcarsgame.com/showthread.php?61913-Making-a-buttonbox-input-wanted&p=1578991&viewfull=1
// https://forum.projectcarsgame.com/showthread.php?62537-Sim-Racing-Telemetry-is-available-for-download

// Reference pour le codage si dessous
// https://github.com/Maskmagog/projectcalc/blob/master/pc2udp/pc2udp/PCars2_UDP.cs



/// 
/// </summary>
/// 

namespace f1_eTelemetry
{
    public class PCars2_UDP : IComparable<PCars2_UDP>
    {
        // triMethode
        public int triMethode = 0;
        public Boolean triauto;
        public Boolean PC2RealTime;
        public Boolean RealTime;
        public Boolean FileSave;
        public Boolean View;

        const int UDP_STREAMER_CAR_PHYSICS_HANDLER_VERSION = 2;
        const int TYRE_NAME_LENGTH_MAX = 40;
        const int PARTICIPANT_NAME_LENGTH_MAX = 64;
        const int PARTICIPANTS_PER_PACKET = 16;
        const int UDP_STREAMER_PARTICIPANTS_SUPPORTED = 32;
        const int TRACKNAME_LENGTH_MAX = 64;
        const int VEHICLE_NAME_LENGTH_MAX = 64;
        const int CLASS_NAME_LENGTH_MAX = 20;
        const int VEHICLES_PER_PACKET = 16;
        const int CLASSES_SUPPORTED_PER_PACKET = 60;
        const int UDP_STREAMER_TIMINGS_HANDLER_VERSION = 1;
        const int UDP_STREAMER_TIME_STATS_HANDLER_VERSION = 1;
        const int UDP_STREAMER_PARTICIPANT_VEHICLE_NAMES_HANDLER_VERSION = 2;
        const int UDP_STREAMER_GAME_STATE_HANDLER_VERSION = 2;


        private UdpClient _listener;
        private IPEndPoint _groupEP;

        // Telemetry
        private UInt32 _PacketNumber;
        private UInt32 _CategoryPacketNumber;
        private byte _PartialPacketIndex;
        private byte _PartialPacketNumber;
        private byte _PacketType;
        private byte _PacketVersion;
        private sbyte _ViewedParticipantIndex;
        private byte _UnfilteredThrottle;
        private byte _UnfilteredBrake;
        private sbyte _UnfilteredSteering;
        private byte _UnfilteredClutch;
        private byte _CarFlags;
        private Int16 _OilTempCelsius;
        private UInt16 _OilPressureKPa;
        private Int16 _WaterTempCelsius;
        private UInt16 _WaterPressureKpa;
        private UInt16 _FuelPressureKpa;
        private byte _FuelCapacity;
        private byte _Brake;
        private byte _Throttle;
        private byte _Clutch;
        private float _FuelLevel;
        private float _Speed;
        private UInt16 _Rpm;
        private UInt16 _MaxRpm;
        private sbyte _Steering;
        private byte _GearNumGears;
        private byte _BoostAmount;
        private byte _CrashState;
        private float _OdometerKM;
        private float[] _Orientation = new float[3];
        private float[] _LocalVelocity = new float[3];
        private float[] _WorldVelocity = new float[3];
        private float[] _AngularVelocity = new float[3];
        private float[] _LocalAcceleration = new float[3];
        private float[] _WorldAcceleration = new float[3];
        private float[] _ExtentsCentre = new float[3];
        private byte[] _TyreFlags = new byte[4];
        private byte[] _Terrain = new byte[4];
        private float[] _TyreY = new float[4];
        private float[] _TyreRPS = new float[4];
        private byte[] _TyreTemp = new byte[4];
        private float[] _TyreHeightAboveGround = new float[4];
        private byte[] _TyreWear = new byte[4];
        private byte[] _BrakeDamage = new byte[4];
        private byte[] _SuspensionDamage = new byte[4];
        private Int16[] _BrakeTempCelsius = new Int16[4];
        private UInt16[] _TyreTreadTemp = new UInt16[4];
        private UInt16[] _TyreLayerTemp = new UInt16[4];
        private UInt16[] _TyreCarcassTemp = new UInt16[4];
        private UInt16[] _TyreRimTemp = new UInt16[4];
        private UInt16[] _TyreInternalAirTemp = new UInt16[4];
        private UInt16[] _TyreTempLeft = new UInt16[4];
        private UInt16[] _TyreTempCenter = new UInt16[4];
        private UInt16[] _TyreTempRight = new UInt16[4];
        private float[] _WheelLocalPositionY = new float[4];
        private float[] _RideHeight = new float[4];
        private float[] _SuspensionTravel = new float[4];
        private float[] _SuspensionVelocity = new float[4];
        private UInt16[] _SuspensionRideHeight = new UInt16[4];
        private UInt16[] _AirPressure = new UInt16[4];
        private float _EngineSpeed;
        private float _EngineTorque;
        private byte[] _Wings = new byte[2];

        // New Franck 1/11/2021
        private byte _HandBrake;
        private byte _AeroDamage;
        private byte _EngineDamage;
        private UInt32 _JoyPad0;                                    //unsigned int
        private byte _DPad;
        private byte[][] _TyreCompund = new byte[4][];                               // char sTyreCompound[4][TYRE_NAME_LENGTH_MAX];
        private float _TurboBoostPressure;
        // private float[] _FullPosition = new float[4];
        private float _FullPosition;
        private byte _BrakeBias;
        private UInt32 _TickCount;


        //RaceData New Franck 1/11/2021
        private float _WorldFastestLapTime;
        private float _PersonalFastestLapTime;
        private float _PersonalFastestSector1Time;
        private float _PersonalFastestSector2Time;
        private float _PersonalFastestSector3Time;
        private float _WorldFastestSector1Time;
        private float _WorldFastestSector2Time;
        private float _WorldFastestSector3Time;
        private float _TrackLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public string _TrackLocation;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public string _TrackVariation;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public string _TranslatedTrackLocation;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public string _TranslatedTrackVariation;
        private UInt16 _LapsTimeInEvent;
        private sbyte _EnforcedPitStopLap;

        // Participants Data New Franck 1/11/2021
        private UInt32 _ParticipantsChangedTimestamp2;
        public string _Name;

        //Game Data (Gamestate and sessionstate probably wrong/bad coding New Franck 1/11/2021
        private UInt16 _BuildVersionNumber;
        private Int32 _GameState;
        public string GameState2;
        public Int32 GameState3;
        private sbyte _AmbientTemperature;
        private sbyte _TrackTemperature;
        private double _RainDensity;
        private double _SnowDensity;
        private sbyte _WindSpeed;
        private sbyte _WindDirectionX;
        private sbyte _WindDirectionY;


        // Participant Stats Info New Franck 1/11/2021
        private double[,] _ParticipantStatsInfo = new double[32, 16];



        //Timing
        private sbyte _NumberParticipants;
        private UInt32 _ParticipantsChangedTimestamp;
        private float _EventTimeRemaining;
        private float _SplitTimeAhead;
        private float _SplitTimeBehind;
        private float _SplitTime;
        private double[,] _ParticipantInfo = new double[32, 16];


        // New Franck 1/11/2021
        public struct _VehicleInfo
        {
            public static ushort _Index; // 0 2
            public static uint _Class; // 2 6 
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
            public static string _Name; // 6 70
        }; // padded to 72

        [Serializable]
        public struct _ParticipantVehicleNamesData
        {
            // starts with packet base (0-12)
            public uint _PacketNumber;                      //0 counter reflecting all the packets that have been sent during the game run
            public uint _CategoryPacketNumber;              //4 counter of the packet groups belonging to the given category
            public byte _PartialPacketIndex;            //8 If the data from this class had to be sent in several packets, the index number
            public byte _PartialPacketNumber;           //9 If the data from this class had to be sent in several packets, the total number
            public byte _PacketType;                            //10 what is the type of this packet (see EUDPStreamerPacketHanlderType for details)
            public byte _PacketVersion;                     //11 what is the version of protocol for this handler, to be bumped with data structure change

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]
            public static _VehicleInfo[] sVehicles; //12 16*72
        };  // 1164

        public struct _ClassInfo
        {
            public static uint _Index; // 0 4 
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 20)]
            public static string _Name; // 4 24
        };

        [Serializable]
        public struct _VehicleClassNamesData
        {
            // starts with packet base (0-12)
            public uint _PacketNumber;                      //0 counter reflecting all the packets that have been sent during the game run
            public uint _CategoryPacketNumber;      //4 counter of the packet groups belonging to the given category
            public byte _PartialPacketIndex;            //8 If the data from this class had to be sent in several packets, the index number
            public byte _PartialPacketNumber;           //9 If the data from this class had to be sent in several packets, the total number
            public byte _PacketType;                            //10 what is the type of this packet (see EUDPStreamerPacketHanlderType for details)
            public byte _PacketVersion;                     //11 what is the version of protocol for this handler, to be bumped with data structure change

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 60)]
            public _ClassInfo[] _Classes; //12 24*60
        };                          // 1452


        public PCars2_UDP() // Constructeur
        {
            Console.WriteLine("UDP Constructeur vide");
        }

        public PCars2_UDP(Stream stream, BinaryReader binaryReader) // Constructeur avec ini
        {
            InitPackets(stream, binaryReader);
        }
        public PCars2_UDP(UdpClient listen, IPEndPoint group)
        {
            Console.WriteLine("Constructeur _listener _GroupeEP");

            _listener = listen;
            _groupEP = group;
        }


        //https://www.it-swarm-fr.com/fr/c%23/comment-trier-une-liste-t-par-une-propriete-de-lobjet/969537064/
        public int CompareToOdometerKM(PCars2_UDP that) // utilisé pour le tri
        {
            if (that == null) return 1;
            if (this.OdometerKM > that.OdometerKM) return 1;
            if (this.OdometerKM < that.OdometerKM) return -1;
            return 0;
        }
        public int CompareToPacketNumber(PCars2_UDP that) // utilisé pour le tri
        {
            if (that == null) return 1;
            if (this.PacketNumber > that.PacketNumber) return 1;
            if (this.PacketNumber < that.PacketNumber) return -1;
            return 0;
        }

        public int CompareTo(PCars2_UDP that) // utilisé pour le tri
        {
            switch (triMethode)
            {
                case 0:
                    return CompareToOdometerKM(that);
                //break;

                case 1:
                    return CompareToPacketNumber(that);
                //break;

                default:
                    return 0;
                    //break;
            }
        }
        public void InitPackets(Stream stream, BinaryReader binaryReader)
        {
            stream.Position = 0;

            PacketNumber = binaryReader.ReadUInt32();                   //0  unsigned int counter reflecting all the packets that have been sent during the game run
            CategoryPacketNumber = binaryReader.ReadUInt32();           //4  unsigned int counter of the packet groups belonging to the given category
            PartialPacketIndex = binaryReader.ReadByte();               //8  unsigned char If the data from this class had to be sent in several packets, the index number
            PartialPacketNumber = binaryReader.ReadByte();              //9  unsigned char If the data from this class had to be sent in several packets, the total number
            PacketType = binaryReader.ReadByte();                       //10 unsigned char what is the type of this packet (see EUDPStreamerPacketHanlderType for details)
            PacketVersion = binaryReader.ReadByte();                    //11 unsigned charwhat is the version of protocol for this handler, to be bumped with data structure change
        }

        //https://github.com/Maskmagog/projectcalc/blob/master/pc2udp/pc2udp/PCars2_UDP.cs
        public void ReadBaseUDP(Stream stream, BinaryReader binaryReader)
        {
            //Console.Write("ReadBadeUDP Méthode");

            stream.Position = 0;
            PacketNumber = binaryReader.ReadUInt32();
            CategoryPacketNumber = binaryReader.ReadUInt32();
            PartialPacketIndex = binaryReader.ReadByte();
            PartialPacketNumber = binaryReader.ReadByte();
            PacketType = binaryReader.ReadByte();
            PacketVersion = binaryReader.ReadByte();
        }

        public void readPackets()
        {
            //Console.WriteLine("readpacket Memorystream et BinaryStream");

            byte[] UDPpacket = listener.Receive(ref _groupEP);
            Stream stream = new MemoryStream(UDPpacket);
            var binaryReader = new BinaryReader(stream);

            readPackets(stream, binaryReader);
        }


        public void readPackets(Stream stream, BinaryReader binaryReader)
        {
            if (PacketType == 0) //10839
            {
                ReadTelemetryData(stream, binaryReader);
            }
            else if (PacketType == 1) //2
            {
                ReadRaceData(stream, binaryReader);
            }
            else if (PacketType == 2) //8
            {
                ReadParticipantsData(stream, binaryReader);
            }
            else if (PacketType == 3) //1100
            {
                ReadTimings(stream, binaryReader);
            }
            else if (PacketType == 4) //36
            {
                ReadGameData(stream, binaryReader);
            }
            else if (PacketType == 7) //145
            {
                ReadParticipantsStatsInfo(stream, binaryReader);
            }
            else if (PacketType == 8) //6
            {
                /*
                if (PartialPacketIndex < PartialPacketNumber)
                {
                    Console.WriteLine(
                        "\n 8 - " +
                        " PacketNumber : " + PacketNumber +
                        " PartialPacketIndex : " + PartialPacketIndex +
                        " PartialPacketNumber : " + PartialPacketNumber
                        );
                }
                */

                if (PartialPacketIndex == PartialPacketNumber)
                {
                    ReadVehicleClassNames(stream, binaryReader);
                }
                else
                {
                    /*
                    if (PartialPacketIndex < PartialPacketNumber)
                    {
                        Console.WriteLine(
                            "\n <" +
                            " PacketNumber : "+PacketNumber+
                            " PartialPacketIndex : "+ PartialPacketIndex+
                            " PartialPacketNumber : "+ PartialPacketNumber
                            );
                    }
                    else
                    {
                        Console.WriteLine(
                        "\n >" +
                        " PacketNumber : " + PacketNumber +
                        " PartialPacketIndex : " + PartialPacketIndex +
                        " PartialPacketNumber : " + PartialPacketNumber
                        );
                    }
                    */

                    if (PartialPacketIndex == 1)
                    {
                        ReadParticipantVehicleNamesData1(stream, binaryReader);
                    }
                    if (PartialPacketIndex == 2)
                    {
                        ReadParticipantVehicleNamesData2(stream, binaryReader);
                    }
                }
            }
        }

        public void ReadTelemetryData(Stream stream, BinaryReader binaryReader) //PacketType == 0
        {
            stream.Position = 12;

            ViewedParticipantIndex = binaryReader.ReadSByte();
            UnfilteredThrottle = binaryReader.ReadByte();
            UnfilteredBrake = binaryReader.ReadByte();
            UnfilteredSteering = binaryReader.ReadSByte();
            UnfilteredClutch = binaryReader.ReadByte();
            CarFlags = binaryReader.ReadByte();
            OilTempCelsius = binaryReader.ReadInt16();
            OilPressureKPa = binaryReader.ReadUInt16();
            WaterTempCelsius = binaryReader.ReadInt16();
            WaterPressureKpa = binaryReader.ReadUInt16();
            FuelPressureKpa = binaryReader.ReadUInt16();
            FuelCapacity = binaryReader.ReadByte();
            Brake = binaryReader.ReadByte();
            Throttle = binaryReader.ReadByte();
            Clutch = binaryReader.ReadByte();
            FuelLevel = binaryReader.ReadSingle();
            Speed = binaryReader.ReadSingle();
            Rpm = binaryReader.ReadUInt16();
            MaxRpm = binaryReader.ReadUInt16();
            Steering = binaryReader.ReadSByte();        // [ RANGE = 15 (Reverse)  0 (Neutral)  1 (Gear 1)  2 (Gear 2)  etc... ]
            GearNumGears = binaryReader.ReadByte();
            BoostAmount = binaryReader.ReadByte();
            CrashState = binaryReader.ReadByte();
            OdometerKM = binaryReader.ReadSingle();  // [ UNITS = Kilometers ] [ UNSET = -1.0f ]

            Orientation[0] = binaryReader.ReadSingle(); // [ UNITS = Euler Angles ]
            Orientation[1] = binaryReader.ReadSingle();
            Orientation[2] = binaryReader.ReadSingle();

            LocalVelocity[0] = binaryReader.ReadSingle();   // [ UNITS = Metres per-second ]
            LocalVelocity[1] = binaryReader.ReadSingle();
            LocalVelocity[2] = binaryReader.ReadSingle();

            WorldVelocity[0] = binaryReader.ReadSingle();   // [ UNITS = Metres per-second ]
            WorldVelocity[1] = binaryReader.ReadSingle();
            WorldVelocity[2] = binaryReader.ReadSingle();

            AngularVelocity[0] = binaryReader.ReadSingle(); // [ UNITS = Radians per-second ]
            AngularVelocity[1] = binaryReader.ReadSingle();
            AngularVelocity[2] = binaryReader.ReadSingle();

            LocalAcceleration[0] = binaryReader.ReadSingle();   // [ UNITS = Metres per-second ]
            LocalAcceleration[1] = binaryReader.ReadSingle();
            LocalAcceleration[2] = binaryReader.ReadSingle();

            WorldAcceleration[0] = binaryReader.ReadSingle();   // [ UNITS = Metres per-second ]
            WorldAcceleration[1] = binaryReader.ReadSingle();
            WorldAcceleration[2] = binaryReader.ReadSingle();

            ExtentsCentre[0] = binaryReader.ReadSingle();   // [ UNITS = Local Space  X  Y  Z ]
            ExtentsCentre[1] = binaryReader.ReadSingle();
            ExtentsCentre[2] = binaryReader.ReadSingle();

            TyreFlags[0] = binaryReader.ReadByte();
            TyreFlags[1] = binaryReader.ReadByte();
            TyreFlags[2] = binaryReader.ReadByte();
            TyreFlags[3] = binaryReader.ReadByte();

            Terrain[0] = binaryReader.ReadByte();
            Terrain[1] = binaryReader.ReadByte();
            Terrain[2] = binaryReader.ReadByte();
            Terrain[3] = binaryReader.ReadByte();

            TyreY[0] = binaryReader.ReadSingle();
            TyreY[1] = binaryReader.ReadSingle();
            TyreY[2] = binaryReader.ReadSingle();
            TyreY[3] = binaryReader.ReadSingle();

            TyreRPS[0] = binaryReader.ReadSingle(); // [ UNITS = Revolutions per second ]
            TyreRPS[1] = binaryReader.ReadSingle();
            TyreRPS[2] = binaryReader.ReadSingle();
            TyreRPS[3] = binaryReader.ReadSingle();

            //tyreSlipSpeed: Array[Float], // [ UNITS = Metres per-second ]

            TyreTemp[0] = binaryReader.ReadByte();  // [ UNITS = Celsius ]
            TyreTemp[1] = binaryReader.ReadByte();
            TyreTemp[2] = binaryReader.ReadByte();
            TyreTemp[3] = binaryReader.ReadByte();

            TyreHeightAboveGround[0] = binaryReader.ReadSingle();
            TyreHeightAboveGround[1] = binaryReader.ReadSingle();
            TyreHeightAboveGround[2] = binaryReader.ReadSingle();
            TyreHeightAboveGround[3] = binaryReader.ReadSingle();

            TyreWear[0] = binaryReader.ReadByte();
            TyreWear[1] = binaryReader.ReadByte();
            TyreWear[2] = binaryReader.ReadByte();
            TyreWear[3] = binaryReader.ReadByte();

            BrakeDamage[0] = binaryReader.ReadByte();
            BrakeDamage[1] = binaryReader.ReadByte();
            BrakeDamage[2] = binaryReader.ReadByte();
            BrakeDamage[3] = binaryReader.ReadByte();

            SuspensionDamage[0] = binaryReader.ReadByte();
            SuspensionDamage[1] = binaryReader.ReadByte();
            SuspensionDamage[2] = binaryReader.ReadByte();
            SuspensionDamage[3] = binaryReader.ReadByte();

            BrakeTempCelsius[0] = binaryReader.ReadInt16();
            BrakeTempCelsius[1] = binaryReader.ReadInt16();
            BrakeTempCelsius[2] = binaryReader.ReadInt16();
            BrakeTempCelsius[3] = binaryReader.ReadInt16();

            TyreTreadTemp[0] = binaryReader.ReadUInt16();
            TyreTreadTemp[1] = binaryReader.ReadUInt16();
            TyreTreadTemp[2] = binaryReader.ReadUInt16();
            TyreTreadTemp[3] = binaryReader.ReadUInt16();

            TyreLayerTemp[0] = binaryReader.ReadUInt16();
            TyreLayerTemp[1] = binaryReader.ReadUInt16();
            TyreLayerTemp[2] = binaryReader.ReadUInt16();
            TyreLayerTemp[3] = binaryReader.ReadUInt16();

            TyreCarcassTemp[0] = binaryReader.ReadUInt16();
            TyreCarcassTemp[1] = binaryReader.ReadUInt16();
            TyreCarcassTemp[2] = binaryReader.ReadUInt16();
            TyreCarcassTemp[3] = binaryReader.ReadUInt16();

            TyreRimTemp[0] = binaryReader.ReadUInt16();
            TyreRimTemp[1] = binaryReader.ReadUInt16();
            TyreRimTemp[2] = binaryReader.ReadUInt16();
            TyreRimTemp[3] = binaryReader.ReadUInt16();

            TyreInternalAirTemp[0] = binaryReader.ReadUInt16();
            TyreInternalAirTemp[1] = binaryReader.ReadUInt16();
            TyreInternalAirTemp[2] = binaryReader.ReadUInt16();
            TyreInternalAirTemp[3] = binaryReader.ReadUInt16();

            TyreTempLeft[0] = binaryReader.ReadUInt16();
            TyreTempLeft[1] = binaryReader.ReadUInt16();
            TyreTempLeft[2] = binaryReader.ReadUInt16();
            TyreTempLeft[3] = binaryReader.ReadUInt16();

            TyreTempCenter[0] = binaryReader.ReadUInt16();
            TyreTempCenter[1] = binaryReader.ReadUInt16();
            TyreTempCenter[2] = binaryReader.ReadUInt16();
            TyreTempCenter[3] = binaryReader.ReadUInt16();

            TyreTempRight[0] = binaryReader.ReadUInt16();
            TyreTempRight[1] = binaryReader.ReadUInt16();
            TyreTempRight[2] = binaryReader.ReadUInt16();
            TyreTempRight[3] = binaryReader.ReadUInt16();

            WheelLocalPositionY[0] = binaryReader.ReadSingle();
            WheelLocalPositionY[1] = binaryReader.ReadSingle();
            WheelLocalPositionY[2] = binaryReader.ReadSingle();
            WheelLocalPositionY[3] = binaryReader.ReadSingle();

            RideHeight[0] = binaryReader.ReadSingle();
            RideHeight[1] = binaryReader.ReadSingle();
            RideHeight[2] = binaryReader.ReadSingle();
            RideHeight[3] = binaryReader.ReadSingle();

            SuspensionTravel[0] = binaryReader.ReadSingle();
            SuspensionTravel[1] = binaryReader.ReadSingle();
            SuspensionTravel[2] = binaryReader.ReadSingle();
            SuspensionTravel[3] = binaryReader.ReadSingle();

            SuspensionVelocity[0] = binaryReader.ReadSingle();
            SuspensionVelocity[1] = binaryReader.ReadSingle();
            SuspensionVelocity[2] = binaryReader.ReadSingle();
            SuspensionVelocity[3] = binaryReader.ReadSingle();

            SuspensionRideHeight[0] = binaryReader.ReadUInt16();
            SuspensionRideHeight[1] = binaryReader.ReadUInt16();
            SuspensionRideHeight[2] = binaryReader.ReadUInt16();
            SuspensionRideHeight[3] = binaryReader.ReadUInt16();

            AirPressure[0] = binaryReader.ReadUInt16();
            AirPressure[1] = binaryReader.ReadUInt16();
            AirPressure[2] = binaryReader.ReadUInt16();
            AirPressure[3] = binaryReader.ReadUInt16();

            EngineSpeed = binaryReader.ReadSingle();
            EngineTorque = binaryReader.ReadSingle();

            Wings[0] = binaryReader.ReadByte();
            Wings[1] = binaryReader.ReadByte();

            // New Franck 1 Nov 2021
            HandBrake = binaryReader.ReadByte();
            AeroDamage = binaryReader.ReadByte();
            EngineDamage = binaryReader.ReadByte();
            JoyPad0 = binaryReader.ReadUInt32();                                    //unsigned int

            //if (JoyPad0 > 0)
            //    for (int i = 0; i<1; i++) ;
            DPad = binaryReader.ReadByte();
            TyreCompund[0] = binaryReader.ReadBytes(TYRE_NAME_LENGTH_MAX);                               // char sTyreCompound[4][TYRE_NAME_LENGTH_MAX];
            TyreCompund[1] = binaryReader.ReadBytes(TYRE_NAME_LENGTH_MAX);                               // char sTyreCompound[4][TYRE_NAME_LENGTH_MAX];
            TyreCompund[2] = binaryReader.ReadBytes(TYRE_NAME_LENGTH_MAX);                               // char sTyreCompound[4][TYRE_NAME_LENGTH_MAX];
            TyreCompund[3] = binaryReader.ReadBytes(TYRE_NAME_LENGTH_MAX);                               // char sTyreCompound[4][TYRE_NAME_LENGTH_MAX];
            TurboBoostPressure = binaryReader.ReadSingle();
            //FullPosition[0] = binaryReader.ReadSingle();
            //FullPosition[1] = binaryReader.ReadSingle();
            //FullPosition[2] = binaryReader.ReadSingle();
            //FullPosition[3] = binaryReader.ReadSingle();
            FullPosition = binaryReader.ReadSingle();
            BrakeBias = binaryReader.ReadByte();
            TickCount = binaryReader.ReadUInt32();
        }

        public void ReadTimings(Stream stream, BinaryReader binaryReader)  //PacketType == 3
        {
            // https://forum.projectcarsgame.com/showthread.php?63374-C-Udp-V2-library/
            //  the PacketBase contains 3 important values. mPartialPacketIndex, mPartialPacketNumber, mPacketType.
            //  Without those you can't decipher the udp packets correctly.
            //  Unless your library does that for the user, of course.
            // Gardez simplement à l'esprit que l'indice n'est pas égal à la position de course. cependant,
            // si vous voulez juste les données pour vous-même (disons votre "Current Sector Time" pour votre voiture,
            // votre objet appelé ressemblera à ceci . MyCurrentSectorTime = uDP.ParticipantInfo[uDP.ViewedPlayerIndex,15].
            // https://forum.projectcarsgame.com/showthread.php?61913-Making-a-buttonbox-input-wanted/page6

            //Gardez simplement à l'esprit que l'indice n'est pas égal à la position de course.
            //cependant, si vous voulez juste les données pour vous-même
            //(disons votre "Current Sector Time" pour votre voiture, votre objet appelé ressemblera à ceci .
            //MyCurrentSectorTime = uDP.ParticipantInfo[uDP. ViewedPlayerIndex ,15].

            stream.Position = 12;
            NumberParticipants = binaryReader.ReadSByte();
            ParticipantsChangedTimestamp = binaryReader.ReadUInt32(); //Les participants ont changé l'horodatage


            EventTimeRemaining = binaryReader.ReadSingle(); //Temps d'événement restant
            SplitTimeAhead = binaryReader.ReadSingle(); //Temps intermédiaire à venir
            SplitTimeBehind = binaryReader.ReadSingle(); //Temps intermédiaire en retard
            SplitTime = binaryReader.ReadSingle(); //Temps intermédiaire 

            for (int i = 0; i < 32; i++)
            {
                ParticipantInfo[i, 0] = Convert.ToDouble(binaryReader.ReadInt16());  //WorldPosition 
                ParticipantInfo[i, 1] = Convert.ToDouble(binaryReader.ReadInt16());  //WorldPosition
                ParticipantInfo[i, 2] = Convert.ToDouble(binaryReader.ReadInt16());  //WorldPosition
                ParticipantInfo[i, 3] = Convert.ToDouble(binaryReader.ReadInt16());  //Orientation
                ParticipantInfo[i, 4] = Convert.ToDouble(binaryReader.ReadInt16()); //Orientation 
                ParticipantInfo[i, 5] = Convert.ToDouble(binaryReader.ReadInt16());  //Orientation
                ParticipantInfo[i, 6] = Convert.ToDouble(binaryReader.ReadUInt16());  //sCurrentLapDistance
                ParticipantInfo[i, 7] = Convert.ToDouble(binaryReader.ReadByte()) - 128;  //sRacePosition
                byte Sector_ALL = binaryReader.ReadByte();
                var Sector_Extracted = Sector_ALL & 0x0F;
                ParticipantInfo[i, 8] = Convert.ToDouble(Sector_Extracted + 1);   //sSector
                ParticipantInfo[i, 9] = Convert.ToDouble(binaryReader.ReadByte());  //sHighestFlag
                ParticipantInfo[i, 10] = Convert.ToDouble(binaryReader.ReadByte()); //sPitModeSchedule
                ParticipantInfo[i, 11] = Convert.ToDouble(binaryReader.ReadUInt16());//sCarIndex
                ParticipantInfo[i, 12] = Convert.ToDouble(binaryReader.ReadByte()); //sRaceState
                ParticipantInfo[i, 13] = Convert.ToDouble(binaryReader.ReadByte()); //sCurrentLap
                ParticipantInfo[i, 14] = Convert.ToDouble(binaryReader.ReadSingle()); //sCurrentTime
                ParticipantInfo[i, 15] = Convert.ToDouble(binaryReader.ReadSingle());  //sCurrentSectorTime
                // manque ParticipantIndex
            }
        }

        // New Franck 1/11/2021
        public void ReadRaceData(Stream stream, BinaryReader binaryReader) // PacketType == 1
        {
            stream.Position = 12;

            WorldFastestLapTime = binaryReader.ReadSingle();
            PersonalFastestLapTime = binaryReader.ReadSingle();
            PersonalFastestSector1Time = binaryReader.ReadSingle();
            PersonalFastestSector2Time = binaryReader.ReadSingle();
            PersonalFastestSector3Time = binaryReader.ReadSingle();
            WorldFastestSector1Time = binaryReader.ReadSingle();
            WorldFastestSector2Time = binaryReader.ReadSingle();
            WorldFastestSector3Time = binaryReader.ReadSingle();
            TrackLength = binaryReader.ReadSingle();
            byte[] str = binaryReader.ReadBytes(64);
            int lengthOfStr = Array.IndexOf(str, (byte)0); // e.g. 4 for "clip\0"
            TrackLocation = System.Text.ASCIIEncoding.Default.GetString(str, 0, lengthOfStr);
            byte[] str2 = binaryReader.ReadBytes(64);
            int lengthOfStr2 = Array.IndexOf(str2, (byte)0); // e.g. 4 for "clip\0"
            TrackVariation = System.Text.ASCIIEncoding.Default.GetString(str2, 0, lengthOfStr2);

            byte[] str3 = binaryReader.ReadBytes(64);
            int lengthOfStr3 = Array.IndexOf(str3, (byte)0); // e.g. 4 for "clip\0"
            TranslatedTrackLocation = System.Text.ASCIIEncoding.Default.GetString(str3, 0, lengthOfStr3);
            byte[] str4 = binaryReader.ReadBytes(64);
            int lengthOfStr4 = Array.IndexOf(str4, (byte)0); // e.g. 4 for "clip\0"
            TranslatedTrackVariation = System.Text.ASCIIEncoding.Default.GetString(str4, 0, lengthOfStr4);

            LapsTimeInEvent = binaryReader.ReadUInt16();
            EnforcedPitStopLap = binaryReader.ReadSByte();
        }

        public void ReadParticipantsData(Stream stream, BinaryReader binaryReader) // PacketType == 2
        {
            stream.Position = 12;
            ParticipantsChangedTimestamp = binaryReader.ReadUInt32();
            byte[] strName = binaryReader.ReadBytes(64);
            int lengthOfStrName = Array.IndexOf(strName, (byte)0); // e.g. 4 for "clip\0"
            //Name = System.Text.ASCIIEncoding.Default.GetString(strName, 0, lengthOfStrName);
            //Console.WriteLine("Name : " + Name);
        }

        public void ReadGameData(Stream stream, BinaryReader binaryReader) //PacketType == 4
        {
            stream.Position = 12;

            BuildVersionNumber = binaryReader.ReadUInt16();
            GameState = binaryReader.ReadByte();
            GameState2 = Convert.ToString(GameState, 2).PadLeft(8, '0');
            GameState3 = Convert.ToInt32(GameState2);
            AmbientTemperature = binaryReader.ReadSByte();
            TrackTemperature = binaryReader.ReadSByte();
            RainDensity = Convert.ToDouble(binaryReader.ReadByte());
            SnowDensity = Convert.ToDouble(binaryReader.ReadByte());
            WindSpeed = binaryReader.ReadSByte();
            WindDirectionX = binaryReader.ReadSByte();
            WindDirectionY = binaryReader.ReadSByte();
        }

        public void ReadParticipantsStatsInfo(Stream stream, BinaryReader binaryReader) // PacketType == 7
        {
            uint sParticipantOnlineRep;                         // 24 (unsigned short rank type + unsigned short strength, 0 in SP races)
            ushort sMPParticipantIndex;							// 28 -- matching sIndex from sParticipantsData

            stream.Position = 12;

            //Console.WriteLine("ReadParticipantsStatsInfo");

            for (int i = 0; i < 18; i++) // 32 au lieu de18
            {
                ParticipantStatsInfo[i, 1] = Convert.ToDouble(binaryReader.ReadSingle());  //? 0 
                ParticipantStatsInfo[i, 2] = Convert.ToDouble(binaryReader.ReadSingle());  //FastestLap
                ParticipantStatsInfo[i, 3] = Convert.ToDouble(binaryReader.ReadSingle());  //LastLap
                ParticipantStatsInfo[i, 4] = Convert.ToDouble(binaryReader.ReadSingle());  //LastsectorTime [ UNITS = seconds ]
                ParticipantStatsInfo[i, 5] = Convert.ToDouble(binaryReader.ReadSingle());  //FastestSector1
                ParticipantStatsInfo[i, 6] = Convert.ToDouble(binaryReader.ReadSingle());  //FastestSector2
                ParticipantStatsInfo[i, 7] = Convert.ToDouble(binaryReader.ReadSingle());  //FastestSector3
                // New 14 Fev 2022
                // https://github.com/lmirel/mfc/blob/master/clients/pcars/SMS_UDP_Definitions_v2p5.hpp
                //ParticipantStatsInfo[i, 8] = Convert.ToDouble(binaryReader.ReadUInt16());
                //ParticipantStatsInfo[i, 9] = Convert.ToDouble(binaryReader.ReadUInt32());
            }
        }

        public void ReadVehicleClassNames(Stream stream, BinaryReader binaryReader) // PacketType == 8
        {
            stream.Position = 12;

            ClassIndex = binaryReader.ReadUInt32();
            byte[] arrayClassName = binaryReader.ReadBytes(64);
            int lengthOfarrayClassName = Array.IndexOf(arrayClassName, (byte)0); // e.g. 4 for "clip\0"
            ClassName = System.Text.UTF8Encoding.Default.GetString(arrayClassName, 0, lengthOfarrayClassName);
            //Console.WriteLine("ClassName from PCars2_UDP.cs" + ClassName);
            //Console.WriteLine("ClassIndex from PCars2_UDP.cs" + ClassIndex);
        }

        public void ReadParticipantVehicleNamesData1(Stream stream, BinaryReader binaryReader) //PacketType == 8 and PartialPacketIndex == 1
        {
            //Console.WriteLine("PartVehNamData 1 received");
            stream.Position = 12;

            VehicleIndex = binaryReader.ReadUInt16();
            VehicleClass = binaryReader.ReadUInt32();
            byte[] arrayVehicleName = binaryReader.ReadBytes(64);
            int lengthOfarrayVehicleName = Array.IndexOf(arrayVehicleName, (byte)0); // e.g. 4 for "clip\0"
            VehicleName = System.Text.UTF8Encoding.Default.GetString(arrayVehicleName, 0, lengthOfarrayVehicleName);
            //Console.WriteLine("VehicleName from PCars2_UDP.cs" + VehicleName);
        }

        public void ReadParticipantVehicleNamesData2(Stream stream, BinaryReader binaryReader) //PacketType == 8 and PartialPacketIndex == 2
        {
            //Console.WriteLine("PartVehNamData 2 received");
            stream.Position = 12;

            VehicleIndex = binaryReader.ReadUInt16();
            VehicleClass = binaryReader.ReadUInt32();
            byte[] arrayVehicleName = binaryReader.ReadBytes(64);
            int lengthOfarrayVehicleName = Array.IndexOf(arrayVehicleName, (byte)0); // e.g. 4 for "clip\0"
            VehicleName = System.Text.UTF8Encoding.Default.GetString(arrayVehicleName, 0, lengthOfarrayVehicleName);
            //Console.WriteLine("VehicleNameData2 " + VehicleName);
        }

        public void close_UDP_Connection()
        {
            listener.Close();
        }

        public UdpClient listener
        {
            get
            {
                return _listener;
            }
            set
            {
                _listener = value;
            }
        }

        public IPEndPoint groupEP
        {
            get
            {
                return _groupEP;
            }
            set
            {
                _groupEP = value;
            }
        }

        public UInt32 PacketNumber
        {
            get
            {
                return _PacketNumber;
            }
            set
            {
                _PacketNumber = value;
            }
        }

        public UInt32 CategoryPacketNumber
        {
            get
            {
                return _CategoryPacketNumber;
            }
            set
            {
                _CategoryPacketNumber = value;
            }
        }

        public byte PartialPacketIndex
        {
            get
            {
                return _PartialPacketIndex;
            }
            set
            {
                _PartialPacketIndex = value;
            }
        }

        public byte PartialPacketNumber
        {
            get
            {
                return _PartialPacketNumber;
            }
            set
            {
                _PartialPacketNumber = value;
            }
        }

        public byte PacketType
        {
            get
            {
                return _PacketType;
            }
            set
            {
                _PacketType = value;
            }
        }

        public byte PacketVersion
        {
            get
            {
                return _PacketVersion;
            }
            set
            {
                _PacketVersion = value;
            }
        }

        public sbyte ViewedParticipantIndex
        {
            get
            {
                return _ViewedParticipantIndex;
            }
            set
            {
                _ViewedParticipantIndex = value;
            }
        }

        public byte UnfilteredThrottle
        {
            get
            {
                return _UnfilteredThrottle;
            }
            set
            {
                _UnfilteredThrottle = value;
            }
        }

        public byte UnfilteredBrake
        {
            get
            {
                return _UnfilteredBrake;
            }
            set
            {
                _UnfilteredBrake = value;
            }
        }

        public sbyte UnfilteredSteering
        {
            get
            {
                return _UnfilteredSteering;
            }
            set
            {
                _UnfilteredSteering = value;
            }
        }

        public byte UnfilteredClutch
        {
            get
            {
                return _UnfilteredClutch;
            }
            set
            {
                _UnfilteredClutch = value;
            }
        }

        public byte CarFlags
        {
            get
            {
                return _CarFlags;
            }
            set
            {
                _CarFlags = value;
            }
        }

        public Int16 OilTempCelsius
        {
            get
            {
                return _OilTempCelsius;
            }
            set
            {
                _OilTempCelsius = value;
            }
        }

        public UInt16 OilPressureKPa
        {
            get
            {
                return _OilPressureKPa;
            }
            set
            {
                _OilPressureKPa = value;
            }
        }

        public Int16 WaterTempCelsius
        {
            get
            {
                return _WaterTempCelsius;
            }
            set
            {
                _WaterTempCelsius = value;
            }
        }

        public UInt16 WaterPressureKpa
        {
            get
            {
                return _WaterPressureKpa;
            }
            set
            {
                _WaterPressureKpa = value;
            }
        }

        public UInt16 FuelPressureKpa
        {
            get
            {
                return _FuelPressureKpa;
            }
            set
            {
                _FuelPressureKpa = value;
            }
        }

        public byte FuelCapacity
        {
            get
            {
                return _FuelCapacity;
            }
            set
            {
                _FuelCapacity = value;
            }
        }

        public byte Brake
        {
            get
            {
                return _Brake;
            }
            set
            {
                _Brake = value;
            }
        }

        public byte Throttle
        {
            get
            {
                return _Throttle;
            }
            set
            {
                _Throttle = value;
            }
        }

        public byte Clutch
        {
            get
            {
                return _Clutch;
            }
            set
            {
                _Clutch = value;
            }
        }

        public float FuelLevel
        {
            get
            {
                return _FuelLevel;
            }
            set
            {
                _FuelLevel = value;
            }
        }

        public float Speed
        {
            get
            {
                return _Speed;
            }
            set
            {
                _Speed = value;
            }
        }

        public UInt16 Rpm
        {
            get
            {
                return _Rpm;
            }
            set
            {
                _Rpm = value;
            }
        }

        public UInt16 MaxRpm
        {
            get
            {
                return _MaxRpm;
            }
            set
            {
                _MaxRpm = value;
            }
        }

        public sbyte Steering
        {
            get
            {
                return _Steering;
            }
            set
            {
                _Steering = value;
            }
        }

        public byte GearNumGears
        {
            get
            {
                return _GearNumGears;
            }
            set
            {
                _GearNumGears = value;
            }
        }

        public byte BoostAmount
        {
            get
            {
                return _BoostAmount;
            }
            set
            {
                _BoostAmount = value;
            }
        }

        public byte CrashState
        {
            get
            {
                return _CrashState;
            }
            set
            {
                _CrashState = value;
            }
        }

        public float OdometerKM
        {
            get
            {
                return _OdometerKM;
            }
            set
            {
                _OdometerKM = value;
            }
        }

        public float[] Orientation
        {
            get
            {
                return _Orientation;
            }
            set
            {
                _Orientation = value;
            }
        }

        public float[] LocalVelocity
        {
            get
            {
                return _LocalVelocity;
            }
            set
            {
                _LocalVelocity = value;
            }
        }

        public float[] WorldVelocity
        {
            get
            {
                return _WorldVelocity;
            }
            set
            {
                _WorldVelocity = value;
            }
        }

        public float[] AngularVelocity
        {
            get
            {
                return _AngularVelocity;
            }
            set
            {
                _AngularVelocity = value;
            }
        }

        public float[] LocalAcceleration
        {
            get
            {
                return _LocalAcceleration;
            }
            set
            {
                _LocalAcceleration = value;
            }
        }

        public float[] WorldAcceleration
        {
            get
            {
                return _WorldAcceleration;
            }
            set
            {
                _WorldAcceleration = value;
            }
        }

        public float[] ExtentsCentre
        {
            get
            {
                return _ExtentsCentre;
            }
            set
            {
                _ExtentsCentre = value;
            }
        }

        public byte[] TyreFlags
        {
            get
            {
                return _TyreFlags;
            }
            set
            {
                _TyreFlags = value;
            }
        }

        public byte[] Terrain
        {
            get
            {
                return _Terrain;
            }
            set
            {
                _Terrain = value;
            }
        }

        public float[] TyreY
        {
            get
            {
                return _TyreY;
            }
            set
            {
                _TyreY = value;
            }
        }

        public float[] TyreRPS
        {
            get
            {
                return _TyreRPS;
            }
            set
            {
                _TyreRPS = value;
            }
        }

        public byte[] TyreTemp
        {
            get
            {
                return _TyreTemp;
            }
            set
            {
                _TyreTemp = value;
            }
        }

        public float[] TyreHeightAboveGround
        {
            get
            {
                return _TyreHeightAboveGround;
            }
            set
            {
                _TyreHeightAboveGround = value;
            }
        }

        public byte[] TyreWear
        {
            get
            {
                return _TyreWear;
            }
            set
            {
                _TyreWear = value;
            }
        }

        public byte[] BrakeDamage
        {
            get
            {
                return _BrakeDamage;
            }
            set
            {
                _BrakeDamage = value;
            }
        }

        public byte[] SuspensionDamage
        {
            get
            {
                return _SuspensionDamage;
            }
            set
            {
                _SuspensionDamage = value;
            }
        }

        public Int16[] BrakeTempCelsius
        {
            get
            {
                return _BrakeTempCelsius;
            }
            set
            {
                _BrakeTempCelsius = value;
            }
        }

        public UInt16[] TyreTreadTemp
        {
            get
            {
                return _TyreTreadTemp;
            }
            set
            {
                _TyreTreadTemp = value;
            }
        }

        public UInt16[] TyreLayerTemp
        {
            get
            {
                return _TyreLayerTemp;
            }
            set
            {
                _TyreLayerTemp = value;
            }
        }

        public UInt16[] TyreCarcassTemp
        {
            get
            {
                return _TyreCarcassTemp;
            }
            set
            {
                _TyreCarcassTemp = value;
            }
        }

        public UInt16[] TyreRimTemp
        {
            get
            {
                return _TyreRimTemp;
            }
            set
            {
                _TyreRimTemp = value;
            }
        }

        public UInt16[] TyreInternalAirTemp
        {
            get
            {
                return _TyreInternalAirTemp;
            }
            set
            {
                _TyreInternalAirTemp = value;
            }
        }

        public UInt16[] TyreTempLeft
        {
            get
            {
                return _TyreTempLeft;
            }
            set
            {
                _TyreTempLeft = value;
            }
        }

        public UInt16[] TyreTempCenter
        {
            get
            {
                return _TyreTempCenter;
            }
            set
            {
                _TyreTempCenter = value;
            }
        }

        public UInt16[] TyreTempRight
        {
            get
            {
                return _TyreTempRight;
            }
            set
            {
                _TyreTempRight = value;
            }
        }

        public float[] WheelLocalPositionY
        {
            get
            {
                return _WheelLocalPositionY;
            }
            set
            {
                _WheelLocalPositionY = value;
            }
        }

        public float[] RideHeight
        {
            get
            {
                return _RideHeight;
            }
            set
            {
                _RideHeight = value;
            }
        }

        public float[] SuspensionTravel
        {
            get
            {
                return _SuspensionTravel;
            }
            set
            {
                _SuspensionTravel = value;
            }
        }

        public float[] SuspensionVelocity
        {
            get
            {
                return _SuspensionVelocity;
            }
            set
            {
                _SuspensionVelocity = value;
            }
        }

        public UInt16[] SuspensionRideHeight
        {
            get
            {
                return _SuspensionRideHeight;
            }
            set
            {
                _SuspensionRideHeight = value;
            }
        }

        public UInt16[] AirPressure
        {
            get
            {
                return _AirPressure;
            }
            set
            {
                _AirPressure = value;
            }
        }

        public float EngineSpeed
        {
            get
            {
                return _EngineSpeed;
            }
            set
            {
                _EngineSpeed = value;
            }
        }

        public float EngineTorque
        {
            get
            {
                return _EngineTorque;
            }
            set
            {
                _EngineTorque = value;
            }
        }

        public byte[] Wings
        {
            get
            {
                return _Wings;
            }
            set
            {
                _Wings = value;
            }
        }

        // New Franck 1/11/2021
        public byte HandBrake
        {
            get
            {
                return _HandBrake;
            }
            set
            {
                _HandBrake = value;
            }
        }
        public byte AeroDamage
        {
            get
            {
                return _AeroDamage;
            }
            set
            {
                _AeroDamage = value;
            }
        }
        public byte EngineDamage
        {
            get
            {
                return _EngineDamage;
            }
            set
            {
                _EngineDamage = value;
            }
        }
        public UInt32 JoyPad0                                    //unsigned int 
        {
            get
            {
                return _JoyPad0;
            }
            set
            {
                _JoyPad0 = value;
            }
        }
        public byte DPad
        {
            get
            {
                return _DPad;
            }
            set
            {
                _DPad = value;
            }
        }
        public byte[][] TyreCompund                               // char sTyreCompound[4][TYRE_NAME_LENGTH_MAX];
        {
            get
            {
                return _TyreCompund;
            }
            set
            {
                _TyreCompund = value;
            }
        }
        public float TurboBoostPressure
        {
            get
            {
                return _TurboBoostPressure;
            }
            set
            {
                _TurboBoostPressure = value;
            }
        }
        public float FullPosition
        {
            get
            {
                return _FullPosition;
            }
            set
            {
                _FullPosition = value;
            }
        }

        public byte BrakeBias
        {
            get
            {
                return _BrakeBias;
            }
            set
            {
                _BrakeBias = value;
            }

        }

        public UInt32 TickCount
        {
            get
            {
                return _TickCount;
            }
            set
            {
                _TickCount = value;
            }
        }




        public sbyte NumberParticipants
        {
            get
            {
                return _NumberParticipants;
            }
            set
            {
                _NumberParticipants = value;
            }
        }

        public UInt32 ParticipantsChangedTimestamp
        {
            get
            {
                return _ParticipantsChangedTimestamp;
            }
            set
            {
                _ParticipantsChangedTimestamp = value;
            }
        }

        public float EventTimeRemaining
        {
            get
            {
                return _EventTimeRemaining;
            }
            set
            {
                _EventTimeRemaining = value;
            }
        }

        public float SplitTimeAhead
        {
            get
            {
                return _SplitTimeAhead;
            }
            set
            {
                _SplitTimeAhead = value;
            }
        }

        public float SplitTimeBehind
        {
            get
            {
                return _SplitTimeBehind;
            }
            set
            {
                _SplitTimeBehind = value;
            }
        }

        public float SplitTime
        {
            get
            {
                return _SplitTime;
            }
            set
            {
                _SplitTime = value;
            }
        }

        public double[,] ParticipantInfo
        {
            get
            {
                return _ParticipantInfo;
            }
            set
            {
                _ParticipantInfo = value;
            }
        }
        // RaceData New Franck 1/11/2021
        public float WorldFastestLapTime
        {
            get
            {
                return _WorldFastestLapTime;
            }
            set
            {
                _WorldFastestLapTime = value;
            }
        }

        public float PersonalFastestLapTime
        {
            get
            {
                return _PersonalFastestLapTime;
            }
            set
            {
                _PersonalFastestLapTime = value;
            }
        }

        public float PersonalFastestSector1Time
        {
            get
            {
                return _PersonalFastestSector1Time;
            }
            set
            {
                _PersonalFastestSector1Time = value;
            }
        }

        public float PersonalFastestSector2Time
        {
            get
            {
                return _PersonalFastestSector2Time;
            }
            set
            {
                _PersonalFastestSector2Time = value;
            }
        }

        public float PersonalFastestSector3Time
        {
            get
            {
                return _PersonalFastestSector3Time;
            }
            set
            {
                _PersonalFastestSector3Time = value;
            }
        }

        public float WorldFastestSector1Time
        {
            get
            {
                return _WorldFastestSector1Time;
            }
            set
            {
                _WorldFastestSector1Time = value;
            }
        }

        public float WorldFastestSector2Time
        {
            get
            {
                return _WorldFastestSector2Time;
            }
            set
            {
                _WorldFastestSector2Time = value;
            }
        }

        public float WorldFastestSector3Time
        {
            get
            {
                return _WorldFastestSector3Time;
            }
            set
            {
                _WorldFastestSector3Time = value;
            }
        }

        public float TrackLength
        {
            get
            {
                return _TrackLength;
            }
            set
            {
                _TrackLength = value;
            }
        }

        public string TrackLocation
        {
            get
            {
                return _TrackLocation;
            }
            set
            {
                _TrackLocation = value;
            }
        }

        public string TrackVariation
        {
            get
            {
                return _TrackVariation;
            }
            set
            {
                _TrackVariation = value;
            }
        }

        public string TranslatedTrackLocation
        {
            get
            {
                return _TranslatedTrackLocation;
            }
            set
            {
                _TranslatedTrackLocation = value;
            }
        }

        public string TranslatedTrackVariation
        {
            get
            {
                return _TranslatedTrackVariation;
            }
            set
            {
                _TranslatedTrackVariation = value;
            }
        }

        public UInt16 LapsTimeInEvent
        {
            get
            {
                return _LapsTimeInEvent;
            }
            set
            {
                _LapsTimeInEvent = value;
            }
        }

        public sbyte EnforcedPitStopLap
        {
            get
            {
                return _EnforcedPitStopLap;
            }
            set
            {
                _EnforcedPitStopLap = value;
            }
        }


        // Participants Data

        public UInt32 ParticipantsChangedTimestamp2
        {
            get
            {
                return _ParticipantsChangedTimestamp2;
            }
            set
            {
                _ParticipantsChangedTimestamp2 = value;
            }
        }

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }
        // GameState
        public UInt16 BuildVersionNumber
        {
            get
            {
                return _BuildVersionNumber;
            }
            set
            {
                _BuildVersionNumber = value;
            }
        }

        public int GameState
        {
            get
            {
                return _GameState;
            }
            set
            {
                _GameState = value;
            }
        }

        public sbyte AmbientTemperature
        {
            get
            {
                return _AmbientTemperature;
            }
            set
            {
                _AmbientTemperature = value;
            }
        }

        public sbyte TrackTemperature
        {
            get
            {
                return _TrackTemperature;
            }
            set
            {
                _TrackTemperature = value;
            }
        }

        public double RainDensity
        {
            get
            {
                return _RainDensity;
            }
            set
            {
                _RainDensity = value;
            }
        }

        public double SnowDensity
        {
            get
            {
                return _SnowDensity;
            }
            set
            {
                _SnowDensity = value;
            }
        }

        public sbyte WindSpeed
        {
            get
            {
                return _WindSpeed;
            }
            set
            {
                _WindSpeed = value;
            }
        }

        public sbyte WindDirectionX
        {
            get
            {
                return _WindDirectionX;
            }
            set
            {
                _WindDirectionX = value;
            }
        }

        public sbyte WindDirectionY
        {
            get
            {
                return _WindDirectionY;
            }
            set
            {
                _WindDirectionY = value;
            }
        }

        // Participanst stats info
        public double[,] ParticipantStatsInfo
        {
            get
            {
                return _ParticipantStatsInfo;
            }
            set
            {
                _ParticipantStatsInfo = value;
            }
        }

        // VehicleInfo

        public UInt16 VehicleIndex
        {
            get
            {
                return _VehicleInfo._Index;
            }
            set
            {
                _VehicleInfo._Index = value;
            }
        }
        public UInt32 VehicleClass
        {
            get
            {
                return _VehicleInfo._Class;
            }
            set
            {
                _VehicleInfo._Class = value;
            }
        }

        public string VehicleName
        {
            get
            {
                return _VehicleInfo._Name;
            }
            set
            {
                _VehicleInfo._Name = value;
            }
        }

        public UInt32 ClassIndex
        {
            get
            {
                return _ClassInfo._Index;
            }
            set
            {
                _ClassInfo._Index = value;
            }
        }

        public string ClassName
        {
            get
            {
                return _ClassInfo._Name;
            }
            set
            {
                _ClassInfo._Name = value;
            }
        }


    }
}
