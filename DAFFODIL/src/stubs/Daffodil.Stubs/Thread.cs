using System;
using System.Threading;

namespace Daffodil.Stubs
{
    class Thread
    {
        readonly ThreadStart tStart;

        public Thread(ThreadStart start)
        {
            tStart = start;
        }

        public void Start()
        {
            tStart();
        }
    }
}
