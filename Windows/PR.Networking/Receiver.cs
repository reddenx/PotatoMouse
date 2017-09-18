using SMT.Networking.NetworkConnection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR.Networking
{
    /// <summary>
    /// abstracts message protocol from application layer
    /// </summary>
    public class Receiver
    {
        public event EventHandler OnMouseSignalReceived;

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

        public event EventHandler<string> OnKeyboardKeyClick; //this should be unambiguously cleaned up
        public event EventHandler<string> OnKeyboardKeyPress; //this should be unambiguously cleaned up
        public event EventHandler<string> OnKeyboardKeyRelease; //this should be unambiguously cleaned up
        public event EventHandler<string> OnKeyboardKeyString;

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
            //no need for this, shouldn't be sending data
            OnError?.Invoke(this, new IOException($"the underlying udp client is sending data through the receiver, this can cause unexpexted behavior and should be avoided"));
        }

        private void Udp_OnError(object sender, Exception e)
        {
            OnError?.Invoke(sender, e);
        }

        private void Udp_OnMessageReceived(object sender, string message)
        {
            //message format:     [COMMAND_ID:DATA:TARGET_NAME]



            OnMouseSignalReceived?.Invoke(this, EventArgs.Empty);

            //is message valid
            if (!message.StartsWith("[") || !message.EndsWith("]"))//message isn't boxed properly
            {
                OnError?.Invoke(this, new IOException($"message wasn't boxed properly: {message}"));
                return;
            }

            var commandParts = message.Trim('[', ']').Split(':');
            if (commandParts.Length != 3)//message had too many parts
            {
                OnError?.Invoke(this, new IOException($"message is not internally formatted correctly, should be 3 parts: {message}"));
                return;
            }

            var commandId = 0;
            if (!int.TryParse(commandParts[0], out commandId))
            {
                OnError?.Invoke(this, new IOException($"invalid commandId string, must be an integer value, received {commandParts[0]}"));
                return;
            }

            //route message
            switch (commandId)
            {
                case PhoneRemoteCommands.MOUSE_POSITION_MOVE:
                    {
                        //format of x,y
                        var dataParts = commandParts[1].Split(',');
                        var x = 0;
                        var y = 0;
                        if (dataParts.Length != 2 || !int.TryParse(dataParts[0], out x) || !int.TryParse(dataParts[1], out y))
                        {
                            OnError?.Invoke(this, new IOException($"move command data invalid: {commandParts[1]} in message {message}"));
                        }

                        if (x == 0 && y == 0) //not an error, just not useful
                            return;

                        OnMouseMove?.Invoke(this, new Point(x, y));
                        break;
                    }
                case PhoneRemoteCommands.MOUSE_POSITION_SET:
                    {
                        var dataParts = commandParts[1].Split(',');
                        var x = 0;
                        var y = 0;
                        if (dataParts.Length != 2 || !int.TryParse(dataParts[0], out x) || !int.TryParse(dataParts[1], out y))
                        {
                            OnError?.Invoke(this, new IOException($"set command data invalid: {commandParts[1]} in message {message}"));
                        }

                        OnMouseSet?.Invoke(this, new Point(x, y));
                        break;
                    }
                case PhoneRemoteCommands.MOUSE_LEFT_CLICK:
                    OnMouseLeftClick?.Invoke(this, EventArgs.Empty);
                    break;
                case PhoneRemoteCommands.MOUSE_LEFT_DOUBLE_CLICK:
                    OnMouseDoubleLeftClick?.Invoke(this, EventArgs.Empty);
                    break;
                case PhoneRemoteCommands.MOUSE_LEFT_PRESS:
                    OnMouseLeftPress?.Invoke(this, EventArgs.Empty);
                    break;
                case PhoneRemoteCommands.MOUSE_LEFT_RELEASE:
                    OnMouseLeftRelease?.Invoke(this, EventArgs.Empty);
                    break;
                case PhoneRemoteCommands.MOUSE_RIGHT_CLICK:
                    OnMouseRightClick?.Invoke(this, EventArgs.Empty);
                    break;
                case PhoneRemoteCommands.MOUSE_RIGHT_DOUBLE_CLICK:
                    OnMouseDoubleRightClick?.Invoke(this, EventArgs.Empty);
                    break;
                case PhoneRemoteCommands.MOUSE_RIGHT_PRESS:
                    OnMouseRightPress?.Invoke(this, EventArgs.Empty);
                    break;
                case PhoneRemoteCommands.MOUSE_RIGHT_RELEASE:
                    OnMouseRightRelease?.Invoke(this, EventArgs.Empty);
                    break;
                case PhoneRemoteCommands.MOUSE_SCROLLWHEEL_CLICK:
                    OnScrollwheelClick?.Invoke(this, EventArgs.Empty);
                    break;
                case PhoneRemoteCommands.MOUSE_SCROLLWHEEL_SCROLL_MOVE:
                    {
                        var moveAmount = 0;
                        if (!int.TryParse(commandParts[1], out moveAmount))
                        {
                            OnError?.Invoke(this, new IOException($"mouse scroll data invalid: {commandParts[1]}, message: {message}"));
                            return;
                        }

                        OnScrollwheelMove?.Invoke(this, moveAmount);
                        break;
                    }
                case PhoneRemoteCommands.KEYBOARD_KEY_CLICK:
                    {
                        if(string.IsNullOrWhiteSpace(commandParts[1]))
                        {
                            OnError?.Invoke(this, new IOException($"keyboard click data missing, message: {message}"));
                            return;
                        }

                        OnKeyboardKeyClick?.Invoke(this, commandParts[1]);
                        break;
                    }
                case PhoneRemoteCommands.KEYBOARD_KEY_PRESS:
                    {
                        if (string.IsNullOrWhiteSpace(commandParts[1]))
                        {
                            OnError?.Invoke(this, new IOException($"keyboard press data missing, message: {message}"));
                            return;
                        }

                        OnKeyboardKeyPress?.Invoke(this, commandParts[1]);
                        break;
                    }
                case PhoneRemoteCommands.KEYBOARD_KEY_RELEASE:
                    {
                        if (string.IsNullOrWhiteSpace(commandParts[1]))
                        {
                            OnError?.Invoke(this, new IOException($"keyboard release data missing, message: {message}"));
                            return;
                        }

                        OnKeyboardKeyRelease?.Invoke(this, commandParts[1]);
                        break;
                    }
                case PhoneRemoteCommands.KEYBOARD_KEY_STRING:
                    {
                        if (string.IsNullOrWhiteSpace(commandParts[1]))
                        {
                            OnError?.Invoke(this, new IOException($"keyboard string data missing, message: {message}"));
                            return;
                        }

                        OnKeyboardKeyString?.Invoke(this, commandParts[1]);
                        break;
                    }
                case PhoneRemoteCommands.INVALID_COMMAND:
                default:
                    {
                        OnError?.Invoke(this, new IOException($"invalid commandId, no command associated with ID: {commandId}, message: {message}"));
                        return;
                    }
            }
        }
    }



   
}

