﻿
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Cci;
using Microsoft.Cci.Immutable;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Model;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Values;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Instructions;
using Microsoft.Torch.ExceptionFlowAnalysis.Common;
using System;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole
{
    public class RTAAnalyzer
    {
        public IList<ITypeDefinition> classWorkList;
        public ISet<ITypeDefinition> entryPtClasses;
        public ISet<ITypeDefinition> visitedClasses;
        public ISet<ITypeDefinition> clinitProcessedClasses;
        public ISet<ITypeDefinition> ignoredClasses;
        public ISet<IMethodDefinition> visitedMethods;
        public ISet<IMethodDefinition> ignoredMethods;
        public ISet<IMethodDefinition> entryPtMethods;
        public ISet<ITypeDefinition> allocClasses;
        public ISet<ITypeDefinition> classes;
        public ISet<IMethodDefinition> methods;
        public ISet<ITypeDefinition> types;
        public readonly bool rootIsExe;

        public readonly ISet<IFieldDefinition> addrTakenInstFlds;
        public readonly ISet<IFieldDefinition> addrTakenStatFlds;
        public readonly ISet<IVariable> addrTakenLocals;
        public readonly ISet<IMethodDefinition> addrTakenMethods;

        public StreamWriter rtaLogSW;

        public RTAAnalyzer(bool rootIsExe, StreamWriter sw)
        {
            TypeDefinitionComparer tdc = new TypeDefinitionComparer();
            MethodReferenceDefinitionComparer mdc = MethodReferenceDefinitionComparer.Default;
            FieldReferenceComparer frc = new FieldReferenceComparer();
            VariableComparer vc = new VariableComparer();

            classWorkList = new List<ITypeDefinition>();
            entryPtClasses = new HashSet<ITypeDefinition>(tdc);
            visitedClasses = new HashSet<ITypeDefinition>(tdc);
            clinitProcessedClasses = new HashSet<ITypeDefinition>(tdc);
            ignoredClasses = new HashSet<ITypeDefinition>(tdc);
            visitedMethods = new HashSet<IMethodDefinition>(mdc);
            ignoredMethods = new HashSet<IMethodDefinition>(mdc);
            entryPtMethods = new HashSet<IMethodDefinition>(mdc);
            allocClasses = new HashSet<ITypeDefinition>(tdc);
            classes = new HashSet<ITypeDefinition>(tdc);
            methods = new HashSet<IMethodDefinition>(mdc);
            types = new HashSet<ITypeDefinition>(tdc);
            this.rootIsExe = rootIsExe;
            this.rtaLogSW = sw;

            addrTakenInstFlds = new HashSet<IFieldDefinition>(frc);
            addrTakenStatFlds = new HashSet<IFieldDefinition>(frc);
            addrTakenLocals = new HashSet<IVariable>(vc);
            addrTakenMethods = new HashSet<IMethodDefinition>(mdc);
        }

        public void VisitMethod(MethodBody mBody, ControlFlowGraph cfg)
        {
            // Going through the instructions via cfg nodes instead of directly iterating over the instructions
            // of the methodBody becuase Phi instructions may not have been inserted in the insts of the methodBody.
            foreach (var node in cfg.Nodes)
            {
                foreach (var instruction in node.Instructions)
                {
                    // System.Console.WriteLine("{0}", instruction.ToString());
                    // System.Console.WriteLine("{0}", instruction.GetType().FullName());
                    // System.Console.WriteLine();

                    if (instruction is LoadInstruction)
                    {
                        LoadInstruction lInst = instruction as LoadInstruction;
                        IValue rhsOperand = lInst.Operand;   
                        if (rhsOperand is StaticFieldAccess)
                        {
                            StaticFieldAccess rhsAcc = rhsOperand as StaticFieldAccess;
                            IFieldReference fld = rhsAcc.Field;
                            ITypeDefinition fldType = fld.ContainingType.ResolvedType;
                            Stubber.CheckAndAdd(fldType);
                        }
                        // Note: calls to static methods and instance methods appear as a StaticMethodReference
                        else if (rhsOperand is StaticMethodReference)
                        {
                            StaticMethodReference sMethAddr = rhsOperand as StaticMethodReference;
                            IMethodDefinition tgtMeth = sMethAddr.Method.ResolvedMethod;
                            ITypeDefinition containingTy = tgtMeth.ContainingTypeDefinition;
                            Stubber.CheckAndAdd(containingTy);
                            IMethodDefinition addedMeth = Stubber.CheckAndAdd(tgtMeth);
                            // addrTakenMethods do not contain templates.
                            if (addedMeth != null) addrTakenMethods.Add(addedMeth);
                        }
                        //Note: calls to virtual, abstract or interface methods appear as VirtualMethodReference
                        else if (rhsOperand is VirtualMethodReference)
                        {
                            VirtualMethodReference sMethAddr = rhsOperand as VirtualMethodReference;
                            IMethodDefinition tgtMeth = sMethAddr.Method.ResolvedMethod;
                            ITypeDefinition containingTy = tgtMeth.ContainingTypeDefinition;
                            ITypeDefinition addedTy = Stubber.CheckAndAdd(containingTy);
                            IMethodDefinition addedMeth = Stubber.CheckAndAdd(tgtMeth);
                            if (addedTy != null && addedMeth != null)
                            {
                                // addrTakenMethods do not contain templates.
                                addrTakenMethods.Add(addedMeth);
                                ProcessVirtualInvoke(addedMeth, addedTy, true);
                            }
                        }
                        else if (rhsOperand is Reference)
                        {
                            Reference rhsRef = rhsOperand as Reference;
                            IReferenceable refOf = rhsRef.Value;
                            if (refOf is StaticFieldAccess)
                            {
                                StaticFieldAccess refAcc = refOf as StaticFieldAccess;
                                IFieldDefinition fld = refAcc.Field.ResolvedField;
                                ITypeDefinition fldType = fld.ContainingType.ResolvedType;
                                Stubber.CheckAndAdd(fldType);
                                addrTakenStatFlds.Add(fld);
                            }
                            else if (refOf is IVariable)
                            {
                                IVariable refVar = refOf as IVariable;
                                if (!refVar.Type.IsValueType || refVar.Type.ResolvedType.IsStruct) addrTakenLocals.Add(refVar);
                            }
                            else if (refOf is InstanceFieldAccess)
                            {
                                InstanceFieldAccess refAcc = refOf as InstanceFieldAccess;
                                IFieldDefinition fld = refAcc.Field.ResolvedField;
                                addrTakenInstFlds.Add(fld);
                            }
                            else if (refOf is ArrayElementAccess)
                            {
                                // All arrays will be added into domX as potential address taken.
                            }
                        }
                    }
                    else if (instruction is StoreInstruction)
                    {
                        StoreInstruction sInst = instruction as StoreInstruction;
                        IAssignableValue lhs = sInst.Result;
                        if (lhs is StaticFieldAccess)
                        {
                            StaticFieldAccess lhsAcc = lhs as StaticFieldAccess;
                            IFieldReference fld = lhsAcc.Field;
                            ITypeDefinition fldType = fld.ContainingType.ResolvedType;
                            Stubber.CheckAndAdd(fldType);
                        }
                    }
                    else if (instruction is CreateObjectInstruction)
                    {
                        CreateObjectInstruction newObjInst = instruction as CreateObjectInstruction;
                        ITypeReference objType = newObjInst.AllocationType;
                        ITypeDefinition objTypeDef = objType.ResolvedType;
                        if (objTypeDef is IGenericTypeInstance) objTypeDef = objTypeDef.ResolvedType;
                        ITypeDefinition addedTy = Stubber.CheckAndAdd(objTypeDef);
                        if (addedTy != null && !allocClasses.Contains(addedTy))
                        {
                            allocClasses.Add(addedTy);
                        }
                        else if (addedTy == null)
                        // addedTy will be null for non-stubbed System.* types.
                        // We don't want to analyze methods of such types, but we still want to track such objects.
                        {
                            allocClasses.Add(objTypeDef);
                        }
                    }
                    else if (instruction is CreateArrayInstruction)
                    {
                        CreateArrayInstruction newArrInst = instruction as CreateArrayInstruction;
                        ITypeReference elemType = newArrInst.ElementType;
                        ITypeDefinition elemTypeDef = elemType.ResolvedType;
                        ITypeDefinition addedTy = Stubber.CheckAndAdd(elemTypeDef);
                        if (addedTy != null && !allocClasses.Contains(addedTy))
                        {
                            allocClasses.Add(addedTy);
                        }
                        else if (addedTy == null)
                        // addedTy will be null for non-stubbed System.* types.
                        // We don't want to analyze methods of such types, but we still want to track such objects.
                        {
                            allocClasses.Add(elemTypeDef);
                        }
                    }
                    else if (instruction is MethodCallInstruction)
                    {
                        MethodCallInstruction invkInst = instruction as MethodCallInstruction;
                        IMethodReference callTgt = invkInst.Method;
                        IMethodDefinition callTgtDef = callTgt.ResolvedMethod;
                        ITypeDefinition declType = callTgtDef.ContainingTypeDefinition;
                        ITypeDefinition addedType = Stubber.CheckAndAdd(declType);
                        IMethodDefinition addedMeth = Stubber.CheckAndAdd(callTgtDef);
                        MethodCallOperation callType = invkInst.Operation;
                        if (callType == MethodCallOperation.Virtual && addedType != null && addedMeth != null)
                        {
                            ProcessVirtualInvoke(addedMeth, addedType, false);
                        }
                    }
                    else
                    {
                        // System.Console.WriteLine("{0}", instruction.ToString());
                        // System.Console.WriteLine("Not currently handled: {0}", instruction.GetType().ToString());
                        // System.Console.WriteLine();
                    }
                }
            }
        }

        public void ProcessVirtualInvoke(IMethodDefinition mCallee, ITypeDefinition calleeClass, bool isAddrTaken)
        {
            // mCallee is an ordinary method - never a template method.
            // calleeClass and mCallee are either both stubbed or, both unstubbed - i.e. they are consistent.
            bool isInterface = calleeClass.IsInterface;
            
            foreach (ITypeDefinition cl in allocClasses)
            {
                if (!Stubber.Suppress(cl))
                {
                    bool process = false;
                    if (isInterface && Utils.ImplementsInterface(cl, calleeClass))
                        process = true;
                    if (Utils.ExtendsClass(cl, calleeClass))
                        process = true;
                    if (!process) continue;
                    foreach (IMethodDefinition meth in cl.Methods)
                    {
                        if (meth is IGenericMethodInstance)
                        {
                            if (Utils.GenericStubMatch(mCallee, meth))
                            {
                                IMethodDefinition instMeth = Generics.GetInstantiatedMeth(meth, mCallee);
                                IMethodDefinition addedMeth = Stubber.CheckAndAdd(instMeth);
                                if (addedMeth != null && isAddrTaken) addrTakenMethods.Add(addedMeth);
                                break;
                            }
                        }
                        else
                        {
                            if (Utils.StubMatch(mCallee, meth))
                            {
                                IMethodDefinition addedMeth = Stubber.CheckAndAdd(meth);
                                if (addedMeth != null && isAddrTaken) addrTakenMethods.Add(addedMeth);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private bool CheckIfValid(string modulesPath, string classesPath, string entClassesPath, string methodsPath)
        {
            bool IsValid = true;
            if (!File.Exists(modulesPath) || (new FileInfo(modulesPath).Length == 0)) IsValid = false;
            if (!File.Exists(classesPath) || (new FileInfo(classesPath).Length == 0)) IsValid = false;
            if (!File.Exists(entClassesPath) || (new FileInfo(entClassesPath).Length == 0)) IsValid = false;
            if (!File.Exists(methodsPath) || (new FileInfo(methodsPath).Length == 0)) IsValid = false;
            return IsValid;
        }

        private void LoadSavedModules(IMetadataHost host, string modulesFN, IDictionary<string, IModule> moduleNameToModuleMap)
        {
            using (StreamReader sr = new StreamReader(modulesFN))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (!string.IsNullOrEmpty(line))
                    {
                        int ndx = line.IndexOf(" LOCATION:");
                        string fileName = line.Substring(ndx + 10);
                        string moduleName = line.Substring(0, ndx).Split(':')[1];
                        IModule module = host.LoadUnitFrom(fileName) as IModule;
                        moduleNameToModuleMap[moduleName] = module;
                        List<INamedTypeDefinition> l = module.GetAllTypes().OfType<INamedTypeDefinition>().ToList();
                        Console.WriteLine("Module: {0}   Num elements: {1}", moduleName, l.Count);
                        if (module == null || module == Dummy.Module || module == Dummy.Assembly)
                            throw new Exception("The input is not a valid CLR module or assembly.");
                    }
                }
            }
        }

        private void LoadSavedClasses(IMetadataHost host, string classesFN, IDictionary<string, IModule> moduleNameToModuleMap,
                                      IDictionary<string, ITypeDefinition> classNameToTypeMap, bool isEntryPt)
        {
            using (StreamReader sr = new StreamReader(classesFN))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    int ndxOfMod = line.IndexOf(" MODULE:");
                    string modName = line.Substring(ndxOfMod).Split(':')[1];
                    string classAndArgs = line.Substring(0, ndxOfMod);
                    int ndxOfArgs = classAndArgs.IndexOf(" ARGS:");
                    string className = classAndArgs.Substring(0, ndxOfArgs).Split(':')[1];
                    string args = classAndArgs.Substring(ndxOfArgs + 6);

                    IModule module = moduleNameToModuleMap[modName];
                    ITypeDefinition clTypDefn;
                    if (args.Length > 0)
                    {
                        IList<ITypeReference> genArgs = getArgsList(args, classNameToTypeMap);
                        // ASSUMPTION: for code in below line: a compiler-generated class that has the spl char '<' in its
                        // name is never generic.
                        int targNdx = className.IndexOf('<');
                        string clname = className.Substring(0, targNdx);
                        clTypDefn = UnitHelper.FindType(host.NameTable, module, clname, genArgs.Count);
                        GenericTypeInstanceReference gtyRef = new GenericTypeInstanceReference
                                                                (clTypDefn as INamedTypeReference, genArgs, host.InternFactory);
                        clTypDefn = gtyRef.ResolvedType;
                    }
                    else
                    {
                        clTypDefn = UnitHelper.FindType(host.NameTable, module, className);
                    }
                    if (clTypDefn != null)
                    {
                        if (isEntryPt)
                        {
                            entryPtClasses.Add(clTypDefn);
                        }
                        else
                        {
                            classes.Add(clTypDefn);
                        }
                        classNameToTypeMap[clTypDefn.FullName()] = clTypDefn;
                    }
                }
            }
        }

        private IList<ITypeReference> getArgsList(string args, IDictionary<string, ITypeDefinition> classNameToTypeMap)
        {
            IList<ITypeReference> argsList = new List<ITypeReference>();
            string[] parts = args.Split(',');
            foreach (string part in parts)
            {
                ITypeDefinition argTy = classNameToTypeMap[part];
                argsList.Add(argTy);
            }
            return argsList;
        }

        private void LoadSavedMethods(IMetadataHost host, string methodsFN,
                                      IDictionary<string, ITypeDefinition> classNameToTypeMap,
                                      IDictionary<string, IMethodDefinition> methodNameToMethodMap)
        {
            using (StreamReader sr = new StreamReader(methodsFN))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    int ndxOfClass = line.IndexOf(" CLASS:");
                    string className = line.Substring(ndxOfClass).Split(':')[1];
                    string methAndArgs = line.Substring(0, ndxOfClass);
                    int ndxOfArgs = methAndArgs.IndexOf(" ARGS:");
                    string methName = methAndArgs.Substring(0, ndxOfArgs).Split(':')[1];
                    string args = methAndArgs.Substring(ndxOfArgs + 6);
                  
                    ITypeDefinition cl = classNameToTypeMap[className];
                    IMethodDefinition methDefn = Utils.GetMethodByFullName(cl, methName);
                    if (methDefn.IsGeneric && args.Length > 0)
                    {
                        IList<ITypeReference> genArgs = getArgsList(args, classNameToTypeMap);
                        GenericMethodInstance newInstMeth = new GenericMethodInstance(methDefn, genArgs, host.InternFactory);
                        methDefn = newInstMeth;
                    }
                    methodNameToMethodMap[methDefn.FullName()] = methDefn;
                    if (methDefn != null) methods.Add(methDefn);
                }
            }
        }

        public bool LoadSavedScope(IMetadataHost host)
        {
            string modulesFN = Path.Combine(ConfigParams.SaveScopePath, "modules.txt");
            string classesFN = Path.Combine(ConfigParams.SaveScopePath, "classes.txt");
            string entClassesFN = Path.Combine(ConfigParams.SaveScopePath, "entrypt_classes.txt");
            string methodsFN = Path.Combine(ConfigParams.SaveScopePath, "methods.txt");
            bool valid = CheckIfValid(modulesFN, classesFN, entClassesFN, methodsFN);
            if (!valid) return false;
           
            IDictionary<string, IModule> moduleNameToModuleMap = new Dictionary<string, IModule>();
            IDictionary<string, ITypeDefinition> classNameToTypeMap = new Dictionary<string, ITypeDefinition>();
            IDictionary<string, IMethodDefinition> methodNameToMethodMap = new Dictionary<string, IMethodDefinition>();

            LoadSavedModules(host, modulesFN, moduleNameToModuleMap);
            LoadSavedClasses(host, classesFN, moduleNameToModuleMap, classNameToTypeMap, false);
            LoadSavedClasses(host, entClassesFN, moduleNameToModuleMap, classNameToTypeMap, true);
            LoadSavedMethods(host, methodsFN, classNameToTypeMap, methodNameToMethodMap);
            return true;
        }

        public void SaveScope(IMetadataHost host)
        {
            string modulesFN = "modules.txt";
            StreamWriter modulesSW = new StreamWriter(Path.Combine(ConfigParams.SaveScopePath, modulesFN));
            List<IModule> moduleList = host.LoadedUnits.OfType<IModule>().ToList();
            ISet<ITypeDefinition> processedGenericTypes = new HashSet<ITypeDefinition>();

            foreach (IModule module in moduleList)
            {
                modulesSW.WriteLine("MODULE:" + module.Name.Value + " LOCATION:" + module.Location);
            }
            modulesSW.Close();

            string classesFN = "classes.txt";
            StreamWriter classesSW = new StreamWriter(Path.Combine(ConfigParams.SaveScopePath, classesFN));
            foreach (ITypeDefinition cl in classes)
            {
                IModule mod = TypeHelper.GetDefiningUnit(cl) as IModule;
                if (cl is IGenericTypeInstance)
                {
                    ProcessGenericType(cl, classesSW, mod, processedGenericTypes);
                }
                else
                {
                    classesSW.WriteLine("CLASS:" + cl.FullName() + " ARGS: MODULE:" + mod.Name.Value);
                }
            }
            classesSW.Close();

            string entClassesFN = "entrypt_classes.txt";
            StreamWriter entClassesSW = new StreamWriter(Path.Combine(ConfigParams.SaveScopePath, entClassesFN));
            foreach (ITypeDefinition cl in entryPtClasses)
            {
                IModule mod = TypeHelper.GetDefiningUnit(cl) as IModule;
                entClassesSW.WriteLine("CLASS:" + cl.FullName() + " ARGS: MODULE:" + mod.Name.Value);
            }
            entClassesSW.Close();

            string methodsFN = "methods.txt";
            StreamWriter methodsSW = new StreamWriter(Path.Combine(ConfigParams.SaveScopePath, methodsFN));
            foreach (IMethodDefinition meth in methods)
            {
                string argStr = "";
                IMethodDefinition methToRecord = meth;
                if (meth is IGenericMethodInstance)
                {
                    IGenericMethodInstance genericM = meth as IGenericMethodInstance;
                    IEnumerable<ITypeReference> genericArgs = genericM.GenericArguments;
                    foreach (ITypeReference ty in genericArgs)
                    {
                        ITypeDefinition tyDefn = ty.ResolvedType;
                        argStr += tyDefn.FullName() + ",";
                    }
                    if (!argStr.Equals("")) argStr = argStr.TrimEnd(',');
                    methToRecord = genericM.GenericMethod.ResolvedMethod;
                }
                methodsSW.WriteLine("METHOD:" + methToRecord.FullName() + " ARGS:" + argStr + " CLASS:" + 
                                    methToRecord.ContainingTypeDefinition.FullName());
            }
            methodsSW.Close();
        }

        private void ProcessGenericType(ITypeDefinition cl, StreamWriter classesSW,
                                        IModule mod, ISet<ITypeDefinition> processedGenericTypes)
        {
            IGenericTypeInstance gcl = cl as IGenericTypeInstance;
            if (gcl != null && !processedGenericTypes.Contains(gcl))
            {
                INamedTypeDefinition templateType = gcl.GenericType.ResolvedType;
                IEnumerable<ITypeReference> genArgs = gcl.GenericArguments;
                string argStr = "";
                foreach (ITypeReference ty in genArgs)
                {
                    ITypeDefinition tyDefn = ty.ResolvedType;
                    if (tyDefn is IGenericTypeInstance)
                    {
                        IModule mod1 = TypeHelper.GetDefiningUnit(tyDefn) as IModule;
                        ProcessGenericType(tyDefn, classesSW, mod1, processedGenericTypes);
                    }
                    argStr += tyDefn.FullName() + ",";
                }
                if (!argStr.Equals("")) argStr = argStr.TrimEnd(',');
                classesSW.WriteLine("CLASS:" + templateType.FullName() + " ARGS:" + argStr + " MODULE:" + mod.Name.Value);
                processedGenericTypes.Add(gcl);
            }
        }
    }
}
