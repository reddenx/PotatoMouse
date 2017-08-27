using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            RunBroadcastTest();
        }

        private static void RunBroadcastTest()
        {
            var udpSender = new UdpClient();

            var myHostname = Dns.GetHostAddresses(Dns.GetHostName()).First(host => host.AddressFamily == AddressFamily.InterNetwork);
            var myHostnameBytes = ASCIIEncoding.ASCII.GetBytes(myHostname.ToString());
            int port = 37015;

            var endpoint = new IPEndPoint(myHostname, port);

            Fork(() =>
            {
                while (true)
                {
                    Thread.Sleep(1051);
                    udpSender.Send(myHostnameBytes, myHostnameBytes.Length, endpoint);
                }
            });

            var udpReceiver = new UdpClient(port);
            var referenceEndpoint = new IPEndPoint(IPAddress.Any, port);
            while (true)
            {
                var data = udpReceiver.Receive(ref referenceEndpoint);
                var message = ASCIIEncoding.ASCII.GetString(data);

                // "yyyy'-'MM'-'dd HH':'mm':'ss'Z'"

                Console.WriteLine("message-{1}: {0}", message, DateTime.Now.ToString(
                    "HH:mm ss.ff"
                ));
            }
        }

        private static Thread Fork(Action fork)
        {
            var thread = new Thread(new ThreadStart(fork));
            thread.IsBackground = true;
            thread.Start();
            return thread;
        }
    }
}
