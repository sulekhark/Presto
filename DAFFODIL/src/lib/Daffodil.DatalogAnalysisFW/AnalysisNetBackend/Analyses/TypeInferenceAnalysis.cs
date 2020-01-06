﻿// Copyright (c) Edgardo Zoppi.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.ThreeAddressCode.Instructions;
using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Visitors;
using Microsoft.Cci;
using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Model;
using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.ThreeAddressCode.Values;
using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Utils;

namespace Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Analyses
{
	public class TypeInferenceAnalysis
	{
        #region class TypeInferer

        private class TypeInferer : InstructionVisitor
        {
            private ITypeReference returnType;

            public TypeInferer(ITypeReference returnType)
            {
	            this.returnType = returnType;
            }

            public override void Visit(LocalAllocationInstruction instruction)
            {
                instruction.TargetAddress.Type = Types.Instance.NativePointerType;
            }

            public override void Visit(SizeofInstruction instruction)
            {
                instruction.Result.Type = Types.Instance.SizeofType;
            }

            public override void Visit(CreateArrayInstruction instruction)
            {
                instruction.Result.Type = Types.Instance.ArrayType(instruction.ElementType, instruction.Rank);
            }

            public override void Visit(CatchInstruction instruction)
            {
                instruction.Result.Type = instruction.ExceptionType;
            }

            public override void Visit(CreateObjectInstruction instruction)
            {
                instruction.Result.Type = instruction.AllocationType;
            }

            public override void Visit(MethodCallInstruction instruction)
            {
                if (instruction.HasResult)
                {
                    instruction.Result.Type = instruction.Method.Type;
                }

                // Skip implicit "this" parameter.
                var offset = instruction.Method.IsStatic ? 0 : 1;

                for (var i = offset; i < instruction.Arguments.Count; ++i)
                {
                    var argument = instruction.Arguments[i];
                    var parameter = instruction.Method.Parameters.ElementAt(i - offset);

                    // Set the null variable a type.
                    if (argument.Type == null ||
                        parameter.Type.TypeCode == PrimitiveTypeCode.Boolean)
                    {
                        argument.Type = parameter.Type;
                    }
                }
            }

            public override void Visit(IndirectMethodCallInstruction instruction)
            {
                if (instruction.HasResult)
                {
                    instruction.Result.Type = instruction.Function.Type;
                }

                // Skip implicit "this" parameter.
                var offset = instruction.Function.IsStatic ? 0 : 1;

                for (var i = offset; i < instruction.Arguments.Count; ++i)
                {
                    var argument = instruction.Arguments[i];
                    var parameter = instruction.Function.Parameters.ElementAt(i - offset);

                    // Set the null variable a type.
                    if (argument.Type == null ||
                        parameter.Type.TypeCode == PrimitiveTypeCode.Boolean)
                    {
                        argument.Type = parameter.Type;
                    }
                }
            }

            public override void Visit(LoadInstruction instruction)
            {
                var operandAsConstant = instruction.Operand as Constant;
                var operandAsVariable = instruction.Operand as IVariable;

                // Null is a polymorphic value so we handle it specially. We don't set the
                // corresponding variable's type yet. We postpone it to usage of the variable
                // or set it to System.Object if it is never used.
                if (operandAsConstant != null)
                {
                    if (operandAsConstant.Value == null)
                    {
                        instruction.Result.Type = Types.Instance.PlatformType.SystemObject;
                    }
                    else if (instruction.Result.Type != null &&
                             instruction.Result.Type.TypeCode == PrimitiveTypeCode.Boolean)
                    {
                        // If the result of the load has type Boolean,
                        // then we are actually loading a Boolean constant.
                        if (operandAsConstant.Value.Equals(0))
                        {
                            operandAsConstant.Value = false;
                            operandAsConstant.Type = Types.Instance.PlatformType.SystemBoolean;
                        }
                        else if (operandAsConstant.Value.Equals(1))
                        {
                            operandAsConstant.Value = true;
                            operandAsConstant.Type = Types.Instance.PlatformType.SystemBoolean;
                        }
                    }
                }
                // If we have variable to variable assignment where the result was assigned
                // a type but the operand was not, then we set the operand type accordingly.
                else if (operandAsVariable != null &&
                         instruction.Result.Type != null &&
                        (operandAsVariable.Type == null ||
                         operandAsVariable.Type == Types.Instance.PlatformType.SystemObject ||
                         instruction.Result.Type.TypeCode == PrimitiveTypeCode.Boolean))
                {
                    operandAsVariable.Type = instruction.Result.Type;
                }

                if (instruction.Result.Type == null)
                {
                    instruction.Result.Type = instruction.Operand.Type;
                }

                if (instruction.Operand is Dereference)
                {
                    var deref = instruction.Operand as Dereference;
                    // SRK (4th Oct 2019) instruction.Result.Type = deref.Reference.Type;
                    if (deref.Reference.Type.TypeCode == PrimitiveTypeCode.Reference && deref.Type != null)
                    {
                        instruction.Result.Type = deref.Type;
                    }
                    else
                    {
                        instruction.Result.Type = deref.Reference.Type;
                    }
                }

                if (instruction.Operand.Type != null && // we can be in the middle of the type inference analysis
                    instruction.Operand.Type is IManagedPointerType){
                    instruction.Result.Type = instruction.Operand.Type;
                }
            }

