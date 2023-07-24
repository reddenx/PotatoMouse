using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WindowsSocketForms
{
    class SiteConnectionHandler
    {
        public const int PORT = 37075;

        private TcpListener _listener = null;
        private Thread _acceptThread = null;
        private TcpClient _client = null;
        private Thread _clientThread = null;

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
                        if (_client == null)
                        {
                            //handle connection
                            HandleUpgradeHandshake(rawRequestStr, stream, client);
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

        private void HandleUpgradeHandshake(string rawRequestStr, NetworkStream stream, TcpClient client)
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

            //kickoff the client thread
            _client = client;

            if (_clientThread != null)
                _clientThread.Abort();

            _clientThread = new Thread(ClientThreadLoop) { IsBackground = true };
            _clientThread.Start();
        }

        private bool IsWebsocketUpgrade(string rawRequestStr)
        {
            return rawRequestStr.StartsWith("GET") && rawRequestStr.Contains("Upgrade: websocket");
        }

        private void ClientThreadLoop()
        {
            try
            {
                var stream = _client.GetStream();
                while (true)
                {
                    var frame = ReadFrame(stream);

                    if (!frame.Fin)
                    {
                        var frameList = new List<Frame>(new[] { frame });
                        Frame nextFrame;
                        do
                        {
                            nextFrame = ReadFrame(stream);
                            frameList.Add(nextFrame);
                        }
                        while (nextFrame.Fin || nextFrame.GetFrameType() != Frame.FrameTypes.Multipart);

                        //HandleMultiMessage(frameList);
                        new NotImplementedException("TODO: handling of multipart messages still not implemented");
                    }
                    else if (frame.GetFrameType() == Frame.FrameTypes.Close)
                    {
                        //TODO: send close message, this is an invalid way to close a websocket as a server
                        continue;
                    }
                    else
                    {
                        HandleMessage(frame);
                    }
                }
            }
            catch { }

            //TODO: send close message, this is an invalid way to close a websocket as a server
            try { _client.Close(); }
            catch { }

            _client = null;
            _clientThread = null;

            OnDisconnect?.Invoke(this, EventArgs.Empty);
        }

        private void HandleMessage(Frame frame)
        {
            switch (frame.GetFrameType())
            {
                case Frame.FrameTypes.Multipart:
                    throw new IOException("invalid setup, multipart message unexpected");
                case Frame.FrameTypes.Text:
                    var decodedText = Encoding.UTF8.GetString(frame.Data);
                    OnText?.Invoke(this, decodedText);
                    break;
                case Frame.FrameTypes.Binary:
                    //OnBinary?.Invoke(this, frame.Data);
                    break;
                case Frame.FrameTypes.Ping:
                    SendPong();
                    break;
                case Frame.FrameTypes.Pong:
                    //HandlePong();
                    break;
                case Frame.FrameTypes.Close:
                    throw new InvalidOperationException("unexpected handling of close message");
                case Frame.FrameTypes.UnknownInvalid:
                default:
                    throw new IOException("invalid opcode detected");
            }
        }

        private void SendPong()
        {
        }

        class Frame
        {
            public byte[] Data { get; set; }

            public bool Fin { get; internal set; }
            public bool Rsv1 { get; internal set; }
            public bool Rsv2 { get; internal set; }
            public bool Rsv3 { get; internal set; }
            public int OpCode { get; internal set; }
            public bool Masked { get; internal set; }
            public int PayloadLength { get; internal set; }
            public int PayloadLengthBytesCount { get; internal set; }
            public byte[] MaskKey { get; internal set; }

            public enum FrameTypes
            {
                Multipart,
                Text,
                Binary,
                Close,
                Ping,
                Pong,
                UnknownInvalid
            }


            public FrameTypes GetFrameType()
            {
                switch (this.OpCode)
                {
                    case 0x0: return Frame.FrameTypes.Multipart;
                    case 0x1: return Frame.FrameTypes.Text;
                    case 0x2: return Frame.FrameTypes.Binary;
                    case 0x8: return Frame.FrameTypes.Close;
                    case 0x9: return Frame.FrameTypes.Ping;
                    case 0xa: return Frame.FrameTypes.Pong;
                    default: return Frame.FrameTypes.UnknownInvalid;
                }
            }
        }

        //https://programmer.group/c-implement-websocket-server-02-message-frame-analysis-and-code-implementation.html
        private Frame ReadFrame(NetworkStream stream)
        {
            var buffer = new byte[2];
            try
            {
                var readByteCount = stream.Read(buffer, 0, 2);
                if (readByteCount == 0)
                    return null;
            }
            catch
            {
                return null;
            }

            var frame = new Frame();

            //Process first byte
            //The first bit, if 1, represents that the frame is the end frame
            frame.Fin = buffer[0] >> 7 == 1;

            //Three reserved seats, we don't need them
            frame.Rsv1 = (buffer[0] >> 6 & 1) == 1; //method: shift n bits, then and with 1 to only grab the first bit, allowing single bit targeting
            frame.Rsv2 = (buffer[0] >> 5 & 1) == 1;
            frame.Rsv3 = (buffer[0] >> 4 & 1) == 1;

            //5-8 bits, representing frame type
            frame.OpCode = (buffer[0] & 0xf); //00001111

            //Process second byte
            //The first bit, if 1, represents that the Payload has been masked
            frame.Masked = buffer[1] >> 7 == 1;

            //2-7 bits, Payload length identification
            int payloadLengthMask = buffer[1] & 0x7f; //01111111

            //If the value is less than 126, this value represents the actual length of the Payload
            if (payloadLengthMask < 126)
                frame.PayloadLength = payloadLengthMask;

            //126 means that the following two bytes save the Payload length
            else if (payloadLengthMask == 126)
                frame.PayloadLengthBytesCount = 2;

            //126 means that the following 8 bytes save the Payload length. Yes, there are no 4 bytes.
            else if (payloadLengthMask == 127)
                frame.PayloadLengthBytesCount = 8;

            //If there is no mask and no additional bytes are required to determine the Payload length, it is returned directly
            if (frame.Masked || frame.PayloadLengthBytesCount == 0)
            {
                //Read out 2 or 8 bytes of the saved length
                //If there is a mask, you need to continue reading the 4-byte mask
                buffer = frame.Masked
                    ? new byte[frame.PayloadLengthBytesCount + 4]
                    : new byte[frame.PayloadLengthBytesCount];

                //Read Payload length data and mask (if any)
                stream.Read(buffer, 0, buffer.Length);

                //If there is a mask, extract it
                if (frame.Masked)
                    frame.MaskKey = buffer.Skip(frame.PayloadLengthBytesCount).Take(4).ToArray();

                //Get the length of the Payload from the byte data
                if (frame.PayloadLengthBytesCount == 2)
                    frame.PayloadLength = buffer[0] << 8 | buffer[1];
                else if (frame.PayloadLengthBytesCount == 8)
                    throw new IOException("payload too big");
                //I REAAAALLY don't wanna deal with big int conversions, screw this
                //frame.PayloadLength = new Int64() Convert.ToInt64( ToInt64(buffer);
            }

            if (frame.PayloadLength > 0)
            {
                var payloadBuffer = new byte[frame.PayloadLength];
                var readByteCount = stream.Read(payloadBuffer, 0, payloadBuffer.Length);

                if (frame.Masked)
                {
                    var unmaskedData = new byte[payloadBuffer.Length];
                    for (int i = 0; i < payloadBuffer.Length; ++i)
                    {
                        unmaskedData[i] = (byte)(payloadBuffer[i] ^ frame.MaskKey[i % 4]);
                    }
                    frame.Data = unmaskedData;
                }
                else
                {
                    frame.Data = payloadBuffer;
                }
            }

            return frame;
        }
    }
}
