using System;
using System.Collections.Generic;
using System.Linq;
//using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UDP_Test.DataPackets
{

    public abstract class IoT
    {
        public bool DataOk = false;
        public Guid KeyAPI;  //16b
        public ushort IdMSG; //2b
        public uint IdDev;   //4b

        public IoT(byte[] packet)
        {
            var StrGUID = BitConverter.ToString(packet, 0, 16);
            KeyAPI = new Guid(StrGUID.Replace("-", ""));
            IdMSG =  BitConverter.ToUInt16(packet, 16);
            IdDev = BitConverter.ToUInt32(packet, 18);
        }

        public override string ToString()
        {
            return String.Format("KeyAPI:{0}; IdMSG:{1}; IdDev:{2}", KeyAPI.ToString(), IdMSG, IdDev);
        }
    }
    public class NBIoT : IoT
    {
        public ulong IMEI;
        public ulong IMSI;
        public UInt16 TimeOut; 
        public byte[] Data;
        public NBIoT(byte[] packet) : base(packet)
        {
            if (packet.Length > 30) 
            {                
                IMEI = BitConverter.ToUInt64(packet, 22);
                IMSI = BitConverter.ToUInt64(packet, 30);
                TimeOut = BitConverter.ToUInt16(packet, 38);
                Data = new byte[packet.Length - 40]; //Данные  пользователя
                Array.Copy(packet, 40, Data, 0, Data.Length);
                DataOk = true;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}; IMEI:{1}; IMSI:{2}", base.ToString(), IMEI, IMSI);
        }
    }
}
