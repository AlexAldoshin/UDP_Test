using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace UDP_Test.UDP
{
    class UDPSystem
    {
        private UdpClient ServerIn;
        ServersTo ServersToUsers = new ServersTo();

        public UDPSystem(int PORT)
        {            
            ServerIn = new UdpClient(PORT);
            Console.WriteLine("Port: {0}", PORT);
        }

        private static void ToUserAsync(IPEndPoint IpEP, byte[] data)
        {
            string message = Encoding.ASCII.GetString(data);
            var ParsePacket = new DataPackets.NBIoT(data);
            UdpClient ServerToUser = new UdpClient();
            try
            {
                ServerToUser.Send(new byte[] { 0 }, 1, IpEP); //отправим ответ пользователю 0-ok
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            ServerToUser.Close(); // обязательно закроем
            Console.WriteLine("User:{0}; {1}", IpEP, ParsePacket);
        }

        public void Run()
        {
            try
            {
                while (true)
                {
                    //if (Serv.Available==0) Thread.Sleep(2000);
                    IPEndPoint IpEP = null;
                    byte[] data = ServerIn.Receive(ref IpEP); // получаем данные
                    Task task = new Task(() => ToUserAsync(IpEP, data));
                    task.Start();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

  
        public void Stop()
        {
            ServerIn.Close();
        }
    }

    public class ServersTo
    {
        public int PortCount;
        public int PortFrom;
        public Dictionary<int, UdpClient> Servers = new Dictionary<int, UdpClient>();

        public ServersTo() : this(10000, 1000)
        {

        }

        public ServersTo(int portFrom, int portCount)
        {
            PortFrom = portFrom;
            PortCount = portCount;            
        }
        
        public UdpClient GetServerTo()
        {
            UdpClient server;
            int port = PortFrom;

            while (true)
            {
      
                if (Servers.ContainsKey(port))
                {
                    server = Servers[port];
                }
                else
                {
                    server = new UdpClient(port);
                    Servers[port] = server;
                }

                if (!server.Client.Connected)
                {
                    return server;
                }
                if (++port > (PortFrom + PortCount))
                {
                    port = PortFrom;
                    Thread.Sleep(10);
                }
            }
        }




    }
}
