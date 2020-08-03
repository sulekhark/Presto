using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScrambledSquares
{
    public class Program
    {
        public static void Main(string[] args)
        {
           if (args.Length % 2 != 0)
            {
                Console.WriteLine("Invalid usage: Please enter dimension of puzzle followed by the puzzle input file name.");
            }
           for (int i = 0; i < args.Length; i += 2)
            {
                CheckArg(args[i + 1]);
                ProcessPuzzle(Int32.Parse(args[i]), args[i + 1]);
            }
        }

        public static void ProcessPuzzle(int sz, string ipFile)
        {
            ScrambledSquares ss = new ScrambledSquares(sz, ipFile);
            ScrambledSquares.IgnoreExceptions = true;
            ss.Solve();
            foreach (var item in ss.Results)
            {
                item.PrintResult("result.txt");
            }
        }

        public static void CheckArg(string ipArg)
        {
            if (!File.Exists(ipArg))
                throw new FileNotFoundException();
        }
    }
}
