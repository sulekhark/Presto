﻿// Copyright (c) Edgardo Zoppi.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Analyses;
using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.ThreeAddressCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.ThreeAddressCode.Values;
using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.ThreeAddressCode.Expressions;
using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.ThreeAddressCode.Instructions;
using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Visitors;
using Microsoft.Cci;
using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Model;

namespace Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Utils
{
	public static class Extensions
	{
		public static bool DictionaryEquals<K,V>(this IDictionary<K,V> self, IDictionary<K,V> other, Func<V, V, bool> valueEquals = null)
		{
			if (object.ReferenceEquals(self, other)) return true;
			if (self.Count != other.Count) return false;

			if (valueEquals == null)
			{
				valueEquals = (a, b) => object.Equals(a, b);
			}

			foreach (var key in self.Keys)
			{
				var otherContainsKey = other.ContainsKey(key);
				if (!otherContainsKey) return false;
			}

			foreach (var entry in self)
			{
				var value = other[entry.Key];
				var valuesAreEquals = valueEquals(entry.Value, value);

				if (!valuesAreEquals) return false;
			}

			return true;
		}

		public static IDictionary<K, V> Union<K, V>(this IDictionary<K, V> self, IEnumerable<KeyValuePair<K, V>> other, Func<V, V, V> valueUnion)
		{
			var result = new Dictionary<K, V>(self);
			result.UnionWith(other, valueUnion);
			return result;
		}

		public static void UnionWith<K, V>(this IDictionary<K, V> self, IEnumerable<KeyValuePair<K, V>> other, Func<V, V, V> valueUnion)
		{
			foreach (var entry in other)
			{
				V value;

				if (self.TryGetValue(entry.Key, out value))
				{
					value = valueUnion(value, entry.Value);

					if (value != null)
			{
						self[entry.Key] = value;
					}
					else
					{
						self.Remove(entry.Key);
					}
				}
				else
				{
					self.Add(entry.Key, entry.Value);
				}
			}
		}

		public static IDictionary<K, V> Intersect<K, V>(this IDictionary<K, V> self, IEnumerable<KeyValuePair<K, V>> other, Func<V, V, V> valueIntersect)
		{
			var result = new Dictionary<K, V>();

			foreach (var entry in other)
			{
				V value;

				if (self.TryGetValue(entry.Key, out value))
				{
					value = valueIntersect(value, entry.Value);

					if (value != null)
					{
						result.Add(entry.Key, value);
					}
				}
			}

			return result;
		}

		public static void IntersectWith<K, V>(this IDictionary<K, V> self, IEnumerable<KeyValuePair<K, V>> other, Func<V, V, V> valueIntersect)
		{
			var keys = new HashSet<K>();

			foreach (var entry in other)
			{
				V value;

				if (self.TryGetValue(entry.Key, out value))
				{
					value = valueIntersect(value, entry.Value);

					if (value != null)
					{
						self[entry.Key] = value;
						keys.Add(entry.Key);
					}
					else
					{
						self.Remove(entry.Key);
					}
				}
			}

			var keysToRemove = self.Keys.Except(keys).ToArray();

			foreach (var key in keysToRemove)
			{
				self.Remove(key);
			}
		}

		public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> elements)
		{
			foreach (var element in elements)
			{
				collection.Add(element);
			}
		}

		public static void RemoveAll<T>(this ICollection<T> self, IEnumerable<T> elements)
		{
			foreach (var element in elements)
			{
				self.Remove(element);
			}
		}

		public static void SetRange<K, V>(this IDictionary<K, V> self, IEnumerable<KeyValuePair<K, V>> elements)
		{
			foreach (var element in elements)
			{
				self[element.Key] = element.Value;
			}
		}

		public static MapSet<K, V> ToMapSet<K, V>(this IEnumerable<V> elements, Func<V, K> keySelector)
		{
			var result = new MapSet<K, V>();

			foreach (var element in elements)
			{
				var key = keySelector(element);
				result.Add(key, element);
			}

			return result;
		}

		public static MapList<K, V> ToMapList<K, V>(this IEnumerable<V> elements, Func<V, K> keySelector)
		{
			var result = new MapList<K, V>();

			foreach (var element in elements)
			{
				var key = keySelector(element);
				result.Add(key, element);
			}

			return result;
		}

		public static Subset<T> ToSubset<T>(this T[] universe)
		{
			return new Subset<T>(universe, false);
		}

		public static Subset<T> ToEmptySubset<T>(this T[] universe)
		{
			return new Subset<T>(universe, true);
		}

