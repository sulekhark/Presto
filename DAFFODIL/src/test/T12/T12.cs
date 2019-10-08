
namespace T12
{
    class T12
    {
        static void Main(string[] args)
        {
            Bar svar = new Bar(null, null);
            Foo fooFirst = new Foo();
            Foo zzz;
            FreshOutFoo(out zzz);
            FreshRefFoo(ref fooFirst);
            IBaz bazArg = new Baz();
            Taz tazArg = new Taz();
            fooFirst.FooFunc<IBaz>(ref bazArg, ref tazArg);
            fooFirst.FooFuncTrial(ref bazArg, ref tazArg);
            Baz bazArgOrd = bazArg as Baz;
            fooFirst.FooFuncOrdinary(bazArgOrd);
            svar.objSec = fooFirst;
            Foo yyy = svar.objSec;
            Bar rvar = svar;
            rvar = Modify(rvar);
            LocMod(rvar);
            Bar freshBar = new Bar(null,null);
            FreshBar(out freshBar);
            FreshOutFoo(out freshBar.objFir);
            FreshOutFoo(out freshBar.objSec);
        }

        
        static void FreshBar(out Bar freshparam)
        {
            freshparam = new Bar(new Foo(), new Foo());
        }

        static void FreshRefFoo(ref Foo ff)
        {
            ff = new Foo();
        }

        static void FreshOutFoo(out Foo ff)
        {
            ff = new Foo();
        }

        static Bar Modify(Bar mod)
        {
            Foo orig = mod.GetF();
            if (orig == null)
            {
                Foo repl = new Foo();
                mod.SetF(repl);
            }
            return mod;
        }

        static void LocMod(Bar mod)
        {
            Foo orig = mod.GetF();
            if (orig == null)
            {
                Foo repl = new Foo();
                mod.SetF(repl);
            }
        }
    }
    struct Bar
    {
        public Foo objFir;
        public Foo objSec;

        public Bar(Foo x1, Foo x2)
        {
            objFir = x1;
            objSec = x2;
        }

        public Foo GetF()
        {
            return objFir;
        }

        public void SetF(Foo o)
        {
            objFir = o;
        }
    }

    class Foo
    {
        public void FooFunc<T>(ref T bazObj, ref Taz tazObj) where T : IBaz
        {
            bazObj.BazFunc();
        }

        public void FooFuncTrial(ref IBaz bazObj, ref Taz tazObj)
        {
            bazObj.BazFunc();
        }

        public void FooFuncOrdinary(Baz ordBaz)
        {
            ordBaz.BazFunc();
        }
    }

    interface IBaz
    {
        void BazFunc();
    }

    class Baz : IBaz
    {
        //public Taz tazMember;
        public void BazFunc() { }
    }

    class Taz { }
}
