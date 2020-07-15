using System;
using System.IO;

namespace ScrambledSquares
{
    public class Result
    {
        public Card[,] Cards { get; private set; }
        public int SearchDepth { get; private set; }
        //public int ResultNo { get; private set; }

        public Result(Card[,] crds, int deep)
        {
            this.Cards = crds;
            this.SearchDepth = deep;
            //this.ResultNo = no;
        }

        private void Print(string filePath)
        {
            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine();
                sw.WriteLine("Result For search depth {0}.",  this.SearchDepth);
                foreach (var item in Cards)
                {
                    sw.WriteLine(item.ToString(true));
                }
                sw.WriteLine("-------------------------------------------");
            }
        }

        public void PrintResult(string filePath)
        {
            try
            {
                if (!(filePath == ""))
                {
                    Print(filePath);
                }
            }
            catch (ObjectDisposedException e)
            {
                if (!ScrambledSquares.IgnoreExceptions) throw e;
            }
            catch (FormatException e)
            {
                if (!ScrambledSquares.IgnoreExceptions) throw e;
            }
            catch (EndOfStreamException e)
            {
                if (!ScrambledSquares.IgnoreExceptions) throw e;
            }
            catch (ArgumentNullException e)
            {
                if (!ScrambledSquares.IgnoreExceptions) throw e;
            }
            catch (ArgumentException e)
            {
                if (!ScrambledSquares.IgnoreExceptions) throw e;
            }
            catch (PathTooLongException e)
            {
                if (!ScrambledSquares.IgnoreExceptions) throw e;
            }
        }
    }
}