		public static uint StartOffset(this IInstructionContainer block)
		{
			var instruction = block.Instructions.First();
			return instruction.Offset;
		}

		public static uint EndOffset(this IInstructionContainer block)
		{
			var instruction = block.Instructions.Last();
			return instruction.Offset;
		}

		public static ISet<IVariable> GetVariables(this IInstructionContainer block)
		{
			var result = from i in block.Instructions
						 from v in i.Variables
						 select v;

			//var result = block.Instructions.SelectMany(i => i.Variables);
			return new HashSet<IVariable>(result);
		}

		public static ISet<IVariable> GetModifiedVariables(this IInstructionContainer block)
		{
			var result = from i in block.Instructions
						 from v in i.ModifiedVariables
						 select v;

			//var result = block.Instructions.SelectMany(i => i.ModifiedVariables);
			return new HashSet<IVariable>(result);
		}

		public static ISet<IVariable> GetUsedVariables(this IInstructionContainer block)
		{
			var result = from i in block.Instructions
						 from v in i.UsedVariables
						 select v;

			//var result = block.Instructions.SelectMany(i => i.UsedVariables);
			return new HashSet<IVariable>(result);
		}

		public static ISet<IVariable> GetDefinedVariables(this IInstructionContainer block)
		{
			var result = from i in block.Instructions
						 let d = i as DefinitionInstruction
						 where d != null && d.HasResult
						 select d.Result;

			return new HashSet<IVariable>(result);
			//var result = new HashSet<IVariable>();

			//foreach (var instruction in block.Instructions)
			//{
			//    var definition = instruction as DefinitionInstruction;

			//    if (definition != null && definition.HasResult)
			//    {
			//        result.Add(definition.Result);
			//    }
			//}

			//return result;
		}

		public static ISet<IVariable> GetModifiedVariables(this CFGLoop loop)
		{
			var result = from n in loop.Body
						 from v in n.GetModifiedVariables()
						 select v;

			//var result = loop.Body.SelectMany(n => n.GetModifiedVariables());
			return new HashSet<IVariable>(result);
		}

		public static ISet<IVariable> GetVariables(this ControlFlowGraph cfg)
		{
			var result = from n in cfg.Nodes
						 from v in n.GetVariables()
						 select v;

			//var result = cfg.Nodes.SelectMany(n => n.GetVariables());
			return new HashSet<IVariable>(result);
		}

		public static ISet<CFGNode> GetExitNodes(this CFGLoop loop)
		{
			var result = from n in loop.Body
						 from m in n.Successors
						 where !loop.Body.Contains(m)
						 select n;

			//var result = loop.Body.Where(n => n.Successors.Any(m => !loop.Body.Contains(m)));
			return new HashSet<CFGNode>(result);
		}

		public static IExpression ToExpression(this IValue value)
		{
			return value as IExpression;
		}

		//public static IExpression GetValueOriginal(this IDictionary<IVariable, IExpression> equalities, IVariable variable)
		//{
		//    var result = equalities.ContainsKey(variable) ? equalities[variable] : variable;
		//    return result;
		//}
		
		public static IExpression GetValue(this IDictionary<IVariable, IExpression> equalities, IVariable variable)
		{
			IExpression result = variable;

			while (variable != null && equalities.ContainsKey(variable))
			{
				result = equalities[variable];
				variable = result as IVariable;
			}

			return result;
		}

		public static bool IsTemporal(this IVariable variable)
		{
			while (variable is DerivedVariable)
			{
				var derived = variable as DerivedVariable;
				variable = derived.Original;
			}

			var result = variable is TemporalVariable;
			return result;
		}

		public static bool IsCopy(this Instruction instruction, out IVariable left, out IVariable right)
		{
			var result = false;
			left = null;
			right = null;

			if (instruction is LoadInstruction)
			{
				var load = instruction as LoadInstruction;

				if (load.Operand is IVariable)
				{
					left = load.Result;
					right = load.Operand as IVariable;
					result = true;
				}
			}

			return result;
		}

		public static IExpression ReplaceVariables<T>(this IExpression expr, IDictionary<IVariable, T> equalities) where T : IExpression
		{
			foreach (var variable in expr.Variables)
			{
				if (variable.IsTemporal())
				{
					var hasValue = equalities.ContainsKey(variable);

					if (hasValue)
					{
						var value = equalities[variable];
						var isUnknown = value is UnknownValue;
						var isPhi = value is PhiExpression;
						var isMethodCall = value is MethodCallExpression;

						if (isUnknown || isPhi || isMethodCall)
							continue;

						expr = expr.Replace(variable, value);
					}
				}
			}

			return expr;
		}

