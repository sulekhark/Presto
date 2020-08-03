using System;
using System.Collections.Generic;
using System.IO;

namespace ScrambledSquares
{
    public class ScrambledSquares
    {
        public List<Result> Results { get; private set; }
        //public int SolutionCount { get; private set; }
        public int SearchDepth { get; private set; }
        public static bool IgnoreExceptions { get; set; }

        private Card[,] cards;
        private int MAX_CARDS;
        private int dimension;
        private DraftSpace draft;
        private StreamWriter logger;

        public ScrambledSquares(int dimension, string inputFile)
        {
            Results = new List<Result>();
            this.dimension = dimension;
            MAX_CARDS = dimension * dimension;
            cards = new Card[dimension, dimension];
            draft = new DraftSpace(dimension);

            CardReader cr = new CardReader(inputFile);
            this.cards = cr.MakeAllCard(dimension);
        }
        public void Solve()
        {
            try
            {
                logger = new StreamWriter("log.txt");
                logger.AutoFlush = true;
                Solve(0);
                logger.Close();
                logger.Dispose();
            }
            catch (Exception e)
            {
                if (!IgnoreExceptions) throw e;
            }
        }

        private bool Solve(int pos)
        {
            logger.WriteLine("----------------------------------------------");
            logger.WriteLine("Position = {0}\t\t\tSearch depth = {1}", pos, SearchDepth);
            ++SearchDepth;
            if (pos == MAX_CARDS)
            {
                logger.WriteLine("Done");
                RegisterResult();
                draft.Clear();
                //++SolutionCount;
                return true;
            }
            for (int current = 0; current < MAX_CARDS; current++)
            {
                Card c = PickCard(current);
                if (!c.Used)
                {
                    c.ResetOrientation();
                    while (c.CanRotate)
                    {
                        if (draft.TryAdd(c, pos))
                        {
                            logger.WriteLine("((TRY)) {0} --> {1} after {2} rotation", c.Name, pos, c.Orientation);
                            c.Used = true;
                            c.Position = pos;
                            if (Solve(pos + 1))
                            {
                                return true;
                            }
                            c.Used = false;
                            logger.WriteLine("[REJECT] {0} from position {1}", c.Name, pos);
                        }
                        c.Rotate();
                        logger.WriteLine("<<ROTATE>> {0} in position {1}", c.Name, pos);
                    }
                }
            }
            return false;
        }

        /*
         * 
        //bool res;
        //private void Solve(int pos, bool result)
        //{
        //    sw.WriteLine("----------------------------------------------");
        //    sw.WriteLine("Position = {0}\t\t\tSearch depth = {1}", pos, SearchDepth);
        //    ++SearchDepth;

        //    for (int current = 0; current < MAX_CARDS; current++)
        //    {
        //        Card c = PickCard(current);
        //        if (!c.Used)
        //        {
        //            c.ResetOrientation();
        //            while (c.CanRotate)
        //            {
        //                if (draft.TryAdd(c, pos))
        //                {
        //                    sw.WriteLine("((TRY)) {0} --> {1} after {2} rotation", c.Name, pos, c.Orientation);
        //                    c.Used = true;
        //                    c.Position = pos;
        //                    if (pos + 1 < MAX_CARDS)
        //                    {
        //                        Solve(pos + 1, res);
        //                        if (!res)
        //                        {
        //                            c.Used = false;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("search depth : " + SearchDepth);
        //                        for (int k = 0; k < MAX_CARDS; k++)
        //                        {
        //                            Card crd = PickCard(k);
        //                            Console.WriteLine(crd.ToString() + ", at position "
        //                                    + crd.Position + ", after " + crd.Orientation);
        //                            crd.Used = false;
        //                        }
        //                        Console.WriteLine(); ;
        //                        RegisterResult();
        //                        ++SolutionCount;
        //                        res = true;
        //                    }
        //                }
        //                c.Rotate();
        //                sw.WriteLine("<<ROTATE>> {0} in position {1}", c.Name, pos);
        //            }
        //        }
        //    }
        //    //return false;
        //}
     
         */

        private Card PickCard(int pos)
        {
            try
            {
                return cards[pos / dimension, pos % dimension];
            }
            catch (Exception)
            {
                return null;
            }
        }
        private void RegisterResult()
        {
            Results.Add(new Result(draft.Cards, SearchDepth));
        }
    }
}
