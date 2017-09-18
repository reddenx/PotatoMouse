using SMT.Networking.NetworkConnection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR.Networking
{
    /// <summary>
    /// abstracts message protocol from application layer
    /// </summary>
    public class Sender
    {
        private readonly UdpNetworkConnection<string> Udp;
        private readonly Guid Identifier = Guid.NewGuid();

        public Sender(string hostname, int port)
        {
            Udp = new UdpNetworkConnection<string>(new AsciiSerializer());
            Udp.OnError += Udp_OnError;
            Udp.Target(hostname, port);
        }

        public Sender(UdpNetworkConnection<string> targetedUdpClient)
        {
            Udp = targetedUdpClient;
            Udp.OnError += Udp_OnError;
        }

        private void Udp_OnError(object sender, Exception e) { }

        public void MouseMove(int x, int y)
        {
            Udp.Send($"[{PhoneRemoteCommands.MOUSE_POSITION_MOVE}:{x},{y}:{Identifier}]");
        }

        public void MouseSet(int x, int y)
        {
            Udp.Send($"[{PhoneRemoteCommands.MOUSE_POSITION_SET}:{x},{y}:{Identifier}]");
        }

        public void MouseLeftClick()
        {
            Udp.Send($"[{PhoneRemoteCommands.MOUSE_LEFT_CLICK}::{Identifier}]");
        }

        public void MouseDoubleLeftClick()
        {
            Udp.Send($"[{PhoneRemoteCommands.MOUSE_LEFT_DOUBLE_CLICK}::{Identifier}]");
        }

        public void MouseLeftPress()
        {
            Udp.Send($"[{PhoneRemoteCommands.MOUSE_LEFT_PRESS}::{Identifier}]");
        }

        public void MouseLeftRelease()
        {
            Udp.Send($"[{PhoneRemoteCommands.MOUSE_LEFT_RELEASE}::{Identifier}]");
        }

        public void MouseRightClick()
        {
            Udp.Send($"[{PhoneRemoteCommands.MOUSE_RIGHT_CLICK}::{Identifier}]");
        }

        public void MouseDoubleRightClick()
        {
            Udp.Send($"[{PhoneRemoteCommands.MOUSE_RIGHT_DOUBLE_CLICK}::{Identifier}]");
        }

        public void MouseRightPress()
        {
            Udp.Send($"[{PhoneRemoteCommands.MOUSE_RIGHT_PRESS}::{Identifier}]");
        }

        public void MouseRightRelease()
        {
            Udp.Send($"[{PhoneRemoteCommands.MOUSE_RIGHT_RELEASE}::{Identifier}]");
        }

        public void ScrollwheelClick()
        {
            Udp.Send($"[{PhoneRemoteCommands.MOUSE_SCROLLWHEEL_CLICK}::{Identifier}]");
        }

        public void ScrollwheelMove(int amount)
        {
            Udp.Send($"[{PhoneRemoteCommands.MOUSE_SCROLLWHEEL_SCROLL_MOVE}:{amount}:{Identifier}]");
        }

        //public void KeyClick(string key){}
        //public void KeyPress(string key){}
        //public void KeyRelease(string key){}
        //public void KeyString(string keys){}
    }
}
