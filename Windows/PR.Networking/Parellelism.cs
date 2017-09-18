using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PR.Networking
{
    internal static class Parellelism
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
