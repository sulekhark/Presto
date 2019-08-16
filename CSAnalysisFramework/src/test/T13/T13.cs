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
            A a = null;
            try
            {
                a = foo.FooProc(b);
                foo.Dummy(true);
            }
            catch (Exception e)
            {
                s = e.Message;
            }
            finally
            {
                Console.WriteLine("Done: {0}", s);
            }
            if (a.pri == b)
            {
                Console.WriteLine("ok");
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

        public void Dummy(bool flag)
        {
            string s;
            try
            {
                s = "try1";
                try
                {
                    if (flag)
                    {
                        s = "try11 - true";
                    }
                    else
                    {
                        s = "try11 - false";
                    }
                    s = "try11 - post";
                }
                catch { s = "catch111"; }
                try { s = "try12"; }
                catch { s = "catch121"; }
                finally { s = "finally12"; }
            }
            catch (ArgumentException ae) { s = "catch11"; }
            catch (Exception e)
            {
                s = "catch12";
                try { s = "try121"; }
                catch { s = "catch1211"; }
                finally { s = "finally121"; }
            }
            finally { s = "finally1"; }
        }
    }

    class A
    {
        public B pri;
        public B sec;
    }

    class B { }
}
