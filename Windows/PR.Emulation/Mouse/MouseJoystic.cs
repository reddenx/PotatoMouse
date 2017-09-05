using SMT.Utilities.InputEvents.HardwareEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PR.Emulation.Mouse
{
    //runs an internal loop to cause mouse movement based on joystic input
    public class MouseJoystic : IDisposable
    {
        private const float DEAD_ZONE_X = 0f;
        private const float DEAD_ZONE_Y = 0f;
        private const int CLICK_DOWN_MILLI = 200;
        private const int DOUBLE_CLICK_DOWN_MILLI = 100;
        private const int LOOP_WAIT_TIME = 20;
        private const float CALIBRATION = 1f;

        private uint TotalElapsedMilli;

        private float XCarry; //carryover from last update for x
        private float YCarry; //carryover from last update for y

        private readonly MouseEventRunner MouseRunner;

        private JoysticState CurrentState;
        private readonly Thread MoveLoopThread;
        private bool Disposed;

        private List<ScheduledMouseEvent> ScheduledEvents;

        public MouseJoystic()
        {
            MouseRunner = new MouseEventRunner();
            Disposed = false;
            ScheduledEvents = new List<ScheduledMouseEvent>();

            XCarry = 0f;
            YCarry = 0f;
            TotalElapsedMilli = 0;

            CurrentState = new JoysticState() { X = 0, Y = 0 };

            MoveLoopThread = new Thread(new ThreadStart(MoveLoop));
            MoveLoopThread.IsBackground = true;
            MoveLoopThread.Start();
        }

        //background loop, runs commands and updates the elapsed time for the thread timer
        private void MoveLoop()
        {
            while (!Disposed)
            {
                Thread.Sleep(LOOP_WAIT_TIME);
                MoveMouseFromState();
                HandleFollowupCommands();
                TotalElapsedMilli += LOOP_WAIT_TIME;
            }
        }

        //handled scheduled events, probably gonna run everything through this loop, make it easier to diagnose problems if the thread can be isolated
        private void HandleFollowupCommands()
        {
            var eventsToRun = new ScheduledMouseEvent[0];
            lock (ScheduledEvents)
            {
                eventsToRun = ScheduledEvents.Where(e => e.FireTime < TotalElapsedMilli).OrderBy(e => e.FireTime).ToArray();
                foreach (var e in eventsToRun)
                {
                    ScheduledEvents.Remove(e);
                }
            }

            foreach (var e in eventsToRun)
            {
#if DEBUG
                Console.WriteLine($"event: {e.FireTime}");
#endif

                MouseRunner.DoEvent(e.Command);
            }
        }

        //moves the mouse based on the current joystic state and refresh loop speed
        private void MoveMouseFromState()
        {
            //setup some temps
            var x = 0f;
            var y = 0f;

            //determine raw input for this update loop
            if (Math.Abs(CurrentState.X) >= DEAD_ZONE_X)
                x = CurrentState.X * LOOP_WAIT_TIME * CALIBRATION;

            if (Math.Abs(CurrentState.Y) >= DEAD_ZONE_Y)
                y = CurrentState.Y * LOOP_WAIT_TIME * CALIBRATION;

            //add previous carryover
            x += XCarry;
            y += YCarry;

            //round to ints
            var moveX = (int)Math.Round(x);
            var moveY = (int)Math.Round(y);

            //set carryover
            XCarry = x - (float)moveX;
            YCarry = y - (float)moveY;

            //make sure we're even moving
            if (Math.Abs(moveX) + Math.Abs(moveY) > 0)
            {
                MouseRunner.DoEvent(MouseEventArgs.Move(moveX, moveY));
            }
        }

        public void SetJoysticState(JoysticState joystic)
        {
            CurrentState = joystic;
        }

        public void LeftClick()
        {
            lock (ScheduledEvents)
            {
                ScheduledEvents.Add(new ScheduledMouseEvent()
                {
                    Command = MouseEventArgs.LeftDown(),
                    FireTime = 0,
                });
                ScheduledEvents.Add(new ScheduledMouseEvent()
                {
                    Command = MouseEventArgs.LeftUp(),
                    FireTime = TotalElapsedMilli + CLICK_DOWN_MILLI,
                });
            }
        }

        public void RightClick()
        {
            lock (ScheduledEvents)
            {
                ScheduledEvents.Add(new ScheduledMouseEvent()
                {
                    Command = MouseEventArgs.RightDown(),
                    FireTime = 0,
                });
                ScheduledEvents.Add(new ScheduledMouseEvent()
                {
                    Command = MouseEventArgs.RightUp(),
                    FireTime = TotalElapsedMilli + CLICK_DOWN_MILLI,
                });
            }
        }

        //left click, series of 4 events
        public void LeftDoubleClick()
        {
            lock (ScheduledEvents)
            {
                //yes it looks silly but it's easier for me to tell what's going on when it's stacked like this
                ScheduledEvents.Add(new ScheduledMouseEvent()
                {
                    Command = MouseEventArgs.LeftDown(),
                    FireTime = 0
                });
                ScheduledEvents.Add(new ScheduledMouseEvent()
                {
                    Command = MouseEventArgs.LeftUp(),
                    FireTime = TotalElapsedMilli + DOUBLE_CLICK_DOWN_MILLI
                });
                ScheduledEvents.Add(new ScheduledMouseEvent()
                {
                    Command = MouseEventArgs.LeftDown(),
                    FireTime = TotalElapsedMilli + DOUBLE_CLICK_DOWN_MILLI + DOUBLE_CLICK_DOWN_MILLI
                });
                ScheduledEvents.Add(new ScheduledMouseEvent()
                {
                    Command = MouseEventArgs.LeftUp(),
                    FireTime = TotalElapsedMilli + DOUBLE_CLICK_DOWN_MILLI + DOUBLE_CLICK_DOWN_MILLI + DOUBLE_CLICK_DOWN_MILLI
                });
            }
        }

        public void LeftDown()
        {
            lock (ScheduledEvents)
            {
                ScheduledEvents.Add(new ScheduledMouseEvent()
                {
                    Command = MouseEventArgs.LeftDown(),
                    FireTime = 0
                });
            }
        }

        public void LeftUp()
        {
            lock (ScheduledEvents)
            {
                ScheduledEvents.Add(new ScheduledMouseEvent()
                {
                    Command = MouseEventArgs.LeftUp(),
                    FireTime = 0
                });
            }
        }

        public void Dispose()
        {
            Disposed = true;
            if (!MoveLoopThread.Join(500))
            {
                try
                {
                    MoveLoopThread.Abort();
                }
                catch { }
            }
        }

        private struct ScheduledMouseEvent
        {
            public uint FireTime;
            public MouseEventArgs Command;
        }
    }

    public class JoysticState
    {
        public float X { get; set; }
        public float Y { get; set; }
    }
}
