using Dock_Examples.Interrogator;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
namespace Dock_Examples.Helpers
{
    public class ConnectionManager
    {
        private static TcpClient si155;
        private static NetworkStream tcpNetworkStream;
   
        public async Task<bool> ConnectAsync(string instrumentIpAddress)
        {
            try
            {
                si155 = new TcpClient();
                await si155.ConnectAsync(instrumentIpAddress, Command.TcpPort); //peak port
                si155.ReceiveTimeout = 0;

           
                tcpNetworkStream = si155.GetStream();
          
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bağlantı hatası: " + ex.Message);
                return false;
            }
        }
    
        public NetworkStream GetNetworkStream()
        {
            return tcpNetworkStream;
        }
        public PeakData GetPeakData()
        {
            try
            {
                CommandResponse response = Command.Execute(tcpNetworkStream, CommandName.GetPeaks);
                return response.AsPeakData();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Veri alma hatası: " + ex.Message);
                return null;
            }
        }
        public void CloseConnection()
        {
            if (si155 != null)
            {
                si155.Close();
                si155 = null;
            }
            if (tcpNetworkStream != null)
            {
                tcpNetworkStream.Close();
                tcpNetworkStream = null;
            }
        }
    }
}