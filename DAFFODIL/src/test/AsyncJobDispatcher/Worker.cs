using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;

namespace AsyncJobDispatcher {
    public class Worker {

        private static int workerIdCounter = 0;
        public Queue<JobTask> jobTaskQueue;

        public int Id { get; set; }
        public Worker() {
            this.Id = workerIdCounter++;
            jobTaskQueue = new Queue<JobTask>();
        }

        public void Start()
        {
            Thread wth = new Thread(ProcessJobTasks);
            wth.Start();
        }
        
        public void ProcessJobTasks()
        {
            while (jobTaskQueue.Count > 0)
            {
                JobTask jt = jobTaskQueue.Dequeue();
                try
                {
                    Execute(jt);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("Job Task with id: {0} has a null file spec.", jt.Id);
                }
            }
        }
        public void Execute(JobTask jobTask) {
            if (jobTask.Text == "")
            {
                ThreadPool.QueueUserWorkItem(state => this.TaskExecuted(new TaskResult
                {
                    TaskId = jobTask.Id,
                    Letters = 0,
                    Words = 0
                }));
                throw new ArgumentException();
            }
            string fileText = File.ReadAllText(jobTask.Text);
            int wordCount = fileText.Split(new char[] { ' ', '\t' }).Length;
            ThreadPool.QueueUserWorkItem(state => this.TaskExecuted(new TaskResult {
                TaskId = jobTask.Id,
                Letters = fileText.Count(),
                Words = wordCount
            }));
        }

        public event Action<TaskResult> TaskExecuted = result => { };
    }
}