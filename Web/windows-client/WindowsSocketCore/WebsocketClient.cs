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
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }

        private void MessageLoop()
        {
            var buffer = WebSocket.CreateClientBuffer(1024, 1024);
            try
            {
                while (true)
                {
                    var msgTask = _socket.ReceiveAsync(buffer, CancellationToken.None);
                    var msgMeta = msgTask.Result;

                    switch (msgMeta.MessageType)
                    {
                        case WebSocketMessageType.Binary:
                            Console.Error.WriteLine("received unexpected binary data");
                            break;
                        case WebSocketMessageType.Close:
                            OnDisconnected?.Invoke(this, EventArgs.Empty);
                            return;
                        case WebSocketMessageType.Text:
                            var message = ASCIIEncoding.ASCII.GetString(buffer.Array, 0, msgMeta.Count);
                            OnMessageReceived?.Invoke(this, message);
                            break;
                        default:
                            //???
                            break;
                    }
                }
            }
            catch { }
            finally
            {
                this.IsListening = false;
            }
        }


    }
}
