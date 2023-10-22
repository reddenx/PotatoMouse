using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace WindowsSocketCore
{
    public interface IResponseHandler
    {
        bool IsResolved { get; }
        void Resolve(int statusCode, string body, KeyValuePair<string, string>[] headers = null);
        void WebsocketUpgrade(KeyValuePair<string, string>[] headers = null);
    }

    internal class ResponseHandler : IResponseHandler
    {
        public bool IsResolved { get; private set; }

        internal bool SpinOffWebRequest { get; private set; }

        private readonly Request _request;
        private readonly NetworkStream _stream;

        public ResponseHandler(Request request, NetworkStream stream)
        {
            _request = request;
            _stream = stream;
        }

        public void Resolve(int statusCode, string body, KeyValuePair<string, string>[] headers = null)
        {
#error you left off here
            throw new NotImplementedException();
        }

        public void WebsocketUpgrade(KeyValuePair<string, string>[] headers = null)
        {
            throw new NotImplementedException();
        }
    }
}