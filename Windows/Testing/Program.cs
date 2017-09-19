using PR.Emulation.Mouse;
using SMT.Networking.NetworkConnection;
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
            //RunJoysticMouseTest();
            //RunBroadcastTest();
            RunProtoLoop();
        }

        private static void RunProtoLoop()
        {
            //setup network
            var udp = new UdpNetworkConnection<string>(new AsciiSerializer());
            udp.StartListening(37015);
            udp.Target(IPAddress.Broadcast.ToString(), 37015);

            //run broadcast
            var broadcastLoop = Fork(() =>
            {
                var myHostname = Dns.GetHostAddresses(Dns.GetHostName()).First(host => host.AddressFamily == AddressFamily.InterNetwork).ToString();
                while (true)
                {
                    Thread.Sleep(1000);
                    udp.Send(myHostname);
                }
            });

            //var joystic = new MouseJoystic();
            var joystic = new Mousepad();
            joystic.SetMovementScale(1f);

            //andle messages
            udp.OnMessageReceived += (sender, args) =>
            {
                //validate
                if (!args.StartsWith("[") || !args.EndsWith("]"))
                {
                    return;
                }


                //parse
                var tokens = args.Trim('[', ']').Split(':');
                switch (tokens[0])
                {
                    case "down"://set to 0 to prep
                        {
                            float x, y;
                            var pieces = tokens[1].Split(',');
                            if (float.TryParse(pieces[0], out x) && float.TryParse(pieces[1], out y))
                            {
                                //joystic.Start(x, y);
                            }
                            break;
                        }
                    case "up"://set to 0 to make it stop
                        {
                            float x, y;
                            var pieces = tokens[1].Split(',');
                            if (float.TryParse(pieces[0], out x) && float.TryParse(pieces[1], out y))
                            {
                                //joystic.Stop(x, y);
                            }
                            break;
                        }
                    case "move"://set to input
                        {
                            float x, y;
                            var pieces = tokens[1].Split(',');
                            if (float.TryParse(pieces[0], out x) && float.TryParse(pieces[1], out y))
                            {
                                //joystic.Moved(x, y);
                            }
                            else
                            {
                            }
                            break;
                        }
                    case "click_left":
                        {
                            joystic.LeftClick();
                            break;
                        }
                    case "click_right":
                        {
                            joystic.RightClick();
                            break;
                        }
                    default: { break; }
                }
            };

            //hold UI
            while (true)
            {
                Console.ReadKey();
            }
        }


        private static void RunJoysticMouseTest()
        {
            // x right, y down
            var joystic = new MouseJoystic();

            //joystic.SetJoysticState(new JoysticState() { X = .5f, Y = 1 }); Thread.Sleep(500);
            //joystic.SetJoysticState(new JoysticState() { X = 1, Y = -.5f }); Thread.Sleep(500);
            //joystic.SetJoysticState(new JoysticState() { X = -.5f, Y = -1 }); Thread.Sleep(500);
            //joystic.SetJoysticState(new JoysticState() { X = -1, Y = .5f }); Thread.Sleep(500);

            //joystic.SetJoysticState(new JoysticState() { X = .5f, Y = 1 }); Thread.Sleep(500);
            //joystic.SetJoysticState(new JoysticState() { X = 1, Y = -.5f }); Thread.Sleep(500);
            //joystic.SetJoysticState(new JoysticState() { X = -.5f, Y = -1 }); Thread.Sleep(500);
            //joystic.SetJoysticState(new JoysticState() { X = -1, Y = .5f }); Thread.Sleep(500);

            //joystic.SetJoysticState(new JoysticState() { X = 2, Y = 0 }); Thread.Sleep(500);
            //joystic.SetJoysticState(new JoysticState() { X = -2, Y = 0 }); Thread.Sleep(500);

            //joystic.SetJoysticState(new JoysticState() { X = 0, Y = 0 });

            //Thread.Sleep(2000);
            //joystic.LeftDoubleClick();
            //Thread.Sleep(1000);

            Console.ReadLine();
        }

        private static void RunBroadcastTest()
        {
            var udpSender = new UdpClient();

            var myHostname = Dns.GetHostAddresses(Dns.GetHostName()).First(host => host.AddressFamily == AddressFamily.InterNetwork);
            var myHostnameBytes = ASCIIEncoding.ASCII.GetBytes(myHostname.ToString());
            int port = 37015;

            var endpoint = new IPEndPoint(IPAddress.Broadcast, port);

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

                Console.WriteLine($"message-{message}:{referenceEndpoint.Address.ToString()} {DateTime.Now.ToString("HH:mm ss.ff")}");
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
