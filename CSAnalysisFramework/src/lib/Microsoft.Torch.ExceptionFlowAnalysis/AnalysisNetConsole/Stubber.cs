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

        public static IMethodDefinition CheckAndAdd(IMethodDefinition m)
        {
            IMethodDefinition methToAdd = m;
            IMethodDefinition lookFor = m;
            if (methToAdd.IsGeneric)
            {
                lookFor = Generics.GetTemplate(m);
            }
            
            ITypeDefinition containingType = m.ContainingTypeDefinition; // Test methToAdd's containingTypeDefinition
            bool matches = MatchesSuppress(m);
            if (matches)
            {
                containingType = Stubs.GetStubType(containingType);
                if (containingType == null) return null; // This entire containingType is to be ignored.
                methToAdd = Utils.GetStubMatchMethod(containingType, lookFor);
                if (methToAdd == null) return null; // containingType itself is stubbed, but the stub does not define a method equivalent to m.
            }

            IMethodDefinition instMeth = methToAdd;
            if (methToAdd.IsGeneric) instMeth = Generics.RecordInfo(methToAdd, m, matches);
            if (!rtaAnalyzer.visitedMethods.Contains(instMeth) && !rtaAnalyzer.methods.Contains(instMeth))
            {
                System.Console.WriteLine("SRK_DBG: Adding method: {0}", instMeth.GetName());
                rtaAnalyzer.methods.Add(instMeth);
                return instMeth;
            }
            return null;
        }

        public static ITypeDefinition CheckAndAdd(ITypeDefinition t)
        {
            ITypeDefinition toAdd = t;
            bool matches = MatchesSuppress(t);
            if (matches)
            {
                ITypeDefinition stubType = Stubs.GetStubType(t);
                if (stubType != null) toAdd = stubType;
            }
            if (!rtaAnalyzer.visitedClasses.Contains(toAdd) && !rtaAnalyzer.classes.Contains(toAdd))
            {
                System.Console.WriteLine("SRK_DBG: Adding class: {0}", toAdd.FullName());
                rtaAnalyzer.classes.Add(toAdd);
                rtaAnalyzer.classWorkList.Add(toAdd);
                return toAdd;
            }
            return null;
        }

        // This method is used during FactGeneration. The above CheckAndAdd methods are used during RTAAnalysis.
        // Since we are not modifying invoke statments to invoke methods in stubs (whenever stubs are used), we need the below method.
        public static IMethodDefinition GetMethodToAnalyze(IMethodDefinition m)
        {
            // Three cases: m should completely be ignored, should be analyzed as is, or the equivalent stub should be analyzed.
            IMethodDefinition methToAnalyze = m;
            bool matches = MatchesSuppress(m);
            if (matches)
            {
                ITypeDefinition containingType = m.ContainingTypeDefinition;
                containingType = Stubs.GetStubType(containingType);
                if (containingType == null) return null; // This entire containingType is to be ignored.
                if (methToAnalyze.IsGeneric) methToAnalyze = Generics.GetTemplate(m);
                methToAnalyze = Utils.GetStubMatchMethod(containingType, methToAnalyze);
                if (methToAnalyze == null) return null; // containingType itself is stubbed, but the stub does not define a method equivalent to m.
                if (methToAnalyze.IsGeneric) methToAnalyze = Generics.GetInstantiatedMeth(methToAnalyze, m);
            }
            return methToAnalyze;
        }

        public static bool Suppress(IMethodDefinition m)
        {
            return (GetMethodToAnalyze(m) == null);
        }

        public static bool Suppress(ITypeDefinition ty)
        {
            bool retval = false;
            bool matches = MatchesSuppress(ty);
            if (matches)
            {
                ITypeDefinition stubType = Stubs.GetStubType(ty);
                retval = stubType == null ? true : false;
            }
            return retval;
        }
    }
}
