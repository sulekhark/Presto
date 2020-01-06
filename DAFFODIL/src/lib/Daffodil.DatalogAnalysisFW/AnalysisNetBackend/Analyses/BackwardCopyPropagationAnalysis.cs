﻿// Copyright (c) Edgardo Zoppi.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.ThreeAddressCode;
using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Utils;
using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.ThreeAddressCode.Values;
using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.ThreeAddressCode.Instructions;
using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Model;

namespace Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Analyses
{
	//[Obsolete("The analysis implementation could have some bugs!")]
	public class BackwardCopyPropagationAnalysis : BackwardDataFlowAnalysis<IDictionary<IVariable, IVariable>>
	{
		private DataFlowAnalysisResult<IDictionary<IVariable, IVariable>>[] result;
		private IDictionary<IVariable, IVariable>[] GEN;
		private ISet<IVariable>[] KILL;

		public BackwardCopyPropagationAnalysis(ControlFlowGraph cfg)
			: base(cfg)
		{
		}

		public void Transform(MethodBody body)
		{
			if (this.result == null) throw new InvalidOperationException("Analysis result not available.");

            // TODO: Not sure if we need to process the cfg nodes in backward order
            // since there is no global information used, the order shouldn't matter.
            //var sorted_nodes = cfg.BackwardOrder;
            //
            //foreach (var node in sorted_nodes)
            foreach (var node in this.cfg.Nodes)
			{
				var node_result = this.result[node.Id];
				var copies = new Dictionary<IVariable, IVariable>();

				if (node_result.Output != null)
				{
					copies.AddRange(node_result.Output);
				}

                for (var i = node.Instructions.Count - 1; i >= 0; --i)
				{
					var instruction = node.Instructions[i];

					foreach (var variable in instruction.ModifiedVariables)
					{
						// Only replace temporal variables
						if (variable.IsTemporal() &&
							copies.ContainsKey(variable))
						{
							var operand = copies[variable];

							instruction.Replace(variable, operand);
						}
					}

					var isTemporalCopy = this.Flow(instruction, copies);

					foreach (var variable in instruction.UsedVariables)
					{
                        // Only replace temporal variables
                        if (variable.IsTemporal() &&
							copies.ContainsKey(variable))
						{
							var operand = copies[variable];
							instruction.Replace(variable, operand);
						}
					}

                    // Only replace temporal variables
                    if (isTemporalCopy)
					{
						// Remove the copy instruction
						if (i == 0)
						{
							// The copy is the first instruction of the basic block
							// Replace the copy instruction with a nop to preserve branch targets
							var nop = new NopInstruction(instruction.Offset);
							var index = body.Instructions.IndexOf(instruction);
							body.Instructions[index] = nop;
							node.Instructions[i] = nop;
						}
						else
						{
							// The copy is not the first instruction of the basic block
							body.Instructions.Remove(instruction);
							node.Instructions.RemoveAt(i);
						}
					}
				}
			}

			body.UpdateVariables();
		}

		public override DataFlowAnalysisResult<IDictionary<IVariable, IVariable>>[] Analyze()
		{
			this.ComputeGen();
			this.ComputeKill();

			var result = base.Analyze();

			this.result = result;
			this.GEN = null;
			this.KILL = null;

			return result;
		}

		protected override IDictionary<IVariable, IVariable> InitialValue(CFGNode node)
		{
			return GEN[node.Id];
		}

		protected override bool Compare(IDictionary<IVariable, IVariable> left, IDictionary<IVariable, IVariable> right)
		{
			return left.DictionaryEquals(right);
		}

		protected override IDictionary<IVariable, IVariable> Join(IDictionary<IVariable, IVariable> left, IDictionary<IVariable, IVariable> right)
		{
			Func<IVariable, IVariable, IVariable> intersectVariables = (a, b) => a.Equals(b) ? a : null;
			var result = left.Intersect(right, intersectVariables);
			return result;
		}

		protected override IDictionary<IVariable, IVariable> Flow(CFGNode node, IDictionary<IVariable, IVariable> output)
		{
			var input = new Dictionary<IVariable, IVariable>(output);
			var kill = KILL[node.Id];
			var gen = GEN[node.Id];

			foreach (var variable in kill)
			{
				this.RemoveCopiesWithVariable(input, variable);
			}

			input.SetRange(gen);
			return input;
		}

		private void ComputeGen()
		{
			GEN = new IDictionary<IVariable, IVariable>[this.cfg.Nodes.Count];

			foreach (var node in this.cfg.Nodes)
			{
				var gen = new Dictionary<IVariable, IVariable>();

				for (var i = node.Instructions.Count - 1; i >= 0; --i)
				{
					var instruction = node.Instructions[i];
					this.Flow(instruction, gen);
				}

				GEN[node.Id] = gen;
			}
		}

		private void ComputeKill()
		{
			KILL = new ISet<IVariable>[this.cfg.Nodes.Count];

			foreach (var node in this.cfg.Nodes)
			{
				var kill = new HashSet<IVariable>();

				foreach (var instruction in node.Instructions)
				{
					kill.UnionWith(instruction.ModifiedVariables);
				}

				KILL[node.Id] = kill;
			}
		}

		private void RemoveCopiesWithVariable(IDictionary<IVariable, IVariable> copies, IVariable variable)
		{
			var array = copies.ToArray();

			foreach (var copy in array)
			{
				if (copy.Key == variable ||
					copy.Value == variable)
				{
					copies.Remove(copy.Key);
				}
			}
		}

		private bool Flow(Instruction instruction, IDictionary<IVariable, IVariable> copies)
		{
			IVariable left;
			IVariable right;

			var isCopy = instruction.IsCopy(out left, out right);

			if (isCopy)
			{
				// Only replace temporal variables
				if (left.IsTemporal() &&
					copies.ContainsKey(left))
				{
					left = copies[left];
				}
			}

			foreach (var variable in instruction.ModifiedVariables)
			{
				this.RemoveCopiesWithVariable(copies, variable);
			}

			if (isCopy)
			{
				this.RemoveCopiesWithVariable(copies, right);
				copies.Add(right, left);
			}

			var result = isCopy && right.IsTemporal();
			return result;
		}
	}
}
