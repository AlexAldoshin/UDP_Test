using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebIoT.Models
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
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float Elevation { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] Image { get; set; }
    }
}
