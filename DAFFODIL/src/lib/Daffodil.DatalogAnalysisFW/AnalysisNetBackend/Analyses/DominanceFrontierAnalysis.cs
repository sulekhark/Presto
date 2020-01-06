﻿// Copyright (c) Edgardo Zoppi.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Analyses
{
	public class DominanceFrontierAnalysis
	{
		private ControlFlowGraph cfg;

		public DominanceFrontierAnalysis(ControlFlowGraph cfg)
		{
			this.cfg = cfg;
		}

		public ControlFlowGraph Analyze()
		{
			foreach (var node in cfg.Nodes)
			{
				node.DominanceFrontier.Clear();
			}

			foreach (var node in cfg.Nodes)
			{
				if (node.Predecessors.Count < 2) continue;

				foreach (var pred in node.Predecessors)
				{
					var runner = pred;

					while (runner != null &&
						   node.ImmediateDominator != null &&
						   runner.Id != node.ImmediateDominator.Id)
					{
						runner.DominanceFrontier.Add(node);
						runner = runner.ImmediateDominator;
					}
				}
			}

			return cfg;
		}
	}
}
