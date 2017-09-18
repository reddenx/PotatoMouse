using SMT.Networking.NetworkConnection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR.Networking
{
    public class Receiver
    {
        public event EventHandler<Point> OnMouseMove;
        public event EventHandler<Point> OnMouseSet;

        public event EventHandler OnMouseLeftClick;
        public event EventHandler OnMouseDoubleLeftClick;
        public event EventHandler OnMouseLeftPress;
        public event EventHandler OnMouseLeftRelease;

        public event EventHandler OnMouseRightClick;
        public event EventHandler OnMouseDoubleRightClick;
        public event EventHandler OnMouseRightPress;
        public event EventHandler OnMouseRightRelease;

        public event EventHandler OnScrollwheelClick;
        public event EventHandler<int> OnScrollwheelMove;

        public event EventHandler<Exception> OnError;

        private readonly int Port;
        public bool IsListening { get; private set; }
        private UdpNetworkConnection<string> Udp;

        public Receiver(int port)
        {
            Port = port;
            Udp = new UdpNetworkConnection<string>(new AsciiSerializer());

            Udp.OnError += Udp_OnError;
            Udp.OnMessageReceived += Udp_OnMessageReceived;
            Udp.OnMessageSent += Udp_OnMessageSent;
        }
        
        public void Start()
        {
            IsListening = true;
            Udp.StartListening(Port);
        }

        public void Stop()
        {
            IsListening = false;
            Udp.StopListening();
        }

        private void Udp_OnMessageSent(object sender, string e)
        {
            throw new NotImplementedException();
        }

        private void Udp_OnMessageReceived(object sender, string e)
        {
            //parse message
        }

        private void Udp_OnError(object sender, Exception e)
        {
            OnError?.Invoke(sender, e);
        }
    }



    internal static class PhoneRemoteCommands //protocol to adhere to
    {
        public const int MOUSE_POSITION_MOVE = 0;
        public const int MOUSE_POSITION_SET = 1;

        public const int MOUSE_LEFT_CLICK = 10;
        public const int MOUSE_LEFT_DOUBLE_CLICK = 11;
        public const int MOUSE_LEFT_PRESS = 12;
        public const int MOUSE_LEFT_RELEASE = 13;

        public const int MOUSE_RIGHT_CLICK = 20;
        public const int MOUSE_RIGHT_DOUBLE_CLICK = 21;
        public const int MOUSE_RIGHT_PRESS = 22;
        public const int MOUSE_RIGHT_RELEASE = 23;

        public const int MOUSE_SCROLLWHEEL_CLICK = 30;
        public const int MOUSE_SCROLLWHEEL_SCROLL_MOVE = 31;

        public const int KAYBOERD_KEY_CLICK = 40;
        public const int KEYBOARD_KEY_PRESS = 41;
        public const int KEYBOARD_KEY_RELEASE = 42;
        public const int KEYBOARD_KEY_STRING = 43;
    }
}

