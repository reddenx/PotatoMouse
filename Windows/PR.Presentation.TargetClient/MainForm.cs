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
        private const int IDLE_LOOP_TIME_MILLI = 1000;

        private readonly Thread IdleLoopThread;
        private readonly Receiver Receiver;
        private readonly Mousepad Mousepad;
        private readonly KeyboardEventRunner Keyboard;

        private DateTime LastMessage;

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

            Receiver.OnMouseMove += (o, e) => { Mousepad.MoveMouse(e.X, e.Y); };
            Receiver.OnMouseLeftClick += (o, e) => { Mousepad.LeftClick(); };
            Receiver.OnMouseRightClick += (o, e) => { Mousepad.RightClick(); };
            Receiver.OnMouseDoubleLeftClick += (o, e) => { Mousepad.LeftDoubleClick(); };
            Receiver.OnMouseLeftPress += (o, e) => { Mousepad.LeftDown(); };
            Receiver.OnMouseLeftRelease += (o, e) => { Mousepad.LeftUp(); };
            Receiver.OnScrollwheelMove += (o, e) =>
            {
                if (e > 0) Mousepad.ScrollDown();
                else Mousepad.ScrollUp();
            };

            Receiver.OnMouseSignalReceived += Receiver_OnMouseSignalReceived;
            Receiver.OnKeyboardKeyClick += Receiver_OnKeyboardKeyClick;

            LastMessage = DateTime.Now;
            IdleLoopThread = new Thread(new ThreadStart(IdleTimerLoop));
            IdleLoopThread.IsBackground = true;
            IdleLoopThread.Start();

            HandleIdleStateChange();
        }

        private void Receiver_OnKeyboardKeyClick(object sender, string e)
        {
            //TODO currently only supports the two media buttons called out, should move to emulation project with full keyboard support
            switch (e)
            {
                case "media-play-pause":
                    PressKey(0xB3, 0);
                    break;
                case "media-next":
                    PressKey(0xB0, 0);
                    break;
                default:
                    LogError($"unknown key command {e}");
                    break;
            }
        }

        private void PressKey(int vk, int sk)
        {
            Parallelism.Fork(() =>
            {
                Keyboard.DoEvent(new KeyboardEventArgs(vk, sk, true));
                Thread.Sleep(100);
                Keyboard.DoEvent(new KeyboardEventArgs(vk, sk, false));
            });
        }

        private void Receiver_OnError(object sender, Exception e)
        {
            LogError(e.Message);
        }

        private void LogError(string errorMessage)
        {
            this.MessageTextBox.InvokeControl(t => t.Text = $"{errorMessage}\r\n{t.Text}");
        }

        private void Receiver_OnMouseSignalReceived(object sender, string e)
        {
            if (this.MessageTextBox.Text.Length > 1000)
                this.MessageTextBox.InvokeControl(t => t.Text = "");

            this.MessageTextBox.InvokeControl(t => t.Text = $"{e}\r\n{t.Text}");
            LastMessage = DateTime.Now;

            if (CurrentIdleState != IdleStates.Active)
            {
                CurrentIdleState = IdleStates.Active;
                HandleIdleStateChange();
            }
            Broadcaster.Pause(30);
        }

        private void IdleTimerLoop()
        {
            while (true)
            {
                if (LastMessage.AddMilliseconds(IDLE_TIME_MILLI) < DateTime.Now)
                {
                    CurrentIdleState = IdleStates.Idle;
                    HandleIdleStateChange();
                }
                Thread.Sleep(IDLE_LOOP_TIME_MILLI);
            }
        }

        private void HandleIdleStateChange()
        {
            switch (CurrentIdleState)
            {
                case IdleStates.Unset:
                    CurrentIdleState = IdleStates.Inactive;
                    HandleIdleStateChange();
                    break;
                case IdleStates.Active:
                    this.StatusImage.InvokeControl(img => img.BackColor = Color.Green);
                    this.StatusLabel.InvokeControl(label => label.Text = "Active");
                    Broadcaster.Stop();
                    break;

                case IdleStates.Inactive:
                    this.StatusImage.InvokeControl(img => img.BackColor = Color.Gray);
                    this.StatusLabel.InvokeControl(label => label.Text = "Searching");
                    Broadcaster.Start();
                    break;

                case IdleStates.Idle:
                    this.StatusImage.InvokeControl(img => img.BackColor = Color.Yellow);
                    this.StatusLabel.InvokeControl(label => label.Text = "Idle, Searching");
                    Broadcaster.Start();
                    break;
            }
        }
    }
}
