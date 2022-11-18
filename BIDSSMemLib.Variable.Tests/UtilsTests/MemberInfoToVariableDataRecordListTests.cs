using BIDS.Parser.Variable;

using TR.BIDSSMemLib.Variable;

namespace BIDSSMemLib.Variable.Tests.UtilsTests;

public class MemberInfoToVariableDataRecordListTests
{
	class SampleBaseClass
	{
		public int SampleBaseIntProperty { get; }
		public long SampleBaseLongField;

		private int PrivateBaseIntProperty { get; }
		protected string ProtectedBaseStringField = string.Empty;
		internal char InternalBaseCharProperty { get; }

		public event EventHandler? BasePublicEvent;
		private event EventHandler? BasePrivateEvent;

		public void BasePublicMethod() => BasePrivateEvent?.Invoke(this, new());
		private void BasePrivateMethod() => BasePrivateEvent?.Invoke(this, new());

		static public byte PublicBaseStaticCharProperty { get; }
	}

	class SampleClass : SampleBaseClass
	{
		public int SampleIntProperty { get; }
		public long[]? SampleLongArrayField;

		private int PrivateIntProperty { get; }
		protected string ProtectedStringField = string.Empty;
		internal char InternalCharProperty { get; }
	}

	[Test]
	public void NormalTest()
	{
		Assert.That(
			typeof(SampleClass).ToVariableDataRecordList(),
			Is.EquivalentTo(new List<VariableStructure.IDataRecord>()
			{
				new VariableStructure.DataRecord(VariableDataType.Int32, nameof(SampleClass.SampleIntProperty)),
				new VariableStructure.ArrayStructure(VariableDataType.Int64, nameof(SampleClass.SampleLongArrayField)),

				new VariableStructure.DataRecord(VariableDataType.Int32, nameof(SampleBaseClass.SampleBaseIntProperty)),
				new VariableStructure.DataRecord(VariableDataType.Int64, nameof(SampleBaseClass.SampleBaseLongField)),
			})
		);
	}
}
