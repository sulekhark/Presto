
namespace T12
{
    class T12
    {
        static void Main(string[] args)
        {
            Bar svar = new Bar(null, null);
            Foo f = new Foo();
            Foo zzz;
            FreshFoo(out zzz);
            FreshFoo1(ref f);
            svar.objSec = f;
            Foo z = svar.objSec;
            Bar rvar = svar;
            rvar = Modify(rvar);
            LocMod(rvar);
            Bar fresh = new Bar(null,null);
            Fresh(out fresh);
            FreshFoo(out fresh.objFir);
            FreshFoo(out fresh.objSec);
        }

        
        static void Fresh(out Bar freshparam)
        {
            freshparam = new Bar(new Foo(), new Foo());
        }

        static void FreshFoo1(ref Foo ff)
        {
            ff = new Foo();
        }

        static void FreshFoo(out Foo ff)
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

    class Foo { }
}
