using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using WindowsSocketCore;

namespace WindowsSocketForms
{
    class SiteConnectionHandler
    {
        public const int PORT = 37075;

        private TcpListener _listener = null;
        private Thread _acceptThread = null;
        private WebsocketClient _client = null;

        public event EventHandler<string> OnText;
        public event EventHandler OnDisconnect;
        public event EventHandler OnConnect;

        public SiteConnectionHandler()
        { }


        public void Start()
        {
            if (_listener == null)
            {
                _listener = new TcpListener(IPAddress.Any, PORT);

                var previousThread = _acceptThread;

                _acceptThread = new Thread(AcceptLoop) { IsBackground = true };
                _acceptThread.Start();

                if (previousThread != null)
                    previousThread.Abort();
            }
        }

        private void AcceptLoop()
        {
            _listener.Start();
            const int READ_BUFFER_SIZE = 1024 * 4;
            try
            {
                while (true)
                {
                    var client = _listener.AcceptTcpClient();
                    var stream = client.GetStream();
                    var buffer = new byte[READ_BUFFER_SIZE];
                    var readCount = stream.Read(buffer);
                    if (readCount == 0)
                    {
                        stream.Close();
                        continue;
                    }

                    var rawRequestStr = Encoding.UTF8.GetString(buffer, 0, readCount);

                    if (IsWebsocketUpgrade(rawRequestStr))
                    {
                        if (_client?.IsConnected != true) //is likely not connected
                        {
                            //handle connection
                            SendUpgradeMessage(rawRequestStr, stream);
                            SpinOffWebsocket(stream);
                            OnConnect?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            //return "already connected" error
                            var failureBuffer = Encoding.ASCII.GetBytes(HttpConsts.BAD_REQUEST);
                            stream.Write(failureBuffer);
                            stream.Close();
                        }
                    }
                    else if (IsWebsiteRequest(rawRequestStr))
                    {
                        //return the website
                        var websiteBuffer = Encoding.ASCII.GetBytes(HttpConsts.WEBSITE);
                        stream.Write(websiteBuffer);
                        stream.Close();
                    }
                    else
                    {
                        //return "dafuq" error
                        var failureBuffer = Encoding.ASCII.GetBytes(HttpConsts.BAD_REQUEST);
                        stream.Write(failureBuffer);
                        stream.Close();
                    }
                }
            }
            catch { }
            try
            {
                _listener.Stop();
            }
            catch { }
            _listener = null;

            try
            {
                Start();
            }
            catch (ThreadAbortException) { }
        }

        private bool IsWebsiteRequest(string rawRequestStr)
        {
            return rawRequestStr.StartsWith("GET")
                && !rawRequestStr.Contains("Upgrade: websocket");
        }

        private void SendUpgradeMessage(string rawRequestStr, NetworkStream stream)
        {
            //handle security header sillyness if it exists
            var secHeaderIndex = rawRequestStr.IndexOf(HttpConsts.SOCKET_SEC_KEY_HEADER);

            string response = null;
            if (secHeaderIndex != -1)
            {
                var key = rawRequestStr.Substring(
                    startIndex: secHeaderIndex + HttpConsts.SOCKET_SEC_KEY_HEADER.Length,
                    length: rawRequestStr.IndexOf("\r\n", secHeaderIndex) - (secHeaderIndex + HttpConsts.SOCKET_SEC_KEY_HEADER.Length));

                var responseKeyBytes = Encoding.UTF8.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11");
                byte[] hashedKeyBytes;
                using (var sha1 = System.Security.Cryptography.SHA1.Create())
                    hashedKeyBytes = sha1.ComputeHash(responseKeyBytes);
                var key64 = Convert.ToBase64String(hashedKeyBytes);
                response = $"{HttpConsts.UPGRADE_HEADERS}Sec-WebSocket-Accept: {key64}\r\n\r\n";
            }
            else
            {
                response += "\r\n";
            }

            var responseBuffer = Encoding.ASCII.GetBytes(response);
            stream.Write(responseBuffer);
        }

        private void SpinOffWebsocket(NetworkStream stream)
        {
            var websock = new WebsocketClient(WebSocket.CreateFromStream(stream, true, null, TimeSpan.FromSeconds(10)));
            websock.OnDisconnected += (s, e) => { this.OnDisconnect(this, EventArgs.Empty); };
            websock.OnMessageReceived += (s, e) => { this.OnText(this, e); };
            websock.OnDisconnected += (s, e) =>
            {
                _ = websock.Close();
                this.OnDisconnect(this, e);
            };
            websock.StartListening();
            _client = websock;
        }

        private bool IsWebsocketUpgrade(string rawRequestStr)
        {
            return rawRequestStr.StartsWith("GET") && rawRequestStr.Contains("Upgrade: websocket");
        }
    }
}
