
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
        static int counter = 0;
        static IEnumerable<int> fooIt()
        {
            yield return bazIt();
           
        }

        public static void barIt()
        {
            foreach (var x in fooIt())
            {
                Console.WriteLine(x);
            }
        }

        public static int bazIt()
        {
            return counter++;
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
        public async Task<WrapperObj> CallAsync()
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
            WrapperObj wObj = new WrapperObj();
            wObj.mo = myObj;
            return wObj;
        }
    }

    class MyObj
    {
        public int i;
       
    }

    class WrapperObj
    {
        public MyObj mo;

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
            WrapperObj zzz = demo.CallAsync().Result;
            zzz.Process();
            Console.ReadLine();
            Iterator.barIt();
            AnonymousFunction.barAnon();
        }
    }
}