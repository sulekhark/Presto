﻿// Copyright (c) Edgardo Zoppi.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Analyses;
using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Model;

namespace Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Serialization
{
	public static class DOTSerializer
	{
		public static string Serialize(ControlFlowGraph cfg)
		{
			var sb = new StringBuilder();
			sb.AppendLine("digraph ControlFlow\n{");
			sb.AppendLine("\tnode[shape=\"rect\"];");

			foreach (var node in cfg.Nodes)
			{
				var label = DOTSerializer.Serialize(node);
				sb.AppendFormat("\t{0}[label=\"{1}\"];\n", node.Id, label);

				foreach (var successor in node.Successors)
				{
					sb.AppendFormat("\t{0} -> {1};\n", node.Id, successor.Id);
				}
			}

			sb.AppendLine("}");
			return sb.ToString();
		}

		private static string Serialize(CFGNode node)
		{
			string result;

			switch (node.Kind)
			{
				case CFGNodeKind.Entry: result = "entry"; break;
				case CFGNodeKind.Exit: result = "exit"; break;
				default: result = string.Join("\\l", node.Instructions) + "\\l"; break;
			}

			return result;
		}
	}
}
