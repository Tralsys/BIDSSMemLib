using BIDS.Parser.Variable;

using TR;
using TR.BIDSSMemLib;

namespace BIDSSMemLib.Variable.Tests;

public partial class VariableSMemTests
{
	const long SMemCapacity = 0x10000;

	[Test]
	public void GenericsTypeLoadTest()
	{
		SMemIFMock SMemIF = new("test", SMemCapacity);

		VariableSMem<SampleClass> variableSMem = new(SMemIF);

		Assert.That(variableSMem.Members, Is.EquivalentTo(new VariableStructure.IDataRecord[]
		{
			SampleClass.Expected_vUInt16,
			SampleClass.Expected_vInt32,
			SampleClass.Expected_vInt64,
			SampleClass.Expected_vFloat64,
			SampleClass.Expected_vString,
			SampleClass.Expected_vInt32Arr,
		}));

		// `GetStructureBytes`メソッドは正常に動作していることを期待する
		byte[] expectedMemory =
			new byte[16]
				// Data ID
				.Concat(BitConverter.GetBytes((int)-1))

				// Structure Name (= string.Empty)
				.Concat(new byte[] { 0 })

				// Structure
				.Concat(SampleClass.Expected_vUInt16.GetStructureBytes())
				.Concat(SampleClass.Expected_vInt32.GetStructureBytes())
				.Concat(SampleClass.Expected_vInt64.GetStructureBytes())
				.Concat(SampleClass.Expected_vFloat64.GetStructureBytes())
				.Concat(SampleClass.Expected_vString.GetStructureBytes())
				.Concat(SampleClass.Expected_vInt32Arr.GetStructureBytes())
				.ToArray();

		byte[] expectedContentAreaOffset = BitConverter.GetBytes(
			(long)expectedMemory.Length
			+ VariableSMem.PaddingBetweenStructreAndContent
		);
		expectedContentAreaOffset.CopyTo(expectedMemory, 0);

		Assert.Multiple(() =>
		{
			// 領域の前方は構造情報が書き込まれた状態
			// 構造情報以外はゼロ埋めされた状態
			Assert.That(SMemIF.Memory[0..(expectedMemory.Length)], Is.EquivalentTo(expectedMemory));
			Assert.That(SMemIF.Memory[(expectedMemory.Length)..], Is.All.EqualTo((byte)0));
		});
	}

	[Test]
	public void StructureLoadFromSMemTest()
	{
		SMemIFMock SMemIF = new("test", SMemCapacity);

		VariableSMem<SampleClass> variableSMem = new(SMemIF);

		SMemIFMock SMemIF1 = new(SMemIF);
		VariableSMem variableSMem1 = new(SMemIF1, variableSMem.Structure);

		Assert.Pass();
	}

	[Test]
	public void WriteToSMemTest()
	{
		SMemIFMock SMemIF = new("test", SMemCapacity);

		VariableSMem<SampleClass> variableSMem = new(SMemIF);

		SampleClass SampleData = new()
		{
			vUInt16 = 2,
			vInt32 = -2,
			vInt64 = 0x7FFFFFFF12,
			vFloat64 = 1.23456789,
			vString = "testString😀",
			vInt32Arr = new int[]
			{
				1,
				2,
				3
			}
		};

		variableSMem.WriteToSMem(SampleData);

		// (Content部分のみ)
		byte[] expectedBytes = new byte[]
		{
			// Content Length = 56 bytes
			0x38,
			0x00,
			0x00,
			0x00,
			0x00,
			0x00,
			0x00,
			0x00,

			// vUInt16 = 2
			0x02,
			0x00,

			// vInt32 = -2
			0xFE,
			0xFF,
			0xFF,
			0xFF,

			// vInt64 = 549755813649 (0x0000007F_FFFFFF12)
			0x12,
			0xFF,
			0xFF,
			0xFF,
			0x7F,
			0x00,
			0x00,
			0x00,

			// vFloat64 = 1.3
			0x1B,
			0xDE,
			0x83,
			0x42,
			0xCA,
			0xC0,
			0xF3,
			0x3F,

			// vString = "testString😀"
			// count (length) = 14
			0x0E,
			0x00,
			0x00,
			0x00,

			0x74,
			0x65,
			0x73,
			0x74,
			0x53,
			0x74,
			0x72,
			0x69,
			0x6e,
			0x67,
			0xf0,
			0x9f,
			0x98,
			0x80,

			// vInt32Arr = { 1, 2, 3 }
			// count = 3
			0x03,
			0x00,
			0x00,
			0x00,

			0x01,
			0x00,
			0x00,
			0x00,

			0x02,
			0x00,
			0x00,
			0x00,

			0x03,
			0x00,
			0x00,
			0x00,
		};

		Assert.That(
			SMemIF.Memory[(int)variableSMem.ContentAreaOffset..(int)(variableSMem.ContentAreaOffset + expectedBytes.Length)],
			Is.EquivalentTo(expectedBytes)
		);
	}

