using TR;
using TR.BIDSSMemLib;

namespace BIDSSMemLib.Variable.Tests;

public class VariableSMemNameManagerTests
{
	const long SMemCapacity = 0x10000;

	const string TEST_STR_1 = "test";
	const int TEST_STR_1_LEN = 4;

	const string TEST_STR_2 = "testTest";
	const int TEST_STR_2_LEN = 8;

	const string TEST_STR_3 = "v";
	const int TEST_STR_3_LEN = 1;

	const string TEST_STR_4 = "testTestSample";
	const int TEST_STR_4_LEN = 14;

	static readonly VariableSMemNameManager.SMemName expected1 = new(
		0,
		TEST_STR_1_LEN,
		TEST_STR_1
	);
	static readonly VariableSMemNameManager.SMemName expected2 = new(
		sizeof(ushort) + TEST_STR_1_LEN,
		TEST_STR_2_LEN,
		TEST_STR_2
	);
	static readonly VariableSMemNameManager.SMemName expected3 = new(
		sizeof(ushort) + TEST_STR_1_LEN
		+ sizeof(ushort) + TEST_STR_2_LEN,
		TEST_STR_3_LEN,
		TEST_STR_3
	);
	static readonly VariableSMemNameManager.SMemName expected4 = new(
		sizeof(ushort) + TEST_STR_1_LEN
		+ sizeof(ushort) + TEST_STR_2_LEN
		+ sizeof(ushort) + TEST_STR_3_LEN,
		TEST_STR_4_LEN,
		TEST_STR_4
	);


	[Test]
	public void AddOneTest()
	{
		SMemIFMock SMemIF = new("test", SMemCapacity);

		VariableSMemNameManager manager = new(SMemIF);

		VariableSMemNameManager.SMemName? nameRecord1 = manager.AddName(TEST_STR_1);

		Assert.That(nameRecord1, Is.EqualTo(expected1));
	}

	[Test]
	public void AddManyTest()
	{
		SMemIFMock SMemIF = new("test", SMemCapacity);

		VariableSMemNameManager manager = new(SMemIF);

		VariableSMemNameManager.SMemName? nameRecord1 = manager.AddName(TEST_STR_1);
		VariableSMemNameManager.SMemName? nameRecord2 = manager.AddName(TEST_STR_2);
		VariableSMemNameManager.SMemName? nameRecord3 = manager.AddName(TEST_STR_3);

		Assert.Multiple(() =>
		{
			Assert.That(nameRecord1, Is.EqualTo(expected1));
			Assert.That(nameRecord2, Is.EqualTo(expected2));
			Assert.That(nameRecord3, Is.EqualTo(expected3));
		});
	}

	[Test]
	public void AddSameNameManyTimesTest()
	{
		SMemIFMock SMemIF = new("test", SMemCapacity);

		VariableSMemNameManager manager = new(SMemIF);

		VariableSMemNameManager.SMemName? nameRecord1 = manager.AddName(TEST_STR_1);
		VariableSMemNameManager.SMemName? nameRecord2 = manager.AddName(TEST_STR_2);
		VariableSMemNameManager.SMemName? nameRecord3 = manager.AddName(TEST_STR_1);

		Assert.Multiple(() =>
		{
			Assert.That(nameRecord1, Is.EqualTo(expected1));
			Assert.That(nameRecord2, Is.EqualTo(expected2));
			Assert.That(nameRecord3, Is.EqualTo(expected1));
		});
	}

	[Test]
	public void IEnumerableTest()
	{
		SMemIFMock SMemIF = new("test", SMemCapacity);

		VariableSMemNameManager manager = new(SMemIF);

		manager.AddName(TEST_STR_1);
		manager.AddName(TEST_STR_2);
		manager.AddName(TEST_STR_3);

		Assert.That(manager, Is.EquivalentTo(new VariableSMemNameManager.SMemName[]
		{
			expected1,
			expected2,
			expected3,
		}));
	}

	[Test]
	public void DeleteNameTest()
	{
		SMemIFMock SMemIF = new("test", SMemCapacity);

		VariableSMemNameManager manager = new(SMemIF);

		manager.AddName(TEST_STR_1);
		VariableSMemNameManager.SMemName? nameRecord2 = manager.AddName(TEST_STR_2);
		manager.AddName(TEST_STR_3);

		Assert.That(nameRecord2, Is.Not.Null);
		manager.DeleteName(nameRecord2);

		Assert.That(manager, Is.EquivalentTo(new VariableSMemNameManager.SMemName[]
		{
			expected1,
			expected3,
		}));
	}

	[Test]
	public void AddAfterDeleteTest_SameLength()
	{
		SMemIFMock SMemIF = new("test", SMemCapacity);

		VariableSMemNameManager manager = new(SMemIF);

		manager.AddName(TEST_STR_1);
		VariableSMemNameManager.SMemName? nameRecord2 = manager.AddName(TEST_STR_2);
		manager.AddName(TEST_STR_3);

		Assert.That(nameRecord2, Is.Not.Null);
		manager.DeleteName(nameRecord2);

		manager.AddName(TEST_STR_2);

		Assert.That(manager, Is.EquivalentTo(new VariableSMemNameManager.SMemName[]
		{
			expected1,
			expected2,
			expected3,
		}));
	}

	[Test]
	public void AddAfterDeleteTest_LongerThanDeleted()
	{
		SMemIFMock SMemIF = new("test", SMemCapacity);

		VariableSMemNameManager manager = new(SMemIF);

		manager.AddName(TEST_STR_1);
		VariableSMemNameManager.SMemName? nameRecord2 = manager.AddName(TEST_STR_2);
		manager.AddName(TEST_STR_3);


		Assert.That(nameRecord2, Is.Not.Null);
		manager.DeleteName(nameRecord2);

		manager.AddName(TEST_STR_4);

		Assert.That(manager, Is.EquivalentTo(new VariableSMemNameManager.SMemName[]
		{
			expected1,
			expected3,
			expected4,
		}));
	}

	[Test]
	public void AddNameTest_SoLongName()
	{
		SMemIFMock SMemIF = new("test", SMemCapacity * 2);

		VariableSMemNameManager manager = new(SMemIF);

		string name = new string('a', (int)ushort.MaxValue);

		VariableSMemNameManager.SMemName? nameRecord = manager.AddName(name);

		VariableSMemNameManager.SMemName expected = new(
			0,
			ushort.MaxValue,
			name
		);

		Assert.That(nameRecord, Is.EqualTo(expected));
	}

	[Test]
	public void ErrorTest_AddName_NameEmpty()
	{
		SMemIFMock SMemIF = new("test", SMemCapacity);

		VariableSMemNameManager manager = new(SMemIF);

		Assert.Throws(typeof(ArgumentException), () => manager.AddName(string.Empty));
	}

	[Test]
	public void ErrorTest_AddName_TooLongName()
	{
		SMemIFMock SMemIF = new("test", SMemCapacity);

		VariableSMemNameManager manager = new(SMemIF);

		string name = new string('a', (int)ushort.MaxValue + 1);

		Assert.Throws(typeof(ArgumentOutOfRangeException), () => manager.AddName(name));
	}
}
