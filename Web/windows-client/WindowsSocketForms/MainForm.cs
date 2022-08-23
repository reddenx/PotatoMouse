using QRCoder;
using SMT.Utilities.InputEvents.HardwareEvents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

        private readonly MessageHandler _handler;
        //private readonly HttpListener _listener;
        //private WebsocketClient _socket = null;
        //private Thread _connectThread;

        public MainForm()
        {
            InitializeComponent();

            _handler = new MessageHandler();
            label4.InvokeControl(l => l.Text = _handler.ScrollScale.ToString());
            label1.InvokeControl(l => l.Text = _handler.MoveScale.ToString());

            GenerateQrCode();

            var listener = new SiteConnectionHandler();
            listener.OnConnect += (o, e) => ClientIsConnected();
            listener.OnDisconnect += (o, e) => ClientDisconnected();
            listener.OnText += (o, msg) => _handler.RunCommand(msg);
            listener.Start();

            //_listener = new HttpListener();
            //_listener.Prefixes.Add("http://*:37075/");

            //_connectThread = new Thread(ListenForClientLoop) { IsBackground = true };
            //_connectThread.Start();
        }

        //private void ListenForClientLoop()
        //{
        //    if (_socket != null)
        //        return;

        //    _listener.Start();
        //    while (true)
        //    {
        //        var context = _listener.GetContextAsync().Result;
        //        if (context.Request.IsWebSocketRequest)
        //        {
        //            if (_socket != null)
        //            {
        //                context.Response.StatusCode = 400;
        //                context.Response.StatusDescription = "Connection Already Established";
        //                context.Response.Close();
        //                continue;
        //            }

        //            var socket = context.AcceptWebSocketAsync(null).Result;
        //            _socket = new WebsocketClient(socket.WebSocket);
        //            _socket.OnDisconnected += (o, e) => ClientDisconnected();
        //            _socket.OnMessageReceived += (o, e) => HandleMessageReceived(e);
        //            _socket.StartListening();

        //            ClientIsConnected();
        //        }
        //        else
        //        {
        //            using (var stream = new StreamWriter(context.Response.OutputStream))
        //            {
        //                stream.Write(HtmlTrash.GARBO);
        //            }
        //            context.Response.StatusCode = 200;
        //            context.Response.Close();
        //        }
        //    }
        //    _listener.Stop();
        //}

        private void GenerateQrCode()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ip = host.AddressList.FirstOrDefault(h => h.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            var data = QRCodeGenerator.GenerateQrCode($"http://{ip}:37075/#/{ip}", QRCodeGenerator.ECCLevel.L);
            var code = new QRCode(data);
            var image = code.GetGraphic(20);

            var bgImage = ImageGenerator.CloseResize(image, new Size(this.Width, this.Height));

            this.BackgroundImage = bgImage;
            HideControls();
        }

        private void ShowControls()
        {
            //yes I know this is stupid, I'm just being lazy and prototyping
            label1.InvokeControl(c => c.Visible = true);
            label2.InvokeControl(c => c.Visible = true);
            label3.InvokeControl(c => c.Visible = true);
            label4.InvokeControl(c => c.Visible = true);
            button1.InvokeControl(c => c.Visible = true);
            button3.InvokeControl(c => c.Visible = true);
            button4.InvokeControl(c => c.Visible = true);
            button5.InvokeControl(c => c.Visible = true);
        }
        private void HideControls()
        {
            label1.InvokeControl(c => c.Visible = false);
            label2.InvokeControl(c => c.Visible = false);
            label3.InvokeControl(c => c.Visible = false);
            label4.InvokeControl(c => c.Visible = false);
            button1.InvokeControl(c => c.Visible = false);
            button3.InvokeControl(c => c.Visible = false);
            button4.InvokeControl(c => c.Visible = false);
            button5.InvokeControl(c => c.Visible = false);
        }

        private void ClientIsConnected()
        {
            this.InvokeControl(f => f.BackgroundImage = null);
            ShowControls();
        }

        private void ClientDisconnected()
        {
            GenerateQrCode();
            //ListenForClientLoop();
            //_socket.Close();
            //_socket = null;
        }

        private void HandleMessageReceived(string msg)
        {
            _handler.RunCommand(msg);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_handler.MoveScale > 1)
                _handler.MoveScale -= 1;
            label1.InvokeControl(l => l.Text = _handler.MoveScale.ToString());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _handler.MoveScale += 1;
            label1.InvokeControl(l => l.Text = _handler.MoveScale.ToString());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_handler.ScrollScale > 1)
                _handler.ScrollScale -= 1;
            label4.InvokeControl(l => l.Text = _handler.ScrollScale.ToString());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _handler.ScrollScale += 1;
            label4.InvokeControl(l => l.Text = _handler.ScrollScale.ToString());
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
