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
    public class MouseJoystic : LoopedMouseEventBase
    {
        private const float DEAD_ZONE_X = 0f;
        private const float DEAD_ZONE_Y = 0f;
        private const float CALIBRATION = 1f;

        private float XCarry; //carryover from last update for x
        private float YCarry; //carryover from last update for y

        private JoysticState CurrentState;

        public MouseJoystic()
        {
            XCarry = 0f;
            YCarry = 0f;
            CurrentState = new JoysticState() { X = 0, Y = 0 };
        }

        protected override void DoScheduledMovementEvents()
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
    }

    public class JoysticState
    {
        public float X { get; set; }
        public float Y { get; set; }
    }
}
