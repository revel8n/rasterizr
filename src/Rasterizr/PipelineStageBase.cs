using System.Collections.Generic;

namespace Rasterizr
{
	public abstract class PipelineStageBase
	{
		
	}

	public abstract class PipelineStageBase<TInput, TOutput> : PipelineStageBase
	{
		public abstract void Run(List<TInput> inputs, List<TOutput> outputs);
	}
}