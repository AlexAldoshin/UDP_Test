using System;
using System.Linq;

namespace UDP_Test.DB
{
    public class NBIoTCommand
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public byte IdDev { get; set; }
        public byte[] Data { get; set; }
    }
}
