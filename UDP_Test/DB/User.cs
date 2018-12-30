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
        public string DataShema { get; set; }
    }
}
