using QRCoder;
using SMT.Utilities.InputEvents.HardwareEvents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsSocketCore;

namespace WindowsSocketForms
{
    public partial class MainForm : Form
    {
        /**
         * desired behaviors:
         * - display qr code for client to connect
         * - when a client is connected, show messages and hide qr code
         * - one client connects, any later ones rejected
         * - when client disconnects, show qr code again
         * */

        private readonly MouseEventRunner _mouse;
        private readonly KeyboardEventRunner _keyboard;
        private readonly HttpListener _listener;
        private WebsocketClient _socket = null;
        private Thread _connectThread;

        public MainForm()
        {
            InitializeComponent();

            _mouse = new MouseEventRunner();
            _keyboard = new KeyboardEventRunner();

            GenerateQrCode();
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:37075/");

            _connectThread = new Thread(ListenForClientLoop) { IsBackground = true };
            _connectThread.Start();
        }

        private void ListenForClientLoop()
        {
            if (_socket != null)
                return;

            _listener.Start();
            while (true)
            {
                var context = _listener.GetContextAsync().Result;
                if (_socket != null)
                {
                    context.Response.StatusCode = 400;
                    context.Response.StatusDescription = "Connection Already Established";
                    context.Response.Close();
                    continue;
                }

                var socket = context.AcceptWebSocketAsync(null).Result;
                _socket = new WebsocketClient(socket.WebSocket);
                _socket.OnDisconnected += (o,e) => ClientDisconnected();
                _socket.OnMessageReceived += (o, e) => HandleMessageReceived(e);
                _socket.StartListening();

                ClientIsConnected();
            }
            _listener.Stop();
        }

        private void GenerateQrCode()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ip = host.AddressList.FirstOrDefault(h => h.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            var data = QRCodeGenerator.GenerateQrCode(ip.ToString(), QRCodeGenerator.ECCLevel.L);
            var code = new QRCode(data);
            var image = code.GetGraphic(20);

            var bgImage = ImageGenerator.CloseResize(image, new Size(this.Width, this.Height));

            this.BackgroundImage = bgImage;
        }

        private void ClientIsConnected()
        {
            this.InvokeControl(f => f.BackgroundImage = null);
        }

        private void ClientDisconnected()
        {
            GenerateQrCode();
            ListenForClientLoop();
            _socket.Close();
            _socket = null;
        }

        private void HandleMessageReceived(string msg)
        {
            
        }
    }

    internal static class ControlExtensions
    {
        public static void InvokeControl<TControl>(this TControl control, Action<TControl> action)
            where TControl : Control
        {
            if (control.InvokeRequired)
            {
                control.Invoke((Action)(() => InvokeControl(control, action)));
            }
            else
            {
                action?.Invoke(control);
            }
        }
    }
}
