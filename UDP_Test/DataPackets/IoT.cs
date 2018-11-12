using System;
using System.Collections.Generic;
using System.Linq;
//using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UDP_Test.DataPackets
{

    public unsafe abstract class IoT
    {
        public bool DataOk = false;
        public Guid KeyAPI;
        public ushort IdMSG; //2b
        public byte IdDev; //1b                           

        public IoT(byte[] packet)
        {
            if (packet.Length > 17) // throw new ArgumentOutOfRangeException("packet", "packet Length < 14");
            {
                byte[] KeyAPIByte = new byte[16]; //GUID=16B
                for (int i = 0; i < 16; i++)
                {
                    KeyAPIByte[i] = packet[i];
                }
                KeyAPI = new Guid(KeyAPIByte);
                //Загрузка 13-15 байтов 12..14
                byte* pp = stackalloc byte[3];
                for (int i = 16; i < 19; i++)
                {
                    pp[i - 16] = packet[i];
                }
                IdMSG = *(ushort*)(pp); //
                IdDev = *(pp + 2); //
            }
        }

        public override string ToString()
        {
            return String.Format("KeyAPI:{0}; IdMSG:{1}; IdDev:{2}", KeyAPI.ToString(), IdMSG, IdDev);
        }
    }
    public unsafe class NBIoT : IoT
    {
        public ulong IMEI;
        public ulong IMSI;
        public byte[] Data;
        public NBIoT(byte[] packet) : base(packet)
        {
            if (packet.Length > 30) //throw new ArgumentOutOfRangeException("packet", "packet Length error");
            {
                byte* pp = stackalloc byte[16];
                IMEI = *(ulong*)(pp);
                IMSI = *(ulong*)(pp + 8);
                Data = new byte[packet.Length - 35];
                for (int i = 19; i < 35; i++)
                {
                    pp[i - 19] = packet[i];
                }
                Array.Copy(packet, 35, Data, 0, packet.Length - 35);
                DataOk = true;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}; IMEI:{1}; IMSI:{2}", base.ToString(), IMEI, IMSI);
        }
    }
}
