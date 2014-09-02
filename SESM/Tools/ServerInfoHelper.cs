using System.Net.Sockets;

namespace SESM.Tools
{
    public class ServerInfoHelper
    {
        public static void GetInfo()
        {
            UdpClient socket = new UdpClient();
            socket.Connect("62.210.123.61", 27016);

        }
    }
}