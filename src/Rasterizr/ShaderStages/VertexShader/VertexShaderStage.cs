using System.Collections.Generic;

namespace Rasterizr.ShaderStages.VertexShader
{
	/// <summary>
	/// Applies a vertex program to the input vertices. The vertex program
	/// minimally computes the clip-space coordinates of the vertex
	/// positions and returns these as outputs to be used by the clipper
	/// and rasterizer.
	/// </summary>
	public class VertexShaderStage : PipelineStageBase<object, IVertexShaderOutput>
	{
		public IVertexShader VertexShader { get; set; }

		public override void Run(List<object> inputs, List<IVertexShaderOutput> outputs)
		{
			if (VertexShader == null)
				throw new RasterizrException("VertexShader must be set");

			foreach (object input in inputs)
			{
				// Apply vertex shader.
				IVertexShaderOutput vertexShaderOutput = VertexShader.Execute(input);
				outputs.Add(vertexShaderOutput);
			}
		}
	}

	// TODO: Implement cache for recently shaded vertices.
}