		public static void RemoveTemporalVariables(this PointsToGraph ptg)
		{
			foreach (var variable in ptg.Variables)
			{
				if (variable.IsTemporal())
				{
					ptg.Remove(variable);
				}
			}
		}

		public static ISet<PTGNode> GetTargets(this PointsToGraph ptg, InstanceFieldAccess access)
		{
			var result = ptg.GetTargets(access.Instance, access.Field);
			return result;
		}

		#region May Alias

		public static bool MayAlias(this PointsToGraph ptg, IVariable variable1, IVariable variable2)
		{
			var targets1 = ptg.GetTargets(variable1);
			var targets2 = ptg.GetTargets(variable2);
			var alias = targets1.Intersect(targets2);
			return alias.Any();
		}

		public static bool MayAlias(this PointsToGraph ptg, InstanceFieldAccess access, IVariable variable)
		{
			var targetsAccess = ptg.GetTargets(access);
			var targetsVariable = ptg.GetTargets(variable);
			var alias = targetsAccess.Intersect(targetsVariable);
			return alias.Any();
		}

		public static bool MayAlias(this PointsToGraph ptg, InstanceFieldAccess access1, InstanceFieldAccess access2)
		{
			var targets1 = ptg.GetTargets(access1);
			var targets2 = ptg.GetTargets(access2);
			var alias = targets1.Intersect(targets2);
			return alias.Any();
		}

		#endregion

		#region Points-to graph reachability

		public static bool IsReachable(this PointsToGraph ptg, IVariable variable, PTGNode target)
		{
			var result = false;
			var visitedNodes = new HashSet<PTGNode>();
			var worklist = new Queue<PTGNode>();
			var nodes = ptg.GetTargets(variable);

			foreach (var node in nodes)
			{
				worklist.Enqueue(node);
				visitedNodes.Add(node);
			}

			while (worklist.Any())
			{
				var node = worklist.Dequeue();

				if (node.Equals(ptg.Null))
				{
					continue;
				}

				if (node.Equals(target))
				{
					result = true;
					break;
				}

				foreach (var targets in node.Targets.Values)
				{
					foreach (var nodeTarget in targets)
					{
						if (!visitedNodes.Contains(nodeTarget))
						{
							worklist.Enqueue(nodeTarget);
							visitedNodes.Add(nodeTarget);
						}
					}
				}
			}

			return result;
		}

		public static IEnumerable<PTGNode> GetReachableNodes(this PointsToGraph ptg, IVariable variable)
		{
			var visitedNodes = new HashSet<PTGNode>();
			var worklist = new Queue<PTGNode>();
			var nodes = ptg.GetTargets(variable);

			foreach (var node in nodes)
			{
				worklist.Enqueue(node);
				visitedNodes.Add(node);
			}

			while (worklist.Any())
			{
				var node = worklist.Dequeue();

				yield return node;

				if (node.Equals(ptg.Null))
				{
					continue;
				}

				foreach (var targets in node.Targets.Values)
				{
					foreach (var nodeTarget in targets)
					{
						if (!visitedNodes.Contains(nodeTarget))
						{
							worklist.Enqueue(nodeTarget);
							visitedNodes.Add(nodeTarget);
						}
					}
				}
			}
		}

		public static IEnumerable<PTGNode> GetReachableNodes(this PointsToGraph ptg)
		{
			var result = ptg.Variables.SelectMany(v => ptg.GetReachableNodes(v))
							.Distinct();
			return result;
		}

		public static ISet<IVariable> GetAliases(this PointsToGraph ptg, IVariable variable)
		{
			var result = new HashSet<IVariable>() { variable };

			foreach (var ptgNode in ptg.GetTargets(variable))
			{
				if (!ptgNode.Equals(ptg.Null))
				{
					result.UnionWith(ptgNode.Variables);
				}
			}

			return result;
		}

		#endregion

		public static bool IsPure(this IMethodReference method)
		{
			var result = method.Attributes.Any(a => a.Type == Types.Instance.PureAttributeType);

			if (!result)
			{
				result = method.Name.Value == "get_Count" && Types.Instance.IsContainer(method.ContainingType);
			}

			return result;
		}

