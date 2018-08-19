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
        public byte[] KeyAPI = new byte[12]; //12B                
        public ushort IdMSG; //2b
        public byte IdDev; //1b                           

        public IoT(byte[] packet)
        {
            if (packet.Length < 14) throw new ArgumentOutOfRangeException("packet", "packet Length < 14");

            //Загрузка первых 12 байт 0..11
            for (int i = 0; i < 12; i++)
            {
                KeyAPI[i] = packet[i];
            }
            //Загрузка 13-15 байтов 12..14
            byte* pp = stackalloc byte[3];            
            for (int i = 12; i < 15; i++)
            {
                pp[i-12] = packet[i];
            }
            IdMSG = *(ushort*)(pp); //13-14 байты; 12..13
            IdDev = *(pp+2); //15 байт; id=14
        }

        public override string ToString()
        {
            return String.Format("KeyAPI:{0}; IdMSG:{1}; IdDev:{2}",BitConverter.ToString(KeyAPI), IdMSG, IdDev);
        }
    }
    public unsafe class NBIoT : IoT
    {
        public ulong IMEI; 
        public ulong IMSI;
        public byte[] Data;
        public NBIoT(byte[] packet) : base(packet)
        {
            if (packet.Length<31) throw new ArgumentOutOfRangeException("packet", "packet Length error");
            
            //Загрузка 16-31 байтов
            byte* pp = stackalloc byte[16];
            for (int i = 15; i < 31; i++)
            {
                pp[i - 15] = packet[i];
            }
            IMEI = *(ulong*)(pp); //15..22
            IMSI = *(ulong*)(pp + 8); //23..30
            Data = new byte[packet.Length - 31];
            for (int i = 15; i < 31; i++)
            {
                pp[i - 15] = packet[i];
            }
            Array.Copy(packet, 31, Data, 0, packet.Length - 31);
        }

        public override string ToString()
        {
            return  string.Format("{0}; IMEI:{1}; IMSI:{2}", base.ToString(), IMEI, IMSI);
        }
    }
}
