﻿using System;

namespace E2EDemoR
{
    class MyException : Exception
    {
        public Exception innerEx;

        public MyException(Exception e) { innerEx = e; }
    }

    class E2EDemoR
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
                M4();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                if (e is NullReferenceException) M7(e);
            }
            finally
            {
                M3();
            }
        }

        static void M4()
        {
            CFoo objFoo = new CFoo();
            M2(objFoo);
        }

        static void M2(CFoo paramFoo)
        {
            if (paramFoo != null)
            {
                M5();
                M6();
            }
        }

        static void M3() { if (flag) throw new ArgumentException(); }
        static void M5() { if (flag) throw new NullReferenceException(); }
        static void M6() { if (flag) throw new FieldAccessException(); }
        static void M7(Exception e) { throw new MyException(e); }
    }

    class CFoo { }
}
