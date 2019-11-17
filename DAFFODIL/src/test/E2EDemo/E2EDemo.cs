using System;

namespace E2EDemo
{
    class E2EDemo
    {
        static bool flag = false;
        static void Main(string[] args)
        {
            M1();
            M2(null);
            System.Console.WriteLine("Done");
        }

        static void M1()
        {
            try
            {
                CFoo objFoo = new CFoo();
                M2(objFoo);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
            finally
            {
                M3();
            }
        }

        static void M2(CFoo paramFoo)
        {
            if (paramFoo != null)
            {
                M4();
                M5();
            }
        }

        static void M3() { if (flag) throw new ArgumentException(); }
        static void M4() { if (flag) throw new NullReferenceException(); }
        static void M5() { if (flag) throw new FieldAccessException(); }
    }

    class CFoo { }
}