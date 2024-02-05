using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace f1_eTelemetry
{
    internal class UDP_Read
    {
        private static UdpClient listener = new UdpClient(20777);                       //Create a UDPClient object
        public IPEndPoint RemoteIP;
        private List<byte[]> ListReceived = new List<byte[]>();

        private Boolean Flaglistener = true;
        RichTextBox rTB_Terminal;
        Button btn_uDP;

        public void UDPread(RichTextBox _rTB_Terminal, Button _btn_uDP)
        {
            try
            {
                rTB_Terminal = _rTB_Terminal;
                btn_uDP = _btn_uDP;

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

        // Lecture des informations UDP et traitement
        private void UDPrecevalt(IAsyncResult res)
        {
            // Compatible PC2 et F1202X au port prêt

            if (Flaglistener) // seule façon trouvée pour le moment de ne pas réécouter l'UDP les autres bloquent le programme
            {
                btn_uDP.BackColor = Color.Green;

                byte[] receivedRAW = listener.EndReceive(res, ref RemoteIP);
                ListReceived.Add(receivedRAW); // stock les données brutes avant le traitement

                listener.BeginReceive(new AsyncCallback(UDPrecevalt), null);
            }
        }

        public void GetListReceived()
        {

        }
    }
}
