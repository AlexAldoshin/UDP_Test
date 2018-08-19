using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDP_Test.UDP;
using UDP_Test.Act;
using System.Threading;
using System.Net;

namespace UDP_Test
{
    class Program
    {
        static void Main()
        {
            ActWithServer A = new ActWithServer();
            A._act();
        }
    }
}