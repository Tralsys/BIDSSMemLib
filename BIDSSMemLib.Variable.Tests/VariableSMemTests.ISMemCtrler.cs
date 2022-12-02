using System;

using TR;
using TR.BIDSSMemLib;

namespace BIDSSMemLib.Variable.Tests;

public class VariableSMemTests_SMemCtrler
{
	static readonly long SMem_Capacity = 0x100;
	static int rand_int => new Random().Next();

	public struct CustomStruct
	{
		public float A;
		public int B;
		public double C;
		public bool D;

		public override string ToString()
			=> $"A(float):{A},\tB(int):{B},\tC(double):{C},\tD(bool):{D}";
	}

	[Parallelizable]
	[Test]
	public void CustomStructDataRWTest([Range(1, 10)] int randSeed)
	{
		Random rand = new Random(randSeed);
		float A = rand.NextSingle();
		int B = rand.Next();
		double C = rand.NextDouble();

		string smem_name = $"{nameof(CustomStructDataRWTest)}_{B}";
		CustomStruct value = new()
		{
			A = A,
			B = B,
			C = C,
			D = (B % 2) == 0
		};

		RunTest(smem_name, value);
	}

	static void RunTest<T>(string smem_name, T value) where T : struct
	{
		using SMemIFMock mock = new(smem_name, SMem_Capacity);
		using ISMemCtrler<T> reader = new VariableSMem<T>(mock);
		using ISMemCtrler<T> writer = new VariableSMem<T>(mock);

		writer.Write(value);

		T result = reader.Read();

		Assert.That(result, Is.EqualTo(value));
	}
}
