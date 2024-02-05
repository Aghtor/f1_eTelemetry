using f1_eTelemetry.rFactor2Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml.Serialization;


namespace f1_eTelemetry
{
    public class Setting
    {
        public struct folders_simu
        {
            public string folderRawdata; //= "C:\\Users\\franc\\VisualStudio\\F1Telemetry\\f1_eTelemetry\\raw";
            public string folderDeepdata; //= "C:\\Users\\franc\\VisualStudio\\F1Telemetry\\f1_eTelemetry\\deep";
            public string folderExportdata;// = "C:\\Users\\franc\\VisualStudio\\F1Telemetry\\f1_eTelemetry\\export";
        }

        public folders_simu[] Folders_simu = new folders_simu[10];

        public int ButtonBox = 4;
        public Boolean Joystick;
        public string Box = "Generic USB Joystick";
        public Boolean datacompress;
        public Boolean dataEssentielles;
        public Boolean dataonefile = true;
        public Boolean datatwofiles = false;

        private string fileName = ".\\eTelemetry.conf";
        public float trigger_time = 10000;
        public float synchroTolerance = 5;


        // MAP 3D View
        public double Xc = 500;
        public double Yc = 500;
        public double Zc = 500;

        public int Simulation; // 0 F1 - 1 PC2 - 2 RF2 - 3 ACC - 4 AC 5 Raceroom - 6 Forza

        // influxDB
        public string token = "vJ9uxxgcSnj5ZQ6BFpDqoiMxJ6nZyR1k-EovUOPD8xzPEKfCce-3YoJQQH_fuUCLp5WII7b5d7M-sqwxNhzESA==";
        public string bucket = "ProjectCars2";
        public string org = "honskirchgroup";

        public string InfluxDBAdress = "http://192.168.1.42:8086/";
        public string InfluxDBAdress1 = "http://25.42.134.226:8086";
        public string InfluxDBAdress2 = "http://192.168.1.42:8086/";
        public string InfluxDBAdress3 = "http://25.42.134.226:8086";

        public bool SaveInfluxDB;
        public bool DataCompress;

        public bool DataCompress2;

        //https://www.c-sharpcorner.com/UploadFile/mahesh/working-with-listview-in-C-Sharp/
        public List<Color> TrackColor = new List<Color>()
        {
            Color.Beige,
            Color.AliceBlue,
            Color.Blue,
            Color.BlueViolet,
            Color.Crimson,
            Color.Coral,
            Color.DarkBlue,
            Color.DarkGray,
            Color.Firebrick,
            Color.ForestGreen,
            Color.Green,
            Color.Goldenrod,
            Color.Honeydew,
            Color.Indigo,
            Color.Honeydew,
            Color.Khaki,
            Color.Lavender,
            Color.LightBlue,
            Color.Magenta,
            Color.MediumPurple,
            Color.Orange,
            Color.Olive,
            Color.PaleGoldenrod,
            Color.PeachPuff,
            Color.Red,
            Color.RoyalBlue,
            Color.SeaGreen,
            Color.SteelBlue,
            Color.Turquoise,
            Color.Violet,
            Color.Yellow,
            Color.YellowGreen
        };

        public Color ColorGet(int NbColor)
        {
            if (NbColor < TrackColor.Count - 1)
                return TrackColor[NbColor];
            else
            {
                TrackColor.Add(Color.Black);
                return TrackColor[NbColor];
            }

        }

        public List<Color> ColorDegradee = new List<Color>()
        {
            Color.Blue,
            Color.Cyan,
            Color.Yellow,
            Color.Orange,
            Color.OrangeRed,
            Color.Red
        };

        public void init()
        {
          for (int i=0; i<10;i++)
            {
                Folders_simu[i].folderDeepdata = ".\\";
                Folders_simu[i].folderRawdata = ".\\";
                Folders_simu[i].folderExportdata = ".\\";
            }
        }
        public void Save(string fileName)
        {
            //if (System.IO.File.Exists(fileName)== false) /// 
            using (var writer = new StreamWriter(fileName))
            {
                var serializer = new XmlSerializer(typeof(Setting));
                serializer.Serialize(writer, this);
            }
        }

        public static Setting Load(string fileName)
        {
            if (System.IO.File.Exists(fileName)) /// 
            using (var reader = new StreamReader(fileName))
            {
                    try
                    {
                        var serializer = new XmlSerializer(typeof(Setting));
                        return (Setting)serializer.Deserialize(reader);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error:" + ex + " " + ex.Message + "Objet :" + ex.Source + "Link :" + ex.HelpLink); // A mettre dans fenêtre
                        return null;
                    }
            }
            else 
            {
                return null; 
            }
        }

        public string GetSimulationName()
        {
            switch (Simulation)
            {
                case 0:
                    return "F1 2022";
                case 1:
                    return "Project Cars 2";
                case 2:
                    return "RFactor 2";
                case 3:
                    return "Assetto Corsa Competizione";
                case 4:
                    return "Assetto Corsa";
                case 5:
                    return "Forza";
                case 6:
                    return "Raceroom";
                case 7:
                    return "Automobilista 2";
            }
            return "";
        }
    }
}
 