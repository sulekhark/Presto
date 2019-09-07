using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Torch.Stubs
{
    public class TaskAwaiter
    {
        readonly Task task;

        public TaskAwaiter(Task t)
        {
            task = t;
        }

        public void GetResult() { }
    }

    public class TaskAwaiter<TResult>
    {
        readonly Task<TResult> task;

        public TaskAwaiter(Task<TResult> t)
        {
            task = t;
        }

        public TResult GetResult()
        {
            return task.Result;
        }
    }
}
