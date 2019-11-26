using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
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
        static object lockerCom = new object();
        private UdpClient ServerIn, ServerCommand;

        private Dictionary<Guid, User> UsersCashe = new Dictionary<Guid, User>();
        private Dictionary<Tuple<int, int>, bool> CommandBufer;

        private ConcurrentQueue<NBIoTData> QueueNBIoTData = new ConcurrentQueue<NBIoTData>();
        public int ServerPort;
        public int CommandServerPort;
        public int ConnectionCount = 0;
        public UDPSystem(int PORT, int CommandPORT)
        {
            ServerIn = new UdpClient(PORT);
            ServerCommand = new UdpClient(CommandPORT);
            Console.WriteLine("ServerPort: {0}  CommandServerPort: {1}", PORT, CommandPORT);
            ServerPort = PORT;
            CommandServerPort = CommandPORT;
            CommandBufer = new Dictionary<Tuple<int, int>, bool>();
        }

        private void ToUserAsync(IPEndPoint IpEP, byte[] data)
        {
            ConnectionCount++; //Еще один девайс
            var ParsePacket = new DataPackets.NBIoT(data);
            if (ParsePacket.DataOk)
            {
                User user = null;
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
                            lock (locker)
                            {
                                if (!UsersCashe.ContainsKey(ParsePacket.KeyAPI))
                                {
                                    UsersCashe.Add(user.KeyAPI, user);
                                }
                            }
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
                        IdDev = unchecked((int)ParsePacket.IdDev),
                        IdMSG = unchecked((short)ParsePacket.IdMSG),
                        IMEI = unchecked((long)ParsePacket.IMEI),
                        IMSI = unchecked((long)ParsePacket.IMSI),
                        Data = ParsePacket.Data,
                        DateTime=DateTime.Now
                    };
                    QueueNBIoTData.Enqueue(iotData);
                    var IdMSGBytes = BitConverter.GetBytes(ParsePacket.IdMSG);

                    Console.SetCursorPosition(0, 4);
                    Console.Write("User IP {0}:{1} IMSI:{2} MSG:{3} TimeOut:{4}     ", IpEP.Address, IpEP.Port, ParsePacket.IMSI, ParsePacket.IdMSG, ParsePacket.TimeOut);

                    byte[] dgram = new byte[] { 0x4D, 0x53, 0x47, IdMSGBytes[0], IdMSGBytes[1] }; //Ответ MSG+IdMSG

                    var timeStart = DateTime.Now;
                    bool newCommand = false;
                    bool startCommand = true;
                    while ((DateTime.Now - timeStart).TotalSeconds < ParsePacket.TimeOut) //Устройство задает время ожидания команды
                    {
                        var command = GetNewCommand(unchecked((int)ParsePacket.IdDev), user.Id, startCommand);
                        startCommand = false;
                        if (command != null)
                        {
                            //Сравним старые и новые команды                            
                            if (TestBytes(command, ParsePacket.Data)) // Команды в конце пакета. Если была новая команда - работаем
                            {
                                var dgram_command = dgram.Concat(command).ToArray(); //Объеденим ответ (MSG+IdMSG) и команды 
                                ServerIn.Send(dgram_command, dgram_command.Length, IpEP); //отправим ответ и команду
                                Console.SetCursorPosition(0, 6);
                                Console.Write("Send Commands            ");
                                newCommand = true;
                                break;
                            }
                        }
                        Thread.Sleep(1000); // Спим 1 сек.
                        Console.SetCursorPosition(0, 6);
                        Console.Write("Сommand waiting {0} sec       ", (DateTime.Now - timeStart).TotalSeconds);
                    }
                    if (!newCommand)
                    {
                        ServerIn.Send(dgram, dgram.Length, IpEP); //отправим ответ без команды
                        Console.SetCursorPosition(0, 6);
                        Console.Write("Send NoCommands       ");
                    }                    
                    Console.SetCursorPosition(0, 5);
                    Console.Write("Connections: {0}       ", ConnectionCount);
                }
            }
            ConnectionCount--; // Одним девайсом меньше
        }

        /// <summary>
        /// Команды соответствуют структуре данных от устройства. Сверка идет с конца
        /// </summary>
        /// <param name="newData">Данные для сверки. Длина newData не более oldData</param>
        /// <param name="oldData">То с чем сверяем</param>
        /// <returns>Если есть отличия  -  true</returns>
        private static bool TestBytes(byte[] newData, byte[] oldData)
        {
            for (int i = 1; i <= newData.Length; i++)
            {
                if (oldData[oldData.Length - i] != newData[newData.Length - i])
                {
                    return true;
                }
            }
            return false;
        }

        private byte[] GetNewCommand(int IdDev, int userId, bool startCommand)
        {
            byte[] NewCommand = null;
            var UserDevId = new Tuple<int, int>(userId, IdDev);
            if (!CommandBufer.ContainsKey(UserDevId))
            {
                using (var db = new UserContext())
                {
                    var sel = db.NBIoTCommands.AsNoTracking()
                        .FirstOrDefault(p => (p.UserId == userId && p.IdDev == IdDev));
                    if(sel != null)
                    {
                        NewCommand = sel.Data;
                        lock(lockerCom)
                        {
                            CommandBufer.Add(UserDevId, false);
                        }
                    }
                }
            }
            else if (CommandBufer[UserDevId] || startCommand)  // Есть новая команда или первый запрос
            {
                using (var db = new UserContext())
                {
                    var sel = db.NBIoTCommands.AsNoTracking()
                        .FirstOrDefault(p => (p.UserId == userId && p.IdDev == IdDev));
                    if(sel != null)
                    {
                        NewCommand = sel.Data;
                        lock(lockerCom)
                        {
                            CommandBufer[UserDevId] = false;
                        }
                    }
                }
            }
            return NewCommand;
        }               
        public void Run()
        {
            //QueueNBIoTData. Очередь сообщений
            Task taskSave = new Task(() => SaveData());
            taskSave.Start();

            //Сервер команд
            Task taskCommanData = new Task(() => CommanDataServer());
            taskCommanData.Start();
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

        private void CommanDataServer()
        {
            while (true)
            {
                try
                {
                    IPEndPoint IpEP = null;
                    byte[] data = ServerCommand.Receive(ref IpEP); // получаем данные команд
                    var userId = BitConverter.ToInt32(data, 0);
                    var IdDev = BitConverter.ToInt32(data, 4);
                    var UserDevId = new Tuple<int, int>(userId, IdDev);
                    Console.SetCursorPosition(0, 8);
                    Console.Write("Command to {0}:{1}        ", userId, IdDev);
                    if (!CommandBufer.ContainsKey(UserDevId))
                    {
                        lock(lockerCom)
                        {
                            CommandBufer.Add(UserDevId, true);
                        }
                    }
                    else
                    {
                        lock(lockerCom)
                        {
                            CommandBufer[UserDevId] = true;
                        }
                    }
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
            int st = QueueNBIoTData.Count;
            using (var db = new UserContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;

                NBIoTData iotd;
                int i = count;
                while (QueueNBIoTData.TryDequeue(out iotd) || (i-- < 1))
                {
                    if (iotd != null)
                    {
                        db.NBIoTDatas.Add(iotd);
                    }
                }

                db.SaveChanges();
                db.Dispose();
                Console.SetCursorPosition(0, 2);
                Console.Write("Buf:{0}; All_Start:{1}; All_Stop:{2}        ", count, st, QueueNBIoTData.Count);

            }
        }

    }
}
