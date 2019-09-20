
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Cci;
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

        private bool CheckIfValid(string modulesPath, string classesPath, string methodsPath)
        {
            bool IsValid = true;
            if (!File.Exists(modulesPath) || (new FileInfo(modulesPath).Length == 0)) IsValid = false;
            if (!File.Exists(classesPath) || (new FileInfo(classesPath).Length == 0)) IsValid = false;
            if (!File.Exists(methodsPath) || (new FileInfo(methodsPath).Length == 0)) IsValid = false;
            return IsValid;
        }

        public bool LoadSavedScope(IMetadataHost host)
        {
            string modulesFN = Path.Combine(ConfigParams.SaveScopePath, "modules.txt");
            string classesFN = Path.Combine(ConfigParams.SaveScopePath, "classes.txt");
            string methodsFN = Path.Combine(ConfigParams.SaveScopePath, "methods.txt");
            bool valid = CheckIfValid(modulesFN, classesFN, methodsFN);
            if (!valid) return false;

            List<string> moduleNames = new List<string>();
            IDictionary<string, IModule> moduleNameToModuleMap = new Dictionary<string, IModule>();
            IDictionary<IModule, IList<string>> bucketedClasses = new Dictionary<IModule, IList<string>>();
            using (StreamReader sr = new StreamReader(modulesFN))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (!string.IsNullOrEmpty(line))
                    {
                        string[] parts = line.Split();
                        string moduleName = parts[0];
                        string fileName = parts[1];
                        moduleNames.Add(moduleName);
                        IModule module = host.LoadUnitFrom(fileName) as IModule;
                        moduleNameToModuleMap[moduleName] = module;
                        bucketedClasses[module] = new List<string>();
                        List<INamedTypeDefinition> l = module.GetAllTypes().OfType<INamedTypeDefinition>().ToList();
                        Console.WriteLine("Module: {0}   Num elements: {1}", moduleName, l.Count);

                        if (module == null || module == Dummy.Module || module == Dummy.Assembly)
                            throw new Exception("The input is not a valid CLR module or assembly.");
                    }
                }
                moduleNames.Sort();
                moduleNames.Reverse();
            }

            using (StreamReader sr = new StreamReader(classesFN))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    foreach (string modName in moduleNames)
                    {
                        if (line.StartsWith(modName)) bucketedClasses[moduleNameToModuleMap[modName]].Add(line);
                    }
                }
            }
            foreach (IModule module in bucketedClasses.Keys)
            {
                IList<string> clNameList = bucketedClasses[module];
                foreach (string clName in clNameList)
                {
                    INamedTypeDefinition clTypDefn = UnitHelper.FindType(host.NameTable, module, clName);
                    if (clTypDefn != null) classes.Add(clTypDefn);
                }
            }
            using (StreamReader sr = new StreamReader(methodsFN))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {

                }
            }
            return true;
        }

        public void SaveScope(IMetadataHost host)
        {
            string modulesFN = "modules.txt";
            StreamWriter modulesSW = new StreamWriter(Path.Combine(ConfigParams.SaveScopePath, modulesFN));
            List<IModule> moduleList = host.LoadedUnits.OfType<IModule>().ToList();
            foreach (IModule module in moduleList)
            {
                modulesSW.WriteLine(module.Name.Value + " " + module.Location);
            }
            modulesSW.Close();

            string classesFN = "classes.txt";
            StreamWriter classesSW = new StreamWriter(Path.Combine(ConfigParams.SaveScopePath, classesFN));
            foreach (ITypeDefinition cl in classes)
            {
            }
        }
    }
}
