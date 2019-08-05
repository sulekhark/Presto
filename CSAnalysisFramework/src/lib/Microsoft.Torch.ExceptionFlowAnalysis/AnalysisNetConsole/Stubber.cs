using System.Collections.Generic;
using Microsoft.Cci;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole
{
    class Stubber
    {
        private static IList<string> prefixesToSuppress;
        private static FactGenerator factGen;
        private static RTAAnalyzer rtaAnalyzer;

        public static void SetupFactGenerator(FactGenerator fg)
        {
            factGen = fg;
        }

        public static void SetupPrefixesToSuppress(IList<string> igPfx)
        {
            prefixesToSuppress = igPfx;
        }

        public static void SetupRTAAnalyzer(RTAAnalyzer rta)
        {
            rtaAnalyzer = rta;
        }

        public static ITypeDefinition GetStubType(ITypeDefinition ty)
        {
            string clName = ty.FullName();
            if (StubMap.Map.ContainsKey(clName))
            {
                string stubClName = StubMap.Map[clName];
                if (StubMap.NameToTypeDefMap.ContainsKey(stubClName))
                {
                    ITypeDefinition stubCl = StubMap.NameToTypeDefMap[stubClName];
                    if (stubCl != null) return stubCl;
                }
            }
            return null;
        }

        public static IMethodDefinition GetStubMethod(IMethodDefinition m)
        {
            ITypeDefinition declClass = m.ContainingTypeDefinition;
            string clName = declClass.FullName();
            if (StubMap.Map.ContainsKey(clName))
            {
                string stubClName = StubMap.Map[clName];
                if (StubMap.NameToTypeDefMap.ContainsKey(stubClName))
                {
                    ITypeDefinition stubCl = StubMap.NameToTypeDefMap[stubClName];
                    IMethodDefinition stubMeth = Utils.GetSignMatchMethod(stubCl, m);
                    if (stubMeth != null) return stubMeth;
                }
            }
            return null;
        }

        public static bool MatchesSuppress(IMethodDefinition m)
        {
            string mSign = m.FullName();
            bool matches = false;
            foreach (string s in prefixesToSuppress)
            {
                if (mSign.StartsWith(s))
                {
                    matches = true;
                    break;
                }
            }
            return matches;
        }

        public static bool MatchesSuppress(ITypeDefinition t)
        {
            bool matches = false;
            string tName = t.FullName();
            foreach (string s in prefixesToSuppress)
            {
                if (tName.StartsWith(s))
                {
                    matches = true;
                    break;
                }
            }
            return matches;
        }

        public static bool CheckAndAdd(IMethodDefinition m)
        {
            IMethodDefinition toAdd = m;
            bool matches = MatchesSuppress(m);
            if (matches)
            {
                IMethodDefinition stubMethod = GetStubMethod(m);
                if (stubMethod == null) return false;
                toAdd = stubMethod;
            }
            if (!rtaAnalyzer.visitedMethods.Contains(toAdd))
            {
                System.Console.WriteLine("SRK_DBG: Adding method: {0}", toAdd.GetName());
                rtaAnalyzer.methods.Add(toAdd);
                return true;
            }
            return false;
        }

        public static bool CheckAndAdd(ITypeDefinition t)
        {
            ITypeDefinition toAdd = t;
            bool matches = MatchesSuppress(t);
            if (matches)
            {
                ITypeDefinition stubType = GetStubType(t);
                if (stubType != null) toAdd = stubType;
            }
            if (!rtaAnalyzer.visitedClasses.Contains(t))
            {
                System.Console.WriteLine("SRK_DBG: Adding class: {0}", t.FullName());
                rtaAnalyzer.classes.Add(t);
                return true;
            }
            return false;
        }

        //this method is used during FactGeneration. The above CheckAndAdd methods are used during RTAAnalysis.
        public static IMethodDefinition GetMethodToAnalyze(IMethodDefinition meth)
        {
            IMethodDefinition toAnalyze;
            // Three chioces: meth should completely be ignored, should be analyzed as is, or the equivalent stub should be analyzed.
            toAnalyze = meth;
            bool matches = MatchesSuppress(meth);
            if (matches)
            {
                IMethodDefinition stubMethod = GetStubMethod(meth);
                toAnalyze = stubMethod ?? null;
            }
            return toAnalyze;
        }

        public static bool Suppress(IMethodDefinition m)
        {
            bool retval = false;
            bool matches = MatchesSuppress(m);
            if (matches)
            {
                IMethodDefinition stubMethod = GetStubMethod(m);
                retval = stubMethod == null ? true : false;
            }
            return retval;
        }

        public static bool Suppress(ITypeDefinition ty)
        {
            bool retval = false;
            bool matches = MatchesSuppress(ty);
            if (matches)
            {
                ITypeDefinition stubType = GetStubType(ty);
                retval = stubType == null ? true : false;
            }
            return retval;
        }
    }
}
