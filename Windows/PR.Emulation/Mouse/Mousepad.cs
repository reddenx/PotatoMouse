using SMT.Utilities.InputEvents.HardwareEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR.Emulation.Mouse
{
    public class Mousepad : LoopedMouseEventBase
    {
        private Vector2 LastPosition;
        private Vector2 CarryOver;

        public Mousepad() { }

        protected override void DoScheduledMovementEvents()
        {
            //this type doesn't have state based movement.
        }

        public void MoveMouse(int diffx, int diffy)
        {
            base.MouseRunner.DoEvent(MouseEventArgs.Move(diffx, diffy));
        }

        //public void Moved(int x, int y)
        //{
        //    //x *= MovementScale;
        //    //y *= MovementScale;
        //    //var currentPosition = new Vector2(x, y);


        //    ////get diff
        //    //var diff = (currentPosition - LastPosition) + CarryOver;

        //    ////get rounded movements
        //    //var moveX = (int)Math.Round(diff.X);
        //    //var moveY = (int)Math.Round(diff.Y);

        //    ////reselect carryover
        //    //CarryOver = new Vector2(diff.X - (float)moveX, diff.Y - (float)moveY);

        //    //if (Math.Abs(moveX) + Math.Abs(moveY) > 0)
        //    //{
        //    //    Console.WriteLine($"move {moveX,3},{moveY,3} car {CarryOver.X.ToString("n4")},{CarryOver.Y.ToString("n4")} raw {x.ToString("n4")},{y.ToString("n4")}");
        //    //    LastPosition = currentPosition;
        //    //    MouseRunner.DoEvent(MouseEventArgs.Move(moveX, moveY));
        //    //}



        //    ////setup some temps
        //    //var movement = new Vector2(x - StartPosition.X, y - StartPosition.Y) + CarryOver;

        //    ////round to ints
        //    //var moveX = (int)Math.Round(movement.X);
        //    //var moveY = (int)Math.Round(movement.Y);

        //    ////set carryover
        //    //CarryOver = new Vector2((float)moveX - movement.X, (float)moveY - movement.Y);

        //    ////make sure we're even moving
        //    //if (Math.Abs(moveX) + Math.Abs(moveY) > 0)
        //    //{
        //    //    Console.WriteLine($"move {moveX,3},{moveY,3} car {CarryOver.X.ToString("n4")},{CarryOver.Y.ToString("n4")} raw {x.ToString("n4")},{y.ToString("n4")}");
        //    //    MouseRunner.DoEvent(MouseEventArgs.Move(moveX, moveY));
        //    //}
        //}
    }
}
