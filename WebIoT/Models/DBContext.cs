﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace WebIoT.Models
{
    public class DBContext : DbContext
    {
        public DBContext() : base("DefaultConnection") { }
        public DbSet<User> Users { get; set; }
        public DbSet<NBIoTData> NBIoTDatas { get; set; }
        public DbSet<NBIoTCommand> NBIoTCommands { get; set; }
    }
}