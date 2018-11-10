﻿using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace UDP_Test.UDP
{
    class UDPSystem
    {
        private UdpClient ServerIn;
        public int ServerPort;
        public UDPSystem(int PORT)
        {
            ServerIn = new UdpClient(PORT);
            Console.WriteLine("Port: {0}", PORT);
            ServerPort = PORT;
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
        private void ToUserAsync(IPEndPoint IpEP, byte[] data)
        {

            UdpClient ServerToUser = new UdpClient();
            try
            {
                var ParsePacket = new DataPackets.NBIoT(data);
                if (ParsePacket.DataOk)
                {
                    byte[] dgram = new byte[] { 0x64, 0x61, 0x74, 0x61, 0x20, 0x6f, 0x6b, 0x00 }; // ответ для примера
                    ServerToUser.Send(dgram, dgram.Length, IpEP); //отправим ответ пользователю 0 в конце
                    Console.WriteLine("Port:{2}; User:{0}; {1}", IpEP, ParsePacket, ServerPort);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                ServerToUser.Close(); // обязательно закроем
            }



        }
    }
}
