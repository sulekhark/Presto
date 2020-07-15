using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrambledSquares
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ScrambledSquares ss = new ScrambledSquares(3, @"input1.in");
            ScrambledSquares.IgnoreExceptions = true;
            ss.Solve();
            foreach (var item in ss.Results)
            {
                item.PrintResult("result.txt");
            }
        }
    }
}
