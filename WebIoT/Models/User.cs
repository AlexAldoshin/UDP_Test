using System;
using System.Linq;

namespace WebIoT.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid KeyAPI { get; set; }
        public string DataShema { get; set; }
    }
}