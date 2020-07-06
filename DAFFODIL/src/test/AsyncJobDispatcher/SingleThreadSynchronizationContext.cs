using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace AsyncJobDispatcher {
    internal class SingleThreadSynchronizationContext : SynchronizationContext {
        private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> queue
            = new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

        private readonly Thread workerThread;

        public SingleThreadSynchronizationContext() {
            this.workerThread = new Thread(this.RunOnCurrentThread) {
                Name = "Sync Context Thread",
                IsBackground = true
            };

            this.workerThread.Start();
        }

        public override void Post(SendOrPostCallback d, object state) {
            this.queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
        }

        public override void Send(SendOrPostCallback d, object state) {
            throw new NotSupportedException();
        }

        public event UnhandledExceptionEventHandler UnhandledException = (sender, args) => { };

        private void RunOnCurrentThread() {
            SetSynchronizationContext(this);

            foreach (var workItem in this.queue.GetConsumingEnumerable())
                try {
                    workItem.Key(workItem.Value);
                } catch (Exception exception) {
                    this.UnhandledException(this, new UnhandledExceptionEventArgs(exception, false));
                }
        }
    }
}