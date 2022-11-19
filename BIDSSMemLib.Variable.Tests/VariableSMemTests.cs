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
			// (Data Id = (Int32)-1)
			0xFF,
			0xFF,
			0xFF,
			0xFF,

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
}
