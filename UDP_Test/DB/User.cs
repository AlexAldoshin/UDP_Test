using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace UDP_Test.DB
{

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Index]
        public Guid KeyAPI { get; set; }
        [Index]
        public Guid ReadKeyAPI { get; set; }

        public ICollection<NBIoTCommand> NBIoTCommands { get; set; }
        public User()
        {
            NBIoTCommands = new List<NBIoTCommand>();
        }

    }

}
