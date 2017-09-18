using SMT.Utilities.InputEvents.HardwareEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PR.Emulation.Mouse
{
    public abstract class LoopedMouseEventBase : IDisposable
    {
        private const int CLICK_DOWN_MILLI = 200;
        private const int DOUBLE_CLICK_DOWN_MILLI = 100;
        protected const int LOOP_WAIT_TIME = 20;

        protected readonly MouseEventRunner MouseRunner;

        private List<ScheduledMouseEvent> ScheduledEvents;
        private uint TotalElapsedMilli;
        private readonly Thread MoveLoopThread;
        private bool Disposed;
        protected float MovementScale { get; private set; }

        public LoopedMouseEventBase()
        {
            MouseRunner = new MouseEventRunner();
            Disposed = false;
            ScheduledEvents = new List<ScheduledMouseEvent>();

            TotalElapsedMilli = 0;

            MoveLoopThread = new Thread(new ThreadStart(MoveLoop));
            MoveLoopThread.IsBackground = true;
            MoveLoopThread.Start();
        }

        public void SetMovementScale(float scale)
        {
            MovementScale = scale;
        }

        //background loop, runs commands and updates the elapsed time for the thread timer
        private void MoveLoop()
        {
            while (!Disposed)
            {
                Thread.Sleep(LOOP_WAIT_TIME);
                HandleFollowupCommands();
                DoScheduledMovementEvents();
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

        /// <summary>
        /// when overriden, should handle the movement of the mouse based on whatever scheme the parent defines
        /// do not block this thread, it will ruin all input based events
        /// </summary>
        protected abstract void DoScheduledMovementEvents();
      
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
}