            public override void Visit(LoadTokenInstruction instruction)
            {
                instruction.Result.Type = Types.Instance.TokenType(instruction.Token);
            }

            public override void Visit(StoreInstruction instruction)
            {
                // Set the null variable a type.
                if (instruction.Result.Type != null &&
                   (instruction.Operand.Type == null ||
                    instruction.Operand.Type == Types.Instance.PlatformType.SystemObject ||
                    instruction.Result.Type.TypeCode == PrimitiveTypeCode.Boolean))
                {
                    instruction.Operand.Type = instruction.Result.Type;
                }
            }

			public override void Visit(ReturnInstruction instruction)
			{
				// Set the null variable a type.
				if (instruction.HasOperand &&
					returnType != null &&
				   (instruction.Operand.Type == null ||
					instruction.Operand.Type == Types.Instance.PlatformType.SystemObject ||
					returnType.TypeCode == PrimitiveTypeCode.Boolean))
				{
					instruction.Operand.Type = returnType;
				}
			}

            public override void Visit(UnaryInstruction instruction)
            {
                instruction.Result.Type = instruction.Operand.Type;
            }

            public override void Visit(ConvertInstruction instruction)
            {
                var type = instruction.Operand.Type;

                switch (instruction.Operation)
                {
                    case ConvertOperation.Conv:
                    case ConvertOperation.Cast:
                    case ConvertOperation.Box:
                    case ConvertOperation.Unbox:
                        // ConversionType is the data type of the result.
                        type = instruction.ConversionType;
                        break;

                    case ConvertOperation.UnboxPtr:
                        // Pointer to ConversionType is the data type of the result
                        type = Types.Instance.PointerType(instruction.ConversionType);
                        break;
                }

                instruction.Result.Type = type;
            }

            public override void Visit(PhiInstruction instruction)
            {
                var type = instruction.Arguments.First().Type;
                var arguments = instruction.Arguments.Skip(1);

                foreach (var argument in arguments)
                {
                    type = TypeHelper.MergedType(type, argument.Type);
                }

                instruction.Result.Type = type;
            }

