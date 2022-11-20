using BIDS.Parser.Variable;

namespace BIDSSMemLib.Variable.Tests;

public partial class VariableSMemTests
{
	class SampleClass
	{
		public ushort vUInt16;
		public static readonly VariableStructure.DataRecord Expected_vUInt16
			= new(VariableDataType.UInt16, "vUInt16");

		public int vInt32;
		public static readonly VariableStructure.DataRecord Expected_vInt32
			= new(VariableDataType.Int32, "vInt32");

		public long vInt64;
		public static readonly VariableStructure.DataRecord Expected_vInt64
			= new(VariableDataType.Int64, "vInt64");

		public double vFloat64;
		public static readonly VariableStructure.DataRecord Expected_vFloat64
			= new(VariableDataType.Float64, "vFloat64");

		public string? vString;
		public static readonly VariableStructure.ArrayStructure Expected_vString
			= new(VariableDataType.UInt8, "vString");

		public int[]? vInt32Arr;
		public static readonly VariableStructure.ArrayStructure Expected_vInt32Arr
			= new(VariableDataType.Int32, "vInt32Arr");

		public override bool Equals(object? obj)
			=> obj is SampleClass v
			&& vUInt16 == v.vUInt16
			&& vInt32 == v.vInt32
			&& vInt64 == v.vInt64
			&& vFloat64 == v.vFloat64
			&& vString == v.vString
			&& Enumerable.SequenceEqual(vInt32Arr ?? Array.Empty<int>(), v.vInt32Arr ?? Array.Empty<int>());

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
