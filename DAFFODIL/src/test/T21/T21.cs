
namespace T21
{
    class Program
    {
        public static void Main()
        {
            MyObj<string> xxx = new MyObj<string>();
            xxx.Bar();
        }
        public static void Foo() { }
    }
    class MyObj<T>
    {
        public void Bar() { Program.Foo();}
    }
}
