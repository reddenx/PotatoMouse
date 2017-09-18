using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PR.Presentation.TargetClient
{
    internal static class Parallelism
    {
        public static Thread Fork(Action action)
        {
            var thread = new Thread(new ThreadStart(action));
            thread.IsBackground = true;
            thread.Start();
            return thread;
        }
    }
}
