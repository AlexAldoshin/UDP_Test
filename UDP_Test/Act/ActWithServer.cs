using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using UDP_Test.UDP;

namespace UDP_Test.Act
{
    class ActWithServer
    {
        private List<UDPSystem> UDPSystems = new List<UDPSystem>();
        private List<Thread> threads = new List<Thread>();
        public void _act()
        {
            IPAddress ip = IPAddress.Any;
            for (int i = 8310; i < 8320; i++)
            {
                var r = new UDPSystem(i);
                UDPSystems.Add(r);
                threads.Add(new Thread(new ThreadStart(UDPSystems[UDPSystems.Count - 1].Run)));
                threads[threads.Count-1].Start();
            }

            Console.ReadLine();
        }
    }
}
