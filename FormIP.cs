using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace f1_eTelemetry
{
    public partial class FormIP : Form
    {
        public FormIP()
        {
            InitializeComponent();
            AdresseIP();
        }

        public void AdresseIPold()
        {
            // Récupérer le nom de l'hôte
            string host = Dns.GetHostName();
            Console.WriteLine("Le nom de l'hôte est :" + host);
            // Récupérer l'adresse IP
            string ip = Dns.GetHostByName(host).AddressList[0].ToString();
            Console.WriteLine("Mon adresse IP est :" + ip);
            lBAdresseIP.Text = ip;
        }

        public void AdresseIP()
        {
            string localIP = string.Empty;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            Console.WriteLine("IP Address = " + localIP);
            lBAdresseIP.Text = localIP;
        }
    }
}
