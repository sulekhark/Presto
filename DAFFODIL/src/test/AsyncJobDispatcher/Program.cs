using System;
using System.Linq;
using System.IO;

namespace AsyncJobDispatcher {
    internal class Program {

        static StreamWriter logSW;
        private static void Main(string[] args) {
            try
            {
                SetupLogging();
            }
            catch (Exception e)
            {
                if (CheckValid(e)) throw e;
            }
            if (logSW != null)
            {
                AsyncAwait();
                Test1();
                Test2();
                try
                {
                    EndLogging();
                }
                catch (Exception e)
                {
                    if (CheckValid(e)) throw e;
                }
            }
        }

        private static void AsyncAwait() {
            var master = new MasterAsyncAwait();
            SingleThreadSynchronizationContext sc = null;
            try
            {
                sc = new SingleThreadSynchronizationContext();
                sc.UnhandledException += (sender, eventArgs) => Console.WriteLine(eventArgs.ExceptionObject);
                master.SynchronizationContext = sc;
                master.LogSW = logSW;
                var workers = Enumerable.Range(0, 2).Select(_ => new Worker()).ToDictionary(x => x.Id);
                master.Workers = workers;
                foreach (int wId in workers.Keys) master.OnWorkerConnected(wId);
            }
            catch (Exception e)
            {
                if (CheckValid(e)) throw e;
            }

            string f1 = "C:\\Users\\sulek\\work\\ExcAnalysis\\Presto\\DAFFODIL\\src\\test\\AsyncJobDispatcher\\bin\\Debug\\TestData\\f1.txt";
            string f2 = "C:\\Users\\sulek\\work\\ExcAnalysis\\Presto\\DAFFODIL\\src\\test\\AsyncJobDispatcher\\bin\\Debug\\TestData\\f2.txt";
            string f3 = "C:\\Users\\sulek\\work\\ExcAnalysis\\Presto\\DAFFODIL\\src\\test\\AsyncJobDispatcher\\bin\\Debug\\TestData\\f3.txt";
            string f4 = "C:\\Users\\sulek\\work\\ExcAnalysis\\Presto\\DAFFODIL\\src\\test\\AsyncJobDispatcher\\bin\\Debug\\TestData\\f4.txt";
            string f5 = "C:\\Users\\sulek\\work\\ExcAnalysis\\Presto\\DAFFODIL\\src\\test\\AsyncJobDispatcher\\bin\\Debug\\TestData\\f5.txt";
            var job1 = new Job {
                // Lines = new[] {"Test1Job1", "Test1Job22", "Test1Job333"}
                Lines = new[] { f1, f2, f3 }
            };
            var job2 = new Job
            {
                Lines = new[] { f4, f5 }
            };
            sc.Post(o => master.OnJobReceived(job1), null);
            sc.Post(o => master.OnJobReceived(job2), null);

            master.Start();
        }

        private static void Test1()
        {
            Tests testObj = new Tests(logSW);
            testObj.SchedulesJob();
        }

        private static void Test2()
        {
            Tests testObj = new Tests(logSW);
            testObj.SplitsTasksBetweenWorkersFairly();
        }

        public static bool CheckValid(Exception e)
        {
            if (e is ArgumentNullException ||
                e is ArgumentException ||
                e is IOException ||
                e is AggregateException ||
                e is ObjectDisposedException ||
                e is Exception ||
                e is InvalidOperationException ||
                e is ArgumentOutOfRangeException)
                return false;
            return true;
        }

        private static void SetupLogging()
        {
            logSW = new StreamWriter("log.txt");
        }

        private static void EndLogging()
        {
            logSW.Close();
        }

        public static void EmitLogging(string msg)
        {
            logSW.WriteLine(msg);
        }
    }
}
