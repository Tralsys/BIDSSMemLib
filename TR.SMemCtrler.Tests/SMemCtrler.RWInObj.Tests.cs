using NUnit.Framework;

using System;

namespace TR
{
	public class SMemCtrler_RWInObjTests
	{
		static int rand_int => new Random().Next();

		[Parallelizable]
		[Test]
		public void IntDataRWTest([Range(1, 10)] int randSeed)
		{
			int value = new Random(randSeed).Next();
			string smem_name = $"{nameof(IntDataRWTest)}_{value}";

			RunTest(smem_name, value);
		}

		[Parallelizable]
		[Test]
		public void DoubleDataRWTest([Range(1, 10)] int randSeed)
		{
			double value = new Random(randSeed).NextDouble();
			string smem_name = $"{nameof(DoubleDataRWTest)}_{value}";

			RunTest(smem_name, value);
		}

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
			IReadWriteInObject? reader = null;
			IReadWriteInObject? writer = null;
			try
			{
				reader = new SMemCtrler<T>(smem_name, false, true);
				writer = new SMemCtrler<T>(smem_name, false, true);

				writer.WriteInObject(value);
				object result = reader.ReadInObject();
				Assert.AreEqual(value, result);
			}
			finally
			{
				reader?.Dispose();
				writer?.Dispose();
			}
		}
	}
}