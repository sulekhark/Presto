// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace ExamineCatchBlock
{
    public static class ExamineCB
    {
        static int asyncTaskMethCount = 0;
        static int asyncTaskEHCnt = 0;
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Please provide the full-path-name of the assembly as an argument.");
                return;
            }
            string assemblyName = args[0];
            using (var assembly = AssemblyDefinition.ReadAssembly(assemblyName))
            {
                foreach (ModuleDefinition module in assembly.Modules)
                {
                    foreach (TypeDefinition type in module.Types)
                    {
                        processType(type);
                    }
                }
            }
            Console.WriteLine("asyncTaskMethCount: {0},   asyncTaskEHCnt: {1}", asyncTaskMethCount, asyncTaskEHCnt);
            return;
        }

        static void processType(TypeDefinition type)
        {
            if (type.IsClass)
            {
                var typeName = type.Name;
                foreach (MethodDefinition method in type.Methods)
                {
                    var mName = "";
                    var isMoveNextMethod = false;
                    if (method.Name.Equals("MoveNext"))
                    {
                        mName = typeName;
                        asyncTaskMethCount++;
                        isMoveNextMethod = true;
                    }
                    else
                    {
                        mName = method.FullName.Split(' ').Last();
                    }
                    processMethod(method, mName, isMoveNextMethod);
                }
                foreach (TypeDefinition nestedType in type.NestedTypes)
                {
                    processType(nestedType);
                }
            }
        }

        static void processMethod(MethodDefinition method, string mName, bool isMoveNextMethod)
        {
            MethodBody body = method.Body;
            if (body != null)
            {
                body.SimplifyMacros();
                if (body.HasExceptionHandlers)
                {
                    List<ExceptionHandler> ehList = body.ExceptionHandlers.ToList();
                    int currEh = 0;
                    foreach (ExceptionHandler eh in ehList)
                    {
                        string handlerType = eh.HandlerType.ToString();
                        if (handlerType.Equals("Catch"))
                        {
                            string ehType = eh.CatchType.ToString();
                            Instruction ehi = eh.HandlerStart;
                            int ndxOfStloc = getNdxOfStloc(ehi);
                            string loadinfo = "noload";
                            if (ndxOfStloc != -1) loadinfo = checkForLdloc(eh, ndxOfStloc);
                            List<string> ehCallees = getEhCallees(eh);
                            string throwInEH = checkForThrow(eh);
                            Console.WriteLine("{0} {1} {2} {3} {4} {5}", mName, currEh.ToString(), ehType, loadinfo, throwInEH, String.Join(";", ehCallees));
                            if (isMoveNextMethod) asyncTaskEHCnt++;
                        }
                        currEh++;
                    }
                }
                else
                {
                    if (!mName.Equals(".ctor"))
                    {
                        Console.WriteLine("{0}", mName);
                    }
                }
            }
            else
            {
                if (!mName.Equals(".ctor"))
                {
                    Console.WriteLine("{0}", mName);
                }
            }
        }

        static int getNdxOfStloc(Instruction i)
        {
            if (i.OpCode == OpCodes.Stloc_0) return 0;
            else if (i.OpCode == OpCodes.Stloc_1) return 1;
            else if (i.OpCode == OpCodes.Stloc_2) return 2;
            else if (i.OpCode == OpCodes.Stloc_3) return 3;
            else if (i.OpCode == OpCodes.Stloc_S || i.OpCode == OpCodes.Stloc)
            {
                VariableDefinition vardef = (VariableDefinition)i.Operand;
                return vardef.Index;
            }
            else return -1;
        }

        static int getNdxOfLdloc(Instruction i)
        {
            if (i.OpCode == OpCodes.Ldloc_0) return 0;
            else if (i.OpCode == OpCodes.Ldloc_1) return 1;
            else if (i.OpCode == OpCodes.Ldloc_2) return 2;
            else if (i.OpCode == OpCodes.Ldloc_3) return 3;
            else if (i.OpCode == OpCodes.Ldloc_S || i.OpCode == OpCodes.Ldloc)
            {
                VariableDefinition vardef = (VariableDefinition)i.Operand;
                return vardef.Index;
            }
            else return -1;
        }

        static string checkForLdloc(ExceptionHandler eh, int stNdx)
        {
            string res = "noload";
            Instruction i = eh.HandlerStart;
            while (i != eh.HandlerEnd)
            {
                int ldNdx = getNdxOfLdloc(i);
                if (ldNdx == stNdx)
                {
                    res = "load";
                    break;
                }
                i = i.Next;
            }
            return res;
        }

        static List<string> getEhCallees(ExceptionHandler eh)
        {
            List<string> ehCallees = new List<string>();
            Instruction i = eh.HandlerStart;
            while (i != eh.HandlerEnd)
            {
                if (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt)
                {
                    string calleeM = ((MethodReference)i.Operand).ToString();
                    calleeM = calleeM.Split(' ').Last();
                    ehCallees.Add(calleeM);
                }
                else if (i.OpCode == OpCodes.Calli)
                {
                    ehCallees.Add("Function_Pointer");
                }
                i = i.Next;
            }
            return ehCallees;
        }

        static string checkForThrow(ExceptionHandler eh)
        {
            string retEHStr = "no_thr_rethr";
            bool throwPres = false;
            bool rethrowPres = false;
            Instruction i = eh.HandlerStart;
            while (i != eh.HandlerEnd)
            {
                if (i.OpCode == OpCodes.Throw)
                {
                    throwPres = true;
                }
                else if (i.OpCode == OpCodes.Rethrow)
                {
                    rethrowPres = true;
                }
                i = i.Next;
            }
            if (throwPres && rethrowPres)
            {
                retEHStr = "both_thr_rethr";
            }
            else if (throwPres && !rethrowPres)
            {
                retEHStr = "only_throw";
            }
            else if (!throwPres && rethrowPres)
            {
                retEHStr = "only_rethrow";
            }
            return retEHStr;
        }
    }
}