		public static void Inline(this MethodBody callerBody, MethodCallInstruction methodCall, MethodBody calleeBody)
		{
			// TODO: Fix local variables (and parameters) name clashing

			var index = callerBody.Instructions.IndexOf(methodCall);
			callerBody.Instructions.RemoveAt(index);

			Instruction nextInstruction = null;

			if (callerBody.Instructions.Count > index)
			{
				// The caller method has more instructions after the method call
				nextInstruction = callerBody.Instructions[index];
			}			

			for (var i = 0; i < calleeBody.Parameters.Count; ++i)
			{
				var parameter = calleeBody.Parameters[i];
				var argument = methodCall.Arguments[i];
				var copy = new LoadInstruction(methodCall.Offset, parameter, argument);

				copy.Label = string.Format("{0}_{1}", methodCall.Label, copy.Label);
				callerBody.Instructions.Insert(index, copy);
				index++;
			}

			var lastCalleeInstructionIndex = calleeBody.Instructions.Count - 1;

			for (var i = 0; i < calleeBody.Instructions.Count; ++i)
			{
				var instruction = calleeBody.Instructions[i];

				if (instruction is ReturnInstruction)
				{
					var ret = instruction as ReturnInstruction;

					if (ret.HasOperand && methodCall.HasResult)
					{
						// Copy the return value of the callee to the result variable of the method call
						var copy = new LoadInstruction(ret.Offset, methodCall.Result, ret.Operand);

						copy.Label = string.Format("{0}_{1}", methodCall.Label, copy.Label);
						callerBody.Instructions.Insert(index, copy);
						index++;
					}

					if (nextInstruction != null && i < lastCalleeInstructionIndex)
					{
						// Jump to the instruction after the method call
						var branch = new UnconditionalBranchInstruction(ret.Offset, nextInstruction.Offset);

						branch.Label = string.Format("{0}_{1}", methodCall.Label, branch.Label);
						callerBody.Instructions.Insert(index, branch);
						index++;
					}
				}
				else
				{
					// TODO: Fix! We should clone the instruction
					// so the original is not modified
					// and calleeBody remain intacted

					if (instruction is BranchInstruction)
					{
						var branch = instruction as BranchInstruction;
						branch.Target = string.Format("{0}_{1}", methodCall.Label, branch.Target);
					}
					else if (instruction is SwitchInstruction)
					{
						var branch = instruction as SwitchInstruction;

						for (var j = 0; j < branch.Targets.Count; ++j)
						{
							var target = branch.Targets[j];
							branch.Targets[j] = string.Format("{0}_{1}", methodCall.Label, target);
						}
					}

					instruction.Label = string.Format("{0}_{1}", methodCall.Label, instruction.Label);
					callerBody.Instructions.Insert(index, instruction);
					index++;
				}
			}
		}

		#region Control-flow helper methods

		public static bool IsBranch(this Instruction instruction, out IList<string> targets)
		{
			var result = false;
			targets = null;

			if (instruction is UnconditionalBranchInstruction ||
				instruction is ConditionalBranchInstruction)
			{
				var branch = instruction as BranchInstruction;

				targets = new List<string>() { branch.Target };
				result = true;
			}
			else if (instruction is SwitchInstruction)
			{
				var branch = instruction as SwitchInstruction;

				targets = branch.Targets;
				result = true;
			}

			return result;
		}

		public static bool IsExitingMethod(this Instruction instruction)
		{
			var result = instruction is ReturnInstruction ||
						 instruction is ThrowInstruction;

			return result;
		}

		public static bool CanFallThroughNextInstruction(this Instruction instruction)
		{
			var result = instruction is UnconditionalBranchInstruction ||
						 instruction.IsExitingMethod();

			return !result;
		}

		#endregion

		public static void RemoveUnusedLabels(this MethodBody body)
		{
			var usedLabels = new HashSet<string>();

			foreach (var protectedBlock in body.ExceptionInformation)
			{
				usedLabels.Add(protectedBlock.Start);
				usedLabels.Add(protectedBlock.End);
				usedLabels.Add(protectedBlock.Handler.Start);
				usedLabels.Add(protectedBlock.Handler.End);
			}

			foreach (var instruction in body.Instructions)
			{
				IList<string> targets;
				var isBranch = instruction.IsBranch(out targets);

				if (isBranch)
				{
					usedLabels.UnionWith(targets);
				}
			}

			foreach (var instruction in body.Instructions)
			{
				var used = usedLabels.Contains(instruction.Label);

				if (used)
				{
					usedLabels.Remove(instruction.Label);
				}
				else
				{
					instruction.Label = null;
				}
			}
		}
	}
}
