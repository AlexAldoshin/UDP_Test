using System;
using System.Linq;
using System.Threading;
using UDP_Test.Act;


namespace UDP_Test
{
    class Program
    {
        static void Main()
        {            
            ActWithServer A = new ActWithServer();
            A.run();
        }
    }
}