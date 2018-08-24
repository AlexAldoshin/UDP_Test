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
            var ParsePacket = new DataPackets.NBIoT(data);
            UdpClient ServerToUser = new UdpClient();
            try
            {
                byte[] dgram = new byte[] { 0xFF, 0x64, 0x61, 0x74, 0x61, 0x20, 0x6f, 0x6b, 0x00 }; // ответ для примера
                ServerToUser.Send(dgram, dgram.Length, IpEP); //отправим ответ пользователю 0-ok
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
}
