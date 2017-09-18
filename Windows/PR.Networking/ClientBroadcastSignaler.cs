using SMT.Networking.NetworkConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PR.Networking
{
    public class ClientBroadcastSignaler
    {
        public bool IsBroadcasting { get; private set; }

        public event EventHandler<Exception> OnError;

        private readonly Thread BroadcastThread;
        private DateTime PauseUntil;
        private int BeaconIntervalMilli;
        private readonly int Port;

        public ClientBroadcastSignaler(int port)
        {
            IsBroadcasting = false;
            PauseUntil = DateTime.Now;
            BeaconIntervalMilli = 3000;
            Port = port;

            BroadcastThread = Parrellelism.Fork(BroadcastLoop);
        }

        public void Start()
        {
            IsBroadcasting = true;
        }

        public void Stop()
        {
            IsBroadcasting = false;
        }

        public void Pause(uint pauseSeconds)
        {
            PauseUntil = DateTime.Now.AddSeconds(pauseSeconds);
        }

        public void SetBeaconInterval(int milli)
        {
            BeaconIntervalMilli = Math.Min(milli, 500);
        }

        private void BroadcastLoop()
        {
            var udp = new UdpNetworkConnection<string>(new AsciiSerializer());
            udp.Target(IPAddress.Broadcast.ToString(), Port);
            udp.OnError += (sender, args) =>
            {
                OnError?.Invoke(this, args);
            };

            //should be my hostname on this network
            var message = Dns.GetHostAddresses(Dns.GetHostName()).First(host => host.AddressFamily == AddressFamily.InterNetwork).ToString();

            while (true)
            {
                Thread.Sleep(BeaconIntervalMilli);

                if (IsBroadcasting)
                    udp.Send(message);
            }
        }
    }
}
