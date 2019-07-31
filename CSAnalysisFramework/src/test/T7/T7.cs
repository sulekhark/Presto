using System;
using System.IO;

namespace T7NS
{

    class PString
    {
        static FileStream fs;
        static StreamWriter sw;

        // delegate declaration
        public delegate void printString(string s);

        // this method prints to the console
        public void WriteToScreen(string str)
        {
            Console.WriteLine("The String is: {0}", str);
        }

        //this method prints to a file
        public static void WriteToFile(string s)
        {
            fs = new FileStream("c:\\message.txt",
            FileMode.Append, FileAccess.Write);
            sw = new StreamWriter(fs);
            sw.WriteLine(s);
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        // this method takes the delegate as parameter and uses it to
        // call the methods as required
        public static void sendString(printString ps)
        {
            ps("Hello World");
        }

        static void Main(string[] args)
        {
            PString p = new PString();
            printString ps1 = new printString(p.WriteToScreen);
            printString ps2 = new printString(WriteToFile);
            sendString(ps1);
            sendString(ps2);

            printString ps = ps1;
            ps += ps2;
            sendString(ps);

            Foo xxx;
            p.getFoo(out xxx);

            Bar bbb, ccc;
            bbb = new Bar(null, null);
            bbb.obj1 = xxx;
            bbb.obj2 = xxx;
            p.copy(out ccc, bbb);
            string s = ccc.PrintInfo();
        }

        void getFoo(out Foo xfoo)
        {
            xfoo = new Foo("msg1");
        }

        void copy(out Bar dest, Bar src)
        {
            dest = src;
        }
    }

    class Foo
    {
        public string info;

        public Foo(string s)
        {
            info = s;
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
        public string PrintInfo()
        {
            System.Console.WriteLine(obj1.info);
            System.Console.WriteLine(obj2.info);
            return obj2.info;
        }
    }
}