using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace WebIoT.Models
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