            public override void Visit(BinaryInstruction instruction)
            {
                var left = instruction.LeftOperand.Type;
                var right = instruction.RightOperand.Type;
                var unsigned = instruction.UnsignedOperands;

                switch (instruction.Operation)
                {
                    case BinaryOperation.Add:
                    case BinaryOperation.Div:
                    case BinaryOperation.Mul:
                    case BinaryOperation.Rem:
                    case BinaryOperation.Sub:
                        instruction.Result.Type = Types.Instance.BinaryNumericOperationType(left, right, unsigned);
                        break;

                    case BinaryOperation.And:
                    case BinaryOperation.Or:
                    case BinaryOperation.Xor:
                        instruction.Result.Type = Types.Instance.BinaryLogicalOperationType(left, right);
                        break;

                    case BinaryOperation.Shl:
                    case BinaryOperation.Shr:
                        instruction.Result.Type = left;
                        break;

                    case BinaryOperation.Eq:
                    case BinaryOperation.Neq:
                        // If one of the operands has type Boolean,
                        // then the other operand must also have type Boolean.
                        if (left != null && left.TypeCode == PrimitiveTypeCode.Boolean)
                        {
                            instruction.RightOperand.Type = Types.Instance.PlatformType.SystemBoolean;
                        }
                        else if (right != null && right.TypeCode == PrimitiveTypeCode.Boolean)
                        {
                            instruction.LeftOperand.Type = Types.Instance.PlatformType.SystemBoolean;
                        }

                        instruction.Result.Type = Types.Instance.PlatformType.SystemBoolean;
                        break;

                    case BinaryOperation.Gt:
                    case BinaryOperation.Ge:
                    case BinaryOperation.Lt:
                    case BinaryOperation.Le:
                        // If one of the operands has a reference type,
                        // then the operator must be != instead of >.
                        if ((left != null && !left.IsValueType) ||
                            (right != null && !right.IsValueType))
                        {
                            instruction.Operation = BinaryOperation.Neq;
                        }

                        instruction.Result.Type = Types.Instance.PlatformType.SystemBoolean;
                        break;
                }
            }
            
            public override void Visit(ConditionalBranchInstruction instruction)
            {
                if (instruction.LeftOperand.Type != null &&
                    instruction.RightOperand.Type == null)
                {
                    instruction.RightOperand.Type = instruction.LeftOperand.Type;
                }
                else if (instruction.LeftOperand.Type == null &&
                         instruction.RightOperand.Type != null)
                {
                    instruction.LeftOperand.Type = instruction.RightOperand.Type;
                }

                if (instruction.Operation == BranchOperation.Eq &&
                    instruction.RightOperand is Constant &&
                    instruction.LeftOperand.Type != null &&
                    instruction.LeftOperand.Type.TypeCode != PrimitiveTypeCode.Boolean)
                {
                    var constant = instruction.RightOperand as Constant;

                    if (constant.Value is bool)
                    {
                        var value = (bool)constant.Value;

                        if (value)
                        {
                            // Change num == true to num != true
                            instruction.Operation = BranchOperation.Neq;
                        }

                        if (instruction.LeftOperand.Type.IsValueType)
                        {
                            // Change num == false to num == 0 or
                            // num != true to num != 0
                            constant.Value = 0;
                        }
                        else
                        {
                            // Change num == false to num == null or
                            // num != true to num != null
                            constant.Value = null;
                        }

                        constant.Type = instruction.LeftOperand.Type;
                    }
                }
            }
        }
		
		#endregion

		private ControlFlowGraph cfg;
        private ITypeReference returnType;
        public int fixptCount;

        public TypeInferenceAnalysis(ControlFlowGraph cfg, ITypeReference returnType)
		{
			this.cfg = cfg;
            this.returnType = returnType;
            fixptCount = 0;
		}

		public void Analyze()
		{
			var inferer = new TypeInferer(returnType);
			var sorted_nodes = cfg.ForwardOrder;

			// Propagate types over the CFG until a fixedpoint is reached
			// (i.e. when types do not change anymore)
			bool changed;

			do
			{
				var result = GetTypeInferenceResult();

				for (var i = 0; i < sorted_nodes.Length; ++i)
				{
					var node = sorted_nodes[i];
					inferer.Visit(node);
				}

				changed = !SameTypes(result);
                fixptCount++;
			}
			while (changed);
		}

		private IDictionary<IVariable, ITypeReference> GetTypeInferenceResult()
		{
			var result = new Dictionary<IVariable, ITypeReference>();
			var variables = cfg.GetVariables();

			foreach (var variable in variables)
			{
				result[variable] = variable.Type;
			}

			return result;
		}

		private bool SameTypes(IDictionary<IVariable, ITypeReference> oldTypes)
		{
			var result = true;
			var variables = cfg.GetVariables();

			foreach (var variable in variables)
			{
				var oldType = oldTypes[variable];
				var newType = variable.Type;

				if (oldType == null || newType == null ||
					!TypeHelper.TypesAreEquivalent(oldType, newType, true))
				{
					result = false;
					break;
				}
			}

			return result;
		}
	}
}
