using System;

namespace T13
{
    class T13
    {
        static void Main(string[] args)
        {
            B b = new B();
            Foo foo = new Foo();
            string s = "";
            try
            {
                A a = foo.FooProc(b);
            }
            catch (Exception e)
            {
                s = e.Message;
            }
            finally
            {
                Console.WriteLine("Done: {0}", s);
            }
        }
    }

    class Foo
    {
        public A FooProc(B b)
        {
            B bsec = null;
            try
            {
                bsec = FooCallee();
            }
            catch (ArgumentNullException ane)
            {
                throw ane;
            }
            catch { }
            A a = new A();
            a.pri = b;
            a.sec = bsec;
            return a;
        }

        public B FooCallee()
        {
            B b = new B();
            if (b == null)
            {
                throw new ArgumentNullException();
            }
            return b;
        }
    }

    class A
    {
        public B pri;
        public B sec;
    }

    class B { }
}
