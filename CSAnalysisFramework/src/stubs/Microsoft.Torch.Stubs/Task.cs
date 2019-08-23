using System;

namespace Microsoft.Torch.Stubs
{
    public class Task
    {
        public Exception e;
        readonly TaskAwaiter ta;
        readonly Action action;
        readonly Action<Object> action1;
        readonly Object action1Arg;

        public Task() { }

        public Task(Action a)
        {
            action = a;
            ta = new TaskAwaiter(this);
        }

        public Task(Action<Object> a1, Object obj)
        {
            action1 = a1;
            action1Arg = obj;
            ta = new TaskAwaiter(this);
        }
        // public Run()

        public TaskAwaiter GetAwaiter()
        {
            return ta;
        }

        public void Wait()
        {
            if (e != null) throw e;
        }
    }
    public class Task<TResult> : Task
    {
        public TResult Result { get; set; }
        readonly TaskAwaiter<TResult> ta;
        readonly Func<TResult> func;
        readonly Func<Object, TResult> func1;
        readonly Object func1Arg;

        public Task() { }

        public Task(TResult tr)
        {
            Result = tr;
        }
        public Task(Func<TResult> f)
        {
            func = f;
            ta = new TaskAwaiter<TResult>(this);
        }

        public Task(Func<Object, TResult> f1, Object obj)
        {
            func1 = f1;
            func1Arg = obj;
            ta = new TaskAwaiter<TResult>(this);
        }
        // public Run()

        new public TaskAwaiter<TResult> GetAwaiter()
        {
            return ta;
        }
    }
}
