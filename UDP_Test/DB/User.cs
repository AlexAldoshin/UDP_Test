using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDP_Test.DB
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid KeyAPI { get; set; }     
    }

    public class NBIoTData
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public ushort IdMSG { get; set; }
        public byte IdDev { get; set; }
        public ulong IMEI { get; set; }
        public ulong IMSI { get; set; }
        public long address { get; set; }
        public int port { get; set; }
        public byte[] Data { get; set; }
    }

}
