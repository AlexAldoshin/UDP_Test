using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace UDP_Test.DB
{
    class UserContext : DbContext
    {
        public UserContext() : base("IoTDB") { }

        public DbSet<User> Users { get; set; }
        public DbSet<NBIoTData> NBIoTDatas { get; set; }
        public DbSet<NBIoTCommand> NBIoTCommands { get; set; }
    }
}
