using PR.Emulation.Mouse;
using PR.Networking;
using SMT.Utilities.InputEvents.HardwareEvents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PR.Presentation.TargetClient
{
    public partial class MainForm : Form
    {
        private const int IDLE_TIME_MILLI = 1000 * 30;

        private readonly Receiver Receiver;
        private readonly Mousepad Mousepad;
        private readonly KeyboardEventRunner Keyboard;
        private readonly System.Threading.Timer IdleTimer;

        private readonly ClientBroadcastSignaler Broadcaster;

        private IdleStates CurrentIdleState;

        public MainForm()
        {
            InitializeComponent();

            CurrentIdleState = IdleStates.Unset;

            Mousepad = new Mousepad();
            Keyboard = new KeyboardEventRunner();

            Broadcaster = new ClientBroadcastSignaler(37015);

            Receiver = new Receiver(37015);
            Receiver.Start();
            Receiver.OnError += Receiver_OnError;

            Receiver.OnMouseMove += Receiver_OnMouseMove;
            Receiver.OnMouseLeftClick += Receiver_OnMouseLeftClick;
            Receiver.OnMouseRightClick += Receiver_OnMouseRightClick;

            Receiver.OnMouseSignalReceived += Receiver_OnMouseSignalReceived;
            IdleTimer = new System.Threading.Timer(HandleIdleTimer);
            IdleTimer.Change(0, IDLE_TIME_MILLI);
        }

        

        private void Receiver_OnMouseRightClick(object sender, EventArgs e)
        {
            Mousepad.RightClick();
        }

        private void Receiver_OnMouseLeftClick(object sender, EventArgs e)
        {
            Mousepad.LeftClick();
        }

        private void Receiver_OnMouseMove(object sender, Point e)
        {
            Mousepad.MoveMouse(e.X, e.Y);
        }

        private void Receiver_OnError(object sender, Exception e)
        {
            this.MessageTextBox.InvokeControl(t => t.Text = $"{e.Message}\r\n{t.Text}");
        }

        private void Receiver_OnMouseSignalReceived(object sender, string e)
        {
            this.MessageTextBox.InvokeControl(t => t.Text = $"{e}\r\n{t.Text}");

            this.StatusImage.InvokeControl(img => img.BackColor = Color.Green);
            this.StatusLabel.InvokeControl(label => label.Text = "Active");
            CurrentIdleState = IdleStates.Active;
            Broadcaster.Pause(30);
        }

        private void HandleIdleTimer(object state)
        {
            switch (CurrentIdleState)
            {
                case IdleStates.Unset:
                    this.StatusImage.InvokeControl(img => img.BackColor = Color.Gray);
                    this.StatusLabel.InvokeControl(label => label.Text = "No Activity");
                    Broadcaster.Start();
                    break;
                case IdleStates.Active:
                    this.StatusImage.InvokeControl(img => img.BackColor = Color.Green);
                    this.StatusLabel.InvokeControl(label => label.Text = "Active");
                    break;

                case IdleStates.Inactive:
                    break;
                case IdleStates.Idle:
                    break;
            }
        }
    }
}
