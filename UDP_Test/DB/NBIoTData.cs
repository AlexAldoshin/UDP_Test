using System;
using System.Linq;

namespace UDP_Test.DB
{
    public class NBIoTData
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public short IdMSG { get; set; }
        public byte IdDev { get; set; }
        public long IMEI { get; set; }
        public long IMSI { get; set; }
        public long address { get; set; }
        public int port { get; set; }
        public byte[] Data { get; set; }
        public DateTime DateTime { get; set; }
    }
}
