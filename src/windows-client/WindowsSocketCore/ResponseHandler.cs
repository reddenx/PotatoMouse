using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace WindowsSocketCore
{
    public interface IResponseHandler
    {
        bool IsResolved { get; }
        void Resolve(int statusCode, string body = null, KeyValuePair<string, string>[] headers = null);
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
            IsResolved = true;
            SpinOffWebRequest = false;
            var response = BuildResult("1.0", statusCode, headers, body);

            _stream.Write(Encoding.UTF8.GetBytes(response));
            _stream.Close();
        }

        public void WebsocketUpgrade(KeyValuePair<string, string>[] headers = null)
        {
            IsResolved = true;
            SpinOffWebRequest = true;

            string securityResponseHeader = null;
            if(_request.Headers.Any(h => h.Key.ToUpper() == "SEC-WEBSOCKET-KEY"))
            {
                var securityHeader = _request.Headers.FirstOrDefault(h => h.Key.ToUpper() == "SEC-WEBSOCKET-KEY");
                var securityResponseBytes = Encoding.UTF8.GetBytes(securityHeader.Value + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11");
                byte[] hashedKeyBytes;
                using (var sha1 = System.Security.Cryptography.SHA1.Create())
                    hashedKeyBytes = sha1.ComputeHash(securityResponseBytes);
                securityResponseHeader = Convert.ToBase64String(hashedKeyBytes);
            }

            var outHeaders = new List<KeyValuePair<string, string>>(headers ?? new KeyValuePair<string, string>[] { });
            var finalHeaders = outHeaders.Where(h => h.Key.ToUpper() != "UPGRADE").Concat(new [] 
            {
                new KeyValuePair<string,string>("Upgrade", "websocket"),
                new KeyValuePair<string,string>("Connection", "Upgrade"),
            }).ToList();
            if(securityResponseHeader != null)
            {
                finalHeaders.Add(new KeyValuePair<string, string>("Sec-WebSocket-Accept", securityResponseHeader));
            }

            var result = BuildResult("1.1", 101, finalHeaders.ToArray(), null);
            _stream.Write(Encoding.UTF8.GetBytes(result));
        }


        private string BuildResult(string version, int code, KeyValuePair<string, string>[] headers, string body)
        {
            var headerBlock = string.Join("\r\n", headers?.Select(kvp => $"{kvp.Key}: {kvp.Value}").ToArray() ?? new string[] { });

            return
@$"HTTP/{version} {code} {ReadableCode(code)}
{headerBlock}

{body ?? ""}";
        }

        private string ReadableCode(int code)
        {
            switch (code)
            {
                case 101: return "Switching Protocols";
                case 200:
                case 204: return "OK";
                case 302: return "redirect";
                case 304: return "redirect";
                case 400: return "invalid input";
                case 401: return "forbidden";
                case 403: return "forbidden";
                case 404: return "not found";
                case 420: return "chill out bro";
                case 500: return "server error";
                case 504: return "gateway error";
                default: return "unknown";
            }
        }
    }
}