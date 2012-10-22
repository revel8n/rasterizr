using System;
using System.Collections.Generic;
using System.Text;
using SlimShader.IO;
using SlimShader.Shader;

namespace SlimShader.InputOutputSignature
{
	public class InputOutputSignatureChunk : DxbcChunk
	{
		public List<SignatureParameterDescription> Parameters { get; private set; }

		public InputOutputSignatureChunk()
		{
			Parameters = new List<SignatureParameterDescription>();
		}

		public static InputOutputSignatureChunk Parse(BytecodeReader reader, bool input, ChunkType chunkType,
			ProgramType programType)
		{
			InputOutputSignatureChunk result = (input) ?
				(InputOutputSignatureChunk) new InputSignatureChunk()
				: new OutputSignatureChunk();

			var chunkReader = reader.CopyAtCurrentPosition();
			var elementCount = chunkReader.ReadUInt32();
			var uniqueKey = chunkReader.ReadUInt32();

			SignatureElementSize elementSize;
			switch (chunkType)
			{
				case ChunkType.Osg5 :
					elementSize = SignatureElementSize._7;
					break;
				case ChunkType.Isgn:
				case ChunkType.Osgn:
					elementSize = SignatureElementSize._6;
					break;
				default:
					throw new ArgumentOutOfRangeException("chunkType");
			}

			for (int i = 0; i < elementCount; i++)
				result.Parameters.Add(SignatureParameterDescription.Parse(reader, chunkReader, chunkType, elementSize,
					programType));

			return result;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.AppendLine("// Name                 Index   Mask Register SysValue Format   Used");
			sb.AppendLine("// -------------------- ----- ------ -------- -------- ------ ------");

			foreach (var parameter in Parameters)
				sb.AppendLine("// " + parameter);

			return sb.ToString();
		}
	}
}