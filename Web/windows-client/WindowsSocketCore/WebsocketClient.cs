using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsSocketCore
{
    public class WebsocketClient
    {
        private readonly WebSocket _socket;
        private Thread _messageLoop;

        public event EventHandler<string> OnMessageReceived;
        public event EventHandler OnDisconnected;

        public bool IsListening { get; private set; } = false;
        public bool IsConnected => _socket.State == WebSocketState.Open;
        private bool _hasFiredDisconnectEvents = false;

        public WebsocketClient(WebSocket socket)
        {
            _socket = socket;
            _messageLoop = new Thread(MessageLoop) { IsBackground = true };
        }

        public void StartListening()
        {
            lock (this)
            {
                if (IsListening)
                    return;
                else
                    IsListening = true;
            }

            _messageLoop.Start();
        }

        public async Task Send(string message)
        {
            var bytes = ASCIIEncoding.ASCII.GetBytes(message);
            await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task Close()
        {
            //note: this is called internally from the end of the message loop

            switch (_socket.State)
            {
                case WebSocketState.Aborted:
                case WebSocketState.Closed:
                    //done do nothing
                    break;
                case WebSocketState.CloseReceived:
                case WebSocketState.CloseSent:
                    //in the process of closing, let the library handle the handshake
                    break;
                case WebSocketState.Connecting:
                case WebSocketState.None:
                case WebSocketState.Open:
                    //do the close
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    break;
            }


            lock (this)
            {
                if (!_hasFiredDisconnectEvents)
                    _hasFiredDisconnectEvents = true;
                else
                    return;
            }
            this.IsListening = false;
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }

        private void MessageLoop()
        {
            var buffer = WebSocket.CreateClientBuffer(1024, 1024);
            try
            {
                WebSocketReceiveResult msgMeta;
                do
                {
                    var msgTask = _socket.ReceiveAsync(buffer, CancellationToken.None);
                    msgMeta = msgTask.Result;

                    switch (msgMeta.MessageType)
                    {
                        case WebSocketMessageType.Binary:
                            Console.Error.WriteLine("received unexpected binary data");
                            break;
                        case WebSocketMessageType.Close:
                            try { OnDisconnected?.Invoke(this, EventArgs.Empty); } catch(Exception e) { Console.WriteLine(e); }
                            return;
                        case WebSocketMessageType.Text:
                            var message = ASCIIEncoding.ASCII.GetString(buffer.Array, 0, msgMeta.Count);
                            //to protect the loop from shitty user code
                            try { OnMessageReceived?.Invoke(this, message); } catch(Exception e) { Console.WriteLine(e); }
                            break;
                        default:
                            //???
                            break;
                    }

                } while (msgMeta.MessageType != WebSocketMessageType.Close);
            }
            catch(Exception e) { Console.WriteLine(e); }

            this.IsListening = false;
            this.Close().Wait();
        }


    }
}
