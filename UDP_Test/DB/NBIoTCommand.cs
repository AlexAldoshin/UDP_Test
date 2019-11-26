using System;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace UDP_Test.DB
{
    public class NBIoTCommand
    {
        public int Id { get; set; }
        [Index]
        public int IdDev { get; set; }
        public byte[] Data { get; set; }
        public string DataShema { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
