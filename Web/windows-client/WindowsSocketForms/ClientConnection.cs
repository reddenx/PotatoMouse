using WindowsSocketCore;

namespace WindowsSocketForms
{
    public interface IClientConnection
    {}

    public class ClientConnection : IClientConnection
    {
        private readonly IWebsocketConnection _connection;

        public ClientConnection(IWebsocketConnection connection)
        {
            _connection = connection;
        }
    }
}