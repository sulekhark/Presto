
namespace T12
{
    class T12
    {
        static void Main(string[] args)
        {
            Bar svar = new Bar(null, null);
            Foo f = new Foo();
            svar.obj2 = f;
            Foo z = svar.obj2;
            Bar rvar = svar;
        }
    }
    struct Bar
    {
        public Foo obj1;
        public Foo obj2;

        public Bar(Foo x1, Foo x2)
        {
            obj1 = x1;
            obj2 = x2;
        }
    }

    class Foo { }
}
