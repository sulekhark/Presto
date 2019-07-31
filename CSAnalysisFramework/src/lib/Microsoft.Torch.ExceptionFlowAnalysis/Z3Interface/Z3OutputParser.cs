using System;
using Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts;

namespace Microsoft.Torch.ExceptionFlowAnalysis.Z3Interface
{
    public static class Z3OutputParser
    {
        //Parsing of Z3 output is stateful
        static string currRelName = "";
        static Rel currRel = null;
        static int numDomsInCurrRel = 0;
        static bool matches = false;

        // Assumption: The output relation for which Z3 is dumping the tuples to the console
        // must be defined in ProgramRels and must be initialized.
        //
        // A snippet of the syntax being parsed below:
        // Tuples in S:
        //     (x=5(0))
        //     (x=1(5))
        // Tuples in Gt:
        //     (x=2(1),y=3(2))
        //     (x=6(3),y=4(4))
        //     (x=3(2),y=4(4))
        // --------------
        // original rules
        // ; rule count: 4
        // ; predicate count: 3
        // ---------------
        // generated rules
        // ; rule count: 6

        public static void ParseZ3Output(string line)
        {
            line = line.Trim();
            // Line indicates the start of the dump for a relation
            if (line.StartsWith("Tuples in "))
            {
                if (currRel != null)
                {
                    currRel.Save();
                }
                currRelName = line.Split()[2];
                currRelName = currRelName.Remove(currRelName.Length - 1, 1);
                currRel = ProgramRels.nameToRelMap.ContainsKey(currRelName) ?
                          ProgramRels.nameToRelMap[currRelName] : null;
                if (currRel != null)
                {
                    numDomsInCurrRel = currRel.GetNumDoms();
                    matches = false;
                }
            }
            // Line is a tuple belongign to the relation
            else if (line.StartsWith("("))
            {
                string[] elems = line.Split(new char[] { ',' });
                if (!matches)
                {
                    if (numDomsInCurrRel == elems.Length)
                    {
                        matches = true;
                    }
                }
                if (matches)
                {
                    int[] vals = new int[numDomsInCurrRel];
                    for (int i = 0; i < numDomsInCurrRel; i++)
                    {
                        string idxStr;
                        if (i == 0)
                        {
                            idxStr = elems[i].Split(new char[] { '(', '=' })[2];
                        }
                        else
                        {
                            idxStr = elems[i].Split(new char[] { '(', '=' })[1];
                        }
                        vals[i] = Int32.Parse(idxStr);
                    }
                    currRel.Add(vals);
                }
            }
            // All other lines
            else
            {
                if (currRel != null)
                {
                    currRel.Save();
                }
                currRelName = "";
                currRel = null;
                numDomsInCurrRel = 0;
                matches = false;
            }
        }

        public static void ParseZ3Error(string line)
        {
        }
    }
}
