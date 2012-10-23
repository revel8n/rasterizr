using System;
using System.Collections.Generic;
using SlimShader.IO;
using SlimShader.Util;

namespace SlimShader.Shader.Tokens
{
	public class InstructionToken : OpcodeToken
	{
		public bool Saturate { get; internal set; }
		public InstructionTestBoolean TestBoolean { get; internal set; }
		public List<InstructionTokenExtendedType> ExtendedTypes { get; private set; }
		public sbyte[] SampleOffsets { get; private set; }
		public ResourceDimension ResourceTarget { get; internal set; }
		public ResourceReturnType[] ResourceReturnTypes { get; private set; }

		/// <summary>
		/// Gets the function index to call in the function table specified for the given interface.
		/// Only relevant for OpcodeType.InterfaceCall instructions.
		/// </summary>
		public uint FunctionIndex { get; private set; }

		public List<Operand> Operands { get; private set; }

		public InstructionToken()
		{
			ExtendedTypes = new List<InstructionTokenExtendedType>();
			SampleOffsets = new sbyte[3];
			ResourceReturnTypes = new ResourceReturnType[4];
			Operands = new List<Operand>();
		}

		public static InstructionToken Parse(BytecodeReader reader, OpcodeHeader header)
		{
			var instructionToken = new InstructionToken();

			// Advance to next token.
			var instructionEnd = reader.CurrentPosition + (header.Length * sizeof(uint));
			var token0 = reader.ReadUInt32();

			instructionToken.Saturate = (token0.DecodeValue(13, 13) == 1);
			instructionToken.TestBoolean = token0.DecodeValue<InstructionTestBoolean>(18, 18);

			bool extended = header.IsExtended;
			while (extended)
			{
				uint extendedToken = reader.ReadUInt32();
				var extendedType = extendedToken.DecodeValue<InstructionTokenExtendedType>(0, 5);
				instructionToken.ExtendedTypes.Add(extendedType);
				extended = (extendedToken.DecodeValue(31, 31) == 1);

				switch (extendedType)
				{
					case InstructionTokenExtendedType.SampleControls:
						instructionToken.SampleOffsets[0] = DecodeSigned4BitValue(extendedToken, 09, 12);
						instructionToken.SampleOffsets[1] = DecodeSigned4BitValue(extendedToken, 13, 16);
						instructionToken.SampleOffsets[2] = DecodeSigned4BitValue(extendedToken, 17, 20);
						break;
					case InstructionTokenExtendedType.ResourceDim:
						instructionToken.ResourceTarget = extendedToken.DecodeValue<ResourceDimension>(6, 10);
						break;
					case InstructionTokenExtendedType.ResourceReturnType:
						instructionToken.ResourceReturnTypes[0] = extendedToken.DecodeValue<ResourceReturnType>(06, 09);
						instructionToken.ResourceReturnTypes[1] = extendedToken.DecodeValue<ResourceReturnType>(10, 13);
						instructionToken.ResourceReturnTypes[2] = extendedToken.DecodeValue<ResourceReturnType>(14, 17);
						instructionToken.ResourceReturnTypes[3] = extendedToken.DecodeValue<ResourceReturnType>(18, 21);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			if (header.OpcodeType == OpcodeType.InterfaceCall)
			{
				// Interface call
				//
				// OpcodeToken0:
				//
				// [10:00] D3D10_SB_OPCODE_INTERFACE_CALL
				// [23:11] Ignored, 0
				// [30:24] Instruction length in DWORDs including the opcode token.
				// [31]    0 normally. 1 if extended operand definition, meaning next DWORD
				//         contains extended operand description.  If it is extended, then
				//         it contains the actual instruction length in DWORDs, since
				//         it may not fit into 7 bits if enough types are used.
				//
				// OpcodeToken0 is followed by a DWORD that gives the function index to
				// call in the function table specified for the given interface. 
				// Next is the interface operand.
				instructionToken.FunctionIndex = reader.ReadUInt32();
			}

			while (reader.CurrentPosition < instructionEnd)
				instructionToken.Operands.Add(Operand.Parse(reader, header.OpcodeType));

			return instructionToken;
		}

		private static sbyte DecodeSigned4BitValue(uint token, byte start, byte end)
		{
			if (end - start != 3)
				throw new ArgumentOutOfRangeException();
			var value = token.DecodeValue<sbyte>(start, end);
			if (value > 7)
				return (sbyte) (value - 16);
			return value;
		}

		public override string ToString()
		{
			string result = TypeDescription;
			
			if (ExtendedTypes.Contains(InstructionTokenExtendedType.ResourceDim))
				result += "_indexable";

			if (ExtendedTypes.Contains(InstructionTokenExtendedType.SampleControls))
				result += string.Format("({0},{1},{2})", SampleOffsets[0], SampleOffsets[1], SampleOffsets[2]);

			if (ExtendedTypes.Contains(InstructionTokenExtendedType.ResourceDim))
				result += string.Format("({0})", ResourceTarget.GetDescription());

			if (ExtendedTypes.Contains(InstructionTokenExtendedType.ResourceReturnType))
				result += string.Format("({0},{1},{2},{3})",
					ResourceReturnTypes[0].GetDescription(),
					ResourceReturnTypes[1].GetDescription(),
					ResourceReturnTypes[2].GetDescription(),
					ResourceReturnTypes[3].GetDescription());

			if (Header.OpcodeType.IsConditionalInstruction())
				result += "_" + TestBoolean.GetDescription();

			if (Saturate)
				result += "_sat";
			result += " ";

			for (int i = 0; i < Operands.Count; i++)
			{
				result += Operands[i].ToString();
				if (i < Operands.Count - 1)
					result += ", ";
			}

			return result;
		}
	}
}