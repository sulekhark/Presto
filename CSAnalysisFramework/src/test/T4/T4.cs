
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace FactTest
{
    static class AnonymousFunction
    {
        static int fooAnon(Func<int,int> f, int a)
        {
            return f(a);
        }
        public static void barAnon()
        {
            Console.WriteLine(fooAnon(x => x + 1, 3));
        }
    }
    static class Iterator
    {
        static IEnumerable<int> fooIt()
        {
            yield return 1;
            yield return 2;
        }

        public static void barIt()
        {
            foreach (var x in fooIt())
            {
                Console.WriteLine(x);
            }
        }
    }
    class T4
    {
        public Task<MyObj> ShowAsync(bool flag)
        {
            MyObj xxx;
            Task<MyObj> tmo;

            if (flag)
            {
                tmo = Task.Run(() =>
                {
                    xxx = new MyObj();
                    Console.WriteLine("Hello");
                    xxx.i = 5;
                    return xxx;
                });
            }
            else
            {
                tmo = Task.Run(() =>
                {
                    xxx = new MyObj();
                    Console.WriteLine("Hello");
                    xxx.i = 7;
                    return xxx;
                });
            }
            return tmo;
        }
        public async Task<MyObj> CallAsync()
        {
            MyObj myObj = null;
            try
            {
                myObj = await ShowAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return myObj;
        }
    }

    class MyObj
    {
        public int i;
        public void Process()
        {
            Console.WriteLine("Processing...");
        }
    }
    class Program
    {
        public static void Main(String[] args)
        {
            T4 demo = new T4();
            MyObj zzz = demo.CallAsync().Result;
            zzz.Process();
            Console.ReadLine();
            Iterator.barIt();
            AnonymousFunction.barAnon();
        }
    }
}