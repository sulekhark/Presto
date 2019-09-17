// Copyright (c) Edgardo Zoppi.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.Cci;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole
{
	public class Assembly
	{
		public string FileName { get; private set; }
		public bool IsLoaded { get; private set; }
		public IMetadataHost Host { get; private set; }
		public IModule Module { get; private set; }
		
		public Assembly(IMetadataHost host)
		{
			this.Host = host;
		}

		public Assembly(IMetadataHost host, IModule module)
		{
			this.Host = host;
			this.Module = module;
			this.IsLoaded = true;
		}

		public void Load(string fileName)
		{
			this.Module = this.Host.LoadUnitFrom(fileName) as IModule;

			if (this.Module == null || this.Module == Dummy.Module || this.Module == Dummy.Assembly)
				throw new Exception("The input is not a valid CLR module or assembly.");

			this.FileName = fileName;
			this.IsLoaded = true;
		}

		public void Unload()
		{
			if (!this.IsLoaded) return;
			this.Module = null;
			this.FileName = null;
			this.IsLoaded = false;
		}
	}
}
