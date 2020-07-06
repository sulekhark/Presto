using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace AsyncJobDispatcher {
    internal class MasterAsyncAwait {
        private readonly List<int> connectedWorkers;
        private readonly Queue<Job> jobQueue;
        private readonly Action<JobTask, int> scheduleTask;
        private readonly Action<Job> jobFinished;
        private readonly List<TaskResult> results = new List<TaskResult>();
        public StreamWriter LogSW { get; set; }
        public Dictionary<int, Worker> Workers { get; set; }
        public SynchronizationContext SynchronizationContext { get; set; }
       
        public MasterAsyncAwait() {
            this.jobQueue = new Queue<Job>();
            this.connectedWorkers = new List<int>();
            scheduleTask = (jobTask, workerId) =>
            {
                Console.WriteLine("Scheduling task to {0}", workerId);
                Workers[workerId].jobTaskQueue.Enqueue(jobTask);
            };
            jobFinished = finishedJob => Console.WriteLine("Job result: Letters: {0}  Words: {1}", finishedJob.ResultL, finishedJob.ResultW);
        }

        public void Start() {
            try
            {
                StartMasterInt();
            }
            catch (Exception e)
            {
                if (IsFatal(e))
                {
                    throw e;
                }
            }
        }

        private void StartMasterInt()
        {
            foreach (int workerId in connectedWorkers)
                Workers[workerId].TaskExecuted += result => SynchronizationContext.Post(o => this.OnTaskResultReceived(result), null);
            try
            {
                ExecuteJobs().Wait();
            }
            catch (Exception e)
            {
                Exception inner = e.InnerException;
                throw inner;
            }
        }

        public bool IsFatal(Exception e)
        {  
            if (e is ArgumentNullException || e is AggregateException) return false;
            return true;
        }

        private async Task<int> ExecuteJobs() {
            SynchronizationContext.SetSynchronizationContext(this.SynchronizationContext);

            while (jobQueue.Count > 0)
            {
                var job = await this.GetJobAsync();
                var tasks = job.Lines.Select(line => new JobTask { Text = line }).ToList();
                var workers = await this.GetWorkersAsync();
                results.Clear();

                var i = 0;
                foreach (var jobTask in tasks)
                {
                    CheckFile(jobTask.Text);
                    this.scheduleTask(jobTask, workers[i++ % workers.Count()]);
                }
                foreach (var wId in workers)
                {
                    Worker w = Workers[wId];
                    w.Start();
                }

                while (tasks.Count > results.Count)
                {
                    var result = await this.ReceiveTaskResultAsync();
                }

                job.ResultL = results.Sum(x => x.Letters);
                job.ResultW = results.Sum(x => x.Words);
                this.jobFinished(job);
            }
            return 0;
        }

        private void CheckFile(string fname)
        {
            if (!File.Exists(fname))
                throw new Exception("FATAL: File not found");
        }

        private static Task<TResult> WaitFor<TResult>(
            Action<Action<TResult>> subscribe,
            Action<Action<TResult>> unsubscribe) {
            var source = new TaskCompletionSource<TResult>();

            if (subscribe == null || unsubscribe == null)
                throw new ArgumentNullException();

            Action<TResult> handler = null;

            handler = result => {
                unsubscribe(handler);
                source.TrySetResult(result);
            };

            subscribe(handler);
            return source.Task;
        }

        private Task<Job> ReceiveJobAsync() {
            return WaitFor<Job>(
                handler => this.JobReceived += handler,
                handler => this.JobReceived -= handler);
        }

        private Task<TaskResult> ReceiveTaskResultAsync() {
            return WaitFor<TaskResult>(
                handler => this.TaskResultReceived += handler,
                handler => this.TaskResultReceived -= handler);
        }

        private async Task<IReadOnlyList<int>> GetWorkersAsync() {
            if (this.connectedWorkers.Count == 0)
                return new[] { await this.WaitForWorkerToConnect() };

            return this.connectedWorkers;
        }

        private Task<int> WaitForWorkerToConnect() {
            return WaitFor<int>(
                handler => this.WorkerConnected += handler,
                handler => this.WorkerConnected -= handler);
        }

        private async Task<Job> GetJobAsync() {
            while (this.jobQueue.Count == 0)
                await this.ReceiveJobAsync();
            return this.jobQueue.Dequeue();
        }

        public void OnJobReceived(Job job) {
            this.jobQueue.Enqueue(job);
            this.JobReceived(job);
        }

        public void OnTaskResultReceived(TaskResult result) {
             results.Add(result);
            this.TaskResultReceived(result);
        }

        public void OnWorkerConnected(int worker) {
            this.connectedWorkers.Add(worker);
            this.WorkerConnected(worker);
        }

        private void OnWorkerDisconnected(int worker) {
            this.connectedWorkers.Remove(worker);
            this.WorkerDisconnected(worker);
        }

        private event Action<Job> JobReceived = _ => { };
        private event Action<int> WorkerConnected = _ => { };
        private event Action<int> WorkerDisconnected = _ => { };
        private event Action<TaskResult> TaskResultReceived = _ => { };
    }
}