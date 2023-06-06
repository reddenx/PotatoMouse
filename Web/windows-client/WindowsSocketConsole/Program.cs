using System;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using WindowsSocketCore;

namespace WindowsSocketConsole
{
    class Program
    {
        static void Main(string[] args)
        {



            //var host = Dns.GetHostEntry(Dns.GetHostName());
            //var ip = host.AddressList.Single(h => h.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            //Console.WriteLine(ip);



            var listen = new System.Net.HttpListener();
            listen.Prefixes.Add("http://*:37070/");
            listen.Start();
            var context = listen.GetContext();
            var socketContext = context.AcceptWebSocketAsync(null).Result;
            Console.WriteLine("Connection Received");

            var socket = new WebsocketClient(socketContext.WebSocket);
            socket.OnDisconnected += (o, e) =>
            {
                Console.WriteLine("disconnected");
            };
            socket.OnMessageReceived += async (o, e) =>
            {
                Console.WriteLine(e);
                await socket.Send($"hi {e} I'm dad");
            };
            socket.StartListening();

            Console.ReadLine();

            //var socket = socketContext.WebSocket;
            //var recvBuffer = WebSocket.CreateClientBuffer(1024, 1024);

            //do
            //{
            //    var receipt = socket.ReceiveAsync(recvBuffer, CancellationToken.None).Result;
            //    if (receipt.MessageType == WebSocketMessageType.Close)
            //        break;

            //    var msg = ASCIIEncoding.ASCII.GetString(recvBuffer);
            //    Console.WriteLine($"TYPE: {receipt.MessageType} MSG: {msg}");
            //}
            //while (socket.State != WebSocketState.Closed);

            //Console.WriteLine("closed");
            //Console.ReadLine();
        }
    }
}
