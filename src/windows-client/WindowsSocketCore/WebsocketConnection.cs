using System;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsSocketCore
{
    public interface IWebsocketConnection
    {
        event EventHandler<string> OnText;
        event EventHandler<byte[]> OnBinary;
        event EventHandler OnDisconnected;

        Task Send(string message);
        Task Send(byte[] data);
        Task Close();
    }

    public class WebsocketConnection : IWebsocketConnection
    {
        public event EventHandler<string> OnText;
        public event EventHandler<byte[]> OnBinary;
        public event EventHandler OnDisconnected;
        private bool _hasFiredDisconnectEvents = false;

        public bool IsConnected => _client.State == WebSocketState.Open;

        private readonly Request _initialRequest;
        private readonly WebSocket _client;
        private readonly Thread _messageLoop;

        public WebsocketConnection(Request initialRequest, WebSocket client)
        {
            _initialRequest = initialRequest;
            _client = client;
            _messageLoop = new Thread(MessageLoop) { IsBackground = true };
            _messageLoop.Start();
        }

        public async Task Close()
        {
            //note: this is called internally from the end of the message loop

            switch (_client.State)
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
                    await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    break;
            }


            lock (this)
            {
                if (!_hasFiredDisconnectEvents)
                    _hasFiredDisconnectEvents = true;
                else
                    return;
            }
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }

        public async Task Send(string message)
        {
            var bytes = Encoding.ASCII.GetBytes(message);
            await Send(bytes);
        }

        public async Task Send(byte[] data)
        {
            await _client.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private void MessageLoop()
        {
            var buffer = WebSocket.CreateClientBuffer(1024, 1024);
            try
            {
                WebSocketReceiveResult msgMeta;
                do
                {
                    var msgTask = _client.ReceiveAsync(buffer, CancellationToken.None);
                    msgMeta = msgTask.Result;

                    switch (msgMeta.MessageType)
                    {
                        case WebSocketMessageType.Binary:
                            var bytes = new ArraySegment<byte>(buffer.Array, 0, msgMeta.Count);
                            try { OnBinary?.Invoke(this, bytes.Array); } catch(Exception e) { Console.WriteLine(e); }
                            break;
                        case WebSocketMessageType.Close:
                            try { OnDisconnected?.Invoke(this, EventArgs.Empty); } catch(Exception e) { Console.WriteLine(e); }
                            return;
                        case WebSocketMessageType.Text:
                            var message = Encoding.ASCII.GetString(buffer.Array, 0, msgMeta.Count);
                            //to protect the loop from shitty user code
                            try { OnText?.Invoke(this, message); } catch(Exception e) { Console.WriteLine(e); }
                            break;
                        default:
                            //???
                            break;
                    }

                } while (msgMeta.MessageType != WebSocketMessageType.Close);
            }
            catch(Exception e) 
            {
                Console.WriteLine(e);
            }
            this.Close().Wait();
        }
    }
}