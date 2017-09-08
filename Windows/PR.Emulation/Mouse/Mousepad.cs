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
        public Mousepad()
        {

        }

        protected override void DoScheduledMovementEvents()
        {
            //this type doesn't have state based movement.
        }

        public void MoveMouse(int diffx, int diffy)
        {
            base.MouseRunner.DoEvent(MouseEventArgs.Move(diffx, diffy));
        }
    }
}
