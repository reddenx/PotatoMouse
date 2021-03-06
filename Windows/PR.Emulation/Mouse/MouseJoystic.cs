﻿using SMT.Utilities.InputEvents.HardwareEvents;
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

        private float XCarry; //carryover from last update for x
        private float YCarry; //carryover from last update for y

        private bool Running;
        private Vector2 StartPosition;
        private Vector2 EndPosition;

        public MouseJoystic()
        {
            XCarry = 0f;
            YCarry = 0f;
            Running = false;

            StartPosition = Vector2.Empty;
            EndPosition = Vector2.Empty;
        }

        protected override void DoScheduledMovementEvents()
        {
            //setup some temps
            var x = 0f;
            var y = 0f;

            var joysticState = new Vector2(EndPosition.X - StartPosition.X, EndPosition.Y - StartPosition.Y);

            //determine raw input for this update loop
            if (Math.Abs(joysticState.X) >= DEAD_ZONE_X)
                x = joysticState.X * LOOP_WAIT_TIME * MovementScale;

            if (Math.Abs(joysticState.Y) >= DEAD_ZONE_Y)
                y = joysticState.Y * LOOP_WAIT_TIME * MovementScale;

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

        public void Moved(float x, float y)
        {
            EndPosition = new Vector2(x, y);
            Console.WriteLine($"MOVED {StartPosition.X},{StartPosition.Y} - {x},{y}");
        }

        public void Stop(float x, float y)
        {
            Running = false;
            StartPosition = EndPosition = Vector2.Empty;
        }

        public void Start(float x, float y)
        {
            Running = true;
            StartPosition = EndPosition = new Vector2(x, y);
        }
    }
}
