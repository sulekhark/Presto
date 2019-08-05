﻿using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Torch.Stubs
{
    internal class VoidTaskResult
    {
    }

    public struct AsyncTaskMethodBuilder
    {
        AsyncTaskMethodBuilder<VoidTaskResult> m_builder;
        public Task Task
        {
            get { return m_builder.Task; }
        }

        public static AsyncTaskMethodBuilder Create()
        {
            return default(AsyncTaskMethodBuilder);
        }

        public void SetException(Exception exception)
        {
            m_builder.SetException(exception);
        }

        public void SetResult()
        {
            m_builder.SetResult(null);
        }

        public void Start<TStateMachine>(ref TStateMachine sm) where TStateMachine : IAsyncStateMachine
        {
            sm.MoveNext();
        }
    }

    
    public struct AsyncTaskMethodBuilder<TResult>
    {
        Task<TResult> m_task;
        static Task<TResult> s_defaultResultTask;

        public Task<TResult> Task
        {
            get
            {
                Task<TResult> task = m_task;
                if (task == null)
                {
                    task = (m_task = new Task<TResult>());
                }
                return task;
            }
        }

        public AsyncTaskMethodBuilder<TResult> Create()
        {
            return default(AsyncTaskMethodBuilder<TResult>);
        }

        static AsyncTaskMethodBuilder()
        {
            s_defaultResultTask = new Task<TResult>(default(TResult));
        }

        public void Start<TStateMachine>(ref TStateMachine sm) where TStateMachine : IAsyncStateMachine
        {
            sm.MoveNext();
        }

        public void SetException(Exception e)
        {
            Task.e = e;
        }

        public void SetResult(TResult res)
        {
            if (res == null)

            Task.Result = res;
        }
    }
}