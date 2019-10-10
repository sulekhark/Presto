using System.Collections.Generic;
using Microsoft.Cci;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole
{
    class Stubber
    {
        private static IList<string> prefixesToSuppress;
        private static FactGenerator factGen;
        private static RTAAnalyzer rtaAnalyzer;

        static Stubber()
        {
            prefixesToSuppress = new List<string>();
            prefixesToSuppress.Add("System.Console"); // Has bytecode arglist
            prefixesToSuppress.Add("System.String");  // causes crash in TypeInference
            prefixesToSuppress.Add("System.Environment");
            prefixesToSuppress.Add("System.ComponentModel");
            prefixesToSuppress.Add("System.Data");
            prefixesToSuppress.Add("System.Diagnostics");
            prefixesToSuppress.Add("System.Globalization");
            prefixesToSuppress.Add("System.IO");
            prefixesToSuppress.Add("System.Linq");
            prefixesToSuppress.Add("System.Net");
            prefixesToSuppress.Add("System.Reflection");
            prefixesToSuppress.Add("System.Resources");
            prefixesToSuppress.Add("System.Runtime");
            prefixesToSuppress.Add("System.Security");
            prefixesToSuppress.Add("System.Text");
            prefixesToSuppress.Add("System.Threading");
            prefixesToSuppress.Add("System.Xml");
            prefixesToSuppress.Add("Microsoft.Cci");
        }

        public static void SetupFactGenerator(FactGenerator fg)
        {
            factGen = fg;
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

        /****
           Four cases:
           1. Ordinary method (not stubbed and not generic)
           2. Stubbed but not generic
           3. Not stubbed but generic
           4. Stubbed and generic
           Note that there are two sub-cases for "is stubbed": "stub present" or "stub absent".
           So that makes it a total of 6 cases.
        ****/
        public static IMethodDefinition CheckAndAdd(IMethodDefinition m)
        {
            IMethodDefinition lookFor = m;
            if (m is IGenericMethodInstance)
            {
                lookFor = GenericMethods.GetTemplate(m);
            }
            
            ITypeDefinition containingType = m.ContainingTypeDefinition;
            if (containingType.InternedKey == 0) return m; // Ignore methods from Cci's Dummy typeref.
            bool matches = MatchesSuppress(m);
            if (matches)
            {
                containingType = Stubs.GetStubType(containingType);
                if (containingType == null) return null; // This entire containingType is to be ignored.
                lookFor = Utils.GetMethodSignMatch(containingType, lookFor);
                if (lookFor == null) return null; // containingType itself is stubbed, but the stub does not define a method equivalent to m.
            }

            IMethodDefinition methToAdd;
            if (m is IGenericMethodInstance)
            {
                // Here, lookFor is a template method
                methToAdd = GenericMethods.RecordInfo(lookFor, m, matches);
            }
            else
            {
                methToAdd = lookFor;
            }
            if (!rtaAnalyzer.visitedMethods.Contains(methToAdd) && !rtaAnalyzer.methods.Contains(methToAdd))
            {
                rtaAnalyzer.rtaLogSW.WriteLine("SRK_DBG: Adding method: {0}", methToAdd.GetName());
                rtaAnalyzer.methods.Add(methToAdd);
            }
            return methToAdd;
        }

        public static ITypeDefinition CheckAndAdd(ITypeDefinition t)
        {
            if (t is IGenericTypeInstance)
            {
                IGenericTypeInstance gty = t as IGenericTypeInstance;
                IEnumerable<ITypeReference> genArgs = gty.GenericArguments;
                foreach (ITypeReference garg in genArgs) CheckAndAdd(garg.ResolvedType);
            }

            ITypeDefinition toAdd = t;
            bool matches = MatchesSuppress(t);
            if (matches)
            {
                ITypeDefinition stubType = Stubs.GetStubType(t);
                if (stubType != null) toAdd = stubType;
            }
            if (!rtaAnalyzer.visitedClasses.Contains(toAdd) && !rtaAnalyzer.classes.Contains(toAdd))
            {
                rtaAnalyzer.rtaLogSW.WriteLine("SRK_DBG: Adding class: {0}", toAdd.FullName());
                rtaAnalyzer.classes.Add(toAdd);
                rtaAnalyzer.classWorkList.Add(toAdd);
            }
            return toAdd;
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
                if (methToAnalyze is IGenericMethodInstance) methToAnalyze = GenericMethods.GetTemplate(m);
                methToAnalyze = Utils.GetMethodSignMatch(containingType, methToAnalyze);
                if (methToAnalyze == null) return null; // containingType itself is stubbed, but the stub does not define a method equivalent to m.
                if (methToAnalyze.IsGeneric) methToAnalyze = GenericMethods.GetInstantiatedMeth(methToAnalyze, m);
            }
            return methToAnalyze;
        }

        public static ITypeDefinition GetTypeToAnalyze(ITypeDefinition ty)
        {
            ITypeDefinition retTy = ty;
            bool matches = MatchesSuppress(ty);
            if (matches)
            {
                ITypeDefinition stubType = Stubs.GetStubType(ty);
                if (stubType != null)
                    retTy = stubType;
                else
                    retTy = null;
            }
            return retTy;
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

        public static bool IsStubbed(ITypeDefinition ty)
        {
            bool retval = false;
            bool matches = MatchesSuppress(ty);
            if (matches)
            {
                ITypeDefinition stubType = Stubs.GetStubType(ty);
                retval = stubType == null ? false : true;
            }
            return retval;
        }
    }
}
