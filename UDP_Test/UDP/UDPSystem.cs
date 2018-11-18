using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using UDP_Test.DB;

namespace UDP_Test.UDP
{
    class UDPSystem
    {
        static object locker = new object();
        private UdpClient ServerIn;
        private Dictionary<Guid, User> UsersCashe = new Dictionary<Guid, User>();
        private int CounterForSaveData = 100;



        private ConcurrentQueue<NBIoTData> QueueNBIoTData = new ConcurrentQueue<NBIoTData>();
        public int ServerPort;
        public UDPSystem(int PORT)

        {
            ServerIn = new UdpClient(PORT);
            Console.WriteLine("Port: {0}", PORT);
            ServerPort = PORT;
        }

        private void ToUserAsync(IPEndPoint IpEP, byte[] data)
        {
            var ParsePacket = new DataPackets.NBIoT(data);
            if (ParsePacket.DataOk)
            {
                User user;
                if (UsersCashe.ContainsKey(ParsePacket.KeyAPI))
                {
                    user = UsersCashe[ParsePacket.KeyAPI];
                }
                else
                {
                    using (var db = new UserContext())
                    {
                        IQueryable<User> queryable = db.Users.Where(p => p.KeyAPI == ParsePacket.KeyAPI);
                        if (queryable.Count() > 0)
                        {
                            user = queryable.First();
                        }
                        else
                        {
                            db.Users.Add(new User { KeyAPI = ParsePacket.KeyAPI, Name = ParsePacket.KeyAPI.ToString() });
                            db.SaveChanges();
                            user = db.Users.Where(p => p.KeyAPI == ParsePacket.KeyAPI)?.First();
                        }
                    }
                    lock (locker)
                    {
                        if (!UsersCashe.ContainsKey(ParsePacket.KeyAPI))
                        {
                            UsersCashe.Add(user.KeyAPI, user);
                        }
                    }
                }

                if (user != null)
                {
                    //Внесем данные устройства
                    NBIoTData iotData = new NBIoTData
                    {
                        UserId = user.Id,
                        address = IpEP.Address.Address,
                        port = IpEP.Port,
                        IdDev = ParsePacket.IdDev,
                        IdMSG = (short)ParsePacket.IdMSG,
                        IMEI = (long)ParsePacket.IMEI,
                        IMSI = (long)ParsePacket.IMSI,
                        Data = ParsePacket.Data
                    };

                    //int buf = QueueNBIoTData.Count;

                    QueueNBIoTData.Enqueue(iotData);

                    //SaveIoTData(db, iotData);
                    //добавляем в бд
                    //db.NBIoTDatas.Add(iotData);
                    //db.SaveChanges();

                    byte[] dgram = new byte[] { 0x64, 0x61, 0x74, 0x61, 0x20, 0x6f, 0x6b, 0x00 }; // ответ для примера 0 в конце
                    ServerIn.Send(dgram, dgram.Length, IpEP); //отправим ответ пользователю

                    //
                    //if (ParsePacket.IdMSG % 100 == 0)
                    //{
                    //    Console.WriteLine("Buf:{0}; Port:{1}; User:{2}; {3}", buf, ServerPort, IpEP, ParsePacket);
                    //}
                }

            }
        }
        private static void SaveIoTData(UserContext db, NBIoTData iotData)
        {
            var par = new Npgsql.NpgsqlParameter[] {

                            new Npgsql.NpgsqlParameter("@p1", iotData.UserId) ,
                            new Npgsql.NpgsqlParameter("@p2", iotData.IdMSG),
                            new Npgsql.NpgsqlParameter("@p3", iotData.IdDev),
                            new Npgsql.NpgsqlParameter("@p4", iotData.IMEI),
                            new Npgsql.NpgsqlParameter("@p5", iotData.IMSI),
                            new Npgsql.NpgsqlParameter("@p6", iotData.address),
                            new Npgsql.NpgsqlParameter("@p7", iotData.port),
                            new Npgsql.NpgsqlParameter("@p8", iotData.Data)
                        };
            db.Database.ExecuteSqlCommand("INSERT INTO dbo.\"NBIoTDatas\"( \"UserId\", \"IdMSG\", \"IdDev\", \"IMEI\", \"IMSI\", address, port, \"Data\") VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8)", par);
        }

        public void Run()
        {
         
            //QueueNBIoTData.
            Task taskSave = new Task(() => SaveData());
            taskSave.Start();

            while (true)
            {
                try
                {
                    IPEndPoint IpEP = null;
                    byte[] data = ServerIn.Receive(ref IpEP); // получаем данные
                    Task task = new Task(() => ToUserAsync(IpEP, data));
                    task.Start();
                }
                catch (Exception ex)
                {

                }

            }
        }

        private void SaveData()
        {
            while (true)
            {
                if (QueueNBIoTData.Count > 100)
                {   
                     var task = Task.Run(() => Save(100));                 
                }
                else
                {                    
                    if (QueueNBIoTData.Count > 0) // будем сохранять немного пока нет нагрузки
                    {
                        Save(QueueNBIoTData.Count);
                    }                    
                }
                Thread.Sleep(10); //не грузим проц
            }
        }

        private void Save(int count)
        {
            Console.WriteLine("NewSave {0}", QueueNBIoTData.Count);
            int st = QueueNBIoTData.Count;
            using (var db = new UserContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;

                NBIoTData iotd;
                int i = count;
                while (QueueNBIoTData.TryDequeue(out iotd) || (i-- < 1))
                {
                    if(iotd != null)
                    {
                        db.NBIoTDatas.Add(iotd);                        
                    }
                }

                db.SaveChanges();
                db.Dispose();

                Console.WriteLine("Buf:{0}; All_Start:{1}; All_Stop:{2}", count, st, QueueNBIoTData.Count);
            }
        }

        public void Stop()
        {
            ServerIn.Close();
        }
    }
}