	[Test]
	public void ReadFromSMemTest_ReturnPayload()
	{
		SMemIFMock SMemIF = new("test", SMemCapacity);

		VariableSMem<SampleClass> variableSMem = new(SMemIF);
		VariableSMem<SampleClass> variableSMem1 = new(new SMemIFMock(SMemIF));

		SampleClass SampleData = new()
		{
			vUInt16 = 2,
			vInt32 = -2,
			vInt64 = 0x7FFFFFFF12,
			vFloat64 = 1.23456789,
			vString = "testString😀",
			vInt32Arr = new int[]
			{
				1,
				2,
				3
			}
		};

		variableSMem.WriteToSMem(SampleData);

		VariableStructurePayload payload = variableSMem1.ReadFromSMem();

		Assert.That(payload, Has.Count.EqualTo(6));

		Assert.That(payload, Has.Member(new KeyValuePair<string, VariableStructure.IDataRecord>(
			nameof(SampleClass.vUInt16),
			SampleClass.Expected_vUInt16 with
			{
				Value = SampleData.vUInt16,
			})
		));
		Assert.That(payload, Has.Member(new KeyValuePair<string, VariableStructure.IDataRecord>(
			nameof(SampleClass.vInt32),
			SampleClass.Expected_vInt32 with
			{
				Value = SampleData.vInt32,
			})
		));
		Assert.That(payload, Has.Member(new KeyValuePair<string, VariableStructure.IDataRecord>(
			nameof(SampleClass.vInt64),
			SampleClass.Expected_vInt64 with
			{
				Value = SampleData.vInt64,
			})
		));
		Assert.That(payload, Has.Member(new KeyValuePair<string, VariableStructure.IDataRecord>(
			nameof(SampleClass.vFloat64),
			SampleClass.Expected_vFloat64 with
			{
				Value = SampleData.vFloat64,
			})
		));

		ArrayStructureInPayloadTestUtil(payload, nameof(SampleClass.vString), SampleClass.Expected_vString, new byte[]
		{
			0x74,
			0x65,
			0x73,
			0x74,
			0x53,
			0x74,
			0x72,
			0x69,
			0x6e,
			0x67,
			0xf0,
			0x9f,
			0x98,
			0x80,
		});

		ArrayStructureInPayloadTestUtil(payload, nameof(SampleClass.vInt32Arr), SampleClass.Expected_vInt32Arr, SampleData.vInt32Arr);
	}

	static void ArrayStructureInPayloadTestUtil(VariableStructurePayload payload, string name, VariableStructure.ArrayStructure expectedStructure, Array expectedArray)
	{
		Assert.That(payload, Does.ContainKey(name));
		VariableStructure.ArrayStructure? actual = payload[name] as VariableStructure.ArrayStructure;

		Assert.That(actual, Is.EqualTo(expectedStructure with
		{
			ValueArray = actual!.ValueArray
		}));

		Assert.That(actual.ValueArray, Is.EquivalentTo(expectedArray));
	}

	[Test]
	public void ReadFromSMemTest_WriteToInstance()
	{
		SMemIFMock SMemIF = new("test", SMemCapacity);

		VariableSMem<SampleClass> variableSMem = new(SMemIF);
		VariableSMem<SampleClass> variableSMem1 = new(new SMemIFMock(SMemIF));

		SampleClass SampleData = new()
		{
			vUInt16 = 2,
			vInt32 = -2,
			vInt64 = 0x7FFFFFFF12,
			vFloat64 = 1.23456789,
			vString = "testString😀",
			vInt32Arr = new int[]
			{
				1,
				2,
				3
			}
		};

		variableSMem.WriteToSMem(SampleData);

		object ActualData = new SampleClass();

		variableSMem1.ReadFromSMem(ref ActualData);

		Assert.That(ActualData, Is.EqualTo(SampleData));
	}

	[Test]
	public void CreateWithoutTypeTest()
	{
		SMemIFMock SMemIF = new("test", SMemCapacity);

		VariableSMem<SampleClass> variableSMem = new(SMemIF);

		VariableSMem variableSMem1 = VariableSMem.CreateWithoutType(new SMemIFMock(SMemIF));

		Assert.That(variableSMem1.Members, Is.EquivalentTo(variableSMem.Members));
	}
}
