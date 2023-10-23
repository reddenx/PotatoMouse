using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace WindowsSocketCore
{
    public delegate void RequestHandler(object sender, Request request, IResponseHandler response);
    public delegate void WebsocketHandler(object sender, Request request, IWebsocketConnection socket);
    public interface IHttpConnectionListener
    {
        event RequestHandler OnRequest;
        event WebsocketHandler OnWebsocket;

        void Start(IPEndPoint localEndpoint);
        void Stop();
    }

    public class HttpConnectionListener : IHttpConnectionListener
    {
        private TcpListener _listener = null;
        private Thread _acceptThread = null;
        public bool IsListening => _listener != null;

        public event RequestHandler OnRequest;
        public event WebsocketHandler OnWebsocket;

        public HttpConnectionListener() { }

        public void Start(IPEndPoint localEndpoint)
        {
            if (_listener != null)
                return;

            try
            {
                _listener = new TcpListener(localEndpoint);
                _listener.Start();
                StartAcceptLoop();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                _listener = null;
                KillAcceptLoop();
            }
        }

        public void Stop()
        {
            if (_listener == null)
                return;

            var temp = _listener;
            _listener = null;

            temp.Stop();
            KillAcceptLoop();
        }

        private void KillAcceptLoop()
        {
            try { _acceptThread.Abort(); }
            catch(Exception e) { Console.WriteLine(2); }
        }

        private void StartAcceptLoop()
        {
            _acceptThread = new Thread(AcceptLoop) { IsBackground = true };
            _acceptThread.Start();
        }

        private int _acceptLoopReEntry = 0;
        private const int ACCEPT_LOOP_ALLOWED_REENTRY = 10;
        private const int READ_BUFFER_SIZE = 1024 * 4;
        private void AcceptLoop()
        {
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
                    else
                    {
                        _acceptLoopReEntry = 0; //might as well reset the failure count, stackoverflow could happen legitimately eventually, a later todo could make this iterative rather than recursive
                    }

                    var rawRequestStr = Encoding.UTF8.GetString(buffer, 0, readCount);

                    try
                    {
                        var request = Request.Parse(rawRequestStr);
                        var handler = new ResponseHandler(request, stream);
                        this.OnRequest(this, request, handler);
                        if (!handler.IsResolved)
                        {
                            handler.Resolve(500, "server did not handle request");
                        }
                        else if (handler.SpinOffWebRequest)
                        {
                            //todo parse out subprotocol from initial request headers
                            var sock = new WebsocketConnection(request, WebSocket.CreateFromStream(stream, true, null, TimeSpan.FromSeconds(10)));
                            this.OnWebsocket(this, request, sock);
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e);
                        try { stream.Close(); } catch(Exception e2) { Console.WriteLine(e2); }
                    }
                }
            }
            catch(Exception e) { Console.WriteLine(e); }

            if (_acceptLoopReEntry < ACCEPT_LOOP_ALLOWED_REENTRY)
            {
                _acceptLoopReEntry++;
                AcceptLoop();
            }
            else
                Stop();
        }
    }
}