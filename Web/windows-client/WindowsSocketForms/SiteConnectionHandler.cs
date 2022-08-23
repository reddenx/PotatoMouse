using System;
using System.Collections;
using System.Collections.Generic;
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

        public bool IsListening { get; private set; } = false;

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
                if (_acceptThread != null)
                    _acceptThread.Abort();
                _acceptThread = new Thread(AcceptLoop) { IsBackground = true };
                _acceptThread.Start();
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
                    var rawRequestStr = Encoding.UTF8.GetString(buffer, 0, readCount);

                    if (rawRequestStr.StartsWith("GET"))
                    {
                        if (rawRequestStr.Contains("Upgrade: websocket"))
                        {
                            if (_client == null)
                            {
                                //if it's an upgrade and we haven't accepted a client
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
                                _client = client;

                                if (_clientThread != null)
                                    _clientThread.Abort();

                                _clientThread = new Thread(ClientThreadLoop) { IsBackground = true };
                                _clientThread.Start();

                                OnConnect?.Invoke(this, EventArgs.Empty);
                            }
                            else
                            {
                                //if there's already a client send a failure
                                var failureBuffer = Encoding.ASCII.GetBytes(HttpConsts.BAD_REQUEST);
                                stream.Write(failureBuffer);
                                stream.Close();
                            }
                        }
                        else
                        {
                            //if it's a standard web request, send the website to connect
                            var websiteBuffer = Encoding.ASCII.GetBytes(HttpConsts.WEBSITE);
                            stream.Write(websiteBuffer);
                            stream.Close();
                        }
                    }
                    else
                    {
                        //if it's not a GET wtf you doin?
                        var failureBuffer = Encoding.ASCII.GetBytes(HttpConsts.BAD_REQUEST);
                        stream.Write(failureBuffer);
                        stream.Close();
                    }
                }
            }
            catch { }
            _listener.Stop();
        }

        private void ClientThreadLoop()
        {
            try
            {
                var stream = _client.GetStream();
                while (true)
                {
                    var frameInfo = ReadFrame(stream);
                    var buffer = new byte[frameInfo.PayloadLength];
                    var readByteCount = stream.Read(buffer, 0, buffer.Length);
                    if (readByteCount == 0)
                        break;


                    if (frameInfo.Masked)
                    {
                        var unmaskedBuffer = new byte[buffer.Length];
                        for (int i = 0; i < buffer.Length; ++i)
                        {
                            unmaskedBuffer[i] = (byte)(buffer[i] ^ frameInfo.MaskKey[i % 4]);
                        }
                        buffer = unmaskedBuffer;
                    }

                    HandleMessage(frameInfo, buffer);
                }
            }
            catch { }

            try { _client.Close(); }
            catch { }

            OnDisconnect?.Invoke(this, EventArgs.Empty);

            _client = null;
            _clientThread = null;
        }

        private void HandleMessage(Frame frameInfo, byte[] buffer)
        {
            var type = frameInfo.OpCode;
            switch (type)
            {
                case 0x0:
                    //multipart message
                    throw new NotImplementedException("yeah I didn't get to multipart frames");
                    break;
                case 0x1:
                    //text
                    OnText?.Invoke(this, Encoding.UTF8.GetString(buffer));
                    break;
                case 0x2:
                    //binary
                    throw new NotImplementedException("didn't get to binary message handling");
                    break;
                case 0x8:
                    throw new NotImplementedException("This is definitely the wrong way to close this thread");
                    //close
                    break;
                case 0x9:
                    //ping
                    throw new NotImplementedException("TODO actually implement ping");
                    break;
                case 0xa:
                    //pong
                    throw new NotImplementedException("TODO actually implement pong");
                    break;
                default:
                    throw new NotImplementedException("unknown type");
            }
        }

        class Frame
        {
            public bool Fin { get; internal set; }
            public bool Rsv1 { get; internal set; }
            public bool Rsv2 { get; internal set; }
            public bool Rsv3 { get; internal set; }
            public int OpCode { get; internal set; }
            public bool Masked { get; internal set; }
            public int PayloadLength { get; internal set; }
            public int PayloadLengthBytesCount { get; internal set; }
            public byte[] MaskKey { get; internal set; }
        }

        //https://programmer.group/c-implement-websocket-server-02-message-frame-analysis-and-code-implementation.html
        private Frame ReadFrame(NetworkStream stream)
        {
            var buffer = new byte[2];
            var readByteCount = stream.Read(buffer, 0, 2);
            if (readByteCount == 0)
                throw new Exception("this socket is gone baby!");

            var frame = new Frame();

            //Process first byte
            //The first bit, if 1, represents that the frame is the end frame
            frame.Fin = buffer[0] >> 7 == 1;

            //Three reserved seats, we don't need them
            frame.Rsv1 = (buffer[0] >> 6 & 1) == 1;
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
            {
                frame.PayloadLength = payloadLengthMask;
            }
            //126 means that the following two bytes save the Payload length
            else if (payloadLengthMask == 126)
            {
                frame.PayloadLengthBytesCount = 2;

            }
            //126 means that the following 8 bytes save the Payload length. Yes, there are no 4 bytes.
            else if (payloadLengthMask == 127)
            {
                frame.PayloadLengthBytesCount = 8;

            }

            //If there is no mask and no additional bytes are required to determine the Payload length, it is returned directly
            //Later, just read the Payload according to the Payload length
            if (!frame.Masked && frame.PayloadLengthBytesCount == 0)
            {
                return frame;
            }

            //Read out 2 or 8 bytes of the saved length
            //If there is a mask, you need to continue reading the 4-byte mask
            buffer = frame.Masked
                ? new byte[frame.PayloadLengthBytesCount + 4]
                : new byte[frame.PayloadLengthBytesCount];

            //Read Payload length data and mask (if any)
            stream.Read(buffer, 0, buffer.Length);

            //If there is a mask, extract it
            if (frame.Masked)
            {
                frame.MaskKey = buffer.Skip(frame.PayloadLengthBytesCount).Take(4).ToArray();
            }

            //Get the length of the Payload from the byte data
            if (frame.PayloadLengthBytesCount == 2)
            {
                frame.PayloadLength = buffer[0] << 8 | buffer[1];

            }
            else if (frame.PayloadLengthBytesCount == 8)
            {
                throw new NotImplementedException();
                //frame.PayloadLength = new Int64() Convert.ToInt64( ToInt64(buffer);
            }

            //So far, all data representing frame element information are read out
            //We will read the Payload data in a stream
            //For some special frames, the Payload will also have a specific data format, which will be introduced separately later

            return frame;
        }

        static T[] Slice<T>(T[] array, int index, int length)
        {
            var stuff = new T[length];
            for (int i = 0; i < length; ++i)
            {
                stuff[i] = array[index + i];
            }
            return stuff;
        }
    }
}
