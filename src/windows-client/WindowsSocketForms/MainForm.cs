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

        public MainForm()
        {
            InitializeComponent();

            _handler = new MessageHandler();
            label4.InvokeControl(l => l.Text = _handler.ScrollScale.ToString());
            label1.InvokeControl(l => l.Text = _handler.MoveScale.ToString());

            GenerateQrCode();

            var listener = new HttpConnectionListener();
            listener.OnRequest += (s, req, resp) => 
            {
                if(req.IsWebsocketUpgrade)
                    resp.WebsocketUpgrade();
                else if(req.IsWebsiteRequest)
                    resp.Resolve(200, HttpConsts.WEBSITE_BODY, HttpConsts.WEBSITE_HEADERS);
                else
                    resp.Resolve(404);
            };
            listener.OnWebsocket += (s, req, sock) => 
            {
                ClientIsConnected();
                
                sock.OnText += (s, msg) => 
                {
                    _handler.RunCommand(msg);
                };
                sock.OnDisconnected += (s, e) => 
                {
                    ClientDisconnected();
                };
                sock.OnBinary += (s, bytes) => { Console.WriteLine("got binary for some reason"); };
            };
            listener.Start(new IPEndPoint(IPAddress.Any, 37075));
        }

        private void GenerateQrCode()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ip = host.AddressList.FirstOrDefault(h => h.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            var data = QRCodeGenerator.GenerateQrCode($"http://{ip}:37075", QRCodeGenerator.ECCLevel.L);
            var code = new QRCode(data);
            var image = code.GetGraphic(20);

            // this.BackColor = Color.Red;
            this.BackgroundImage = image;
            this.BackgroundImageLayout = ImageLayout.Stretch;
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
