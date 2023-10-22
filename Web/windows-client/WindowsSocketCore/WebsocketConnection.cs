using System;
using System.Net.Sockets;
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
        
        private readonly Request _initialRequest;
        private readonly TcpClient _client;

        public WebsocketConnection(Request initialRequest, TcpClient client)
        {
            _initialRequest = initialRequest;
            _client = client;
        }

        public Task Close()
        {
            #error this is incomplete
            throw new NotImplementedException();
        }

        public Task Send(string message)
        {
            throw new NotImplementedException();
        }

        public Task Send(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}