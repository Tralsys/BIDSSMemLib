using BIDS.Parser.Variable;

using TR.BIDSSMemLib.Variable;

namespace BIDSSMemLib.Variable.Tests.UtilsTests;

public class TypeToVariableDataTypeTests
{
	[TestCase(typeof(bool), VariableDataType.Boolean)]
	[TestCase(typeof(sbyte), VariableDataType.Int8)]
	[TestCase(typeof(short), VariableDataType.Int16)]
	[TestCase(typeof(int), VariableDataType.Int32)]
	[TestCase(typeof(long), VariableDataType.Int64)]
	[TestCase(typeof(byte), VariableDataType.UInt8)]
	[TestCase(typeof(ushort), VariableDataType.UInt16)]
	[TestCase(typeof(uint), VariableDataType.UInt32)]
	[TestCase(typeof(ulong), VariableDataType.UInt64)]
	[TestCase(typeof(float), VariableDataType.Float32)]
	[TestCase(typeof(double), VariableDataType.Float64)]
	[TestCase(typeof(Half), VariableDataType.Float16)]
	[TestCase(typeof(string), VariableDataType.Array)]
	[TestCase(typeof(object[]), VariableDataType.Array)]
	public void NormalCaseTests(Type type, VariableDataType expectedVariableDataType)
	{
		Assert.That(type.ToVariableDataType(), Is.EqualTo(expectedVariableDataType));
	}

	struct SampleStruct { }

	[TestCase(typeof(SampleStruct))]
	[TestCase(typeof(object))]
	[TestCase(typeof(Array))]
	public void ExceptionTests(Type type)
	{
		Assert.Throws(typeof(NotSupportedException), () => type.ToVariableDataType());
	}
}
