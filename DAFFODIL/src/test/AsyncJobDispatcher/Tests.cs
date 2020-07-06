using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;

namespace AsyncJobDispatcher {
    public class Tests {
        private readonly MasterAsyncAwait master;
        public Tests(StreamWriter logSW) {
            master = new MasterAsyncAwait();
            try
            {
                master.SynchronizationContext = new SingleThreadSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(master.SynchronizationContext);
                master.LogSW = logSW;
            }
            catch (Exception e)
            {
                if (Program.CheckValid(e)) throw e;
            }
        }

        public void SchedulesJob()
        {
            Worker w = new Worker();
            Dictionary<int, Worker> workers = null;
            try
            {
                workers = new Dictionary<int, Worker>();
                workers.Add(w.Id, w);
            }
            catch (Exception e)
            {
                if (Program.CheckValid(e)) throw e;
            }
            this.master.Workers = workers;
            this.master.OnWorkerConnected(w.Id);
            string f6 = "C:\\Users\\sulek\\work\\ExcAnalysis\\Presto\\DAFFODIL\\src\\test\\AsyncJobDispatcher\\bin\\Debug\\TestData\\f6.txt";
            string f7 = "C:\\Users\\sulek\\work\\ExcAnalysis\\Presto\\DAFFODIL\\src\\test\\AsyncJobDispatcher\\bin\\Debug\\TestData\\f7.txt";
            this.master.OnJobReceived(new Job { Lines = new[] { f6, f7 } });
            this.master.Start();
        }

        public void SplitsTasksBetweenWorkersFairly() { 
            Worker w1 = new Worker();
            Worker w2 = new Worker();
            Dictionary<int, Worker> workers = null;
            try
            {
                workers = new Dictionary<int, Worker>();
                workers.Add(w1.Id, w1);
                workers.Add(w2.Id, w2);
            }
            catch (Exception e)
            {
                if (Program.CheckValid(e)) throw e;
            }
            this.master.Workers = workers;
            this.master.OnWorkerConnected(w1.Id);
            this.master.OnWorkerConnected(w2.Id);
            string f6 = "C:\\Users\\sulek\\work\\ExcAnalysis\\Presto\\DAFFODIL\\src\\test\\AsyncJobDispatcher\\bin\\Debug\\TestData\\f6.txt";
            string f7 = "C:\\Users\\sulek\\work\\ExcAnalysis\\Presto\\DAFFODIL\\src\\test\\AsyncJobDispatcher\\bin\\Debug\\TestData\\f7.txt";
            this.master.OnJobReceived(new Job { Lines = new[] { f6, f7 } });
            this.master.Start();
        }
    }
}
