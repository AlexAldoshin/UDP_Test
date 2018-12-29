using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebIoT.Models
{
    public class NBIoTCommand
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public byte IdDev { get; set; }
        public byte[] Data { get; set; }
    }
}