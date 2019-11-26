using System;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace UDP_Test.DB
{
    public class NBIoTData
    {
        public int Id { get; set; }
        [Index]
        public int UserId { get; set; }
        [Index]
        public short IdMSG { get; set; }
        [Index]
        public int IdDev { get; set; }
        [Index]
        public long IMEI { get; set; }
        [Index]
        public long IMSI { get; set; }
        [Index]
        public DateTime DateTime { get; set; }
        public long address { get; set; }
        public int port { get; set; }
        public byte[] Data { get; set; }
    }
}
