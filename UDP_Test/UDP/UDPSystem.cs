using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UDP_Test.DB;

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

            using (UdpClient ServerToUser = new UdpClient())
            {
                try
                {
                    var ParsePacket = new DataPackets.NBIoT(data);
                    if (ParsePacket.DataOk)
                    {
                        using (UserContext db = new UserContext())
                        {
                            db.Configuration.AutoDetectChangesEnabled = false;
                            db.Configuration.ValidateOnSaveEnabled = false;
                            var user = db.Users.Where(p => p.KeyAPI == ParsePacket.KeyAPI).First();
                            if (user != null)
                            {
                                //Внесем данные устройства
                                NBIoTData iotData = new NBIoTData {
                                    UserId = user.Id, address = IpEP.Address.Address,
                                    port = IpEP.Port, IdDev =ParsePacket.IdDev, IdMSG= ParsePacket.IdMSG,
                                    IMEI = ParsePacket.IMEI, IMSI= ParsePacket.IMSI, Data= ParsePacket.Data };
                                db.NBIoTDatas.Add(iotData);
                                //добавляем в бд
                                db.SaveChanges();
                                Console.WriteLine("Данные сохранены. User=" + user.Id);

                                byte[] dgram = new byte[] { 0x64, 0x61, 0x74, 0x61, 0x20, 0x6f, 0x6b, 0x00 }; // ответ для примера 0 в конце
                                ServerToUser.Send(dgram, dgram.Length, IpEP); //отправим ответ пользователю
                                Console.WriteLine("Port:{0}; User:{1}; {2}", ServerPort, IpEP, ParsePacket);
                            }
                        }                        
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
}
